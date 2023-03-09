namespace Client.Pages.Exceptions;

public class NullParameterException : Exception
{
    public NullParameterException()
    {
    }

    public NullParameterException(string parameter)
        : base(parameter)
    {
    }

    public NullParameterException(string parameter, Exception inner)
        : base(parameter, inner)
    {
    }
}