using FlaUI.Core.AutomationElements;

namespace UIDriver;

// База для декораторов: навешивает «нюансы» поверх основной стратегии, не меняя саму стратегию.
public abstract class FinderDecorator : IFinder
{
    protected readonly IFinder Inner;

    protected FinderDecorator(IFinder inner) => Inner = inner;

    public abstract AutomationElement? Find(AutomationElement source);
    public abstract AutomationElement[] FindAll(AutomationElement source);
}
