using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.UnitTests.Core.SpatialModel;


/// <summary>
/// Test class for the LocalizationData record.
/// </summary>
public class LocalizationDataTests
{
    /// <summary>
    /// Test the basic construction of LocalizationData and 
    /// ensure immutability (encapsulation).
    /// </summary>
    [Fact]
    public void Constructor_ShouldEnsureImmutablity()
    {
        // Arrange
        var areasFullyContaining = new List<int> { 1, 2, 3 };
        var areasPartiallyContaining = new List<int> { 4, 5 };

        // Act
        var localizationData = new LocalizationData(areasFullyContaining,
            areasPartiallyContaining);

        // Assert - Ensure properties are initialized correctly
        Assert.Equal(areasFullyContaining,
            localizationData.AreasFullyContaining);
        Assert.Equal(areasPartiallyContaining,
            localizationData.AreasPartiallyContaining);

        // Act - Modify the original lists (which should not affect the record)
        areasFullyContaining.Add(6);
        areasPartiallyContaining.Add(7);

        // Assert - Check immutability: localizationData should not reflect
        // changes in the original lists
        Assert.DoesNotContain(6, localizationData.AreasFullyContaining);
        Assert.DoesNotContain(7, localizationData.AreasPartiallyContaining);
    }

    /// <summary>
    /// Test the Merge method for correctness and handling of empty sets.
    /// </summary>
    [Fact]
    public void Merge_ShouldCombineAreasAndHandleEmptySetsCorrectly()
    {
        // Arrange
        var data1 = new LocalizationData(
            new List<int> { 1, 2, 3 },
            new List<int> { 4, 5 });

        var data2 = new LocalizationData(
            new List<int> { 3, 4, 5 },
            new List<int> { 6, 7 });

        var emptyData = new LocalizationData(new List<int>(), new List<int>());

        // Act - Merge data1 and data2
        var mergedData = data1.Merge(data2);

        // Assert - Check merged areas
        var expectedFullyContaining = new List<int> { 1, 2, 3, 4, 5 };
        var expectedPartiallyContaining = new List<int> { 4, 5, 6, 7 };
        Assert.Equal(expectedFullyContaining.OrderBy(x => x),
            mergedData.AreasFullyContaining.OrderBy(x => x));
        Assert.Equal(expectedPartiallyContaining.OrderBy(x => x),
            mergedData.AreasPartiallyContaining.OrderBy(x => x));

        // Act - Merge with empty data
        var mergedWithEmpty = data1.Merge(emptyData);
        var emptyWithPopulated = emptyData.Merge(data2);

        // Assert - Check that merging with empty sets doesn't alter the
        // populated data
        Assert.Equal(new List<int> { 1, 2, 3 },
            mergedWithEmpty.AreasFullyContaining);
        Assert.Equal(new List<int> { 4, 5 },
            mergedWithEmpty.AreasPartiallyContaining);

        Assert.Equal(new List<int> { 3, 4, 5 },
            emptyWithPopulated.AreasFullyContaining);
        Assert.Equal(new List<int> { 6, 7 },
            emptyWithPopulated.AreasPartiallyContaining);
    }

    /// <summary>
    /// Test that the Merge method does not modify the original LocalizationData
    /// objects.
    /// </summary>
    /// <param name="data1FullyContaining">TODO</param>
    /// TODO
    [Theory]
    [MemberData(nameof(MergeTestData))]
    public void Merge_ShouldNotModifyOriginalObjects(
        List<int> data1FullyContaining,
        List<int> data1PartiallyContaining,
        List<int> data2FullyContaining,
        List<int> data2PartiallyContaining)
    {
        // Arrange - create LocalizationData objects
        var data1 = new LocalizationData(data1FullyContaining,
            data1PartiallyContaining);
        var data2 = new LocalizationData(data2FullyContaining,
            data2PartiallyContaining);

        // Act - merge
        var mergedData = data1.Merge(data2);

        // Assert - merge had no effect on original data
        Assert.Equal(data1FullyContaining, data1.AreasFullyContaining);
        Assert.Equal(data1PartiallyContaining, data1.AreasPartiallyContaining);
        Assert.Equal(data2FullyContaining, data2.AreasFullyContaining);
        Assert.Equal(data2PartiallyContaining, data2.AreasPartiallyContaining);
    }

    /// <summary>
    /// Test data for Merge_ShouldNotModifyOriginalObjects, of the form:
    /// - data1FullyContaining (List<int>)
    /// - data1PartiallyContaining (List<int>)
    /// - data2FullyContaining (List<int>)
    /// - data2PartiallyContaining (List<int>)
    /// </summary>
    public static IEnumerable<object[]> MergeTestData => new List<object[]>
    {
        // Test many
        new object[]
        {
            new List<int> { 1, 2, 3 },  // data1 fully containing
            new List<int> { 4, 5 },     // data1 partially containing
            new List<int> { 3, 4, 5 },  // data2 fully containing
            new List<int> { 6, 7 }      // data2 partially containing
        },

        // Test 0
        new object[]
        {
            new List<int> {  },
            new List<int> { -1, 1 },
            new List<int> {  },
            new List<int> { 1, -1 }
        },

        // Test 1
        new object[]
        {
            new List<int> { 0, 0 },
            new List<int> { Int32.MinValue },
            new List<int> { 0, -1 },
            new List<int> { Int32.MaxValue }
        }
    };
}