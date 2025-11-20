using System.ComponentModel;

using ThingsGateway.Foundation.Common.Configuration;
using ThingsGateway.Foundation.Common.Log;

namespace ThingsGateway.Foundation.Common;

/// <summary>核心设置</summary>
[Config("Common", Provider = "json")]
public class Setting : Config<Setting>
{
    #region 属性
    /// <summary>是否启用全局调试。默认启用</summary>
    [Description("全局调试。XTrace.Debug")]
    public Boolean Debug { get; set; } = true;

    /// <summary>日志等级，只输出大于等于该级别的日志，All/Debug/Info/Warn/Error/Fatal，默认Info</summary>
    [Description("日志等级。只输出大于等于该级别的日志，All/Debug/Info/Warn/Error/Fatal，默认Info")]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    /// <summary>文件日志目录。默认Log子目录</summary>
    [Description("文件日志目录。默认Log子目录")]
    public String LogPath { get; set; } = "Logs/XTrace";

    /// <summary>日志文件上限。超过上限后拆分新日志文件，默认5MB，0表示不限制大小</summary>
    [Description("日志文件上限。超过上限后拆分新日志文件，默认5MB，0表示不限制大小")]
    public Int32 LogFileMaxMegabytes { get; set; } = 5;

    /// <summary>日志文件备份。超过备份数后，最旧的文件将被删除，0表示不限制个数</summary>
    [Description("日志文件备份。超过备份数后，最旧的文件将被删除，网络安全法要求至少保存6个月日志，默认50，0表示不限制个数")]
    public Int32 LogFileBackups { get; set; } = 10;

    /// <summary>日志文件格式。默认{0:yyyy_MM_dd}.log，支持日志等级如 {1}_{0:yyyy_MM_dd}.log</summary>
    [Description("日志文件格式。默认{0:yyyy_MM_dd}.log，支持日志等级如 {1}_{0:yyyy_MM_dd}.log")]
    public String LogFileFormat { get; set; } = "{0:yyyy_MM_dd}.log";


    #endregion


}