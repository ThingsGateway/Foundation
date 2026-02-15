using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Modbus;
#pragma warning disable CA2000 // 丢失范围之前释放对象

namespace ThingsGateway.Foundation.Sample
{
    internal class ModbusTest
    {

        /// <summary>
        /// 新建协议对象
        /// </summary>
        /// <returns></returns>
        public ModbusMaster GetDevice()
        {
            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var client = new ModbusMaster();
            var clientChannel = client.CreateChannel(clientConfig, new ChannelOptions() { ChannelType = ChannelTypeEnum.SerialPort, PortName = "COM1" });
            client.InitChannel(new(clientChannel));

            client.Station = 1;

            return client;
        }


        public async Task Test()
        {
            var modbus = GetDevice();
            {
                var data = await modbus.ReadAsync("0;S=1;F=3", 1).ConfigureAwait(false); //读取保持寄存器地址0，长度1的值
                if (data.IsSuccess)
                {
                    var uint16 = modbus.BitConverter.ToUInt16(data.Content.Span, 0);//自行解析
                }

                var int32Result = await modbus.ReadInt32Async("0;S=1;F=3", 1).ConfigureAwait(false); //读取保持寄存器地址0，长度2个modbus寄存器的值



            }
            {
                var data = await modbus.ReadAsync("0;S=1;F=3", 1).ConfigureAwait(false); //读取保持寄存器地址0，长度1的值
            }
        }


    }
}
