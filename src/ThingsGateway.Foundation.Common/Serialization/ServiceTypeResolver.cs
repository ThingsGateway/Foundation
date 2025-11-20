#if NET8_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;


namespace ThingsGateway.Foundation.Common.Serialization;

/// <summary>支持服务提供者的类型解析器</summary>
[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
public class ServiceTypeResolver
{
    /// <summary>服务提供者</summary>
    public Func<IServiceProvider>? GetServiceProvider { get; set; }

    /// <summary>匹配修改</summary>
    /// <param name="typeInfo"></param>
    public void Modifier(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

        var type = typeInfo.Type;
        if (!type.IsBaseType())
        {
            var provider = GetServiceProvider?.Invoke();
            if (provider?.GetService(type) is not null)
            {
                typeInfo.CreateObject = () => provider.GetService(type) ?? provider.CreateInstance(type) ?? type.CreateInstance()!;
            }
            else if (type.IsInterface && type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    var type2 = typeof(List<>).MakeGenericType(type.GetGenericArguments());
                    typeInfo.CreateObject = () => type2.CreateInstance()!;
                }
                else if (type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var type2 = typeof(Dictionary<,>).MakeGenericType(type.GetGenericArguments());
                    typeInfo.CreateObject = () => type2.CreateInstance()!;
                }
            }
        }
    }
}
#endif