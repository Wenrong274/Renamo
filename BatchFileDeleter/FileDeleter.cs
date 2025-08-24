using System.IO;

namespace BatchFileDeleter;

public static class FileDeleter
{
    /// <summary>
    /// 依條件列舉「待刪檔案候選」。
    /// 預設以檔名 (OrdinalIgnoreCase) 排序，確保結果可重現。
    /// 索引 i 為 0-based，且隨排序結果由小到大遞增。
    /// </summary>
    public static IEnumerable<FileInfo> EnumerateFilesForDeletion(
        string folderPath,
        Func<int, FileInfo, bool> shouldDelete,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        IComparer<FileInfo>? comparer = null)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentNullException(nameof(folderPath));

        ArgumentNullException.ThrowIfNull(shouldDelete);

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

        // 注意：排序需要一次收齊，對超大資料夾會使用較多記憶體
        var files = new DirectoryInfo(folderPath)
            .GetFiles(searchPattern, searchOption)
            .OrderBy(f => f, comparer ?? FileInfoNameComparer.OrdinalIgnoreCase);

        int i = 0;
        foreach (var file in files)
        {
            if (shouldDelete(i, file))
                yield return file;
            i++;
        }
    }

    /// <summary>
    /// 僅依「索引（0-based）」決定是否刪除。
    /// </summary>
    public static IEnumerable<FileInfo> SelectFilesForDeletion(
        string folderPath,
        Predicate<int> shouldDeleteIndex,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        IComparer<FileInfo>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(shouldDeleteIndex);

        return EnumerateFilesForDeletion(
            folderPath,
            (idx, _) => shouldDeleteIndex(idx),
            searchPattern,
            searchOption,
            comparer
        );
    }

    public sealed class FileInfoNameComparer : IComparer<FileInfo>
    {
        private readonly StringComparer nameComparer;

        public static readonly FileInfoNameComparer OrdinalIgnoreCase = new(StringComparer.OrdinalIgnoreCase);

        public FileInfoNameComparer(StringComparer nameComparer)
        {
            this.nameComparer = nameComparer;
        }

        public int Compare(FileInfo? x, FileInfo? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return nameComparer.Compare(x.Name, y.Name);
        }
    }

    /// <summary>
    /// 從 offset（0-based）開始，每隔 n 個即命中刪除。
    /// 例：n=2, offset=0 => 刪除 0,2,4,6...
    /// </summary>
    public static Predicate<int> DeleteEveryNth(int n, int offset = 0)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        return i => i >= offset + n && ((i - offset) % n == 0);
    }

    /// <summary>
    /// 以組為單位（組大小 = take + skip）：每組保留前 take 個、刪除後續 skip 個。
    /// 例：take=2, skip=1 => 對索引 0..2、3..5、6..8... 每組保留 [0,1] 刪除 [2]；保留 [3,4] 刪除 [5]；...
    /// </summary>
    public static Predicate<int> DeleteByTakeSkip(int take, int skip)
    {
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));
        if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));

        int group = take + skip;
        if (group == 0) throw new ArgumentOutOfRangeException(nameof(skip), "take + skip must be > 0");

        return i =>
        {
            int r = i % group;
            return r >= take; // 刪除在每組中落在 take 之後的那些
        };
    }
}
