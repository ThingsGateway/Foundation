using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Dlt645;
using TouchSocket.Core;
using TouchSocket.Sockets;

#pragma warning disable CA2000 // 丢失范围之前释放对象
namespace ThingsGateway.Foundation.Sample
{
    internal class DltTest
    {

        /// <summary>
        /// 新建协议对象
        /// </summary>
        /// <returns></returns>
        public Dlt645_2007Master GetDevice()
        {
            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var client = new Dlt645_2007Master();
            var clientChannel = client.CreateChannel(clientConfig, new ChannelOptions() { ChannelType = ChannelTypeEnum.SerialPort, PortName = "COM1" });
            client.InitChannel(new(clientChannel));

            client.Station = "311111111114";//表号

            return client;
        }


        public async Task Test()
        {
            var dlt2007 = GetDevice();

            var data = await dlt2007.ReadDoubleAsync("02010100").ConfigureAwait(false); //读取A相电压
            Console.WriteLine(data.IsSuccess ? $"A相电压：{data}" : data.ToString());
        }


    }
}
