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
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public static class PluginUtil
{
    /// <summary>
    /// 作为DTU终端
    /// </summary>
    public static Action<IPluginManager> GetDtuClientPlugin(IChannelOptions channelOptions)
    {
        if (!channelOptions.DtuId.IsNullOrWhiteSpace())
        {
            Action<IPluginManager> action = a => { };

            action += a =>
            {
                var plugin = a.Add<HeartbeatAndReceivePlugin>();
                plugin.HeartbeatHex = channelOptions.HeartbeatHex;
                plugin.DtuIdHex = channelOptions.DtuIdHex;
                plugin.Heartbeat = channelOptions.Heartbeat;
                plugin.DtuId = channelOptions.DtuId;
                plugin.HeartbeatTime = channelOptions.HeartbeatTime;
            };

            if (channelOptions.ChannelType == ChannelTypeEnum.TcpClient)
            {
                action += a => a.UseReconnection<IClientChannel>(a =>
                {
                    a.PollingInterval = TimeSpan.FromSeconds(5);
                    a.ConnectAction = async (client, cancellationToken) =>
                    {
                        var attempts = 0;
                        var currentInterval = a.BaseInterval;

                        while (a.MaxRetryCount < 0 || attempts < a.MaxRetryCount)
                        {
                            if(cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            if (client.PauseReconnection)
                            {
                                continue;
                            }

                            attempts++;

                            try
                            {
                                if (client.Online)
                                {
                                    a.OnSuccessed?.Invoke(client);
                                    return;
                                }

                                await client.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                                a.OnSuccessed?.Invoke(client);

                                if (a.LogReconnection)
                                {
                                    client.Logger?.Info(a, $"重连成功，尝试次数: {attempts}");
                                }
                                return;
                            }
                            catch (Exception ex)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }
                                a.OnFailed?.Invoke(client, attempts, ex);

                                if (a.LogReconnection)
                                {
                                    client.Logger?.Warning(a, $"重连失败，尝试次数: {attempts}，错误: {ex.Message}");
                                }

                                if (a.MaxRetryCount > 0 && attempts >= a.MaxRetryCount)
                                {
                                    a.OnGiveUp?.Invoke(client, attempts);
                                    if (a.LogReconnection)
                                    {
                                        client.Logger?.Error(a, $"达到最大重连次数 {a.MaxRetryCount}，放弃重连");
                                    }
                                    return;
                                }

                                // 计算下次重连间隔
                                currentInterval = CalculateNextInterval(a,attempts, currentInterval);

                                await Task.Delay(currentInterval, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            }
                        }
                    };
                }
                );
            }
            return action;
        }
        return a => { };
    }
    /// <summary>
    /// 计算下次重连间隔
    /// </summary>
    /// <param name="reconnectionOption">option</param>
    /// <param name="attemptCount">当前尝试次数</param>
    /// <param name="currentInterval">当前间隔</param>
    /// <returns>下次重连间隔</returns>
    private static TimeSpan CalculateNextInterval(ReconnectionOption<IClientChannel> reconnectionOption, int attemptCount, TimeSpan currentInterval)
    {
        return reconnectionOption.Strategy switch
        {
            ReconnectionStrategy.Simple => reconnectionOption.BaseInterval,
            ReconnectionStrategy.ExponentialBackoff => TimeSpan.FromMilliseconds(Math.Min(
                reconnectionOption.BaseInterval.TotalMilliseconds * Math.Pow(reconnectionOption.BackoffMultiplier, attemptCount - 1),
                reconnectionOption.MaxInterval.TotalMilliseconds)),
            ReconnectionStrategy.LinearBackoff => TimeSpan.FromMilliseconds(Math.Min(
                reconnectionOption.BaseInterval.TotalMilliseconds + (attemptCount - 1) * reconnectionOption.BackoffMultiplier,
                reconnectionOption.MaxInterval.TotalMilliseconds)),
            _ => reconnectionOption.BaseInterval
        };
    }
    /// <summary>
    /// 作为DTU服务
    /// </summary>
    public static Action<IPluginManager> GetDtuPlugin(IChannelOptions channelOptions)
    {
        Action<IPluginManager> action = a => { };

        action += GetTcpServicePlugin(channelOptions);
        //if (!channelOptions.Heartbeat.IsNullOrWhiteSpace())
        {
            action += a =>
            {
                var plugin = a.Add<DtuPlugin>();
                plugin.HeartbeatHex = channelOptions.HeartbeatHex;
                plugin.Heartbeat = channelOptions.Heartbeat;
                plugin.DtuIdHex = channelOptions.DtuIdHex;
            };
        }
        return action;
    }

    /// <summary>
    /// 作为TCP服务
    /// </summary>
    /// <param name="channelOptions"></param>
    /// <returns></returns>
    public static Action<IPluginManager> GetTcpServicePlugin(IChannelOptions channelOptions)
    {
        Action<IPluginManager> action = a => { };
        if (channelOptions.CheckClearTime > 0)
        {
            action += a =>
            {
                a.UseTcpSessionCheckClear(options =>
                {
                    options.CheckClearType = CheckClearType.All;
                    options.Tick = TimeSpan.FromMilliseconds(channelOptions.CheckClearTime);
                    options.OnClose = (c, t) =>
                    {
                        return c.CloseAsync($"{channelOptions.CheckClearTime}ms Timeout");
                    };
                });
            };
        }
        return action;
    }
}
