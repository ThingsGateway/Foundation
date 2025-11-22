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
/// 演示例子，报文构建格式
/// </summary>
public class DemoSend : ISendMessage
{
    public DemoSend(byte station, ushort address, ushort length)
    {
        this.station = station;
        this.address = address;
        this.length = length;
    }
    byte station;
    ushort address;
    ushort length;
    public int MaxLength => 300;
    public int Sign { get; set; }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IBytesWriter
    {
        //假设发送格式是 [站号][地址高字节][地址低字节][长度高字节][长度低字节][CRC低字节][CRC高字节]

        var span = byteBlock.GetSpan(512);
        WriterExtension.WriteValue(ref byteBlock, (byte)station);
        WriterExtension.WriteValue(ref byteBlock, (ushort)address, EndianType.Big);
        WriterExtension.WriteValue(ref byteBlock, (ushort)length, EndianType.Big);

        var crclen = byteBlock.WrittenCount;
        byteBlock.Write(CrcHelper.Crc16Only(span.Slice(0, (int)crclen)));
    }
}
