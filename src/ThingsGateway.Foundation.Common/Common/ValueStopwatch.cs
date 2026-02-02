namespace ThingsGateway.Foundation.Common;


using System;
using System.Diagnostics;

/// <summary>
/// 轻量级的值类型计时器，用于高性能场景下的耗时测量。
///
/// 与 <see cref="Stopwatch"/> 相比：
/// - 为 <c>struct</c>，可以避免在热点路径上产生堆分配；
/// - 通常配合 <see cref="StartNew"/> 静态方法使用。
/// </summary>
public struct ValueStopwatch : IEquatable<ValueStopwatch>
{
#if !NET7_0_OR_GREATER
    /// <summary>
    /// 从 <see cref="Stopwatch"/> 时间戳转换为 <see cref="TimeSpan.Ticks"/> 的比例系数。
    /// 在旧框架上缺少 <see cref="Stopwatch.GetTimestamp"/> API，
    /// 因此需要手动做频率换算。
    /// </summary>
    private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
#endif

    /// <summary>
    /// 启动时间的原始时间戳
    /// 值为 0 通常表示默认值 <c>default(ValueStopwatch)</c>，未被正确初始化。
    /// </summary>
    private readonly long _startTimestamp;

    /// <summary>
	/// 获取当前实例是否处于“活动”状态。
	/// 当 <c>_startTimestamp != 0</c> 时视为已启动，可用于计算耗时。
    /// </summary>
    public bool IsActive => _startTimestamp != 0;

    private ValueStopwatch(long startTimestamp)
    {
        _startTimestamp = startTimestamp;
    }

    /// <summary>
    /// 创建并启动一个新的 <see cref="ValueStopwatch"/> 实例。
    /// 等价于调用 <see cref="Stopwatch.GetTimestamp"/> 记录当前时间戳。
    /// </summary>
    public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

    /// <summary>
    /// 根据起始与结束时间戳计算耗时。
    /// </summary>
    public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
    {
#if !NET7_0_OR_GREATER
        var timestampDelta = endingTimestamp - startingTimestamp;
        var ticks = (long)(TimestampToTicks * timestampDelta);
        return new TimeSpan(ticks);
#else
        return Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);
#endif
    }

    /// <summary>
    /// 获取从构造（或 <see cref="StartNew"/>）到当前时刻的耗时。
    /// 若实例未初始化（为 <c>default</c>），则会抛出 <see cref="InvalidOperationException"/>。
    /// </summary>
    public TimeSpan GetElapsedTime()
    {
        // Start timestamp can't be zero in an initialized ValueStopwatch. It would have to be literally the first thing executed when the machine boots to be 0.
        // So it being 0 is a clear indication of default(ValueStopwatch)
        if (!IsActive)
        {
            throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
        }

        var end = Stopwatch.GetTimestamp();

        return GetElapsedTime(_startTimestamp, end);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is ValueStopwatch stopwatch)
        {
            return _startTimestamp == stopwatch._startTimestamp;
        }
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _startTimestamp.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(ValueStopwatch other)
    {
        return _startTimestamp == other._startTimestamp;
    }

    /// <inheritdoc/>
    public static bool operator ==(ValueStopwatch left, ValueStopwatch right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(ValueStopwatch left, ValueStopwatch right)
    {
        return !(left == right);
    }
}