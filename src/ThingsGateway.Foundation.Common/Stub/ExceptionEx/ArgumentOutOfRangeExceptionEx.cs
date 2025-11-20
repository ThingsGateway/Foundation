using System.Numerics;
using System.Runtime.CompilerServices;
namespace System;

#if NETFRAMEWORK || NETSTANDARD ||NET6_0
public static partial class ArgumentOutOfRangeExceptionEx
{
    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.</summary>
    /// <param name="value">The argument to validate as non-negative.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    public static void ThrowIfNegative(Int32 value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
            ThrowHelper.ThrowNegative(value, paramName);
    }

}

#else

public static partial class ArgumentOutOfRangeExceptionEx
{
    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.</summary>
    /// <param name="value">The argument to validate as non-negative.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative<T>(value, paramName);
    }
}

#endif
