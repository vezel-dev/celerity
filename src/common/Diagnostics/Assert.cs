namespace Vezel.Celerity.Diagnostics;

[StackTraceHidden]
internal static class Assert
{
    public static void Fast(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string expression = "")
    {
        if (!condition)
            throw new AssertionException(expression);
    }

    [Conditional("DEBUG")]
    public static void Slow(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string expression = "")
    {
        if (!condition)
            throw new AssertionException(expression);
    }
}
