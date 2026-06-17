using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;

namespace FlaUILibTest.Inspector;

public enum ModuleState
{
    NotReady,
    Ready,
    Updated,
    NotInitialized
}

public class Module
{
    private readonly AutomationElement _parentElement;
    private readonly List<AutomationSubscriberBase> _subscribers = new();
    private readonly object _subscribersLock = new();

    public AutomationElement? Self { get; private set; }
    public ConditionBase SelfCondition { get; }
    private ModuleState _state = ModuleState.NotReady;

    public ModuleState State => _state;

    public Module(AutomationElement parentElement, ConditionBase condition)
    {
        _parentElement = parentElement;
        SelfCondition = condition;
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

    public void AddSubscriber(AutomationSubscriberBase subscriber)
    {
        lock (_subscribersLock)
        {
            _subscribers.Add(subscriber);
        }
    }

    public void RemoveSubscriber(AutomationSubscriberBase subscriber)
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
        try
        {
            if (Self == null) return;

            List<AutomationSubscriberBase> snapshot;
            lock (_subscribersLock)
            {
                snapshot = new List<AutomationSubscriberBase>(_subscribers);
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
        }
    }

    private void InvalidateAllSubscribers()
    {
        List<AutomationSubscriberBase> snapshot;
        lock (_subscribersLock)
        {
            snapshot = new List<AutomationSubscriberBase>(_subscribers);
        }

        foreach (var element in snapshot)
        {
            element.Update(null);
        }
    }
}
