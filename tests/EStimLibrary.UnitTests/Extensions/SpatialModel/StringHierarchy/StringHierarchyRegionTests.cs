using EStimLibrary.Extensions.SpatialModel.StringHierarchy;


namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy;


/// <summary>
/// Test class for StringHierarchyRegion.
/// </summary>
public class StringHierarchyRegionTests
{
    private readonly ITestOutputHelper _output;

    private static StringHierarchyRegion ConstructEmptyRoot()
    {
        return new StringHierarchyRegion("rootEmpty");
    }

    private static StringHierarchyRegion ConstructTwoLevelRoot()
    {
        StringHierarchyRegion rootTwoLevel;
        StringHierarchyRegion middleTwoLevelA;
        StringHierarchyRegion middleTwoLevelB;
        StringHierarchyRegion leafTwoLevelAa;
        StringHierarchyRegion leafTwoLevelAb;

        //          root: arm
        //          /        \
        //      midA: hand    midB: random
        //     /           \
        // leafAa: finger  leafAb: handbody
        // Leaf regions
        leafTwoLevelAa = new StringHierarchyRegion("finger",
            parentOptions: new HashSet<string> { "left", "right" },
            options: new HashSet<string> { "thumb", "index", "middle", "ring", "pinky" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "x", new HashSet<string> { "lateral", "medial" } },
                { "y", new HashSet<string> { "proximal", "middle", "distal" } },
                { "z", new HashSet<string> { "palmar", "dorsal" } } }
            );
        leafTwoLevelAb = new StringHierarchyRegion("handbody",
            parentOptions: new(),
            options: new HashSet<string> { "left", "right" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "x", new HashSet<string> { "lateral", "medial" } },
                { "y", new HashSet<string> { "proximal", "middle", "distal" } },
                { "z", new HashSet<string> { "palmar", "dorsal" } } }
            );
        // Middle regions
        middleTwoLevelA = new StringHierarchyRegion("hand",
            parentOptions: new HashSet<string> { "upper", "lower" },
            options: new HashSet<string> { "left", "right" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "x", new HashSet<string> { "lateral", "medial" } },
                { "y", new HashSet<string> { "proximal", "middle", "distal" } },
                { "z", new HashSet<string> { "palmar", "dorsal" } } },
            subregions: new Dictionary<string, StringHierarchyRegion> {
                { "finger", leafTwoLevelAa },
                { "handbody", leafTwoLevelAb } }
            );
        middleTwoLevelB = new StringHierarchyRegion("middleTwoLevelB",
            parentOptions: new HashSet<string> { "upper", "lower" },
            options: new HashSet<string> { "ulnar", "radial" });
        // Root region
        rootTwoLevel = new StringHierarchyRegion("rootTwoLevel", 
            options: new(),
            subregions: new Dictionary<string, StringHierarchyRegion> {
                { "hand", middleTwoLevelA }, 
                { "middleTwoLevelB", middleTwoLevelB } }
            );
        
        // Set parent references for all regions.
        leafTwoLevelAa.ParentRegion = middleTwoLevelA;
        leafTwoLevelAb.ParentRegion = middleTwoLevelA;
        middleTwoLevelA.ParentRegion = rootTwoLevel;
        middleTwoLevelB.ParentRegion = rootTwoLevel;

        return rootTwoLevel;
    }

    /// <summary>
    /// Test class constructor creates an output helper so it can write console
    /// output.
    /// </summary>
    /// <param name="testOutputHelper"></param>
    public StringHierarchyRegionTests(ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    #region Constructor
    /// <summary>
    /// Test the constructor with null parameter values: 
    ///     no parent region,
    ///     optional params passed null.
    /// </summary>
    [Fact]
    public void Constructor_ShouldAcceptNull()
    {
        var stringHierarchyRegion = new StringHierarchyRegion("base", null!,
            null!, null!, null!, null!);

        // Check that all fields correctly instantiate
        Assert.Equal("base", stringHierarchyRegion.BaseName);
        Assert.Null(stringHierarchyRegion.ParentRegion);
        Assert.Empty(stringHierarchyRegion.ParentOptions);
        Assert.Empty(stringHierarchyRegion.Options);
        Assert.False(stringHierarchyRegion.HasOptions);
        Assert.Single(stringHierarchyRegion.OptionedRegionNames);
        Assert.Equal("base", stringHierarchyRegion.OptionedRegionNames[0]);
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
        // Create region with all default parameters unspecified
        var stringHierarchyRegion = new StringHierarchyRegion("base", null!);

        // Check that all fields correctly instantiate
        Assert.Equal("base", stringHierarchyRegion.BaseName);
        Assert.Null(stringHierarchyRegion.ParentRegion);
        Assert.Empty(stringHierarchyRegion.ParentOptions);
        Assert.Empty(stringHierarchyRegion.Options);
        Assert.False(stringHierarchyRegion.HasOptions);
        Assert.Single(stringHierarchyRegion.OptionedRegionNames);
        Assert.Equal("base", stringHierarchyRegion.OptionedRegionNames[0]);
        Assert.Empty(stringHierarchyRegion.Modifiers);
        Assert.False(stringHierarchyRegion.HasModifiers);
        Assert.Empty(stringHierarchyRegion.Subregions);
        Assert.False(stringHierarchyRegion.HasSubregions);
        Assert.True(stringHierarchyRegion.IsLeaf);
        Assert.Empty(stringHierarchyRegion.SavedLocations);
        Assert.Empty(stringHierarchyRegion.SavedAreas);
    }

    /// <summary>
    /// Test the constructor with non-null, empty parameter values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldAcceptEmpty()
    {
        // Create simple parent region
        var parent = new StringHierarchyRegion("root", null!);

        // Create region with all parameters non-null
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, new HashSet<string>(),
            new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());

        // Check that all fields correctly instantiate
        Assert.Equal("base", stringHierarchyRegion.BaseName);
        Assert.StrictEqual(parent, stringHierarchyRegion.ParentRegion);
        Assert.Empty(stringHierarchyRegion.ParentOptions);
        Assert.Empty(stringHierarchyRegion.Options);
        Assert.False(stringHierarchyRegion.HasOptions);
        Assert.Single(stringHierarchyRegion.OptionedRegionNames);
        Assert.Equal("base", stringHierarchyRegion.OptionedRegionNames[0]);
        Assert.Empty(stringHierarchyRegion.Modifiers);
        Assert.False(stringHierarchyRegion.HasModifiers);
        Assert.Empty(stringHierarchyRegion.Subregions);
        Assert.False(stringHierarchyRegion.HasSubregions);
        Assert.True(stringHierarchyRegion.IsLeaf);
        Assert.Empty(stringHierarchyRegion.SavedLocations);
        Assert.Empty(stringHierarchyRegion.SavedAreas);
    }

    /// <summary>
    /// Test the constructor with non-null, non-empty parameter values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldDeepCopy()
    {
        // Create parent region with non-null object parameters for deep copy
        // testing
        var parent = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "right", new HashSet<string>() { "mod1", "mod2", "mod2" } },
            { "left", new HashSet<string>() { "mod2", "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region with the above parameters for deep copy testing
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, regionOptions, regionModifiers, regionSubregions);

        // Check that all non-region object fields are deep copies of
        // parameters and all region fields are shallow copies (i.e.,
        // check for value equality and reference equality)

        Assert.Equal("base", stringHierarchyRegion.BaseName);
        // Check parent shallow copy
        Assert.Same(parent, stringHierarchyRegion.ParentRegion);
        // Check parent options deep copy
        Assert.Equal(parent.Options, stringHierarchyRegion.ParentOptions);
        Assert.NotSame(parent.Options, stringHierarchyRegion.ParentOptions);
        // Check options deep copy
        Assert.Equal(regionOptions, stringHierarchyRegion.Options);
        Assert.NotSame(regionOptions, stringHierarchyRegion.Options);
        Assert.True(stringHierarchyRegion.HasOptions);
        // Check optioned region names (since not tested previously)
        Assert.Equal(2, stringHierarchyRegion.OptionedRegionNames.Count);
        Assert.Equal(new List<string>() {
            $"right{StringHierarchySpec.OPTION_REGION_DELIMITER}base",
            $"left{StringHierarchySpec.OPTION_REGION_DELIMITER}base"},
            stringHierarchyRegion.OptionedRegionNames);
        // Check modifiers deep copy
        Assert.Equal(regionModifiers, stringHierarchyRegion.Modifiers);
        Assert.NotSame(regionModifiers, stringHierarchyRegion.Modifiers);
        Assert.True(stringHierarchyRegion.HasModifiers);
        // Check subregions deep copy dictionary but shallow copy subregions
        Assert.Equal(regionSubregions, stringHierarchyRegion.Subregions);
        Assert.NotSame(regionSubregions, stringHierarchyRegion.Subregions);
        Assert.Same(regionSubregions["child1"],
            stringHierarchyRegion.Subregions["child1"]);
        Assert.Same(regionSubregions["child2"],
            stringHierarchyRegion.Subregions["child2"]);
        // Check miscellaneous subregion-related fields (since not tested
        // previously)
        Assert.True(stringHierarchyRegion.HasSubregions);
        Assert.False(stringHierarchyRegion.IsLeaf);
        Assert.Empty(stringHierarchyRegion.SavedLocations);
        Assert.Empty(stringHierarchyRegion.SavedAreas);

        // Create region with above region as parent for further parent testing
        var child = new StringHierarchyRegion("child", stringHierarchyRegion,
            stringHierarchyRegion.Options, new HashSet<string>(),
            new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());

        // Check for parent and parent option equality with non-null parent options
        Assert.Same(stringHierarchyRegion, child.ParentRegion);
        Assert.Equal(stringHierarchyRegion.Options, child.ParentOptions);
        Assert.NotSame(stringHierarchyRegion.Options, child.ParentOptions);
    }
    #endregion Constructor

    /// <summary>
    /// Test the ToString method.
    /// </summary>
    [Fact]
    public void ToString_ShouldOutputStringRep()
    {
        // Create region with null parameter values
        var nullRegion = new StringHierarchyRegion("base", null!);
        // Create region with non-null but empty options, modifiers, and
        // subregions
        var emptyRegion = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        // to cover all components of string representation
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var subregionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "grandchild", new StringHierarchyRegion("grandchild", null!) }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!, null!,
                new HashSet<string> { "opt1", "opt2" }, null!,
                subregionSubregions) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region with the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base",
            emptyRegion, emptyRegion.Options, regionOptions, regionModifiers,
            regionSubregions);

        // Check string outputs for correctness
        Assert.Equal("base", nullRegion.ToString());
        Assert.Equal("root", emptyRegion.ToString());
        Assert.Equal("[right,left] base | [mod1,mod2], [mod3,mod4]\n" +
            "    [right,left] base, [opt1,opt2] child1\n" +
            "        [right,left] base, [opt1,opt2] child1, grandchild\n" +
            "    [right,left] base, child2", stringHierarchyRegion.ToString());
    }

    #region TryGetSubregion
    /// <summary>
    /// Test the TryGetSubregion method with various malformed or otherwise
    /// incorrect regions.
    /// </summary>
    [Fact]
    public void TryGetSubregion_ShouldOutputNull()
    {
        // Create simple parent region
        var parent = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region with the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, regionOptions, regionModifiers, regionSubregions);

        // TODO: make comment on PR abt using InlineData[] tags in future for cleaner code
        // Test with empty string representation
        StringHierarchyRegion? foundSubregion;
        var output = stringHierarchyRegion.TryGetSubregion("",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test with empty base region
        output = stringHierarchyRegion.TryGetSubregion(", left base",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test with invalid base region
        output = stringHierarchyRegion.TryGetSubregion("base2",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test with valid base region but invalid option
        output = stringHierarchyRegion.TryGetSubregion("middle base",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test with valid base region but no options specified
        output = stringHierarchyRegion.TryGetSubregion("base",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test valid base region and option but specified modifier
        output = stringHierarchyRegion.TryGetSubregion("left base | modSet1",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test with valid subregion but no base region
        output = stringHierarchyRegion.TryGetSubregion("child1",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test with invalid subregion
        output = stringHierarchyRegion.TryGetSubregion("left base, ",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test with multiple options
        output = stringHierarchyRegion.TryGetSubregion("right left base",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
        // Test with additional whitespace between option and region
        output = stringHierarchyRegion.TryGetSubregion("left   base",
            out foundSubregion);
        Assert.False(output);
        Assert.Null(foundSubregion);
    }

    /// <summary>
    /// Test the TryGetSubregion method with region present in current level
    /// and region present in subregion.
    /// </summary>
    [Fact]
    public void TryGetSubregion_ShouldOutputRegion()
    {
        // Create simple parent region
        var parent = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var subregionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "grandchild", new StringHierarchyRegion("grandchild", null!) }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!, null!,
                new HashSet<string> { "opt1", "opt2" }, null!,
                subregionSubregions) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region with the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, regionOptions, regionModifiers, regionSubregions);

        // Test with valid option and base region
        StringHierarchyRegion? foundSubregion;
        var output = stringHierarchyRegion.TryGetSubregion("right base",
            out foundSubregion);
        Assert.True(output);
        Assert.Equal(stringHierarchyRegion, foundSubregion);
        // Test with valid subregion
        output = stringHierarchyRegion.TryGetSubregion("left base, child2",
            out foundSubregion);
        Assert.True(output);
        Assert.Equal(regionSubregions["child2"], foundSubregion);
        // Test with valid subregion option
        output = stringHierarchyRegion.TryGetSubregion("left base, opt2 child1",
            out foundSubregion);
        Assert.True(output);
        Assert.Equal(regionSubregions["child1"], foundSubregion);
        // Test with valid subregion of subregion
        output = stringHierarchyRegion.TryGetSubregion("left base, opt2 child1, " +
            "grandchild", out foundSubregion);
        Assert.True(output);
        Assert.Equal(subregionSubregions["grandchild"], foundSubregion);
        // Test with additional whitespace between regions
        output = stringHierarchyRegion.TryGetSubregion("left base,  child1",
            out foundSubregion);
        Assert.True(output);
        Assert.Equal(regionSubregions["child1"], foundSubregion);
    }
    #endregion TryGetSubregion

    #region AddSubregion
    /// <summary>
    /// Test the AddSubregion method with new subregions.
    /// </summary>
    [Fact]
    public void AddSubregion_ShouldAddSubregion()
    {
        // Create simple parent region
        var parent = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region with the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, regionOptions, regionModifiers, regionSubregions);

        // Create variable to store old region output
        StringHierarchyRegion? oldRegion;

        // Create new region with different name to existing subregions
        var newChildRegion = new StringHierarchyRegion("child3", null!);

        // Check shallow copy of subregion into base region and old subregion
        // output
        stringHierarchyRegion.AddSubregion(newChildRegion, out oldRegion);
        Assert.Null(oldRegion);
        Assert.Same(newChildRegion,
            stringHierarchyRegion.Subregions["child3"]);
    }

    /// <summary>
    /// Test the AddSubregion method with duplicate subregions.
    /// </summary>
    [Fact]
    public void AddSubregion_ShouldReplaceSubregion()
    {
        // Create simple parent region
        var parent = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region with the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, regionOptions, regionModifiers, regionSubregions);

        // Create variable to store old region output
        StringHierarchyRegion? oldRegion;

        // Test re-adding existing subregion
        stringHierarchyRegion.AddSubregion(regionSubregions["child2"],
            out oldRegion);
        Assert.Equal(regionSubregions["child2"], oldRegion);
        Assert.Same(regionSubregions["child2"],
            stringHierarchyRegion.Subregions["child2"]);

        // Create new region with same name as existing subregion
        var newChildRegion = new StringHierarchyRegion("child1", null!);

        // Test adding new subregion with same name as existing subregion
        stringHierarchyRegion.AddSubregion(newChildRegion, out oldRegion);
        Assert.Equal(regionSubregions["child1"], oldRegion);
        Assert.Same(newChildRegion,
            stringHierarchyRegion.Subregions["child1"]);
        Assert.Same(stringHierarchyRegion, newChildRegion.ParentRegion);
    }
    #endregion AddSubregion

    #region DeepCopy
    /// <summary>
    /// Test the DeepCopy method with various root region structures.
    /// </summary>
    /// <param name="root"></param>
    [Theory]
    [MemberData(nameof(DeepCopy_TestData))]
    public void DeepCopy_ShouldSucceed(StringHierarchyRegion root)
    {
        // Create the deep copy
        var deepCopy = root.DeepCopy();

        // Recursive check.
        DeepCopy_ShouldSucceed_Helper(root, deepCopy);
    }

    /// <summary>
    /// Recursive helper method for DeepCopy_ShouldSucceed.
    /// </summary>
    /// <param name="original"></param>
    /// <param name="deepCopy"></param>
    private void DeepCopy_ShouldSucceed_Helper(StringHierarchyRegion original, 
        StringHierarchyRegion deepCopy)
    {
        // Check root object is different address.
        Assert.NotSame(original, deepCopy);

        // Check basename is same value.
        Assert.Equal(original.BaseName, deepCopy.BaseName);

        // Check parent region is different address.
        if (original.ParentRegion is null)
        {
            Assert.Null(deepCopy.ParentRegion);
        }
        else
        {
            Assert.NotSame(original.ParentRegion, deepCopy.ParentRegion);
        }

        // Check parent options is different address.
        Assert.NotSame(original.ParentOptions, deepCopy.ParentOptions);

        // Check parent options is same value.
        Assert.Equal(original.ParentOptions, deepCopy.ParentOptions);

        // Check options is different address.
        Assert.NotSame(original.Options, deepCopy.Options);

        // Check options is same value.
        Assert.Equal(original.Options, deepCopy.Options);

        // Check modifiers is different address.
        Assert.NotSame(original.Modifiers, deepCopy.Modifiers);

        // Check modifiers is same value.
        Assert.Equal(original.Modifiers, deepCopy.Modifiers);

        // Check saved locations and areas are different address, same values.
        Assert.NotSame(original.SavedLocations, deepCopy.SavedLocations);
        Assert.Equal(original.SavedLocations, deepCopy.SavedLocations);
        Assert.NotSame(original.SavedAreas, deepCopy.SavedAreas);
        Assert.Equal(original.SavedAreas, deepCopy.SavedAreas);

        // Check subregions collection is different address.
        Assert.NotSame(original.Subregions, deepCopy.Subregions);

        // Check subregion collections have same length.
        Assert.Equal(original.Subregions.Count, deepCopy.Subregions.Count);

        // Check subregions.
        foreach (var (subregionName, originalSubregion) in original.Subregions)
        {
            // Check copy has a subregion of the same name.
            Assert.True(deepCopy.Subregions.TryGetValue(subregionName, 
                // Get copied subregion inherently.
                out var copiedSubregion));

            // Check value (address) of subregion is different.
            Assert.NotSame(originalSubregion, copiedSubregion);

            // Recurse into subregion.
            DeepCopy_ShouldSucceed_Helper(originalSubregion, copiedSubregion);
        }
    }

    /// <summary>
    /// Test data for DeepCopy method.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> DeepCopy_TestData()
    {
        return new List<object[]>
        {
            new object[] { ConstructEmptyRoot() },
            new object[] { ConstructTwoLevelRoot() }
        };
    }

    /// <summary>
    /// Test the DeepCopy method with retaining the parent reference.
    /// </summary>
    [Fact]
    public void DeepCopy_ShouldDeepCopy_Fact()
    {
        // Create region with null parameter values
        var nullRegion = new StringHierarchyRegion("base", null!);
        // Create region with non-null but empty options, modifiers, and subregions
        var emptyRegion = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options and modifiers parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region with the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base",
            emptyRegion, emptyRegion.Options, regionOptions, regionModifiers,
            new Dictionary<string, StringHierarchyRegion>());

        // Create variable to store old region output
        StringHierarchyRegion? oldRegion;
        // Add subregions after creating region to ensure correct parent region fields
        stringHierarchyRegion.AddSubregion(new StringHierarchyRegion("child1", null!), out oldRegion);
        stringHierarchyRegion.AddSubregion(new StringHierarchyRegion("child2", null!), out oldRegion);

        // Check deep copy for region with null parameter values (i.e., value
        // equality but not reference equality)
        var nullRegionCopy = nullRegion.DeepCopy(true);
        Assert.Equivalent(nullRegion, nullRegionCopy, strict: true);
        Assert.NotEqual(nullRegion, nullRegionCopy);
        Assert.NotSame(nullRegion, nullRegionCopy);

        // Check deep copy for region with empty parameter values (i.e., value
        // equality but not reference equality)
        var emptyRegionCopy = emptyRegion.DeepCopy(true);
        Assert.Equivalent(emptyRegion, emptyRegionCopy, strict: true);
        Assert.NotEqual(emptyRegion, emptyRegionCopy);
        Assert.NotSame(emptyRegion, emptyRegionCopy);

        // Check deep copy for region with non-empty parameter values (i.e., value
        // equality but not reference equality for all non-region fields)
        var stringHierarchyRegionCopy = stringHierarchyRegion.DeepCopy(true);
        Assert.Equal(stringHierarchyRegion.ToString(),
            stringHierarchyRegionCopy.ToString());
        Assert.Same(stringHierarchyRegion.ParentRegion,
            stringHierarchyRegionCopy.ParentRegion);
        Assert.Equal(stringHierarchyRegion.Options,
            stringHierarchyRegionCopy.Options);
        Assert.NotSame(stringHierarchyRegion.Options,
            stringHierarchyRegionCopy.Options);
        Assert.Equal(stringHierarchyRegion.Modifiers,
            stringHierarchyRegionCopy.Modifiers);
        Assert.NotSame(stringHierarchyRegion.Modifiers,
            stringHierarchyRegionCopy.Modifiers);
        // Testing Note: Due to the presence of parentRegion in each of the
        // subregions and due to the deep copying of the subregions themselves,
        // it is not feasible to check equality or equivalence of the
        // subregions
        Assert.NotEqual(stringHierarchyRegion.Subregions,
            stringHierarchyRegionCopy.Subregions);
        Assert.NotSame(stringHierarchyRegion.Subregions,
            stringHierarchyRegionCopy.Subregions);
    }

    /// <summary>
    /// Test the DeepCopy method without retaining the parent reference.
    /// </summary>
    [Fact]
    public void DeepCopy_ShouldDeepCopyResetParent()
    {
        // Create region with null parameter values
        var nullRegion = new StringHierarchyRegion("base", null!);
        // Create region with non-null but empty options, modifiers, and subregions
        var parent = new StringHierarchyRegion("root", nullRegion, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        // Create region with the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, regionOptions, regionModifiers,
            new Dictionary<string, StringHierarchyRegion>());

        // Create variable to store old region output
        StringHierarchyRegion? oldRegion;
        // Add subregions after creating region to ensure correct parent region fields
        stringHierarchyRegion.AddSubregion(new StringHierarchyRegion("child1", null!), out oldRegion);
        stringHierarchyRegion.AddSubregion(new StringHierarchyRegion("child2", null!), out oldRegion);

        // Check deep copy for region with empty parameter values (i.e., value
        // equality but not reference equality) before and after setting parent
        // region of original to null (should be not equal before, equal after)
        var parentCopy = parent.DeepCopy(false);
        Assert.NotEqual(parent.ParentRegion, parentCopy.ParentRegion);
        parent.ParentRegion = null!;
        Assert.Equal(parent.ParentRegion, parentCopy.ParentRegion);
        Assert.Equivalent(parent, parentCopy, strict: true);
        Assert.NotEqual(parent, parentCopy);
        Assert.NotSame(parent, parentCopy);

        // Check deep copy for region with non-empty parameter values (i.e.,
        // value equality but not reference equality) before and after setting
        // parent region of original to null
        var stringHierarchyRegionCopy = stringHierarchyRegion.DeepCopy(false);
        Assert.NotEqual(stringHierarchyRegion.ParentRegion,
            stringHierarchyRegionCopy.ParentRegion);
        stringHierarchyRegion.ParentRegion = null!;
        Assert.Equal(stringHierarchyRegion.ParentRegion,
            stringHierarchyRegionCopy.ParentRegion);
        Assert.Equal(stringHierarchyRegion.ToString(),
            stringHierarchyRegionCopy.ToString());
        Assert.Same(stringHierarchyRegion.ParentRegion,
            stringHierarchyRegionCopy.ParentRegion);
        Assert.Equal(stringHierarchyRegion.Options,
            stringHierarchyRegionCopy.Options);
        Assert.NotSame(stringHierarchyRegion.Options,
            stringHierarchyRegionCopy.Options);
        Assert.Equal(stringHierarchyRegion.Modifiers,
            stringHierarchyRegionCopy.Modifiers);
        Assert.NotSame(stringHierarchyRegion.Modifiers,
            stringHierarchyRegionCopy.Modifiers);
        // Testing Note: Due to the presence of parentRegion in each of the
        // subregions and due to the deep copying of the subregions themselves,
        // it is not feasible to check equality or equivalence of the
        // subregions
        Assert.NotEqual(stringHierarchyRegion.Subregions,
            stringHierarchyRegionCopy.Subregions);
        Assert.NotSame(stringHierarchyRegion.Subregions,
            stringHierarchyRegionCopy.Subregions);
    }
    #endregion DeepCopy

    #region IsValidModifierSpec
    /// <summary>
    /// Test the IsValidModifierSpec method with invalid modifier specs.
    /// </summary>
    [Fact]
    public void IsValidModifierSpec_ShouldOutputFalse()
    {
        // Create simple parent region
        var parent = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region using the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, regionOptions, regionModifiers, regionSubregions);

        // Create invalid test modifier specs
        var modSpec1 = "";
        var modSpec2 = "mod5";
        var modSpec3 = "mod1, mod2";
        var modSpec4 = "mod1, mod3, mod4";

        // Test empty modifier spec
        Assert.False(stringHierarchyRegion.IsValidModifierSpec(modSpec1));
        // Test invalid modifier
        Assert.False(stringHierarchyRegion.IsValidModifierSpec(modSpec2));
        // Test invalid modifier set (all in same set)
        Assert.False(stringHierarchyRegion.IsValidModifierSpec(modSpec3));
        // Test invalid modifier set (pair in same set)
        Assert.False(stringHierarchyRegion.IsValidModifierSpec(modSpec4));
    }

    /// <summary>
    /// Test the IsValidModifierSpec method with valid modifier specs.
    /// </summary>
    [Fact]
    public void IsValidModifierSpec_ShouldOutputTrue()
    {
        // Create simple parent region
        var parent = new StringHierarchyRegion("root", null!, null!,
            new HashSet<string>(), new Dictionary<string, HashSet<string>>(),
            new Dictionary<string, StringHierarchyRegion>());
        // Create non-empty options, modifiers, and subregions parameters
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null!) },
            { "child2", new StringHierarchyRegion("child2", null!) }
        };
        // Create region using the above parameters
        var stringHierarchyRegion = new StringHierarchyRegion("base", parent,
            parent.Options, regionOptions, regionModifiers, regionSubregions);

        // Create invalid test modifier specs
        var modSpec1 = "mod3";
        var modSpec2 = "mod1, mod4";

        // Test valid modifier
        Assert.True(stringHierarchyRegion.IsValidModifierSpec(modSpec1));
        // Test valid modifier set
        Assert.True(stringHierarchyRegion.IsValidModifierSpec(modSpec2));
    }

    /// <summary>
    /// Test the IsValidModifierSpec method handles cases with same-valued
    /// modifier axes.
    /// </summary>
    [Theory]
    #region Single shared modifier. Same-length axis modifier value sets.
    // Shared modifier value used in first axis.
    [InlineData(
        new string[] { "left", "middle", "right" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "middle, proximal")]
    // Shared modifier value used in second axis.
    [InlineData(
        new string[] { "left", "middle", "right" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "left, middle")]
    // Shared modifier value used in both axes.
    [InlineData(
        new string[] { "left", "middle", "right" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "middle, middle")]
    // Shared modifier value used in one axis, no modifier value used in other.
    // TODO: Should this non-determinism be allowed!!!
    [InlineData(
        new string[] { "left", "middle", "right" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "middle")]
    // Shared modifier not used.
    [InlineData(
        new string[] { "left", "middle", "right" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "distal")]
    #endregion
    #region Single shared modifier. Different-length axis modifier value sets.
    // Shared modifier value used in first (shorter) axis. Listed first.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "middle, proximal")]
    // Shared modifier value used in first (shorter) axis. Listed second.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "proximal, middle")]
    // Shared modifier value used in second (longer) axis. Listed first.
    // TODO: this test case fails!!! --> REQUIRES FIX in Region!!! larger refactor/reimplement
    // [InlineData(
    //     new string[] { "left", "middle" }, 
    //     new string[] { "proximal", "middle", "distal" }, 
    //     "middle, left")]
    // Shared modifier value used in second (longer) axis. Listed second.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "left, middle")]
    // Shared modifier value used in both axes.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "middle, middle")]
    // Shared modifier value used in one axis, no modifier value used in other.
    // TODO: Should this non-determinism be allowed!!!
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "middle")]
    // Shared modifier not used.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "distal")]
    #endregion
    #region Two shared modifiers. Different-length axis modifier value sets.
    // TODO: Which axis are shared modifiers bucketed into? Should this 
    // pseudo-non-determinism (rly just unknown-to-the-user behavior) be 
    // allowed???!!!
    // Both values used: order 1.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "left", "middle", "right" }, 
        "middle, left")]
    // Both values used: order 2.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "left", "middle", "right" },
        "left, middle")]
    // One value use: axis 1, listed first.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "left", "middle", "right" }, 
        "middle, right")]
    // One value used: axis 1, listed second.
    [InlineData(
        new string[] { "left", "middle" }, 
        new string[] { "left", "middle", "right" }, 
        "right, middle")]
    #endregion
    #region Single shared modifier. One axis only has the shared modifier.
    // Shared modifier value used in first (shorter) axis. Listed first.
    [InlineData(
        new string[] { "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "middle, proximal")]
    // Shared modifier value used in first (shorter) axis. Listed second.
    [InlineData(
        new string[] { "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "proximal, middle")]
    // Shared modifier value used in one axis, no modifier value used in other.
    // TODO: Should this non-determinism be allowed!!!
    [InlineData(
        new string[] { "middle" }, 
        new string[] { "proximal", "middle", "distal" }, 
        "middle")]
    #endregion
    public void IsValidModifierSpec_DuplicateModifierValues_ShouldSucceed(
        string[] axis1Values, string[] axis2Values,
        string modSpec)
    {
        // Create a region with the same value on two modifier axes.
        var region = new StringHierarchyRegion("hand", null!, 
            modifiers: new Dictionary<string, HashSet<string>>
            {
                { "x", new HashSet<string>(axis1Values) },
                { "y", new HashSet<string>(axis2Values) }
            });

        // Test valid modifier set
        Assert.True(region.IsValidModifierSpec(modSpec));
    }
    #endregion IsValidModifierSpec
}
