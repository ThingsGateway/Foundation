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

    // 版本号：用于隔离 Timer 回调
    private int _version;

    // 当前生效版本
    private int _currentVersion;

    // 超时状态（线程安全）
    private int _timeoutStatus;

    public bool TimeoutStatus => Volatile.Read(ref _timeoutStatus) == 1;

    private readonly LinkedCancellationTokenSourceCache _linkedCtsCache = new();

    public ReusableCancellationTokenSource()
    {
        _timer = new Timer(OnTimeout, this, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// 获取 Token，并启动超时控制
    /// </summary>
    public CancellationToken GetTokenSource(
        long timeout,
        CancellationToken external1 = default,
        CancellationToken external2 = default,
        CancellationToken external3 = default)
    {
        Volatile.Write(ref _timeoutStatus, 0);

        // 获取（或复用）CTS
        var cts = _linkedCtsCache.GetLinkedTokenSource(external1, external2, external3);

        if (!ReferenceEquals(cts, _cts))
        {
            _cts?.Dispose();
            _cts = cts;
        }

        // 版本号递增（关键）
        var version = Interlocked.Increment(ref _version);
        Volatile.Write(ref _currentVersion, version);

        // 启动 Timer
        _timer.Change(timeout, Timeout.Infinite);

        return _cts.Token;
    }

    /// <summary>
    /// 停止超时（例如成功完成）
    /// </summary>
    public void Set()
    {
        // 失效当前版本
        Interlocked.Increment(ref _version);

        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// 手动取消
    /// </summary>
    public void Cancel()
    {
        var cts = _cts;
        try
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }
        catch { }
    }

    private static void OnTimeout(object? state)
    {
        if (state is not ReusableCancellationTokenSource self)
            return;

        if (Volatile.Read(ref self._currentVersion) != Volatile.Read(ref self._version))
            return;

        Volatile.Write(ref self._timeoutStatus, 1);

        var cts = self._cts;

        if (cts != null && !cts.IsCancellationRequested)
        {
            try { cts.Cancel(); } catch { }
        }
    }

    public void Dispose()
    {
        try { _timer.Dispose(); } catch { }

        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        catch { }

        try { _linkedCtsCache.Dispose(); } catch { }
        GC.SuppressFinalize(this);
    }
}