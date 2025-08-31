using System.Text.RegularExpressions;

namespace SmartFileSelector.Core;

public class RenamePatternParser
{
    private static readonly Regex Pattern = new Regex(@"^(.*)\{(0+)\}$", RegexOptions.Compiled);

    /// <summary>
    /// 解析批次更名的樣板字串，取得自訂檔名前綴與流水號長度。
    /// <para>使用規則：</para>
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///     樣板字串必須包含一組 <c>{0...}</c>，大括號內只能有連續的零。<br/>
    ///     例如：
    ///       <c>"MyFile_{00}"</c> → 前綴 = "MyFile_"，流水號長度 = 2<br/>
    ///       <c>"Report_{000}"</c> → 前綴 = "Report_"，流水號長度 = 3
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     回傳結果：<br/>
    ///     <c>customName</c> = 大括號之前的字串（會完整保留，含底線等字元）。<br/>
    ///     <c>digitCount</c> = 大括號內 0 的數量。
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     不合法輸入會拋出 <see cref="ArgumentException"/>：<br/>
    ///     - 缺少大括號 (如 "MyFile_00")<br/>
    ///     - 大括號內含非零字元 (如 "{0a}"、"{01}")<br/>
    ///     - 大括號為空 (如 "{}")
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     若輸入為 <c>null</c> 則拋出 <see cref="ArgumentNullException"/>。
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>總結：格式必須是「自訂檔名前綴 + { + 一串零 + }」。</para>
    /// <para>例：<c>{00}</c> → 兩位數流水號；<c>{000}</c> → 三位數流水號。</para>
    /// </summary>
    /// <param name="input">樣板字串，例如 <c>"Photo_{000}"</c>。</param>
    /// <returns>
    /// 回傳一組 (customName, digitCount)：<br/>
    /// - <c>customName</c>：自訂檔名前綴<br/>
    /// - <c>digitCount</c>：流水號位數
    /// </returns>
    /// <exception cref="ArgumentException">格式錯誤，樣板不符合規則時拋出。</exception>
    /// <exception cref="ArgumentNullException">輸入為 null 時拋出。</exception>
    public static (string customName, int digitCount) Parse(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var match = Pattern.Match(input);
        if (!match.Success)
            throw new ArgumentException("格式錯誤，必須是 {自訂檔名}_{00 或 000}");

        string customName = match.Groups[1].Value;
        int digitCount = match.Groups[2].Value.Length;
        return (customName, digitCount);
    }
}
