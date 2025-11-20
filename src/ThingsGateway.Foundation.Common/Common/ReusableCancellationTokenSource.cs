namespace ThingsGateway.Foundation.Common;

using System;
using System.Threading;

public sealed class ReusableCancellationTokenSource : IDisposable
{
    ~ReusableCancellationTokenSource()
    {
        Dispose();
    }

    private readonly Timer _timer;
    private CancellationTokenSource? _cts;

    public ReusableCancellationTokenSource()
    {
        _timer = new Timer(OnTimeout, null, Timeout.Infinite, Timeout.Infinite);
    }

    public bool TimeoutStatus;

    private void OnTimeout(object? state)
    {
        try
        {
            TimeoutStatus = true;

            if (_cts?.IsCancellationRequested == false)
                _cts?.Cancel();
        }
        catch
        {

        }
    }

    private readonly LinkedCancellationTokenSourceCache _linkedCtsCache = new();

    /// <summary>
    /// 获取一个 CTS，并启动超时
    /// </summary>
    public CancellationToken GetTokenSource(long timeout, CancellationToken external1 = default, CancellationToken external2 = default, CancellationToken external3 = default)
    {
        TimeoutStatus = false;

        // 创建新的 CTS
        _cts = _linkedCtsCache.GetLinkedTokenSource(external1, external2, external3);

        // 启动 Timer
        _timer.Change(timeout, Timeout.Infinite);

        return _cts.Token;
    }


    public void Set()
    {
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// 手动取消
    /// </summary>
    public void Cancel()
    {
        try { _cts?.Cancel(); } catch { }
    }

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        try { _cts?.Dispose(); } catch { }
        try { _linkedCtsCache?.Dispose(); } catch { }
        try { _timer?.Dispose(); } catch { }
        GC.SuppressFinalize(this);
    }
}



