---
name: uidriver-status
description: UIDriver (FlaUI event-driven desktop automation) — current state, what's done, what's deferred by choice
metadata:
  type: project
---

UIDriver — событийная библиотека автоматизации Windows-десктопа на FlaUI/UIA3 (аналог Playwright для нативного UI). Тестируется на Excel + DealCloud аддын. .NET 9, C# preview.

**Статус на 2026-06-21:** живой механизм работы с окнами готов и проверен на Excel/Format Cells. Ключевые проблемы закрыты (структурное равенство ключа `WindowRunTimeId`; watch-first в `RegisterAndGetElementAsync`; таймаут с отменой; `WindowClosed` на `TreeScope.Element` + id берётся из захваченного при жизни `RootRuntimeId`; способ 2 — `IElementSource`/`WindowElementSource`, Locator знает только условие; монитор `FlaUIMonitor` как отдельный WinForms-проект + статик `MonitorHelper`). **Следующий шаг: вставлять в реальный живой тест.**

**Отложено сознательно (не баги, решение пользователя):**
- `FinderBase.Dispose` — заглушка; UIA-подписки не снимаются (registration-объекты сохранены в полях, но не используются). Утечка хендлеров на долгой сессии.
- `WindowOpened` заводит finder на КАЖДОЕ открытое окно (desktop слушает Subtree) — включая блокнот/cmd/скриншот. Лишнее, но «пока хрен с ним». Чистый фикс = фильтр по ProcessId/ClassName в `WindowOpened` (заодно снимет «Failed to get RuntimeId» на транзитных окнах).
- `FindAll`/возврат коллекции — `IElementSource.FindAllAsync` пока `NotImplementedException`.
- `By` — хелперы (`By.Button` и т.п.) ещё не сделаны, условия строятся лямбдой `cf => ...`.

**Инвестигейт стратегии поиска (2026-06-27/28, в FlaUILibTest/Program.cs, не в проекте):**
- Замерили: `FindFirstDescendant` ~90мс; ручной обход raw-walker ~0.8мс/шаг; FindFirst через condition-walker быстр (1 шаг) только потому что рано останавливается.
- **Interop ≡ FlaUI по скорости** — доказано, спускаться на голый Interop ради скорости смысла нет.
- **condition-walker (`CreateTreeWalker(property-condition)`) ПРОТЕКАЕТ за границу root**: ищешь Close в одном окне Excel — `GetNextSibling` тащит Close со всего десктопа (находил 35-39 кнопок). `GetFirstChild` (спуск) границу держит, а `GetNextSibling` (вбок) — нет. Это и есть «баг» в семантике condition-walker.
- **raw/control/content view-walker'ы границу root ДЕРЖАТ** (count строго по окну). → фундамент движка = view-walker (raw) + свой matcher, condition-walker из навигации выкинуть.
- **PropertyMatcher (поэлементное чтение свойств) добавляет ~600мс** на полный обход окна (~905 элементов). Рычаг на будущее: `CacheRequest` — грузить свойства пакетом вместе с элементом при обходе (FlaUI это умеет: ветки `...BuildCache`).
- Нестабильность count между прогонами (2↔35) РАЗОБРАНА: это condition-walker протекает за root и ловит Close из ВСЕХ окон десктопа — число зависит от того, сколько окон открыто в момент прогона, а не от кода. RoundTrip `WrapNativeElement().ToNative()` НЕ виноват. «count менялся через 2 сек» — артефакт буферизации вывода терминала VS Code, не данные.
- Полезная находка: невалидный AutomationId возвращает false за ~170мс (UIA не чешет всё дерево впустую — есть внутренние индексы/таблицы). Не проверяли разницу скорости поиска по AutomationId vs Name vs ClassName — на будущее.

**★★★ ФИНАЛЬНОЕ РЕШЕНИЕ ДВИЖКА (2026-06-28, после всего исследования) ★★★**
- **FindFirst → FlaUI `GetCustomTreeWalker(condition)`** (кейс [11]). Native custom быстрее лишь на ~1мс — не стоит возни с Interop. FlaUI комфортнее в использовании.
- **FindAll → HYBRID (кейс [12], построен нами)** — бесспорный фаворит, обогнал ВСЕХ: 74мс/2 элемента/12 шагов против view-walker'ов 100-110мс/301 шаг. И корректнее (Content потерял один элемент).
- **Механизм HybridSearch:** condition-walker даёт кандидатов даром (телепорт к совпадениям, НЕ обход дерева) → каждого проверяем `IsPresentInWindow` (raw-walker): сначала ProcessId == windowProcessId (отсекает 34 из 36 мгновенно без подъёма), потом `GetParent` вверх по RuntimeId до window(true)/desktop(false)/null(exception). Прошедшие → результат.
- **Почему быстрее view-walker:** гибрид = O(число кандидатов) шагов (12), view-walker = O(весь размер дерева) шагов (301-1809). На больших деревьях разрыв растёт в разы.
- **ПОДТВЕРЖДЕНО:** condition-walker отдаёт совпадения упорядоченно «от окна вниз» — наши 2 идут ПЕРВЫМИ, потом первый чужой → можно `break` (steps=12, не 36). НО это эмпирика, не гарантия UIA — на проде оставить флаг «строгий режим без break» (полный проход) как fallback.
- У всех элементов есть ProcessId и RuntimeId (exception ни разу не кинуло). 2 окна одного Excel = один ProcessId → ProcessId отсекает чужие приложения, RuntimeId различает окна одного приложения. Оба нужны.

**ГЛАВНЫЙ ВЫВОД полного прогона 11 walker'ов (2026-06-28, FindFirst+FindAll, цели Close на поверхности и A1 глубоко в гриде ~1800 узлов):**
- **РЕВИЗИЯ прежнего «Interop ≡ FlaUI»:** на лёгкой цели (Close) равны, НО на тяжёлом обходе (A1, 1800 узлов) **native Interop в ~2-3x быстрее FlaUI** при ИДЕНТИЧНОМ числе шагов. Причина: FlaUI на каждом `GetFirstChild`/`GetNextSibling` делает `ToNative()`+`WrapNativeElement()` (managed-обёртка на каждый узел). Native ходит по COM напрямую. A1 FindFirst: native raw ~128мс vs FlaUI raw ~328мс; FindAll: native ~500мс vs FlaUI ~900мс. → **уход на Interop ради скорости ТЕПЕРЬ оправдан** (реальный 2x на больших деревьях, не 2мс).
- **FindFirst — лидер: condition-walker** (`CreateTreeWalker(condition)`): A1 за 1 шаг ~61-71мс, остальные 130-330мс (сотни шагов). Граница не важна — берёт первый матч и стоп, первый матч всегда в окне.
- **FindAll — condition-walker ХУДШИЙ** (протекает за root + ~560мс), лидер = **view-walker (raw/control/content) + свой matcher** (~500мс native, держит границу).
- ПОДТВЕРЖДЕНО на 5 экземплярах Excel: при FindAll condition-walker'ы (`native CreateTreeWalker(convertedCondition)`, `FlaUI GetCustomTreeWalker`) нашли A1 во ВСЕХ 5 окнах (count=5, медленно 1000-1240мс) — протечка за root по всему десктопу. Остальные 9 walker'ов (native RawViewWalker/ControlViewWalker/ContentViewWalker, native CreateTreeWalker(Raw/Control/Content ViewCondition), FlaUI Raw/Control/Content ViewWalker) нашли только своё окно (count=1). Негативный сценарий (нет A1): все 11 → count=0, честно. ИТОГ-топ: FindFirst → FlaUI GetCustomTreeWalker; FindAll → native RawViewWalker + свой matcher.
- ControlType: FlaUI `ControlType` enum — ПОРЯДКОВЫЙ (Button=2), а нативный UIA ControlTypeId = 50000. Для native-матчинга конвертить через `ControlTypeConverter.ToControlTypeNative()`, не `Convert.ToInt32` напрямую.
- Тестовый стенд: `FlaUILibTest/Program.cs` — generic `TreeSearch<TWalker,TElement>` (делегаты навигации/matcher/describe), 11 кейсов в Main, `targetCondition` подменяется close↔A1, `Leaderboard` печатает WINNERS/LOSERS. `PropertyMatcher` (FlaUI) и `NativePropertyMatcher` (Interop) — рекурсивный разбор ConditionBase. Старьё в классе `old`.

**Открытый приём (2026-06-28, проверено): два walker'а вместе.**
- Элемент, найденный condition-walker'ом — это НАСТОЯЩИЙ COM-узел в реальном дереве (condition-walker наврал только про СВЯЗИ/siblings, схлопнув дерево; сам узел реальный, со своим настоящим parent). Конвенция: элемент валиден независимо от того, какой walker его добыл.
- **Найти настоящих родных siblings (один parent, не following-sibling) / parent:** FindFirst чем угодно (быстрый condition-walker) → прокинуть найденный элемент в RawViewWalker → навигировать от него (`GetNextSibling`/`GetParent`) по РЕАЛЬНОМУ дереву. Raw уважает настоящие связи. Проверено: raw от переданного scrollbar не идёт дальше него, если у того нет реальных siblings (а не хватает чужих с десктопа).
- **Идея решения «condition-walker протекает за окно»:** наложить два дерева. condition-walker даёт «кто подходит» (но без границ окна, схлопнуто, уровень вложенности сохранён). raw даёт «кто под кем / в каком окне». Пересечение = все подходящие элементы СТРОГО в своём окне — то, что нужно для FindAll. Реализация позже.
- Гоча native Interop: у элемента есть Current* и Cached* свойства. Cached часто ПУСТОЙ (CachedName вернул ничего) — кэш надо сначала наполнить через CacheRequest/BuildCache, иначе читать Current*. На будущее: проверять кэш, падать на Current если пусто.

**Невыводимые из кода гочи:**
- `OnWindowOpened` читает id ЖИВЫМ запросом → racy так же, как было на закрытии. Транзитные окна ОС умирают до обработчика → `Failed to get RuntimeId` (безвредно, finder им просто не заводится). Это НЕ «окно уже было» (то — тихий дедуп без ошибки).
- RuntimeId переиспользуется после смерти элемента → риск stale-finder, если `WindowClosed` не пойман. По заметкам: LoginDialog вообще не шлёт WindowClosed.
- desktop-finder ОБЯЗАН слушать (Subtree WindowOpened) — иначе не обнаруживаются новые окна; пробовали убрать — сломалось, вернули.
