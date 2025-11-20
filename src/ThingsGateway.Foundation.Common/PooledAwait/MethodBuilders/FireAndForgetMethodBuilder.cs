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
    public struct FireAndForgetMethodBuilder : IEquatable<FireAndForgetMethodBuilder>
    {
        public override bool Equals(object? obj) => PooledAwaitThrowHelper.ThrowNotSupportedException<bool>();
        public bool Equals(FireAndForgetMethodBuilder obj) => PooledAwaitThrowHelper.ThrowNotSupportedException<bool>();
        public override int GetHashCode() => PooledAwaitThrowHelper.ThrowNotSupportedException<int>();
        public override string ToString() => nameof(FireAndForgetMethodBuilder);

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FireAndForgetMethodBuilder Create() => default;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // 将成员标记为 static
        public void SetStateMachine(IAsyncStateMachine _) => Counters.SetStateMachine.Increment();
#pragma warning restore CA1822 // 将成员标记为 static

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // 将成员标记为 static
        public void SetResult() { }
#pragma warning restore CA1822 // 将成员标记为 static

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // 将成员标记为 static
        public void SetException(Exception exception) => FireAndForget.OnException(exception);
#pragma warning restore CA1822 // 将成员标记为 static

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1822 // 将成员标记为 static
        public FireAndForget Task
#pragma warning restore CA1822 // 将成员标记为 static
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // 将成员标记为 static
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
#pragma warning restore CA1822 // 将成员标记为 static
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => StateMachineBox<TStateMachine>.AwaitOnCompleted(ref awaiter, ref stateMachine);

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // 将成员标记为 static
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
#pragma warning restore CA1822 // 将成员标记为 static
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => StateMachineBox<TStateMachine>.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822 // 将成员标记为 static
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
#pragma warning restore CA1822 // 将成员标记为 static
            where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();

        public static bool operator ==(FireAndForgetMethodBuilder left, FireAndForgetMethodBuilder right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FireAndForgetMethodBuilder left, FireAndForgetMethodBuilder right)
        {
            return !(left == right);
        }
    }
}
