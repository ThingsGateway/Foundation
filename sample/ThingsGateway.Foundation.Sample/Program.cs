#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {


            SerialPortReadTest_M.Run();
            await SerialPortSendTest.Run().ConfigureAwait(false);

            Console.ReadLine();

            //XTrace.LogLevel = Common.Log.LogLevel.All;
            //Console.WriteLine($"Aot:\"{Runtime.Aot}");
            //var mi = MachineInfo.GetCurrent();
            //var info = JsonSerializer.Serialize(mi, AotJsonContext.Default.MachineInfo);
            //Console.WriteLine(info);


            Console.ReadLine();

        }
    }
}
