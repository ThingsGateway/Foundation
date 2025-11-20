using System.Runtime.InteropServices;

using ThingsGateway.Foundation.Common.Log;

namespace ThingsGateway.Foundation.Common;



public static class Host
{
    static Host()
    {
        AppDomain.CurrentDomain.ProcessExit += OnExit;
        Console.CancelKeyPress += OnExit;
#if NETCOREAPP
        System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += ctx => OnExit(ctx, EventArgs.Empty);
#endif
#if NET6_0_OR_GREATER
#pragma warning disable CA2000 // 丢失范围之前释放对象
        _ = PosixSignalRegistration.Create(PosixSignal.SIGINT, ctx => OnExit(ctx, EventArgs.Empty));
        _ = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, ctx => OnExit(ctx, EventArgs.Empty));
        _ = PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx => OnExit(ctx, EventArgs.Empty));
#pragma warning restore CA2000 // 丢失范围之前释放对象
#endif
    }

    #region 退出事件
    private static readonly List<Action> _events2 = [];
    private static Int32 _exited;

    /// <summary>注册应用退出事件。仅执行一次</summary>
    /// <param name="onExit">回调函数</param>
    public static void RegisterExit(Action onExit) => _events2.Add(onExit);

    private static void OnExit(Object? sender, EventArgs e)
    {
        // 只执行一次
        if (Interlocked.Increment(ref _exited) > 1) return;

        foreach (var item in _events2)
        {
            try
            {
                item();
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
        }
    }
    #endregion
}

