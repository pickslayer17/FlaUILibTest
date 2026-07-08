public static class Leaderboard
{
    private static readonly List<(string Label, double Time, int Count, int steps)> _findFirst = new();
    private static readonly List<(string Label, double Time, int Count, int steps)> _findAll = new();

    public static void ReportFindFirst(string label, double time, bool found, int steps)
    {
        _findFirst.Add((label, time, found ? 1 : 0, steps));
    }

    public static void ReportFindAll(string label, double time, int count, int steps)
    {
        _findAll.Add((label, time, count, steps));
    }

    public static void PrintResults()
    {
        Print("FindFirst", _findFirst);
        Print("FindAll", _findAll);
    }

    private static void Print(string title, List<(string Label, double Time, int Count, int steps)> results)
    {
        var sorted = results.OrderBy(result => result.Count == 0).ThenBy(result => result.Time).ToList();
        var labelWidth = sorted.Count == 0 ? 0 : sorted.Max(result => result.Label.Length);

        Console.WriteLine($"\n========== {title} ==========");
        foreach (var (item, rank) in sorted.Select((item, index) => (item, index + 1)))
            Console.WriteLine($"[{rank,2}]  {item.Label.PadRight(labelWidth)}  {item.Time,8:F2}ms  count={item.Count}, steps = {item.steps}");
    }
}
