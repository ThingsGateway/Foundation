namespace ThingsGateway.Foundation.OpcUa;

public static class NewtonsoftJsonUtils
{

    /// <summary>
    /// 解析获取 DataValue
    /// </summary>
    public static DataValue Decode(
        IServiceMessageContext Context,
        NodeId dataTypeId,
        BuiltInType builtInType,
        int valueRank,
        JToken json
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
        JToken json
    )
    {
        string newData;
        if (builtInType == BuiltInType.ExtensionObject)
        {
            newData = JsonHelper.BuildExtensionObjectJson(dataTypeId, json);
        }
        else if (builtInType == BuiltInType.Variant)
        {
            var type = TypeInfo.GetDataTypeId(JsonHelper.GetSystemType(json.Type));
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
    /// 将 OPC UA 值编码为 JToken
    /// </summary>
    internal static JToken Encode(
        IServiceMessageContext Context,
        BuiltInType type,
        object value
    )
    {
        using var encoder = JsonHelper.CreateEncoder(Context, null, false);
        JsonHelper.Encode(encoder, type, "Value", value);
        var textbuffer = encoder.CloseAndReturnText();
        return JToken.Parse(textbuffer)["Value"];
    }


}
