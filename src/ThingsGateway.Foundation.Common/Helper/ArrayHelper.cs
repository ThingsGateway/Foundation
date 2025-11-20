using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;

namespace ThingsGateway.Foundation.Common.Extension;

public static class ArrayHelper
{

    /// <summary>集合转为数组，加锁确保安全</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static T[] ToArray<T>(this ICollection<T> collection)
    {
        //if (collection == null) return null;

        lock (collection)
        {
            var count = collection.Count;
            if (count == 0) return [];

            var arr = new T[count];
            collection.CopyTo(arr, 0);

            return arr;
        }
    }


    #region 字符串扩展
    /// <summary>转字符串</summary>
    /// <param name="span"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static String ToStr(this ReadOnlySpan<Byte> span, Encoding? encoding = null)
    {
        if (span.Length == 0) return String.Empty;
        return (encoding ?? Encoding.UTF8).GetString(span);
    }

    /// <summary>转字符串</summary>
    /// <param name="span"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static String ToStr(this Span<Byte> span, Encoding? encoding = null)
    {
        if (span.Length == 0) return String.Empty;
        return (encoding ?? Encoding.UTF8).GetString(span);
    }

    /// <summary>获取字符串的字节数组</summary>
    public static unsafe Int32 GetBytes(this Encoding encoding, ReadOnlySpan<Char> chars, Span<Byte> bytes)
    {
        fixed (Char* chars2 = &MemoryMarshal.GetReference(chars))
        {
            fixed (Byte* bytes2 = &MemoryMarshal.GetReference(bytes))
            {
                return encoding.GetBytes(chars2, chars.Length, bytes2, bytes.Length);
            }
        }
    }

    /// <summary>获取字节数组的字符串</summary>
    public static unsafe String GetString(this Encoding encoding, ReadOnlySpan<Byte> bytes)
    {
        if (bytes.IsEmpty) return String.Empty;

#if NET452
        return encoding.GetString(bytes.ToArray());
#else
        fixed (Byte* bytes2 = &MemoryMarshal.GetReference(bytes))
        {
            return encoding.GetString(bytes2, bytes.Length);
        }
#endif
    }

    /// <summary>把字节数组编码为十六进制字符串</summary>
    /// <param name="data">字节数组</param>
    /// <returns></returns>
    public static String ToHex(this ReadOnlySpan<Byte> data)
    {
        if (data.Length == 0) return String.Empty;

        Span<Char> cs = stackalloc Char[data.Length * 2];
        for (Int32 i = 0, j = 0; i < data.Length; i++, j += 2)
        {
            var b = data[i];
            cs[j] = GetHexValue(b >> 4);
            cs[j + 1] = GetHexValue(b & 0x0F);
        }
        return cs.ToString();
    }

    /// <summary>把字节数组编码为十六进制字符串</summary>
    /// <param name="data">字节数组</param>
    /// <param name="maxLength">最大长度</param>
    /// <returns></returns>
    public static String ToHex(this ReadOnlySpan<Byte> data, Int32 maxLength)
    {
        if (data.Length == 0) return String.Empty;

        if (maxLength > 0 && data.Length > maxLength) data = data[..maxLength];

        return data.ToHex();
    }

    /// <summary>把字节数组编码为十六进制字符串</summary>
    /// <param name="data">字节数组</param>
    /// <returns></returns>
    public static String ToHex(this Span<Byte> data) => ToHex((ReadOnlySpan<Byte>)data);

    private static Char GetHexValue(Int32 i) => i < 10 ? (Char)(i + '0') : (Char)(i - 10 + 'A');

    /// <summary>以十六进制编码表示</summary>
    /// <param name="data"></param>
    /// <param name="separate">分隔符</param>
    /// <param name="groupSize">分组大小，为0时对每个字节应用分隔符，否则对每个分组使用</param>
    /// <param name="maxLength">最大显示多少个字节。默认-1显示全部</param>
    /// <returns></returns>
    public static String ToHex(this ReadOnlySpan<Byte> data, String? separate, Int32 groupSize = 0, Int32 maxLength = -1)
    {
        if (data.Length == 0 || maxLength == 0) return String.Empty;

        if (maxLength > 0 && data.Length > maxLength) data = data[..maxLength];
        //return data.ToArray().ToHex(separate, groupSize);

        if (groupSize < 0) groupSize = 0;

        var count = data.Length;
        if (groupSize == 0)
        {
            // 没有分隔符
            if (String.IsNullOrEmpty(separate)) return data.ToHex();

            //// 特殊处理
            //if (separate == "-") return BitConverter.ToString(data, 0, count);
        }

        var len = count * 2;
        if (!separate.IsNullOrEmpty()) len += (count - 1) * separate.Length;
        if (groupSize > 0)
        {
            // 计算分组个数
            var g = (count - 1) / groupSize;
            len += g * 2;
            // 扣除间隔
            if (!separate.IsNullOrEmpty()) _ = g * separate.Length;
        }

        using var sb = new ValueStringBuilder();
        for (var i = 0; i < count; i++)
        {
            if (sb.Length > 0)
            {
                if (groupSize <= 0 || i % groupSize == 0)
                    sb.Append(separate);
            }

            var b = data[i];
            sb.Append(GetHexValue(b >> 4));
            sb.Append(GetHexValue(b & 0x0F));
        }

        return sb.ToString();
    }

    /// <summary>以十六进制编码表示</summary>
    /// <param name="span"></param>
    /// <param name="separate">分隔符</param>
    /// <param name="groupSize">分组大小，为0时对每个字节应用分隔符，否则对每个分组使用</param>
    /// <param name="maxLength">最大显示多少个字节。默认-1显示全部</param>
    /// <returns></returns>
    public static String ToHex(this Span<Byte> span, String? separate, Int32 groupSize = 0, Int32 maxLength = -1)
    {
        if (span.Length == 0 || maxLength == 0) return String.Empty;

        return ToHex((ReadOnlySpan<Byte>)span, separate, groupSize, maxLength);
    }

    /// <summary>通过指定开始与结束边界来截取数据源</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static ReadOnlySpan<T> Substring<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> start, ReadOnlySpan<T> end) where T : IEquatable<T>
    {
        var startIndex = source.IndexOf(start);
        if (startIndex == -1) return [];

        startIndex += start.Length;

        var endIndex = source[startIndex..].IndexOf(end);
        if (endIndex == -1) return [];

        return source.Slice(startIndex, endIndex);
    }

    /// <summary>通过指定开始与结束边界来截取数据源</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static Span<T> Substring<T>(this Span<T> source, ReadOnlySpan<T> start, ReadOnlySpan<T> end) where T : IEquatable<T>
    {
        var startIndex = source.IndexOf(start);
        if (startIndex == -1) return [];

        startIndex += start.Length;

        var endIndex = source[startIndex..].IndexOf(end);
        if (endIndex == -1) return [];

        return source.Slice(startIndex, endIndex);
    }

    /// <summary>在数据源中查找开始与结束边界</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static (Int32 offset, Int32 count) IndexOf<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> start, ReadOnlySpan<T> end) where T : IEquatable<T>
    {
        var startIndex = source.IndexOf(start);
        if (startIndex == -1) return (-1, -1);

        startIndex += start.Length;

        var endIndex = source[startIndex..].IndexOf(end);
        if (endIndex == -1) return (startIndex, -1);

        return (startIndex, endIndex + 1);
    }

    /// <summary>在数据源中查找开始与结束边界</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static (Int32 offset, Int32 count) IndexOf<T>(this Span<T> source, ReadOnlySpan<T> start, ReadOnlySpan<T> end) where T : IEquatable<T>
    {
        var startIndex = source.IndexOf(start);
        if (startIndex == -1) return (-1, -1);

        startIndex += start.Length;

        var endIndex = source[startIndex..].IndexOf(end);
        if (endIndex == -1) return (startIndex, -1);

        return (startIndex, endIndex);
    }
    #endregion

    /// <summary>写入Memory到数据流。从内存池借出缓冲区拷贝，仅作为兜底使用</summary>
    /// <param name="stream"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static void Write(this Stream stream, ReadOnlyMemory<Byte> buffer)
    {
        if (MemoryMarshal.TryGetArray(buffer, out var segment))
        {
            stream.Write(segment.Array!, segment.Offset, segment.Count);

            return;
        }

        var array = ArrayPool<Byte>.Shared.Rent(buffer.Length);

        try
        {
            buffer.Span.CopyTo(array);

            stream.Write(array, 0, buffer.Length);
        }
        finally
        {
            ArrayPool<Byte>.Shared.Return(array);
        }
    }

    /// <summary>写入Memory到数据流。从内存池借出缓冲区拷贝，仅作为兜底使用</summary>
    /// <param name="stream"></param>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task WriteAsync(this Stream stream, ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
    {
        if (MemoryMarshal.TryGetArray(buffer, out var segment))
            return stream.WriteAsync(segment.Array!, segment.Offset, segment.Count, cancellationToken);

        var array = ArrayPool<Byte>.Shared.Rent(buffer.Length);
        buffer.Span.CopyTo(array);

        var writeTask = stream.WriteAsync(array, 0, buffer.Length, cancellationToken);
        return Task.Run(async () =>
        {
            try
            {
                await writeTask.ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<Byte>.Shared.Return(array);
            }
        }, cancellationToken);
    }

#if NETFRAMEWORK || NETSTANDARD
    /// <summary>去掉前后字符</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <param name="trimElement"></param>
    /// <returns></returns>
    public static ReadOnlySpan<T> Trim<T>(this ReadOnlySpan<T> span, T trimElement) where T : IEquatable<T>
    {
        var start = ClampStart(span, trimElement);
        var length = ClampEnd(span, start, trimElement);
        return span.Slice(start, length);
    }

    /// <summary>去掉前后字符</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="span"></param>
    /// <param name="trimElement"></param>
    /// <returns></returns>
    public static Span<T> Trim<T>(this Span<T> span, T trimElement) where T : IEquatable<T>
    {
        var start = ClampStart(span, trimElement);
        var length = ClampEnd(span, start, trimElement);
        return span.Slice(start, length);
    }

    private static Int32 ClampStart<T>(ReadOnlySpan<T> span, T trimElement) where T : IEquatable<T>
    {
        var i = 0;
        for (; i < span.Length; i++)
        {
            ref var reference = ref trimElement;
            if (!reference.Equals(span[i]))
            {
                break;
            }
        }
        return i;
    }

    private static Int32 ClampEnd<T>(ReadOnlySpan<T> span, Int32 start, T trimElement) where T : IEquatable<T>
    {
        var num = span.Length - 1;
        while (num >= start)
        {
            ref var reference = ref trimElement;
            if (!reference.Equals(span[num]))
            {
                break;
            }
            num--;
        }
        return num - start + 1;
    }
#endif


    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static JsonArray ToJsonArray<T>(this T[] array)
    {
        var jsonArray = new JsonArray();
        foreach (var item in array)
        {
            jsonArray.Add(JsonValue.Create(item));
        }
        return jsonArray;
    }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static JsonArray ToJsonArray<T>(this ReadOnlySpan<T> array)
    {
        var jsonArray = new JsonArray();
        foreach (var item in array)
        {
            jsonArray.Add(JsonValue.Create(item));
        }
        return jsonArray;
    }

    public static bool HasValue<T>([NotNullWhen(false)] this IReadOnlyCollection<T>? thisValue)
    {
        return thisValue != null && thisValue.Count > 0;
    }

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IReadOnlyCollection<T>? thisValue)
    {
        return thisValue == null || thisValue.Count <= 0;
    }
    public static bool IsIn<T>(this T thisValue, params T[] values)
    {
        return thisValue != null && values.Contains(thisValue);
    }

    /// <summary>
    /// 将一个数组进行扩充到指定长度，或是缩短到指定长度<br />
    /// </summary>
    public static T[] ArrayExpandToLength<T>(this T[]? data, int length)
    {
        if (data == null)
        {
            return new T[length];
        }

        if (data.Length == length)
        {
            return data;
        }
        Array.Resize(ref data, length);

        return data;
    }

    /// <summary>
    /// 将一个数组进行扩充到指定长度，或是缩短到指定长度<br />
    /// </summary>
    public static Memory<T> ArrayExpandToLength<T>(this Memory<T> data, int length)
    {
        if (data.IsEmpty)
        {
            return Memory<T>.Empty;
        }

        if (data.Length == length)
        {
            return data;
        }

        var result = new T[length];
        data.Slice(0, Math.Min(data.Length, length)).CopyTo(result);
        return result;
    }

    /// <summary>
    /// 将一个数组进行扩充到指定长度，或是缩短到指定长度<br />
    /// </summary>
    public static ReadOnlyMemory<T> ArrayExpandToLength<T>(this ReadOnlyMemory<T> data, int length)
    {
        if (data.IsEmpty)
        {
            return ReadOnlyMemory<T>.Empty;
        }

        if (data.Length == length)
        {
            return data;
        }

        var result = new T[length];
        data.Slice(0, Math.Min(data.Length, length)).CopyTo(result);
        return result;
    }

    /// <summary>
    /// 将一个数组进行扩充到指定长度，或是缩短到指定长度<br />
    /// </summary>
    public static ReadOnlySpan<T> ArrayExpandToLength<T>(this ReadOnlySpan<T> data, int length)
    {
        if (data.IsEmpty)
        {
            return ReadOnlySpan<T>.Empty;
        }

        if (data.Length == length)
        {
            return data;
        }

        var result = new T[length];
        data.Slice(0, Math.Min(data.Length, length)).CopyTo(result);
        return result;
    }


    /// <summary>
    /// 将一个数组进行扩充到指定长度，或是缩短到指定长度<br />
    /// </summary>
    public static Span<T> ArrayExpandToLength<T>(this Span<T> data, int length)
    {
        if (data.IsEmpty)
        {
            return Span<T>.Empty;
        }

        if (data.Length == length)
        {
            return data;
        }

        var result = new T[length];
        data.Slice(0, Math.Min(data.Length, length)).CopyTo(result);
        return result;
    }

    /// <summary>
    /// 将一个数组进行扩充到偶数长度<br />
    /// </summary>
    public static T[] ArrayExpandToLengthEven<T>(this T[] data)
    {
        if (data == null)
        {
            return Array.Empty<T>();
        }

        return data.Length % 2 == 1 ? data.ArrayExpandToLength(data.Length + 1) : data;
    }
    /// <summary>
    /// 将一个数组进行扩充到偶数长度<br />
    /// </summary>
    public static ReadOnlyMemory<T> ArrayExpandToLengthEven<T>(this ReadOnlyMemory<T> data)
    {
        if (data.IsEmpty)
        {
            return Array.Empty<T>();
        }

        return data.Length % 2 == 1 ? data.ArrayExpandToLength(data.Length + 1) : data;
    }


    /// <summary>
    /// 将一个数组进行扩充到偶数长度<br />
    /// </summary>
    public static Memory<T> ArrayExpandToLengthEven<T>(this Memory<T> data)
    {
        if (data.IsEmpty)
        {
            return Array.Empty<T>();
        }

        return data.Length % 2 == 1 ? data.ArrayExpandToLength(data.Length + 1) : data;
    }




    public static T[] ArrayRemoveBegin<T>(this T[] value, int length) => ArrayRemoveDouble(value, length, 0);
    public static T[] ArrayRemoveLast<T>(this T[] value, int length) => ArrayRemoveDouble(value, 0, length);

    public static ReadOnlySpan<T> ArrayRemoveBegin<T>(ReadOnlySpan<T> value, int length) => ArrayRemoveDouble(value, length, 0);
    public static ReadOnlySpan<T> ArrayRemoveLast<T>(ReadOnlySpan<T> value, int length) => ArrayRemoveDouble(value, 0, length);

    public static ReadOnlyMemory<T> ArrayRemoveBegin<T>(ReadOnlyMemory<T> value, int length) => ArrayRemoveDouble(value, length, 0);
    public static ReadOnlyMemory<T> ArrayRemoveLast<T>(ReadOnlyMemory<T> value, int length) => ArrayRemoveDouble(value, 0, length);

    /// <summary>
    /// 从数组中移除指定数量的元素，并返回新的数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="value">要移除元素的数组</param>
    /// <param name="leftLength">从左侧移除的元素个数</param>
    /// <param name="rightLength">从右侧移除的元素个数</param>
    /// <returns>移除元素后的新数组</returns>
    public static T[] ArrayRemoveDouble<T>(T[] value, int leftLength, int rightLength)
    {
        return ArrayRemoveDouble((ReadOnlySpan<T>)value, leftLength, rightLength).ToArray();
    }
    /// <summary>
    /// 从数组中移除指定数量的元素，并返回新的数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="value">要移除元素的数组</param>
    /// <param name="leftLength">从左侧移除的元素个数</param>
    /// <param name="rightLength">从右侧移除的元素个数</param>
    /// <returns>移除元素后的新数组</returns>
    public static ReadOnlySpan<T> ArrayRemoveDouble<T>(ReadOnlySpan<T> value, int leftLength, int rightLength)
    {
        // 如果输入数组为空或者剩余长度不足以移除左右两侧指定的元素，则返回空数组
        if (value.IsEmpty || value.Length <= leftLength + rightLength)
        {
            return Array.Empty<T>();
        }

        // 计算新数组的长度
        int newLength = value.Length - leftLength - rightLength;

        return value.Slice(leftLength, newLength);
    }
    /// <summary>
    /// 从数组中移除指定数量的元素，并返回新的数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="value">要移除元素的数组</param>
    /// <param name="leftLength">从左侧移除的元素个数</param>
    /// <param name="rightLength">从右侧移除的元素个数</param>
    /// <returns>移除元素后的新数组</returns>
    public static ReadOnlyMemory<T> ArrayRemoveDouble<T>(ReadOnlyMemory<T> value, int leftLength, int rightLength)
    {
        // 如果输入数组为空或者剩余长度不足以移除左右两侧指定的元素，则返回空数组
        if (value.IsEmpty || value.Length <= leftLength + rightLength)
        {
            return Array.Empty<T>();
        }

        // 计算新数组的长度
        int newLength = value.Length - leftLength - rightLength;

        return value.Slice(leftLength, newLength);
    }

    /// <summary>
    /// 将指定的数据按照指定长度进行分割
    /// </summary>
    public static List<T[]> ArraySplitByLength<T>(this T[] array, int length)
    {
        if (array == null || array.Length == 0)
        {
            return new List<T[]>();
        }

        int arrayLength = array.Length;
        int numArrays = (arrayLength + length - 1) / length; // 计算所需的数组数量

        List<T[]> objArrayList = new List<T[]>(numArrays);
        for (int i = 0; i < arrayLength; i += length)
        {
            int remainingLength = Math.Min(arrayLength - i, length);
            T[] destinationArray = new T[remainingLength];
            Array.Copy(array, i, destinationArray, 0, remainingLength);
            objArrayList.Add(destinationArray);
        }

        return objArrayList;
    }

    public static IEnumerable<List<T>> ChunkBetter<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (chunkSize <= 0) yield break;

        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var chunk = new List<T>(chunkSize) { enumerator.Current };
            for (int i = 1; i < chunkSize && enumerator.MoveNext(); i++)
            {
                chunk.Add(enumerator.Current);
            }

            yield return chunk;
        }
    }
    public static IEnumerable<ReadOnlyMemory<T>> ChunkBetter<T>(this ReadOnlyMemory<T> span, int groupSize)
    {
        for (int i = 0; i < span.Length; i += groupSize)
        {
            yield return span.Slice(i, Math.Min(groupSize, span.Length - i));
        }
    }

    public static IEnumerable<ReadOnlySequence<T>> ChunkBetter<T>(this ReadOnlySequence<T> sequence, int groupSize)
    {
        if (groupSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(groupSize));

        var start = sequence.Start;
        var remaining = sequence.Length;

        while (remaining > 0)
        {
            var len = (int)Math.Min(groupSize, remaining);

            // 计算 chunk 的 end 位置
            var end = sequence.GetPosition(len, start);

            yield return sequence.Slice(start, end);

            // 移动起始位置
            start = end;
            remaining -= len;
        }
    }

    /// <summary>拷贝当前的实例数组，是基于引用层的浅拷贝，如果类型为值类型，那就是深度拷贝，如果类型为引用类型，就是浅拷贝</summary>
    public static T[] CopyArray<T>(this T[] value)
    {
        if (value == null)
        {
            return Array.Empty<T>();
        }

        T[] destinationArray = new T[value.Length];
        Array.Copy(value, destinationArray, value.Length);
        return destinationArray;
    }

    /// <summary>将一个一维数组中的所有数据按照行列信息拷贝到二维数组里，返回当前的二维数组</summary>
    public static T[,] CreateTwoArrayFromOneArray<T>(this T[] array, int row, int col)
    {
        T[,] arrayFromOneArray = new T[row, col];
        int index = 0;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                arrayFromOneArray[i, j] = array[index++];
                if (index >= array.Length) return arrayFromOneArray; // 防止数组越界
            }
        }

        return arrayFromOneArray;
    }



    /// <summary>
    /// 选择数组中的最后几个元素组成新的数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="value">输入数组</param>
    /// <param name="length">选择的元素个数</param>
    /// <returns>由最后几个元素组成的新数组</returns>
    public static T[] SelectLast<T>(this T[] value, int length) => ArrayRemoveBegin(value, value.Length - length);

    /// <summary>
    /// 从数组中获取指定索引开始的中间一段长度的子数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="value">输入数组</param>
    /// <param name="index">起始索引</param>
    /// <param name="length">选择的元素个数</param>
    /// <returns>中间指定长度的子数组</returns>
    public static T[] SelectMiddle<T>(this T[] value, int index, int length) => ArrayRemoveDouble(value, index, value.Length - index - length);

    /// <summary>
    /// 数组内容分别相加某个数字
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] BytesAdd(this byte[] bytes, int value)
    {
        return BytesAdd((ReadOnlySpan<byte>)bytes, value);
    }
    /// <summary>
    /// 数组内容分别相加某个数字
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] BytesAdd(this Span<byte> bytes, int value)
    {
        return BytesAdd((ReadOnlySpan<byte>)bytes, value);
    }

    /// <summary>
    /// 数组内容分别相加某个数字
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] BytesAdd(this ReadOnlySpan<byte> bytes, int value)
    {
        if (bytes.IsEmpty) return Array.Empty<byte>();
        byte[] result = new byte[bytes.Length];
        for (int index = 0; index < bytes.Length; index++)
        {
            result[index] = (byte)(bytes[index] + value);
        }

        return result;
    }


    /// <summary>
    /// 将 ReadOnlySequence 的每个字节加上指定值，返回新的 byte 数组。
    /// </summary>
    public static byte[] BytesAdd(this ReadOnlySequence<byte> sequence, int value)
    {
        if (sequence.Length == 0)
            return Array.Empty<byte>();

        byte[] result = new byte[sequence.Length];
        int offset = 0;

        foreach (var segment in sequence)
        {
            var span = segment.Span;
            for (int i = 0; i < span.Length; i++)
            {
                result[offset + i] = (byte)(span[i] + value);
            }
            offset += span.Length;
        }

        return result;
    }

    /// <summary>
    /// 获取异或校验,返回ASCII十六进制字符串的字节数组<br />
    /// </summary>
    public static byte[] GetAsciiXOR(this ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return Array.Empty<byte>();

        byte xor = data[0];
        for (int i = 1; i < data.Length; i++)
        {
            xor ^= data[i];
        }

        // 将结果转换为 2 位 ASCII 十六进制字符串，如 "3F" -> [0x33, 0x46]
        byte[] result = Encoding.ASCII.GetBytes(xor.ToString("X2"));
        return result;
    }






    /// <summary>
    /// 将整数进行有效的拆分成数组，指定每个元素的最大值
    /// </summary>
    /// <param name="integer">整数信息</param>
    /// <param name="everyLength">单个的数组长度</param>
    /// <returns>拆分后的数组长度</returns>
    public static int[] SplitIntegerToArray(int integer, int everyLength)
    {
        int[] array = new int[(integer / everyLength) + (integer % everyLength == 0 ? 0 : 1)];
        for (int index = 0; index < array.Length; ++index)
            array[index] = index != array.Length - 1 ? everyLength : (integer % everyLength == 0 ? everyLength : integer % everyLength);
        return array;
    }

    /// <summary>
    /// 拼接任意个泛型数组为一个总的泛型数组对象。
    /// </summary>
    /// <typeparam name="T">数组的类型信息</typeparam>
    /// <param name="arrays">任意个长度的数组</param>
    /// <returns>拼接之后的最终的结果对象</returns>
    public static T[] SpliceArray<T>(params T[][] arrays)
    {
        if (arrays == null || arrays.Length == 0)
            return Array.Empty<T>();

        // 预先计算所有数组的总长度，避免多次扩容
        int totalLength = 0;
        foreach (var array in arrays)
        {
            if (array != null)
                totalLength += array.Length;
        }

        if (totalLength == 0)
            return Array.Empty<T>();

        // 分配目标数组
        T[] result = new T[totalLength];
        int offset = 0;

        // 拷贝所有数组到目标数组中
        foreach (var array in arrays)
        {
            if (array == null || array.Length == 0)
                continue;

            Array.Copy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }


    /// <summary>
    /// 拼接任意个泛型数组为一个总的泛型数组对象。
    /// </summary>
    /// <typeparam name="T">数组的类型信息</typeparam>
    /// <param name="arrays">任意个长度的数组</param>
    /// <returns>拼接之后的最终的结果对象</returns>
    public static Memory<T> SpliceArray<T>(params Memory<T>[] arrays)
    {
        if (arrays == null || arrays.Length == 0)
            return Array.Empty<T>();

        // 预先计算所有数组的总长度，避免多次扩容
        int totalLength = 0;
        foreach (var array in arrays)
        {
            if (!array.IsEmpty)
                totalLength += array.Length;
        }

        if (totalLength == 0)
            return Array.Empty<T>();

        // 分配目标数组
        Memory<T> result = new T[totalLength];
        int offset = 0;

        // 拷贝所有数组到目标数组中
        foreach (var array in arrays)
        {
            if (array.IsEmpty)
                continue;

            array.CopyTo(result.Slice(offset, array.Length));
            offset += array.Length;
        }

        return result;
    }

    /// <summary>
    /// 按原长度写入字节数组
    /// </summary>
    /// <param name="value"></param>
    /// <param name="trueData"></param>
    /// <returns></returns>
    public static byte[] ByteToBoolByte(this ReadOnlySpan<byte> value, byte trueData = 0xff)
    {
        byte[] bytes = new byte[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            bytes[i] = value[i] != 0 ? (byte)trueData : (byte)0;
        }
        return bytes;
    }
    /// <summary>
    /// 按原长度写入字节数组
    /// </summary>
    /// <param name="value"></param>
    /// <param name="trueData"></param>
    /// <returns></returns>
    public static byte[] ByteToBoolByte(this Span<byte> value, byte trueData = 0xff)
    {
        return ByteToBoolByte((ReadOnlySpan<byte>)value, trueData); ;
    }
    /// <summary>
    /// 按原长度写入字节数组
    /// </summary>
    /// <param name="value"></param>
    /// <param name="trueData"></param>
    /// <returns></returns>
    public static byte[] ByteToBoolByte(this byte[] value, byte trueData = 0xff)
    {
        return ByteToBoolByte((ReadOnlySpan<byte>)value, trueData); ;
    }

    /// <summary>
    /// 按原长度写入字节数组
    /// </summary>
    /// <param name="value"></param>
    /// <param name="trueData"></param>
    /// <returns></returns>
    public static byte[] BoolToByte(this ReadOnlySpan<bool> value, byte trueData = 0xff)
    {
        byte[] bytes = new byte[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            bytes[i] = value[i] ? (byte)trueData : (byte)0;
        }
        return bytes;
    }
    /// <summary>
    /// 按原长度写入字节数组
    /// </summary>
    /// <param name="value"></param>
    /// <param name="trueData"></param>
    /// <returns></returns>
    public static byte[] BoolToByte(this Span<bool> value, byte trueData = 0xff)
    {
        return BoolToByte((ReadOnlySpan<bool>)value, trueData); ;
    }
    /// <summary>
    /// 按原长度写入字节数组
    /// </summary>
    /// <param name="value"></param>
    /// <param name="trueData"></param>
    /// <returns></returns>
    public static byte[] BoolToByte(this bool[] value, byte trueData = 0xff)
    {
        return BoolToByte((ReadOnlySpan<bool>)value, trueData); ;
    }


    /// <summary>
    /// 从 <see cref="ReadOnlySequence{T}"/> 中提取位数组，length 代表位数
    /// </summary>
    /// <param name="sequence">原始字节序列</param>
    /// <param name="length">想要转换的位数，如果超出字节序列长度 * 8，则自动缩小为最大位数</param>
    /// <returns>转换后的布尔数组</returns>
    public static bool[] ByteToBoolArray(this ReadOnlySequence<byte> sequence, int length)
    {
        // 计算字节序列能提供的最大位数
        long maxBitLength = sequence.Length * 8;
        if (length > maxBitLength)
            length = (int)maxBitLength;

        bool[] boolArray = new bool[length];

        int bitIndex = 0; // 目标位索引

        foreach (var segment in sequence)
        {
            var span = segment.Span;
            for (int i = 0; i < span.Length * 8 && bitIndex < length; i++)
            {
                boolArray[bitIndex] = span[i / 8].BoolOnByteIndex(i % 8);
                bitIndex++;
            }
        }

        return boolArray;
    }
    /// <summary>
    /// 从字节数组中提取位数组，length 代表位数
    /// </summary>
    /// <param name="inBytes">原始的字节数组</param>
    /// <param name="length">想要转换的位数，如果超出字节数组长度 * 8，则自动缩小为数组最大长度</param>
    /// <returns>转换后的布尔数组</returns>
    public static bool[] ByteToBoolArray(this ReadOnlySpan<byte> inBytes, int length)
    {
        // 计算字节数组能够提供的最大位数
        int maxBitLength = inBytes.Length * 8;

        // 如果指定长度超出最大位数，则将长度缩小为最大位数
        if (length > maxBitLength)
        {
            length = maxBitLength;
        }

        // 创建对应长度的布尔数组
        bool[] boolArray = new bool[length];

        // 从字节数组中提取位信息并转换为布尔值存储到布尔数组中
        for (int index = 0; index < length; ++index)
        {
            boolArray[index] = inBytes[index / 8].BoolOnByteIndex(index % 8);
        }

        return boolArray;
    }

    /// <summary>
    /// 从字节数组中提取位数组，length 代表位数
    /// </summary>
    /// <param name="inBytes">原始的字节数组</param>
    /// <param name="length">想要转换的位数，如果超出字节数组长度 * 8，则自动缩小为数组最大长度</param>
    /// <returns>转换后的布尔数组</returns>
    public static bool[] ByteToBoolArray(this Span<byte> inBytes, int length)
    {
        return ByteToBoolArray((ReadOnlySpan<byte>)inBytes, length);
    }
    /// <summary>
    /// 从字节数组中提取位数组，length 代表位数
    /// </summary>
    /// <param name="inBytes">原始的字节数组</param>
    /// <param name="length">想要转换的位数，如果超出字节数组长度 * 8，则自动缩小为数组最大长度</param>
    /// <returns>转换后的布尔数组</returns>
    public static bool[] ByteToBoolArray(this byte[] inBytes, int length)
    {
        return ByteToBoolArray((ReadOnlySpan<byte>)inBytes, length);
    }

    /// <summary>
    /// 将压缩的位流（每位表示 true/false，低位在前）转换为按字节存储的数组。
    /// 支持 ReadOnlySequence 输入，可处理多段内存，并指定起始 bit 索引。
    /// </summary>
    /// <param name="inBytes">压缩位流（分段或连续内存）</param>
    /// <param name="length">要转换的位数</param>
    /// <param name="startBitIndex">从 inBytes 的第几个 bit 开始读取</param>
    /// <param name="trueData">每位为 true 时对应的字节值</param>
    /// <returns>展开后的字节数组</returns>
    public static byte[] ByteBitsToBytes(this ReadOnlySequence<byte> inBytes, long length, long startBitIndex, byte trueData = 0xFF)
    {
        if (inBytes.Length == 0 || length <= 0)
            return Array.Empty<byte>();

        long maxBitLength = inBytes.Length * 8;

        // 确保 startBitIndex 不超过总 bit 数
        if (startBitIndex >= maxBitLength)
            return Array.Empty<byte>();

        // 调整 length，不超过剩余可用 bit
        if (startBitIndex + length > maxBitLength)
            length = maxBitLength - startBitIndex;

        byte[] result = new byte[length];

        long bitIndex = 0; // result 的目标索引
        long globalBitIndex = startBitIndex; // inBytes 的全局 bit 索引

        foreach (var segment in inBytes)
        {
            var span = segment.Span;
            for (int i = 0; i < span.Length && bitIndex < length; i++)
            {
                for (int bit = 0; bit < 8 && bitIndex < length; bit++, globalBitIndex++)
                {
                    // 跳过 startBitIndex 之前的 bit
                    if (globalBitIndex < startBitIndex)
                        continue;

                    // 取出当前 bit
                    bool isSet = (span[i] & (1 << bit)) != 0;
                    result[bitIndex++] = isSet ? trueData : (byte)0;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 将压缩的位流（每位表示 true/false，低位在前）转换为按字节存储的数组。
    /// 可指定每个位为 true 时对应的字节值（默认 0xFF）。
    /// length 表示要转换的位数，如果超出原始字节数组可提供的位数，则自动截断。
    /// </summary>
    /// <param name="inBytes">压缩位流</param>
    /// <param name="length">要转换的位数</param>
    /// <param name="startBitIndex">从 inBytes 的第几个 bit 开始读取</param>
    /// <param name="trueData">每位为 true 时对应的字节值</param>
    /// <returns>展开后的字节数组</returns>
    public static byte[] ByteBitsToBytes(this ReadOnlySpan<byte> inBytes, long length, long startBitIndex, byte trueData = 0xFF)
    {
        if (inBytes.Length == 0 || length <= 0)
            return Array.Empty<byte>();

        long maxBitLength = inBytes.Length * 8;

        // 确保 startBitIndex 不超过总 bit 数
        if (startBitIndex >= maxBitLength)
            return Array.Empty<byte>();

        // 调整 length，不超过剩余可用 bit
        if (startBitIndex + length > maxBitLength)
            length = maxBitLength - startBitIndex;

        byte[] result = new byte[length];

        long bitIndex = 0; // result 的目标索引
        long globalBitIndex = startBitIndex; // inBytes 的全局 bit 索引

        var span = inBytes;
        for (int i = 0; i < span.Length && bitIndex < length; i++)
        {
            for (int bit = 0; bit < 8 && bitIndex < length; bit++, globalBitIndex++)
            {
                // 跳过 startBitIndex 之前的 bit
                if (globalBitIndex < startBitIndex)
                    continue;

                // 取出当前 bit
                bool isSet = (span[i] & (1 << bit)) != 0;
                result[bitIndex++] = isSet ? trueData : (byte)0;
            }
        }

        return result;
    }

    /// <summary>
    /// 将压缩的位流（每位表示 true/false，低位在前）转换为按字节存储的数组。
    /// 可指定每个位为 true 时对应的字节值（默认 0xFF）。
    /// length 表示要转换的位数，如果超出原始字节数组可提供的位数，则自动截断。
    /// </summary>
    /// <param name="inBytes">压缩位流</param>
    /// <param name="length">要转换的位数</param>
    /// <param name="trueData">每位为 true 时对应的字节值</param>
    /// <returns>展开后的字节数组</returns>
    public static byte[] ByteBitsToBytes(this Span<byte> inBytes, int length, byte trueData = 0xFF)
    {
        return ByteBitsToBytes((ReadOnlySpan<byte>)inBytes, length, trueData);
    }

    /// <summary>
    /// 将压缩的位流（每位表示 true/false，低位在前）转换为按字节存储的数组。
    /// 可指定每个位为 true 时对应的字节值（默认 0xFF）。
    /// length 表示要转换的位数，如果超出原始字节数组可提供的位数，则自动截断。
    /// </summary>
    /// <param name="inBytes">压缩位流</param>
    /// <param name="length">要转换的位数</param>
    /// <param name="trueData">每位为 true 时对应的字节值</param>
    /// <returns>展开后的字节数组</returns>
    public static byte[] ByteBitsToBytes(this byte[] inBytes, int length, byte trueData = 0xFF)
    {
        return ByteBitsToBytes((ReadOnlySpan<byte>)inBytes, length, trueData);
    }
    /// <summary>
    /// 从 <see cref="ReadOnlySequence{T}"/> 中转换为压缩的字节数组（每个字节不等于0时为true， 每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    /// <param name="sequence">原始字节序列</param>
    /// <returns>转换后的数组</returns>
    public static byte[] ByteToByteArray(this ReadOnlySequence<byte> sequence)
    {
        // 首先取得总长度
        long len = sequence.Length;
        if (len == 0)
            return Array.Empty<byte>();

        int outputLen = (int)((len + 7) / 8); // 每 8 个字节压缩为 1 字节
        byte[] result = new byte[outputLen];

        int bitIndex = 0;
        int byteIndex = 0;

        foreach (var mem in sequence)
        {
            var span = mem.Span;

            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != 0)
                {
                    // 设置低位在前 (LSB first)
                    result[byteIndex] |= (byte)(1 << bitIndex);
                }

                bitIndex++;

                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
        }

        return result;
    }


    /// <summary>
    /// 从 <see cref="ReadOnlySpan{T}"/> 中转换为压缩的字节数组（每个字节不等于0时为true， 每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    /// <param name="span">原始字节</param>
    /// <returns>转换后的数组</returns>
    public static byte[] ByteToByteArray(this ReadOnlySpan<byte> span)
    {
        // 首先取得总长度
        long len = span.Length;
        if (len == 0)
            return Array.Empty<byte>();

        int outputLen = (int)((len + 7) / 8); // 每 8 个字节压缩为 1 字节
        byte[] result = new byte[outputLen];

        int bitIndex = 0;
        int byteIndex = 0;


        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] != 0)
            {
                // 设置低位在前 (LSB first)
                result[byteIndex] |= (byte)(1 << bitIndex);
            }

            bitIndex++;

            if (bitIndex == 8)
            {
                bitIndex = 0;
                byteIndex++;
            }
        }
        return result;
    }

    /// <summary>
    /// 从 <see cref="Span{T}"/> 中转换为压缩的字节数组（每个字节不等于0时为true， 每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    /// <param name="span">原始字节</param>
    /// <returns>转换后的数组</returns>
    public static byte[] ByteToByteArray(this Span<byte> span)
    {
        return ByteToByteArray((ReadOnlySpan<byte>)span);
    }
    /// <summary>
    /// 从 <see cref="Span{T}"/> 中转换为压缩的字节数组（每个字节不等于0时为true， 每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    /// <param name="span">原始字节</param>
    /// <returns>转换后的数组</returns>
    public static byte[] ByteToByteArray(this byte[] span)
    {
        return ByteToByteArray((ReadOnlySpan<byte>)span);
    }


    /// <summary>
    /// 将布尔数组转换为压缩的字节数组（每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    /// <param name="array">布尔数组</param>
    /// <returns>压缩后的只读字节内存</returns>
    public static byte[] BoolArrayToByte(this ReadOnlySpan<bool> array)
    {
        if (array.IsEmpty)
            return Array.Empty<byte>();

        int byteLength = (array.Length + 7) / 8;
        byte[] result = new byte[byteLength];

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i])
            {
                result[i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return result;
    }

    /// <summary>
    /// 将布尔数组转换为压缩的字节数组（每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    public static byte[] BoolArrayToByte(this Span<bool> array)
        => ((ReadOnlySpan<bool>)array).BoolArrayToByte();
    /// <summary>
    /// 将布尔数组转换为压缩的字节数组（每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    public static byte[] BoolArrayToByte(this bool[] array)
        => ((ReadOnlySpan<bool>)array).BoolArrayToByte();

    public static ReadOnlyMemory<byte> CombineMemoryBlocks(this ICollection<ReadOnlyMemory<byte>> blocks)
    {
        if (blocks == null || blocks.Count == 0)
            return ReadOnlyMemory<byte>.Empty;

        // 计算总长度
        int totalLength = 0;
        foreach (var block in blocks)
        {
            totalLength += block.Length;
        }

        if (totalLength == 0)
            return ReadOnlyMemory<byte>.Empty;

        // 分配目标数组
        byte[] result = new byte[totalLength];
        int offset = 0;

        // 拷贝每一段内存
        foreach (var block in blocks)
        {
            block.Span.CopyTo(result.AsSpan(offset));
            offset += block.Length;
        }

        return result;
    }

    public static ReadOnlyMemory<byte> CombineMemoryBlocks(this ReadOnlySpan<ReadOnlyMemory<byte>> blocks)
    {
        if (blocks == null || blocks.Length == 0)
            return ReadOnlyMemory<byte>.Empty;

        // 计算总长度
        int totalLength = 0;
        foreach (var block in blocks)
        {
            totalLength += block.Length;
        }

        if (totalLength == 0)
            return ReadOnlyMemory<byte>.Empty;

        // 分配目标数组
        byte[] result = new byte[totalLength];
        int offset = 0;

        // 拷贝每一段内存
        foreach (var block in blocks)
        {
            block.Span.CopyTo(result.AsSpan(offset));
            offset += block.Length;
        }

        return result;
    }

    public static string ArrayToString(this ReadOnlySpan<string> blocks)
    {
        using var sb = new ValueStringBuilder();

        // 拷贝每一段内存
        foreach (var block in blocks)
        {
            sb.Append(block);
        }

        return sb.ToString();
    }

    public static string ToHexString(ReadOnlySequence<byte> InBytes, char segment = default, int newLineCount = 0) => ToHexString(InBytes, 0, InBytes.Length, segment, newLineCount);

    public static string ToHexString(ReadOnlySequence<byte> sequence, long offset, long length, char segment = default, int newLineCount = 0)
    {
        if (length <= 0 || offset < 0 || sequence.Length < offset + length)
            return string.Empty;

        var estimatedSize = length * (segment != default ? 3 : 2) + (length / Math.Max(newLineCount, int.MaxValue));
        using ValueStringBuilder sb = new ValueStringBuilder((int)estimatedSize);

        int totalConsumed = 0;
        int totalWritten = 0;

        foreach (var memory in sequence)
        {
            var span = memory.Span;

            for (int i = 0; i < span.Length && totalWritten < length; i++)
            {
                if (totalConsumed >= offset)
                {
                    sb.Append(span[i].ToString("X2"));

                    if (totalWritten < length - 1)
                    {
                        if (segment != default)
                            sb.Append(segment);

                        if (newLineCount > 0 && (totalWritten + 1) % newLineCount == 0)
                            sb.Append(Environment.NewLine);
                    }

                    totalWritten++;
                }

                totalConsumed++;
                if (totalConsumed >= offset + length)
                    break;
            }

            if (totalWritten >= length)
                break;
        }

        return sb.ToString();
    }


    /// <summary>
    /// 获取Byte数组的第 boolIndex 偏移的bool值，这个偏移值可以为 10，就是第 1 个字节的 第3位 <br />
    /// </summary>
    /// <param name="bytes">字节数组信息</param>
    /// <param name="boolIndex">指定字节的位偏移</param>
    /// <returns>bool值</returns>
    public static bool GetBoolByIndex(this ReadOnlySpan<byte> bytes, int boolIndex)
    {
        return bytes[boolIndex / 8].BoolOnByteIndex(boolIndex % 8);
    }
    /// <summary>
    /// 获取byte数据类型的第offset位，是否为True<br />
    /// </summary>
    /// <param name="value">byte数值</param>
    /// <param name="offset">索引位置</param>
    /// <returns>结果</returns>
    public static bool BoolOnByteIndex(this byte value, int offset)
    {
        if (offset < 0 || offset > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset value must be between 0 and 7.");
        }

        byte mask = (byte)(1 << offset);
        return (value & mask) == mask;
    }


    /// <summary>
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐<br />
    /// </summary>
    /// <param name="inBytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    /// <summary>
    /// 将字节数组按“字（2字节）”为单位反转高低位，奇数长度自动补齐 0。
    /// </summary>
    public static Memory<byte> BytesReverseByWord(this Memory<byte> inBytes)
    {
        int len = inBytes.Length;
        if (len == 0)
            return inBytes;

        // 如果是奇数，自动补齐 0
        int evenLen = (len % 2 == 0) ? len : len + 1;
        var memory = inBytes;
        Memory<byte> result = new byte[evenLen];
        memory.CopyTo(result);
        memory = result;
        var span = memory.Span;
        // 逐字（2 字节）交换
        for (int i = 0; i < evenLen; i += 2)
        {
            byte temp = span[i];
            span[i] = span[i + 1];
            span[i + 1] = temp;
        }
        return memory;
    }

    /// <summary>
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐<br />
    /// </summary>
    /// <param name="inBytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    /// <summary>
    /// 将字节数组按“字（2字节）”为单位反转高低位，奇数长度自动补齐 0。
    /// </summary>
    public static ReadOnlyMemory<byte> BytesReverseByWord(this ReadOnlyMemory<byte> inBytes)
    {
        int len = inBytes.Length;
        if (len == 0)
            return inBytes;

        // 如果是奇数，自动补齐 0
        int evenLen = (len % 2 == 0) ? len : len + 1;

        var memory = inBytes;
        Memory<byte> result = new byte[evenLen];
        memory.CopyTo(result);
        memory = result;
        var span = result.Span;
        // 逐字（2 字节）交换
        for (int i = 0; i < evenLen; i += 2)
        {
            byte temp = span[i];
            span[i] = span[i + 1];
            span[i + 1] = temp;
        }
        return result;
    }
    /// <summary>
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐<br />
    /// </summary>
    /// <param name="inBytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    /// <summary>
    /// 将字节数组按“字（2字节）”为单位反转高低位，奇数长度自动补齐 0。
    /// </summary>
    public static Span<byte> BytesReverseByWord(this Span<byte> inBytes)
    {
        int len = inBytes.Length;
        if (len == 0)
            return inBytes;

        // 如果是奇数，自动补齐 0
        int evenLen = (len % 2 == 0) ? len : len + 1;

        if (evenLen != len)
        {
            Span<byte> result = new byte[evenLen];
            inBytes.CopyTo(result);
            inBytes = result;
        }

        // 逐字（2 字节）交换
        for (int i = 0; i < evenLen; i += 2)
        {
            byte temp = inBytes[i];
            inBytes[i] = inBytes[i + 1];
            inBytes[i + 1] = temp;
        }
        return inBytes;

    }

    /// <summary>
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐<br />
    /// </summary>
    /// <param name="inBytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    /// <summary>
    /// 将字节数组按“字（2字节）”为单位反转高低位，奇数长度自动补齐 0。
    /// </summary>
    public static ReadOnlySpan<byte> BytesReverseByWord(this ReadOnlySpan<byte> inBytes)
    {
        int len = inBytes.Length;
        if (len == 0)
            return ReadOnlySpan<byte>.Empty;

        // 如果是奇数，自动补齐 0
        int evenLen = (len % 2 == 0) ? len : len + 1;

        Span<byte> result = new byte[evenLen];
        inBytes.CopyTo(result);

        // 逐字（2 字节）交换
        for (int i = 0; i < evenLen; i += 2)
        {
            byte temp = result[i];
            result[i] = result[i + 1];
            result[i + 1] = temp;
        }

        return result;
    }


    /// <summary>
    /// 根据指定的字节数组和Bcd格式返回对应的Bcd值
    /// </summary>
    /// <param name="buffer">输入的字节数组</param>
    /// <param name="format">Bcd格式枚举</param>
    /// <returns>转换后的Bcd值字符串</returns>
    public static string GetBcdValue(this ReadOnlySpan<byte> buffer, BcdFormatEnum format)
    {
        // 用于存储最终的Bcd值的字符串构建器
        using var stringBuilder = new ValueStringBuilder();


        // 遍历字节数组进行Bcd值计算
        for (int index = 0; index < buffer.Length; ++index)
        {
            // 获取当前字节的低四位和高四位
            int num1 = buffer[index] & 15;
            int num2 = buffer[index] >> 4;

            // 根据指定的Bcd格式将每个字节转换为Bcd并追加到字符串构建器中
            stringBuilder.Append(StringHelper.GetBcdFromByte(num2, format));
            stringBuilder.Append(StringHelper.GetBcdFromByte(num1, format));
        }

        // 返回最终的Bcd值字符串
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <returns></returns>
    public static string ToHexString(this Span<byte> buffer, char segment = default, int newLineCount = 0)
    {
        return ToHexString((ReadOnlySpan<byte>)buffer, segment, newLineCount);
    }
    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <returns></returns>
    public static string ToHexString(this byte[] buffer, char segment = default, int newLineCount = 0)
    {
        return ToHexString((ReadOnlySpan<byte>)buffer, segment, newLineCount);
    }
    /// <summary>
    /// 将字节数组转换为十六进制表示的字符串
    /// </summary>
    /// <param name="buffer">输入的字节数组</param>
    /// <param name="segment">用于分隔每个字节的字符</param>
    /// <param name="newLineCount">指定在何处换行，设为0则不换行</param>
    /// <returns>转换后的十六进制字符串</returns>
    public static string ToHexString(this ReadOnlySpan<byte> buffer, char segment = default, int newLineCount = 0)
    {
        // 合法性检查：避免越界和空输入
        if (buffer.IsEmpty)
            return string.Empty;

        var offset = 0;
        var length = buffer.Length;
        // 每个字节占 2 个字符，如果有分隔符 +1，再加换行（估算上限）
        int estimatedSize = length * (segment != default ? 3 : 2) + (length / Math.Max(newLineCount, int.MaxValue));
        using ValueStringBuilder sb = new ValueStringBuilder(estimatedSize);

        int end = offset + length;
        for (int i = offset; i < end; i++)
        {
            sb.Append(buffer[i].ToString("X2")); // 转大写16进制



            if (newLineCount > 0 && (i - offset + 1) % newLineCount == 0 && i! != end - 1)
                sb.Append(Environment.NewLine);
            else if (segment != default && i < end - 1)
                sb.Append(segment);
        }

        return sb.ToString();
    }

}