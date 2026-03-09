using Xunit;

namespace ThingsGateway.Foundation.Modbus.Tests;

public class ModbusPackTests
{
    private static ModbusMaster Device => new ModbusMaster();

    [Fact]
    public void Pack_Int16_Sequential_ShouldPackInOneBlock()
    {
        var vars = new List<FakeVariable>
        {
            new() { RegisterAddress = "41001", DataType = DataTypeEnum.Int16 },
            new() { RegisterAddress = "41002", DataType = DataTypeEnum.Int16 },
            new() { RegisterAddress = "41003", DataType = DataTypeEnum.Int16 },
        };

        var result = PackHelper.LoadSourceRead<FakeVariableSource, FakeVariable>(
            Device, vars, 10, "1000");

        Assert.Single(result); // ✅ 必须合并成1包

        var pack = result[0];
        Assert.Equal("S=1;F=3;1000", pack.RegisterAddress); // ✅ 起始地址
        Assert.Equal(3, pack.Length); // ✅ 3个寄存器 * 2 byte = 6 byte

        var items = pack.Vars;
        Assert.Equal(0, items[0].Index); // 1001 偏移 0
        Assert.Equal(2, items[1].Index); // 1002 偏移 2 byte
        Assert.Equal(4, items[2].Index); // 1003 偏移 4 byte
    }

    [Fact]
    public void Pack_BitVariables_SameRegister_ShouldPackAndOffsetCorrectly()
    {
        var vars = new List<FakeVariable>
        {
            new() { RegisterAddress = "11001.0", DataType = DataTypeEnum.Boolean },
            new() { RegisterAddress = "11001.3", DataType = DataTypeEnum.Boolean },
            new() { RegisterAddress = "11001.7", DataType = DataTypeEnum.Boolean },
        };

        var result = PackHelper.LoadSourceRead<FakeVariableSource, FakeVariable>(
            Device, vars, 10, "1000");

        Assert.Single(result);
        var v = result[0].Vars;

        Assert.Equal(0, v[0].Index);
        Assert.Equal(3, v[1].Index);
        Assert.Equal(7, v[2].Index);
    }

    [Fact]
    public void Pack_Int16_WithGap_ShouldSplitOrIndexCorrect()
    {
        var vars = new List<FakeVariable>
        {
            new() { RegisterAddress = "41001", DataType = DataTypeEnum.Int16 },
            new() { RegisterAddress = "41005", DataType = DataTypeEnum.Int16 },
        };

        var result = PackHelper.LoadSourceRead<FakeVariableSource, FakeVariable>(
            Device, vars, 10, "1000");

        Assert.Single(result);

        var pack = result[0];
        Assert.Equal("S=1;F=3;1000", pack.RegisterAddress);
        Assert.Equal(5, pack.Length); // (1005 - 1001)=4寄存器 → 4*2 +2 = 10byte

        var v = pack.Vars;
        Assert.Equal(0, v[0].Index);  // 1001 偏移 0
        Assert.Equal(8, v[1].Index);  // 1005 偏移 4寄存器 = 8 byte
    }

    [Fact]
    public void Pack_MixedInt16AndBool_CorrectIndexing()
    {
        var vars = new List<FakeVariable>
        {
            new() { RegisterAddress = "31001", DataType = DataTypeEnum.Int16 },  // 2 byte
            new() { RegisterAddress = "31002.3", DataType = DataTypeEnum.Boolean }, // 第2寄存器 bit3
        };

        var result = PackHelper.LoadSourceRead<FakeVariableSource, FakeVariable>(Device, vars, 10, "1000");

        var pack = result[0];
        Assert.Equal("S=1;F=4;1000", pack.RegisterAddress);
        Assert.Equal(2, pack.Length); // 2寄存器 = 4 byte

        var v = pack.Vars;
        Assert.Equal(0, v[0].Index); // int16
        Assert.Equal(16 + 3, v[1].Index); // 第2个寄存器起始 bit=16 +3
    }

    [Fact]
    public void Pack_ByteIndex_SwapRule_ShouldFlip()
    {
        var vars = new List<FakeVariable>
        {
            new() { RegisterAddress = "41001", DataType = DataTypeEnum.Byte },
            new() { RegisterAddress = "41002", DataType = DataTypeEnum.Byte },
        };

        var result = PackHelper.LoadSourceRead<FakeVariableSource, FakeVariable>(Device, vars, 10, "1000");
        var v = result[0].Vars;

        // ✅ byte index 规则：偶数+1，奇数-1
        Assert.Equal(1, v[0].Index);
        Assert.Equal(3, v[1].Index); // 2nd byte 实际位置应为 2+(-1)=1 → 逻辑整体偏移 2 byte
    }

    [Fact]
    public void Pack_ArrayBoolean_ShouldIncreaseLengthByBits()
    {
        var vars = new List<FakeVariable>
        {
            new() { RegisterAddress = "31001", DataType = DataTypeEnum.Boolean, ArrayLength = 16 },
        };

        var result = PackHelper.LoadSourceRead<FakeVariableSource, FakeVariable>(Device, vars, 10, "1000");
        var pack = result[0];

        Assert.Equal(1, pack.Length);
        Assert.Equal(0, pack.Vars[0].Index);
    }

    [Fact]
    public void Pack_ShouldSeparateByFunctionCode()
    {
        var vars = new List<FakeVariable>
        {
            new() { RegisterAddress = "41001", DataType = DataTypeEnum.Int16 },
            new() { RegisterAddress = "31001", DataType = DataTypeEnum.Int16 },
        };

        var result = PackHelper.LoadSourceRead<FakeVariableSource, FakeVariable>(Device, vars, 10, "1000");
        Assert.Equal(2, result.Count); // ✅ 不同 FunctionCode 必须分包
    }

    [Fact]
    public void Pack_ComplexSequentialBlock_MustComputeExactOffsets()
    {
        var vars = new List<FakeVariable>
        {
            new() { RegisterAddress = "41001", DataType = DataTypeEnum.Int16 }, // 2 byte
            new() { RegisterAddress = "41002", DataType = DataTypeEnum.Int32 }, // 4 byte
            new() { RegisterAddress = "41004", DataType = DataTypeEnum.Byte  }, // 1 byte (flip)
            new() { RegisterAddress = "41005", DataType = DataTypeEnum.Boolean }, // bit in next reg
        };

        var result = PackHelper.LoadSourceRead<FakeVariableSource, FakeVariable>(Device, vars, 20, "1000");
        var pack = result[0];
        var v = pack.Vars;

        Assert.Equal("S=1;F=3;1000", pack.RegisterAddress);

        Assert.Equal(0, v[0].Index);   // 1001 int16
        Assert.Equal(2, v[1].Index);   // 1002 int32
        Assert.Equal(6 + 1, v[2].Index); // 1004 byte flip → 6+1
        Assert.Equal((4 * 16) + 0, v[3].Index); // 1005 bit0 = 4 reg offset = 64 bit
    }
}
