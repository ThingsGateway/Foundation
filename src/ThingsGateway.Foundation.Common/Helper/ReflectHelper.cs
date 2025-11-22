using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace ThingsGateway.Foundation.Common.Extension;

public static class ReflectHelper
{
    private const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    private const BindingFlags bfic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;


    /// <summary>获取属性</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public static PropertyInfo? GetPropertyEx(this Type type, String name, Boolean ignoreCase = false)
    {
        // 父类私有属性的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
        var type2 = type;
        while (type2 != null && type2 != typeof(Object))
        {
            //var pi = type.GetProperty(name, ignoreCase ? bfic : bf);
            var pi = type2.GetProperty(name, bf);
            if (pi != null) return pi;
            if (ignoreCase)
            {
                pi = type2.GetProperty(name, bfic);
                if (pi != null) return pi;
            }

            type2 = type2.BaseType;
        }
        return null;
    }

    /// <summary>获取字段</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public static FieldInfo? GetFieldEx(this Type type, String name, Boolean ignoreCase = false)
    {
        // 父类私有字段的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
        var type2 = type;
        while (type2 != null && type2 != typeof(Object))
        {
            //var fi = type.GetField(name, ignoreCase ? bfic : bf);
            var fi = type2.GetField(name, bf);
            if (fi != null) return fi;
            if (ignoreCase)
            {
                fi = type2.GetField(name, bfic);
                if (fi != null) return fi;
            }

            type2 = type2.BaseType;
        }
        return null;
    }



    /// <summary>是否可空类型。继承泛型定义Nullable的类型</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsNullable(this Type type) => type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    private static Type NullableType = typeof(Nullable<>);
    /// <summary>
    /// 判断该类型是否为可空类型
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static bool IsNullableType(PropertyInfo propertyInfo)
    {
        var att = propertyInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        return att != null || propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.
          GetGenericTypeDefinition().Equals(NullableType);
    }

    /// <summary>根据名称获取类型</summary>
    /// <param name="typeName">类型名</param>
    /// <returns></returns>
    [RequiresUnreferencedCode("此方法会使用Type.GetType，与剪裁不兼容。")]
    public static Type? GetTypeEx(this String typeName)
    {
        if (String.IsNullOrEmpty(typeName))
        {
            return null;
        }

        return Type.GetType(typeName);

    }



    /// <summary>是否泛型列表</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsList([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type) => type?.IsGenericType == true && type.IsFrom(typeof(IList<>));

    /// <summary>是否泛型字典</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsDictionary([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type) => type?.IsGenericType == true && type.IsFrom(typeof(IDictionary<,>));

    /// <summary>是否基础类型。识别常见基元类型和String，支持可空类型</summary>
    /// <remarks>
    /// 基础类型可以方便的进行字符串转换，用于存储于传输。
    /// 在序列化时，基础类型作为原子数据不可再次拆分，而复杂类型则可以进一步拆分。
    /// 包括：Boolean/Char/SByte/Byte/Int16/UInt16/Int32/UInt32/Int64/UInt64/Single/Double/Decimal/DateTime/String/枚举，以及这些类型的可空类型
    /// </remarks>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsBaseType(this Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return Type.GetTypeCode(type) != TypeCode.Object;
    }

    /// <summary>是否数字类型。包括整数、小数、字节等</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsNumber(this Type type)
    {
        var code = type.GetTypeCode();
        return code >= TypeCode.SByte && code <= TypeCode.Decimal;
    }
    /// <summary>是否数字类型。包括整数、小数、字节等</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Boolean IsNullNumber(this Type type)
    {
        if (type == null)
        {
            return false;
        }
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = Nullable.GetUnderlyingType(type)!;
        }

        var code = type.GetTypeCode();
        return code >= TypeCode.SByte && code <= TypeCode.Decimal;
    }

    public static bool IsNumberArray([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        if (type == null)
        {
            return false;
        }

        // 数组
        if (type.IsArray)
        {
            return IsNumber(type.GetElementType()!);
        }

        // 泛型集合，如 List<int>、HashSet<long>、ICollection<float> 等
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();

            // 只要实现了 IEnumerable<T> 就认为是集合
            var enumerableInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
            {
                var elementType = enumerableInterface.GetGenericArguments()[0];
                return IsNumber(elementType);
            }
        }

        return false;
    }

    public static bool IsNullNumberArray([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        if (type == null)
        {
            return false;
        }

        // 数组
        if (type.IsArray)
        {
            return IsNullNumber(type.GetElementType()!);
        }

        // 泛型集合，如 List<int>、HashSet<long>、ICollection<float> 等
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();

            // 只要实现了 IEnumerable<T> 就认为是集合
            var enumerableInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
            {
                var elementType = enumerableInterface.GetGenericArguments()[0];
                return IsNullNumber(elementType);
            }
        }

        return false;
    }

    public static bool IsCollectionsList(this Type type)
    {
        if (type == null)
        {
            return false;
        }
        return type.FullName!.StartsWith("System.Collections.Generic.List") || type.FullName.StartsWith("System.Collections.Generic.IEnumerable");
    }


    public static bool IsIterator(this Type type)
    {
        if (type == null)
        {
            return false;
        }
        if (type.BaseType == null)
        {
            return false;
        }
        if (type.BaseType.IsGenericType)
        {
            return type.BaseType.GetGenericTypeDefinition()?.FullName == "System.Linq.Enumerable+Iterator`1";
        }
        return false;
    }

    public static bool IsEnum(this Type type)
    {
        if (type == null)
        {
            return false;
        }
        return type.GetTypeInfo().IsEnum;
    }

    /// <summary>获取类型代码，支持可空类型</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static TypeCode GetTypeCode(this Type type) => type == null ? TypeCode.Empty : Type.GetTypeCode(Nullable.GetUnderlyingType(type) ?? type);

    /// <summary>反射调用指定对象的方法。target为类型时调用其静态方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="type"></param>
    /// <param name="name">方法名</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
    public static Object? InvokeEx(this Object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, String name, params Object?[] parameters)
    {
        ArgumentNullExceptionEx.ThrowIfNull(target);
        if (String.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (TryInvokeEx(target, type, name, out var value, parameters))
        {
            return value;
        }

        throw new Exception(string.Format("Cannot find method named {1} in class {0}!", type, name));
    }
    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="type"></param>
    /// <param name="name">方法名</param>
    /// <param name="value">数值</param>
    /// <param name="parameters">方法参数</param>
    /// <remarks>反射调用是否成功</remarks>
    public static Boolean TryInvokeEx(this Object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, String name, out Object? value, params Object?[] parameters)
    {
        value = null;

        if (String.IsNullOrEmpty(name))
        {
            return false;
        }

        // 参数类型数组
        var ps = parameters.Select(e => e?.GetType()).ToArray();

        // 如果参数数组出现null，则无法精确匹配，可按参数个数进行匹配
        var method = ps.Any(e => e == null) ? GetMethodEx(type, name) : GetMethodEx(type, name, ps!);
        method ??= GetMethodsEx(type, name, ps.Length > 0 ? ps.Length : -1).FirstOrDefault();
        if (method == null)
        {
            return false;
        }

        value = InvokeEx(target, method, parameters);

        return true;
    }

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object? InvokeEx(this Object? target, MethodBase method, params Object?[]? parameters)
    {
        //if (target == null) throw new ArgumentNullException("target");
        ArgumentNullExceptionEx.ThrowIfNull(method);
        if (!method.IsStatic && target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }
        return method.Invoke(target, parameters);
    }

    /// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
    /// <returns></returns>
    public static MethodInfo[] GetMethodsEx([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] this Type type, String name, Int32 paramCount = -1)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Array.Empty<MethodInfo>();
        }

        return GetMethods(type, name, paramCount);
    }

    /// <summary>获取方法</summary>
    /// <remarks>用于具有多个签名的同名方法的场合，不确定是否存在性能问题，不建议普通场合使用</remarks>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="paramTypes">参数类型数组</param>
    /// <returns></returns>
    public static MethodInfo? GetMethodEx([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] this Type type, String name, params Type[] paramTypes)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        // 如果其中一个类型参数为空，得用别的办法
        if (paramTypes.Length > 0 && paramTypes.Any(e => e == null))
        {
            return GetMethods(type, name, paramTypes.Length).FirstOrDefault();
        }

        return GetMethod(type, name, paramTypes);
    }
    /// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
    /// <returns></returns>
    public static MethodInfo[] GetMethods([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] Type type, String name, Int32 paramCount = -1)
    {
        var ms = type.GetMethods(bf);
        //if (ms == null || ms.Length == 0) return ms;

        using var list = new ValueListBuilder<MethodInfo>();
        foreach (var item in ms)
        {
            if (item.Name == name)
            {
                if (paramCount >= 0 && item.GetParameters().Length == paramCount)
                {
                    list.Add(item);
                }
            }
        }
        return list.AsSpan().ToArray();
    }
    /// <summary>获取方法</summary>
    /// <remarks>用于具有多个签名的同名方法的场合</remarks>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="paramTypes">参数类型数组</param>
    /// <returns></returns>
    public static MethodInfo? GetMethod([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, String name, params Type[] paramTypes)
    {
        MethodInfo? mi = null;
        do
        {
            if (paramTypes == null || paramTypes.Length == 0)
            {
                mi = type.GetMethod(name, bf);
            }
            else
            {
                mi = type.GetMethod(name, bf, null, paramTypes, null);
            }

            if (mi != null)
            {
                return mi;
            }

            if (type.BaseType == null)
            {
                break;
            }
            type = type.BaseType;
        }
        while (type != typeof(Object));
        return null;
    }

    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="type"></param>
    /// <param name="name">名称</param>
    /// <param name="throwOnError">出错时是否抛出异常</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"></exception>
    [DebuggerHidden]
    public static Object? GetValueEx(this Object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, String name, Boolean throwOnError = true)
    {
        ArgumentNullExceptionEx.ThrowIfNull(target);
        if (String.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (TryGetValueEx(target, type, name, out var value))
        {
            return value;
        }

        if (!throwOnError)
        {
            return null;
        }

        throw new ArgumentException($"The [{name}] property or field does not exist in class [{type.FullName}].");
    }
    /// <summary>获取目标对象指定名称的属性/字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="type"></param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <returns>是否成功获取数值</returns>
    internal static Boolean TryGetValueEx(this Object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, String name, out Object? value)
    {
        value = null;

        if (String.IsNullOrEmpty(name))
        {
            return false;
        }

        var mi = GetMemberEx(type, name, true);
        if (mi == null)
        {
            return false;
        }

        value = GetValueEx(target, mi);

        return true;
    }

    /// <summary>获取目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <returns></returns>
    [DebuggerHidden]
    public static Object? GetValueEx(this Object? target, MemberInfo member)
    {
        // 有可能跟普通的 PropertyInfo.GetValue(Object target) 搞混了
        if (member == null && target is MemberInfo mi)
        {
            member = mi;
            target = null;
        }

        //if (target is IModel model && member is PropertyInfo) return model[member.Name];

        if (member is PropertyInfo property)
        {
            return property.GetValue(target);
        }
        else if (member is FieldInfo field)
        {
            return field.GetValue(target);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(member));
        }
    }

    /// <summary>设置目标对象指定名称的属性/字段值，若不存在返回false</summary>
    /// <param name="target">目标对象</param>
    /// <param name="type"></param>
    /// <param name="name">名称</param>
    /// <param name="value">数值</param>
    /// <remarks>反射调用是否成功</remarks>
    [DebuggerHidden]
    public static Boolean SetValueEx(this Object target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, String name, Object? value)
    {
        if (String.IsNullOrEmpty(name))
        {
            return false;
        }

        var mi = type.GetMemberEx(name, true);
        if (mi == null)
        {
            return false;
        }

        target.SetValueEx(mi, value);

        return true;
    }
    /// <summary>设置目标对象的成员值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="member">成员</param>
    /// <param name="value">数值</param>
    [DebuggerHidden]
    public static void SetValueEx(this Object target, MemberInfo member, Object? value)
    {
        if (member is PropertyInfo pi)
        {
            pi.SetValue(target, value);
        }
        else if (member is FieldInfo fi)
        {
            fi.SetValue(target, value);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(member));
        }
    }

    /// <summary>获取成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    public static MemberInfo? GetMemberEx([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] this Type type, String name, Boolean ignoreCase)
    {
        // 父类私有成员的获取需要递归，可见范围则不需要，有些类型的父类为空，比如接口
        var type2 = type;
        while (type2 != null && type2 != typeof(Object))
        {
            var fs = type2.GetMember(name, ignoreCase ? bfic : bf);
            if (fs?.Length > 0)
            {
                // 得到多个的时候，优先返回精确匹配
                if (ignoreCase && fs.Length > 1)
                {
                    foreach (var fi in fs)
                    {
                        if (fi.Name == name)
                        {
                            return fi;
                        }
                    }
                }
                return fs[0];
            }

            type2 = type2.BaseType;
        }
        return null;
    }

    /// <summary>获取成员的类型，字段和属性是它们的类型，方法是返回类型，类型是自身</summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public static Type? GetMemberType(this MemberInfo member)
    {

        if (member is ConstructorInfo ctor) return ctor.DeclaringType;
        if (member is FieldInfo field) return field.FieldType;
        if (member is MethodInfo method) return method.ReturnType;
        if (member is PropertyInfo property) return property.PropertyType;
        if (member is Type type) return type;

        return null;
    }
    /// <summary>获取类型的友好名称</summary>
    /// <param name="type">指定类型</param>
    /// <param name="isfull">是否全名，包含命名空间</param>
    /// <returns></returns>
    public static String GetName(this Type type, Boolean isfull = false) => isfull ? (type.FullName ?? type.Name) : type.Name;
    /// <summary>是否子类</summary>
    /// <returns></returns>
    public static bool IsFrom<
    TBase>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type)
    {
        var baseType = typeof(TBase);
        return IsFrom(type, baseType);
    }

    /// <summary>是否子类</summary>
    /// <returns></returns>
    public static bool IsFrom<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T, TBase>()
    {
        var type = typeof(T);
        var baseType = typeof(TBase);
        return IsFrom(type, baseType);
    }
    /// <summary>是否子类</summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public static bool IsFrom([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type? type, Type? baseType)
    {
        if (type is null || baseType is null)
        {
            return false;
        }

        if (type == baseType)
        {
            return true;
        }

        // 快速路径
        if (!baseType.IsGenericTypeDefinition && baseType.IsAssignableFrom(type))
        {
            return true;
        }

        // 提取泛型定义（如果有）
        Type? baseGenericDef = baseType.IsGenericTypeDefinition
            ? baseType
            : baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : null;

        // 遍历继承链
        for (var current = type; current != null && current != typeof(object); current = current.BaseType)
        {
            if (baseGenericDef != null && current.IsGenericType && current.GetGenericTypeDefinition() == baseGenericDef)
            {
                // 如果 baseType 是开放泛型定义（List<>），只匹配定义即可
                if (baseType.IsGenericTypeDefinition)
                {
                    return true;
                }

                // 否则要求泛型参数完全一致（List<int> vs List<string>）
                if (current.GenericTypeArguments.SequenceEqual(baseType.GenericTypeArguments))
                {
                    return true;
                }
            }
        }

        // 遍历接口
        foreach (var iface in type.GetInterfaces())
        {
            if (baseGenericDef != null && iface.IsGenericType && iface.GetGenericTypeDefinition() == baseGenericDef)
            {
                if (baseType.IsGenericTypeDefinition)
                {
                    return true;
                }

                if (iface.GenericTypeArguments.SequenceEqual(baseType.GenericTypeArguments))
                {
                    return true;
                }
            }
        }

        return false;
    }

    [RequiresUnreferencedCode("此方法可能会使用反射构建访问器，与剪裁不兼容。")]
    public static T? ChangeTypeEx<T>(this object? value)
    {
        return ChangeTypeEx(value, typeof(T)) is T t ? t : default;
    }
    /// <summary>类型转换</summary>
    /// <param name="value">数值</param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("此方法可能会使用反射构建访问器，与剪裁不兼容。")]
    public static object? ChangeTypeEx(this object? value, Type conversionType)
    {
        if (value == DBNull.Value)
        {
            value = null;
        }

        Type? vtype = value?.GetType();
        if (vtype == conversionType)
        {
            return value;
        }

        var utype = Nullable.GetUnderlyingType(conversionType);
        if (utype != null)
        {
            if (value == null)
            {
                return null;
            }
            if (value is string s && string.IsNullOrWhiteSpace(s))
            {
                return null;
            }
            if (value is DateTime dt && dt == DateTime.MinValue)
            {
                return null;
            }
            conversionType = utype;
        }

        if (value is byte[] bytes && conversionType == typeof(string))
        {
            return Encoding.UTF8.GetString(bytes);
        }


        if (conversionType.IsEnum)
        {
            if (value is string s)
            {
                if (string.IsNullOrEmpty(s))
                {
                    return Activator.CreateInstance(conversionType);
                }
                return Enum.Parse(conversionType, s, true);
            }
            if (value is IConvertible)
            {
                return Enum.ToObject(conversionType, Convert.ChangeType(value, Enum.GetUnderlyingType(conversionType)));
            }
        }

        var code = Type.GetTypeCode(conversionType);

        if (vtype == typeof(string))
        {
            var str = (string)(value ?? string.Empty);

            if (code == TypeCode.Decimal)
            {
                str = str.TrimStart('$', '￥');
            }

            if (conversionType == typeof(Type))
            {
                return Type.GetType(str);
            }

            if (code >= TypeCode.Int16 && code <= TypeCode.UInt64 && str.Length <= 10)
            {
                return Convert.ChangeType(value.ToLong(), conversionType);
            }

            value = str;
        }

        if (value != null)
        {
            switch (code)
            {
                case TypeCode.Boolean: return value.ToBoolean();
                case TypeCode.DateTime: return value.ToDateTime();
                case TypeCode.Double: return value.ToDouble();
                case TypeCode.Single: return (float)value.ToDouble();
                case TypeCode.Decimal: return value.ToDecimal();
                case TypeCode.Int16: return (short)value.ToInt();
                case TypeCode.Int32: return value.ToInt();
                case TypeCode.Int64: return value.ToLong();
                case TypeCode.UInt16: return (ushort)value.ToInt();
                case TypeCode.UInt32: return (uint)value.ToInt();
                case TypeCode.UInt64: return (ulong)value.ToLong();
                case TypeCode.Byte: return (byte)value.ToInt();
                case TypeCode.SByte: return (sbyte)value.ToInt();
                case TypeCode.Char: return Convert.ToChar(value);
            }

            if (conversionType == typeof(DateTimeOffset))
            {
                return value.ToDateTimeOffset();
            }

            if (value is string str)
            {
                if (conversionType == typeof(Guid))
                {
                    return Guid.TryParse(str, out var g) ? g : Guid.Empty;
                }
                if (conversionType == typeof(TimeSpan))
                {
                    return TimeSpan.TryParse(str, out var ts) ? ts : default;
                }
                if (conversionType == typeof(Version))
                {
                    return Version.TryParse(str, out var ver) ? ver : null;
                }

#if NET6_0_OR_GREATER
                if (conversionType == typeof(IntPtr)) return IntPtr.Parse(str);
                if (conversionType == typeof(UIntPtr)) return UIntPtr.Parse(str);
                if (conversionType == typeof(Half)) return Half.Parse(str);
                if (conversionType == typeof(DateOnly)) return DateOnly.Parse(str);
                if (conversionType == typeof(TimeOnly)) return TimeOnly.Parse(str);
#endif

#if NET8_0_OR_GREATER
                // 支持IParsable<TSelf>接口
                if (conversionType.GetInterfaces().Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IParsable<>)))
                {
                    // 获取 TryParse 静态方法
                    var tryParse = conversionType.GetMethod("TryParse", [typeof(String), typeof(IFormatProvider), conversionType.MakeByRefType()]);
                    if (tryParse != null)
                    {
                        var parameters = new Object?[] { str, null, null };
                        var success = (Boolean)tryParse.Invoke(null, parameters)!;
                        if (success) return parameters[2];
                        //return null;
                    }
                    else
                    {
                        var mi = conversionType.GetMethod("Parse", [typeof(String), typeof(IFormatProvider)]);
                        if (mi != null) return mi.Invoke(null, [value, null]);
                    }
                }
#endif
            }

            if (value is IConvertible)
            {
                return Convert.ChangeType(value, conversionType);
            }
        }
        else if (conversionType.IsValueType)
        {
            // 如果原始值是null，要转为值类型，则new一个空白的返回
            return Activator.CreateInstance(conversionType);
        }

        return value;
    }


    #region 反射获取 字段/属性
    private static readonly NonBlockingDictionary<Type, IList<FieldInfo>> _cache1 = new();
    private static readonly NonBlockingDictionary<Type, IList<FieldInfo>> _cache2 = new();
    /// <summary>获取字段</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    public static IList<FieldInfo> GetFieldsEx([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] this Type type, Boolean baseFirst = true)
    {
        if (baseFirst)
            return _cache1.GetOrAdd(type, key => GetFields2(key, true));
        else
            return _cache2.GetOrAdd(type, key => GetFields2(key, false));
    }

    private static List<FieldInfo> GetFields2([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, Boolean baseFirst)
    {
        var list = new List<FieldInfo>();

        // Void*的基类就是null
        if (type == typeof(Object) || type.BaseType == null) return list;

        if (baseFirst) list.AddRange(GetFieldsEx(type.BaseType));

        var fis = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var fi in fis)
        {
            if (fi.GetCustomAttribute<NonSerializedAttribute>() != null) continue;

            list.Add(fi);
        }

        if (!baseFirst) list.AddRange(GetFieldsEx(type.BaseType));

        return list;
    }

    private static readonly NonBlockingDictionary<Type, IList<PropertyInfo>> _cache3 = new();
    private static readonly NonBlockingDictionary<Type, IList<PropertyInfo>> _cache4 = new();
    /// <summary>获取属性</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    public static IList<PropertyInfo> GetPropertiesEx([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] this Type type, Boolean baseFirst = true)
    {
        if (baseFirst)
            return _cache3.GetOrAdd(type, key => GetProperties2(key, true));
        else
            return _cache4.GetOrAdd(type, key => GetProperties2(key, false));
    }

    private static List<PropertyInfo> GetProperties2([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, Boolean baseFirst)
    {
        var list = new List<PropertyInfo>();

        // Void*的基类就是null
        if (type == typeof(Object) || type.BaseType == null) return list;

        // 本身type.GetProperties就可以得到父类属性，只是不能保证父类属性在子类属性之前
        if (baseFirst) list.AddRange(GetPropertiesEx(type.BaseType));

        // 父类子类可能因为继承而有重名的属性，此时以子类优先，否则反射父类属性会出错
        var set = new HashSet<String>(list.Select(e => e.Name));

        //var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var pi in pis)
        {
            if (pi.GetIndexParameters().Length > 0) continue;
            if (pi.GetCustomAttribute<XmlIgnoreAttribute>() != null) continue;
            if (pi.GetCustomAttribute<ScriptIgnoreAttribute>() != null) continue;
            if (pi.GetCustomAttribute<IgnoreDataMemberAttribute>() != null) continue;

            if (!set.Contains(pi.Name))
            {
                list.Add(pi);
                set.Add(pi.Name);
            }
        }

        // 获取用于序列化的属性列表时，加上非公有的数据成员
        pis = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var pi in pis)
        {
            if (pi.GetIndexParameters().Length > 0) continue;
            if (pi.GetCustomAttribute<XmlElementAttribute>() == null && pi.GetCustomAttribute<DataMemberAttribute>() == null) continue;

            if (!set.Contains(pi.Name))
            {
                list.Add(pi);
                set.Add(pi.Name);
            }
        }

        if (!baseFirst) list.AddRange(GetPropertiesEx(type.BaseType).Where(e => !set.Contains(e.Name)));

        return list;
    }
    #endregion


    /// <summary>
    /// 反射创建指定类型的实例。
    /// </summary>
    /// <param name="type">要创建的类型。</param>
    /// <param name="parameters">构造函数参数。</param>
    /// <returns>新实例对象。</returns>
    /// <exception cref="MissingMemberException">类型为抽象类或接口时抛出。</exception>
    /// <exception cref="Exception">创建失败时抛出。</exception>
    [RequiresUnreferencedCode("此方法可能会使用反射构建访问器，与剪裁不兼容。")]
    [RequiresDynamicCode("此方法可能会使用MakeGenericType动态生成类型，与剪裁/AOT不兼容。")]
    public static object? CreateInstance(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] this Type type,
    params object?[]? parameters)
    {
        try
        {
            var code = Type.GetTypeCode(type);

            // IList / IDictionary 自动替代
            if (type.IsInterface || type.IsAbstract)
            {
                var impl = MapToConcreteType(type);
                if (impl != null)
                {
                    return Activator.CreateInstance(impl);
                }
            }

            // 无参数构造
            if (parameters == null || parameters.Length == 0)
            {
                return code switch
                {
                    TypeCode.Boolean => false,
                    TypeCode.Char => '\0',
                    TypeCode.SByte => (sbyte)0,
                    TypeCode.Byte => (byte)0,
                    TypeCode.Int16 => (short)0,
                    TypeCode.UInt16 => (ushort)0,
                    TypeCode.Int32 => 0,
                    TypeCode.UInt32 => 0U,
                    TypeCode.Int64 => 0L,
                    TypeCode.UInt64 => 0UL,
                    TypeCode.Single => 0F,
                    TypeCode.Double => 0D,
                    TypeCode.Decimal => 0M,
                    TypeCode.DateTime => DateTime.MinValue,
                    TypeCode.String => string.Empty,
                    _ => Activator.CreateInstance(type, true),
                };
            }

            // 有参数构造
            if (parameters.Length == 1 && parameters[0] == null)
            {
                return Activator.CreateInstance(type);
            }

            return Activator.CreateInstance(type, parameters);
        }
        catch (Exception ex)
        {
            string SafeToString(object? o)
            {
                try { return o?.ToString() ?? "null"; }
                catch { return $"<{o?.GetType().Name ?? "null"}>"; }
            }

            var paramInfo = parameters == null ? "null" :
                string.Join(", ", parameters.Select(SafeToString));

            throw new Exception(
                $"Fail to create instance of {type.FullName} with parameters [{paramInfo}]: {ex.GetTrueException()?.Message}",
                ex);
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        static Type? MapToConcreteType(Type type)
        {
            if (type.IsGenericType)
            {
                var def = type.GetGenericTypeDefinition();
                var args = type.GetGenericArguments();

                if (def == typeof(IDictionary<,>))
                {
                    return typeof(Dictionary<,>).MakeGenericType(args);
                }
                if (def == typeof(IReadOnlyDictionary<,>))
                {
                    return typeof(Dictionary<,>).MakeGenericType(args);
                }
                if (def == typeof(IList<>))
                {
                    return typeof(List<>).MakeGenericType(args);
                }
                if (def == typeof(ICollection<>))
                {
                    return typeof(List<>).MakeGenericType(args);
                }
                if (def == typeof(IEnumerable<>))
                {
                    return typeof(List<>).MakeGenericType(args);
                }
            }

            // 非泛型接口的一些常见映射
            if (type == typeof(IDictionary))
            {
                return typeof(Hashtable);
            }
            if (type == typeof(IList))
            {
                return typeof(ArrayList);
            }
            if (type == typeof(IEnumerable))
            {
                return typeof(ArrayList);
            }

            return null; // 没有匹配到
        }

    }

    /// <summary>获取一个类型的元素类型</summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public static Type? GetElementTypeEx([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type)
    {
        if (type.HasElementType) return type.GetElementType();

        if (type.IsFrom<IEnumerable>())
        {
            // 如果实现了IEnumerable<>接口，那么取泛型参数
            foreach (var item in type.GetInterfaces())
            {
                if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return item.GetGenericArguments()[0];
            }
            //// 通过索引器猜测元素类型
            //var pi = type.GetProperty("Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            //if (pi != null) return pi.PropertyType;
        }

        return null;
    }

    /// <summary>把一个方法转为泛型委托，便于快速反射调用</summary>
    /// <typeparam name="TFunc"></typeparam>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static TFunc? As<TFunc>(this MethodInfo method, object? target = null) where TFunc : class
    {
        if (method == null)
        {
            return default;
        }

        var key = new DelegateCacheKey(method, typeof(TFunc), target);

        var func = DelegateCache<TFunc>.Cache.GetOrAdd(
             key,
             _ =>
             {
                 return target == null
                         ? Delegate.CreateDelegate(typeof(TFunc), method, true) as TFunc
                         : Delegate.CreateDelegate(typeof(TFunc), target, method, true) as TFunc;
             });

        return func;
    }
    /// <summary>把一个方法转为泛型委托，便于快速反射调用</summary>
    /// <typeparam name="TFunc"></typeparam>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static void RemoveCache<TFunc>(this MethodInfo method, object? target = null) where TFunc : class
    {
        if (method == null)
        {
            return;
        }

        var key = new DelegateCacheKey(method, typeof(TFunc), target);

        DelegateCache<TFunc>.Cache.TryRemove(key);
    }
}
public static class DelegateCache<TFunc>
{
    public static readonly ExpiringDictionary<DelegateCacheKey, TFunc> Cache = new(comparer: EqualityComparer<DelegateCacheKey>.Default, tryDispose: false);
}
public readonly struct DelegateCacheKey : IEquatable<DelegateCacheKey>
{
    public readonly MethodInfo Method;
    public readonly Type FuncType;
    public readonly object? Target;

    public DelegateCacheKey(MethodInfo method, Type funcType, object? target)
    {
        Method = method;
        FuncType = funcType;
        Target = target;
    }

    public bool Equals(DelegateCacheKey other) =>
        Method.Equals(other.Method)
        && FuncType.Equals(other.FuncType)
        && ReferenceEquals(Target, other.Target);

    public override bool Equals(object? obj) =>
        obj is DelegateCacheKey other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = Method.GetHashCode();
            hash = (hash * 397) ^ FuncType.GetHashCode();
            if (Target != null)
            {
                hash = (hash * 397) ^ RuntimeHelpers.GetHashCode(Target);
            } // 不受对象重写 GetHashCode 影响
            return hash;
        }
    }

    public static bool operator ==(DelegateCacheKey left, DelegateCacheKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DelegateCacheKey left, DelegateCacheKey right)
    {
        return !(left == right);
    }
}