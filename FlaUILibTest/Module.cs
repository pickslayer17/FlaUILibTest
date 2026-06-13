using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System.Xml.Linq;

namespace FlaUILibTest;

public enum ModuleState
{
    NotReady,
    Ready,
    Updated
}

public class Module
{
    private readonly AutomationElement _window;
    private readonly List<Element> _subscribers = new();
    private readonly object _subscribersLock = new();
    private readonly SemaphoreSlim _rebuildLock = new(1, 1);

    private AutomationElement? _anchor;
    private ConditionBase _condition;
    private ModuleState _state = ModuleState.NotReady;

    public ModuleState State => _state;

    public Module(AutomationElement window, ConditionBase condition)
    {
        _window = window;
        _condition = condition;
        EventManager.Instance.Register(this);
    }

    public void AddSubscriber(Element element)
    {
        lock (_subscribersLock)
        {
            _subscribers.Add(element);
        }
    }

    public void RemoveSubscriber(Element element)
    {
        lock (_subscribersLock)
        {
            _subscribers.Remove(element);
        }
    }

    public bool MatchesEvent(AutomationElement source, StructureChangeType changeType, int[] runtimeId)
    {
        try { return ConditionMatcher.Matches(source, _condition); }
        catch { return false; }
    }

    public void Notify(AutomationElement source, StructureChangeType changeType)
    {
        switch (changeType)
        {
            case StructureChangeType.ChildAdded:
                _anchor = source;
                _state = ModuleState.Ready;
                Rebuild();
                break;

            case StructureChangeType.ChildRemoved:
                _anchor = null;
                _state = ModuleState.NotReady;
                InvalidateAllSubscribers();
                break;

            case StructureChangeType.ChildrenInvalidated:
                _state = ModuleState.Updated;
                Rebuild();
                break;
        }
    }

    private void Rebuild()
    {
        _rebuildLock.Wait();
        try
        {
            if (_anchor == null) return;

            List<Element> snapshot;
            lock (_subscribersLock)
            {
                snapshot = new List<Element>(_subscribers);
            }

            foreach (var element in snapshot)
            {
                try
                {
                    var found = _anchor.FindFirstDescendant(element.Condition);
                    element.Update(found);
                }
                catch
                {
                    element.Update(null);
                }
            }

            _state = ModuleState.Ready;
        }
        finally
        {
            _rebuildLock.Release();
        }
    }

    private void InvalidateAllSubscribers()
    {
        List<Element> snapshot;
        lock (_subscribersLock)
        {
            snapshot = new List<Element>(_subscribers);
        }

        foreach (var element in snapshot)
        {
            element.Update(null);
        }
    }
}
