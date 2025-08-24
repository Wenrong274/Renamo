using BatchFileDeleter;
namespace TestRenamo;

public class FileDeleterTests
{
    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "FileDeleterTests_" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string[] CreateFiles(string dir, params string[] names)
    {
        foreach (var name in names)
            File.WriteAllText(Path.Combine(dir, name), "test");
        return names.Select(n => Path.Combine(dir, n)).ToArray();
    }

    [Fact]
    public void SelectFilesForDeletion_NullFolderPath_ThrowsArgumentNullException()
    {
        string folderPath = null!;
        Predicate<int> del = _ => true;

        Assert.Throws<ArgumentNullException>(() =>
            FileDeleter.SelectFilesForDeletion(folderPath, del).ToList());
    }

    [Fact]
    public void SelectFilesForDeletion_EmptyFolderPath_ThrowsArgumentNullException()
    {
        string folderPath = "";
        Predicate<int> del = _ => true;

        Assert.Throws<ArgumentNullException>(() =>
            FileDeleter.SelectFilesForDeletion(folderPath, del).ToList());
    }

    [Fact]
    public void SelectFilesForDeletion_NullPredicate_ThrowsArgumentNullException()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "a.txt");
            Assert.Throws<ArgumentNullException>(() =>
                FileDeleter.SelectFilesForDeletion(dir, null!).ToList());
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void SelectFilesForDeletion_DirectoryNotExist_ThrowsDirectoryNotFoundException()
    {
        var notExist = Path.Combine(Path.GetTempPath(), "FileDeleter_NotExist_" + Guid.NewGuid());
        Predicate<int> del = _ => true;

        Assert.Throws<DirectoryNotFoundException>(() =>
            FileDeleter.SelectFilesForDeletion(notExist, del).ToList());
    }

    [Fact]
    public void SelectFilesForDeletion_EmptyDirectory_WithAnyPredicate_ReturnsEmpty()
    {
        var dir = CreateTempDir();
        try
        {
            var deleted = FileDeleter.SelectFilesForDeletion(dir, _ => true).ToList();
            Assert.Empty(deleted);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void SelectFilesForDeletion_DeterministicOrder_WhenDeletingAll()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "D.txt", "c.txt", "B.txt", "a.txt");

            var deleted = FileDeleter.SelectFilesForDeletion(dir, _ => true);
            Assert.Equal(new[] { "a.txt", "B.txt", "c.txt", "D.txt" }, deleted.Select(f => f.Name).ToArray());
        }
        finally { Directory.Delete(dir, true); }
    }


    [Fact]
    public void SelectFilesForDeletion_DeleteEvery2_DeletesIndex2()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "a.txt", "b.txt", "c.txt", "d.txt", "e.txt", "f.txt");
            var del = FileDeleter.DeleteEveryNth(2);

            var deleted = FileDeleter.SelectFilesForDeletion(dir, del);
            Assert.Equal(new[] { "c.txt", "e.txt" }, deleted.Select(f => f.Name).ToArray());
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void SelectFilesForDeletion_DeleteEvery100_WhenFilesAre3_DeletesOnlyIndex0()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "a.txt", "b.txt", "c.txt");
            var del = FileDeleter.DeleteEveryNth(100);

            var deleted = FileDeleter.SelectFilesForDeletion(dir, del);
            Assert.Equal(Array.Empty<string>(), deleted.Select(f => f.Name).ToArray());
        }
        finally { Directory.Delete(dir, true); }
    }


    [Fact]
    public void SelectFilesForDeletion_Take2Skip1_DeletesOnlyIndex2()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "a.txt", "b.txt", "c.txt", "d.txt", "e.txt");
            var del = FileDeleter.DeleteByTakeSkip(2, 1);

            var deleted = FileDeleter.SelectFilesForDeletion(dir, del);
            Assert.Equal(new[] { "c.txt" }, deleted.Select(f => f.Name).ToArray());
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void SelectFilesForDeletion_Take1Skip1_Deletes_b_and_d()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "a.txt", "b.txt", "c.txt", "d.txt");
            var del = FileDeleter.DeleteByTakeSkip(1, 1);

            var deleted = FileDeleter.SelectFilesForDeletion(dir, del);
            Assert.Equal(new[] { "b.txt", "d.txt" }, deleted.Select(f => f.Name).ToArray());
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void SelectFilesForDeletion_PredicateAlwaysFalse_DeletesNone()
    {
        var dir = CreateTempDir();
        try
        {
            CreateFiles(dir, "a.txt", "b.txt");
            Predicate<int> delNone = _ => false;

            var deleted = FileDeleter.SelectFilesForDeletion(dir, delNone).ToList();
            Assert.Empty(deleted);
        }
        finally { Directory.Delete(dir, true); }
    }
}
