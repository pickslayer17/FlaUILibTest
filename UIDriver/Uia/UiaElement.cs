using Interop.UIAutomationClient;

namespace UIDriver.Uia;

public sealed class UiaElement
{
    internal IUIAutomationElement Native { get; }

    public CachedAccessor Cached { get; }
    public LiveAccessor Live { get; }

    internal UiaElement(IUIAutomationElement native, UiaAutomation automation)
    {
        Native = native;
        Cached = new CachedAccessor(native, automation);
        Live = new LiveAccessor(native, automation);
    }
}
