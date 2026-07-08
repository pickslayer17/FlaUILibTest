static class Probe
{
    public static void Run(UiNode root, UiNodeWalker walker, string xpath)
    {
        try
        {
            var navigator = new UiNodeNavigator(root, walker);
            var iterator = navigator.Select(xpath);
            var count = 0;
            var first = "";
            while (iterator.MoveNext())
            {
                count++;
                if (count == 1)
                {
                    var node = ((UiNodeNavigator)iterator.Current).Current;
                    first = $"[{node.ControlType}] id='{node.AutomationId}' class='{node.ClassName}'";
                }
            }
            Console.WriteLine($"count={count,3}  first={first,-55}  <- {xpath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"THROW {ex.GetType().Name}: {ex.Message}  <- {xpath}");
        }
    }
}
