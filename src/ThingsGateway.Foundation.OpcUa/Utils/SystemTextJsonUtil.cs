using System.Text.Json.Nodes;

namespace ThingsGateway.Foundation.OpcUa;

public static class SystemTextJsonUtil
{

    /// <summary>
    /// 解析获取 DataValue（等价原逻辑）
    /// </summary>
    public static DataValue Decode(
        IServiceMessageContext Context,
        NodeId dataTypeId,
        BuiltInType builtInType,
        int valueRank,
        JsonNode json
    )
    {
        var data = DecoderObject(Context, dataTypeId, builtInType, valueRank, json);
        var dataValue = new DataValue(new Variant(data));
        return dataValue;
    }

    public static object DecoderObject(
        IServiceMessageContext Context,
        NodeId dataTypeId,
        BuiltInType builtInType,
        int valueRank,
        JsonNode json
    )
    {
        // 直接构造 JSON 字符串，避免 anonymous object -> JSON -> JsonDecoder 的双重开销
        string newData;
        if (builtInType == BuiltInType.ExtensionObject)
        {
            newData = JsonHelper.BuildExtensionObjectJson(dataTypeId, json);
        }
        else if (builtInType == BuiltInType.Variant)
        {
            var type = TypeInfo.GetDataTypeId(JsonHelper.GetSystemType(json));
            newData = JsonHelper.BuildVariantJson(type, json);
        }
        else
        {
            newData = JsonHelper.BuildSimpleValueJson(json);
        }

        using var decoder = new JsonDecoder(newData, Context);
        var data = JsonHelper.DecodeRawData(decoder, builtInType, valueRank, "Value");
        return data;
    }
    /// <summary>
    /// 将 OPC UA 值编码为 JToken（等价原逻辑）
    /// </summary>
    internal static JsonNode Encode(
        IServiceMessageContext Context,
        BuiltInType type,
        object value
    )
    {
        using var encoder = JsonHelper.CreateEncoder(Context, null, false);
        JsonHelper.Encode(encoder, type, "Value", value);
        var textbuffer = encoder.CloseAndReturnText();
        return JsonNode.Parse(textbuffer)["Value"];
    }





}
