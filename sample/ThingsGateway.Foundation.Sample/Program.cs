using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Modbus;

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {

            //XTrace.LogLevel = Common.Log.LogLevel.All;
            //Console.WriteLine($"Aot:\"{Runtime.Aot}");
            //var mi = MachineInfo.GetCurrent();
            //var info = JsonSerializer.Serialize(mi, AotJsonContext.Default.MachineInfo);
            //Console.WriteLine(info);


            Console.ReadLine();
            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var thingsgatewaymodbus = new ModbusMaster()
            {
                //modbus协议格式
                ModbusType = ModbusTypeEnum.ModbusRtu,
            };
            var clientChannel = thingsgatewaymodbus.CreateChannel(clientConfig, new ChannelOptions() { ChannelType = ChannelTypeEnum.SerialPort, PortName = "COM1" });
            thingsgatewaymodbus.InitChannel(new(clientChannel));
            await clientChannel.SetupAsync(clientChannel.Config).ConfigureAwait(false);
            clientChannel.Logger.LogLevel = TouchSocket.Core.LogLevel.Warning;
            await thingsgatewaymodbus.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
            await thingsgatewaymodbus.ReadByteAsync(new ModbusAddress() { FunctionCode = 3, StartAddress = 0, Length = 100 }).ConfigureAwait(false);

            while (true)
            {
                ValueStopwatch valueStopwatch = ValueStopwatch.StartNew();

                List<Task> tasks = new List<Task>(10000);

                for (int i = 0; i < 10000; i++)
                {
                    var task = Task.Run(async () =>
                    {
                        var result = await thingsgatewaymodbus.ReadAsync("400001", 1).ConfigureAwait(false);
                        var data = thingsgatewaymodbus.BitConverter.ToUInt16(result.Content.Span, 0);
                        await thingsgatewaymodbus.WriteAsync("400001", (ushort)(i)).ConfigureAwait(false);
                    });
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks).ConfigureAwait(false);

                TimeSpan elapsedTime = valueStopwatch.GetElapsedTime();
                Console.WriteLine($"1W 次读写通讯耗时：{elapsedTime}");
                Console.ReadLine();
            }

        }
    }
}
