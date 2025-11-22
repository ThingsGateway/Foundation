//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Common;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Demo;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class DemoMessage : DeviceMessage, IResultMessage
{
    /// <inheritdoc/>
    public override long HeaderLength => 3;
    private byte station;
    ushort length;
    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        //假设演示协议的报文头格式为：[站号(1字节)][长度高字节][长度低字节][数据体][CRC低字节][CRC高字节]

        station = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
        length = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock);

        //报文头设置是3，读取完站号和长度后，数据区长度为length字节，外加2字节CRC，剩余的交给CheckBody处理
        BodyLength = length + 2; //数据区+crc
        return true;
    }


    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {

        OperCode = 0;
        Content = byteBlock.ToArrayTake(BodyLength - 2);//获取数据体

        //校验
        var pos = byteBlock.BytesRead - HeaderLength;
        var crcLen = HeaderLength + BodyLength - 2;
        var crc = CrcHelper.Crc16Only(byteBlock.TotalSequence.Slice(pos, crcLen));
        var checkCrc = byteBlock.TotalSequence.Slice(pos + crcLen, 2);
        if (checkCrc.SequenceEqual(crc))
        {
            return FilterResult.Success;
        }
        return FilterResult.GoOn;
    }


}
