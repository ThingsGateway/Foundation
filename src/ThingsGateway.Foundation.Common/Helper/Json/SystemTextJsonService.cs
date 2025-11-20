//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsGateway.Foundation.Common.Json.Extension;

/// <summary>
/// System.Text.Json 扩展
/// </summary>
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "使用该序列化时，会和源生成配合使用")]
[UnconditionalSuppressMessage("AOT", "IL3050:", Justification = "使用该序列化时，会和源生成配合使用")]
public class SystemTextJsonService
{
    /// <summary>
    /// 默认Json规则（带缩进）
    /// </summary>
    public JsonSerializerOptions IndentedOptions { get; }

    /// <summary>
    /// 默认Json规则（无缩进）
    /// </summary>
    public JsonSerializerOptions NoneIndentedOptions { get; }


    /// <summary>
    /// 默认Json规则（带缩进）
    /// </summary>
    public JsonSerializerOptions IgnoreNullIndentedOptions { get; }

    /// <summary>
    /// 默认Json规则（无缩进）
    /// </summary>
    public JsonSerializerOptions IgnoreNullNoneIndentedOptions { get; }

    public static JsonSerializerOptions GetOptions(bool writeIndented, bool ignoreNull)
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = writeIndented,
            DefaultIgnoreCondition = ignoreNull
                ? JsonIgnoreCondition.WhenWritingNull
                : JsonIgnoreCondition.Never,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };

        options.Converters.Add(new SystemTextJsonByteArrayToNumberArrayConverter());
        options.Converters.Add(new JTokenSystemTextJsonConverter());
        options.Converters.Add(new JValueSystemTextJsonConverter());
        options.Converters.Add(new JObjectSystemTextJsonConverter());
        options.Converters.Add(new JArraySystemTextJsonConverter());

        return options;
    }
    public SystemTextJsonService()
    {


        IndentedOptions = GetOptions(true, false);
        NoneIndentedOptions = GetOptions(false, false);

        IgnoreNullIndentedOptions = GetOptions(true, true);
        IgnoreNullNoneIndentedOptions = GetOptions(false, true);

    }



    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public object? FromSystemTextJsonString(string json, Type type, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize(json, type, options ?? IndentedOptions);
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    public T? FromSystemTextJsonString<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? IndentedOptions);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="item"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public string ToSystemTextJsonString(object item, JsonSerializerOptions? options)
    {
        return JsonSerializer.Serialize(item, item?.GetType() ?? typeof(object), options ?? IndentedOptions);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    public string ToSystemTextJsonString(object item, bool indented = true, bool ignoreNull = true)
    {
        return JsonSerializer.Serialize(item, item?.GetType() ?? typeof(object), ignoreNull ? indented ? IgnoreNullIndentedOptions : IgnoreNullNoneIndentedOptions : indented ? IndentedOptions : NoneIndentedOptions);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    public byte[] ToSystemTextJsonUtf8Bytes(object item, bool indented = true, bool ignoreNull = true)
    {
        return JsonSerializer.SerializeToUtf8Bytes(item, item.GetType(), ignoreNull ? indented ? IgnoreNullIndentedOptions : IgnoreNullNoneIndentedOptions : indented ? IndentedOptions : NoneIndentedOptions);
    }

}
