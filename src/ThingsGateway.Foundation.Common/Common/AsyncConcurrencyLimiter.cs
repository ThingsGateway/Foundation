//using System.Runtime.CompilerServices;
//using System.Threading.Tasks.Sources;
//using ThingsGateway.Foundation.Common;

//public sealed class WaitLock : DisposeBase
//{
//    const int MaxPoolSize = 1024;
//    public readonly int MaxCount;
//    private int _currentCount;

//    private readonly Queue<Waiter> _waiters = new();
//    private readonly Stack<Waiter> _pool = new();

//#if NET9_0_OR_GREATER
//    private readonly Lock _lock = new();
//#else
//    private readonly object _lock = new();
//#endif
//    public WaitLock(int maxCount)
//    {
//        if (maxCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxCount));
//        MaxCount = maxCount;
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public ValueTask WaitAsync()
//    {
//        if (DisposedValue)
//        {
//            throw new ObjectDisposedException(nameof(WaitLock));
//        }
//        lock (_lock)
//        {
//            if (DisposedValue)
//            {
//                throw new ObjectDisposedException(nameof(WaitLock));
//            }
//            if (_currentCount < MaxCount)
//            {
//                _currentCount++;
//                return default;
//            }

//            var w = RentWaiter();
//            _waiters.Enqueue(w);
//            return new ValueTask(w, w.Version);
//        }
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void Release()
//    {
//        if (DisposedValue)
//        {
//            return;
//        }
//        Waiter? w = null;

//        lock (_lock)
//        {
//            if (_waiters.Count > 0)
//            {
//                w = _waiters.Dequeue();
//            }
//            else
//            {
//                if (_currentCount > 0)
//                    _currentCount--;
//                return;
//            }
//        }

//        // 放在锁外完成
//        w.SetResult();
//    }
//    protected override void Dispose(bool disposing)
//    {
//        base.Dispose(disposing);
//        lock (_lock)
//        {
//            while (_waiters.Count > 0)
//            {
//                var w = _waiters.Dequeue();
//                w.TrySetCanceled();
//            }
//            _pool.Clear();
//            _currentCount = 0;
//        }
//    }
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private Waiter RentWaiter()
//    {
//        if (_pool.Count > 0)
//        {
//            var w = _pool.Pop();
//            w.Reset(this);
//            return w;
//        }

//        return new Waiter(this);
//    }

//    private void Return(Waiter waiter)
//    {
//        if (_pool.Count < MaxPoolSize)
//            _pool.Push(waiter);
//    }

//    // -------------------------

//    private sealed class Waiter : IValueTaskSource
//    {
//        private ManualResetValueTaskSourceCore<bool> _core;
//        private WaitLock _owner;

//        public short Version => _core.Version;

//        public Waiter(WaitLock owner)
//        {
//            _owner = owner;
//            _core.RunContinuationsAsynchronously = false;
//        }

//        public void Reset(WaitLock owner)
//        {
//            _owner = owner;
//            _core.Reset();
//        }

//        public void SetResult()
//        {
//            _core.SetResult(true);
//        }

//        void IValueTaskSource.GetResult(short token)
//        {
//            try
//            {
//                _core.GetResult(token);
//            }
//            finally
//            {
//                // 归还对象池
//                lock (_owner._lock)
//                {
//                    if (!_owner.DisposedValue)
//                        _owner.Return(this);
//                }
//            }
//        }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public void TrySetCanceled(CancellationToken cancellationToken = default)
//          => _core.SetException(new OperationCanceledException(cancellationToken));
//        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
//            => _core.GetStatus(token);

//        void IValueTaskSource.OnCompleted(
//            Action<object?> continuation,
//            object? state,
//            short token,
//            ValueTaskSourceOnCompletedFlags flags)
//            => _core.OnCompleted(continuation, state, token, flags);
//    }
//}