using Interop.UIAutomationClient;

namespace UIDriver.Uia;

public sealed class UiaAutomation
{
    internal IUIAutomation Native { get; } = new CUIAutomation8();

    public UiaElement Desktop => Wrap(Native.GetRootElement());

    public void RemoveAllEventHandlers() => Native.RemoveAllEventHandlers();

    internal UiaElement Wrap(IUIAutomationElement native) => new(native, this);

    internal UiaElement? WrapOrNull(IUIAutomationElement? native) => native is null ? null : Wrap(native);
}
