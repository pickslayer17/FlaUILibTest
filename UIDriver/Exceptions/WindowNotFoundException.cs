namespace UIDriver.Exceptions;

public sealed class WindowNotFoundException : UIDriverException
{
    public WindowNotFoundException(string message) : base(message)
    {
    }
}
