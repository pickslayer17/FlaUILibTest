using UIDriver.Uia.Constants;

namespace UIDriver.Uia.Listening;

public interface IStructureChangedListener
{
    public void NotifyOnStructureChanged(UiaElement source, UiaStructureChangeType changeType, RunTimeId? targetRunTimeId);
}
