#if NET462 || NETSTANDARD2_0 || NETSTANDARD2_1
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// 无条件抑制消息特性
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
internal sealed class UnconditionalSuppressMessageAttribute : Attribute
{
    /// <summary>
    /// 初始化 <see cref="UnconditionalSuppressMessageAttribute"/> 类的新实例
    /// </summary>
    /// <param name="category">抑制的类别</param>
    /// <param name="checkId">抑制的检查ID</param>
    public UnconditionalSuppressMessageAttribute(string category, string checkId)
    {
        this.Category = category;
        this.CheckId = checkId;
    }

    /// <summary>
    /// 获取抑制的类别
    /// </summary>
    public string? Category { get; }

    /// <summary>
    /// 获取抑制的检查ID
    /// </summary>
    public string? CheckId { get; }

    /// <summary>
    /// 获取或设置抑制的理由
    /// </summary>
    public string? Justification { get; set; }

    /// <summary>
    /// 获取或设置消息ID
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// 获取或设置范围
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// 获取或设置目标
    /// </summary>
    public string? Target { get; set; }
}
#endif

#if NET462 || NETSTANDARD2_0 || NETSTANDARD2_1
/// <summary>
/// 指示方法需要未引用的代码的特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
public sealed class RequiresUnreferencedCodeAttribute : Attribute
{
    /// <summary>
    /// 初始化 <see cref="RequiresUnreferencedCodeAttribute"/> 类的新实例
    /// </summary>
    /// <param name="message">描述为什么需要未引用代码的消息</param>
    public RequiresUnreferencedCodeAttribute(string message)
    {
        this.Message = message;
    }

    /// <summary>
    /// 获取描述为什么需要未引用代码的消息
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// 获取或设置可选的URL，提供有关该方法为何需要未引用代码的更多信息
    /// </summary>
    public string? Url { get; set; }
}
#endif