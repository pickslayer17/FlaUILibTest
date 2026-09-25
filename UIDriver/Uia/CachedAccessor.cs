using System.Runtime.InteropServices;
using Interop.UIAutomationClient;
using UIDriver.Exceptions;
using UIDriver.Uia.Constants;

namespace UIDriver.Uia;

public sealed class CachedAccessor
{
    private readonly IUIAutomationElement _native;
    private readonly UiaAutomation _automation;

    internal CachedAccessor(IUIAutomationElement native, UiaAutomation automation)
    {
        _native = native;
        _automation = automation;
    }

    public RunTimeId? RunTimeId => RunTimeId.FromArray(GetValue(UiaProperty.RuntimeId) as int[]);

    public UiaControlType ControlType => (UiaControlType)(int)GetValue(UiaProperty.ControlType);

    public string? Name => GetValue(UiaProperty.Name) as string;

    public IReadOnlyList<UiaElement> Children => ReadChildren();

    public object GetValue(UiaProperty property)
    {
        try
        {
            return _native.GetCachedPropertyValue((int)property);
        }
        catch (Exception exception) when (exception is COMException or ArgumentException)
        {
            throw new PropertyNotCachedException(property, exception);
        }
    }

    private UiaElement[] ReadChildren()
    {
        var children = _native.GetCachedChildren();
        if (children is null)
            return [];

        return Enumerable.Range(0, children.Length)
            .Select(index => _automation.Wrap(children.GetElement(index)))
            .ToArray();
    }
}
