using ThingsGateway.Foundation.Common.Data;

using ThingsGateway.Foundation.Common.Json.Extension;

namespace ThingsGateway.Foundation.Common.Caching;

/// <summary>Redis编码器</summary>
public class RedisJsonEncoder : DefaultPacketEncoder
{
    #region 属性
    private static SystemTextJsonService _host;
    #endregion

    static RedisJsonEncoder() => _host = GetJsonHost();

    /// <summary>实例化Redis编码器</summary>
    public RedisJsonEncoder() => JsonHost = _host;

    internal static SystemTextJsonService GetJsonHost()
        => SystemTextJsonService.Default;

    /// <summary>字符串解码为对象。复杂类型采用Json反序列化</summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    protected override Object? OnDecode(String value, Type type)
    {
        if (type == typeof(Boolean) && value == "OK") return true;

        return base.OnDecode(value, type);
    }
}