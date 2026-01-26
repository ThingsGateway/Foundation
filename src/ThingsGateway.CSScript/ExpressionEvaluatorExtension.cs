//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using CSScripting;

using CSScriptLib;

using System.Text;

using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Common.Caching;

namespace ThingsGateway.Gateway.Application.Extensions;

/// <summary>
/// 读写表达式脚本
/// </summary>
public abstract class ReadWriteExpressions
{
    public WeakReference<TouchSocket.Core.ILog> Log { get; set; }
    public TouchSocket.Core.ILog? Logger => Log.TryGetTarget(out var log) ? log : null;

    /// <summary>
    /// 获取新值
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public abstract object GetNewValue(object a);
}

/// <summary>
/// 表达式扩展
/// </summary>
public static class ExpressionEvaluatorExtension
{
    private static readonly object m_waiterLock = new object();

    static ExpressionEvaluatorExtension()
    {
        var temp = Environment.GetEnvironmentVariable("CSS_CUSTOM_TEMPDIR");
        if (string.IsNullOrWhiteSpace(temp))
        {
            var tempDir = Path.Combine(AppContext.BaseDirectory, "CSSCRIPT");
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                }
            }

            Directory.CreateDirectory(tempDir);//重新创建，防止缓存的一些目录信息错误
            Environment.SetEnvironmentVariable("CSS_CUSTOM_TEMPDIR", tempDir); //传入变量
        }

        Instance.KeyExpired += Instance_KeyExpired;
    }

    private static void Instance_KeyExpired(object sender, KeyEventArgs e)
    {
        try
        {
            if (Instance.GetAll().TryGetValue(e.Key, out var item))
            {
                item?.Value?.TryDispose();
                item?.Value?.GetType().Assembly.Unload();
            }
        }
        catch
        {
        }
    }

    private static MemoryCache Instance { get; set; } = new MemoryCache();

    /// <summary>
    /// 添加或获取脚本，非线程安全
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static ReadWriteExpressions GetOrAddScript(string source)
    {
        var field = source;
        var runScript = Instance.Get<ReadWriteExpressions>(field);
        if (runScript == null)
        {
            var hasValue = Instance.TryGetValue<ReadWriteExpressions>(field, out runScript);
            if (!hasValue)
            {


                if (!source.Contains("return"))
                {
                    source = $"return {source}";//只判断简单脚本中可省略return字符串
                }
                var src = source.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                using var _using = new ValueStringBuilder();
                using var _body = new ValueStringBuilder();
                foreach (var item in src)
                {
                    if (item.StartsWith("using "))
                    {
                        _using.AppendLine(item);
                    }
                    else
                    {
                        _body.AppendLine(item);
                    }
                }
                // 动态加载并执行代码
                try
                {
                    runScript = CSScript.Evaluator.With(eval => eval.IsAssemblyUnloadingEnabled = true).LoadCode<ReadWriteExpressions>(
$@"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Linq;
        using ThingsGateway.Gateway.Application;
        using ThingsGateway.Foundation.Common.StringExtension;
        using ThingsGateway.Foundation.Common;
        using ThingsGateway.Foundation.Common.Extension;
        using ThingsGateway.Foundation.Common.Json.Extension;
        using ThingsGateway.Gateway.Application.Extensions;
        {_using.ToString()}
        public class Script:ReadWriteExpressions
        {{
            public override object GetNewValue(object raw)
            {{
                   {_body.ToString()};
            }}
        }}
    ");
                    Instance.Set(field, runScript);
                }
                catch (Exception ex)
                {
                    //如果编译失败，应该不重复编译，避免oom
                    Instance.Set<ReadWriteExpressions>(field, null, TimeSpan.FromHours(1));
                    var exfield = $"Exception-{source}";
                    Instance.Set(exfield, ex, TimeSpan.FromHours(1));
                    throw;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }

        Instance.SetExpire(field, TimeSpan.FromHours(1));
        if (runScript == null)
        {
            var exfield = $"Exception-{source}";
            throw (Instance.Get<Exception>(exfield) ?? new Exception("compilation error"));
        }
        return runScript;
    }

    /// <summary>
    /// 计算表达式：例如：(int)raw*100，raw为原始值
    /// </summary>
    public static object GetExpressionsResult(this string expressions, object rawvalue)
    {
        if (string.IsNullOrWhiteSpace(expressions))
        {
            return rawvalue;
        }
        var readWriteExpressions = GetReadWriteExpressions(expressions);
        var value = readWriteExpressions.GetNewValue(rawvalue);
        return value;
    }

    /// <summary>
    /// 计算表达式：例如：(int)raw*100，raw为原始值
    /// </summary>
    public static object GetExpressionsResult(this string expressions, object rawvalue, TouchSocket.Core.ILog logger)
    {
        if (string.IsNullOrWhiteSpace(expressions))
        {
            return rawvalue;
        }
        var readWriteExpressions = GetReadWriteExpressions(expressions);
        readWriteExpressions.Log = new WeakReference<TouchSocket.Core.ILog>(logger);
        var value = readWriteExpressions.GetNewValue(rawvalue);
        return value;
    }

    /// <summary>
    /// 执行脚本获取返回值ReadWriteExpressions
    /// </summary>
    public static ReadWriteExpressions GetReadWriteExpressions(string source)
    {
        var field = source;
        var runScript = Instance.Get<ReadWriteExpressions>(field);
        if (runScript == null)
        {
            lock (m_waiterLock)
            {
                runScript = GetOrAddScript(source);
            }
        }
        Instance.SetExpire(field, TimeSpan.FromHours(1));

        return runScript;
    }
    public static void SetExpire(string source, TimeSpan? timeSpan = null)
    {
        var field = source;
        Instance.SetExpire(field, timeSpan ?? TimeSpan.FromHours(1));
    }
}
