using System.Runtime.InteropServices;
using Interop.UIAutomationClient;
using UIDriver.Exceptions;
using UIDriver.Uia.Constants;

namespace UIDriver.Uia;

public sealed class LiveAccessor
{
    private const int ElementNotAvailableHResult = unchecked((int)0x80040201);

    private readonly IUIAutomationElement _native;
    private readonly UiaAutomation _automation;

    internal LiveAccessor(IUIAutomationElement native, UiaAutomation automation)
    {
        _native = native;
        _automation = automation;
    }

    public RunTimeId? RunTimeId => Read(() => RunTimeId.FromArray(_native.GetRuntimeId()));

    public UiaControlType ControlType => (UiaControlType)(int)GetValue(UiaProperty.ControlType);

    public string? Name => GetValue(UiaProperty.Name) as string;

    public int ProcessId => (int)GetValue(UiaProperty.ProcessId);

    public UiaElement? Parent => Read(() => _automation.WrapOrNull(_automation.Native.RawViewWalker.GetParentElement(_native)));

    public object GetValue(UiaProperty property) => Read(() => _native.GetCurrentPropertyValue((int)property));

    public UiaElement? FindFirstChild(UiaProperty property, object value)
    {
        var condition = _automation.Native.CreatePropertyCondition((int)property, value);
        return Read(() => _automation.WrapOrNull(_native.FindFirst(TreeScope.TreeScope_Children, condition)));
    }

    public UiaElement BuildUpdatedCache(CacheProfile profile)
        => Read(() => _automation.Wrap(_native.BuildUpdatedCache(profile.CreateRequest(_automation))));

    private static T Read<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch (COMException exception) when (exception.HResult == ElementNotAvailableHResult)
        {
            throw new ElementNotAvailableException(exception);
        }
    }
}
