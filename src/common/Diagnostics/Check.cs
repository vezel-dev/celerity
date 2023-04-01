namespace Vezel.Celerity.Diagnostics;

[StackTraceHidden]
internal static class Check
{
    public static void Argument<T>(
        [DoesNotReturnIf(false)] bool condition,
        in T value,
        [CallerArgumentExpression(nameof(value))] string name = "")
    {
        _ = value;

        if (!condition)
            throw new ArgumentException(null, name);
    }

    public static void Null([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string name = "")
    {
        if (value == null)
            throw new ArgumentNullException(name);
    }

    public static void NullOrEmpty(
        [NotNull] string? value, [CallerArgumentExpression(nameof(value))] string name = "")
    {
        if (string.IsNullOrEmpty(value))
            throw value == null ? new ArgumentNullException(name) : new ArgumentException(null, name);
    }

    public static void NullOrWhiteSpace(
        [NotNull] string? value, [CallerArgumentExpression(nameof(value))] string name = "")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw value == null ? new ArgumentNullException(name) : new ArgumentException(null, name);
    }

    public static void Range<T>(
        [DoesNotReturnIf(false)] bool condition,
        in T value,
        [CallerArgumentExpression(nameof(value))] string name = "")
    {
        _ = value;

        if (!condition)
            throw new ArgumentOutOfRangeException(name);
    }

    public static void Enum<T>(T value, [CallerArgumentExpression(nameof(value))] string name = "")
        where T : struct, Enum
    {
        if (!System.Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(name);
    }

    public static void Operation([DoesNotReturnIf(false)] bool condition)
    {
        if (!condition)
            throw new InvalidOperationException();
    }

    public static void All<T>(
        IEnumerable<T> value,
        Func<T, bool> predicate,
        [CallerArgumentExpression(nameof(value))] string name = "")
    {
        foreach (var item in value)
            if (!predicate(item))
                throw new ArgumentException(null, name);
    }

    public static void All<T, TState>(
        IEnumerable<T> value,
        TState state,
        Func<T, TState, bool> predicate,
        [CallerArgumentExpression(nameof(value))] string name = "")
    {
        foreach (var item in value)
            if (!predicate(item, state))
                throw new ArgumentException(null, name);
    }
}
