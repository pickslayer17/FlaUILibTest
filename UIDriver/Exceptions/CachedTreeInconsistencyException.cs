namespace UIDriver.Exceptions;

public sealed class CachedTreeInconsistencyException : UIDriverException
{
    public CachedTreeInconsistencyException(string message) : base(message)
    {
    }
}
