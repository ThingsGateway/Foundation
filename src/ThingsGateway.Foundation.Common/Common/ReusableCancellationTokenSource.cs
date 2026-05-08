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
    private readonly object _lock = new();

    private CancellationTokenSource? _cts;

    private int _version;

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
        // 在锁外获取新 CTS（可能耗时）
        var newCts = _linkedCtsCache.GetLinkedTokenSource(external1, external2, external3);

        CancellationToken token;
        CancellationTokenSource? oldCtsToDispose = null;
        lock (_lock)
        {
            Volatile.Write(ref _timeoutStatus, 0);

            if (!ReferenceEquals(newCts, _cts))
            {
                oldCtsToDispose = _cts;
                _cts = newCts;
            }

            // 版本递增
            Interlocked.Increment(ref _version);

            // 启动 Timer
            _timer.Change(timeout, Timeout.Infinite);

            token = _cts!.Token;
        }

        // 在锁外取消并释放旧 CTS，避免 Cancel 同步回调尝试获取 _lock 导致死锁
        if (oldCtsToDispose != null)
        {
            try { oldCtsToDispose.Cancel(); } catch { }
            try { oldCtsToDispose.Dispose(); } catch { }
        }

        return token;
    }

    /// <summary>
    /// 停止超时（例如成功完成）
    /// </summary>
    public void Set()
    {
        lock (_lock)
        {
            // 递增版本使当前 Timer 回调失效
            Interlocked.Increment(ref _version);
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    /// <summary>
    /// 手动取消
    /// </summary>
    public void Cancel()
    {
        CancellationTokenSource? cts;
        lock (_lock)
        {
            // 递增版本，防止 Timer 回调再次取消
            Interlocked.Increment(ref _version);
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            cts = _cts;
        }
        try
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
        }
        catch { }
    }

    private static void OnTimeout(object? state)
    {
        if (state is not ReusableCancellationTokenSource self)
            return;

        CancellationTokenSource? cts;
        int versionAtCapture;

        // 在锁内原子地：读版本 + 读 CTS + 写 timeoutStatus
        // 这样就不会和 GetTokenSource 的版本递增 + CTS 替换产生竞态
        lock (self._lock)
        {
            versionAtCapture = Volatile.Read(ref self._version);
            cts = self._cts;
        }

        // 锁外取消，避免 Cancel 同步回调尝试获取 _lock 导致死锁
        // 取消前验证版本号，防止 Set/GetTokenSource 已经更新
        if (versionAtCapture != Volatile.Read(ref self._version))
            return;

        Volatile.Write(ref self._timeoutStatus, 1);
        try { cts?.Cancel(); } catch { }
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