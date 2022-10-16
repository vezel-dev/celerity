namespace Vezel.Celerity.Diagnostics;

internal static class Check
{
    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void Assert(
            [DoesNotReturnIf(false)] bool condition, [CallerArgumentExpression("condition")] string? expression = null)
        {
            if (!condition)
                throw new UnreachableException($"Assertion '{expression}' failed.");
        }
    }

    public static void Argument([DoesNotReturnIf(false)] bool condition)
    {
        if (!condition)
            throw new ArgumentException(null);
    }

    public static void Argument<T>(
        [DoesNotReturnIf(false)] bool condition,
        scoped in T value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        _ = value;

        if (!condition)
            throw new ArgumentException(null, name);
    }

    public static void Null([NotNull] object? value, [CallerArgumentExpression("value")] string? name = null)
    {
        ArgumentNullException.ThrowIfNull(value, name);
    }

    public static unsafe void Null(void* value, [CallerArgumentExpression("value")] string? name = null)
    {
        ArgumentNullException.ThrowIfNull(value, name);
    }

    public static void NullOrEmpty([NotNull] string? value, [CallerArgumentExpression("value")] string? name = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, name);
    }

    public static void Range<T>(
        [DoesNotReturnIf(false)] bool condition,
        scoped in T value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        _ = value;

        if (!condition)
            throw new ArgumentOutOfRangeException(name);
    }

    public static void Range(Range value, int length)
    {
        _ = value.GetOffsetAndLength(length);
    }

    public static void Enum<T>(T value, [CallerArgumentExpression("value")] string? name = null)
        where T : struct, Enum
    {
        if (!System.Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(name);
    }

    public static void Enum<T>(T? value, [CallerArgumentExpression("value")] string? name = null)
        where T : struct, Enum
    {
        if (value is T v && !System.Enum.IsDefined(v))
            throw new ArgumentOutOfRangeException(name);
    }

    public static void Operation([DoesNotReturnIf(false)] bool condition)
    {
        if (!condition)
            throw new InvalidOperationException();
    }

    public static void ForEach<T>(IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
            action(item);
    }
}
