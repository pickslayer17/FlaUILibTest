using UIDriver.Uia.Constants;

namespace UIDriver.Exceptions;

public sealed class PropertyNotCachedException : UIDriverException
{
    public UiaProperty Property { get; }

    public PropertyNotCachedException(UiaProperty property, Exception innerException)
        : base($"Property {property} is not in the cache of this element. Add it to the CacheProfile or read it through Live.", innerException)
    {
        Property = property;
    }
}
