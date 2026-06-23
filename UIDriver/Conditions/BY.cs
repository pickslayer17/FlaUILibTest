using FlaUI.Core.Conditions;

namespace UIDriver;

// Откуда искать на уровне окна: рабочий стол / окно по умолчанию (назначит AppManager) / кастомное окно.
public enum WindowScope
{
    Desktop,
    Default,
    Custom
}

// Декларативное описание ИСКОМОГО: собственное условие элемента + реляционные связи + scope окна + таймаут.
// BY НЕ держит ссылку на живое окно — только scope-намёк для роутинга; живой source даёт WindowManager.
public sealed class BY
{
    public WindowScope Scope { get; init; } = WindowScope.Default;

    // Условие самого элемента.
    public ConditionBase? Element { get; init; }

    // Реляционная часть (пример: элемент, лежащий внутри родителя). Сюда же позже Ancestors/Siblings/etc.
    public ConditionBase? Parent { get; init; }

    // Условие окна — нужно только для Scope.Custom (окно может всплыть где угодно → ищем от desktop).
    public ConditionBase? Window { get; init; }

    // Таймаут контракта задаётся здесь, чтобы Watcher знал, сколько ждать.
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);
}
