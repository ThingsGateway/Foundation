using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsGateway.Foundation.Common.Serialization;

/// <summary>面向Type的Json序列化转换器</summary>
/// <remarks>借助字符串序列化Type.FullName</remarks>
public class SystemTextJsonTypeConverter : JsonConverter<Type>
{
    /// <summary>读取类型</summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "AOT环境下不调用")]
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString()?.GetTypeEx();

    /// <summary>写入类型</summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options) => writer.WriteStringValue(value.AssemblyQualifiedName);
}
