using BenchmarkDotNet.Running;

namespace ThingsGateway.Foundation.Benchmark
{
    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}