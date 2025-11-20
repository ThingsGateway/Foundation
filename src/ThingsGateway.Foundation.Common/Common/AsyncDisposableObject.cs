using System.Runtime.CompilerServices;

namespace ThingsGateway.Foundation.Common;

/// <summary>
/// 具有释放的对象。内部实现了<see cref="GC.SuppressFinalize(object)"/>，但不包括析构函数相关。
/// </summary>
public abstract partial class AsyncDisposableObject : IDisposableValue, IAsyncDisposable
{
    /// <summary>
    /// 判断当前对象是否已经被释放。
    /// 如果已经被释放，则抛出<see cref="ObjectDisposedException"/>异常。
    /// </summary>
    /// <exception cref="ObjectDisposedException">当对象已经被释放时抛出此异常</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfDisposed()
    {
        ObjectDisposedExceptionEx.ThrowIf(this.m_disposedValue, this);
    }

    private int m_asyncCount;
    /// <summary>
    /// 判断是否已释放。
    /// </summary>
    private volatile bool m_disposedValue;

    /// <inheritdoc/>
    public bool DisposedValue => this.m_disposedValue;

    /// <summary>
    /// 处置资源
    /// </summary>
    /// <param name="disposing">一个值，表示是否释放托管资源</param>
    protected virtual Task DisposeAsync(bool disposing)
    {
        // 标记当前对象为已处置状态
        this.m_disposedValue = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 释放资源。内部已经处理了<see cref="GC.SuppressFinalize(object)"/>
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (this.DisposedValue)
        {
            return;
        }

        if (Interlocked.Increment(ref this.m_asyncCount) == 1)
        {
            await this.DisposeAsync(disposing: true).ConfigureAwait(false);
        }
        GC.SuppressFinalize(this);
    }
}