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
using ThingsGateway.Foundation.Common.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Demo;

/// <inheritdoc/>
public partial class DemoMaster : DeviceBase
{
    public DemoMaster()
    {
        BitConverter.EndianType = EndianType.Big;
    }
    public override void InitChannel(IChannel channel, ILog? deviceLog = null)
    {
        base.InitChannel(channel, deviceLog);

        RegisterByteLength = 2;
    }

    /// <summary>
    /// 站号
    /// </summary>
    public byte Station { get; set; } = 1;


    /// <inheritdoc/>
    public override DataHandlingAdapter GetDataAdapter()
    {
        ArgumentNullExceptionEx.ThrowIfNull(Channel?.ChannelOptions, nameof(IChannel.ChannelOptions));
        
                switch (Channel.ChannelType)
                {
                    case ChannelTypeEnum.TcpClient:
                    case ChannelTypeEnum.TcpService:
                    case ChannelTypeEnum.SerialPort:
                        return new DeviceSingleStreamDataHandleAdapter<DemoMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                            IsSingleThread = false
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new DeviceUdpDataHandleAdapter<DemoMessage>()
                        {
                            IsSingleThread = false
                        };
                }
                return new DeviceSingleStreamDataHandleAdapter<DemoMessage>()
                {
                    CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                    IsSingleThread = false
                };

    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T, T2>(IEnumerable<T2> deviceVariables, int maxPack, string defaultIntervalTime)
    {
        //这部分需要自己理解，因为不同的协议地址解析不一样，这里只是一个简单的示例
        //但是整体思路是一样的，就是把地址相同的进行打包读取，理解好开始地址，长度，寄存器类型，要读取的数据类型长度等等

        return new List<T>();
    }

    public override bool BitReverse(IDeviceAddress address)
    {
        return false;
    }

    public override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(IDeviceAddress address, DataTypeEnum dataType, IThingsGatewayBitConverter bitConverter, CancellationToken cancellationToken = default)
    {
        if (address is DemoAddress mAddress)
        {
            var send = new DemoSend(Station, mAddress.StartAddress, (ushort)mAddress.Length);
            return SendThenReturnAsync(send, cancellationToken);
        }
        else
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(new ArgumentException("address must be of type ModbusAddress", nameof(address))));
        }
    }

    public override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(string address, int length, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            //解析自己的地址字符串，转换成起始地址等信息
            ushort startAddress = (ushort)address.ToInt();
            var send =new DemoSend(Station, startAddress, (ushort)length);
            return  SendThenReturnAsync(send, cancellationToken);
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(ex));
        }
    }

    public override ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<byte> value, DataTypeEnum dataType, IThingsGatewayBitConverter? bitConverter = null, CancellationToken cancellationToken = default)
    {
        //发送接受的方法和读取一样的，Send构建不同，演示例子就不重复写了
        throw new();
    }


}
