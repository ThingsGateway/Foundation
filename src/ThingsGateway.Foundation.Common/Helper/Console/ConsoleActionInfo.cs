namespace ThingsGateway.Foundation.Common;

/// <summary>
/// 控制台行为信息。
/// </summary>
#pragma warning disable CA1815 // 重写值类型上的 Equals 和相等运算符
public readonly struct ConsoleActionInfo
#pragma warning restore CA1815 // 重写值类型上的 Equals 和相等运算符
{
    /// <summary>
    /// 初始化<see cref="ConsoleActionInfo"/>结构体。
    /// </summary>
    /// <param name="description">行为描述。</param>
    /// <param name="fullOrder">完整命令。</param>
    /// <param name="action">执行的动作。</param>
    public ConsoleActionInfo(string description, string fullOrder, Func<Task> action)
    {
        this.FullOrder = fullOrder;
        this.Action = action ?? throw new ArgumentNullException(nameof(action));
        this.Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    /// <summary>
    /// 获取控制台行为对应的动作。
    /// </summary>
    public Func<Task> Action { get; }

    /// <summary>
    /// 获取控制台行为描述。
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 获取完整命令。
    /// </summary>
    public string FullOrder { get; }

}
