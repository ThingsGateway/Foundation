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

using ThingsGateway.Foundation.Common.StringExtension;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation;

/// <summary>
/// 协议基类
/// </summary>
public abstract class DeviceBase : ReceivedDeviceBase, IDevice
{
    /// <inheritdoc/>
    ~DeviceBase()
    {
        this.SafeDispose();
    }
    public override bool SupportMultipleDevice()
    {
        return true;
    }
    #region 变量地址解析

    public abstract bool BitReverse(IDeviceAddress address);

    public abstract List<T> LoadSourceRead<T, T2>(IEnumerable<T2> deviceVariables, int maxPack, string defaultIntervalTime)
        where T : IVariableSource<T2>, new()
        where T2 : IVariable;


    /// <inheritdoc/>
    public virtual string GetAddressDescription()
    {
        return AppResource.DefaultAddressDes;
    }
    /// <summary>
    /// 获取bit偏移量
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public virtual int GetBitOffsetDefault(string address)
    {
        return GetBitOffset(address) ?? 0;
    }
    /// <summary>
    /// 获取bit偏移量
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public virtual int? GetBitOffset(string address)
    {
        int? bitIndex = null;
        if (address?.IndexOf('.') > 0)
            bitIndex = address.SplitStringByDelimiter()!.Last().ToInt();
        return bitIndex;
    }

    /// <inheritdoc/>
    public virtual bool BitReverse(string address)
    {
        return address?.IndexOf('.') > 0;
    }

    /// <inheritdoc/>
    public virtual int GetLength(string address, int length, int typeLength, bool isBool = false)
    {
        var result = Math.Ceiling((double)length * typeLength / RegisterByteLength);
        if (isBool && GetBitOffset(address) != null)
        {
            var data = Math.Ceiling((double)length / RegisterByteLength / 8);
            return (int)data;
        }
        else
        {
            return (int)result;
        }
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<string> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= BitConverter.GetTransByAddress(address);
        if (bitConverter.StringLength == null) return EasyValueTask.FromResult(new OperResult(AppResource.StringAddressError));
        var data = value.Span.ArrayToString();

        return WriteAsync(address, data, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= BitConverter.GetTransByAddress(address);
        var data = bitConverter.GetBytes(value);
        return WriteAsync(address, data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length), DataTypeEnum.String, bitConverter, cancellationToken);
    }
    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<String>> ReadStringAsync(string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await ReadStringAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }
    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<String[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= BitConverter.GetTransByAddress(address);
        if (bitConverter.StringLength == null) return new OperResult<String[]>(AppResource.StringAddressError);
        var len = bitConverter.StringLength * length;

        var result = await ReadAsync(address, GetLength(address, len.Value, 1), DataTypeEnum.String, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() =>
        {
            String[] strings = new String[length];
            for (int i = 0; i < length; i++)
            {
                var data = bitConverter.ToString(result.Content.Span, i * bitConverter.StringLength.Value, bitConverter.StringLength.Value);
                strings[i] = data;
            }
            return strings;
        }
        );
    }


    public abstract ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(IDeviceAddress address, DataTypeEnum dataType, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default);

    public abstract ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(string address, int length, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);

    public virtual ValueTask<OperResult> WriteAsync(string address, JsonNode? value, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        return this.WriteJsonNodeAsync(address, value, dataType, bitConverter, cancellationToken);
    }

    public abstract ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<byte> value, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default);


    #endregion 变量地址解析



}
