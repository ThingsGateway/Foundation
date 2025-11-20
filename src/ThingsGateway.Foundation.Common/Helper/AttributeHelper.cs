using System.ComponentModel;
using System.Reflection;

namespace ThingsGateway.Foundation.Common.Extension;



public static class AttributeHelper
{

    public static T? GetCustomAttributeEx<T>(this Type type) where T : Attribute
    {
        if (type.Assembly.IsDynamic)
            return type.GetCustomAttribute<T>();

        if (AttributeCache<T>.TypeCache.TryGetValue(type, out var attr))
            return attr;

        attr = type.GetCustomAttribute<T>();
        AttributeCache<T>.TypeCache[type] = attr;
        return attr;
    }

    public static T? GetCustomAttributeEx<T>(this PropertyInfo type) where T : Attribute
    {
        if (type.PropertyType.Assembly.IsDynamic)
            return type.GetCustomAttribute<T>();

        if (AttributeCache<T>.PropertyInfoCache.TryGetValue(type, out var attr))
            return attr;

        attr = type.GetCustomAttribute<T>();
        AttributeCache<T>.PropertyInfoCache[type] = attr;
        return attr;
    }


    /// <summary>获取成员绑定的显示名</summary>
    /// <param name="member"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static String? GetDisplayName(this MemberInfo member, Boolean inherit = true)
    {
        var att = member.GetCustomAttribute<DisplayNameAttribute>(inherit);
        if (string.IsNullOrWhiteSpace(att?.DisplayName) == false) return att.DisplayName;

        return null;
    }

    /// <summary>获取成员绑定的备注</summary>
    /// <param name="member"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static String? GetDescription(this MemberInfo member, Boolean inherit = true)
    {
        var att2 = member.GetCustomAttribute<DescriptionAttribute>(inherit);
        if (string.IsNullOrWhiteSpace(att2?.Description) == false) return att2.Description;

        return null;
    }

    /// <summary>获取自定义属性的值。可用于ReflectionOnly加载的程序集</summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static TResult? GetCustomAttributeValue<TAttribute, TResult>(this Assembly target) where TAttribute : Attribute
    {
        if (target == null) return default;

        // CustomAttributeData可能会导致只反射加载，需要屏蔽内部异常
        try
        {
            var list = CustomAttributeData.GetCustomAttributes(target);
            if (list == null || list.Count <= 0) return default;

            foreach (var item in list)
            {
                if (typeof(TAttribute) != item.Constructor.DeclaringType) continue;

                var args = item.ConstructorArguments;
                if (args?.Count > 0) return (TResult?)args[0].Value;
            }
        }
        catch { }

        return default;
    }

    /// <summary>获取自定义属性的值。可用于ReflectionOnly加载的程序集</summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="target">目标对象</param>
    /// <param name="inherit">是否递归</param>
    /// <returns></returns>
    public static TResult? GetCustomAttributeValue<TAttribute, TResult>(this MemberInfo target, Boolean inherit = true) where TAttribute : Attribute
    {
        if (target == null) return default;

        try
        {
            var list = CustomAttributeData.GetCustomAttributes(target);
            if (list?.Count > 0)
            {
                foreach (var item in list)
                {
                    if (typeof(TAttribute).FullName != item.Constructor.DeclaringType?.FullName) continue;

                    var args = item.ConstructorArguments;
                    if (args?.Count > 0) return (TResult?)args[0].Value;
                }
            }
            if (inherit && target is Type type && type.BaseType != null)
            {
                target = type.BaseType;
                if (target != null && target != typeof(Object))
                    return GetCustomAttributeValue<TAttribute, TResult>(target, inherit);
            }
        }
        catch
        {
            // 出错以后，如果不是仅反射加载，可以考虑正面来一次
            if (!target.Module.Assembly.ReflectionOnly)
            {
                //var att = GetCustomAttribute<TAttribute>(target, inherit);
                var att = target.GetCustomAttribute<TAttribute>(inherit);
                if (att != null)
                {
                    var pi = typeof(TAttribute).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(TResult));
                    if (pi != null) return (TResult?)att.GetValueEx(pi);
                }
            }
        }

        return default;
    }

}


internal static class AttributeCache<T> where T : Attribute
{
    internal static readonly Dictionary<Type, T?> TypeCache = new(128);
    internal static readonly Dictionary<PropertyInfo, T?> PropertyInfoCache = new(128);
}