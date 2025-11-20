//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsGateway.Foundation.Common.Json.Extension;

/// <summary>
/// 将 byte[] 序列化为数值数组，反序列化数值数组为 byte[]
/// </summary>
public class SystemTextJsonByteArrayToNumberArrayConverter : JsonConverter<byte[]>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token.");
        }

        var bytes = new List<byte>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetByte(out byte value))
                {
                    bytes.Add(value);
                }
                else
                {
                    throw new JsonException("Invalid number value for byte array.");
                }
            }
            else
            {
                throw new JsonException($"Unexpected token {reader.TokenType} in byte array.");
            }
        }

        return bytes.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var b in value)
        {
            writer.WriteNumberValue(b);
        }
        writer.WriteEndArray();
    }
}
