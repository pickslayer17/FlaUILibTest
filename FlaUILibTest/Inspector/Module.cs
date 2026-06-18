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
    private readonly ModuleFinder _moduleFinder;
    private readonly List<AutomationSubscriberBase> _subscribers = new();
    private List<AutomationSubscriberBase> Subscribers
    {
        get
        {
            lock (_subscribersLock)
            {
                return _subscribers;
            }
        }
    }

    private readonly object _subscribersLock = new();

    public AutomationElement? Self { get; private set; }
    private ModuleState _state = ModuleState.NotReady;

    public ModuleState State => _state;

    public Module(ModuleFinder moduleFinder, AutomationElement self)
    {
        Self = self;
        _moduleFinder = moduleFinder;
    }

    public void AddSubscriber(AutomationSubscriberBase subscriber)
    {
        Subscribers.Add(subscriber);
        subscriber.SetModule(this);
    }

    public void RemoveSubscriber(AutomationSubscriberBase subscriber)
    {
        Subscribers.Remove(subscriber);
    }

    public void Notify(StructureChangeType changeType)
    {
        switch (changeType)
        {
            case StructureChangeType.ChildAdded:
                _state = ModuleState.Updated;
                Rebuild();
                _state = ModuleState.Ready;
                break;

            case StructureChangeType.ChildRemoved:
                Self = null;
                _state = ModuleState.Updated;
                InvalidateAllSubscribers();
                _state = ModuleState.NotReady;
                break;

            case StructureChangeType.ChildrenInvalidated:
                _state = ModuleState.Updated;
                Rebuild();
                _state = ModuleState.Ready;
                break;
        }
    }

    private void Rebuild()
    {
        Console.WriteLine("REBUILD by module");
        var snapshot = new List<AutomationSubscriberBase>(Subscribers);
        foreach (var element in snapshot)
        {
            try
            {
                var found = _moduleFinder.DefaultSearch(Self, element.SelfCondition);
                if (found != null)
                    element.Update(found);
                else
                    element.Update(null);
            }
            catch
            {
                element.Update(null);
            }
        }
    }

    private void InvalidateAllSubscribers()
    {
        Console.WriteLine("INVALIDATE CHILD in module ");
        var snapshot = new List<AutomationSubscriberBase>(Subscribers);
        foreach (var element in snapshot)
        {
            element.Update(null);
        }
    }
}
