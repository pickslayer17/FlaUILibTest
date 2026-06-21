using FlaUILibTest.Inspector;

namespace FlaUILibTest.Extensions;

public static class OtherExtensions
{
    public static WindowRunTimeId ToWindowRunTimeId(this int[] runTimeId) => new WindowRunTimeId(runTimeId);

    public static string ToFormattedString(this int[] runTimeId) => $"[{string.Join(",", runTimeId)}]";
}
