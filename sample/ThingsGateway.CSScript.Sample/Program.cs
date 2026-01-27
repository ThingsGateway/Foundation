using ThingsGateway.Gateway.Application.Extensions;

namespace ThingsGateway.Foundation.Sample
{
    public interface Test
    {
        public string Add(int num1, int num2);
        public string Multiply(int num1, int num2);

    }
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {
            Console.ReadLine();
            for (int i = 0; i < 11; i++)
            {
                var data = $"{i}".GetExpressionsResult(1);
                Console.WriteLine(data);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Console.ReadLine();
            }
            Console.ReadLine();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.ReadLine();
        }

    }
}
