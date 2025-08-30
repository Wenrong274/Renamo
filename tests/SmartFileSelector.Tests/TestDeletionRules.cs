namespace SmartFileSelector.Tests;

public class TestFileSelectorRules
{
    #region EveryNth 測試
    [Theory]
    [InlineData(1, 0, new[] { 1, 3, 5, 7, 9 })]     // 每2個一組，刪除索引 1,3,5,7,9
    [InlineData(2, 0, new[] { 2, 5, 8 })]           // 每3個一組，刪除索引 2,5,8
    [InlineData(3, 0, new[] { 3, 7 })]              // 每4個一組，刪除索引 3,7
    [InlineData(4, 0, new[] { 4, 9 })]              // 每5個一組，刪除索引 4,9
    [InlineData(9, 0, new[] { 9 })]                 // 每10個一組，刪除索引 9
    [InlineData(10, 0, new int[0])]                 // 每11個一組，10個檔案時不刪除
    [InlineData(100, 0, new int[0])]                // 每101個一組，10個檔案時不刪除
    public void EveryNth_VariousIntervals_ReturnsExpectedIndices(
        int interval, int startOffset, int[] expectedDeletedIndices)
    {
        var rule = FileSelectorRules.EveryNth(interval, startOffset);
        var actualDeleted = Enumerable.Range(0, 10)
            .Where(x => rule(x))
            .ToArray();

        Assert.Equal(expectedDeletedIndices, actualDeleted);
    }

    [Theory]
    [InlineData(2, 1, new[] { 3, 6, 9 })]           // 從索引1開始，每3個一組，刪除索引 3,6,9
    [InlineData(2, 2, new[] { 4, 7 })]              // 從索引2開始，每3個一組，刪除索引 4,7
    [InlineData(3, 1, new[] { 4, 8 })]              // 從索引1開始，每4個一組，刪除索引 4,8
    public void EveryNth_WithStartOffset_ReturnsExpectedIndices(
        int interval, int startOffset, int[] expectedDeletedIndices)
    {
        var rule = FileSelectorRules.EveryNth(interval, startOffset);
        var actualDeleted = Enumerable.Range(0, 10)
            .Where(x => rule(x))
            .ToArray();

        Assert.Equal(expectedDeletedIndices, actualDeleted);
    }

    [Fact]
    public void EveryNth_SingleFile_WithIntervalGreaterThan0_DeletesNothing()
    {
        var rule = FileSelectorRules.EveryNth(1);
        Assert.False(rule(0)); // 單一檔案，間隔1，不應刪除索引0
    }

    [Fact]
    public void EveryNth_TwoFiles_WithInterval1_DeletesSecondFile()
    {
        var rule = FileSelectorRules.EveryNth(1);
        Assert.False(rule(0)); // 保留第1個
        Assert.True(rule(1));  // 刪除第2個
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void EveryNth_InvalidInterval_ThrowsArgumentOutOfRangeException(int interval)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => FileSelectorRules.EveryNth(interval));
        Assert.Equal("interval", ex.ParamName);
        Assert.Contains("間隔必須大於 0", ex.Message);
    }

    [Fact]
    public void EveryNth_NegativeStartOffset_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => FileSelectorRules.EveryNth(1, -1));
        Assert.Equal("startOffset", ex.ParamName);
        Assert.Contains("偏移量不能為負數", ex.Message);
    }

    [Fact]
    public void EveryNth_StartOffsetGreaterThanAvailableIndices_DeletesNothing()
    {
        var rule = FileSelectorRules.EveryNth(1, startOffset: 10);
        var deleted = Enumerable.Range(0, 5).Where(x => rule(x)).ToArray();
        Assert.Empty(deleted);
    }

    #endregion

    #region KeepThenSelete 測試

    [Theory]
    [InlineData(1, 1, 10, new[] { 1, 3, 5, 7, 9 })]      // 保留1刪除1：刪除索引 1,3,5,7,9
    [InlineData(2, 1, 10, new[] { 2, 5, 8 })]             // 保留2刪除1：刪除索引 2,5,8
    [InlineData(3, 1, 10, new[] { 3, 7 })]                // 保留3刪除1：刪除索引 3,7
    [InlineData(2, 2, 10, new[] { 2, 3, 6, 7 })]          // 保留2刪除2：刪除索引 2,3,6,7
    [InlineData(1, 2, 10, new[] { 1, 2, 4, 5, 7, 8 })]   // 保留1刪除2：刪除索引 1,2,4,5,7,8
    [InlineData(3, 2, 8, new[] { 3, 4 })]                 // 保留3刪除2：刪除索引 3,4
    public void KeepThenSelete_VariousParameters_ReturnsExpectedIndices(
        int keepCount, int deleteCount, int totalFiles, int[] expectedDeletedIndices)
    {
        var rule = FileSelectorRules.KeepThenSelete(keepCount, deleteCount);
        var actualDeleted = Enumerable.Range(0, totalFiles)
            .Where(x => rule(x))
            .ToArray();

        Assert.Equal(expectedDeletedIndices, actualDeleted);
    }

    [Fact]
    public void KeepThenSelete_ExactlyOneGroup_DeletesExpectedFiles()
    {
        var rule = FileSelectorRules.KeepThenSelete(2, 1); // 保留2刪除1，組大小=3

        // 測試恰好一組的情況
        var deleted = Enumerable.Range(0, 3).Where(x => rule(x)).ToArray();
        Assert.Equal(new[] { 2 }, deleted);
    }

    [Fact]
    public void KeepThenSelete_IncompleteLastGroup_OnlyDeletesCompleteGroups()
    {
        var rule = FileSelectorRules.KeepThenSelete(3, 2); // 保留3刪除2，組大小=5

        // 測試 7 個檔案：第一組完整 [0,1,2,3,4]，第二組不完整 [5,6]
        var deleted = Enumerable.Range(0, 7).Where(x => rule(x)).ToArray();
        Assert.Equal(new[] { 3, 4 }, deleted); // 只有第一組的刪除部分
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    public void KeepThenSelete_InvalidKeepCount_ThrowsArgumentOutOfRangeException(
        int keepCount, int deleteCount)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            FileSelectorRules.KeepThenSelete(keepCount, deleteCount));
        Assert.Equal("keepCount", ex.ParamName);
        Assert.Contains("保留數量必須大於 0", ex.Message);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void KeepThenSelete_InvalidDeleteCount_ThrowsArgumentOutOfRangeException(
        int keepCount, int deleteCount)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            FileSelectorRules.KeepThenSelete(keepCount, deleteCount));
        Assert.Equal("seleteCount", ex.ParamName);
        Assert.Contains("選擇數量必須大於 0", ex.Message);
    }

    #endregion

    #region 邊界條件測試

    [Fact]
    public void EveryNth_LargeInterval_WithSmallDataSet_DeletesNothing()
    {
        var rule = FileSelectorRules.EveryNth(1000);
        var deleted = Enumerable.Range(0, 100).Where(rule).ToArray();
        Assert.Empty(deleted);
    }

    [Fact]
    public void KeepThenSelete_LargeKeepCount_WithSmallDataSet_DeletesNothing()
    {
        var rule = FileSelectorRules.KeepThenSelete(1000, 1);
        var deleted = Enumerable.Range(0, 100).Where(rule).ToArray();
        Assert.Empty(deleted);
    }

    [Fact]
    public void EveryNth_Interval1_DeletesEverySecondFile()
    {
        var rule = FileSelectorRules.EveryNth(1);
        var deleted = Enumerable.Range(0, 10).Where(rule).ToArray();
        Assert.Equal(new[] { 1, 3, 5, 7, 9 }, deleted);
    }

    [Fact]
    public void KeepThenSelete_Keep1Delete1_AlternatesCorrectly()
    {
        var rule = FileSelectorRules.KeepThenSelete(1, 1);
        var deleted = Enumerable.Range(0, 10).Where(rule).ToArray();
        Assert.Equal(new[] { 1, 3, 5, 7, 9 }, deleted);
    }

    #endregion

    #region 功能等價性測試

    [Fact]
    public void EveryNth_And_KeepThenSelete_ProduceSameResults_WhenEquivalent()
    {
        // EveryNth(1) 和 KeepThenSelete(1,1) 應該產生相同結果
        var everyNthRule = FileSelectorRules.EveryNth(1);
        var KeepThenSeleteRule = FileSelectorRules.KeepThenSelete(1, 1);

        var indices = Enumerable.Range(0, 20).ToArray();
        var everyNthResult = indices.Where(everyNthRule).ToArray();
        var KeepThenSeleteResult = indices.Where(KeepThenSeleteRule).ToArray();

        Assert.Equal(everyNthResult, KeepThenSeleteResult);
    }

    [Fact]
    public void EveryNth_And_KeepThenSelete_ProduceSameResults_WhenEquivalent_Complex()
    {
        // EveryNth(2) 和 KeepThenSelete(2,1) 應該產生相同結果
        var everyNthRule = FileSelectorRules.EveryNth(2);
        var KeepThenSeleteRule = FileSelectorRules.KeepThenSelete(2, 1);

        var indices = Enumerable.Range(0, 20).ToArray();
        var everyNthResult = indices.Where(everyNthRule).ToArray();
        var KeepThenSeleteResult = indices.Where(KeepThenSeleteRule).ToArray();

        Assert.Equal(everyNthResult, KeepThenSeleteResult);
    }

    #endregion

    #region 實際使用場景測試

    [Fact]
    public void EveryNth_LogFileCleanup_Scenario()
    {
        // 模擬日誌檔案清理：保留每3個中的2個，刪除1個
        var rule = FileSelectorRules.EveryNth(2); // 每3個一組，刪除最後一個

        // 模擬 30 天的日誌檔案
        var deleted = Enumerable.Range(0, 30).Where(rule).ToArray();

        // 應該刪除 2, 5, 8, 11, 14, 17, 20, 23, 26, 29
        var expected = Enumerable.Range(0, 10).Select(i => 2 + i * 3).ToArray();
        Assert.Equal(expected, deleted);

        // 驗證刪除比例約為 1/3
        Assert.Equal(10, deleted.Length);
        Assert.True(deleted.Length / 30.0 > 0.3 && deleted.Length / 30.0 < 0.4);
    }

    [Fact]
    public void KeepThenSelete_BackupRotation_Scenario()
    {
        // 模擬備份輪換：每7個備份保留5個，刪除2個
        var rule = FileSelectorRules.KeepThenSelete(5, 2);

        // 模擬 21 個備份檔案（恰好3組）
        var deleted = Enumerable.Range(0, 21).Where(rule).ToArray();

        // 應該刪除每組的第6,7個：索引 5,6,12,13,19,20
        var expected = new[] { 5, 6, 12, 13, 19, 20 };
        Assert.Equal(expected, deleted);
    }

    [Fact]
    public void EveryNth_PhotoThinning_Scenario()
    {
        // 模擬照片精簡：每10張保留9張，刪除1張
        var rule = FileSelectorRules.EveryNth(9); // 每10個一組，刪除第10個

        // 模擬 50 張照片
        var deleted = Enumerable.Range(0, 50).Where(rule).ToArray();

        // 應該刪除索引 9, 19, 29, 39, 49
        var expected = new[] { 9, 19, 29, 39, 49 };
        Assert.Equal(expected, deleted);

        // 驗證刪除比例為 10%
        Assert.Equal(5, deleted.Length);
        Assert.Equal(0.1, deleted.Length / 50.0, 2);
    }

    #endregion

    #region 邊界條件與特殊情況測試

    [Fact]
    public void EveryNth_EmptySequence_DeletesNothing()
    {
        var rule = FileSelectorRules.EveryNth(1);
        var deleted = Enumerable.Empty<int>().Where(rule).ToArray();
        Assert.Empty(deleted);
    }

    [Fact]
    public void KeepThenSelete_EmptySequence_DeletesNothing()
    {
        var rule = FileSelectorRules.KeepThenSelete(1, 1);
        var deleted = Enumerable.Empty<int>().Where(rule).ToArray();
        Assert.Empty(deleted);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    public void EveryNth_SingleElement_DeletesNothing(int interval)
    {
        var rule = FileSelectorRules.EveryNth(interval);
        Assert.False(rule(0)); // 單一元素時不應該被刪除
    }

    [Theory]
    [InlineData(1, 1, 0, false)] // 第一組的第一個元素
    [InlineData(2, 1, 0, false)] // 第一組的第一個元素
    [InlineData(2, 1, 1, false)] // 第一組的第二個元素
    [InlineData(2, 1, 2, true)]  // 第一組的第三個元素（要刪除）
    public void KeepThenSelete_FirstGroup_BehavesCorrectly(
        int keepCount, int deleteCount, int index, bool expectedDelete)
    {
        var rule = FileSelectorRules.KeepThenSelete(keepCount, deleteCount);
        Assert.Equal(expectedDelete, rule(index));
    }

    [Fact]
    public void EveryNth_VeryLargeNumbers_HandlesCorrectly()
    {
        var rule = FileSelectorRules.EveryNth(1000000);

        // 測試大數值不會造成溢位
        Assert.False(rule(999999));  // 應該保留
        Assert.True(rule(1000000));  // 應該刪除
        Assert.False(rule(1000001)); // 應該保留
    }

    [Fact]
    public void KeepThenSelete_VeryLargeNumbers_HandlesCorrectly()
    {
        var rule = FileSelectorRules.KeepThenSelete(1000000, 1);

        // 測試大數值
        Assert.False(rule(999999));  // 第一組內，應該保留
        Assert.True(rule(1000000));  // 第一組的刪除位置
        Assert.False(rule(1000001)); // 第二組開始，應該保留
    }

    #endregion

    #region 組合條件測試

    [Fact]
    public void CombinedRules_EveryNthWithCustomLogic_WorksCorrectly()
    {
        var everyNthRule = FileSelectorRules.EveryNth(3); // 每4個一組，刪除第4個

        // 組合條件：既要符合 EveryNth，又要是偶數索引
        Func<int, bool> combinedRule = index => everyNthRule(index) && index % 2 == 0;

        var deleted = Enumerable.Range(0, 20).Where(combinedRule).ToArray();

        // EveryNth(3) 會選擇 3,7,11,15,19
        // 與偶數條件交集後應該是空（因為都是奇數）
        Assert.Empty(deleted);
    }

    [Fact]
    public void CombinedRules_MultipleEveryNth_WorksCorrectly()
    {
        var rule1 = FileSelectorRules.EveryNth(2); // 刪除索引 2,5,8,11,14,17...
        var rule2 = FileSelectorRules.EveryNth(4); // 刪除索引 4,9,14,19...

        // 聯集：符合任一條件就刪除
        Func<int, bool> unionRule = index => rule1(index) || rule2(index);

        var deleted = Enumerable.Range(0, 20).Where(unionRule).ToArray();

        // 預期：2,4,5,8,9,11,14,17,19
        var expected = new[] { 2, 4, 5, 8, 9, 11, 14, 17, 19 };
        Assert.Equal(expected, deleted);
    }

    #endregion

    #region 性能特性測試

    [Fact]
    public void EveryNth_RepeatedCalls_ProducesConsistentResults()
    {
        var rule = FileSelectorRules.EveryNth(7);

        // 多次呼叫應該產生相同結果
        var result1 = Enumerable.Range(0, 100).Where(rule).ToArray();
        var result2 = Enumerable.Range(0, 100).Where(rule).ToArray();
        var result3 = Enumerable.Range(0, 100).Where(rule).ToArray();

        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public void KeepThenSelete_RepeatedCalls_ProducesConsistentResults()
    {
        var rule = FileSelectorRules.KeepThenSelete(3, 2);

        // 多次呼叫應該產生相同結果
        var result1 = Enumerable.Range(0, 100).Where(rule).ToArray();
        var result2 = Enumerable.Range(0, 100).Where(rule).ToArray();
        var result3 = Enumerable.Range(0, 100).Where(rule).ToArray();

        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    #endregion

    #region 數學正確性驗證

    [Theory]
    [InlineData(1, 2, 0.5)]    // 每2個刪1個 = 50%
    [InlineData(2, 3, 1.0 / 3)]  // 每3個刪1個 ≈ 33.3%
    [InlineData(3, 4, 0.25)]   // 每4個刪1個 = 25%
    [InlineData(9, 10, 0.1)]   // 每10個刪1個 = 10%
    public void EveryNth_DeletionRatio_IsCorrect(int interval, int groupSize, double expectedRatio)
    {
        var rule = FileSelectorRules.EveryNth(interval);
        var totalFiles = groupSize * 10; // 測試10組
        var deleted = Enumerable.Range(0, totalFiles).Where(rule).ToArray();

        var actualRatio = (double)deleted.Length / totalFiles;
        Assert.Equal(expectedRatio, actualRatio, 3); // 精確到3位小數
    }

    [Theory]
    [InlineData(1, 1, 0.5)]    // 保留1刪1個 = 50%
    [InlineData(2, 1, 1.0 / 3)]  // 保留2刪1個 ≈ 33.3%
    [InlineData(3, 1, 0.25)]   // 保留3刪1個 = 25%
    [InlineData(1, 2, 2.0 / 3)]  // 保留1刪2個 ≈ 66.7%
    public void KeepThenSelete_DeletionRatio_IsCorrect(
        int keepCount, int deleteCount, double expectedRatio)
    {
        var rule = FileSelectorRules.KeepThenSelete(keepCount, deleteCount);
        var groupSize = keepCount + deleteCount;
        var totalFiles = groupSize * 10; // 測試10組
        var deleted = Enumerable.Range(0, totalFiles).Where(rule).ToArray();

        var actualRatio = (double)deleted.Length / totalFiles;
        Assert.Equal(expectedRatio, actualRatio, 3); // 精確到3位小數
    }

    #endregion

    #region 實際測試案例重現

    [Fact]
    public void EveryNth_OriginalFailingTest_ShouldPass()
    {
        // 重現原始測試：DeleteEvery2_DeletesIndex2
        var rule = FileSelectorRules.EveryNth(2);
        var deleted = Enumerable.Range(0, 6).Where(rule).ToArray();

        Assert.Equal(new[] { 2, 5 }, deleted);
    }

    [Fact]
    public void EveryNth_OriginalPassingTest_ShouldPass()
    {
        // 重現原始測試：DeleteEvery100_WhenFilesAre3_DeletesNothing
        var rule = FileSelectorRules.EveryNth(100);
        var deleted = Enumerable.Range(0, 3).Where(rule).ToArray();

        Assert.Empty(deleted);
    }

    [Fact]
    public void KeepThenSelete_OriginalTest_Take2Skip1_DeletesOnlyIndex2()
    {
        var rule = FileSelectorRules.KeepThenSelete(2, 1);
        var deleted = Enumerable.Range(0, 5).Where(rule).ToArray();

        Assert.Equal(new[] { 2 }, deleted);
    }

    [Fact]
    public void KeepThenSelete_OriginalTest_Take1Skip1_Deletes_IndexesOneTwoAndThree()
    {
        var rule = FileSelectorRules.KeepThenSelete(1, 1);
        var deleted = Enumerable.Range(0, 4).Where(rule).ToArray();

        Assert.Equal(new[] { 1, 3 }, deleted);
    }

    #endregion
}