namespace Client.Models.Exceptions;

public class UserMemoryException : Exception
{
    public UserMemoryException()
    {
    }

    public UserMemoryException(string parameter)
        : base(parameter)
    {
    }

    public UserMemoryException(string parameter, Exception inner)
        : base(parameter, inner)
    {
    }
}