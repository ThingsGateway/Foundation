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

/// <summary>
/// 协议设备接口
/// </summary>
public interface IReceivedDevice : IDisposable, IDisposable2, IAsyncDisposable
{
    #region 属性

    /// <summary>
    /// 通道
    /// </summary>
    IChannel? Channel { get; }

    /// <summary>
    /// 日志
    /// </summary>
    ILog? Logger { get; }

    /// <inheritdoc/>
    bool OnLine { get; }

    /// <summary>
    /// 一个寄存器所占的字节长度
    /// </summary>
    int RegisterByteLength { get; }

    /// <summary>
    /// 发送前延时
    /// </summary>
    int SendDelayTime { get; set; }

    /// <summary>
    /// 数据解析规则
    /// </summary>
    IThingsGatewayBitConverter BitConverter { get; }

    /// <summary>
    /// 读写超时时间
    /// </summary>
    int Timeout { get; set; }

    /// <summary>
    /// 字节顺序
    /// </summary>
    DataFormatEnum DataFormat { get; set; }

    /// <summary>
    /// 字符串翻转
    /// </summary>
    bool IsStringReverseByteWord { get; set; }

    bool AutoConnect { get; }
    bool IsHexLog { get; init; }

    #endregion 属性

    /// <summary>
    /// 获取新的适配器实例
    /// </summary>
    DataHandlingAdapter GetDataAdapter();

    ValueTask ConnectAsync(CancellationToken token = default);

    /// <summary>
    /// 配置IPluginManager
    /// </summary>
    Action<IPluginManager> ConfigurePlugins(TouchSocketConfig config);

    /// <summary>
    /// 获取通道
    /// </summary>
    /// <returns></returns>
    OperResult<IClientChannel> GetChannel();

    /// <summary>
    /// 发送，会经过适配器
    /// </summary>
    /// <param name="sendMessage">发送字节数组</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>返回消息体</returns>
    ValueTask<OperResult> SendAsync(ISendMessage sendMessage, CancellationToken cancellationToken);

    /// <summary>
    /// 发送并等待返回，会经过适配器，可传入<see cref="IClientChannel"/>，如果为空，则默认通道必须为<see cref="IClientChannel"/>类型
    /// </summary>
    /// <param name="command">发送字节数组</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <param name="channel">通道</param>
    /// <returns>返回消息体</returns>
    ValueTask<OperResult<ReadOnlyMemory<byte>>> SendThenReturnAsync(ISendMessage command, IClientChannel? channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// 支持通道多设备
    /// </summary>
    /// <returns></returns>
    bool SupportMultipleDevice();

    /// <summary>
    /// 初始化通道信息
    /// </summary>
    /// <param name="channel">通道</param>
    /// <param name="deviceLog">单独设备日志</param>
    void InitChannel(IChannel channel, ILog? deviceLog = null);

    /// <summary>
    /// 创建通道
    /// </summary>
    /// <param name="config"></param>
    /// <param name="channelOptions"></param>
    /// <returns></returns>
    IChannel CreateChannel(TouchSocketConfig config, IChannelOptions channelOptions);
}
