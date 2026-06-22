using FlaUI.Core.AutomationElements;
using FlaUILibTest.Interfaces;
using FlaUILibTest.UIDriver;

namespace FlaUILibTest.Inspector;

public class WindowElementSource : IElementSource
{
    private readonly FinderManager _manager;
    private readonly BY _windowBy;
    private IFinder _finder;

    public WindowElementSource(FinderManager manager, BY windowBy)
    {
        _manager = manager;
        _windowBy = windowBy;
    }

    public async Task<AutomationElement> FindFirstAsync(BY elementBy)
    {
        var finder = await ResolveFinderAsync();
        return await finder.RegisterAndGetElementAsync(elementBy);
    }

    public Task<AutomationElement[]> FindAllAsync(BY elementBy)
        => throw new NotImplementedException("FindAll будет реализован позже");

    private async Task<IFinder> ResolveFinderAsync()
    {
        if (_finder != null)
            return _finder;

        _finder = _windowBy == null
            ? _manager.GetRootWindowFinder()
            : await _manager.FindInDesktop(_windowBy);

        return _finder;
    }
}
