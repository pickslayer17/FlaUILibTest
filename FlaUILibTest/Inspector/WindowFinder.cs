using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace FlaUILibTest.Inspector;

public class WindowFinder : FinderBase
{
    public WindowFinder(AutomationElement window) : base(window)
    {
    }
}