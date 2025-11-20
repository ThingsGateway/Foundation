namespace ThingsGateway.Foundation.Common.Caching.Clusters;

/// <summary>集群中的节点</summary>
public class RedisNode : IRedisNode
{
    #region 属性
    /// <summary>拥有者</summary>
    public Redis Owner { get; set; } = null!;

    /// <summary>节点地址</summary>
    public String EndPoint { get; set; } = null!;

    /// <summary>是否从节点</summary>
    public Boolean Slave { get; set; }

    /// <summary>连续错误次数。达到阈值后屏蔽该节点</summary>
    public Int32 Error { get; set; }

    /// <summary>下一次时间。节点出错时，将禁用一段时间</summary>
    public DateTime NextTime { get; set; }
    #endregion

    #region 构造
    /// <summary>已重载。友好显示节点地址</summary>
    /// <returns></returns>
    public override String ToString() => EndPoint ?? base.ToString();
    #endregion
}
