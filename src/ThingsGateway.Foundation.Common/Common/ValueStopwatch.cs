namespace ThingsGateway.Foundation.Common;


using System;
using System.Diagnostics;

public struct ValueStopwatch : IEquatable<ValueStopwatch>
{
#if !NET7_0_OR_GREATER
    private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
#endif

    private readonly long _startTimestamp;

    public bool IsActive => _startTimestamp != 0;

    private ValueStopwatch(long startTimestamp)
    {
        _startTimestamp = startTimestamp;
    }

    public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

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

    public override bool Equals(object? obj)
    {
        if (obj is ValueStopwatch stopwatch)
        {
            return _startTimestamp == stopwatch._startTimestamp;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return _startTimestamp.GetHashCode();
    }

    public bool Equals(ValueStopwatch other)
    {
        return _startTimestamp == other._startTimestamp;
    }

    public static bool operator ==(ValueStopwatch left, ValueStopwatch right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ValueStopwatch left, ValueStopwatch right)
    {
        return !(left == right);
    }
}