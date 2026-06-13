using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUILibTest.Interfaces;

namespace FlaUILibTest;

public enum ModuleState
{
    NotReady,
    Ready,
    Updated,
    NotInitialized
}

public class Module : ISubscriber
{
    private readonly AutomationElement _parentElement;
    private readonly List<ISubscriber> _subscribers = new();
    private readonly object _subscribersLock = new();
    private readonly SemaphoreSlim _rebuildLock = new(1, 1);

    public AutomationElement? Self { get; private set; }
    public ConditionBase SelfCondition { get; }
    private ModuleState _state = ModuleState.NotReady;

    public ModuleState State => _state;

    public Module(AutomationElement parentElement, ConditionBase condition)
    {
        _parentElement = parentElement;
        SelfCondition = condition;
        EventManager.Instance.Register(this);
    }

    public void TryInitialize()
    {
        try
        {
            var found = _parentElement.FindFirstDescendant(SelfCondition);
            if (found != null)
            {
                Self = found;
                _state = ModuleState.Ready;
                Rebuild();
            }
        }
        catch { }
    }

    public void Update(AutomationElement? element)
    {
        if (element != null)
        {
            Self = element;
            _state = ModuleState.Ready;
            Rebuild();
        }
        else
        {
            Self = null;
            _state = ModuleState.NotReady;
            InvalidateAllSubscribers();
        }
    }

    public void AddSubscriber(ISubscriber subscriber)
    {
        lock (_subscribersLock)
        {
            _subscribers.Add(subscriber);
        }
    }

    public void RemoveSubscriber(ISubscriber subscriber)
    {
        lock (_subscribersLock)
        {
            _subscribers.Remove(subscriber);
        }
    }

    public bool MatchesEvent(AutomationElement source, StructureChangeType changeType, int[] runtimeId)
    {
        try { return ConditionMatcher.Matches(source, SelfCondition); }
        catch { return false; }
    }

    public void Notify(AutomationElement source, StructureChangeType changeType)
    {
        switch (changeType)
        {
            case StructureChangeType.ChildAdded:
                Self = source;
                _state = ModuleState.Ready;
                Rebuild();
                break;

            case StructureChangeType.ChildRemoved:
                Self = null;
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
            if (Self == null) return;

            List<ISubscriber> snapshot;
            lock (_subscribersLock)
            {
                snapshot = new List<ISubscriber>(_subscribers);
            }

            foreach (var element in snapshot)
            {
                try
                {
                    var found = Self.FindFirstDescendant(element.SelfCondition);
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
        List<ISubscriber> snapshot;
        lock (_subscribersLock)
        {
            snapshot = new List<ISubscriber>(_subscribers);
        }

        foreach (var element in snapshot)
        {
            element.Update(null);
        }
    }
}
