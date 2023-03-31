namespace Vezel.Celerity.Driver;

[SuppressMessage("", "CA1032")]
[SuppressMessage("", "CA1064")]
internal sealed class DriverException : Exception
{
    public DriverException(string message)
        : base(message)
    {
    }
}
