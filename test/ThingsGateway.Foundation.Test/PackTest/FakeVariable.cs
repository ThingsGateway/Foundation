using ThingsGateway.Foundation.Common;
namespace ThingsGateway.Foundation.Modbus.Tests;

public class FakeVariable : IVariable
{
    public DataTypeEnum DataType { get; set; }
    public int Index { get; set; }
    public string? IntervalTime { get; set; }
    public required string RegisterAddress { get; set; }
    public int? ArrayLength { get; set; }
    public IThingsGatewayBitConverter? BitConverter { get; set; }
    public IVariableSource? VariableSource { get; set; }


    public object? Value { get; set; }

    public object? RawValue { get; set; }

    public OperResult SetValue(object? value, DateTime dateTime, bool isOnline = true)
    {
        return OperResult.Success;
    }

    public void TimeChanged(DateTime dateTime)
    {
    }
}
