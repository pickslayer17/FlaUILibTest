namespace UIDriver.CustomModels;

public static class RunTimeIdExtensions
{
    public static string ToHexString(this int[] id) => string.Join(",", id.Select(part => part.ToString("X")));

    public static string ToDisplayString(this int[] id) => string.Join(",", id);

    public static bool RuntimeIdEquals(this int[] id, int[] other) => id.SequenceEqual(other);
}
