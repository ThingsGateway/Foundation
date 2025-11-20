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
using System.Text.Json;
using System.Text.Json.Nodes;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

/// <summary>
/// 协议基类
/// </summary>
public static partial class DeviceExtension
{
    /// <inheritdoc/>
    public static async ValueTask<IOperResult<JsonNode>> ReadJsonNodeAsync(this IDevice device, string address, int length, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        if (length <= 1)
        {
            switch (dataType)
            {
                case DataTypeEnum.String:
                    return (await device.ReadStringAsync(address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Boolean:
                    return (await ReadBooleanAsync(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Byte:
                    return (await ReadByteAsync(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Int16:
                    return (await ReadInt16Async(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.UInt16:
                    return (await ReadUInt16Async(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Int32:
                    return (await ReadInt32Async(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.UInt32:
                    return (await ReadUInt32Async(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Int64:
                    return (await ReadInt64Async(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.UInt64:
                    return (await ReadUInt64Async(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Float:
                    return (await ReadFloatAsync(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Double:
                    return (await ReadDoubleAsync(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Decimal:
                    return (await ReadDecimalAsync(device, address, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => JsonValue.Create(a));
                case DataTypeEnum.Object:
                default:
                    return new OperResult<JsonNode>(string.Format(AppResource.DataTypeNotSupported, dataType));
            }
        }
        else
        {
            switch (dataType)
            {
                case DataTypeEnum.String:
                    return (await device.ReadStringAsync(address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.Boolean:
                    return (await ReadBooleanAsync(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.Byte:
                    return (await ReadByteAsync(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.Span.ToJsonArray());
                case DataTypeEnum.Int16:
                    return (await ReadInt16Async(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.UInt16:
                    return (await ReadUInt16Async(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.Int32:
                    return (await ReadInt32Async(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.UInt32:
                    return (await ReadUInt32Async(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.Int64:
                    return (await ReadInt64Async(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.UInt64:
                    return (await ReadUInt64Async(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.Float:
                    return (await ReadFloatAsync(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.Double:
                    return (await ReadDoubleAsync(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.Decimal:
                    return (await ReadDecimalAsync(device, address, length, bitConverter, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToJsonArray());
                case DataTypeEnum.Object:
                default:
                    return new OperResult<JsonNode>(string.Format(AppResource.DataTypeNotSupported, dataType));
            }
        }
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static async ValueTask<OperResult> WriteJsonNodeAsync(this IDevice device, string address, JsonNode value, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (value is JsonArray jArray)
            {
                return dataType switch
                {
                    DataTypeEnum.String => await device.WriteAsync(address, jArray.Deserialize<String[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Boolean => await device.WriteAsync(address, jArray.Deserialize<Boolean[]>().AsMemory(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Byte => await device.WriteAsync(address, jArray.Deserialize<Byte[]>().AsMemory(), dataType, bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int16 => await device.WriteAsync(address, jArray.Deserialize<Int16[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt16 => await device.WriteAsync(address, jArray.Deserialize<UInt16[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int32 => await device.WriteAsync(address, jArray.Deserialize<Int32[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt32 => await device.WriteAsync(address, jArray.Deserialize<UInt32[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int64 => await device.WriteAsync(address, jArray.Deserialize<Int64[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt64 => await device.WriteAsync(address, jArray.Deserialize<UInt64[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Float => await device.WriteAsync(address, jArray.Deserialize<Single[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Double => await device.WriteAsync(address, jArray.Deserialize<Double[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Decimal => await device.WriteAsync(address, jArray.Deserialize<Decimal[]>().AsMemory(), bitConverter, cancellationToken: cancellationToken).ConfigureAwait(false),
                    _ => new OperResult(string.Format(AppResource.DataTypeNotSupported, dataType)),
                };
            }
            else
            {
                return dataType switch
                {
                    DataTypeEnum.String => await device.WriteAsync(address, value.Deserialize<String>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Boolean => await device.WriteAsync(address, value.Deserialize<Boolean>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Byte => await device.WriteAsync(address, value.Deserialize<Byte>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int16 => await device.WriteAsync(address, value.Deserialize<Int16>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt16 => await device.WriteAsync(address, value.Deserialize<UInt16>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int32 => await device.WriteAsync(address, value.Deserialize<Int32>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt32 => await device.WriteAsync(address, value.Deserialize<UInt32>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Int64 => await device.WriteAsync(address, value.Deserialize<Int64>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.UInt64 => await device.WriteAsync(address, value.Deserialize<UInt64>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Float => await device.WriteAsync(address, value.Deserialize<Single>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Double => await device.WriteAsync(address, value.Deserialize<Double>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    DataTypeEnum.Decimal => await device.WriteAsync(address, value.Deserialize<Decimal>(), bitConverter, cancellationToken).ConfigureAwait(false),
                    _ => new OperResult(string.Format(AppResource.DataTypeNotSupported, dataType)),
                };
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }


    #region 读取
    /// <inheritdoc/>
    public static ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadByteAsync(this IDevice device, IDeviceAddress address, CancellationToken cancellationToken = default)
    {
        return device.ReadAsync(address, DataTypeEnum.Byte, null!, cancellationToken);
    }
    /// <inheritdoc/>
    public static ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadByteAsync(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);

        return device.ReadAsync(address, device.GetLength(address, length, device.RegisterByteLength, true), DataTypeEnum.Byte, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<bool[]>> ReadBooleanAsync(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);

        var result = await device.ReadAsync(address, device.GetLength(address, length, device.RegisterByteLength, true), DataTypeEnum.Boolean, bitConverter, cancellationToken).ConfigureAwait(false);

        return result.OperResultFrom(() => bitConverter.ToBoolean(result.Content.Span, device.GetBitOffsetDefault(address), length, device.BitReverse(address)));
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int16[]>> ReadInt16Async(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 2), DataTypeEnum.Int16, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt16(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt16[]>> ReadUInt16Async(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 2), DataTypeEnum.UInt16, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt16(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int32[]>> ReadInt32Async(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 4), DataTypeEnum.Int32, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt32(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt32[]>> ReadUInt32Async(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 4), DataTypeEnum.UInt32, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt32(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int64[]>> ReadInt64Async(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 8), DataTypeEnum.Int64, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt64(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt64[]>> ReadUInt64Async(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 8), DataTypeEnum.UInt64, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt64(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Single[]>> ReadFloatAsync(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 4), DataTypeEnum.Float, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToSingle(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Double[]>> ReadDoubleAsync(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 8), DataTypeEnum.Double, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToDouble(result.Content.Span, 0, length));
    }
    /// <inheritdoc/>
    public static async ValueTask<OperResult<Decimal[]>> ReadDecimalAsync(this IDevice device, string address, int length, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        var result = await device.ReadAsync(address, device.GetLength(address, length, 8), DataTypeEnum.Decimal, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToDecimal(result.Content.Span, 0, length));
    }


    #endregion 读取

    #region 写入

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<bool> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Boolean, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, bool value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Boolean, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, byte value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Byte, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, short value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Int16, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ushort value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.UInt16, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, int value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Int32, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, uint value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.UInt32, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, long value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Int64, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ulong value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.UInt64, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, float value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Float, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, double value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Double, bitConverter, cancellationToken);
    }
    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, decimal value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Decimal, bitConverter, cancellationToken);
    }


    #endregion 写入
    #region 写入数组

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<short> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Int16, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<ushort> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.UInt16, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<int> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Int32, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<uint> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.UInt32, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<long> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Int64, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<ulong> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.UInt64, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<float> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Float, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<double> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Double, bitConverter, cancellationToken);
    }

    /// <inheritdoc/>
    public static ValueTask<OperResult> WriteAsync(this IDevice device, string address, ReadOnlyMemory<decimal> value, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= device.BitConverter.GetTransByAddress(address);
        return device.WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Decimal, bitConverter, cancellationToken);
    }



    #endregion 写入数组

    #region 读取

    /// <inheritdoc/>
    public static async ValueTask<OperResult<byte>> ReadByteAsync(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadByteAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content!.Span[0]);
    }
    /// <inheritdoc/>
    public static async ValueTask<OperResult<Boolean>> ReadBooleanAsync(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadBooleanAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Double>> ReadDoubleAsync(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadDoubleAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int16>> ReadInt16Async(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadInt16Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int32>> ReadInt32Async(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadInt32Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Int64>> ReadInt64Async(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadInt64Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<Single>> ReadFloatAsync(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadFloatAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }



    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt16>> ReadUInt16Async(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadUInt16Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt32>> ReadUInt32Async(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadUInt32Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }

    /// <inheritdoc/>
    public static async ValueTask<OperResult<UInt64>> ReadUInt64Async(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadUInt64Async(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }
    /// <inheritdoc/>
    public static async ValueTask<OperResult<Decimal>> ReadDecimalAsync(this IDevice device, string address, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        var result = await device.ReadDecimalAsync(address, 1, bitConverter, cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => result.Content![0]);
    }
    #endregion 读取



    /// <summary>
    /// 在返回的字节数组中解析每个变量的值
    /// 根据每个变量的<see cref="IVariable.Index"/>
    /// 不支持变长字符串类型变量，不能存在于变量List中
    /// </summary>
    /// <param name="device">设备</param>
    /// <param name="variables">设备变量List</param>
    /// <param name="buffer">返回的字节数组</param>
    /// <param name="exWhenAny">任意一个失败时抛出异常</param>
    /// <returns>解析结果</returns>
    public static OperResult PraseStructContent<T>(this IEnumerable<T> variables, IDevice device, ReadOnlyMemory<byte> buffer, bool exWhenAny) where T : IVariable
    {
        var time = DateTime.Now;
        if (variables is IList<T> collection)
        {
            return PraseCollection(collection, device, buffer, exWhenAny, time);
        }
        else
        {
            return PraseEnumerable(variables, device, buffer, exWhenAny, time);

        }
        static OperResult PraseEnumerable(IEnumerable<T> variables, IDevice device, ReadOnlyMemory<byte> buffer, bool exWhenAny, DateTime time)
        {
            foreach (var variable in variables)
            {
                ArgumentNullExceptionEx.ThrowIfNull(variable.BitConverter, nameof(variable.BitConverter));
                ArgumentNullExceptionEx.ThrowIfNull(variable.RegisterAddress, nameof(variable.RegisterAddress));
                IThingsGatewayBitConverter byteConverter = variable.BitConverter;
                var dataType = variable.DataType;
                int index = variable.Index;
                try
                {
                    var changed = byteConverter.GetChangedDataFormBytes(device, variable.RegisterAddress, buffer, index, dataType, variable.ArrayLength ?? 1, variable.RawValue, out var data);
                    if (changed)
                    {
                        var result = variable.SetValue(data, time);
                        if (exWhenAny)
                            if (!result.IsSuccess)
                                return result;
                    }
                    else
                    {
                        variable.TimeChanged(time);
                    }
                }
                catch (Exception ex)
                {
                    return new OperResult($"Error parsing byte array, address: {variable.RegisterAddress}, array length: {buffer.Length}, index: {index}, type: {dataType}", ex);
                }
            }

            return OperResult.Success;
        }
        static OperResult PraseCollection(IList<T> variables, IDevice device, ReadOnlyMemory<byte> buffer, bool exWhenAny, DateTime time)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];

                ArgumentNullExceptionEx.ThrowIfNull(variable.BitConverter, nameof(variable.BitConverter));
                ArgumentNullExceptionEx.ThrowIfNull(variable.RegisterAddress, nameof(variable.RegisterAddress));
                IThingsGatewayBitConverter byteConverter = variable.BitConverter;
                var dataType = variable.DataType;
                int index = variable.Index;
                try
                {
                    var changed = byteConverter.GetChangedDataFormBytes(device, variable.RegisterAddress, buffer, index, dataType, variable.ArrayLength ?? 1, variable.RawValue, out var data);
                    if (changed)
                    {
                        var result = variable.SetValue(data, time);
                        if (exWhenAny)
                            if (!result.IsSuccess)
                                return result;
                    }
                    else
                    {
                        variable.TimeChanged(time);
                    }
                }
                catch (Exception ex)
                {
                    return new OperResult($"Error parsing byte array, address: {variable.RegisterAddress}, array length: {buffer.Length}, index: {index}, type: {dataType}", ex);
                }
            }

            return OperResult.Success;
        }
    }

    /// <summary>
    /// 当状态不是<see cref="WaitDataStatus.Success"/>时返回异常。
    /// </summary>
    public static OperResult Check(this AsyncWaitData<DeviceMessage> waitDataAsync, bool timeout)
    {
        switch (waitDataAsync.Status)
        {
            case WaitDataStatus.Success:
                return new();

            case WaitDataStatus.Canceled:
                if (timeout)
                {
                    if (waitDataAsync.CompletedData != null)
                    {
                        waitDataAsync.CompletedData.Exception = new TimeoutException();
                        if (waitDataAsync.CompletedData.IsSuccess) waitDataAsync.CompletedData.OperCode = 999;
                        if (waitDataAsync.CompletedData.ErrorMessage.IsNullOrEmpty()) waitDataAsync.CompletedData.ErrorMessage = "Timeout";
                        return new(waitDataAsync.CompletedData);
                    }
                    else
                    {
                        return new(new TimeoutException());
                    }
                }
                else
                {

                    return new(new OperationCanceledException());
                }
            case WaitDataStatus.Overtime:
                {
                    if (waitDataAsync.CompletedData != null)
                    {
                        waitDataAsync.CompletedData.Exception = new TimeoutException();
                        if (waitDataAsync.CompletedData.IsSuccess) waitDataAsync.CompletedData.OperCode = 999;
                        if (waitDataAsync.CompletedData.ErrorMessage.IsNullOrEmpty()) waitDataAsync.CompletedData.ErrorMessage = "Timeout";
                        return new(waitDataAsync.CompletedData);
                    }
                    else
                    {
                        return new(new TimeoutException());
                    }
                }
            case WaitDataStatus.Disposed:
            case WaitDataStatus.Default:
            default:
                {
                    return waitDataAsync.CompletedData == null ? new(new Exception(AppResource.UnknownError)) : new(waitDataAsync.CompletedData);
                }
        }
    }
}
