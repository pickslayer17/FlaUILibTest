using UIDriver.Tree.Snapshots;

namespace UIDriver.Visualization;

internal static class NodeChangeColors
{
    public static Color Highlight => Color.LightYellow;

    public static Color ForeColorOf(NodeChangeState state) => state switch
    {
        NodeChangeState.Original => Color.Empty,
        NodeChangeState.Added => Color.Green,
        NodeChangeState.Replaced => Color.Blue,
        NodeChangeState.Dirty => Color.Red,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
