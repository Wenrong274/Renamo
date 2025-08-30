using System.Text.RegularExpressions;

namespace SmartFileSelector;

public class RenamePatternParser
{
    private static readonly Regex Pattern = new Regex(@"^(.*)\{(0+)\}$", RegexOptions.Compiled);

    public static (string customName, int digitCount) Parse(string input)
    {
        var match = Pattern.Match(input);
        if (!match.Success)
            throw new ArgumentException("格式錯誤，必須是 {自訂檔名}_{00 or 000}");

        string customName = match.Groups[1].Value;
        int digitCount = match.Groups[2].Value.Length;

        return (customName, digitCount);
    }
}
