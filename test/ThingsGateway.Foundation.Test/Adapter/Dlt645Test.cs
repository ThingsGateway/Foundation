// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.Diagnostics;

using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Common.StringExtension;
using ThingsGateway.Foundation.Dlt645;

using TouchSocket.Core;


namespace ThingsGateway.Foundation.Test;

[TestClass]
public class Dlt645Test
{
    public TestContext TestContext { get; set; }


    [TestMethod]
    [DataRow("02010100", "FE FE FE FE 68 11 11 11 11 11 11 68 91 07 33 34 34 35 33 59 36 60 16 ")]
    public async Task Dlt645_Read_OK(string address, string data)
    {
        var dltMaster = new Dlt645_2007Master() { Timeout = 30000, Station = "111111111111" };
        var dltChannel = dltMaster.CreateChannel(new TouchSocketConfig(), new ChannelOptions() { ChannelType = ChannelTypeEnum.Other }) as IClientChannel;

        dltChannel.Config.ConfigureContainer(a =>
        {
            a.AddEasyLogger((a, b, c, d) =>
            {
                TestContext.WriteLine($"{c}{Environment.NewLine}{d?.ToString()}");
            }, LogLevel.Trace);
        });

        dltMaster.InitChannel(dltChannel);
        await dltChannel.SetupAsync(dltChannel.Config).ConfigureAwait(false);
        await dltMaster.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
        var adapter = dltChannel.ReadOnlyDataHandlingAdapter as SingleStreamDataHandlingAdapter;

        var task1 = Task.Run(async () =>
         {
             Stopwatch stopwatch = new Stopwatch();
             stopwatch.Start();
             var result = await dltMaster.ReadByteAsync(address).ConfigureAwait(false);
             stopwatch.Stop();
             Assert.IsTrue(result.IsSuccess, result.ToString());
         });
        await Task.Delay(50).ConfigureAwait(false);
        var task2 = Task.Run(async () =>
        {
            SingleStreamDataHandlingAdapterTest singleStreamDataHandlingAdapterTest = new();
            await singleStreamDataHandlingAdapterTest.SendCallback(data.HexStringToBytes(), (a) => singleStreamDataHandlingAdapterTest.ReceivedAsync(adapter, CancellationToken.None), 1, CancellationToken.None).ConfigureAwait(false);
        });

        await Task.WhenAll(task1, task2).ConfigureAwait(false);
    }

}
