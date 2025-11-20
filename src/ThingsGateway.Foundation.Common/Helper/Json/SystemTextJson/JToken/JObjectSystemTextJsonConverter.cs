//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------
using Newtonsoft.Json.Linq;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsGateway.Foundation.Common.Json.Extension;

/// <summary>
/// System.Text.Json → JToken / JObject / JArray 转换器
/// </summary>
public class JObjectSystemTextJsonConverter : JsonConverter<JObject>
{
    public override JObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var obj = new JObject();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return obj;

            var propertyName = reader.GetString();
            reader.Read();
            var value = ReadJToken(ref reader);
            obj[propertyName!] = value;
        }
        throw new JsonException();
    }

    private static JToken ReadJToken(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                var obj = new JObject();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        return obj;

                    var propertyName = reader.GetString();
                    reader.Read();
                    var value = ReadJToken(ref reader);
                    obj[propertyName!] = value;
                }
                throw new JsonException();

            case JsonTokenType.StartArray:
                var array = new JArray();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        return array;

                    array.Add(ReadJToken(ref reader));
                }
                throw new JsonException();

            case JsonTokenType.String:
                if (reader.TryGetDateTime(out var date))
                    return new JValue(date);
                return new JValue(reader.GetString());

            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var l))
                    return new JValue(l);
                return new JValue(reader.GetDouble());

            case JsonTokenType.True:
                return new JValue(true);

            case JsonTokenType.False:
                return new JValue(false);

            case JsonTokenType.Null:
                return JValue.CreateNull();

            default:
                throw new JsonException($"Unsupported token type {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, JObject value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var prop in (JObject)value)
        {
            writer.WritePropertyName(prop.Key);
            Write(writer, prop.Value!, options);
        }
        writer.WriteEndObject();
    }

    private static void Write(Utf8JsonWriter writer, JToken value, JsonSerializerOptions options)
    {
        switch (value.Type)
        {
            case JTokenType.Object:
                writer.WriteStartObject();
                foreach (var prop in (JObject)value)
                {
                    writer.WritePropertyName(prop.Key);
                    Write(writer, prop.Value!, options);
                }
                writer.WriteEndObject();
                break;

            case JTokenType.Array:
                writer.WriteStartArray();
                foreach (var item in (JArray)value)
                {
                    Write(writer, item!, options);
                }
                writer.WriteEndArray();
                break;

            case JTokenType.Null:
                writer.WriteNullValue();
                break;

            case JTokenType.Boolean:
                writer.WriteBooleanValue(value.Value<bool>());
                break;

            case JTokenType.Integer:
                writer.WriteNumberValue(value.Value<long>());
                break;

            case JTokenType.Float:
                writer.WriteNumberValue(value.Value<double>());
                break;

            case JTokenType.String:
                writer.WriteStringValue(value.Value<string>());
                break;

            case JTokenType.Date:
                writer.WriteStringValue(value.Value<DateTime>());
                break;

            case JTokenType.Guid:
                writer.WriteStringValue(value.Value<Guid>().ToString());
                break;

            case JTokenType.Uri:
                writer.WriteStringValue(value.Value<Uri>()?.ToString());
                break;

            case JTokenType.TimeSpan:
                writer.WriteStringValue(value.Value<TimeSpan>().ToString());
                break;

            default:
                // fallback — 转字符串
                writer.WriteStringValue(value.ToString());
                break;
        }
    }
}
