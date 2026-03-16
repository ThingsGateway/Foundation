namespace ThingsGateway.Foundation.Common;

/// <summary>
/// 池化，保留借出的对象，清空时Dispose销毁
/// </summary>
/// <typeparam name="T"></typeparam>
public class DisposeObjectPool<T> : DisposeBase where T : class
{
    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; }

    /// <summary>空闲个数</summary>
    public Int32 FreeCount => _free.Count;

    /// <summary>繁忙个数</summary>
    public Int32 BusyCount => _busy.Count;

    /// <summary>基础空闲集合。只保存最小个数，最热部分</summary>
    private readonly Stack<T> _free = new();

    /// <summary>借出去的放在这</summary>
    private readonly HashSet<T> _busy = new();

    #endregion

    #region 构造
    /// <summary>实例化一个资源池</summary>
    public DisposeObjectPool()
    {
        Name = $"DisposeObjectPool<{typeof(T).Name}>";
    }
    ~DisposeObjectPool()
    {
        this.TryDispose();
    }
    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);
        Clear();
    }

    #endregion

    #region 主方法
    /// <summary>借出</summary>
    /// <returns></returns>
    public virtual T? Get()
    {
        T? pi = null;
        {
            lock (lockThis)
            {
                if (_free.Count > 0)
                {
                    pi = _free.Pop();
                }
                else
                {
                    pi = null;
                }
            }

            // 如果拿到的对象不可用，则重新借
        }
        if (pi == null)
        {
            return null;
        }
        lock (lockThis)
        {
            // 加入繁忙集合
            _busy.Add(pi);
        }
        return pi;
    }

    /// <summary>借出时是否可用</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual Boolean OnGet(T value) => true;

    /// <summary>归还</summary>
    /// <param name="value"></param>
    public virtual Boolean Return(T value)
    {
        if (value == null) return false;
        lock (lockThis)
        {
            _busy.Remove(value);

            // 是否可用
            if (!OnReturn(value))
            {
                return false;
            }

            if (value is IDisposableValue db && db.DisposedValue)
            {
                return false;
            }
            _free.Push(value);
        }

        return true;
    }

    /// <summary>归还时是否可用</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual Boolean OnReturn(T value) => true;

    /// <summary>清空已有对象</summary>
    public virtual Int32 Clear()
    {
        lock (lockThis)
        {
            var count = _free.Count + _busy.Count;
            while (_free.Count > 0)
            {
                var pi = _free.Pop();
                OnDispose(pi);
            }

            foreach (var item in _busy)
            {
                OnDispose(item);
            }
            _busy.Clear();
            return count;
        }

    }

    /// <summary>销毁</summary>
    /// <param name="value"></param>
    protected virtual void OnDispose(T? value) => value.TryDispose();
    #endregion
#if NET9_0_OR_GREATER
    protected Lock lockThis = new();
#else
    protected object lockThis = new();
#endif
}
