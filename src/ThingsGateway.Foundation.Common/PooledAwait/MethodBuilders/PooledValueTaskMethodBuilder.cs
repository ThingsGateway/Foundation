using System.ComponentModel;
using System.Runtime.CompilerServices;

using ThingsGateway.Foundation.Common.PooledAwait.Internal;

#pragma warning disable CS1591

namespace ThingsGateway.Foundation.Common.PooledAwait.MethodBuilders
{
    /// <summary>
    /// This type is not intended for direct usage
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct PooledValueTaskMethodBuilder : IEquatable<PooledValueTaskMethodBuilder>
    {
        public override bool Equals(object? obj) => PooledAwaitThrowHelper.ThrowNotSupportedException<bool>();
        public bool Equals(PooledValueTaskMethodBuilder obj) => PooledAwaitThrowHelper.ThrowNotSupportedException<bool>();
        public override int GetHashCode() => PooledAwaitThrowHelper.ThrowNotSupportedException<int>();
        public override string ToString() => nameof(PooledValueTaskMethodBuilder);

        private PooledValueTaskSource _source;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledValueTaskMethodBuilder Create() => default;

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
            _source.TrySetResult();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            EnsureHasTask();
            _source.TrySetException(exception);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureHasTask()
        {
            if (!_source.HasTask) _source = PooledValueTaskSource.Create();
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PooledValueTask Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.PooledTask;
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

        public static bool operator ==(PooledValueTaskMethodBuilder left, PooledValueTaskMethodBuilder right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PooledValueTaskMethodBuilder left, PooledValueTaskMethodBuilder right)
        {
            return !(left == right);
        }
    }
}
