using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Extensions.SpatialModel.StringHierarchy;

namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy;

/// <summary>
/// Test class for StringHierarchyAreaFactory.
/// </summary>
public class StringHierarchyAreaFactoryTests
{
    private readonly ITestOutputHelper _output;

    // Test class constructor creates an output helper so can write console
    // output.
    public StringHierarchyAreaFactoryTests(
        ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    // Test method naming convention: LibClassMethodName_ScenarioShouldExpectn

    /// <summary>
    /// Test the constructor with null parameter.
    /// </summary>
    [Fact]
    public void Constructor_ShouldAcceptNull()
    {
        // Create factory with null base region
        StringHierarchyAreaFactory factory = new StringHierarchyAreaFactory(null);

        // Check that HelpMsg and ParamLimits are initialized correctly
        Assert.Equal("A StringHierarchyArea can be built from one of the " +
            "following path specs, selecting one option from any list in [] " +
            "and excluding the []:\nnull", factory.HelpMsg);
        Assert.Single(factory.ParamLimits);
        Assert.NotNull(factory.ParamLimits["fullSpec"]);
    }

    /// <summary>
    /// Test the constructor with non-null parameter.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitBaseRegion()
    {
        // Create base region
        StringHierarchyRegion region = new StringHierarchyRegion("base", null);

        // Create factory with non-null base region
        StringHierarchyAreaFactory factory =
            new StringHierarchyAreaFactory(region);

        // Check that HelpMsg and ParamLimits are initialized correctly
        Assert.Equal("A StringHierarchyArea can be built from one of the " +
            "following path specs, selecting one option from any list in [] " +
            "and excluding the []:\n" + region.ToString(), factory.HelpMsg);
        Assert.Single(factory.ParamLimits);
        Assert.NotNull(factory.ParamLimits["fullSpec"]);
    }

    /// <summary>
    /// Test the TryCreate method with paramValues not including a fullSpec
    /// value and skipping value validation
    /// </summary>
    [Fact]
    public void TryCreate_ShouldNotValidateShouldNotCreate()
    {
        // Create base region
        StringHierarchyRegion region = new StringHierarchyRegion("base", null);

        // Create factory with non-null base region
        StringHierarchyAreaFactory factory =
            new StringHierarchyAreaFactory(region);

        // Create dictionary of parameter values
        Dictionary<string, object> paramValues =
            new Dictionary<string, object>();

        // Create variable to store produced IArea
        IArea product;

        // Check TryCreate return and product
        bool valid = factory.TryCreate(paramValues, out product, true);
        Assert.False(valid);
        Assert.Null(product);
    }

    /// <summary>
    /// Test the TryCreate method with paramValues not including a fullSpec
    /// value and not skipping value validation
    /// </summary>
    [Fact]
    public void TryCreate_ShouldValidateShouldNotCreate()
    {
        // Create base region
        StringHierarchyRegion region = new StringHierarchyRegion("base", null);

        // Create factory with non-null base region
        StringHierarchyAreaFactory factory =
            new StringHierarchyAreaFactory(region);

        // Create dictionary of parameter values
        Dictionary<string, object> paramValues =
            new Dictionary<string, object>();

        // Create variable to store produced IArea
        IArea product;

        // Check TryCreate return and product
        bool valid = factory.TryCreate(paramValues, out product, false);
        Assert.False(valid);
        Assert.Null(product);
    }

    /// <summary>
    /// Test the TryCreate method with paramValues including a fullSpec value
    /// and skipping value validation
    /// </summary>
    [Fact]
    public void TryCreate_ShouldNotValidateShouldCreate()
    {
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null, null, null,
                regionModifiers) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };
        // Create region with the above parameters
        var region = new StringHierarchyRegion("base", null,
            null, regionOptions, null, regionSubregions);

        // Create factory with non-null base region
        StringHierarchyAreaFactory factory =
            new StringHierarchyAreaFactory(region);

        // Create dictionary with valid spec
        Dictionary<string, object> paramValues1 =
            new Dictionary<string, object>(){
                { "fullSpec", "left base, child1" } };

        // Create dictionary with invalid spec
        Dictionary<string, object> paramValues2 =
            new Dictionary<string, object>() { { "fullSpec", "" } };

        // Create variable to store produced IArea
        IArea product;

        // Check TryCreate return and product for valid spec
        bool valid = factory.TryCreate(paramValues1, out product, true);
        Assert.True(valid);
        Assert.NotNull(product);
        Assert.Equal((IArea) new StringHierarchyArea("left base, child1"),
            product);

        // Check TryCreate return and product for invalid spec
        valid = factory.TryCreate(paramValues2, out product, true);
        Assert.True(valid);
        Assert.NotNull(product);
        Assert.Equal((IArea) new StringHierarchyArea(""), product);
    }

    /// <summary>
    /// Test the TryCreate method with paramValues including a fullSpec value
    /// and not skipping value validation
    /// </summary>
    [Fact]
    public void TryCreate_ShouldValidateShouldMaybeCreate()
    {
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null, null, null,
                regionModifiers) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };
        // Create region with the above parameters
        var region = new StringHierarchyRegion("base", null,
            null, regionOptions, null, regionSubregions);

        // Create factory with non-null base region
        StringHierarchyAreaFactory factory =
            new StringHierarchyAreaFactory(region);

        // Create dictionaries with valid specs
        Dictionary<string, object> paramValues1 =
            new Dictionary<string, object>() {
                { "fullSpec", "left base, child1" } };
        Dictionary<string, object> paramValues2 =
            new Dictionary<string, object>() {
                { "fullSpec", "left base, child1 | mod1" } };

        // Create dictionaries with valid specs
        Dictionary<string, object> paramValues3 =
            new Dictionary<string, object>() { { "fullSpec", null } };
        Dictionary<string, object> paramValues4 =
            new Dictionary<string, object>() { { "fullSpec", new object() } };
        Dictionary<string, object> paramValues5 =
            new Dictionary<string, object>() { { "fullSpec", "" } };
        Dictionary<string, object> paramValues6 =
            new Dictionary<string, object>() { { "fullSpec", "child3" } };
        Dictionary<string, object> paramValues7 =
            new Dictionary<string, object>() { { "fullSpec",
                    "left base, child1 | mod1, mod2" } };
        Dictionary<string, object> paramValues8 =
            new Dictionary<string, object>() { { "fullSpec",
                    "left base, child1 | mod1 | mod3" } };

        // Create variable to store produced IArea
        IArea product;

        // Test valid spec without modifiers
        bool valid = factory.TryCreate(paramValues1, out product, false);
        Assert.True(valid);
        Assert.NotNull(product);
        Assert.Equal((IArea) new StringHierarchyArea("left base, child1"),
            product);

        // Test valid spec with modifiers
        valid = factory.TryCreate(paramValues2, out product, false);
        Assert.True(valid);
        Assert.NotNull(product);
        Assert.Equal((IArea)new StringHierarchyArea("left base, child1 | mod1"),
            product);

        // Test null value
        valid = factory.TryCreate(paramValues3, out product, false);
        Assert.False(valid);
        Assert.Null(product);

        // Test non-string value
        valid = factory.TryCreate(paramValues4, out product, false);
        Assert.False(valid);
        Assert.Null(product);

        // Test empty string
        valid = factory.TryCreate(paramValues5, out product, false);
        Assert.False(valid);
        Assert.Null(product);

        // Test invalid subregion
        valid = factory.TryCreate(paramValues6, out product, false);
        Assert.False(valid);
        Assert.Null(product);

        // Test invalid modifiers
        valid = factory.TryCreate(paramValues7, out product, false);
        Assert.False(valid);
        Assert.Null(product);

        // Test incorrect modifier syntax
        valid = factory.TryCreate(paramValues8, out product, false);
        Assert.False(valid);
        Assert.Null(product);
    }
}
