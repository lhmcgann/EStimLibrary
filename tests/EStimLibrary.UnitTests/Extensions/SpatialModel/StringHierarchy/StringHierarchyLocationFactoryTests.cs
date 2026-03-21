using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Extensions.SpatialModel.StringHierarchy;


namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy;


/// <summary>
/// Test class for StringHierarchyLocationFactory.
/// </summary>
public class StringHierarchyLocationFactoryTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Test class constructor creates an output helper so can write console
    /// output.
    /// </summary>
    /// <param name="testOutputHelper">The test console wrapper.</param>
    public StringHierarchyLocationFactoryTests(
        ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    // Test method naming convention: LibClassMethodName_ScenarioShouldExpectn


    #region Constructor Tests

    /// <summary>
    /// Test the constructor with null parameter.
    /// </summary>
    [Fact]
    public void Constructor_Null_ShouldError()
    {
        
        Assert.Throws<NullReferenceException>(() =>
        {
            // Create factory with null base region.
            StringHierarchyLocationFactory factory = new(null!);
        });
    }

    /// <summary>
    /// Test the constructor with non-null parameter.
    /// </summary>
    [Fact]
    public void Constructor_NonNull_ShouldInit()
    {
        // Arrange: Create base region and expected help msg.
        StringHierarchyRegion region =
            new StringHierarchyRegion("base", null!);
        string help = "A StringHierarchyLocation can be built from one of the " +
            "following path specs, selecting one option from any list in [] " +
            "and excluding the []:\n" + region.ToString();

        // Act: Create factory with non-null base region.
        StringHierarchyLocationFactory factory =
            new StringHierarchyLocationFactory(region);

        // Assert: Check that HelpMsg and ParamLimits are initialized correctly.
        Assert.Equal(help, factory.HelpMsg);
        Assert.Single(factory.ParamLimits);
        Assert.NotNull(factory.ParamLimits["fullSpec"]);
        Assert.Equal(help, factory.ParamLimits["fullSpec"].Description);
        Assert.Equal(typeof(string), factory.ParamLimits["fullSpec"].ValidDataType);
        Assert.Equal("Dynamic Data Limits", factory.ParamLimits["fullSpec"].Name);
    }

    #endregion Constructor Tests


    #region TryCreate Tests

    /// <summary>
    /// Test the TryCreate method with skipping value validation but 
    /// paramValues missing "fullSpec" value.
    /// </summary>
    [Fact]
    public void TryCreate_ShouldNotValidateShouldNotCreate()
    {
        // Create base region.
        StringHierarchyRegion region = new("base", null!);

        // Create factory with non-null base region.
        StringHierarchyLocationFactory factory = new(region);

        // Create empty dictionary of parameter values.
        Dictionary<string, object> paramValues = new();

        // Check TryCreate return and product.
        bool valid = factory.TryCreate(paramValues, out ILocation? product, 
            skipValueValidation: true);
        
        Assert.False(valid);
        Assert.Null(product);
    }

    
    #region TryCreate Helpers

    /// <summary>
    /// Helper method to create a StringHierarchyRegion with two child regions
    /// and non-empty options, modifiers, and subregions parameters.
    /// This is used to test the TryCreate method.
    /// </summary>
    /// <returns></returns>
    private static StringHierarchyRegion CreateTwoChildParentRegion()
    {
        // Create non-empty options, modifiers, and subregions parameters.
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!, 
                modifiers: regionModifiers) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region with the above parameters.
        var region = new StringHierarchyRegion("base", null!, 
            options: regionOptions,
            subregions: regionSubregions);

        return region;
    }

    /// <summary>
    /// Helper data struct for TryCreate tests with invalid paramValues.
    /// </summary>
    /// <returns>The expected input to the [MemberData()] flag: a List of 
    /// object arrays, each containing the expected test method parameters.
    /// Each object array represents another test case.</returns>
    public static IEnumerable<object[]> TryCreate_InvalidParamValues()
    {
        return new List<object[]>
        {
            // No spec item (empty dictionary).
            new object[]
            {
                new Dictionary<string, object>()
            },
            // Invalid spec item (null).
            new object[]
            {
                new Dictionary<string, object>() { { "fullSpec", null! } }
            },
            // Invalid spec item (non-string).
            new object[]
            {
                new Dictionary<string, object>() { 
                    { "fullSpec", new object() } }
            },
            // Invalid spec item (empty string).
            new object[]
            {
                new Dictionary<string, object>() { { "fullSpec", "" } }
            },
            // Invalid spec item (invalid subregion).
            new object[]
            {
                new Dictionary<string, object>() { { "fullSpec", "child3" } }
            },
            // Invalid spec item (invalid modifiers).
            new object[]
            {
                new Dictionary<string, object>() { { "fullSpec",
                    "left base, child1 | mod1, mod2" } }
            },
            // Invalid spec item (incorrect modifier syntax).
            new object[]
            {
                new Dictionary<string, object>() { { "fullSpec",
                    "left base, child1 | mod1 | mod3" } }
            }
        };
    }
    
    /// <summary>
    /// Helper data struct for TryCreate tests with valid paramValues.
    /// </summary>
    /// <returns>The expected input to the [MemberData()] flag: a List of 
    /// object arrays, each containing the expected test method parameters.
    /// Each object array represents another test case.</returns>
    public static IEnumerable<object[]> TryCreate_ValidParamValues()
    {
        return new List<object[]>
        {
            // Subregion spec with no modifiers.
            new object[]
            {
                new Dictionary<string, object>() { { "fullSpec", "left base, child1" } }
            },
            // Subregion spec with modifiers.
            new object[]
            {
                new Dictionary<string, object>() { { "fullSpec",
                    "left base, child1 | mod1" } }
            },
            // Subregion spec with multiple modifiers.
            new object[]
            {
                new Dictionary<string, object>() { { "fullSpec",
                    "left base, child1 | mod1, mod3" } }
            }//,
            // TODO: change spec parsing to allow no spaces around the region 
            // and modifier delimiters???
            // // Subregion spec with multiple modifiers and no spaces.
            // new object[]
            // {
            //     new Dictionary<string, object>() { { "fullSpec",
            //         "left base, child1|mod1,mod2" } }
            // }
        };
    }

    #endregion TryCreate Helpers


    /// <summary>
    /// Test the TryCreate method with skipping value validation and 
    /// paramValues including a "fullSpec" item with valid and invalid values.
    /// </summary>
    [Fact]
    public void TryCreate_ShouldNotValidateShouldCreate()
    {
        // Create non-empty options, modifiers, and subregions parameters.
        StringHierarchyRegion region = CreateTwoChildParentRegion();

        // Create factory with non-null base region
        StringHierarchyLocationFactory factory = new(region);


        // Test specs to TryCreate with.
        // Valid spec in params dictionary.
        var paramValues1 = new Dictionary<string, object>() {
            { "fullSpec", "left base, child1" } };

        // Invalid spec in params dictionary.
        var paramValues2 = new Dictionary<string, object>() { 
            { "fullSpec", "" } };


        // Check TryCreate return and product for valid spec
        bool valid = factory.TryCreate(paramValues1, out ILocation? product,
            skipValueValidation: true);
        Assert.True(valid);
        Assert.NotNull(product);
        Assert.Equal(new StringHierarchyLocation("left base, child1"),
            product);

        // Check TryCreate return and product for invalid spec
        valid = factory.TryCreate(paramValues2, out product,
            skipValueValidation: true);
        Assert.True(valid);
        Assert.NotNull(product);
        Assert.Equal(new StringHierarchyLocation(""), product);
    }

    /// <summary>
    /// Test the TryCreate method with value validation and invalid 
    /// paramValues.
    /// </summary>
    [Theory]
    [MemberData(nameof(TryCreate_InvalidParamValues))]
    public void TryCreate_ShouldValidateShouldNotCreate(
        Dictionary<string, object> paramValues)
    {
        // Create non-empty options, modifiers, and subregions parameters.
        StringHierarchyRegion region = CreateTwoChildParentRegion();

        // Create factory with non-null base region
        StringHierarchyLocationFactory factory = new(region);

        // Check TryCreate return and product for invalid param values dict.
        bool valid = factory.TryCreate(paramValues, out ILocation? product,
            skipValueValidation: false);
        Assert.False(valid);
        Assert.Null(product);
    }

    /// <summary>
    /// Test the TryCreate method with value validation and valid 
    /// paramValues.
    /// </summary>
    [Theory]
    [MemberData(nameof(TryCreate_ValidParamValues))]
    public void TryCreate_ShouldValidateShouldCreate(
        Dictionary<string, object> paramValues)
    {
        // Extract valid spec from paramValues.
        string spec = (string)paramValues["fullSpec"]!;

        // Create non-empty options, modifiers, and subregions parameters.
        StringHierarchyRegion region = CreateTwoChildParentRegion();

        // Create factory with non-null base region
        StringHierarchyLocationFactory factory = new(region);


        // Test valid spec without modifiers
        bool valid = factory.TryCreate(paramValues, out ILocation? product,
            skipValueValidation: false);
        Assert.True(valid);
        Assert.NotNull(product);
        Assert.Equal((ILocation)new StringHierarchyLocation(spec), product);
    }

    #endregion TryCreate Tests
}
