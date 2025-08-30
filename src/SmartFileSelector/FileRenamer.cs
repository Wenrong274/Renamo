using System;
using System.IO;
using System.Linq;
namespace SmartFileSelector;

public static class FileRenamer
{
    /// <summary>
    /// 批次重新命名檔案
    /// </summary>
    /// <param name="folderPath">資料夾路徑</param>
    /// <param name="customName">自訂檔名</param>
    /// <param name="digitCount">流水號長度 (例如 2 = 01, 3 = 001)</param>
    /// <param name="searchPattern">搜尋模式，預設 "*"</param>
    public static void RenameFiles(
        string folderPath,
        string customName,
        int digitCount = 2,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        IComparer<FileInfo>? comparer = null)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"找不到資料夾：{folderPath}");

        // 注意：排序需要將所有檔案載入記憶體，對於大型資料夾可能消耗較多記憶體
        var sortedFileInfos = new DirectoryInfo(folderPath)
            .GetFiles(searchPattern, searchOption)
            .OrderBy(file => file, comparer ?? FileInfoComparer.ByNameIgnoreCase);

        int index = 1;
        foreach (var fileInfo in sortedFileInfos)
        {
            string ext = Path.GetExtension(fileInfo.Name);
            string newName = $"{customName}{index.ToString($"D{digitCount}")}{ext}";
            string newPath = Path.Combine(folderPath, newName);

            File.Move(fileInfo.FullName, newPath);
            index++;
        }
    }

    public static void RenameFilesWithPattern(
    string folderPath,
    string pattern,
    string searchPattern = "*",
    SearchOption searchOption = SearchOption.TopDirectoryOnly,
    IComparer<FileInfo>? comparer = null)
    {
        var (customName, digitCount) = RenamePatternParser.Parse(pattern);
        RenameFiles(folderPath, customName, digitCount, searchPattern, searchOption, comparer);
    }
}
