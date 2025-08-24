using System.IO;

namespace BatchFileDeleter;

public static class FileDeleter
{
    /// <summary>
    /// 依條件列舉「待刪檔案候選」。預設以檔名(OrdinalIgnoreCase)排序，確保結果可重現。
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

        var files = new DirectoryInfo(folderPath)
            .GetFiles(searchPattern, searchOption)
            .AsEnumerable();

        files = (comparer ?? FileInfoNameComparer.OrdinalIgnoreCase).SortIfComparer(files);

        int i = 0;
        foreach (var file in files)
        {
            if (shouldDelete(i, file))
                yield return file;

            i++;
        }
    }

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

    private static IEnumerable<FileInfo> SortIfComparer(
        this IComparer<FileInfo> comparer,
        IEnumerable<FileInfo> files)
    {
        return files.OrderBy(f => f, comparer);
    }

    public sealed class FileInfoNameComparer : IComparer<FileInfo>
    {
        public static readonly FileInfoNameComparer OrdinalIgnoreCase = new(StringComparer.OrdinalIgnoreCase);
        private readonly StringComparer nameComparer;

        public FileInfoNameComparer(StringComparer nameComparer) => this.nameComparer = nameComparer;

        public int Compare(FileInfo? x, FileInfo? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return nameComparer.Compare(x.Name, y.Name);
        }
    }

    public static Predicate<int> DeleteEveryNth(int n, int offset = 0)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        return i => (i - offset) > 0 && ((i - offset) % n == 0);
    }

    public static Predicate<int> DeleteByTakeSkip(int take, int skip)
    {
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));
        if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
        int group = take + skip;
        return i => (i % group) >= take; 
    }
}
