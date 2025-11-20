using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

using ThingsGateway.Foundation.Common.Data;
using ThingsGateway.Foundation.Common.Json.Extension;

using JsonArray = System.Text.Json.Nodes.JsonArray;

namespace ThingsGateway.Foundation.Common.Extension;

public static class JsonUtil
{
    /// <summary>目标匿名参数对象转为名值字典</summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IDictionary<String, Object?> ToDictionary(this Object source)
    {
        //!! 即使传入为空，也返回字典，而不是null，避免业务层需要大量判空
        //if (target == null) return null;
#pragma warning disable CA1859 // 尽可能使用具体类型以提高性能
        if (source is IDictionary<String, Object?> dic) return dic;
#pragma warning restore CA1859 // 尽可能使用具体类型以提高性能
        var type = source?.GetType();
        if (type?.IsBaseType() == true)
            throw new InvalidDataException("source is not Object");

        dic = new NullableDictionary<String, Object?>(StringComparer.OrdinalIgnoreCase);
        if (source != null)
        {
            // 修正字符串字典的支持问题
            if (source is IDictionary dic2)
            {
                foreach (var item in dic2)
                {
                    if (item is DictionaryEntry de)
                        dic[de.Key + ""] = de.Value;
                }
            }

            else if (source is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in element.EnumerateObject())
                {
                    Object? v = item.Value.ValueKind switch
                    {
                        JsonValueKind.Object => item.Value.ToDictionary(),
                        JsonValueKind.Array => ToArray(item.Value),
                        JsonValueKind.String => item.Value.GetString(),
                        JsonValueKind.Number when item.Value.GetRawText().Contains('.') => item.Value.GetDouble(),
                        JsonValueKind.Number => item.Value.GetInt64(),
                        JsonValueKind.True or JsonValueKind.False => item.Value.GetBoolean(),
                        _ => item.Value.GetString(),
                    };
                    if (v is Int64 n && n < Int32.MaxValue) v = (Int32)n;
                    dic[item.Name] = v;
                }
            }
            else
            {
                foreach (var pi in source.GetType().GetPropertiesEx(true))
                {
                    var name = Serialization.SerialHelper.GetName(pi);
                    dic[name] = source.GetValueEx(pi);
                }

                // 增加扩展属性
                if (source is IExtend ext && ext.Items != null)
                {
                    foreach (var item in ext.Items)
                    {
                        dic[item.Key] = item.Value;
                    }
                }
            }
        }

        return dic;
    }


    /// <summary>Json对象转为数组</summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static IList<Object?> ToArray(this JsonElement element)
    {
        var list = new List<Object?>();
        foreach (var item in element.EnumerateArray())
        {
            Object? v = item.ValueKind switch
            {
                JsonValueKind.Object => item.ToDictionary(),
                JsonValueKind.Array => ToArray(item),
                JsonValueKind.String => item.GetString(),
                JsonValueKind.Number when item.GetRawText().Contains('.') => item.GetDouble(),
                JsonValueKind.Number => item.GetInt64(),
                JsonValueKind.True or JsonValueKind.False => item.GetBoolean(),
                _ => item.GetString(),
            };
            if (v is Int64 n && n < Int32.MaxValue) v = (Int32)n;
            list.Add(v);
        }

        return list;
    }


    /// <summary>
    /// 将 System.Text.Json.JsonElement 递归转换为 Newtonsoft.Json.Linq.JToken
    /// - tryParseDates: 是否尝试把字符串解析为 DateTime/DateTimeOffset
    /// - tryParseGuids: 是否尝试把字符串解析为 Guid
    /// </summary>
    public static JToken ToJToken(this JsonElement element, bool tryParseDates = true, bool tryParseGuids = true)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new JObject();
                foreach (var prop in element.EnumerateObject())
                    obj.Add(prop.Name, prop.Value.ToJToken(tryParseDates, tryParseGuids));
                return obj;

            case JsonValueKind.Array:
                var arr = new JArray();
                foreach (var item in element.EnumerateArray())
                    arr.Add(item.ToJToken(tryParseDates, tryParseGuids));
                return arr;

            case JsonValueKind.String:
                // 优先按语义尝试解析 Guid / DateTimeOffset / DateTime
                if (tryParseGuids && element.TryGetGuid(out Guid g))
                    return new JValue(g);

                if (tryParseDates && element.TryGetDateTimeOffset(out DateTimeOffset dto))
                    return new JValue(dto);

                if (tryParseDates && element.TryGetDateTime(out DateTime dt))
                    return new JValue(dt);

                return new JValue(element.GetString());

            case JsonValueKind.Number:
                return NumberElementToJToken(element);

            case JsonValueKind.True:
                return new JValue(true);

            case JsonValueKind.False:
                return new JValue(false);

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                return JValue.CreateNull();
        }
    }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    private static JToken NumberElementToJToken(JsonElement element)
    {
        // 取原始文本（保持原始表示，方便处理超出标准类型范围的数字）
        string raw = element.GetRawText(); // 例如 "123", "1.23e4"

        // 如果不含小数点或指数，优先尝试整数解析（long / ulong / BigInteger）
        if (!raw.Contains('.') && !raw.Contains('e') && !raw.Contains('E'))
        {
            if (long.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var l))
                return new JValue(l);

            if (ulong.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var ul))
                return new JValue(ul);

            if (BigInteger.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var bi))
                // BigInteger 可能不被 JValue 直接识别为数字类型，使用 FromObject 保证正确表示
                return JToken.FromObject(bi);
        }

        // 含小数或指数，或整数解析失败，尝试 decimal -> double
        if (decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
            return new JValue(dec);

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            return new JValue(d);

        // 最后兜底：把原始文本当字符串返回（极端情况）
        return new JValue(raw);
    }

    /// <summary>
    /// 把 JToken 转成“平面”字符串，适合用于日志或写入 CSV 的单元格：
    /// - string -> 原文
    /// - bool -> "1"/"0"
    /// - number -> 原始数字文本
    /// - date -> ISO 8601 (o)
    /// - object/array -> 紧凑的 JSON 文本
    /// - null/undefined -> empty string
    /// </summary>
    public static string JTokenToPlainString(this JToken token)
    {
        if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            return string.Empty;

        switch (token.Type)
        {
            case JTokenType.String:
                return token.Value<string>() ?? string.Empty;

            case JTokenType.Boolean:
                return token.Value<bool>() ? "1" : "0";

            case JTokenType.Integer:
            case JTokenType.Float:
                // 保持紧凑数字文本（不加引号）
                return token.ToString(Formatting.None);

            case JTokenType.Date:
                {
                    // Date 类型可能是 DateTime 或 DateTimeOffset
                    var val = token.Value<object>();
                    if (val is DateTimeOffset dto) return dto.ToString("o");
                    if (val is DateTime dt) return dt.ToString("o");
                    return token.ToString(Formatting.None);
                }

            default:
                // 对象/数组等，返回紧凑 JSON 表示
                return token.ToString(Formatting.None);
        }
    }

    /// <summary>
    /// 根据字符串解析对应 JsonNode。
    /// 字符串可以不包含转义双引号，如果解析失败会直接转成 JsonValue。
    /// true/false 可忽略大小写。
    /// </summary>
    public static JsonNode? GetJsonNodeFromString(this string? item)
    {
        if (string.IsNullOrEmpty(item))
            return null;

        try
        {
            if (bool.TryParse(item, out bool b))
                return JsonValue.Create(b);

            // 尝试当作 JSON 解析
            return JsonNode.Parse(item);
        }
        catch
        {
            // 失败则作为普通字符串处理
            return JsonValue.Create(item);
        }
    }


    /// <summary>
    /// JsonNode → CLR 对象（Dictionary / Array / 基础类型）
    /// </summary>
    public static object? GetObjectFromJsonNode(this JsonNode? node)
    {
        if (node == null)
            return null;

        switch (node)
        {
            case JsonValue value:
                return value.GetValue<object?>();

            case JsonObject obj:
                return obj.ToDictionary(
                    kv => kv.Key,
                    kv => GetObjectFromJsonNode(kv.Value)
                );

            case System.Text.Json.Nodes.JsonArray array:
                return ConvertArray(array);

            default:
                return node.ToJsonString();
        }
    }

    private static object? ConvertArray(System.Text.Json.Nodes.JsonArray array)
    {
        // 类型判断
        if (array.All(x => x is JsonValue v && v.TryGetValue<long>(out _)))
            return array.Select(x => x!.GetValue<long>()).ToArray();

        if (array.All(x => x is JsonValue v && v.TryGetValue<double>(out _)))
            return array.Select(x => x!.GetValue<double>()).ToArray();

        if (array.All(x => x is JsonValue v && v.TryGetValue<string>(out _)))
            return array.Select(x => x!.GetValue<string>()).ToArray();

        if (array.All(x => x is JsonValue v && v.TryGetValue<bool>(out _)))
            return array.Select(x => x!.GetValue<bool>()).ToArray();

        if (array.All(x => x is JsonValue v && v.TryGetValue<DateTime>(out _)))
            return array.Select(x => x!.GetValue<DateTime>()).ToArray();

        if (array.All(x => x is JsonValue v && v.TryGetValue<Guid>(out _)))
            return array.Select(x => x!.GetValue<Guid>()).ToArray();

        // 递归
        return array.Select(x => GetObjectFromJsonNode(x)).ToArray();
    }



    /// <summary>
    /// 任意对象 → JsonNode
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static JsonNode? GetJsonNodeFromObj(object? value)
    {
        if (value == null)
            return null;

        switch (value)
        {
            case JsonNode node:
                return node;

            case bool b:
            case string s:
            case int i:
            case long l:
            case double d:
            case float f:
            case decimal m:
            case DateTime dt:
            case DateTimeOffset dto:
            case Guid g:
                return JsonValue.Create(value);

            default:
                return System.Text.Json.JsonSerializer.SerializeToNode(value, SystemTextJsonExtension.SystemTextJsonService.NoneIndentedOptions);
        }
    }


    /// <summary>
    /// 转string，对象为null返回空字符串，string返回字符串，其他对象使用System.Text.Json序列化
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static string GetStringFromObj(object? value, bool parseBoolNumber = false)
    {
        if (value == null)
            return string.Empty;

        switch (value)
        {
            case string node:
                return node;
            case bool boolValue:
                return boolValue ? parseBoolNumber ? "1" : "True" : parseBoolNumber ? "0" : "False";

            case JsonElement elem: // System.Text.Json.JsonElement
                return elem.ValueKind switch
                {
                    JsonValueKind.String => elem.GetString(),
                    JsonValueKind.Number => elem.GetRawText(),  // 或 elem.GetDecimal().ToString()
                    JsonValueKind.True => "1",
                    JsonValueKind.False => "0",
                    JsonValueKind.Null => string.Empty,
                    _ => elem.GetRawText(), // 对象、数组等直接输出 JSON
                };
            case JToken jToken:
                return jToken.ToString();

            default:
                return value.ToSystemTextJsonString();
        }
    }


    #region Rank

    /// <summary>
    /// 获取数组维度（类似 JToken 版本的 CalculateActualValueRank）
    /// </summary>
    public static int CalculateActualValueRank(this JsonNode node)
    {
        if (node is not JsonArray arr)
            return -1;

        int rank = 1;

        JsonArray? current = arr;
        while (IsArrayOfArrays(current))
        {
            current = current!.First()!.AsArray();
            rank++;
        }
        return rank;
    }

    private static bool IsArrayOfArrays(JsonArray array)
    {
        return array.Count > 0 && array[0] is JsonArray;
    }

    #endregion




    /// <summary>
    /// 根据字符串解析对应JToken<br></br>
    /// 字符串可以不包含转义双引号，如果解析失败会直接转成String类型的JValue
    /// true/false可忽略大小写
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static JToken GetJTokenFromString(this string item)
    {
        try
        {
            if (item.IsNullOrEmpty())
                return JValue.CreateNull();

            if (bool.TryParse(item, out bool parseBool))
                return new JValue(parseBool);

            // 尝试解析字符串为 JToken 对象
            return JToken.Parse(item);
        }
        catch
        {
            // 解析失败时，将其转为 String 类型的 JValue
            return new JValue(item);
        }
    }

    /// <summary>
    /// 根据JToken获取Object类型值<br></br>
    /// 对应返回 对象字典 或 类型数组 或 类型值
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static object? GetObjectFromJToken(this JToken token)
    {
        if (token == null)
            return null;

        switch (token.Type)
        {
            case JTokenType.Object:
                var obj = new Dictionary<string, object>();
                foreach (var prop in (JObject)token)
                    obj[prop.Key] = GetObjectFromJToken(prop.Value);
                return obj;

            case JTokenType.Array:
                var array = (JArray)token;

                if (array.All(x => x.Type == JTokenType.Integer))
                    return array.Select(x => x.Value<long>()).ToArray();

                if (array.All(x => x.Type == JTokenType.Float))
                    return array.Select(x => x.Value<double>()).ToArray();

                if (array.All(x => x.Type == JTokenType.String))
                    return array.Select(x => x.Value<string>()).ToArray();

                if (array.All(x => x.Type == JTokenType.Boolean))
                    return array.Select(x => x.Value<bool>()).ToArray();

                if (array.All(x => x.Type == JTokenType.Date))
                    return array.Select(x => x.Value<DateTime>()).ToArray();

                if (array.All(x => x.Type == JTokenType.TimeSpan))
                    return array.Select(x => x.Value<TimeSpan>()).ToArray();

                if (array.All(x => x.Type == JTokenType.Guid))
                    return array.Select(x => x.Value<Guid>()).ToArray();

                if (array.All(x => x.Type == JTokenType.Uri))
                    return array.Select(x => x.Value<Uri>()).ToArray();

                // 否则递归
                return array.Select(x => GetObjectFromJToken(x)).ToArray();

            case JTokenType.Integer:
                return token.ToObject<long>();

            case JTokenType.Float:
                return token.ToObject<double>();

            case JTokenType.String:
                return token.ToObject<string>();

            case JTokenType.Boolean:
                return token.ToObject<bool>();

            case JTokenType.Null:
            case JTokenType.Undefined:
                return null;

            case JTokenType.Date:
                return token.ToObject<DateTime>();

            case JTokenType.TimeSpan:
                return token.ToObject<TimeSpan>();

            case JTokenType.Guid:
                return token.ToObject<Guid>();

            case JTokenType.Uri:
                return token.ToObject<Uri>();

            case JTokenType.Bytes:
                return token.ToObject<byte[]>();

            case JTokenType.Comment:
            case JTokenType.Raw:
            case JTokenType.Property:
            case JTokenType.Constructor:
            default:
                return token.ToString();
        }
    }



    /// <summary>
    /// 把任意对象转换为 JToken。
    /// 支持 JsonElement、JToken、本地 CLR 类型。
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public static JToken GetJTokenFromObj(this object value)
    {
        if (value == null)
            return JValue.CreateNull();

        switch (value)
        {
            case JToken jt:
                return jt;
            case System.Text.Json.JsonElement elem:
                return elem.ToJToken();
            case string s:
                return new JValue(s);

            case bool b:
                return new JValue(b);

            case int i:
                return new JValue(i);

            case long l:
                return new JValue(l);

            case double d:
                return new JValue(d);

            case float f:
                return new JValue(f);

            case decimal m:
                return new JValue(m);

            case DateTime dt:
                return new JValue(dt);

            case DateTimeOffset dto:
                return new JValue(dto);

            case Guid g:
                return new JValue(g);

            default:
                // 兜底：用 Newtonsoft 来包装成 JToken
                return JToken.FromObject(value);
        }
    }

}
