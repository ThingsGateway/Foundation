using ThingsGateway.Foundation.Common.PooledAwait;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

public class CronScheduledTask : DisposeBase, IScheduledTask
{

    private string _interval;
    private readonly Func<object?, CancellationToken, Task>? _taskFunc;
    private readonly Func<object?, CancellationToken, ValueTask>? _valueTaskFunc;
    private readonly Action<object?, CancellationToken>? _taskAction;
    private readonly CancellationToken _token;
    private TimerX? _timer;
    private object? _state;
    private ILog LogMessage;
    public Int32 Period => _timer?.Period ?? 0;
    public bool Enable => _timer?.Disposed != false ? false : true;

    public CronScheduledTask(string interval, Func<object?, CancellationToken, Task> taskFunc, object? state, ILog log, CancellationToken token)
    {
        _interval = interval;
        LogMessage = log;
        _state = state;
        _taskFunc = taskFunc;
        _token = token;
    }
    public CronScheduledTask(string interval, Func<object?, CancellationToken, ValueTask> taskFunc, object? state, ILog log, CancellationToken token)
    {
        _interval = interval;
        LogMessage = log;
        _state = state;
        _valueTaskFunc = taskFunc;
        _token = token;
    }



    public CronScheduledTask(string interval, Action<object?, CancellationToken> taskAction, object? state, ILog log, CancellationToken token)
    {
        _interval = interval;
        LogMessage = log;
        _state = state;
        _taskAction = taskAction;
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
    public void Start()
    {
        _timer?.Dispose();
        if (Check()) return;
        if (_taskAction != null)
            _timer = new TimerX(TimerCallback, _state, _interval, $"{nameof(IScheduledTask)}") { Async = true, Reentrant = false };
        else if (_taskFunc != null || _valueTaskFunc != null)
            _timer = new TimerX(TimerCallbackAsync, _state, _interval, $"{nameof(IScheduledTask)}") { Async = true, Reentrant = false };
    }

    private ValueTask TimerCallbackAsync(object? state)
    {
        return TimerCallbackAsync(this, state);
        static async PooledValueTask TimerCallbackAsync(CronScheduledTask @this, object? state)
        {
            if (@this.Check()) return;
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

    private void TimerCallback(object? state)
    {
        if (Check()) return;
        if (_taskAction == null)
        {
            Dispose();
            return;
        }

        try
        {
            _taskAction(state, _token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
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
