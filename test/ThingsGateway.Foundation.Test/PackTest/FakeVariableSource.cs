namespace ThingsGateway.Foundation.Modbus.Tests;

public class FakeVariableSource : IVariableSource<FakeVariable>
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    public string IntervalTime { get; set; }
    public string RegisterAddress { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    public int Length { get; set; }

    public List<FakeVariable> Vars = new();
    public ICollection<FakeVariable> Variables => Vars;

    public IDeviceAddress? DeviceAddress { get; set; }
    public string? LastErrorMessage { get; set; }

    public void AddVariable(FakeVariable variable) => Vars.Add(variable);

    public void AddVariableRange(IEnumerable<FakeVariable> variables)
    {
        Vars.AddRange(variables);
    }
}
