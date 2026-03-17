using ThingsGateway.Foundation.Common.StringExtension;
using TouchSocket.Core;
using TouchSocket.Sockets;
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {
            List<TcpClient> tcpClients = new();
            for (int i = 0; i < 1000; i++)
            {
                TcpClient tcpClient = new();
                await tcpClient.SetupAsync(new TouchSocket.Core.TouchSocketConfig().SetRemoteIPHost("127.0.0.1:502")).ConfigureAwait(false);

                tcpClients.Add(tcpClient);
            }
            while (true)
            {


                foreach (var tcpClient in tcpClients)
                {
                    try
                    {
                        await tcpClient.TryConnectAsync();
                        await tcpClient.SendAsync("000100000006010300000064".HexStringToBytes());
                    }
                    catch (Exception)
                    {

                    }
                }
                await Task.Delay(10);
            }


            await SerialPortTest.Run().ConfigureAwait(false);


            //XTrace.LogLevel = Common.Log.LogLevel.All;
            //Console.WriteLine($"Aot:\"{Runtime.Aot}");
            //var mi = MachineInfo.GetCurrent();
            //var info = JsonSerializer.Serialize(mi, AotJsonContext.Default.MachineInfo);
            //Console.WriteLine(info);


            Console.ReadLine();

        }
    }
}
