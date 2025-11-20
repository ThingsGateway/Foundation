namespace ThingsGateway.Foundation.Common;

public class ObjectPoolT<T> : DisposeBase where T : class
{
    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; }

    private Int32 _FreeCount;
    /// <summary>空闲个数</summary>
    public Int32 FreeCount => _FreeCount;

    private Int32 _BusyCount;
    /// <summary>繁忙个数</summary>
    public Int32 BusyCount => _BusyCount;

    /// <summary>基础空闲集合。只保存最小个数，最热部分</summary>
    private readonly Stack<T> _free = new();

    /// <summary>借出去的放在这</summary>
    private readonly HashSet<T> _busy = new();

    #endregion

    #region 构造
    /// <summary>实例化一个资源池</summary>
    public ObjectPoolT()
    {
        Name = $"Pool<{typeof(T).Name}>";
    }
    ~ObjectPoolT()
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
                    _FreeCount--;
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

            _BusyCount++;
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
            _ = _busy.Remove(value);
            // 从繁忙队列找到并移除缓存项
            //if (!_busy.Remove(value))
            //{
            //    return false;
            //}

            _BusyCount--;
        }

        // 是否可用
        if (!OnReturn(value))
        {
            return false;
        }

        if (value is IDisposableValue db && db.DisposedValue)
        {
            return false;
        }
        lock (lockThis)
        {
            _free.Push(value);
            _FreeCount++;
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
            var count = _FreeCount + _BusyCount;

            while (_free.Count > 0)
            {
                var pi = _free.Pop();
                OnDispose(pi);
            }

            _FreeCount = 0;

            foreach (var item in _busy)
            {
                OnDispose(item);
            }
            _busy.Clear();
            _BusyCount = 0;
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
