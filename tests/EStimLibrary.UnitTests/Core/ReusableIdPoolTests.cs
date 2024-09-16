using EStimLibrary.Core;


namespace EStimLibrary.UnitTests.Core;


// Test class naming convention: LibClassTests
public class ReusableIdPoolTests
{
    private readonly ITestOutputHelper _output;

    // Test class constructor creates an output helper so can write console
    // output.
    public ReusableIdPoolTests(ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    // Test method naming convention: LibClassMethodName_ScenarioShouldExpectn

    /// <summary>
    /// Test the empty constructor.
    /// </summary>
    [Fact]
    public void EmptyConstuctor_ShouldInitEmpty()
    {
        var reusableIdPool = new ReusableIdPool();

        Assert.Equal(0, reusableIdPool.BaseId);
        Assert.Equal(0, reusableIdPool.NumIds);
        Assert.Empty(reusableIdPool.Ids);
        Assert.Equal(0, reusableIdPool.NumUsedIds);
        Assert.Empty(reusableIdPool.UsedIds);
        Assert.Equal(0, reusableIdPool.NumFreeIds);
        Assert.Empty(reusableIdPool.FreeIds);
    }


    /// <summary>
    /// Test the parameterized constructor with different data values.
    /// </summary>
    [Theory]
    // Test expected valid (non-negative) parameter values.
    [InlineData(0, 0, new int[0])]
    [InlineData(0, 1, new int[] { 0 })]
    [InlineData(0, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
    [InlineData(1, 0, new int[0])]
    [InlineData(1, 1, new int[] { 1 })]
    [InlineData(2, 9, new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
    // Test negative parameter values default to 0 before generating ID set.
    [InlineData(-1, 0, new int[0])]
    [InlineData(-3, 3, new int[] { 0, 1, 2 })]
    [InlineData(0, -1, new int[0])]
    [InlineData(3, -3, new int[0])]
    [InlineData(-5, -4, new int[0])]
    public void Constuctor_ShouldInitIdSets(int baseId, int numIds,
        int[] expectedIds)
    {
        // Init new ReusableIdPool
        var reusableIdPool = new ReusableIdPool(baseId, numIds);

        // Set expected values.
        int expectedBaseId = Math.Max(baseId, 0);
        int expectedNumIds = Math.Max(numIds, 0);
        SortedSet<int> expectedIdSet = new(expectedIds);
        this._output.WriteLine($"\nBase Id: {expectedBaseId}\nNum Ids: " +
            $"{expectedNumIds}");

        // Test ReusableIdPool properties are initialized correctly.
        Assert.Equal(expectedBaseId, reusableIdPool.BaseId);
        Assert.Equal(expectedNumIds, reusableIdPool.NumIds);
        Assert.Equal<int>(expectedIdSet, reusableIdPool.Ids);
        Assert.Equal(0, reusableIdPool.NumUsedIds);
        Assert.Empty(reusableIdPool.UsedIds);
        Assert.Equal(expectedNumIds, reusableIdPool.NumFreeIds);
        Assert.Equal<int>(expectedIdSet, reusableIdPool.FreeIds);
    }
}