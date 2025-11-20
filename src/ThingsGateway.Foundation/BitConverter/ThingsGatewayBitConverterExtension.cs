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

namespace ThingsGateway.Foundation;

/// <summary>
/// ThingsGatewayBitConverterExtensions
/// </summary>
public static class ThingsGatewayBitConverterExtension
{


    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static bool GetChangedDataFormJsonNode(
        JsonNode jToken,
        DataTypeEnum dataType,
        int arrayLength,
        object? oldValue,
        out object? result)
    {
        switch (dataType)
        {
            case DataTypeEnum.Boolean:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<Boolean[]>();
                    if (oldValue is bool[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<Boolean>();
                    if (oldValue is bool oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Byte:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<Byte[]>();
                    if (oldValue is byte[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<Byte>();
                    if (oldValue is byte oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Int16:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<Int16[]>();
                    if (oldValue is short[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<Int16>();
                    if (oldValue is short oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.UInt16:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<UInt16[]>();
                    if (oldValue is ushort[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<UInt16>();
                    if (oldValue is ushort oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Int32:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<Int32[]>();
                    if (oldValue is int[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<Int32>();
                    if (oldValue is int oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.UInt32:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<UInt32[]>();
                    if (oldValue is uint[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<UInt32>();
                    if (oldValue is uint oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Int64:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<Int64[]>();
                    if (oldValue is long[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<Int64>();
                    if (oldValue is long oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.UInt64:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<UInt64[]>();
                    if (oldValue is ulong[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<UInt64>();
                    if (oldValue is ulong oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Float:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<Single[]>();
                    if (oldValue is float[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<Single>();
                    if (oldValue is float oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Double:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<Double[]>();
                    if (oldValue is double[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<Double>();
                    if (oldValue is double oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
            case DataTypeEnum.Decimal:
                if (arrayLength > 1)
                {
                    var newVal = jToken.Deserialize<Decimal[]>();
                    if (oldValue is decimal[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = jToken.Deserialize<Decimal>();
                    if (oldValue is decimal oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
            case DataTypeEnum.String:
            default:
                if (arrayLength > 1)
                {
                    var newArr = new string[arrayLength];
                    for (int i = 0; i < arrayLength; i++)
                    {
                        newArr[i] = jToken.Deserialize<string>();
                    }

                    if (oldValue is string[] oldArr && newArr.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newArr;
                    return true;
                }
                else
                {
                    var str = jToken.Deserialize<string>();
                    if (oldValue is string oldStr && oldStr == str)
                    {
                        result = oldStr;
                        return false;
                    }
                    result = str;
                    return true;
                }
        }
    }


    /// <summary>
    /// 根据数据类型获取字节数组
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static ReadOnlyMemory<byte> GetBytesFromData(this IThingsGatewayBitConverter byteConverter, JsonNode? value, DataTypeEnum dataType)
    {
        ArgumentNullExceptionEx.ThrowIfNull(value);
        bool array = value is JsonArray jsonArray ? jsonArray.Count > 1 ? true : false : false;
        if (array)
        {
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    return byteConverter.GetBytes(GetArray<bool>(value));
                case DataTypeEnum.Byte:
                    return GetArray<byte>(value);
                case DataTypeEnum.Int16:
                    return byteConverter.GetBytes(GetArray<short>(value));
                case DataTypeEnum.UInt16:
                    return byteConverter.GetBytes(GetArray<ushort>(value));
                case DataTypeEnum.Int32:
                    return byteConverter.GetBytes(GetArray<int>(value));
                case DataTypeEnum.UInt32:
                    return byteConverter.GetBytes(GetArray<uint>(value));
                case DataTypeEnum.Int64:
                    return byteConverter.GetBytes(GetArray<long>(value));
                case DataTypeEnum.UInt64:
                    return byteConverter.GetBytes(GetArray<ulong>(value));
                case DataTypeEnum.Float:
                    return byteConverter.GetBytes(GetArray<float>(value));
                case DataTypeEnum.Double:
                    return byteConverter.GetBytes(GetArray<double>(value));
                case DataTypeEnum.Decimal:
                    return byteConverter.GetBytes(GetArray<decimal>(value));
                case DataTypeEnum.String:
                    {
                        var strings = GetArray<string>(value);
                        var str = strings.ArrayToString();
                        var data = byteConverter.GetBytes(str);
                        return data.ArrayExpandToLength(byteConverter.StringLength ?? data.Length);
                        //using ValueListBuilder<ReadOnlyMemory<byte>> bytes = new(strings.Length);
                        //foreach (var str in strings)
                        //{
                        //    var data = byteConverter.GetBytes(str);
                        //    bytes.Add();
                        //}
                        //return bytes.AsSpan().CombineMemoryBlocks();
                    }
                default:

                    throw new NotSupportedException(
                        string.Format(ThingsGateway.Foundation.AppResource.DataTypeNotSupported, dataType));

            }
        }
        else
        {
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    return byteConverter.GetBytes(value.GetValue<bool>());
                case DataTypeEnum.Byte:
                    return byteConverter.GetBytes(value.GetValue<byte>());
                case DataTypeEnum.Int16:
                    return byteConverter.GetBytes(value.GetValue<short>());
                case DataTypeEnum.UInt16:
                    return byteConverter.GetBytes(value.GetValue<ushort>());
                case DataTypeEnum.Int32:
                    return byteConverter.GetBytes(value.GetValue<int>());
                case DataTypeEnum.UInt32:
                    return byteConverter.GetBytes(value.GetValue<uint>());
                case DataTypeEnum.Int64:
                    return byteConverter.GetBytes(value.GetValue<long>());
                case DataTypeEnum.UInt64:
                    return byteConverter.GetBytes(value.GetValue<ulong>());
                case DataTypeEnum.Float:
                    return byteConverter.GetBytes(value.GetValue<float>());
                case DataTypeEnum.Double:
                    return byteConverter.GetBytes(value.GetValue<double>());
                case DataTypeEnum.Decimal:
                    return byteConverter.GetBytes(value.GetValue<decimal>());
                case DataTypeEnum.String:
                    return byteConverter.GetBytes(value.GetValue<string>());
                default:

                    throw new NotSupportedException(
                        string.Format(ThingsGateway.Foundation.AppResource.DataTypeNotSupported, dataType));

            }
        }

        // 本地函数：解析数组
        static T[] GetArray<T>(JsonNode node)
        {
            if (node is JsonArray arr)
            {
                var result = new T[arr.Count];
                for (int i = 0; i < arr.Count; i++)
                {
                    result[i] = arr[i]!.GetValue<T>();
                }
                return result;
            }

            // 容错：单值转为单元素数组
            return new T[] { node.GetValue<T>() };
        }
    }

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static bool GetChangedDataFormBytes(this IThingsGatewayBitConverter byteConverter, IDevice device, string address, ReadOnlyMemory<byte> buffer, int index, DataTypeEnum dataType, int arrayLength, object? oldValue, out object? result)
    {
        var span = buffer.Span;
        switch (dataType)
        {
            case DataTypeEnum.Boolean:
                return GetBool(byteConverter, device, address, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.Byte:
                return GetByte(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.Int16:
                return GetInt16(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.UInt16:
                return GetUInt16(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.Int32:
                return GetInt32(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.UInt32:
                return GetUInt32(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.Int64:
                return GetInt64(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.UInt64:
                return GetUInt64(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.Float:
                return GetFloat(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.Double:
                return GetDouble(byteConverter, index, arrayLength, oldValue, out result, span);
            case DataTypeEnum.Decimal:
                return GetDecimal(byteConverter, index, arrayLength, oldValue, out result, span);

            case DataTypeEnum.String:
                return GetString(byteConverter, index, arrayLength, oldValue, out result, span);

            default:

                throw new NotSupportedException(
                    string.Format(ThingsGateway.Foundation.AppResource.DataTypeNotSupported, dataType));

        }


    }

    private static bool GetString(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newArr = new string[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                newArr[i] = byteConverter.ToString(span, index + i * (byteConverter.StringLength ?? 1), byteConverter.StringLength ?? 1);
            }

            if (oldValue is string[] oldArr && newArr.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newArr;
            return true;
        }
        else
        {
            var str = byteConverter.ToString(span, index, byteConverter.StringLength ?? 1);
            if (oldValue is string oldStr && oldStr == str)
            {
                result = oldStr;
                return false;
            }
            result = str;
            return true;
        }
    }

    private static bool GetDecimal(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToDecimal(span, index, arrayLength);
            if (oldValue is decimal[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToDecimal(span, index);
            if (oldValue is decimal oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetDouble(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToDouble(span, index, arrayLength);
            if (oldValue is double[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToDouble(span, index);
            if (oldValue is double oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetFloat(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToSingle(span, index, arrayLength);
            if (oldValue is float[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToSingle(span, index);
            if (oldValue is float oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetUInt64(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToUInt64(span, index, arrayLength);
            if (oldValue is ulong[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToUInt64(span, index);
            if (oldValue is ulong oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetInt64(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToInt64(span, index, arrayLength);
            if (oldValue is long[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToInt64(span, index);
            if (oldValue is long oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetUInt32(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToUInt32(span, index, arrayLength);
            if (oldValue is uint[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToUInt32(span, index);
            if (oldValue is uint oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetInt32(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToInt32(span, index, arrayLength);
            if (oldValue is int[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToInt32(span, index);
            if (oldValue is int oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetUInt16(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToUInt16(span, index, arrayLength);
            if (oldValue is ushort[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToUInt16(span, index);
            if (oldValue is ushort oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetInt16(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToInt16(span, index, arrayLength);
            if (oldValue is short[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToInt16(span, index);
            if (oldValue is short oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetByte(IThingsGatewayBitConverter byteConverter, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToByte(span, index, arrayLength);
            if (oldValue is byte[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToByte(span, index);
            if (oldValue is byte oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }

    private static bool GetBool(IThingsGatewayBitConverter byteConverter, IDevice device, string address, int index, int arrayLength, object? oldValue, out object result, ReadOnlySpan<byte> span)
    {
        if (arrayLength > 1)
        {
            var newVal = byteConverter.ToBoolean(span, index, arrayLength, device.BitReverse(address));
            if (oldValue is bool[] oldArr && newVal.SequenceEqual(oldArr))
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
        else
        {
            var newVal = byteConverter.ToBoolean(span, index, device.BitReverse(address));
            if (oldValue is bool oldVal && oldVal == newVal)
            {
                result = oldValue;
                return false;
            }
            result = newVal;
            return true;
        }
    }


}
