// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// 注意：本文件核心实现来源于 .NET Runtime（MIT 协议），已经过官方充分性能优化。
// 这里仅在不改变行为和性能的前提下，增加中文注释，便于阅读和维护。

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text;

/// <summary>
/// 高性能字符串构建器，基于 <see cref="Span{T}"/> 和 <see cref="ArrayPool{T}"/> 实现，
/// 与 <see cref="StringBuilder"/> 相比，在高频、临时字符串拼接场景下可以显著减少分配与 GC 压力。
///
/// 关键特性：
/// 1. 结构为 <c>ref struct</c>，只能在栈上使用，不能装箱、不能捕获到闭包、不能存活在托管堆；
/// 2. 可以使用外部提供的 <see cref="Span{T}"/> 作为初始缓冲区（例如 stackalloc）；
/// 3. 需要扩容时，从 <see cref="ArrayPool{T}.Shared"/> 租用数组，并在 <see cref="Dispose"/> 时归还；
/// 4. <see cref="ToString"/> 和 <see cref="TryCopyTo"/> 会在返回结果后自动 <see cref="Dispose"/> 当前实例。
/// </summary>
// Copied from https://github.com/dotnet/runtime/blob/a9c5eadd951dcba73167f72cc624eb790573663a/src/libraries/Common/src/System/Text/ValueStringBuilder.cs
public ref struct ValueStringBuilder : IDisposable
{
    /// <summary>
    /// 若非 null，则表示当前使用的缓冲区是从数组池租用的，需要在 <see cref="Dispose"/> 时归还。
    /// 若为 null，则说明使用的是外部传入的 <see cref="Span{T}"/>（例如 stackalloc），无需归还。
    /// </summary>
    private char[]? _arrayToReturnToPool;

    /// <summary>
    /// 当前实际使用的字符缓冲区，可能来自栈上 span 或托管数组切片。
    /// </summary>
    private Span<char> _chars;

    /// <summary>
    /// 当前已写入的字符数，即逻辑字符串长度。
    /// </summary>
    private int _pos;

    /// <summary>
    /// 无参构造：默认从数组池租用容量为 64 的缓冲区。
    /// 适用于小到中等长度字符串拼接的通用场景。
    /// </summary>
    public ValueStringBuilder() : this(64)
    {

    }

    /// <summary>
    /// 使用调用方提供的初始缓冲区（通常由 <c>stackalloc</c> 创建）。
    /// 使用该构造函数不会向数组池租用缓冲区，<see cref="Dispose"/> 时也不会归还任何数组。
    /// </summary>
    /// <param name="initialBuffer">初始字符缓冲区。</param>
    public ValueStringBuilder(Span<char> initialBuffer)
    {
        _arrayToReturnToPool = null;
        _chars = initialBuffer;
        _pos = 0;
    }

    /// <summary>
    /// 使用一个字符串作为初始内容。
    /// 内部会从数组池租用固定长度（1024）的缓冲区，然后将字符串写入其中。
    /// </summary>
    /// <param name="initValue">初始字符串。</param>
    public ValueStringBuilder(string initValue)
    {
        _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(1024);
        _chars = _arrayToReturnToPool;
        _pos = 0;
        Append(initValue);
    }

    /// <summary>
    /// 使用指定初始容量，从数组池租用缓冲区。
    /// </summary>
    /// <param name="initialCapacity">期望的初始容量。</param>
    public ValueStringBuilder(int initialCapacity)
    {
        _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
        _chars = _arrayToReturnToPool;
        _pos = 0;
    }

    /// <summary>
    /// 当前字符串逻辑长度（已写入的字符数）。
    /// </summary>
    public int Length
    {
        get => _pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= _chars.Length);
            _pos = value;
        }
    }

    /// <summary>
    /// 当前底层缓冲区容量（不等同于 <see cref="Length"/>）。
    /// </summary>
    public int Capacity => _chars.Length;

    /// <summary>
    /// 清空当前内容，仅重置逻辑长度，不会释放缓冲区或缩容。
    /// </summary>
    public void Clear()
    {
        _pos = 0;

    }

    /// <summary>
    /// 确保内部缓冲区容量至少为指定大小；若不足则触发扩容。
    /// </summary>
    /// <param name="capacity">最小所需容量。</param>
    public void EnsureCapacity(int capacity)
    {
        // This is not expected to be called this with negative capacity
        Debug.Assert(capacity >= 0);

        // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
        if ((uint)capacity > (uint)_chars.Length)
        {
            Grow(capacity - _pos);
        }
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// Does not ensure there is a null char after <see cref="Length"/>
    /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
    /// the explicit method call, and write eg "fixed (char* c = builder)"
    /// </summary>
    public ref char GetPinnableReference()
    {
        return ref MemoryMarshal.GetReference(_chars);
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
    public ref char GetPinnableReference(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(Length + 1);
            _chars[Length] = '\0';
        }
        return ref MemoryMarshal.GetReference(_chars);
    }

    /// <summary>
    /// 索引访问当前内容中的某个字符。
    /// 注意：仅在 Debug 模式下做越界断言检查，Release 模式下调用方需自行保证索引合法。
    /// </summary>
    public ref char this[int index]
    {
        get
        {
            Debug.Assert(index < _pos);
            return ref _chars[index];
        }
    }

    /// <summary>
    /// 返回当前内容对应的字符串，并释放内部资源。
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string s = _chars.Slice(0, _pos).ToString();
        Dispose();
        return s;
    }

    /// <summary>
    /// 返回内部底层缓冲区的可写 <see cref="Span{T}"/>（包括尚未使用的空间）。
    /// 注意：一般仅用于特殊高性能场景，调用方需自行保证不越界且不破坏内部状态。
    /// </summary>
    public Span<char> RawChars => _chars;

    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
    public ReadOnlySpan<char> AsSpan(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(Length + 1);
            _chars[Length] = '\0';
        }
        return _chars.Slice(0, _pos);
    }

    /// <summary>
    /// 返回 [0, <see cref="Length"/>) 区间的只读切片。
    /// </summary>
    public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);

    /// <summary>
    /// 返回 [<paramref name="start"/>, <see cref="Length"/>) 区间的只读切片。
    /// 调用方需保证 <paramref name="start"/> 不大于 <see cref="Length"/>，否则会抛出异常。
    /// </summary>
    public ReadOnlySpan<char> AsSpan(int start) => _chars.Slice(start, _pos - start);

    /// <summary>
    /// 返回 [<paramref name="start"/>, <paramref name="start"/> + <paramref name="length"/>) 区间的只读切片。
    /// 调用方需保证区间不会超出当前缓冲区，否则会抛出异常。
    /// </summary>
    public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

    /// <summary>
    /// 尝试将当前内容拷贝到目标 <see cref="Span{T}"/>。
    /// 无论成功与否，都会在返回前调用 <see cref="Dispose"/> 释放内部资源。
    /// </summary>
    public bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
        if (_chars.Slice(0, _pos).TryCopyTo(destination))
        {
            charsWritten = _pos;
            Dispose();
            return true;
        }
        else
        {
            charsWritten = 0;
            Dispose();
            return false;
        }
    }

    /// <summary>
    /// 在指定位置插入若干个相同字符。
    /// </summary>
    /// <param name="index">插入位置。</param>
    /// <param name="value">要插入的字符。</param>
    /// <param name="count">插入次数。</param>
    public void Insert(int index, char value, int count)
    {
        if (_pos > _chars.Length - count)
        {
            Grow(count);
        }

        int remaining = _pos - index;
        _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
        _chars.Slice(index, count).Fill(value);
        _pos += count;
    }

    /// <summary>
    /// 在指定位置插入一个字符串。
    /// </summary>
    /// <param name="index">插入位置。</param>
    /// <param name="s">要插入的字符串，可以为 null。</param>
    public void Insert(int index, string? s)
    {
        if (s == null)
        {
            return;
        }

        int count = s.Length;

        if (_pos > (_chars.Length - count))
        {
            Grow(count);
        }

        int remaining = _pos - index;
        _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
        s.AsSpan().CopyTo(_chars.Slice(index));
        _pos += count;
    }

    /// <summary>
    /// 追加单个字符（性能热点，使用 AggressiveInlining）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        int pos = _pos;
        if ((uint)pos < (uint)_chars.Length)
        {
            _chars[pos] = c;
            _pos = pos + 1;
        }
        else
        {
            GrowAndAppend(c);
        }
    }

    /// <summary>
    /// 追加字符串；对长度为 1 的字符串做了专门优化，以减少常见场景中的开销。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? s)
    {
        if (s == null)
        {
            return;
        }

        int pos = _pos;
        if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        {
            _chars[pos] = s[0];
            _pos = pos + 1;
        }
        else
        {
            AppendSlow(s);
        }
    }

    /// <summary>
    /// 从末尾移除指定长度的内容；若长度超过当前长度，则结果为清空。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int length)
    {
        _pos -= length;
        _pos = Math.Max(0, _pos);
    }

    /// <summary>
    /// 处理 <see cref="Append(string?)"/> 的慢路径（字符串长度 > 1 或容量不足）。
    /// </summary>
    private void AppendSlow(string s)
    {
        int pos = _pos;
        if (pos > _chars.Length - s.Length)
        {
            Grow(s.Length);
        }

        s.AsSpan().CopyTo(_chars.Slice(pos));
        _pos += s.Length;
    }

    /// <summary>
    /// 追加重复字符 <paramref name="c"/>，共 <paramref name="count"/> 次。
    /// </summary>
    public void Append(char c, int count)
    {
        if (_pos > _chars.Length - count)
        {
            Grow(count);
        }

        Span<char> dst = _chars.Slice(_pos, count);
        for (int i = 0; i < dst.Length; i++)
        {
            dst[i] = c;
        }
        _pos += count;
    }

    /// <summary>
    /// 使用指针形式追加一段字符缓冲区。
    /// 适合与非托管代码或 fixed 缓冲区交互的高性能场景。
    /// </summary>
    public unsafe void Append(char* value, int length)
    {
        int pos = _pos;
        if (pos > _chars.Length - length)
        {
            Grow(length);
        }

        Span<char> dst = _chars.Slice(_pos, length);
        for (int i = 0; i < dst.Length; i++)
        {
            dst[i] = *value++;
        }
        _pos += length;
    }

    /// <summary>
    /// 追加一个 <see cref="ReadOnlySpan{T}"/> 的内容。
    /// </summary>
    public void Append(ReadOnlySpan<char> value)
    {
        int pos = _pos;
        if (pos > _chars.Length - value.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    /// <summary>
    /// 预留一段指定长度的连续空间并返回对应的 <see cref="Span{T}"/>，
    /// 调用方可以直接往返回的 Span 中写入内容，从而避免额外拷贝。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length)
    {
        int origPos = _pos;
        if (origPos > _chars.Length - length)
        {
            Grow(length);
        }

        _pos = origPos + length;
        return _chars.Slice(origPos, length);
    }

    /// <summary>
    /// 当追加单个字符时容量不足的慢路径：先扩容，再调用快速路径 <see cref="Append(char)"/>。
    /// 使用 NoInlining 以保持快速路径更小，利于 JIT 内联与指令缓存。
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
        Grow(1);
        Append(c);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="_pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
        char[] poolArray = ArrayPool<char>.Shared.Rent((int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_chars.Length * 2));

        _chars.Slice(0, _pos).CopyTo(poolArray);

        char[]? toReturn = _arrayToReturnToPool;
        _chars = _arrayToReturnToPool = poolArray;
        if (toReturn != null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    /// <summary>
    /// 释放当前实例持有的数组池资源，并将自身重置为 <c>default</c>。
    /// 注意：调用 <see cref="Dispose"/> 之后，该实例不应再被使用。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        char[]? toReturn = _arrayToReturnToPool;
        this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
        if (toReturn != null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    /// <summary>
    /// 追加一行文本并换行：先追加 <paramref name="str"/>（若非 null 或空），再追加 <see cref="Environment.NewLine"/>。
    /// 默认参数为 null，即仅追加换行符。
    /// </summary>
    public void AppendLine(string? str = null)
    {
        if (!string.IsNullOrEmpty(str))
            Append(str);
        Append(Environment.NewLine);
    }
}