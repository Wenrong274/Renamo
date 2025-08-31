using SmartFileSelector.Core;

namespace SmartFileSelector.Tests;

public class RenamePatternParserTests
{
    [Theory]
    [InlineData("MyFile_{00}", "MyFile_", 2)]
    [InlineData("Report_{000}", "Report_", 3)]
    [InlineData("Data Set_{00}", "Data Set_", 2)]
    [InlineData("X_{000}", "X_", 3)]
    [InlineData("A_X_{000}", "A_X_", 3)]
    public void Parse_ValidPatterns_ReturnsExpectedNameAndDigits(
        string input, string expectedName, int expectedDigits)
    {
        // Act
        var (customName, digitCount) = RenamePatternParser.Parse(input);

        // Assert
        Assert.Equal(expectedName, customName);
        Assert.Equal(expectedDigits, digitCount);
    }

    [Fact]
    public void Parse_KeepTrailingUnderscore_InCustomName()
    {
        var (name, digits) = RenamePatternParser.Parse("RenamFiles_{000}");
        Assert.Equal("RenamFiles_", name);
        Assert.Equal(3, digits);
    }

    [Theory]
    [InlineData("MyFile_00")]     // 少了大括號
    [InlineData("MyFile_{0a}")]   // 非純 0
    [InlineData("MyFile_{ 00 }")] // 大括號內有空白
    [InlineData("MyFile_{}")]     // 沒有內容
    [InlineData("MyFile_")]       // 沒有大括號
    public void Parse_InvalidPatterns_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => RenamePatternParser.Parse(input));
    }

    [Fact]
    public void Parse_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RenamePatternParser.Parse(string.Empty));
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RenamePatternParser.Parse(null!));
    }
}

