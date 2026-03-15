namespace ThingsGateway.Foundation.Common;

using System;
using System.Threading;

public sealed class ReusableCancellationTokenSource : IDisposable
{
    ~ReusableCancellationTokenSource()
    {
        Dispose();
    }
    private int _timeoutEnabled;
    private readonly Timer _timer;
    private CancellationTokenSource? _cts;

    public ReusableCancellationTokenSource()
    {
        _timer = new Timer(OnTimeout, this, Timeout.Infinite, Timeout.Infinite);
    }

    public bool TimeoutStatus;

    private static void OnTimeout(object? state)
    {
        if (state is not ReusableCancellationTokenSource @this) return;

        if (Volatile.Read(ref @this._timeoutEnabled) == 0) return;
        try
        {


            @this.TimeoutStatus = true;

            if (@this._cts?.IsCancellationRequested == false)
                @this._cts?.Cancel();
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
        var data = _linkedCtsCache.GetLinkedTokenSource(external1, external2, external3);
        if (!data.Equals(_cts))
        {
            _cts?.Dispose();
            _cts = data;
        }
        Volatile.Write(ref _timeoutEnabled, 1);
        // 启动 Timer
        _timer.Change(timeout, Timeout.Infinite);

        return _cts.Token;
    }


    public void Set()
    {
        Volatile.Write(ref _timeoutEnabled, 0);
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



