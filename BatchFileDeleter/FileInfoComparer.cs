using System.IO;

namespace BatchFileProcessor;

/// <summary>
/// 檔案資訊比較器，提供多種排序方式。
/// </summary>
public static class FileInfoComparer
{
    /// <summary>
    /// 按檔名不分大小寫排序的比較器。
    /// </summary>
    public static readonly IComparer<FileInfo> ByNameIgnoreCase =
        new FileNameComparer(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 按檔名分大小寫排序的比較器。
    /// </summary>
    public static readonly IComparer<FileInfo> ByName =
        new FileNameComparer(StringComparer.Ordinal);

    /// <summary>
    /// 建立自訂檔名比較器。
    /// </summary>
    /// <param name="stringComparer">字串比較器</param>
    /// <returns>檔案比較器</returns>
    public static IComparer<FileInfo> CreateNameComparer(StringComparer stringComparer)
    {
        return new FileNameComparer(stringComparer);
    }

    private sealed class FileNameComparer : IComparer<FileInfo>
    {
        private readonly StringComparer _nameComparer;

        public FileNameComparer(StringComparer nameComparer)
        {
            _nameComparer = nameComparer ?? throw new ArgumentNullException(nameof(nameComparer));
        }

        public int Compare(FileInfo? x, FileInfo? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return _nameComparer.Compare(x.Name, y.Name);
        }
    }
}
