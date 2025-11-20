namespace ThingsGateway;

/// <summary>
/// 注册服务启动配置
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AppStartupAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="order"></param>
    public AppStartupAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// 排序
    /// </summary>
    /// <remarks>优先调用数值较大的</remarks>
    public int Order { get; set; }
}