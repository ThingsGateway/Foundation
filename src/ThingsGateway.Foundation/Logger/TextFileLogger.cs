//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Text;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

/// <summary>
/// 文本文件日志类。提供向文本文件写日志的能力
/// </summary>
public class TextFileLogger : ThingsGateway.Foundation.Common.Log.TextFileLog, TouchSocket.Core.ILog, IDisposable
{
    private static string separator = Environment.NewLine + "-----分隔符-----" + Environment.NewLine;

    /// <summary>
    /// 分隔符
    /// </summary>
    public static string Separator
    {
        get
        {
            return separator;
        }
        set
        {
            separator = value;
            separatorBytes = Encoding.UTF8.GetBytes(separator);
        }
    }

    private static byte[] separatorBytes = Encoding.UTF8.GetBytes(Environment.NewLine + "-----分隔符-----" + Environment.NewLine);

    internal static byte[] SeparatorBytes
    {
        get
        {
            return separatorBytes;
        }
    }

    private static readonly NonBlockingDictionary<string, TextFileLogger> cache = new NonBlockingDictionary<string, TextFileLogger>(comparer: StringComparer.OrdinalIgnoreCase);
    private string CacheKey;

    /// <summary>
    ///  文本日志记录器
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="isfile">单文件</param>
    /// <param name="fileFormat">文件名称格式</param>
    private TextFileLogger(string path, bool isfile, string? fileFormat = null) : base(path, isfile, fileFormat)
    {
        CacheKey = ($"{path}{fileFormat}");
    }

    /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
    /// <param name="path">日志目录或日志文件路径</param>
    /// <returns></returns>
    public static TextFileLogger CreateSingleFileLogger(String path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        return cache.GetOrAdd(path, k =>
        {
            var log = new TextFileLogger(path, true);
            log.CacheKey = k;
            return log;
        });
    }
    /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
    /// <param name="path">日志目录或日志文件路径</param>
    /// <param name="fileFormat"></param>
    /// <returns></returns>
    public static TextFileLogger GetMultipleFileLogger(String path, String? fileFormat = null)
    {
        if (string.IsNullOrEmpty(path)) path = "Log";

        var key = path + fileFormat;
        return cache.GetOrAdd(key, k =>
        {
            var log = new TextFileLogger(path, false, fileFormat);
            log.CacheKey = k;
            return log;
        });
    }
    /// <summary>
    /// TimeFormat
    /// </summary>
    public const string TimeFormat = "yyyy-MM-dd HH:mm:ss:ffff zz";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="source"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    protected void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
    {
        if (!Check()) return;

        using var stringBuilder = new ValueStringBuilder();

        stringBuilder.Append(DateTime.Now.ToString(TimeFormat));
        stringBuilder.Append(',');
        stringBuilder.Append(logLevel.ToString());
        stringBuilder.Append(',');
        stringBuilder.Append('\"');
        stringBuilder.Append(message);
        stringBuilder.Append('\"');

        if (exception != null)
        {
            stringBuilder.Append(',');
            stringBuilder.Append('\"');
            stringBuilder.Append(exception.GetMessage());
            stringBuilder.Append('\"');
        }

        //自定义的分割符，用于读取文件时的每一行判断，而不是单纯换行符
        stringBuilder.Append(Separator);

        // 推入队列
        Enqueue(stringBuilder.ToString());

    }

    /// <inheritdoc/>
    public LogLevel LogLevel { get; set; } = LogLevel.Trace;

    public string DateTimeFormat { get; set; } = TimeFormat;

    /// <inheritdoc/>
    public void Log(LogLevel logLevel, object source, string message, Exception exception)
    {
        if (logLevel < LogLevel)
        {
            return;
        }
        WriteLog(logLevel, source, message, exception);
    }

    protected override void Dispose(bool disposing)
    {
        _ = cache.TryRemove(CacheKey, out _);
        base.Dispose(disposing);
    }
}
