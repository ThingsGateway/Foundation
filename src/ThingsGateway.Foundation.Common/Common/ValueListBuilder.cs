// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;

// Copied from https://github.com/dotnet/runtime/blob/a9ed4168626c14b4d74db0d8c205c69e56fc45ed/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/ValueListBuilder.cs
// with unused members removed.

namespace System.Collections.Generic;

/// <summary>
/// 高性能的可增长列表构建器，基于 <see cref="Span{T}"/> 和 <see cref="ArrayPool{T}"/> 实现。
/// 特点：
/// - 为 <c>ref struct</c>，仅可在栈上使用，适合临时构建列表结果；
/// - 可以使用外部提供的 <see cref="Span{T}"/>（例如 stackalloc）作为初始缓冲区；
/// - 缓冲区不够时从 <see cref="ArrayPool{T}.Shared"/> 租用数组以扩容，减少堆分配。
/// </summary>
public ref partial struct ValueListBuilder<T> : IDisposable
{
    /// <summary>
    /// 当前使用的缓冲区视图，可能来源于外部 span 或数组池租用的数组。
    /// </summary>
    private Span<T> _span;

    /// <summary>
    /// 若非 null，则为从数组池租用的数组引用，需要在 <see cref="Dispose"/> 时归还。
    /// 如果使用外部提供的 span，则该字段为 null。
    /// </summary>
    private T[]? _arrayFromPool;

    /// <summary>
    /// 当前已写入元素个数（逻辑长度）。
    /// </summary>
    private int _pos;

    /// <summary>
    /// 默认构造函数，使用初始容量 32（会根据池的 bucket 实际调整）。
    /// </summary>
    public ValueListBuilder() : this(32)
    {

    }

    /// <summary>
    /// 使用指定初始容量创建构建器。
    /// 会立即通过 <see cref="Grow(int)"/> 从数组池租用相应容量的数组。
    /// </summary>
    /// <param name="length">初始容量。</param>
    public ValueListBuilder(int length)
    {
        Grow(length);
        _pos = 0;
    }

    /// <summary>
    /// 使用外部提供的初始 <see cref="Span{T}"/> 作为缓冲区。
    /// 不会向数组池租用数组，<see cref="Dispose"/> 时也不会归还任何数组。
    /// </summary>
    /// <param name="initialSpan">初始缓冲区。</param>
    public ValueListBuilder(Span<T> initialSpan)
    {
        _span = initialSpan;
        _arrayFromPool = null;
        _pos = 0;
    }

    /// <summary>
    /// 向列表末尾追加一个元素；容量不足时会触发扩容。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        int pos = _pos;

        // Workaround for https://github.com/dotnet/runtime/issues/72004
        Span<T> span = _span;
        if ((uint)pos < (uint)span.Length)
        {
            span[pos] = item;
            _pos = pos + 1;
        }
        else
        {
            AddWithResize(item);
        }
    }

    /// <summary>
    /// 容量不足时的追加慢路径：先扩容，再写入元素。
    /// 独立为非内联方法，保持常规 <see cref="Add(T)"/> 快路径体积更小，以利于 JIT 优化。
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        Debug.Assert(_pos == _span.Length);
        int pos = _pos;
        Grow(1);
        _span[pos] = item;
        _pos = pos + 1;
    }

    /// <summary>
    /// 返回当前已写入部分的只读切片 [0, <c>_pos</c>)。
    /// </summary>
    public ReadOnlySpan<T> AsSpan() => _span.Slice(0, _pos);

    /// <summary>
    /// 释放当前实例持有的数组池资源。
    /// 如果使用的是外部提供的 span，则该方法不会做任何事情。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        T[]? toReturn = _arrayFromPool;
        if (toReturn != null)
        {
            _arrayFromPool = null;
            ArrayPool<T>.Shared.Return(toReturn);
        }
    }

    // Note that consuming implementations depend on the list only growing if it's absolutely
    // required.  If the list is already large enough to hold the additional items be added,
    // it must not grow. The list is used in a number of places where the reference is checked
    // and it's expected to match the initial reference provided to the constructor if that
    // span was sufficiently large.
    private void Grow(int additionalCapacityRequired = 1)
    {
        const int ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Double the size of the span.  If it's currently empty, default to size 4,
        // although it'll be increased in Rent to the pool's minimum bucket size.
        int nextCapacity = Math.Max(_span.Length != 0 ? _span.Length * 2 : 4, _span.Length + additionalCapacityRequired);

        // If the computed doubled capacity exceeds the possible length of an array, then we
        // want to downgrade to either the maximum array length if that's large enough to hold
        // an additional item, or the current length + 1 if it's larger than the max length, in
        // which case it'll result in an OOM when calling Rent below.  In the exceedingly rare
        // case where _span.Length is already int.MaxValue (in which case it couldn't be a managed
        // array), just use that same value again and let it OOM in Rent as well.
        if ((uint)nextCapacity > ArrayMaxLength)
        {
            nextCapacity = Math.Max(Math.Max(_span.Length + 1, ArrayMaxLength), _span.Length);
        }

        // 从数组池租用新的更大数组，并把现有内容拷贝过去。
        T[] array = ArrayPool<T>.Shared.Rent(nextCapacity);
        _span.CopyTo(array);

        T[]? toReturn = _arrayFromPool;
        _span = _arrayFromPool = array;
        if (toReturn != null)
        {
            ArrayPool<T>.Shared.Return(toReturn);
        }
    }
}