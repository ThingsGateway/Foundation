using System.Buffers;
namespace ThingsGateway.Foundation.OpcUa;

public sealed class SimpleBufferWriter<T> : IBufferWriter<T>, IDisposable
{
    private T[] _buffer;
    private int _written;
    private readonly bool _clearOnDispose;

    public SimpleBufferWriter(int initialSize = 256, bool clearOnDispose = false)
    {
        _buffer = ArrayPool<T>.Shared.Rent(initialSize);
        _written = 0;
        _clearOnDispose = clearOnDispose;
    }

    public int WrittenCount => _written;

    public ReadOnlyMemory<T> WrittenMemory => _buffer.AsMemory(0, _written);

    public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _written);

    public void Advance(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (_written + count > _buffer.Length)
            throw new InvalidOperationException("Cannot advance past buffer length");

        _written += count;
    }

    public Memory<T> GetMemory(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return _buffer.AsMemory(_written);
    }

    public Span<T> GetSpan(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return _buffer.AsSpan(_written);
    }

    private void Ensure(int sizeHint)
    {
        if (sizeHint < 0) throw new ArgumentOutOfRangeException(nameof(sizeHint));

        if (sizeHint == 0) sizeHint = 1; // Utf8JsonWriter 常会传 0

        int required = _written + sizeHint;

        if (required > _buffer.Length)
        {
            int newSize = Math.Max(required, _buffer.Length * 2);
            var newBuffer = ArrayPool<T>.Shared.Rent(newSize);

            Array.Copy(_buffer, newBuffer, _written);

            ArrayPool<T>.Shared.Return(_buffer, _clearOnDispose);
            _buffer = newBuffer;
        }
    }

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(_buffer, _clearOnDispose);
        _buffer = null!;
        _written = 0;
    }
}
