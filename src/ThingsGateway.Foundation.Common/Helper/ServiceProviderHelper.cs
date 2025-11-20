using System.Reflection;

namespace ThingsGateway.Foundation.Common.Extension;

internal static class ServiceProviderHelper
{
    /// <summary>获取指定类型的服务对象</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static T? GetService<T>(this IServiceProvider provider)
    {
        if (provider == null) return default;

        //// 服务类是否当前类的基类
        //if (provider.GetType().As<T>()) return (T)provider;

        return (T?)provider.GetService(typeof(T));
    }

    /// <summary>获取必要的服务，不存在时抛出异常</summary>
    /// <param name="provider">服务提供者</param>
    /// <param name="serviceType">服务类型</param>
    /// <returns></returns>
    public static Object GetRequiredService(this IServiceProvider provider, Type serviceType)
    {
        ArgumentNullExceptionEx.ThrowIfNull(provider);
        ArgumentNullExceptionEx.ThrowIfNull(serviceType);

        return provider.GetService(serviceType) ?? throw new InvalidOperationException($"Unregistered type {serviceType.FullName}");
    }

    /// <summary>获取必要的服务，不存在时抛出异常</summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <param name="provider">服务提供者</param>
    /// <returns></returns>
    public static T GetRequiredService<T>(this IServiceProvider provider) => provider == null ? throw new ArgumentNullException(nameof(provider)) : (T)provider.GetRequiredService(typeof(T));

    /// <summary>创建服务对象，使用服务提供者来填充构造函数</summary>
    /// <param name="provider">服务提供者</param>
    /// <param name="serviceType">服务类型</param>
    /// <returns></returns>
    public static Object? CreateInstance(this IServiceProvider provider, Type serviceType) => CreateInstance(serviceType, provider, null, false);

    private static Dictionary<TypeCode, Object?>? _defs;

    public static Object? CreateInstance(Type type, IServiceProvider provider, Func<IServiceProvider, Object>? factory, Boolean throwOnError)
    {
        if (factory != null) return factory(provider);

        // 初始化
        if (_defs == null)
        {
            var dic = new Dictionary<TypeCode, Object?>
            {
                { TypeCode.Empty, null },
                { TypeCode.DBNull, null},
                { TypeCode.Boolean, false },
                { TypeCode.Char, (Char)0 },
                { TypeCode.SByte, (SByte)0 },
                { TypeCode.Byte, (Byte)0 },
                { TypeCode.Int16, (Int16)0 },
                { TypeCode.UInt16, (UInt16)0 },
                { TypeCode.Int32, (Int32)0 },
                { TypeCode.UInt32, (UInt32)0 },
                { TypeCode.Int64, (Int64)0 },
                { TypeCode.UInt64, (UInt64)0 },
                { TypeCode.Single, (Single)0 },
                { TypeCode.Double, (Double)0 },
                { TypeCode.Decimal, (Decimal)0 },
                { TypeCode.DateTime, DateTime.MinValue },
                { TypeCode.String, null }
            };

            _defs = dic;
        }

        ParameterInfo? errorParameter = null;
        if (!type.IsAbstract)
        {
            // 选择构造函数，优先选择参数最多的可匹配构造函数
            var constructors = type.GetConstructors();
            foreach (var constructorInfo in constructors.OrderByDescending(e => e.GetParameters().Length))
            {
                if (constructorInfo.IsStatic) continue;

                ParameterInfo? errorParameter2 = null;
                var ps = constructorInfo.GetParameters();
                var pv = new Object?[ps.Length];
                for (var i = 0; i != ps.Length; i++)
                {
                    if (pv[i] != null) continue;

                    var ptype = ps[i].ParameterType;
                    if (_defs.TryGetValue(Type.GetTypeCode(ptype), out var obj))
                        pv[i] = obj;
                    else
                    {
                        var service = provider.GetService(ps[i].ParameterType);
                        if (service == null)
                        {
                            errorParameter2 = ps[i];

                            break;
                        }
                        else
                        {
                            pv[i] = service;
                        }
                    }
                }

                if (errorParameter2 == null) return constructorInfo.Invoke(pv);
                errorParameter = errorParameter2;
            }
        }

        if (throwOnError)
            throw new InvalidOperationException($"No suitable constructor was found for '{type}'. Please confirm that all required parameters for the type constructor are registered. Unable to parse parameter '{errorParameter}'");

        return null;
    }

}