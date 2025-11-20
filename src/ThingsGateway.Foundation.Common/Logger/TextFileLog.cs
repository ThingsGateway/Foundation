using System.Collections.Concurrent;
using System.Text;

namespace ThingsGateway.Foundation.Common.Log;

/// <summary>文本文件日志类。提供向文本文件写日志的能力</summary>
public class TextFileLog : Logger, IDisposable
{
    #region 属性
    /// <summary>日志目录</summary>
    public String LogPath { get; set; } = "XTrace";

    /// <summary>日志文件格式。默认{0:yyyy_MM_dd}.log</summary>
    public String FileFormat { get; set; } = "{0:yyyy_MM_dd}.log";

    /// <summary>日志文件上限。超过上限后拆分新日志文件，默认5MB，0表示不限制大小</summary>
    public Int32 MaxMegabytes { get; set; } = 5;

    /// <summary>日志文件备份。超过备份数后，最旧的文件将被删除，默认10，0表示不限制个数</summary>
    public Int32 Backups { get; set; } = 10;

    private readonly Boolean _isFile;
    private string _ext = "log";
    #endregion

    #region 构造
    /// <summary>该构造函数没有作用，为了继承而设置</summary>
    private TextFileLog()
    {
    }

    internal protected TextFileLog(String path, Boolean isfile, String? fileFormat = null)
    {
        LogPath = path;
        _isFile = isfile;
        _ext = Path.GetExtension(FileFormat);

        var set = Setting.Current;
        if (!fileFormat.IsNullOrEmpty())
            FileFormat = fileFormat;
        else
            FileFormat = set.LogFileFormat;

        MaxMegabytes = set.LogFileMaxMegabytes;
        Backups = set.LogFileBackups;

        _Timer = new TimerX(DoWriteAndClose, null, 0_000, 60_000) { Async = true };
        _WriteTimer = new TimerX(DoWrite, null, 0_000, 1000, nameof(TextFileLog)) { Async = true };
    }

    private static readonly NonBlockingDictionary<String, TextFileLog> cache = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
    /// <param name="path">日志目录或日志文件路径</param>
    /// <param name="fileFormat"></param>
    /// <returns></returns>
    public static TextFileLog Create(String path, String? fileFormat = null)
    {
        if (string.IsNullOrEmpty(path)) path = "Log";

        var key = path + fileFormat;
        return cache.GetOrAdd(key, k => new TextFileLog(path, false, fileFormat));
    }

    /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
    /// <param name="path">日志目录或日志文件路径</param>
    /// <returns></returns>
    public static TextFileLog CreateFile(String path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        return cache.GetOrAdd(path, k => new TextFileLog(k, true));
    }


    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        _Timer.TryDispose();
        _WriteTimer.TryDispose();

        // 销毁前把队列日志输出
        WriteAndClose(DateTime.MinValue);

        while (_Logs.TryDequeue(out _)) ;
    }
    #endregion

    #region 内部方法
    private StreamWriter? LogWriter;
    private String? CurrentLogFile;
    private Int32 _logFileError;

    /// <summary>初始化日志记录文件</summary>
    private StreamWriter? InitLog(String logfile)
    {
        try
        {
            logfile.EnsureDirectory(true);

            var stream = new FileStream(logfile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            var writer = new StreamWriter(stream, Encoding.UTF8);

            _logFileError = 0;
            return LogWriter = writer;
        }
        catch (Exception ex)
        {
            _logFileError++;
            Console.WriteLine("创建日志文件失败：{0}", ex.GetMessage());
            return null;
        }
    }

    /// <summary>获取日志文件路径</summary>
    /// <returns></returns>
    private string? GetLogFile()
    {
        // 单日志文件
        if (_isFile) return LogPath;
        Directory.CreateDirectory(LogPath);
        // 目录多日志文件
        var baseFile = Path.Combine(LogPath, string.Format(FileFormat, TimerX.Now, Level));

        // 不限制大小
        if (MaxMegabytes == 0) return baseFile;

        var maxBytes = MaxMegabytes * 1024L * 1024L;

        string? latestFile = null;
        long latestLen = 0;
        DateTime latestTime = DateTime.MinValue;

        foreach (var path in Directory.EnumerateFiles(LogPath, $"*{_ext}", SearchOption.TopDirectoryOnly))
        {
            try
            {
                // 使用 File.GetLastWriteTimeUtc
                var writeTime = File.GetLastAccessTimeUtc(path);
                if (writeTime > latestTime)
                {
                    latestTime = writeTime;
                    latestFile = path;
                }
            }
            catch
            {
                // 忽略异常（如权限或文件被删除）
            }
        }

        if (latestFile != null)
        {
            try
            {
                latestLen = new FileInfo(latestFile).Length;
                if (latestLen < maxBytes)
                    return latestFile;
            }
            catch { }
        }

        var dir = Path.GetDirectoryName(baseFile)!;
        var nameWithoutExt = Path.GetFileNameWithoutExtension(baseFile);


        // 依序找下一个可用文件
        // 优化循环逻辑，提前计算好固定部分

        for (var i = 1; i < 1024; i++)
        {
            using var sb = new ValueStringBuilder(256);
            sb.Append(dir);
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(nameWithoutExt);
            if (i > 1)
            {
                sb.Append('_');
                sb.Append(i.ToString());
            }
            sb.Append(_ext);

            var nextFile = sb.ToString();
            if (!File.Exists(nextFile))
                return nextFile;
        }

        return null;
    }

    #endregion

    #region 异步写日志
    private readonly TimerX? _Timer;
    private readonly TimerX? _WriteTimer;
    private readonly ConcurrentQueue<String> _Logs = new();
    private volatile Int32 _logCount;
    private Int32 _writing;
    private DateTime _NextClose;

    /// <summary>写文件</summary>
    private void WriteFile()
    {
        try
        {
            if (_Logs.IsEmpty) return;

            if (Interlocked.CompareExchange(ref _writing, 1, 0) != 0) return;

            var writer = LogWriter;

            var logFile = GetLogFile()!;
            if (string.IsNullOrEmpty(logFile)) return;
            var now = TimerX.Now;

            if (!_isFile && logFile != CurrentLogFile)
            {
                writer.TryDispose();
                writer = null;

                CurrentLogFile = logFile;
                _logFileError = 0;
            }

            // 错误过多时不再尝试创建日志文件。下一天更换日志文件名后，将会再次尝试
            if (writer == null && _logFileError >= 3) return;

            // 初始化日志读写器
            writer ??= InitLog(logFile);
            if (writer == null) return;

            // 依次把队列日志写入文件
            while (_Logs.TryDequeue(out var str))
            {
                Interlocked.Decrement(ref _logCount);

                // 写日志。TextWriter.WriteLine内需要拷贝，浪费资源
                //writer.WriteLine(str);
                writer.Write(str);
                writer.WriteLine();
            }

            // 写完一批后，刷一次磁盘
            writer.Flush();

            // 连续5秒没日志，就关闭
            _NextClose = now.AddSeconds(5);

        }
        finally
        {
            _writing = 0;
        }
    }
    private void DoWrite(Object? state) => WriteFile();

    /// <summary>关闭文件</summary>
    private void DoWriteAndClose(Object? state)
    {
        // 同步写日志
        WriteAndClose(_NextClose);

        // 检查文件是否超过上限
        if (!_isFile && Backups > 0)
        {
            // 判断日志目录是否已存在
            DirectoryInfo? di = new DirectoryInfo(LogPath);
            if (di.Exists)
            {
                // 删除 *.del
                try
                {
                    foreach (var item in di.EnumerateFiles("*.del"))
                    {
                        item.Delete();
                    }
                }
                catch { }

                var ext = Path.GetExtension(FileFormat);
                var fis = di.EnumerateFiles($"*{ext}")
                           .OrderByDescending(e => e.LastWriteTimeUtc)
                           .Skip(Backups);

                foreach (var item in fis)
                {
                    OnWrite(LogLevel.Info, "The log file has reached the maximum limit of {0}, delete {1}, size {2: n0} Byte", Backups, item.Name, item.Length);
                    try
                    {
                        item.Delete();
                    }
                    catch
                    {
                        try
                        {
                            item.MoveTo(item.FullName + ".del");
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }

    /// <summary>写入队列日志并关闭文件</summary>
    protected virtual void WriteAndClose(DateTime closeTime)
    {
        try
        {
            if (Interlocked.CompareExchange(ref _writing, 1, 0) != 0) return;

            // 处理残余
            var writer = LogWriter;
            WriteFile();

            // 连续5秒没日志，就关闭
            if (writer != null && closeTime < TimerX.Now)
            {
                writer.TryDispose();
                LogWriter = null;
            }
        }
        finally
        {
            _writing = 0;
        }
    }
    #endregion

    #region 写日志
    /// <summary>写日志</summary>
    /// <param name="level"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    protected override void OnWrite(LogLevel level, String format, params Object?[] args)
    {
        if (!Check()) return;

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

    protected bool Check()
    {
        if (_Timer.Disposed) return false;

        if (_logCount > 100) return false;
        return true;
    }
#if NET9_0_OR_GREATER
    private readonly Lock @lock = new();
#else
    private readonly object @lock = new();
#endif
    protected void Enqueue(string data)
    {
        _Logs.Enqueue(data);
        Interlocked.Increment(ref _logCount);
    }

    #endregion

    #region 辅助
    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString() => $"{GetType().Name} {LogPath}";
    #endregion
}