namespace SmartFileSelector.Core;

/// <summary>
///  檔案刪除規則。
/// </summary>
public static class FileSelectorRules
{
    public static Func<int, bool> EveryNth(int interval, int startOffset = 0)
    {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "間隔必須大於 0");
        if (startOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(startOffset), "偏移量不能為負數");

        int groupSize = interval + 1;

        return index =>
        {
            var adjustedIndex = index - startOffset;
            if (adjustedIndex < 0) return false;

            var positionInGroup = adjustedIndex % groupSize;
            return positionInGroup == interval;
        };
    }


    public static Func<int, bool> KeepThenSelete(int keepCount, int seleteCount)
    {
        if (keepCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(keepCount), "保留數量必須大於 0");
        if (seleteCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(seleteCount), "選擇數量必須大於 0");

        var groupSize = keepCount + seleteCount;
        return index =>
        {
            var positionInGroup = index % groupSize;
            return positionInGroup >= keepCount;
        };
    }

}
