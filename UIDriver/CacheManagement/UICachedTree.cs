using Interop.UIAutomationClient;

public class UICachedTree
{
    public IUIAutomationElement CachedWindow { get; }

    public UICachedTree(IUIAutomationElement cachedWindow)
    {
        CachedWindow = cachedWindow;
    }
}