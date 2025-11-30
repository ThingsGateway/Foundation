// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Common.Extension;
using ThingsGateway.Foundation.Common.StringExtension;
using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Test;

[TestClass]
public class ModbusTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("400045", true, "00020000002F01032C0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [DataRow("300045", true, "00020000002F01042C0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [DataRow("100045", true, "000200000009010206000000000000")]
    [DataRow("000045", true, "000200000009010106000000000000")]
    [DataRow("400045", false, "0002000000060106002C0001", "1", DataTypeEnum.UInt16)]
    [DataRow("000045", false, "0002000000060105002CFF00", "true", DataTypeEnum.Boolean)]
    [DataRow("400045;w=16", false, "0002000000090110002C0001020001", "1", DataTypeEnum.UInt16)]
    [DataRow("000045;w=15", false, "000200000008010F002C00010101", "true", DataTypeEnum.Boolean)]
    public async Task ModbusTcp_ReadWrite_OK(string address, bool read, string data, string writeData = null, DataTypeEnum dataTypeEnum = DataTypeEnum.UInt16)
    {
        var modbusMaster = new ModbusMaster() { ModbusType = ModbusTypeEnum.ModbusTcp, Timeout = 10000 };
        var modbusChannel = modbusMaster.CreateChannel(new TouchSocketConfig(), new ChannelOptions() { ChannelType = ChannelTypeEnum.Other }) as IClientChannel;

        modbusChannel.Config.ConfigureContainer(a =>
        {
            a.AddEasyLogger((a, b, c, d) =>
            {
                TestContext.WriteLine($"{c}{Environment.NewLine}{d?.ToString()}");
            }, LogLevel.Trace);
        });
        modbusMaster.InitChannel(modbusChannel);
        await modbusChannel.SetupAsync(modbusChannel.Config).ConfigureAwait(false);
        await modbusMaster.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
        var adapter = modbusChannel.ReadOnlyDataHandlingAdapter as SingleStreamDataHandlingAdapter;

        var task1 = Task.Run(async () =>
         {
             if (read)
             {
                 var result = await modbusMaster.ReadByteAsync(address).ConfigureAwait(false);
                 Assert.IsTrue(result.IsSuccess, result.ToString());
             }
             else
             {
                 var result = await modbusMaster.WriteJsonNodeAsync(address, JsonUtil.GetJsonNodeFromString(writeData), dataTypeEnum).ConfigureAwait(false);
                 Assert.IsTrue(result.IsSuccess, result.ToString());
             }
         });
        await Task.Delay(50).ConfigureAwait(false);
        var task2 = Task.Run(async () =>
        {
            SingleStreamDataHandlingAdapterTest singleStreamDataHandlingAdapterTest = new();
            await singleStreamDataHandlingAdapterTest.SendCallback(data.HexStringToBytes(), (a) => singleStreamDataHandlingAdapterTest.ReceivedAsync(adapter, CancellationToken.None), 1, CancellationToken.None).ConfigureAwait(false);
        });
        await Task.WhenAll(task1, task2).ConfigureAwait(false);
    }

    [TestMethod]
    [DataRow("400045", true, "01032C00000000000000000000000000000000000000000000000000000000000000000000000000000000000000007859")]
    [DataRow("300045", true, "01042C00000000000000000000000000000000000000000000000000000000000000000000000000000000000000008ADE")]
    [DataRow("100045", true, "010206000000000000E0B9")]
    [DataRow("000045", true, "010106000000000000A0AC")]
    [DataRow("400045", false, "0106002C000189C3", "1", DataTypeEnum.UInt16)]
    [DataRow("000045", false, "0105002CFF004DF3", "true", DataTypeEnum.Boolean)]
    public async Task ModbusRtu_ReadWrite_OK(string address, bool read, string data, string writeData = null, DataTypeEnum dataTypeEnum = DataTypeEnum.UInt16)
    {
        var modbusMaster = new ModbusMaster() { ModbusType = ModbusTypeEnum.ModbusRtu, Timeout = 10000, Station = 1 };
        var modbusChannel = modbusMaster.CreateChannel(new TouchSocketConfig(), new ChannelOptions() { ChannelType = ChannelTypeEnum.Other }) as IClientChannel;

        modbusChannel.Config.ConfigureContainer(a =>
        {
            a.AddEasyLogger((a, b, c, d) =>
           {
               TestContext.WriteLine($"{c}{Environment.NewLine}{d?.ToString()}");
           }, LogLevel.Trace);
        });
        modbusMaster.InitChannel(modbusChannel);
        await modbusChannel.SetupAsync(modbusChannel.Config).ConfigureAwait(false);
        await modbusMaster.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
        var adapter = modbusChannel.ReadOnlyDataHandlingAdapter as SingleStreamDataHandlingAdapter;

        var task1 = Task.Run(async () =>
        {
            if (read)
            {
                var result = await modbusMaster.ReadByteAsync(address).ConfigureAwait(false);
                Assert.IsTrue(result.IsSuccess, result.ToString());
            }
            else
            {
                var result = await modbusMaster.WriteJsonNodeAsync(address, JsonUtil.GetJsonNodeFromString(writeData), dataTypeEnum).ConfigureAwait(false);
                Assert.IsTrue(result.IsSuccess, result.ToString());
            }
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
