# UIDriver

Событийный (реактивный) движок UI-автоматизации Windows-десктопа — аналог Playwright для нативного UI.
Чистый UIA COM (`Interop.UIAutomationClient`, `CUIAutomation8`), .NET 9, C# preview. Тестовое приложение — Excel.

## Проекты
- `UIDriver/` — библиотека, живой код, без WinForms.
- `UIDriver.Visualization/` — WinForms-визуализаторы: `TreeVisualizer` (дерево окна, изменения подсвечены цветом) и `BranchVisualizer` (прилетевшие ветки).
- `FlaUILibTest/` — консоль-песочница: `Program.cs` запускает Excel через `Driver` и подключает визуализаторы и логгер. `DcPushBenchMark/` — старый бенчмарк, без запроса не читать и не трогать.
- `docs/` — архитектурный docx.

## Слои UIDriver (namespace = папка)
- `Api/` — `Driver`, `DriverOptions`, `UILocator`, `UIBy`.
- `Uia/` — **единственное место работы с Interop**.
  - `UiaElement` — обёртка над нативным элементом. Чтение явное: `element.Cached.X` — только кэш, иначе `PropertyNotCachedException`; `element.Live.X` — живой запрос в процесс.
  - `RunTimeId` — неизменяемый; отсутствие RID = `null`.
  - `CacheProfile` — какие свойства кэшируем.
  - `Listening/` — подписки UIA.
- `Events/` — события UIA идут в одну очередь (`EventQueue`, один поток-потребитель), `EventDispatcher` раздаёт их обработчикам в `Handlers/`. Исключение в потребителе роняет процесс — это задумано.
- `Windows/` — `UIApplicationManager` (связывает всё вместе), `WindowRegistry`, `WindowContainer`.
- `Tree/` — `UICachedTree` (Add / Replace / MarkDirty + версия), `UiNode`, ветки, фабрики. `Snapshots/` — неизменяемые версионные снимки дерева и веток.
- `Diagnostics/` — `ITreeObserver`, `IBranchObserver`, `SnapshotPublisher`.
- `Search/` — заморожено: `UISuperFinder`, `UiNodeNavigator` и прочее.
- `Exceptions/`, `Processes/` (`ProcessKillJob`).

Сборка: `dotnet build FlaUILibTest.sln`. Билд и запуск — только по команде Дениса (запуск поднимает Excel).

## Идея
Окно → локальное кэш-дерево одним `BuildUpdatedCache` (TreeScope Subtree, ~30 мс на ~900 узлов) → держим его актуальным по UIA-событиям → ищем по локальному дереву, а живой UIA дёргаем только для действия и точечной проверки.
Целевая архитектура описана в `docs/ui_automation_cached_tree_architecture.docx`: materialized view + очередь мутаций + схлопывание + версионные снимки + query planner + locator как рецепт.

Цепочка событий: `WindowListener → WindowEventForwarder → EventQueue → EventDispatcher → Handler → UICachedTree → SnapshotPublisher → визуализаторы`.

Поиск пока не подключён и заморожен. Движок поиска будет только на XPath: `UiNodeNavigator : XPathNavigator`. `UIBy` — обёртка, которая будет нести XPath, а условия потом будет переводить в XPath отдельный транслятор. `UISuperFinder` — прежний прототип, не удалять.

Концепция на будущее:
- событийное кэш-дерево меняется только в одном обработчике;
- после каждого события дерево публикует неизменяемый версионный снимок;
- поиск всегда идёт по снимку, никогда по живому кэш-дереву.

## Обработка StructureChanged (схема Дениса)
| Событие | RuntimeId у source | Действие |
|---|---|---|
| ChildAdded | валиден | HeeledBranch → **Add**: прицепить к parent, двусторонне перелинковать parent и children |
| ChildAdded | null | невалидно, пропуск |
| ChildrenInvalidated | валиден у корня и у потомков | **Replace**: заменить всю ветку от parent |
| ChildrenInvalidated | валиден только у корня, у потомков пустой | **Mark Dirty**: пометить ветку |
| ChildrenInvalidated | null | невалидно, пропуск |

ChildRemoved пока в игноре. Узлы без RuntimeId помечаются флагом при построении дерева.

## Фаза разработки
Проект в дебаге. Падения должны быть громкими: `throw` на каждой неожиданной развилке лучше строки в логе, которую никто не увидит. Такие `throw` не убирать и не «смягчать».

## Термины
- **Branch** — полная ветка: верхний узел со всеми реальными детьми. Пример: ChildrenInvalidated → перестроили parent.
- **HeeledBranch** — ветка «с пяткой»: сверху прицеплен неполный узел родителя (`Heel`), по нему ясно, куда вернуть ветку. Пример: ChildAdded → дерево от child + parent через walker.

## Правила кода
- Никаких комментариев. Всё объясняют имена.
- Один тип (class/enum/record/interface) — один файл.
- Полные имена без сокращений: `stepsCount`, не `nSteps`; `condition`, не `cf`.
- KISS прежде всего, плюс SOLID/DRY: короткие методы с одной ответственностью; разросшийся класс — повод выносить.
- Визуализация отделена от модели. В бизнес-коде нет `Console.WriteLine`.
- Namespace = путь папки.
- Правя код Дениса, сохранять его имена и форму, менять точечно.

## UIA: проверенные факты
При сомнениях про перф — мерить, не теоретизировать.
- «Кэшированность» — свойство чтения, а не элемента. `GetCached*` никогда не ходит в живое дерево: если свойство или детей не запросили в CacheRequest, будет exception. `Current*` и `GetRuntimeId` всегда живые (cross-process).
- `CacheRequest.TreeScope` — только вниз (Element/Children/Descendants/Subtree); Parent/Ancestors → ArgumentException. Родителей закэшировать нельзя, поэтому снимок строится от окна.
- Condition-walker (`CreateTreeWalker(condition)`) протекает за границу root по siblings и находит элементы других окон. Raw/Control/Content view-walker границу держат.
- Native ControlTypeId начинается с 50000 (`UiaControlType`).
- Имя главного окна Excel меняется (`Excel` ↔ `Book1 - Excel`) — не матчить окно по точному Name.
- RuntimeId переиспользуется после смерти элемента. Встречаются RuntimeId из одних F (битый элемент). Два окна одного Excel имеют один ProcessId и различаются RuntimeId.

## StructureChanged на Excel (инвестигейшен 2026-07-31)
- **ChildAdded** — рабочий, source всегда валиден. Подслучаи:
  - add ≠ source — к source добавлен реальный ребёнок;
  - add == source — в дереве появился сам source; родителя в событии нет, нужен GetParent;
  - source ≠ target с разной нумерацией тоже встречается.
  
  Приходят дубли подряд — нужен дедуп.
- **ChildrenInvalidated** — параметр runtimeId всегда пустой. Половина событий приходит с пустым source (попапы меню вне поддерева окна), остальные дублируют ChildAdded.
- **ChildRemoved** не наблюдался (был в игноре). Bulk* Excel не шлёт.
- Ключ для обновления кэша — RuntimeId у source, а не параметр события.
