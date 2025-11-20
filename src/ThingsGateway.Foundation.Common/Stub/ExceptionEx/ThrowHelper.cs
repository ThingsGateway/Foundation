namespace System;

public static class ThrowHelper
{
    [DoesNotReturn]
    internal static void ThrowObjectDisposedException(object? instance)
    {
        throw new ObjectDisposedException(instance?.GetType().FullName);
    }

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException<T>(T value, string? paramName) =>
    throw new ArgumentOutOfRangeException(paramName, value, null);


    [DoesNotReturn]
    internal static void ThrowNegative<T>(T value, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must > 0 , but it is {value}");

    [DoesNotReturn]
    public static void ThrowArgumentNullException(string? paramName) =>
    throw new System.ArgumentNullException(paramName);
}