//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace ThingsGateway.Foundation.Common.Json.Extension;

/// <summary>
/// json扩展
/// </summary>
[RequiresDynamicCode("Newtonsoft.Json依赖动态 IL 生成，不支持AOT")]
[RequiresUnreferencedCode("此方法可能会使用反射构建，与剪裁不兼容。")]
public static class NewtonsoftJsonExtension
{
    /// <summary>
    /// 默认Json规则
    /// </summary>
    public static readonly NewtonsoftJsonService NewtonsoftJsonService = new();

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="jsonSerializerSettings"></param>
    /// <returns></returns>
    public static object? FromJsonNetString(this string json, Type type, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        return NewtonsoftJsonService.FromJsonNetString(json, type, jsonSerializerSettings);
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="jsonSerializerSettings"></param>
    /// <returns></returns>
    public static T? FromJsonNetString<T>(this string json, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        return NewtonsoftJsonService.FromJsonNetString<T>(json, jsonSerializerSettings);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="item"></param>
    /// <param name="jsonSerializerSettings"></param>
    /// <returns></returns>
    public static string ToJsonNetString(this object item, JsonSerializerSettings? jsonSerializerSettings)
    {
        return NewtonsoftJsonService.ToJsonNetString(item, jsonSerializerSettings);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    public static string ToJsonNetString(this object item, bool indented = true)
    {
        return NewtonsoftJsonService.ToJsonNetString(item, indented);
    }

}
