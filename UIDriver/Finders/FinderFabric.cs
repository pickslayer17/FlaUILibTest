namespace UIDriver;

// Единая на проект фабрика. Читает форму BY и выдаёт конкретный IFinder:
// Strategy — основной способ поиска (Descendant / Parent / …), Decorator — нюансы поверх.
public sealed class FinderFabric
{
    public IFinder GetFinder(BY by)
    {
        // Скелет: выбор стратегии по форме BY.
        IFinder strategy = by.Parent is not null
            ? new ParentFinder(by.Parent, by.Element!)
            : new DescendantFinder(by.Element!);

        // TODO: навесить декораторы по остальным нюансам BY (scope, siblings, ancestors, ...).
        return strategy;
    }
}
