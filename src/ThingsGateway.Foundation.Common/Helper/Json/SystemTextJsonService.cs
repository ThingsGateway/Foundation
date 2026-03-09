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
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;

using ThingsGateway.Foundation.Common.Serialization;

namespace ThingsGateway.Foundation.Common.Json.Extension;

/// <summary>
/// System.Text.Json 扩展
/// </summary>
public class SystemTextJsonService
{
    public static SystemTextJsonService Default { get; }

    private static readonly Dictionary<string, JsonSerializerOptions> _optionsCache;
    static SystemTextJsonService()
    {
        _optionsCache = new(StringComparer.Ordinal);
        Default = new();
    }
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

    /// <summary>
    /// 读取时使用的默认配置
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; }

    /// <summary>
    /// 服务提供者。用于反序列化时构造内部成员对象
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; } = App.Provider;

    public static JsonSerializerOptions GetOptions(bool writeIndented, bool ignoreNull, bool camelCase = false)
    {
        var key = $"{writeIndented}_{ignoreNull}_{camelCase}";
        lock (_optionsCache)
        {
            if (_optionsCache.TryGetValue(key, out var options))
                return options;

            options = CreateBaseOptions();
            options.WriteIndented = writeIndented;
            options.DefaultIgnoreCondition = ignoreNull
                ? JsonIgnoreCondition.WhenWritingNull
                : JsonIgnoreCondition.Never;
            options.PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null;

            _optionsCache[key] = options;
            return options;
        }
    }

    internal static JsonSerializerOptions CreateOptions(bool writeIndented, bool ignoreNull, bool camelCase = false, bool ignoreCycles = false, bool enumString = false, bool int64AsString = false)
    {
        var options = new JsonSerializerOptions(GetOptions(writeIndented, ignoreNull, camelCase));

        if (ignoreCycles)
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        if (enumString)
            AddConverter<JsonStringEnumConverter>(options, static () => new JsonStringEnumConverter());

        if (int64AsString)
        {
            AddConverter<SystemTextJsonSafeInt64Converter>(options, static () => new SystemTextJsonSafeInt64Converter());
            AddConverter<SystemTextJsonSafeUInt64Converter>(options, static () => new SystemTextJsonSafeUInt64Converter());
        }

        return options;
    }

    public SystemTextJsonService()
    {
        IndentedOptions = GetOptions(true, false);
        NoneIndentedOptions = GetOptions(false, false);
        IgnoreNullIndentedOptions = GetOptions(true, true);
        IgnoreNullNoneIndentedOptions = GetOptions(false, true);
        JsonSerializerOptions = new JsonSerializerOptions(CreateBaseOptions());
    }

    public static JsonSerializerOptions GetDefaultOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNamingPolicy = new PreserveNamingPolicy(),
        };

        AddConverter<SystemTextJsonLocalTimeConverter>(options, static () => new SystemTextJsonLocalTimeConverter());
        AddConverter<SystemTextJsonTypeConverter>(options, static () => new SystemTextJsonTypeConverter());

#if NET8_0_OR_GREATER
        options.TypeInfoResolver = DataMemberResolver.Default;
#endif

        return options;
    }

    private static JsonSerializerOptions CreateBaseOptions()
    {
        var options = GetDefaultOptions();
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;

        AddConverter<SystemTextJsonByteArrayToNumberArrayConverter>(options, static () => new SystemTextJsonByteArrayToNumberArrayConverter());
        AddConverter<JTokenSystemTextJsonConverter>(options, static () => new JTokenSystemTextJsonConverter());
        AddConverter<JValueSystemTextJsonConverter>(options, static () => new JValueSystemTextJsonConverter());
        AddConverter<JObjectSystemTextJsonConverter>(options, static () => new JObjectSystemTextJsonConverter());
        AddConverter<JArraySystemTextJsonConverter>(options, static () => new JArraySystemTextJsonConverter());

        options.TypeInfoResolver ??= new DefaultJsonTypeInfoResolver();

        return options;
    }

    private static void AddConverter<TConverter>(JsonSerializerOptions options, Func<TConverter> converterFactory)
        where TConverter : JsonConverter
    {
        if (options.Converters.Any(static converter => converter is TConverter))
            return;

        options.Converters.Add(converterFactory());
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
        return JsonSerializer.Deserialize(json, type, options ?? JsonSerializerOptions);
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    public T? FromSystemTextJsonString<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? JsonSerializerOptions);
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
        var options = ignoreNull ? indented ? IgnoreNullIndentedOptions : IgnoreNullNoneIndentedOptions : indented ? IndentedOptions : NoneIndentedOptions;
        return JsonSerializer.Serialize(item, item?.GetType() ?? typeof(object), options);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    public byte[] ToSystemTextJsonUtf8Bytes(object item, bool indented = true, bool ignoreNull = true)
    {
        var options = ignoreNull ? indented ? IgnoreNullIndentedOptions : IgnoreNullNoneIndentedOptions : indented ? IndentedOptions : NoneIndentedOptions;
        return JsonSerializer.SerializeToUtf8Bytes(item, item?.GetType() ?? typeof(object), options);
    }


    private sealed class PreserveNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name;
    }

}
