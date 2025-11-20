using System.ComponentModel;

namespace ThingsGateway.Foundation.Common.Log;

/// <summary>日志基类。提供日志的基本实现</summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public abstract class Logger : DisposeBase, ILog
{
    #region 主方法
    /// <summary>调试日志</summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">格式化参数</param>
    public virtual void Debug(String format, params Object?[] args) => Write(LogLevel.Debug, format, args);

    /// <summary>信息日志</summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">格式化参数</param>
    public virtual void Info(String format, params Object?[] args) => Write(LogLevel.Info, format, args);

    /// <summary>警告日志</summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">格式化参数</param>
    public virtual void Warn(String format, params Object?[] args) => Write(LogLevel.Warn, format, args);

    /// <summary>错误日志</summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">格式化参数</param>
    public virtual void Error(String format, params Object?[] args) => Write(LogLevel.Error, format, args);

    /// <summary>严重错误日志</summary>
    /// <param name="format">格式化字符串</param>
    /// <param name="args">格式化参数</param>
    public virtual void Fatal(String format, params Object?[] args) => Write(LogLevel.Fatal, format, args);
    #endregion

    #region 核心方法
    /// <summary>写日志</summary>
    /// <param name="level"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public virtual void Write(LogLevel level, String format, params Object?[] args)
    {
        if (Enable && level >= Level) OnWrite(level, format, args);
    }

    /// <summary>写日志</summary>
    /// <param name="level"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    protected abstract void OnWrite(LogLevel level, String format, params Object?[] args);
    #endregion

    #region 辅助方法
    /// <summary>格式化参数，特殊处理异常和时间</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal protected virtual String Format(String format, Object?[]? args)
    {
        //处理时间的格式化
        if (args?.Length > 0)
        {
            // 特殊处理异常
            if (args.Length == 1 && args[0] is Exception ex && (string.IsNullOrEmpty(format) || format == "{0}"))
                return ex.GetMessage();

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] != null && args[i] is DateTime dt && format.Contains("{" + i + "}"))
                {
                    if (dt.Millisecond > 0)
                        args[i] = dt.ToString("yyyy-MM-dd HH:mm:ss.fff zz");
                    else if (dt.Hour > 0 || dt.Minute > 0 || dt.Second > 0)
                        args[i] = dt.ToString("yyyy-MM-dd HH:mm:ss zz");
                    else
                        args[i] = dt.ToString("yyyy-MM-dd zz");
                }
                if (args[i] != null && args[i] is DateTimeOffset dto && format.Contains("{" + i + "}"))
                {
                    if (dto.Millisecond > 0)
                        args[i] = dto.ToString("yyyy-MM-dd HH:mm:ss.fff zz");
                    else if (dto.Hour > 0 || dto.Minute > 0 || dto.Second > 0)
                        args[i] = dto.ToString("yyyy-MM-dd HH:mm:ss zz");
                    else
                        args[i] = dto.ToString("yyyy-MM-dd zz");
                }
            }
        }
        if (args == null || args.Length <= 0) return format;

        return String.Format(format, args);
    }
    #endregion

    #region 属性
    /// <summary>是否启用日志。默认true</summary>
    public virtual Boolean Enable { get; set; } = true;

    private LogLevel? _Level;
    /// <summary>日志等级，只输出大于等于该级别的日志，默认Info</summary>
    public virtual LogLevel Level
    {
        get
        {
            if (_Level != null) return _Level.Value;
            return Setting.Current.LogLevel;
        }
        set { _Level = value; }
    }
    #endregion

    #region 静态空实现
    /// <summary>空日志实现</summary>
    public static Logger Null { get; } = new NullLogger();

    sealed class NullLogger : Logger
    {
        public override Boolean Enable { get => false; set { } }

        protected override void OnWrite(LogLevel level, String format, params Object?[] args) { }
    }
    #endregion

#if NET9_0_OR_GREATER
    protected Lock lockThis = new();
#else
    protected object lockThis = new();
#endif

}