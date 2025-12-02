//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text.Json.Nodes;

namespace ThingsGateway.Foundation;

/// <summary>
/// 协议设备接口
/// </summary>
public interface IDevice : IReceivedDevice, IDisposable, IDisposable2, IAsyncDisposable
{

    #region 变量地址解析

    /// <summary>
    /// 寄存器地址的详细说明
    /// </summary>
    /// <returns></returns>
    string GetAddressDescription();

    /// <summary>
    /// 获取变量地址对应的bit偏移，默认0
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    int GetBitOffsetDefault(string address);

    /// <summary>
    /// 获取变量地址对应的bit偏移
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    int? GetBitOffset(string address);

    /// <summary>
    /// 布尔量解析时是否需要按字反转
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    bool BitReverse(string address);

    /// <summary>
    /// 布尔量解析时是否需要按字反转
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    bool BitReverse(IDeviceAddress address);

    /// <summary>
    /// 获取数据类型对应的寄存器长度
    /// </summary>
    /// <param name="address">寄存器地址</param>
    /// <param name="length">读取数量</param>
    /// <param name="typeLength">读取数据类型对应的字节长度</param>
    /// <param name="isBool">是否按布尔解析</param>
    /// <returns></returns>
    int GetLength(string address, int length, int typeLength, bool isBool = false);

    /// <summary>
    /// 连读寄存器打包
    /// </summary>
    List<T> LoadSourceRead<T, T2>(IEnumerable<T2> deviceVariables, int maxPack, string defaultIntervalTime) where T : IVariableSource<T2>, new() where T2 : IVariable;

    #endregion 变量地址解析

    ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(IDeviceAddress address, DataTypeEnum dataType, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量读取字节数组信息，需要指定地址和长度
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取寄存器数量，对于不同PLC，对应的字节数量可能不一样</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="bitConverter">bitConverter</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(string address, int length, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);


    /// <summary>
    /// 根据数据类型，写入类型值
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="bitConverter">bitConverter</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult> WriteAsync(string address, JsonNode? value, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入原始的byte数组数据到指定的地址，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<byte> value, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);


    ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<string> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);
    ValueTask<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);
    ValueTask<OperResult<string>> ReadStringAsync(string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);
    ValueTask<OperResult<string[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);
}
