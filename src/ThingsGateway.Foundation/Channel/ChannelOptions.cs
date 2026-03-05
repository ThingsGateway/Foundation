//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public class ChannelOptions : ChannelOptionsBase, IChannelOptions, IDisposable
{
    public AsyncConcurrencyLimiter WaitLock { get; private set; } = new AsyncConcurrencyLimiter(1);

    /// <inheritdoc/>
    public override int MaxConcurrentCount
    {
        get
        {
            return _maxConcurrentCount;
        }
        set
        {
            if (value > 0)
            {
                _maxConcurrentCount = value;
                if (WaitLock?.MaxCount != MaxConcurrentCount)
                {
                    var _lock = WaitLock;
                    WaitLock = new AsyncConcurrencyLimiter(_maxConcurrentCount);
                    _lock?.SafeDispose();
                }
            }
        }
    }

    private volatile int _maxConcurrentCount = 1;
    public TouchSocketConfig Config { get; set; } = new();

    public void Dispose()
    {
        Config?.SafeDispose();
        GC.SuppressFinalize(this);
    }
}
