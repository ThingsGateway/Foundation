//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace ThingsGateway.Foundation.Benchmark;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
#if NET10_0
[SimpleJob(RuntimeMoniker.Net10_0)]
#endif
[MemoryDiagnoser]
public class AsyncConcurrencyLimiterBenchmark
{
    private const int MaxConcurrency = 1;
    private const int Parallelism = 10000;
    private const int Iterations = 3;

    private SemaphoreSlim _semaphore = null!;
    private AsyncConcurrencyLimiter _limiter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _semaphore = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        _limiter = new AsyncConcurrencyLimiter(MaxConcurrency);
    }

    // ----------------------------
    // SemaphoreSlim
    // ----------------------------
    [Benchmark]
    public async Task SemaphoreSlim_WaitRelease()
    {
        var tasks = new Task[Parallelism];

        for (int i = 0; i < Parallelism; i++)
        {
            tasks[i] = WorkerSemaphore();
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task WorkerSemaphore()
    {
        for (int i = 0; i < Iterations; i++)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            _semaphore.Release();
        }
    }

    // ----------------------------
    // AsyncConcurrencyLimiter
    // ----------------------------
    [Benchmark]
    public async Task AsyncLimiter_WaitRelease()
    {
        var tasks = new Task[Parallelism];

        for (int i = 0; i < Parallelism; i++)
        {
            tasks[i] = WorkerLimiter();
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task WorkerLimiter()
    {
        for (int i = 0; i < Iterations; i++)
        {
            await _limiter.WaitAsync().ConfigureAwait(false);
            _limiter.Release();
        }
    }
}