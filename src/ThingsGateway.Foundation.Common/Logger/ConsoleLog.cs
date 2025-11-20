using System.Text;

namespace ThingsGateway.Foundation.Common.Log;

/// <summary>控制台输出日志</summary>
public class ConsoleLog : Logger
{
    private readonly Queue<String> _Logs = new();
    private volatile Int32 _logCount;
    private Int32 _writing;
    public static ConsoleLog Default { get; } = new();

    private readonly TimerX? _WriteTimer;

    private ConsoleLog()
    {
        _WriteTimer = new TimerX(DoWrite, null, 0_000, 500) { Async = true };
    }
    protected override void Dispose(bool disposing)
    {
        _WriteTimer?.Dispose();
        base.Dispose(disposing);
    }
    protected virtual void DoWrite(object? state)
    {
        try
        {
            if (_Logs.IsEmpty) return;

            if (Interlocked.CompareExchange(ref _writing, 1, 0) != 0) return;

            // 依次把队列日志写入文件
            while (_Logs.TryDequeue(out var e))
            {
                Interlocked.Decrement(ref _logCount);

                Console.WriteLine(e);
            }
        }
        finally
        {
            _writing = 0;
        }
    }


#if NET9_0_OR_GREATER
    private readonly Lock @lock = new();
#else
    private readonly object @lock = new();
#endif
    protected void Enqueue(string data)
    {
        lock (@lock)
        {
            _Logs.Enqueue(data);
        }
        Interlocked.Increment(ref _logCount);
    }
    /// <summary>写日志</summary>
    /// <param name="level"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    protected override void OnWrite(LogLevel level, String format, params Object?[] args)
    {
        // 日志队列积压将会导致内存暴增
        if (_logCount > 64) return;

        string body = Format(format, args);

        using var sb = new ValueStringBuilder();

        sb.Append(DateTime.Now.ToString("HH:mm:ss.fff zz"));
        sb.Append(',');
        sb.Append(level.ToString());
        sb.Append(',');
        sb.Append(body);
        // 推入队列
        Enqueue(sb.ToString());

    }

}