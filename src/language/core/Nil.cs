namespace Vezel.Celerity.Language;

public sealed class Nil
{
    public static Nil Value { get; } = new();

    private Nil()
    {
    }
}
