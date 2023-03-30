namespace Vezel.Celerity.Language.Tooling.Projects;

public class ProjectException : Exception
{
    public ProjectException()
        : base("An unknown project error occurred.")
    {
    }

    public ProjectException(string message)
        : base(message)
    {
    }

    public ProjectException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
