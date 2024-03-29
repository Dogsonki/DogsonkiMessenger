namespace Client.Pages.Exceptions;

public class WrongParameterException : Exception
{
    public WrongParameterException()
    {
    }

    public WrongParameterException(string message)
        : base(message)
    {
    }

    public WrongParameterException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
