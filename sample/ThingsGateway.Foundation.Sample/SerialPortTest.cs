using System.IO.Ports;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class SerialPortTest
    {
        internal static async Task Run()
        {


            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var channel = clientConfig.GetChannel(new ChannelOptions() { ChannelType = ChannelTypeEnum.SerialPort, PortName = "COM1", SerialPortReadBufferSize = 1024 * 100, SerialPortWriteBufferSize = 1024 * 100, StreamAsync = false });
            await channel.SetupAsync(channel.Config).ConfigureAwait(false);
            await channel.ConnectAsync().ConfigureAwait(false);
            channel.ChannelReceived.Add(ChannelReceived);


            _ = Task.Run(() =>
            {

                SerialPort serialPort = new();
                serialPort.PortName = "COM2";
                serialPort.Open();
                int i = 100;
                while (i-- > 0)
                {
                    serialPort.Write(new byte[150], 0, 150);
                    Thread.Sleep(10);
                }
            }
);

        }
        private static int index = 0;
        private static async ValueTask ChannelReceived(IClientChannel channel, ReceivedDataEventArgs args, bool arg3)
        {
            var data = args.Memory;
            await Task.Delay(50).ConfigureAwait(false);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + "接收" + (index++) + "数据" + data.Length);
        }
    }
}
