using System.Collections;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;

namespace ThingsGateway.Foundation.Common;


public static partial class Runtime
{
    #region 静态构造
    static Runtime()
    {
        try
        {
            Mono = Type.GetType("Mono.Runtime") != null;
        }
        catch { }
        try
        {
            Unity = Type.GetType("UnityEngine.Application, UnityEngine") != null;
        }
        catch { }
    }
    #endregion

    #region 控制台
    private static Boolean? _IsConsole;
    /// <summary>是否控制台。用于判断是否可以执行一些控制台操作。</summary>
    public static Boolean IsConsole
    {
        get
        {
            if (_IsConsole != null) return _IsConsole.Value;

            // netcore 默认都是控制台，除非主动设置
            _IsConsole = true;

            try
            {
                var flag = Console.ForegroundColor;
                if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
                    _IsConsole = false;
                else
                    _IsConsole = true;
            }
            catch
            {
                _IsConsole = false;
            }

            return _IsConsole.Value;
        }
        set => _IsConsole = value;
    }

    /// <summary>是否在容器中运行</summary>
    public static Boolean Container = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
    #endregion

    #region 系统特性

#if NET8_0_OR_GREATER
    private static Boolean? aot;
    public static Boolean Aot
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        get
        {
            if (aot == null)
            {
                aot = new StackTrace(false).GetFrame(0)?.GetMethod() is null || !System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeCompiled;
            }
            return aot.Value;
        }
    }

#endif

    public static bool WebEnable => Environment.GetEnvironmentVariable(nameof(WebEnable)).ToBoolean(true);

    /// <summary>是否Mono环境</summary>
    public static Boolean Mono { get; }

    /// <summary>是否Unity环境</summary>
    public static Boolean Unity { get; }

#if !NETFRAMEWORK
    private static Boolean? _IsWeb;
    /// <summary>是否Web环境</summary>
    public static Boolean IsWeb
    {
        get
        {
            if (_IsWeb == null)
            {
                try
                {
                    var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.GetName().Name == "Microsoft.AspNetCore");
                    _IsWeb = asm != null;
                }
                catch
                {
                    _IsWeb = false;
                }
            }

            return _IsWeb.Value;
        }
    }

    /// <summary>是否Windows环境</summary>
    public static Boolean Windows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>是否Linux环境</summary>
    public static Boolean Linux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>是否OSX环境</summary>
    public static Boolean OSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

#if NET6_0_OR_GREATER


    public static Boolean IsSystemd
    {
        get
        {
            var id = Environment.GetEnvironmentVariable("INVOCATION_ID");
            return !string.IsNullOrEmpty(id);
        }
    }


    public static Boolean? isLegacyWindows;

    /// <summary>
    /// 判断是否老系统 (Vista/2008/7/2008R2)
    /// </summary>
    public static Boolean IsLegacyWindows
    {
        get
        {
            if (isLegacyWindows != null) return isLegacyWindows.Value;

            if (Windows == false)
            {
                isLegacyWindows = false;
                return isLegacyWindows.Value;
            }
            var version = Environment.OSVersion.Version;

            // 如果能拿到真实的 6.x 就直接判断
            if (version.Major == 6 && version.Minor <= 1)
            {
                isLegacyWindows = true;
                return isLegacyWindows.Value;
            }
            if (version.Major < 6)
            {
                isLegacyWindows = true;
                return isLegacyWindows.Value;
            }

            // 如果拿到的是 10.0（Win8.1 之后有虚拟化问题），用 OSDescription 来兜底
            var desc = RuntimeInformation.OSDescription;
            // desc 示例: "Microsoft Windows 6.1.7601" (Win7/2008R2)
            if (desc.Contains("Windows 6.0") || desc.Contains("Windows 6.1"))
            {
                isLegacyWindows = true;
                return isLegacyWindows.Value;
            }
            isLegacyWindows = false;
            return isLegacyWindows.Value;
        }

    }
#endif

#else

    /// <summary>是否Windows环境</summary>
    public static Boolean Windows { get; } = Environment.OSVersion.Platform <= PlatformID.WinCE;

    /// <summary>是否Linux环境</summary>
    public static Boolean Linux { get; } = Environment.OSVersion.Platform == PlatformID.Unix;

    /// <summary>是否OSX环境</summary>
    public static Boolean OSX { get; } = Environment.OSVersion.Platform == PlatformID.MacOSX;


#endif
    #endregion

    #region 扩展

    public static Int64 AppStartTick = TickCount64;

    /// <summary>软件启动以来的毫秒数</summary>
    public static Int64 AppTickCount64 => TickCount64 - AppStartTick;

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>系统启动以来的毫秒数</summary>
    public static Int64 TickCount64 => Environment.TickCount64;
#else
    /// <summary>系统启动以来的毫秒数</summary>
    public static Int64 TickCount64
    {
        get
        {
            if (Stopwatch.IsHighResolution) return Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency;

            return Environment.TickCount;
        }
    }
#endif



    /// <summary>获取当前UTC时间。基于全局时间提供者，在星尘应用中会屏蔽服务器时间差</summary>
    /// <returns></returns>
    public static DateTimeOffset UtcNow => TimerScheduler.GlobalTimeProvider.GetUtcNow();

    private static Int32 _ProcessId;
#if NET6_0_OR_GREATER
    /// <summary>当前进程Id</summary>
    public static Int32 ProcessId => _ProcessId > 0 ? _ProcessId : _ProcessId = Environment.ProcessId;
#else
    /// <summary>当前进程Id</summary>
    public static Int32 ProcessId => _ProcessId > 0 ? _ProcessId : _ProcessId = ProcessHelper.GetProcessId();
#endif

    /// <summary>
    /// 获取环境变量。不区分大小写
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
    public static String? GetEnvironmentVariable(String variable)
    {
        var val = Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrEmpty(val)) return val;

        foreach (var item in Environment.GetEnvironmentVariables())
        {
            if (item is DictionaryEntry de)
            {
                var key = de.Key as String;
                if (key.EqualIgnoreCase(variable)) return de.Value as String;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取环境变量集合。不区分大小写
    /// </summary>
    /// <returns></returns>
    public static IDictionary<String, String?> GetEnvironmentVariables()
    {
        var dic = new Dictionary<String, String?>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in Environment.GetEnvironmentVariables())
        {
            if (item is not DictionaryEntry de) continue;

            var key = de.Key as String;
            if (!string.IsNullOrEmpty(key)) dic[key!] = de.Value as String;
        }

        return dic;
    }
    #endregion

    #region 设置
    private static Boolean? _createConfigOnMissing;
    /// <summary>默认配置。配置文件不存在时，是否生成默认配置文件</summary>
    public static Boolean CreateConfigOnMissing
    {
        get
        {
            if (_createConfigOnMissing == null)
            {
                var val = Environment.GetEnvironmentVariable("CreateConfigOnMissing");
                _createConfigOnMissing = !string.IsNullOrEmpty(val) ? val.ToBoolean(false) : false;
            }

            return _createConfigOnMissing.Value;
        }
        set { _createConfigOnMissing = value; }
    }
    #endregion

    #region 内存
    /// <summary>释放内存。GC回收后再释放虚拟内存</summary>
    /// <param name="gc">是否GC回收</param>
    /// <param name="workingSet">是否释放工作集</param>
    public static Boolean FreeMemory(Boolean gc = true, Boolean workingSet = true)
    {

        var p = Process.GetProcessById(ProcessId);
        if (p == null) return false;

        if (gc)
        {
            var max = GC.MaxGeneration;
            var mode = GCCollectionMode.Forced;
#if NET8_0_OR_GREATER
            mode = GCCollectionMode.Aggressive;
#endif
#if NET451_OR_GREATER || NETSTANDARD || NETCOREAPP
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
#endif
            GC.Collect(max, mode);
            GC.WaitForPendingFinalizers();
            GC.Collect(max, mode);
        }

        if (workingSet)
        {
            if (Runtime.Windows)
            {
                try
                {
                    EmptyWorkingSet(p.Handle);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        return true;
    }
#if NETFRAMEWORK || NETSTANDARD || NET6_0
    [DllImport("psapi.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern Boolean EmptyWorkingSet(IntPtr hProcess);
#else
    [LibraryImport("psapi.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial Boolean EmptyWorkingSet(IntPtr hProcess);
#endif
    #endregion
}