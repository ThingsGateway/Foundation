using System.IO.Ports;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class SerialPortTest
    {
        internal static void Run()
        {
            _ = Task.Run(() =>
            {

                SerialPort serialPort = new();
                serialPort.PortName = "COM2";
                serialPort.Open();
                while (true)
                {
                    serialPort.Write(new byte[150], 0, 150);
                    Thread.Sleep(10);
                }
            }
            );
            _ = Task.Run(async () =>
            {
                var clientConfig = new TouchSocket.Core.TouchSocketConfig();
                var channel = clientConfig.GetChannel(new ChannelOptions() { ChannelType = ChannelTypeEnum.SerialPort, PortName = "COM1" });
                await channel.SetupAsync(channel.Config);
                await channel.ConnectAsync();
                channel.ChannelReceived.Add(ChannelReceived);

            });
            while (true)
            {
            }
        }
        private static int index = 0;
        private static async ValueTask ChannelReceived(IClientChannel channel, ReceivedDataEventArgs args, bool arg3)
        {
            var data = args.Memory;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + "接收数据");
        }
    }
}
