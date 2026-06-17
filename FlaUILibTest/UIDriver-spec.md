# UIDriver — техническое задание для рефакторинга

## Архитектура

```
UIDriver → ModuleFinder[] → EventManager → ModuleManager → Module[] → Element
```

UIDriver — точка входа, держит список ModuleFinder (по одному на окно).
ModuleFinder — событийный поиск элементов в конкретном окне.
EventManager — централизованная обработка UIA событий, распределяет по ModuleManager и pending watches.
ModuleManager — контейнер для Module[], управляет парами Module→Element, фоновый поиск якорей.
Module — якорь элемента, следит за жизненным циклом через события от ModuleManager.
Element — обёртка над AutomationElement с кэшем и валидацией.

## Что удалить

- `UiTree.cs` — не нужен, замещён ModuleFinder
- `TreeNode.cs` — не нужен без UiTree
- `EventManagerExtended.cs` — логика переедет в EventManager внутри ModuleFinder
- Старый `Subscribe()` из публичного API ModuleFinder — root теперь в конструкторе

## Что оставить как есть

- `ConditionMatcher.cs` — работает, не трогать
- `UiaProperty.cs` — enum с правильными ID, не трогать
- `ISubscriber.cs` — интерфейс подписчика, может пригодиться для Module

## Что переделать

### ModuleFinder

Было: один экземпляр, переподписывается через Subscribe(), один _root.
Стало: создаётся через конструктор с root, живёт с окном от создания до закрытия.

```csharp
public class ModuleFinder : IDisposable
{
    // Конструктор принимает root, сразу подписывается на события
    public ModuleFinder(AutomationElement root, string name = "default")

    // Регистрация ожидания элемента
    // ВАЖНО: race fix — сначала watch, потом FindAllDescendants, результат в watch
    public async Task<AutomationElement> RegisterAndGetElementAsync(ConditionBase condition, int timeoutMs = 7000)

    // EventManager внутри — единственный владелец подписок на UIA события
    // Module[] — подписчики EventManager'а

    // При поиске через FindAllDescendants — исключать не-root Window и их потомков
    // Использовать только FindAllDescendants (не FindFirstDescendant)

    // Обработка событий:
    // StructureChanged ChildAdded → TryResolveByDescendant (FindAllDescendants от element)
    // StructureChanged ChildrenInvalidated → re-scan pending watches от root
    // PropertyChanged → TryResolveByMatch (ConditionMatcher.Matches на сам element)
    // WindowOpened → TryResolveByMatch + TryResolveByDescendant

    public void Dispose() // отписка от всех UIA событий
}
```

Race condition fix в RegisterAndGetElementAsync:
```
1. Создать TCS, добавить в watches
2. FindAllDescendants по condition
3. Если нашли → TrySetResult в TCS, вернуть элемент
4. Если не нашли → await TCS.Task с таймаутом
```
Порядок критичен: watch ДО поиска. Если элемент появляется между шагами — событие попадёт в уже зарегистрированный watch.

### EventManager (НОВЫЙ, внутри ModuleFinder)

НЕ путать со старым EventManager/EventManagerExtended. Это новый класс внутри ModuleFinder.
По сути — контейнер для модулей, чтобы ModuleFinder не занимался управлением Module[] напрямую.

Обязанности:
- Единственный владелец подписок на UIA события (StructureChanged, PropertyChanged, WindowOpened, WindowClosed)
- Подписывается на ВСЕ свойства из UiaPropertyHelper.AllProperties
- Распределяет события по зарегистрированным Module[]
- ModuleFinder тоже получает события для pending watches

```csharp
internal class EventManager : IDisposable
{
    // Подписка на все UIA события от root элемента
    public EventManager(AutomationElement root)

    // Регистрация подписчика (Module)
    public void Register(Module module)
    public void Unregister(Module module)

    // Обработчики — вызывают все зарегистрированные Module + callback в ModuleFinder
    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    private void OnWindowOpened(AutomationElement element, EventId eventId)
    private void OnWindowClosed(AutomationElement element, EventId eventId)

    public void Dispose() // отписка от UIA
}
```

### Module (ПЕРЕДЕЛАТЬ)

Module — якорь элемента. Механизм поменялся: раньше якорь объявлялся явно, теперь элемент и якорь существуют как пара внутри ModuleManager.

**Ключевая механика — watch со статусами и параллельный поиск модуля:**

Watch имеет статусы:
- `Pending` — элемент ещё не найден, ждём событие или нативный поиск
- `ElementFound` — элемент найден и отдан пользователю, ищем модуль (якорь)
- `ModuleExists` — и элемент и модуль найдены, пара передана в ModuleManager

Алгоритм RegisterAndGetElementAsync:
1. Создать watch (статус = Pending), добавить в watches
2. Если watch не completed → запустить нативный FindAllDescendants
3. Если count = 0 → ждём результат от watch (событие найдёт элемент)
4. Если count > 0 → элемент отдаётся пользователю, watch переходит в `ElementFound`

После перехода в `ElementFound`:
- Watch НЕ удаляется из watches
- Асинхронный поток продолжает искать модуль (предка-якоря, который тригернул событие)
- Поиск модуля идёт параллельно с тем как пользователь уже работает с элементом
- Часто элемент находится через нативный поиск ДО события — в этом случае событие придёт позже и даст нам якорь

Когда модуль (якорь) найден:
- Watch переходит в `ModuleExists`
- Пара (Module → Element) передаётся в ModuleManager
- Module начинает следить за жизненным циклом элемента через EventManager

Если модуль НЕ найден за отведённое время:
- Элемент помечается как `orphaned`
- Orphaned элемент работает, но при пере-резолве использует polling fallback

Это сложная часть системы: в одном месте делаем сразу два дела — отдаём элемент и ищем якорь. Но иначе нельзя — разделение этих процессов потребовало бы двух отдельных поисков по дереву.

```csharp
public class Module
{
    public ConditionBase Condition { get; }
    public AutomationElement Element { get; private set; }
    public bool IsValid { get; private set; }
    public bool IsOrphaned { get; private set; } // модуль не найден — fallback на polling

    public Module(ConditionBase condition, AutomationElement element)

    // Привязка якоря после фонового поиска
    public void AttachAnchor(AutomationElement anchor)
    public void MarkOrphaned() // модуль не найден в срок

    // Вызывается ModuleManager'ом (через EventManager):
    // ChildRemoved / ChildrenInvalidated → IsValid = false, Element locked
    // ChildAdded → если ConditionMatcher.Matches → пере-резолв, IsValid = true
    // PropertyChanged → если это наш элемент → обновить кэш свойств
    public void OnStructureChanged(AutomationElement element, StructureChangeType changeType)
    public void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
}
```

### ModuleManager (НОВЫЙ, внутри ModuleFinder, бывший EventManager)

Контейнер для Module[]. Получает события от EventManager, распределяет по модулям. Управляет жизненным циклом пар Module→Element.

```csharp
internal class ModuleManager
{
    public void Register(Module module)
    public void Unregister(Module module)

    // Фоновый поиск якоря для элемента
    // Запускается после того как элемент найден и отдан пользователю
    public Task FindAnchorAsync(Module module)

    // Вызывается EventManager'ом — распределяет по всем Module[]
    public void OnStructureChanged(AutomationElement element, StructureChangeType changeType)
    public void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
}
```

### Element (ПЕРЕДЕЛАТЬ)

Было: обёртка с автоматической подпиской.
Стало: обёртка над AutomationElement + Module. Все действия async.

```csharp
public class Element
{
    private Module _module;

    // При каждом действии: проверить _module.IsValid
    // Если невалидный — ждать пере-резолв через Module (не polling!)
    // Если валидный — выполнить действие

    public async Task ClickAsync()
    public async Task FillAsync(string text)
    public async Task InvokeAsync()
    public async Task<string> GetTextAsync()
    public async Task HoverOverAsync()
}
```

### UIDriver

```csharp
public class UIDriver : IDisposable
{
    private UIA3Automation _automation;
    private ConditionFactory _cf;
    private List<(ModuleFinder finder, Window window)> _contexts;
    private int _activeContextIndex;

    public UIDriver()
    // Внутри: new UIA3Automation(), _cf = automation.ConditionFactory

    public Window LaunchApplication(ProcessStartInfo psi)
    // Application.Launch → GetMainWindow → new ModuleFinder(window) → добавить в _contexts
    // Подписаться на WindowOpened/WindowClosed на main window:
    //   WindowOpened → auto-create ModuleFinder для нового окна, добавить в _contexts
    //   WindowClosed → найти контекст, cancel pending watches, удалить из _contexts

    public UILocator Locator(ConditionBase condition)
    // Создаёт UILocator с condition + ссылкой на активный контекст

    public void SwitchTo(ConditionBase windowCondition)
    // Найти контекст в _contexts по condition, сделать активным

    public void SwitchToMainContent()
    // _activeContextIndex = 0

    public IReadOnlyList<Window> ApplicationWindows { get; }
    // Окна из всех контекстов

    public void Dispose()
}
```

### By (ЗАГЛУШКА на первом этапе)

Минимальная реализация — несколько хелперов чтобы тест компилировался. Полноценный DSL, чейны и XPath парсер — потом.

```csharp
public static class By
{
    internal static ConditionFactory CF { get; set; }

    public static ConditionBase Button(string name) =>
        CF.ByControlType(ControlType.Button).And(CF.ByName(name));

    public static ConditionBase Tab(string name) =>
        CF.ByControlType(ControlType.TabItem).And(CF.ByName(name));

    public static ConditionBase MenuItem(string name) =>
        CF.ByControlType(ControlType.MenuItem).And(CF.ByName(name));

    public static ConditionBase Window(string name) =>
        CF.ByControlType(ControlType.Window).And(CF.ByName(name));

    public static ConditionBase Id(string automationId) =>
        CF.ByAutomationId(automationId);
}
```

Расширение (НЕ на первом этапе): Edit, ComboBox, XPath парсер, чейн Name().AndType(), ConditionExtensions.

### UILocator (НОВЫЙ)

```csharp
public class UILocator
{
    private readonly ConditionBase _condition;
    private readonly UIDriver _driver; // для доступа к активному контексту

    internal UILocator(ConditionBase condition, UIDriver driver)

    // Каждый метод:
    // 1. Получить активный контекст из driver
    // 2. context.finder.RegisterAndGetElementAsync(condition)
    // 3. Выполнить действие
    // Кэширование через Module: если Module для этого condition уже есть и IsValid — использовать кэш

    public async Task ClickAsync()
    public async Task FillAsync(string text)
    public async Task InvokeAsync()
    public async Task<string> GetTextAsync()
    public async Task HoverOverAsync()

    // SwitchTo — для By.Window(...)
    public async Task SwitchToAsync()
    // Внутри: driver.SwitchTo(_condition)
}
```

## Исключения

```csharp
public class UIDriverException : Exception { }

public class ElementNotFoundException : UIDriverException
{
    public ConditionBase Condition { get; }
    public string ContextName { get; }
    public TimeSpan Timeout { get; }
}

public class ElementStaleException : UIDriverException
{
    public ConditionBase Condition { get; }
}

public class WindowClosedException : UIDriverException
{
    public string WindowName { get; }
}
```

## Структура файлов после рефакторинга

```
FlaUILibTest/
├── UIDriver/
│   ├── UIDriver.cs
│   ├── UILocator.cs
│   ├── By.cs
│   ├── Exceptions.cs
│   └── ModuleFinder/
│       ├── ModuleFinder.cs
│       ├── EventManager.cs
│       ├── ModuleManager.cs
│       ├── Module.cs
│       └── Element.cs
├── ConditionMatcher.cs (без изменений)
├── UiaProperty.cs (без изменений)
├── DcPushBenchMark/ (без изменений, для сравнения)
└── Program.cs (тест с UIDriver)
```

## Удалить после рефакторинга

- Inspector/UiTree.cs
- Inspector/TreeNode.cs
- EventManagerExtended.cs
- Старый EventManager.cs (заменён новым внутри ModuleFinder)
- Старый Module.cs (заменён новым)
- Старый Element.cs (заменён новым)
- AutomationSubscriberBase.cs (логика в Module)

## Тест-критерий готовности

Program.cs выполняет без ошибок:
```csharp
var driver = new UIDriver();
var window = driver.LaunchApplication(processStartInfo);

await driver.Locator(By.Tab("Insert")).ClickAsync();
await driver.Locator(By.Button("Table")).ClickAsync();

driver.SwitchTo(By.Window("Create Table"));
await driver.Locator(By.Button("OK")).ClickAsync();

driver.SwitchToMainContent();
```

## Что НЕ делать

- Не трогать DcPushBenchMark — эталон для сравнения
- Не создавать синхронные методы — только async
- Не подписываться на UIA события из Module напрямую — только через EventManager
- Не использовать FindFirstDescendant — только FindAllDescendants
- Не делать XPath парсер на первом этапе — сначала By хелперы
