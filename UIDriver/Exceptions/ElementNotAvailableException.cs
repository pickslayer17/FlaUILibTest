namespace UIDriver.Exceptions;

public sealed class ElementNotAvailableException : UIDriverException
{
    public ElementNotAvailableException(Exception innerException)
        : base("Element is no longer available in the live UI tree.", innerException)
    {
    }
}
