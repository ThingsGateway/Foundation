using System.ComponentModel;
using System.Runtime.CompilerServices;

using ThingsGateway.Foundation.Common.PooledAwait.Internal;

using SystemTask = System.Threading.Tasks.Task;

#pragma warning disable CS1591

namespace ThingsGateway.Foundation.Common.PooledAwait.MethodBuilders
{
    /// <summary>
    /// This type is not intended for direct usage
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct PooledTaskMethodBuilder : IEquatable<PooledTaskMethodBuilder>
    {
        public override bool Equals(object? obj) => PooledAwaitThrowHelper.ThrowNotSupportedException<bool>();
        public bool Equals(PooledTaskMethodBuilder obj) => PooledAwaitThrowHelper.ThrowNotSupportedException<bool>();
        public override int GetHashCode() => PooledAwaitThrowHelper.ThrowNotSupportedException<int>();
        public override string ToString() => nameof(PooledTaskMethodBuilder);

        private ValueTaskCompletionSource<Nothing> _source;
        private Exception _exception;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledTaskMethodBuilder Create() => default;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // 将成员标记为 static
        public void SetStateMachine(IAsyncStateMachine _) => Counters.SetStateMachine.Increment();
#pragma warning restore CA1822 // 将成员标记为 static

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult()
        {
            _source.TrySetResult(default);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            _source.TrySetException(exception);
            _exception = exception;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureHasTask()
        {
            if (_source.IsNull) _source = ValueTaskCompletionSource<Nothing>.Create();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PooledTask Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                SystemTask task;
                if (!_source.IsNull) task = _source.Task;
                else if (_exception is OperationCanceledException) task = TaskUtils.TaskFactory<Nothing>.Canceled;
                else if (_exception != null) task = TaskUtils.FromException(_exception);
                else task = TaskUtils.CompletedTask;
                return new PooledTask(task);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsureHasTask();
            StateMachineBox<TStateMachine>.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsureHasTask();
            StateMachineBox<TStateMachine>.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // 将成员标记为 static
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
#pragma warning restore CA1822 // 将成员标记为 static
            where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();

        public static bool operator ==(PooledTaskMethodBuilder left, PooledTaskMethodBuilder right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PooledTaskMethodBuilder left, PooledTaskMethodBuilder right)
        {
            return !(left == right);
        }
    }
}
