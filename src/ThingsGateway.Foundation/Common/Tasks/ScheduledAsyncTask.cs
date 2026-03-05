using ThingsGateway.Foundation.Common.PooledAwait;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

public class ScheduledAsyncTask : DisposeBase, IScheduledTask, IScheduledIntIntervalTask
{
    public int IntervalMS { get; }
    private readonly Func<object?, CancellationToken, Task>? _taskFunc;
    private readonly Func<object?, CancellationToken, ValueTask>? _valueTaskFunc;
    private readonly CancellationToken _token;
    private TimerX? _timer;
    private object? _state;
    private ILog LogMessage;
    public Int32 Period => _timer?.Period ?? 0;
    public bool Enable => _timer?.Disposed != false ? false : true;
    public ScheduledAsyncTask(int interval, Func<object?, CancellationToken, Task> taskFunc, object? state, ILog log, CancellationToken token)
    {
        IntervalMS = interval;
        LogMessage = log;
        _state = state;
        _taskFunc = taskFunc;
        _token = token;
    }
    public ScheduledAsyncTask(int interval, Func<object?, CancellationToken, ValueTask> taskFunc, object? state, ILog log, CancellationToken token)
    {
        IntervalMS = interval;
        LogMessage = log;
        _state = state;
        _valueTaskFunc = taskFunc;
        _token = token;
    }
    private bool Check()
    {
        if (_token.IsCancellationRequested)
        {
            Dispose();
            return true;
        }
        return false;
    }
    static ScheduledAsyncTask()
    {
        HighMax = Math.Max(Environment.ProcessorCount / 2, 1);
    }
    private readonly static int HighMax; // 允许的最大序号
    private static int _highIndex;
    private static string NextHighName()
    {
        var v = Interlocked.Increment(ref _highIndex);

        // 映射到 1..HighMax
        var index = (v - 1) % HighMax + 1;

        return $"{nameof(IScheduledTask)}High{index}";
    }
    public void Start()
    {
        _timer?.Dispose();
        if (!Check())
        {
            if (IntervalMS < 1000)
            {
                _timer = new TimerX(DoAsync, _state, IntervalMS, IntervalMS, NextHighName()) { Async = true, Reentrant = false };
            }
            else
            {
                _timer = new TimerX(DoAsync, _state, IntervalMS, IntervalMS, $"{nameof(IScheduledTask)}") { Async = true, Reentrant = false };
            }
        }

    }

    private ValueTask DoAsync(object? state)
    {
        return DoAsync(this, state);
        static async PooledValueTask DoAsync(ScheduledAsyncTask @this, object? state)
        {
            if (@this.Check())
                return;

            if (@this._taskFunc == null && @this._valueTaskFunc == null)
            {
                @this.Dispose();
                return;
            }

            try
            {
                if (@this._taskFunc != null)
                    await @this._taskFunc(state, @this._token).ConfigureAwait(false);
                else if (@this._valueTaskFunc != null)
                    await @this._valueTaskFunc(state, @this._token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                @this.LogMessage?.LogWarning(ex);
            }

        }
    }

    public void SetNext(int interval)
    {
        // 延迟触发下一次
        if (!Check())
            _timer?.SetNext(interval);
    }

    public bool Change(int dueTime, int period)
    {
        // 延迟触发下一次
        if (!Check())
            return _timer?.Change(dueTime, period) == true;

        return false;
    }
    public void Stop()
    {
        _timer?.SafeDispose();
        _timer = null;
    }

    protected override void Dispose(bool disposing)
    {
        Stop();
        base.Dispose(disposing);
    }
}
