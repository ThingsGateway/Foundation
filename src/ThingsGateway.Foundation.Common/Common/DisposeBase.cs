using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using ThingsGateway.Foundation.Common.Log;

namespace ThingsGateway.Foundation.Common;
/// <summary>具有是否已释放的接口</summary>
public interface IDisposableValue
{
    /// <summary>是否已经释放</summary>
    [XmlIgnore, IgnoreDataMember]
    Boolean DisposedValue { get; }
}
/// <summary>具有是否已释放和释放后事件的接口</summary>
public interface IDisposable2 : IDisposable, IDisposableValue
{
}

/// <summary>具有销毁资源处理的抽象基类</summary>
public abstract class DisposeBase : IDisposable2
{
    #region 释放资源
    /// <summary>释放资源</summary>
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
        Dispose(true);

        // 告诉GC，不要调用析构函数
        GC.SuppressFinalize(this);
    }

    [NonSerialized]
    private Int32 _disposed = 0;
    /// <summary>是否已经释放</summary>
    [XmlIgnore, IgnoreDataMember]
    public Boolean DisposedValue => _disposed > 0;

    /// <summary>释放资源，参数表示是否由Dispose调用。重载时先调用基类方法</summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(Boolean disposing)
    {

        if (disposing)
        {

            // 告诉GC，不要调用析构函数
            GC.SuppressFinalize(this);
        }

    }

    /// <summary>析构函数</summary>
    /// <remarks>
    /// 如果忘记调用Dispose，这里会释放非托管资源。
    /// 如果曾经调用过Dispose，因为GC.SuppressFinalize(this)，不会再调用该析构函数。
    /// 在 .NET 中，析构函数（Finalizer）不应该抛出未捕获的异常。如果析构函数引发未捕获的异常，它将导致应用程序崩溃或进程退出。
    /// </remarks>
    ~DisposeBase()
    {
        // 在 .NET 中，析构函数（Finalizer）不应该抛出未捕获的异常。如果析构函数引发未捕获的异常，它将导致应用程序崩溃或进程退出。
        try
        {
            Dispose(false);
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
    }
    #endregion
}

/// <summary>销毁助手。扩展方法专用</summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class DisposeHelper
{
    /// <summary>尝试销毁对象，如果有<see cref="IDisposable"/>则调用</summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Object? TryDispose(this Object? obj)
    {
        if (obj == null) return obj;

        // 列表元素销毁
        if (obj is IEnumerable ems)
        {
            // 对于枚举成员，先考虑添加到列表，再逐个销毁，避免销毁过程中集合改变
            if (obj is not IList list)
            {
                list = new List<Object>();
                foreach (var item in ems)
                {
                    if (item is IDisposable) list.Add(item);
                }
            }
            foreach (var item in list)
            {
                if (item is IDisposable disp)
                {
                    try
                    {
                        //(item as IDisposable).TryDispose();
                        // 只需要释放一层，不需要递归
                        // 因为一般每一个对象负责自己内部成员的释放
                        disp.Dispose();
                    }
                    catch { }
                }
            }
        }
        // 对象销毁
        if (obj is IDisposable disp2)
        {
            try
            {
                disp2.Dispose();
            }
            catch { }
        }

        return obj;
    }
}