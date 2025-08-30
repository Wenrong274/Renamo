namespace BatchFileProcessor;

/// <summary>
/// 常用的檔案刪除規則。
/// </summary>
public static class DeletionRules
{
    /// <summary>
    /// 建立每隔 n 個檔案刪除一個的規則。
    /// 以 (n+1) 個檔案為一組，刪除每組的最後一個檔案。
    /// </summary>
    /// <param name="interval">間隔數量，必須大於 0</param>
    /// <param name="startOffset">起始偏移量，預設為 0</param>
    /// <returns>刪除條件委派</returns>
    /// <example>
    /// interval=2 => 每3個為一組，刪除索引 2, 5, 8... 
    /// interval=3 => 每4個為一組，刪除索引 3, 7, 11...
    /// interval=100 => 每101個為一組，刪除索引 100, 201, 302...
    /// </example>
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
            return positionInGroup == interval; // 刪除每組的最後一個
        };
    }

    /// <summary>
    /// 建立分組保留與刪除的規則。
    /// 每組保留前 keepCount 個檔案，刪除後續 deleteCount 個檔案。
    /// </summary>
    /// <param name="keepCount">每組保留的檔案數量</param>
    /// <param name="deleteCount">每組刪除的檔案數量</param>
    /// <returns>刪除條件委派</returns>
    /// <example>
    /// keepCount=2, deleteCount=1 => 
    /// 索引 0,1,2: 保留 [0,1], 刪除 [2]
    /// 索引 3,4,5: 保留 [3,4], 刪除 [5]
    /// </example>
    public static Func<int, bool> KeepThenDelete(int keepCount, int deleteCount)
    {
        if (keepCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(keepCount), "保留數量必須大於 0");
        if (deleteCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(deleteCount), "刪除數量必須大於 0");

        var groupSize = keepCount + deleteCount;
        return index =>
        {
            var positionInGroup = index % groupSize;
            return positionInGroup >= keepCount;
        };
    }

}
