namespace System.Diagnostics.CodeAnalysis;

#if !NETCOREAPP || NET6_0
/// <summary>
/// Indicates that the specified method requires the ability to generate new code at runtime,
/// for example through <see cref="Reflection"/>.
/// </summary>
/// <remarks>
/// This allows tools to understand which methods are unsafe to call when compiling ahead of time.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresDynamicCodeAttribute"/> class
    /// with the specified message.
    /// </summary>
    /// <param name="message">
    /// A message that contains information about the usage of dynamic code.
    /// </param>
    public RequiresDynamicCodeAttribute(string message)
    {
        Message = message;
    }

    /// <summary>
    /// When set to true, indicates that the annotation should not apply to static members.
    /// </summary>
    public bool ExcludeStatics { get; set; }

    /// <summary>
    /// Gets a message that contains information about the usage of dynamic code.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets or sets an optional URL that contains more information about the method,
    /// why it requires dynamic code, and what options a consumer has to deal with it.
    /// </summary>
    public string? Url { get; set; }
}
#endif
#if !NET6_0_OR_GREATER

/// <summary>
/// Specifies the types of members that are dynamically accessed.
///
/// This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a
/// bitwise combination of its member values.
/// </summary>
[Flags]
internal enum DynamicallyAccessedMemberTypes
{
    /// <summary>
    /// Specifies all members.
    /// </summary>
    All = -1,
    /// <summary>
    /// Specifies no members.
    /// </summary>
    None = 0,
    /// <summary>
    /// Specifies the default, parameterless public constructor.
    /// </summary>
    PublicParameterlessConstructor = 1,
    /// <summary>
    /// Specifies all public constructors.
    /// </summary>
    PublicConstructors = 3,
    /// <summary>
    /// Specifies all non-public constructors.
    /// </summary>
    NonPublicConstructors = 4,
    /// <summary>
    /// Specifies all public methods.
    /// </summary>
    PublicMethods = 8,
    /// <summary>
    /// Specifies all non-public methods.
    /// </summary>
    NonPublicMethods = 16,
    /// <summary>
    /// Specifies all public fields.
    /// </summary>
    PublicFields = 32,
    /// <summary>
    /// Specifies all non-public fields.
    /// </summary>
    NonPublicFields = 64,
    /// <summary>
    /// Specifies all public nested types.
    /// </summary>
    PublicNestedTypes = 128,
    /// <summary>
    /// Specifies all non-public nested types.
    /// </summary>
    NonPublicNestedTypes = 256,
    /// <summary>
    /// Specifies all public properties.
    /// </summary>
    PublicProperties = 512,
    /// <summary>
    /// Specifies all non-public properties.
    /// </summary>
    NonPublicProperties = 1024,
    /// <summary>
    /// Specifies all public events.
    /// </summary>
    PublicEvents = 2048,
    /// <summary>
    /// Specifies all non-public events.
    /// </summary>
    NonPublicEvents = 4096,
    /// <summary>
    /// Specifies all interfaces implemented by the type.
    /// </summary>
    Interfaces = 8192
}

/// <summary>
/// Indicates that certain members on a specified System.Type are accessed dynamically,
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
internal sealed class DynamicallyAccessedMembersAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute
    /// </summary>
    /// <param name="memberTypes"></param>
    public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
    {
        this.MemberTypes = memberTypes;
    }

    /// <summary>
    ///  Gets the System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes that
    /// </summary>
    public DynamicallyAccessedMemberTypes MemberTypes { get; }
}
#endif


