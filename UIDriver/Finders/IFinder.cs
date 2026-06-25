namespace UIDriver;

public interface IFinder
{
    AutomationElementObject? Find(AutomationElementObject source);
    AutomationElementObject[] FindAll(AutomationElementObject source);
    bool Matches(AutomationElementObject source);
}
