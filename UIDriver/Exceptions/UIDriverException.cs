namespace UIDriver.Exceptions;

public class UIDriverException : Exception
{
    public UIDriverException(string message) : base(message)
    {
    }

    public UIDriverException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
