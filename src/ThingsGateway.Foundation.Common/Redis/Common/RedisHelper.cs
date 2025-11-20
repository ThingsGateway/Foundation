using ThingsGateway.Foundation.Common.Buffers;

namespace ThingsGateway.Foundation.Common.Caching;

/// <summary>Redis助手</summary>
public static class RedisHelper
{
    /// <summary>获取Span</summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public static Span<T> GetSpan<T>(this IMemoryOwner<T> owner)
    {
        if (owner is MemoryManager<T> manager)
            return manager.GetSpan();

        return owner.Memory.Span;
    }

    internal static Int32 Read(this Stream stream, Span<Byte> buffer)
    {
        var array = ArrayPool<Byte>.Shared.Rent(buffer.Length);
        try
        {
            var num = stream.Read(array, 0, buffer.Length);
            if ((UInt32)num > (UInt32)buffer.Length)
                throw new IOException("IO_StreamTooLong");

            new ReadOnlySpan<Byte>(array, 0, num).CopyTo(buffer);
            return num;
        }
        finally
        {
            ArrayPool<Byte>.Shared.Return(array);
        }
    }

    internal static void WriteAsString(this ref SpanWriter writer, Int64 num)
    {
        Span<Byte> buf = stackalloc Byte[16];

        // 从右向左填充数字
        var n = 0;
        do
        {
            buf[^++n] = (Byte)(num % 10 + '0');
            num /= 10;
        } while (num > 0);

        buf.Slice(buf.Length - n, n).CopyTo(writer.GetSpan());
        writer.Advance(n);
    }
}