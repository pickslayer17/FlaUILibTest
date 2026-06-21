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

**Невыводимые из кода гочи:**
- `OnWindowOpened` читает id ЖИВЫМ запросом → racy так же, как было на закрытии. Транзитные окна ОС умирают до обработчика → `Failed to get RuntimeId` (безвредно, finder им просто не заводится). Это НЕ «окно уже было» (то — тихий дедуп без ошибки).
- RuntimeId переиспользуется после смерти элемента → риск stale-finder, если `WindowClosed` не пойман. По заметкам: LoginDialog вообще не шлёт WindowClosed.
- desktop-finder ОБЯЗАН слушать (Subtree WindowOpened) — иначе не обнаруживаются новые окна; пробовали убрать — сломалось, вернули.
