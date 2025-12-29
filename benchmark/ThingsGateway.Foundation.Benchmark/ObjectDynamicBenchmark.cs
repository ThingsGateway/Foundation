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
public class ObjectDynamicBenchmark
{


    [Benchmark]
    public void ObjectConvert()
    {
        for (int i = 0; i < 1000; i++)
        {
            object obj = 12345;
            var data = GetObjectData(obj);

        }
    }
    [Benchmark]
    public void DynamicConvert()
    {
        for (int i = 0; i < 1000; i++)
        {
            var obj = 12345;
            var data = GetDynamicData(obj);
        }
    }

    private object GetObjectData(object obj)
    {
        return ((int)obj) * 123;
    }
    private dynamic GetDynamicData(dynamic obj)
    {
        return obj * 123;
    }
}