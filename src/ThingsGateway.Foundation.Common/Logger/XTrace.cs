using System.Runtime.InteropServices;

namespace ThingsGateway.Foundation.Common.Log;

/// <summary>日志类</summary>
public static class XTrace
{
    #region 写日志

    /// <summary>文本文件日志</summary>
    private static ILog _Log = Logger.Null;

    /// <summary>日志提供者，默认使用文本文件日志</summary>
    public static ILog Log { get { InitLog(); return _Log; } set { _Log = value; } }
    public static Func<bool> UnhandledExceptionLogEnable { get; set; } = () => true;
    /// <summary>输出日志</summary>
    /// <param name="msg">信息</param>
    public static void WriteLine(String msg)
    {
        // 只过滤null，保留包含空格的字符串
        if (msg == null) return;

        if (!InitLog()) return;

        Log.Info(msg);
    }

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public static void WriteLine(String format, params Object?[] args)
    {
        // 只过滤null，保留包含空格的字符串
        if (format == null) return;

        if (!InitLog()) return;

        Log.Info(format, args);
    }

    /// <summary>输出异常日志</summary>
    /// <param name="ex">异常信息</param>
    public static void WriteException(Exception ex)
    {
        if (!InitLog()) return;

        Log.Error("{0}", ex.GetStackTrace());
    }
    public static void WriteException(Exception ex, string message)
    {
        if (!InitLog()) return;

        Log.Error("{0}, {1}", message, ex.GetStackTrace());
    }
    #endregion 写日志

    #region 构造

    static XTrace()
    {
        _ = Runtime.AppTickCount64;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
#if NETCOREAPP
        System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += ctx => OnProcessExit(null, EventArgs.Empty);
#endif

        TimerScheduler.Init();
        try
        {
            var set = Setting.Current;
            Debug = set.Debug;
            LogPath = set.LogPath;
            LogLevel = set.LogLevel;
        }
        catch { }
    }

    private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
    {
        if (!UnhandledExceptionLogEnable()) return;
        if (e.ExceptionObject is Exception ex)
        {
            WriteException(ex);
        }
        if (e.IsTerminating)
        {
            Log.Fatal("异常退出！");

            OnProcessExit(null, EventArgs.Empty);
        }
    }

    private static void TaskScheduler_UnobservedTaskException(Object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (!UnhandledExceptionLogEnable()) return;
        if (!e.Observed && e.Exception != null)
        {
            //WriteException(e.Exception);
            foreach (var ex in e.Exception.Flatten().InnerExceptions)
            {
                WriteException(ex);
            }
            e.SetObserved();
        }
    }

    private static void OnProcessExit(Object? sender, EventArgs e)
    {
        if (Log is CompositeLog compositeLog)
        {
            var log = compositeLog.Get<TextFileLog>();
            log.TryDispose();
        }
        else
        {
            Log.TryDispose();
        }
    }

    private static readonly Object _lock = new();
    private static Int32 _initing;

    /// <summary>
    /// 2012.11.05 修正初次调用的时候，由于同步BUG，导致Log为空的问题。
    /// </summary>
    private static Boolean InitLog()
    {
        /*
         * 日志初始化可能会触发配置模块，其内部又写日志导致死循环。
         * 1，外部写日志引发初始化
         * 2，标识日志初始化正在进行中
         * 3，初始化日志提供者
         * 4，此时如果再次引发写入日志，发现正在进行中，放弃写入的日志
         * 5，标识日志初始化已完成
         * 6，正常写入日志
         */

        if (_Log != null && _Log != Logger.Null) return true;
        if (_initing > 0 && _initing == Environment.CurrentManagedThreadId) return false;

        lock (_lock)
        {
            if (_Log != null && _Log != Logger.Null) return true;

            _initing = Environment.CurrentManagedThreadId;

            _Log = TextFileLog.Create(LogPath);

            _initing = 0;
        }

        //WriteVersion();

        return true;
    }

    #endregion 构造

    #region 使用控制台输出

    private static Boolean _useConsole;
    /// <summary>使用控制台输出日志。如果是后台服务则只用文本日志不用控制台日志</summary>
    public static void UseConsole()
    {
        if (Environment.UserInteractive)
        {
            UseConsole(true);
        }
        else
        {
            InitLog();
            //UseConsole(true, true);
        }
    }
    /// <summary>使用控制台输出日志，只能调用一次</summary>
    /// <param name="useFileLog">是否同时使用文件日志，默认使用</param>
    public static void UseConsole(Boolean useFileLog = true)
    {
        if (_useConsole) return;
        _useConsole = true;

        //if (!Runtime.IsConsole) return;
        Runtime.IsConsole = true;

        // 适当加大控制台窗口
        try
        {
#if !NETFRAMEWORK
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Console.WindowWidth <= 80) Console.WindowWidth = Console.WindowWidth * 3 / 2;
                if (Console.WindowHeight <= 25) Console.WindowHeight = Console.WindowHeight * 3 / 2;
            }
#else
            if (Console.WindowWidth <= 80) Console.WindowWidth = Console.WindowWidth * 3 / 2;
            if (Console.WindowHeight <= 25) Console.WindowHeight = Console.WindowHeight * 3 / 2;
#endif
        }
        catch { }

        if (useFileLog)
            _Log = new CompositeLog(ConsoleLog.Default, Log);
        else
            _Log = ConsoleLog.Default;
    }

    #endregion 使用控制台输出

    /// <summary>文本日志目录</summary>
    public static String LogPath { get; set; } = "XTrace";
    public static Boolean Debug { get; set; }
    public static LogLevel LogLevel
    {
        get => Log.Level;
        set => Log.Level = value;
    }
}