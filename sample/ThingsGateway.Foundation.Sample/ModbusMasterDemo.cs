//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------


using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Sample;

internal sealed class ModbusMasterDemo : IDisposable
{
    public static int ClientCount = 10;
    public static int TaskNumberOfItems = 1;
    public static int NumberOfItems = 100;

    private List<ModbusMaster> thingsgatewaymodbuss = new();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:丢失范围之前释放对象", Justification = "<挂起>")]
    public async Task Init()
    {
        for (int i = 0; i < ClientCount; i++)
        {

            var clientConfig = new TouchSocket.Core.TouchSocketConfig();

            var clientChannel = clientConfig.GetChannel(new ChannelOptions() { ChannelType = ChannelTypeEnum.TcpClient, RemoteUrl = "127.0.0.1:502", MaxConcurrentCount = 10 });
            var thingsgatewaymodbus = new ModbusMaster()
            {
                //modbus协议格式
                ModbusType = ModbusTypeEnum.ModbusTcp,
            };
            thingsgatewaymodbus.InitChannel(clientChannel);
            await clientChannel.SetupAsync(clientChannel.Config).ConfigureAwait(false);
            clientChannel.Logger.LogLevel = LogLevel.Warning;
            await thingsgatewaymodbus.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
            await thingsgatewaymodbus.ReadByteAsync(new ModbusAddress() { FunctionCode = 3, StartAddress = 0, Length = 100 }).ConfigureAwait(false);
            thingsgatewaymodbuss.Add(thingsgatewaymodbus);
        }

    }

    public async Task ThingsGateway()
    {
        ModbusAddress addr = new ModbusAddress() { FunctionCode = 3, StartAddress = 0, Length = 100 };
        List<Task> tasks = new List<Task>();
        foreach (var thingsgatewaymodbus in thingsgatewaymodbuss)
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



    public void Dispose()
    {
        thingsgatewaymodbuss?.ForEach(a => a.Channel.SafeDispose());
        thingsgatewaymodbuss?.ForEach(a => a.SafeDispose());
    }

}