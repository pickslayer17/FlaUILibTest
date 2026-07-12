namespace UIDriver;

public interface IFinder
{
    UIAutomationElement? Find(UIAutomationElement source);
    UIAutomationElement[] FindAll(UIAutomationElement source);
}
