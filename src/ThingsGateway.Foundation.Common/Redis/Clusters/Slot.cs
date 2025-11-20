namespace ThingsGateway.Foundation.Common.Caching.Clusters;

/// <summary>数据槽区间</summary>
#pragma warning disable CA1815 // 重写值类型上的 Equals 和相等运算符
public struct Slot
#pragma warning restore CA1815 // 重写值类型上的 Equals 和相等运算符
{
    /// <summary>起始</summary>
    public Int32 From;

    /// <summary>结束</summary>
    public Int32 To;

    /// <summary>已重载。返回区间</summary>
    /// <returns></returns>
    public override String ToString() => From == To ? From + "" : $"{From}-{To}";
}