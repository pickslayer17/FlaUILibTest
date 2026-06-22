using FlaUI.Core.AutomationElements;
using FlaUILibTest.UIDriver;

namespace FlaUILibTest.Interfaces;

// Рабочая лошадка поиска: знает, КАК искать элемент в своём окне.
// Минимальный контракт на данном этапе — расширится при выносе listener/watch-координации.
public interface IFinder
{
    Task<AutomationElement> RegisterAndGetElementAsync(BY condition);
}
