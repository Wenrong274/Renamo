using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SmartFileSelector.Tests;

public class FileRenamerTests
{
    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "FileRenamerTests_" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string[] CreateFiles(string dir, params string[] names)
    {
        foreach (var name in names)
            File.WriteAllText(Path.Combine(dir, name), "x");
        return names.Select(n => Path.Combine(dir, n)).ToArray();
    }

    [Fact]
    public void RenameFilesWithPattern_00_RenamesWithTwoDigits_AndKeepsExtensions()
    {
        var dir = CreateTempDir();
        try
        {
            // 排序包含不同大小寫，驗證 ByNameIgnoreCase 的一致命名順序
            CreateFiles(dir, "B.png", "a.jpg", "c.GIF");

            FileRenamer.RenameFilesWithPattern(dir, "RenamFiles_{00}");

            var names = new DirectoryInfo(dir).GetFiles().Select(f => f.Name).OrderBy(n => n).ToArray();
            Assert.Contains("RenamFiles_01.jpg", names);
            Assert.Contains("RenamFiles_02.png", names);
            Assert.Contains("RenamFiles_03.GIF", names);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void RenameFilesWithPattern_000_RenamesWithThreeDigits()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "d.txt", "c.txt", "b.txt", "a.txt");

            FileRenamer.RenameFilesWithPattern(dir, "Out_{000}");

            var names = new DirectoryInfo(dir).GetFiles().Select(f => f.Name).OrderBy(n => n).ToArray();
            Assert.Contains("Out_001.txt", names);
            Assert.Contains("Out_002.txt", names);
            Assert.Contains("Out_003.txt", names);
            Assert.Contains("Out_004.txt", names);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void RenameFilesWithPattern_RespectsSearchPattern()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "a.jpg", "b.png", "c.jpg");

            // 只改 .jpg
            FileRenamer.RenameFilesWithPattern(dir, "Pic_{00}", "*.jpg");

            var names = new DirectoryInfo(dir).GetFiles().Select(f => f.Name).OrderBy(n => n).ToArray();
            // 兩個 jpg 被改名，png 保持原名
            Assert.Contains("Pic_01.jpg", names);
            Assert.Contains("Pic_02.jpg", names);
            Assert.Contains("b.png", names);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void RenameFilesWithPattern_InvalidPattern_Throws()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "a.txt");
            Assert.Throws<ArgumentException>(() =>
                FileRenamer.RenameFilesWithPattern(dir, "BadPattern")); // 缺少 {00}/{000}
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void RenameFilesWithPattern_FolderNotFound_Throws()
    {
        var notExist = Path.Combine(Path.GetTempPath(), "NotExist_" + Guid.NewGuid());
        Assert.Throws<DirectoryNotFoundException>(() =>
            FileRenamer.RenameFilesWithPattern(notExist, "X_{00}"));
    }

    [Fact]
    public void RenameFiles_WithExplicitNameAndDigits_WorksAsExpected()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "c.dat", "a.dat", "b.dat");
            FileRenamer.RenameFiles(dir, "Data_", 3);

            var names = new DirectoryInfo(dir).GetFiles().Select(f => f.Name).OrderBy(n => n).ToArray();
            Assert.Contains("Data_001.dat", names);
            Assert.Contains("Data_002.dat", names);
            Assert.Contains("Data_003.dat", names);
        }
        finally { Directory.Delete(dir, true); }
    }
}
