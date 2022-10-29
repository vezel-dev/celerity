namespace Vezel.Celerity.Diagnostics;

[StackTraceHidden]
internal static class Check
{
    public static class Always
    {
        public static void Assert(
            [DoesNotReturnIf(false)] bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null)
        {
            if (!condition)
                throw new UnreachableException($"Hard assertion '{expression}' failed.");
        }
    }

    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void Assert(
            [DoesNotReturnIf(false)] bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null)
        {
            if (!condition)
                throw new UnreachableException($"Debug assertion '{expression}' failed.");
        }
    }

    public static class Release
    {
        [Conditional("RELEASE")]
        public static void Assert(
            [DoesNotReturnIf(false)] bool condition,
            [CallerArgumentExpression(nameof(condition))] string? expression = null)
        {
            if (!condition)
                throw new UnreachableException($"Release assertion '{expression}' failed.");
        }
    }

    public static void Argument<T>(
        [DoesNotReturnIf(false)] bool condition,
        in T value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        _ = value;

        if (!condition)
            throw new ArgumentException(null, name);
    }

    public static void Null([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        ArgumentNullException.ThrowIfNull(value, name);
    }

    public static void NullOrEmpty(
        [NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, name);
    }

    public static void Enum<T>(T value, [CallerArgumentExpression(nameof(value))] string? name = null)
        where T : struct, Enum
    {
        if (!System.Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(name);
    }

    public static void All<T>(
        IEnumerable<T> value,
        Func<T, bool> predicate,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        foreach (var item in value)
            if (!predicate(item))
                throw new ArgumentException(null, name);
    }
}
