//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;
using ThingsGateway.Foundation.Common.Extension;

namespace ThingsGateway.Foundation.SiemensS7;

internal sealed partial class SiemensHelper
{
    public static List<List<SiemensS7Address>> GroupByLength(SiemensS7Address[] a, int pduLength)
    {
        List<List<SiemensS7Address>> groups = new List<List<SiemensS7Address>>();
        List<SiemensS7Address> sortedItems = a.OrderByDescending(item => item.Length).ToList(); // 按长度降序排序

        while (sortedItems.Count > 0)
        {
            List<SiemensS7Address> currentGroup = new List<SiemensS7Address>();
            int currentGroupLength = 0;

            for (int i = 0; i < sortedItems.Count; i++)
            {
                SiemensS7Address item = sortedItems[i];
                if (currentGroupLength + item.Length <= pduLength) // 如果可以添加到当前组
                {
                    currentGroup.Add(item);
                    currentGroupLength += item.Length;
                    sortedItems.RemoveAt(i); // 从列表中移除已添加到组的项
                    i--; // 因为我们移除了一个元素，所以索引需要回退
                }
                else if (i == sortedItems.Count - 1) // 如果这是最后一个元素且不能添加到当前组
                {
                    // 创建一个新组并添加这个元素
                    groups.Add(new List<SiemensS7Address> { item });
                    sortedItems.RemoveAt(i);
                }
            }

            if (currentGroup.Count > 0) // 如果当前组不为空
            {
                groups.Add(currentGroup);
            }
        }

        return groups;
    }

    internal static string GetCpuError(ushort Error)
    {
        return Error switch
        {
            0x01 => AppResource.ERROR1,
            0x03 => AppResource.ERROR3,
            0x05 => AppResource.ERROR5,
            0x06 => AppResource.ERROR6,
            0x07 => AppResource.ERROR7,
            0x0a => AppResource.ERROR10,
            _ => "Unknown",
        };
    }
    internal static async ValueTask<OperResult<string>> ReadStringAsync(
        SiemensS7Master plc,
        string address,
        IThingsGatewayBitConverter bitConverter,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.ASCII;

        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            // 先读取长度
            var lengthResult = await plc.ReadAsync(address, 2, bitConverter, cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);
            if (!lengthResult.IsSuccess)
                return new OperResult<string>(lengthResult);

            var span = lengthResult.Content.Span;
            if (span[0] == 0 || span[0] == byte.MaxValue)
                return new OperResult<string>(AppResource.NotString);

            // 再读取实际内容
            var dataResult = await plc.ReadAsync(address, 2 + span[1], bitConverter, cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
            if (!dataResult.IsSuccess)
                return new OperResult<string>(dataResult);

            return OperResult.CreateSuccessResult(
                encoding.GetString(dataResult.Content.Span.Slice(2, dataResult.Content.Length - 2)));
        }
        else
        {
            var lengthResult = await plc.ReadAsync(address, 1, bitConverter, cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);
            if (!lengthResult.IsSuccess)
                return new OperResult<string>(lengthResult);

            var span = lengthResult.Content.Span;
            var dataResult = await plc.ReadAsync(address, 1 + span[0], bitConverter, cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
            if (!dataResult.IsSuccess)
                return new OperResult<string>(dataResult);

            return OperResult.CreateSuccessResult(
                encoding.GetString(dataResult.Content.Span.Slice(1, dataResult.Content.Length - 1)));
        }
    }

    internal static async ValueTask<OperResult> WriteStringAsync(
        SiemensS7Master plc,
        string address,
        string value,
        IThingsGatewayBitConverter bitConverter,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        value ??= string.Empty;
        encoding ??= Encoding.ASCII;
        var dataBytes = encoding.GetBytes(value);

        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            var lengthResult = await plc.ReadAsync(address, 2, bitConverter, cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);
            if (!lengthResult.IsSuccess)
                return lengthResult;

            var span = lengthResult.Content.Span;
            var maxLength = span[0] == 0 ? (byte)0xFE : span[0];
            if (span[0] == byte.MaxValue)
                return new OperResult<string>(AppResource.NotString);
            if (dataBytes.Length > maxLength)
                return new OperResult<string>(AppResource.WriteDataLengthMore);

            return await plc.WriteAsync(
                address,
                ArrayHelper.SpliceArray(new byte[] { maxLength, (byte)dataBytes.Length }, dataBytes),
                DataTypeEnum.String, bitConverter, cancellationToken).ConfigureAwait(false);
        }

        return await plc.WriteAsync(
            address,
            ArrayHelper.SpliceArray(new byte[] { (byte)dataBytes.Length }, dataBytes),
            DataTypeEnum.String, bitConverter, cancellationToken).ConfigureAwait(false);
    }

    internal static async ValueTask<OperResult<string>> ReadWStringAsync(
        SiemensS7Master plc,
        string address,
        IThingsGatewayBitConverter bitConverter,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            encoding ??= Encoding.BigEndianUnicode;

            var headerResult = await plc.ReadAsync(address, 4, bitConverter, cancellationToken)
                                        .ConfigureAwait(false);
            if (!headerResult.IsSuccess)
                return new OperResult<string>(headerResult);

            var span = headerResult.Content.Span;
            if (span[0] == 0 || span[0] == byte.MaxValue)
                return new OperResult<string>(AppResource.NotString);

            var length = plc.BitConverter.ToUInt16(span, 2) * 2;
            var dataResult = await plc.ReadAsync(address, 4 + length, bitConverter, cancellationToken)
                                      .ConfigureAwait(false);
            if (!dataResult.IsSuccess)
                return new OperResult<string>(dataResult);

            return OperResult.CreateSuccessResult(
                encoding.GetString(dataResult.Content.Span.Slice(4, dataResult.Content.Length - 4)));
        }
        else
        {
            encoding ??= Encoding.Unicode;

            var lengthResult = await plc.ReadAsync(address, 1, bitConverter, cancellationToken)
                                        .ConfigureAwait(false);
            if (!lengthResult.IsSuccess)
                return new OperResult<string>(lengthResult);

            var length = lengthResult.Content.Span[0] * 2;
            var dataResult = await plc.ReadAsync(address, 1 + length, bitConverter, cancellationToken)
                                      .ConfigureAwait(false);
            if (!dataResult.IsSuccess)
                return new OperResult<string>(dataResult);

            return OperResult.CreateSuccessResult(
                encoding.GetString(dataResult.Content.Span.Slice(1, dataResult.Content.Length - 1)));
        }
    }

    internal static async ValueTask<OperResult> WriteWStringAsync(
        SiemensS7Master plc,
        string address,
        string value,
        IThingsGatewayBitConverter bitConverter,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        value ??= string.Empty;

        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            encoding ??= Encoding.BigEndianUnicode;
            var dataBytes = encoding.GetBytes(value).ArrayExpandToLengthEven();

            var headerResult = await plc.ReadAsync(address, 4, bitConverter, cancellationToken)
                                        .ConfigureAwait(false);
            if (!headerResult.IsSuccess)
                return headerResult;

            var maxLength = plc.BitConverter.ToUInt16(headerResult.Content.Span, 0);
            if (maxLength == 0) maxLength = 0x3FFE;

            if (dataBytes.Length > maxLength * 2)
                return new OperResult<string>(AppResource.WriteDataLengthMore);

            var writeBytes = ArrayHelper.SpliceArray(
                plc.BitConverter.GetBytes(maxLength),
                plc.BitConverter.GetBytes((ushort)(dataBytes.Length / 2)),
                dataBytes);

            return await plc.WriteAsync(address, writeBytes, DataTypeEnum.String, bitConverter, cancellationToken)
                            .ConfigureAwait(false);
        }

        encoding ??= Encoding.Unicode;
        var s200Bytes = encoding.GetBytes(value).ArrayExpandToLengthEven();
        var writeArray = ArrayHelper.SpliceArray(new byte[] { (byte)(s200Bytes.Length / 2) }, s200Bytes);

        return await plc.WriteAsync(address, writeArray, DataTypeEnum.String, bitConverter, cancellationToken)
                        .ConfigureAwait(false);
    }

}
