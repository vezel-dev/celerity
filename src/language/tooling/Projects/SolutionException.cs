namespace Vezel.Celerity.Language.Tooling.Projects;

public class SolutionException : Exception
{
    public SolutionException()
        : base("An unknown solution error occurred.")
    {
    }

    public SolutionException(string message)
        : base(message)
    {
    }

    public SolutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
