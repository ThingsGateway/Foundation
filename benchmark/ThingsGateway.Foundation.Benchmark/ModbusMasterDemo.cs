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

using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;
using TouchSocket.Modbus;

namespace ThingsGateway.Foundation.Benchmark;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[SimpleJob(RuntimeMoniker.NativeAot80)]
#if NET10_0
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.NativeAot10_0)]
#endif
[MemoryDiagnoser]
public class ModbusBenchmark : IDisposable
{
    public static int ClientCount = 10;
    public static int TaskNumberOfItems = 1;
    public static int NumberOfItems = 100;

    private List<ModbusMaster> tgModbusMasters = new();
    private List<NModbus.IModbusMaster> nModbusMasters = new();
    private List<ModbusTcpMaster> tsModbusMasters = new();

    [GlobalSetup]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:丢失范围之前释放对象", Justification = "<挂起>")]
    public async Task Init()
    {
        await InitTG().ConfigureAwait(false);
        await InitN().ConfigureAwait(false);

        await InitTS().ConfigureAwait(false);

        async Task InitTG()
        {
            for (int i = 0; i < ClientCount; i++)
            {

                var clientConfig = new TouchSocket.Core.TouchSocketConfig();

                var thingsgatewaymodbus = new ModbusMaster()
                {
                    //modbus协议格式
                    ModbusType = ModbusTypeEnum.ModbusTcp,
                };
                var clientChannel = thingsgatewaymodbus.CreateChannel(clientConfig, new ChannelOptions() { ChannelType = ChannelTypeEnum.TcpClient, RemoteUrl = "127.0.0.1:502", MaxConcurrentCount = 10 });
                thingsgatewaymodbus.InitChannel(clientChannel);
                await clientChannel.SetupAsync(clientChannel.Config).ConfigureAwait(false);
                clientChannel.Logger.LogLevel = LogLevel.Warning;
                await thingsgatewaymodbus.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
                await thingsgatewaymodbus.ReadByteAsync(new ModbusAddress() { FunctionCode = 3, StartAddress = 0, Length = 100 }).ConfigureAwait(false);
                tgModbusMasters.Add(thingsgatewaymodbus);
            }
        }

        async Task InitN()
        {
            for (int i = 0; i < ClientCount; i++)
            {

                var factory = new NModbus.ModbusFactory();
                var nmodbus = factory.CreateMaster(new System.Net.Sockets.TcpClient("127.0.0.1", 502));
                await nmodbus.ReadHoldingRegistersAsync(1, 0, 100).ConfigureAwait(false);
                nModbusMasters.Add(nmodbus);
            }
        }

        async Task InitTS()
        {
            for (int i = 0; i < ClientCount; i++)
            {
                var client = new ModbusTcpMaster();
                await client.SetupAsync(new TouchSocketConfig()
              .SetRemoteIPHost("127.0.0.1:502")).ConfigureAwait(false);
                await client.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
                await client.ReadHoldingRegistersAsync(0, 100).ConfigureAwait(false);
                tsModbusMasters.Add(client);
            }
        }
    }



    [Benchmark]
    public async Task ModbusOfThingsGateway()
    {
        ModbusAddress addr = new ModbusAddress() { FunctionCode = 3, StartAddress = 0, Length = 100 };
        List<Task> tasks = new List<Task>();
        foreach (var thingsgatewaymodbus in tgModbusMasters)
        {

            for (int i = 0; i < TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < NumberOfItems; i++)
                    {
                        var result = await thingsgatewaymodbus.ReadByteAsync(addr).ConfigureAwait(false);
                        if (!result.IsSuccess)
                        {
                            throw new Exception(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff zz") + result.ToString());
                        }
                        var data = thingsgatewaymodbus.BitConverter.ToUInt16(result.Content.Span, 0);
                    }
                }));
            }
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }


    [Benchmark]
    public async Task ModbusOfTouchSocket()
    {
        List<Task> tasks = new List<Task>();
        foreach (var modbusTcpMaster in tsModbusMasters)
        {
            for (int i = 0; i < TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < NumberOfItems; i++)
                    {
                        var result = await modbusTcpMaster.ReadHoldingRegistersAsync(0, 100).ConfigureAwait(false);
                        var data = TouchSocketBitConverter.ConvertValues<byte, ushort>(result.Data.Span, EndianType.Little);
                        if (!result.IsSuccess)
                        {
                            throw new Exception(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff zz") + result.ToString());
                        }
                    }
                }));
            }
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }


    [Benchmark]
    public async Task ModbusOfNModbus4()
    {
        List<Task> tasks = new List<Task>();
        foreach (var nmodbus in nModbusMasters)
        {
            for (int i = 0; i < TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < NumberOfItems; i++)
                    {
                        var result = await nmodbus.ReadHoldingRegistersAsync(1, 0, 100).ConfigureAwait(false);
                    }
                }));
            }
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }


    public void Dispose()
    {
        tgModbusMasters?.ForEach(a => a.Channel.SafeDispose());
        tgModbusMasters?.ForEach(a => a.SafeDispose());
        nModbusMasters?.ForEach(a => a.SafeDispose());
        tsModbusMasters?.ForEach(a => a.SafeDispose());
    }

}