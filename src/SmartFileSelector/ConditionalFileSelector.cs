namespace SmartFileSelector;

public static class ConditionalFileSelector
{
    /// <summary>
    ///  選擇符合條件的檔案。
    /// 檔案會依指定比較器排序，預設為檔名不分大小寫排序。
    /// </summary>
    /// <param name="folderPath">目標資料夾路徑</param>
    /// <param name="shouldSelect"> 檔案刪除條件</param>
    /// <param name="searchPattern">檔案搜尋模式，預設為 "*"</param>
    /// <param name="searchOption">搜尋選項，預設僅搜尋頂層目錄</param>
    /// <param name="comparer">檔案排序比較器，預設為檔名不分大小寫比較</param>
    /// <returns>符合條件的檔案集合</returns>
    /// <exception cref="ArgumentNullException">當必要參數為 null 時拋出</exception>
    /// <exception cref="DirectoryNotFoundException">當目標資料夾不存在時拋出</exception>
    public static IEnumerable<FileInfo> EnumerateFiles(
        string folderPath,
        Func<int, FileInfo, bool> shouldSelect,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        IComparer<FileInfo>? comparer = null)
    {
        ValidateInputs(folderPath, shouldSelect);

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"資料夾不存在: {folderPath}");

        // 注意：排序需要將所有檔案載入記憶體，對於大型資料夾可能消耗較多記憶體
        var sortedFiles = new DirectoryInfo(folderPath)
            .GetFiles(searchPattern, searchOption)
            .OrderBy(file => file, comparer ?? FileInfoComparer.ByNameIgnoreCase);

        return sortedFiles
            .Select((file, index) => new { File = file, Index = index })
            .Where(item => shouldSelect(item.Index, item.File))
            .Select(item => item.File);
    }

    /// <summary>
    /// 僅基於索引位置選擇要的檔案。
    /// </summary>
    /// <param name="folderPath">目標資料夾路徑</param>
    /// <param name="shouldSelectIndex">索引條件，參數為 0-based 索引</param>
    /// <param name="searchPattern">檔案搜尋模式</param>
    /// <param name="searchOption">搜尋選項</param>
    /// <param name="comparer">檔案排序比較器</param>
    /// <returns>符合索引刪除條件的檔案集合</returns>
    public static IEnumerable<FileInfo> SelectFilesByIndex(
        string folderPath,
        Func<int, bool> shouldSelectIndex,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        IComparer<FileInfo>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(shouldSelectIndex);

        return EnumerateFiles(
            folderPath,
            (index, _) => shouldSelectIndex(index),
            searchPattern,
            searchOption,
            comparer);
    }

    /// <summary>
    /// 驗證輸入參數的有效性。
    /// </summary>
    private static void ValidateInputs(string folderPath, Func<int, FileInfo, bool> shouldDelete)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("資料夾路徑不能為空", nameof(folderPath));

        ArgumentNullException.ThrowIfNull(shouldDelete);
    }
}