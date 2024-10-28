using EStimLibrary.Extensions.SpatialModel.StringHierarchy;


namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy;


// Test class naming convention: LibClassTests
public class StringHierarchyRegionTests
{
    private readonly ITestOutputHelper _output;

    // Test class constructor creates an output helper so can write console output.
    public StringHierarchyRegionTests(ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    // Test method naming convention: LibClassMethodName_ScenarioShouldExpect

    /// <summary>
    /// Test the constructor with null parameter values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldAcceptNull()
    {
        var stringHierarchyRegion = new StringHierarchyRegion("hand", null, null, null, null, null);

        Assert.Equal("hand", stringHierarchyRegion.BaseName);
        Assert.Null(stringHierarchyRegion.ParentRegion);
        Assert.Empty(stringHierarchyRegion.ParentOptions);
        Assert.Empty(stringHierarchyRegion.Options);
        Assert.False(stringHierarchyRegion.HasOptions);
        Assert.Single(stringHierarchyRegion.OptionedRegionNames);
        Assert.Equal("hand", stringHierarchyRegion.OptionedRegionNames[0]);
        Assert.Empty(stringHierarchyRegion.Modifiers);
        Assert.False(stringHierarchyRegion.HasModifiers);
        Assert.Empty(stringHierarchyRegion.Subregions);
        Assert.False(stringHierarchyRegion.HasSubregions);
        Assert.True(stringHierarchyRegion.IsLeaf);
        Assert.Empty(stringHierarchyRegion.SavedLocations);
        Assert.Empty(stringHierarchyRegion.SavedAreas);
    }

    /// <summary>
    /// Test the constructor with unspecified parameter values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldDefaultNull()
    {
        var stringHierarchyRegion = new StringHierarchyRegion("hand", null);

        Assert.Equal("hand", stringHierarchyRegion.BaseName);
        Assert.Null(stringHierarchyRegion.ParentRegion);
        Assert.Empty(stringHierarchyRegion.ParentOptions);
        Assert.Empty(stringHierarchyRegion.Options);
        Assert.False(stringHierarchyRegion.HasOptions);
        Assert.Single(stringHierarchyRegion.OptionedRegionNames);
        Assert.Equal("hand", stringHierarchyRegion.OptionedRegionNames[0]);
        Assert.Empty(stringHierarchyRegion.Modifiers);
        Assert.False(stringHierarchyRegion.HasModifiers);
        Assert.Empty(stringHierarchyRegion.Subregions);
        Assert.False(stringHierarchyRegion.HasSubregions);
        Assert.True(stringHierarchyRegion.IsLeaf);
        Assert.Empty(stringHierarchyRegion.SavedLocations);
        Assert.Empty(stringHierarchyRegion.SavedAreas);
    }
}
