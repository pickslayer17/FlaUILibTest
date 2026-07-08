using System.Diagnostics;

public sealed class TreeSearch<TWalker, TElement> where TElement : class
{
    private readonly string _label;
    private readonly Func<TElement, TElement?> _getFirstChild;
    private readonly Func<TElement, TElement?> _getNextSibling;
    private readonly Func<TElement, bool> _matches;
    private readonly Func<TElement, object?> _describe;

    public TreeSearch(
        string label,
        Func<TElement, TElement?> getFirstChild,
        Func<TElement, TElement?> getNextSibling,
        Func<TElement, bool> matches,
        Func<TElement, object?> describe)
    {
        _label = label;
        _getFirstChild = getFirstChild;
        _getNextSibling = getNextSibling;
        _matches = matches;
        _describe = describe;
    }

    public TElement? FindFirst(TElement root)
    {
        var stepsCount = 0;
        var stopwatch = Stopwatch.StartNew();

        TElement? Search(TElement node)
        {
            if (_matches(node)) return node;

            var child = _getFirstChild(node);
            stepsCount++;
            while (child != null)
            {
                var found = Search(child);
                if (found != null) return found;

                child = _getNextSibling(child);
                stepsCount++;
            }
            return null;
        }

        var result = Search(root);
        stopwatch.Stop();
        Console.WriteLine($"[FindFirst] time={stopwatch.Elapsed.TotalMilliseconds:F2}ms found={result != null} steps={stepsCount}");
        if (result != null) Console.WriteLine(_describe(result));
        Leaderboard.ReportFindFirst(_label, stopwatch.Elapsed.TotalMilliseconds, result != null, stepsCount);
        return result;
    }

    public List<TElement> FindAll(TElement root)
    {
        var founds = new List<TElement>();
        var stepsCount = 0;
        var stopwatch = Stopwatch.StartNew();

        void Search(TElement node)
        {
            if (_matches(node)) founds.Add(node);

            var child = _getFirstChild(node);
            stepsCount++;
            while (child != null)
            {
                Search(child);

                child = _getNextSibling(child);
                stepsCount++;
            }
        }

        Search(root);
        stopwatch.Stop();
        Console.WriteLine($"[FindAll] time={stopwatch.Elapsed.TotalMilliseconds:F2}ms count={founds.Count} steps={stepsCount}");
        foreach (var (element, index) in founds.Select((element, index) => (element, index)))
            Console.WriteLine($"[{index}] - {_describe(element)}");
        Leaderboard.ReportFindAll(_label, stopwatch.Elapsed.TotalMilliseconds, founds.Count, stepsCount);
        return founds;
    }
}
