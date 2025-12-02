//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Net;

using ThingsGateway.Foundation.Common.PooledAwait;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation;

/// <summary>
/// 协议基类，不存在主从关系，所有请求均为被动接收响应
/// </summary>
public abstract class ReceivedDeviceBase : AsyncAndSyncDisposableObject, IReceivedDevice
{
    /// <inheritdoc/>
    public IChannel? Channel { get; private set; }

    public virtual bool SupportMultipleDevice()
    {
        return false;
    }

    /// <inheritdoc/>
    public virtual void InitChannel([NotNullIfNotNull(nameof(Channel))] IChannel channel, ILog? deviceLog = default)
    {
        ArgumentNullExceptionEx.ThrowIfNull(channel, nameof(channel));
        if (channel.Collects.Contains(this))
            return;
        Channel = channel;
        _deviceLogger = deviceLog;
        lock (channel)
        {
            if (channel.Collects.Contains(this))
                return;
            if (channel.Collects.Count > 0)
            {
                if (!SupportMultipleDevice())
                    throw new InvalidOperationException("The proactive response device does not support multiple devices");
            }

            if (channel.Collects.Count == 0)
            {
                channel.Config.ConfigurePlugins(ConfigurePlugins(channel.Config));

                if (Channel is IClientChannel clientChannel)
                {
                    if (clientChannel.ChannelType == ChannelTypeEnum.UdpSession)
                    {
                        channel.Config.SetUdpDataHandlingAdapter(() =>
                        {
                            var adapter = GetDataAdapter() as UdpDataHandlingAdapter;
                            return adapter;
                        });
                    }
                    else
                    {
                        channel.Config.SetSerialDataHandlingAdapter(() =>
                        {
                            var adapter = GetDataAdapter() as SingleStreamDataHandlingAdapter;
                            return adapter;
                        });
                        channel.Config.SetTcpDataHandlingAdapter(() =>
                        {
                            var adapter = GetDataAdapter() as SingleStreamDataHandlingAdapter;
                            return adapter;
                        });
                    }
                }
                else if (Channel is ITcpServiceChannel serviceChannel)
                {
                    channel.Config.SetTcpDataHandlingAdapter(() =>
                    {
                        var adapter = GetDataAdapter() as SingleStreamDataHandlingAdapter;
                        return adapter;
                    });
                }
            }

            channel.Collects.Add(this);
            Channel.Starting.Add(ChannelStarting);
            Channel.Stoped.Add(ChannelStoped);
            Channel.Stoping.Add(ChannelStoping);
            Channel.Started.Add(ChannelStarted);
            Channel.ChannelReceived.Add(ChannelReceived);

            SetChannel();
        }
    }

    protected virtual void SetChannel()
    {
        Channel?.ChannelOptions?.MaxConcurrentCount = 1;
    }

    /// <inheritdoc/>
    ~ReceivedDeviceBase()
    {
        this.SafeDispose();
    }

    #region

    private ILog? _deviceLogger;

    /// <inheritdoc/>
    public virtual ILog? Logger
    {
        get
        {
            return _deviceLogger ?? Channel?.Logger;
        }
    }

    /// <inheritdoc/>
    public virtual int RegisterByteLength { get; protected set; } = 1;

    /// <inheritdoc/>
    public virtual IThingsGatewayBitConverter BitConverter { get; } = ThingsGatewayBitConverter.BigEndian;

    /// <inheritdoc/>
    public bool OnLine => Channel?.Online ?? false;

    #endregion

    #region 属性

    /// <inheritdoc/>
    public virtual int SendDelayTime { get; set; }

    /// <inheritdoc/>
    public virtual int Timeout { get; set; } = 3000;

    /// <summary>
    /// <inheritdoc cref="IThingsGatewayBitConverter.IsStringReverseByteWord"/>
    /// </summary>
    public bool IsStringReverseByteWord
    {
        get
        {
            return BitConverter.IsStringReverseByteWord;
        }
        set
        {
            BitConverter.IsStringReverseByteWord = value;
        }
    }

    /// <inheritdoc/>
    public virtual DataFormatEnum DataFormat
    {
        get => BitConverter.DataFormat;
        set => BitConverter.DataFormat = value;
    }

    #endregion 属性

    #region 适配器

    /// <inheritdoc/>
    public abstract DataHandlingAdapter GetDataAdapter();
    /// <inheritdoc/>
    public virtual IChannel CreateChannel(TouchSocketConfig config, IChannelOptions channelOptions)
    {
        return config.GetChannel(channelOptions);
    }
    /// <summary>
    /// 通道连接成功时，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    protected virtual ValueTask<bool> ChannelStarted(IClientChannel channel, bool last)
    {
        return EasyValueTask.FromResult(true);
    }

    /// <summary>
    /// 通道断开连接前，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    protected virtual ValueTask<bool> ChannelStoping(IClientChannel channel, bool last)
    {
        return EasyValueTask.FromResult(true);
    }

    /// <summary>
    /// 通道断开连接后，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    protected virtual ValueTask<bool> ChannelStoped(IClientChannel channel, bool last)
    {
        try
        {
            channel.WaitHandlePool.CancelAll();
        }
        catch
        {
        }

        return EasyValueTask.FromResult(true);
    }

    /// <summary>
    /// 通道即将连接成功时，会设置适配器，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    protected virtual ValueTask<bool> ChannelStarting(IClientChannel channel, bool last)
    {
        if (Logger != null)
            channel.SetDataHandlingAdapterLogger(Logger);
        return EasyValueTask.FromResult(true);
    }

    /// <summary>
    /// 设置适配器
    /// </summary>
    protected virtual void SetDataAdapter(IClientChannel clientChannel)
    {
        var adapter = clientChannel.ReadOnlyDataHandlingAdapter;
        if (adapter == null)
        {
            var dataHandlingAdapter = GetDataAdapter();
            clientChannel.SetDataHandlingAdapter(dataHandlingAdapter);
        }
        else
        {
            if (Channel?.Collects?.Count > 1)
            {
                var dataHandlingAdapter = GetDataAdapter();
                if (adapter.GetType() != dataHandlingAdapter.GetType())
                {
                    clientChannel.SetDataHandlingAdapter(dataHandlingAdapter);
                }
            }
        }
    }

    #endregion 适配器

    #region 设备异步返回

    /// <summary>
    /// 日志输出16进制
    /// </summary>
    public virtual bool IsHexLog { get; init; } = true;

    /// <summary>
    /// 接收,非主动发送的情况，重写实现非主从并发通讯协议，如果通道存在其他设备并且不希望其他设备处理时，设置<see cref="TouchSocketEventArgs.Handled"/> 为true
    /// </summary>
    protected virtual ValueTask ChannelReceived(IClientChannel client, ReceivedDataEventArgs e, bool last)
    {
        if (e.RequestInfo is DeviceMessage response)
        {
            try
            {
                if (client.WaitHandlePool.Set(response))
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, $"Response {response.Sign}");
            }
        }

        return EasyValueTask.CompletedTask;
    }
    public bool AutoConnect { get; protected set; } = true;
    /// <inheritdoc/>
    private Task SendAsync(ISendMessage sendMessage, IClientChannel channel, CancellationToken token = default)
    {
        if (!channel.Online)
        {
            throw new InvalidOperationException("Channel is offline");
        }
        return SendAsync(this, sendMessage, channel, token);

        static async PooledTask SendAsync(ReceivedDeviceBase @this, ISendMessage sendMessage, IClientChannel channel, CancellationToken token)
        {
            if (@this.SendDelayTime != 0)
                await Task.Delay(@this.SendDelayTime, token).ConfigureAwait(false);

            if (channel is IDtuUdpSessionChannel udpSession)
            {
                var endPoint = @this.GetUdpEndpoint();
                await udpSession.SendAsync(endPoint, sendMessage, token).ConfigureAwait(false);

            }
            else
            {
                await channel.SendAsync(sendMessage, token).ConfigureAwait(false);
            }
        }

    }

    private ValueTask BeforeSendAsync(IClientChannel channel, CancellationToken token)
    {
        SetDataAdapter(channel);
        if (AutoConnect && Channel != null && Channel.Online != true)
        {
            return ConnectAsync(token);
        }
        else
        {
            return EasyValueTask.CompletedTask;
        }
    }

    private WaitLock connectWaitLock = new(nameof(ReceivedDeviceBase));

    public ValueTask ConnectAsync(CancellationToken token = default)
    {
        return ConnectAsync(this, token);

        static async PooledValueTask ConnectAsync(ReceivedDeviceBase @this, CancellationToken token)
        {
            if (@this.AutoConnect && @this.Channel != null && @this.Channel?.Online != true)
            {
                try
                {
                    await @this.connectWaitLock.WaitAsync(token).ConfigureAwait(false);
                    if (@this.AutoConnect && @this.Channel != null && @this.Channel?.Online != true)
                    {
                        if (@this.Channel!.PluginManager == null)
                            await @this.Channel.SetupAsync(@this.Channel.Config.Clone()).ConfigureAwait(false);
                        await @this.Channel.CloseAsync().ConfigureAwait(false);

#pragma warning disable CA2000 // 丢失范围之前释放对象
                        var reusableTimeout = @this._reusableTimeouts.Get() ?? new ReusableCancellationTokenSource();
#pragma warning restore CA2000 // 丢失范围之前释放对象
                        try
                        {

                            var ctsToken = reusableTimeout.GetTokenSource(@this.Channel.ChannelOptions?.ConnectTimeout ?? 3000, token);

                            await @this.Channel.ConnectAsync(ctsToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            @this._reusableTimeouts.Return(reusableTimeout);
                        }
                    }
                }
                finally
                {
                    @this.connectWaitLock.Release();
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> SendAsync(ISendMessage sendMessage, CancellationToken cancellationToken)
    {
        return SendAsync(this, sendMessage, cancellationToken);

        static async PooledValueTask<OperResult> SendAsync(ReceivedDeviceBase @this, ISendMessage sendMessage, CancellationToken cancellationToken)
        {
            try
            {
                var channelResult = @this.GetChannel();
                if (!channelResult.IsSuccess || channelResult.Content == null) return new OperResult<byte[]>(channelResult);
                WaitLock? waitLock = @this.GetWaitLock(channelResult.Content);

                try
                {
                    await @this.BeforeSendAsync(channelResult.Content, cancellationToken).ConfigureAwait(false);

                    await waitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    channelResult.Content.SetDataHandlingAdapterLogger(@this.Logger);


                    await @this.SendAsync(sendMessage, channelResult.Content, cancellationToken).ConfigureAwait(false);
                    return OperResult.Success;
                }
                finally
                {
                    waitLock.Release();
                }
            }
            catch (Exception ex)
            {
                return new(ex);
            }
        }
    }
    /// <inheritdoc/>
    public virtual ValueTask<OperResult> SendAsync(IClientChannel channel, ISendMessage sendMessage, CancellationToken cancellationToken)
    {
        return SendAsync(this, channel, sendMessage, cancellationToken);

        static async PooledValueTask<OperResult> SendAsync(ReceivedDeviceBase @this, IClientChannel channel, ISendMessage sendMessage, CancellationToken cancellationToken)
        {
            try
            {
                WaitLock? waitLock = @this.GetWaitLock(channel);

                try
                {
                    await @this.BeforeSendAsync(channel, cancellationToken).ConfigureAwait(false);

                    await waitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    channel.SetDataHandlingAdapterLogger(@this.Logger);


                    await @this.SendAsync(sendMessage, channel, cancellationToken).ConfigureAwait(false);
                    return OperResult.Success;
                }
                finally
                {
                    waitLock.Release();
                }
            }
            catch (Exception ex)
            {
                return new(ex);
            }
        }
    }

    /// <inheritdoc/>
    public virtual OperResult<IClientChannel> GetChannel()
    {
        if (Channel is IClientChannel clientChannel1)
            return new OperResult<IClientChannel>() { Content = clientChannel1 };

        var socketId = this is IDtu dtu1 ? dtu1.DtuId : null;

        if (string.IsNullOrWhiteSpace(socketId))
        {
            if (Channel is IClientChannel clientChannel)
                return new OperResult<IClientChannel>() { Content = clientChannel };
            else
                return new OperResult<IClientChannel>("The communication link cannot be obtained, DtuId must be set!");
        }


        if (Channel is ITcpServiceChannel serviceChannel)
        {
            if (serviceChannel.TryGetClient($"ID={socketId}", out var client))
            {
                return new OperResult<IClientChannel>() { Content = client };
            }
            else
            {
                if (serviceChannel.TryGetClient($"ID={socketId}", out var client1))
                {
                    return new OperResult<IClientChannel>() { Content = client1 };
                }

                return (new OperResult<IClientChannel>(string.Format(AppResource.DtuNoConnectedWaining, socketId)));

            }
        }
        else
        {
            if (Channel is IClientChannel clientChannel)
                return new OperResult<IClientChannel>() { Content = clientChannel };
            else
                return new OperResult<IClientChannel>("The communication link cannot be obtained!");
        }
    }

    /// <inheritdoc/>
    public virtual EndPoint? GetUdpEndpoint()
    {
        if (Channel is IDtuUdpSessionChannel udpSessionChannel)
        {
            var socketId = this is IDtu dtu1 ? dtu1.DtuId : null;
            if (string.IsNullOrWhiteSpace(socketId))
                return udpSessionChannel.DefaultEndpoint;

            {
                if (udpSessionChannel.TryGetEndPoint($"ID={socketId}", out var endPoint))
                {
                    return endPoint;
                }
                else
                {
                    if (udpSessionChannel.TryGetEndPoint($"ID={socketId}", out var endPoint1))
                    {
                        return endPoint1;
                    }

                    throw new Exception(string.Format(AppResource.DtuNoConnectedWaining, socketId));

                }
            }
        }

        return null;
    }
    /// <inheritdoc/>
    public virtual ValueTask<OperResult<ReadOnlyMemory<byte>>> SendThenReturnAsync(ISendMessage sendMessage, CancellationToken cancellationToken = default)
    {
        return SendThenReturnAsync(sendMessage, null, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult<ReadOnlyMemory<byte>>> SendThenReturnAsync(ISendMessage sendMessage, IClientChannel? channel, CancellationToken cancellationToken = default)
    {
        if (channel == null)
        {
            var channelResult = GetChannel();
            if (!channelResult.IsSuccess) return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(channelResult));
            channel = channelResult.Content!;
        }

        return SendThenReturn(this, sendMessage, channel, cancellationToken);

        static async PooledValueTask<OperResult<ReadOnlyMemory<byte>>> SendThenReturn(ReceivedDeviceBase @this, ISendMessage sendMessage, IClientChannel channel, CancellationToken cancellationToken)
        {
            try
            {
                var result = await @this.SendThenReturnMessageAsync(sendMessage, channel, cancellationToken).ConfigureAwait(false);
                return new OperResult<ReadOnlyMemory<byte>>(result) { Content = result.Content };
            }
            catch (Exception ex)
            {
                return new(ex);
            }
        }
    }

    /// <inheritdoc/>
    protected virtual ValueTask<DeviceMessage> SendThenReturnMessageAsync(ISendMessage sendMessage, CancellationToken cancellationToken = default)
    {
        var channelResult = GetChannel();
        if (!channelResult.IsSuccess) return EasyValueTask.FromResult(new DeviceMessage(channelResult));
        return SendThenReturnMessageAsync(sendMessage, channelResult.Content!, cancellationToken);
    }

    /// <inheritdoc/>
    protected virtual ValueTask<DeviceMessage> SendThenReturnMessageAsync(ISendMessage command, IClientChannel clientChannel, CancellationToken cancellationToken = default)
    {
        return GetResponsedDataAsync(command, clientChannel, Timeout, cancellationToken);
    }

    private ObjectPoolT<ReusableCancellationTokenSource> _reusableTimeouts = new();

    /// <summary>
    /// 发送并等待数据
    /// </summary>
    protected ValueTask<DeviceMessage> GetResponsedDataAsync(
        ISendMessage command,
        IClientChannel clientChannel,
        int timeout = 3000,
        CancellationToken cancellationToken = default)
    {
        return GetResponsedDataAsync(this, command, clientChannel, timeout, cancellationToken);

        static async PooledValueTask<DeviceMessage> GetResponsedDataAsync(ReceivedDeviceBase @this, ISendMessage command, IClientChannel clientChannel, int timeout, CancellationToken cancellationToken)
        {
            var waitData = clientChannel.WaitHandlePool.GetWaitDataAsync(out var sign);
            command.Sign = sign;
            WaitLock? waitLock = null;

            try
            {
                await @this.BeforeSendAsync(clientChannel, cancellationToken).ConfigureAwait(false);

                waitLock = @this.GetWaitLock(clientChannel);

                await waitLock.WaitAsync(cancellationToken).ConfigureAwait(false);

                clientChannel.SetDataHandlingAdapterLogger(@this.Logger);

                await @this.SendAsync(command, clientChannel, cancellationToken).ConfigureAwait(false);

                if (waitData.Status == WaitDataStatus.Success)
                    return waitData.CompletedData;

#pragma warning disable CA2000 // 丢失范围之前释放对象
                var reusableTimeout = @this._reusableTimeouts.Get() ?? new();
#pragma warning restore CA2000 // 丢失范围之前释放对象
                try
                {

                    var ctsToken = reusableTimeout.GetTokenSource(timeout, cancellationToken, @this.Channel?.ClosedToken ?? default);
                    await waitData.WaitAsync(ctsToken).ConfigureAwait(false);

                }
                catch (OperationCanceledException)
                {
                    return reusableTimeout.TimeoutStatus
                        ? new DeviceMessage(new TimeoutException()) { ErrorMessage = $"Timeout, sign: {sign}" }
                        : new DeviceMessage(new OperationCanceledException());
                }
                catch (Exception ex)
                {
                    return new DeviceMessage(ex);
                }
                finally
                {
                    reusableTimeout.Set();
                    @this._reusableTimeouts.Return(reusableTimeout);
                }

                if (waitData.Status == WaitDataStatus.Success)
                {
                    return waitData.CompletedData;
                }
                else
                {
                    var operResult = waitData.Check(reusableTimeout.TimeoutStatus);
                    if (waitData.CompletedData != null)
                    {
                        waitData.CompletedData.ErrorMessage = $"{operResult.ErrorMessage}, sign: {sign}";
                        return waitData.CompletedData;
                    }
                    else
                    {
                        return new DeviceMessage(new OperationCanceledException());
                    }

                    //return new DeviceMessage(operResult) { ErrorMessage = $"{operResult.ErrorMessage}, sign: {sign}" };
                }
            }
            catch (Exception ex)
            {
                return new DeviceMessage(ex);
            }
            finally
            {
                waitLock?.Release();
                waitData?.SafeDispose();

            }
        }
    }


    private WaitLock GetWaitLock(IClientChannel clientChannel)
    {
        WaitLock? waitLock = null;
        if (clientChannel is IDtuUdpSessionChannel udpSessionChannel)
        {
            waitLock = udpSessionChannel.GetLock(this is IDtu dtu1 ? dtu1.DtuId : null);
        }
        waitLock ??= clientChannel.GetLock(null);
        return waitLock;
    }

    #endregion 设备异步返回





    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (Channel != null)
        {
            lock (Channel)
            {

                Channel.Starting.Remove(ChannelStarting);
                Channel.Stoped.Remove(ChannelStoped);
                Channel.Started.Remove(ChannelStarted);
                Channel.Stoping.Remove(ChannelStoping);
                Channel.ChannelReceived.Remove(ChannelReceived);

                if (Channel.Collects.Count == 1)
                {
                    if (Channel is ITcpServiceChannel tcpServiceChannel)
                    {
                        tcpServiceChannel.Clients.ForEach(a => a.WaitHandlePool?.CancelAll());
                    }

                    try
                    {
                        //只关闭，不释放
                        _ = Channel.CloseAsync();
                        if (Channel is IClientChannel client)
                        {
                            client.WaitHandlePool?.CancelAll();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogWarning(ex);
                    }
                }
                else
                {
                    if (Channel is ITcpServiceChannel tcpServiceChannel && this is IDtu dtu)
                    {
                        if (tcpServiceChannel.TryGetClient($"ID={dtu.DtuId}", out var client))
                        {
                            client.WaitHandlePool?.CancelAll();
                            _ = client.CloseAsync();
                        }
                    }
                }

                Channel.Collects.Remove(this);

            }
        }
        _reusableTimeouts?.SafeDispose();
        _deviceLogger?.TryDispose();
        connectWaitLock?.SafeDispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override async Task DisposeAsync(bool disposing)
    {
        if (Channel != null)
        {
            Channel.Starting.Remove(ChannelStarting);
            Channel.Stoped.Remove(ChannelStoped);
            Channel.Started.Remove(ChannelStarted);
            Channel.Stoping.Remove(ChannelStoping);
            Channel.ChannelReceived.Remove(ChannelReceived);

            if (Channel.Collects.Count == 1)
            {
                if (Channel is ITcpServiceChannel tcpServiceChannel)
                {
                    tcpServiceChannel.Clients.ForEach(a => a.WaitHandlePool?.CancelAll());
                }

                try
                {
                    //只关闭，不释放
                    await Channel.CloseAsync().ConfigureAwait(false);
                    if (Channel is IClientChannel client)
                    {
                        client.WaitHandlePool?.CancelAll();
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogWarning(ex);
                }
            }
            else
            {
                if (Channel is ITcpServiceChannel tcpServiceChannel && this is IDtu dtu)
                {
                    if (tcpServiceChannel.TryGetClient($"ID={dtu.DtuId}", out var client))
                    {
                        client.WaitHandlePool?.CancelAll();
                        await client.CloseAsync().ConfigureAwait(false);
                    }
                }
            }

            Channel.Collects.Remove(this);


        }

        _reusableTimeouts?.SafeDispose();
        _deviceLogger?.TryDispose();
        connectWaitLock?.SafeDispose();
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    public virtual Action<IPluginManager> ConfigurePlugins(TouchSocketConfig config)
    {
        ArgumentNullExceptionEx.ThrowIfNull(Channel, nameof(Channel));
        ArgumentNullExceptionEx.ThrowIfNull(Channel.ChannelOptions, nameof(Channel.ChannelOptions));
        switch (Channel.ChannelType)
        {
            case ChannelTypeEnum.TcpService:
                {
                    if (Channel.ChannelOptions.DtuSeviceType == DtuSeviceType.Default)
                        return PluginUtil.GetDtuPlugin(Channel.ChannelOptions);
                    else
                        return PluginUtil.GetTcpServicePlugin(Channel.ChannelOptions);
                }

        }
        return a => { };
    }

}
