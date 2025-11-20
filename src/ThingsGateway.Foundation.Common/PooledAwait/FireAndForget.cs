using System.Runtime.CompilerServices;

using ThingsGateway.Foundation.Common.PooledAwait.Internal;

namespace ThingsGateway.Foundation.Common.PooledAwait
{
    /// <summary>
    /// Represents an operation that completes at the first incomplete await,
    /// with the remainder continuing in the background
    /// </summary>
    [AsyncMethodBuilder(typeof(MethodBuilders.FireAndForgetMethodBuilder))]
    public readonly struct FireAndForget : IEquatable<FireAndForget>
    {
        /// <summary><see cref="Object.Equals(Object)"/></summary>
        public override bool Equals(object? obj) => true;
        public bool Equals(FireAndForget other) => true;
        /// <summary><see cref="Object.GetHashCode"/></summary>
        public override int GetHashCode() => 0;
        /// <summary><see cref="Object.ToString"/></summary>
        public override string ToString() => nameof(FireAndForget);

        /// <summary>
        /// Raised when exceptions occur on fire-and-forget methods
        /// </summary>
        public static event Action<Exception>? Exception;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void OnException(Exception exception)
        {
            if (exception != null) Exception?.Invoke(exception);
        }

        /// <summary>
        /// Gets the instance as a value-task
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask AsValueTask() => default;

        /// <summary>
        /// Gets the instance as a task
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task AsTask() => TaskUtils.CompletedTask;

        /// <summary>
        /// Gets the instance as a task
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Task(FireAndForget _) => FireAndForget.AsTask();

        /// <summary>
        /// Gets the instance as a value-task
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueTask(FireAndForget _) => FireAndForget.AsValueTask();

        /// <summary>
        /// Gets the awaiter for the instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTaskAwaiter GetAwaiter() => default(ValueTask).GetAwaiter();

        public static bool operator ==(FireAndForget left, FireAndForget right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FireAndForget left, FireAndForget right)
        {
            return !(left == right);
        }
    }
}
