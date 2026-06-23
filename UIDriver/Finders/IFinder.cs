using FlaUI.Core.AutomationElements;

namespace UIDriver;

// Чистая СИНХРОННАЯ «инструкция как искать»: знает только КАК найти от source по своим условиям.
// Один проход, без ожидания/ретраев/таймаута — этим занимается Watcher, гоняя Find на каждый Poke.
public interface IFinder
{
    AutomationElement? Find(AutomationElement source);
    AutomationElement[] FindAll(AutomationElement source);
}
