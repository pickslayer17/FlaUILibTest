using FlaUI.Core.AutomationElements;
using FlaUILibTest.UIDriver;

namespace FlaUILibTest.Interfaces;

public interface IElementSource
{
    Task<AutomationElement> FindFirstAsync(BY by);
    Task<AutomationElement[]> FindAllAsync(BY by);
}

