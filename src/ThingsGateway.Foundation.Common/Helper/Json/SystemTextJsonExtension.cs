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

namespace ThingsGateway.Foundation.Common.Json.Extension;

/// <summary>
/// System.Text.Json 扩展
/// </summary>
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "使用该序列化时，会和源生成配合使用")]
[UnconditionalSuppressMessage("AOT", "IL3050:", Justification = "使用该序列化时，会和源生成配合使用")]
public static class SystemTextJsonExtension
{
    /// <summary>
    /// 默认Json规则
    /// </summary>
    public static readonly SystemTextJsonService SystemTextJsonService = new();

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static object? FromSystemTextJsonString(this string json, Type type, JsonSerializerOptions? options = null)
    {
        return SystemTextJsonService.FromSystemTextJsonString(json, type, options);
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    public static T? FromSystemTextJsonString<T>(this string json, JsonSerializerOptions? options = null)
    {
        return SystemTextJsonService.FromSystemTextJsonString<T>(json, options);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="item"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ToSystemTextJsonString(this object item, JsonSerializerOptions? options)
    {
        return SystemTextJsonService.ToSystemTextJsonString(item, options);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    public static string ToSystemTextJsonString(this object item, bool indented = true, bool ignoreNull = true)
    {
        return SystemTextJsonService.ToSystemTextJsonString(item, indented, ignoreNull);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    public static byte[] ToSystemTextJsonUtf8Bytes(this object item, bool indented = true, bool ignoreNull = true)
    {
        return SystemTextJsonService.ToSystemTextJsonUtf8Bytes(item, indented, ignoreNull);
    }



}
