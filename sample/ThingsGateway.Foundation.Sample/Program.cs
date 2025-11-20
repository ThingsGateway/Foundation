using System.Text.Json;

using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Common.Log;

namespace ThingsGateway.Foundation.Sample
{
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {
            XTrace.LogLevel = Common.Log.LogLevel.All;
            Console.WriteLine($"Aot:\"{Runtime.Aot}");
            var mi = MachineInfo.GetCurrent();
            var info = JsonSerializer.Serialize(mi, AotJsonContext.Default.MachineInfo);
            Console.WriteLine(info);

            Console.ReadLine();
            using var master = new ModbusMasterDemo();
            await master.Init().ConfigureAwait(false);
            while (true)
            {
                await master.ThingsGateway().ConfigureAwait(false);
                Console.ReadLine();
            }

        }
    }
}
