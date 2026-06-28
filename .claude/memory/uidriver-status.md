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
- **НЕДОРАЗОБРАНО (на завтра):** нестабильность результата между прогонами — тот же код/condition даёт то count=2/steps=5, то count=35/steps=71. Похоже на утечку состояния (static `automation`? накопление окон/COM-объектов? переиспользование RuntimeId?). RoundTrip через `WrapNativeElement().ToNative()` НЕ виноват (с ним и без — одинаково). Эффект «count=1 потом стал 39 через 2 сек» при синхронном коде — скорее всего артефакт буферизации/склейки вывода в терминале VS Code, а не изменение данных в памяти. Проверить на чистом старте Excel, убрав лишние окна.
- Полезная находка: невалидный AutomationId возвращает false за ~170мс (UIA не чешет всё дерево впустую — есть внутренние индексы/таблицы). Не проверяли разницу скорости поиска по AutomationId vs Name vs ClassName — на будущее.

**Невыводимые из кода гочи:**
- `OnWindowOpened` читает id ЖИВЫМ запросом → racy так же, как было на закрытии. Транзитные окна ОС умирают до обработчика → `Failed to get RuntimeId` (безвредно, finder им просто не заводится). Это НЕ «окно уже было» (то — тихий дедуп без ошибки).
- RuntimeId переиспользуется после смерти элемента → риск stale-finder, если `WindowClosed` не пойман. По заметкам: LoginDialog вообще не шлёт WindowClosed.
- desktop-finder ОБЯЗАН слушать (Subtree WindowOpened) — иначе не обнаруживаются новые окна; пробовали убрать — сломалось, вернули.
