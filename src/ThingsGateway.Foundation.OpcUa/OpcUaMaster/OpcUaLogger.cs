//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

#pragma warning disable CS8633 // 类型参数的约束中的为 Null 性与隐式实现接口方法中的类型参数的约束不匹配。

#pragma warning disable CS8766 // 返回类型中引用类型的为 Null 性与隐式实现的成员不匹配(可能是由于为 Null 性特性)。

namespace ThingsGateway.Plugin.OpcUa;

public class OpcUaTelemetryContext : TelemetryContextBase, IDisposable
{
    public OpcUaTelemetryContext(Action<byte, object, string, Exception> log)
#pragma warning disable CA2000 // 丢失范围之前释放对象
        : base(Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.ClearProviders()
                .AddProvider(new OpcUaLoggerProvider(log));
        }))
#pragma warning restore CA2000 // 丢失范围之前释放对象
    {
    }

    public void Dispose()
    {
        base.LoggerFactory?.Dispose();
    }
}

public sealed class OpcUaLoggerProvider : ILoggerProvider
{
    private Action<byte, object, string, Exception> _log { get; set; }
    public OpcUaLoggerProvider(Action<byte, object, string, Exception> log)
    {
        _log = log;
    }
    public ILogger CreateLogger(string categoryName)
    {
        return new OpcUaLogger(_log);
    }

    public void Dispose()
    {
        _log = null;
    }
}
internal sealed class OpcUaLogger : ILogger, IDisposable
{
    private Action<byte, object, string, Exception> _log;

    public OpcUaLogger(Action<byte, object, string, Exception> log)
    {
        _log = log;
    }

    /// <summary>
    /// Set the log level
    /// </summary>
    public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Trace;

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }

    public void Dispose()
    {
        _log = null;
    }

    /// <inheritdoc/>
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => logLevel >= LogLevel;

    /// <inheritdoc/>
    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        else
        {
            var message = formatter(state, exception);
            //if (logLevel > Microsoft.Extensions.Logging.LogLevel.Warning)
            {
                _log?.Invoke((byte)logLevel, state, message, exception);
            }
        }
    }
}