using EStimLibrary.Extensions.SpatialModel.StringHierarchy;
using System.Runtime.InteropServices;
using Xunit.Sdk;

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
        var stringHierarchyRegion = new StringHierarchyRegion("base", null, null, null, null, null);

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
        var stringHierarchyRegion = new StringHierarchyRegion("base", null);

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
        var parent = new StringHierarchyRegion("root", null);

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());

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
        var parent = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "right", new HashSet<string>() { "mod1", "mod2" } },
            { "left", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, regionOptions, regionModifiers, regionSubregions);
        
        Assert.Equal("base", stringHierarchyRegion.BaseName);
        Assert.Same(parent, stringHierarchyRegion.ParentRegion);
        Assert.Equal(parent.Options, stringHierarchyRegion.ParentOptions);
        Assert.NotSame(parent.Options, stringHierarchyRegion.ParentOptions);
        Assert.Equal(regionOptions, stringHierarchyRegion.Options);
        Assert.NotSame(regionOptions, stringHierarchyRegion.Options);
        Assert.True(stringHierarchyRegion.HasOptions);
        Assert.Equal(2, stringHierarchyRegion.OptionedRegionNames.Count);
        Assert.Equal(new List<string>() {$"right{StringHierarchySpec.OPTION_REGION_DELIMITER}base", $"left{StringHierarchySpec.OPTION_REGION_DELIMITER}base"}, stringHierarchyRegion.OptionedRegionNames);
        Assert.Equal(regionModifiers, stringHierarchyRegion.Modifiers);
        Assert.NotSame(regionModifiers, stringHierarchyRegion.Modifiers);
        Assert.True(stringHierarchyRegion.HasModifiers);
        Assert.Equal(regionSubregions, stringHierarchyRegion.Subregions);
        Assert.NotSame(regionSubregions, stringHierarchyRegion.Subregions);
        Assert.Same(regionSubregions["child1"], stringHierarchyRegion.Subregions["child1"]);
        Assert.Same(regionSubregions["child2"], stringHierarchyRegion.Subregions["child2"]);
        Assert.True(stringHierarchyRegion.HasSubregions);
        Assert.False(stringHierarchyRegion.IsLeaf);
        Assert.Empty(stringHierarchyRegion.SavedLocations);
        Assert.Empty(stringHierarchyRegion.SavedAreas);

        var child = new StringHierarchyRegion("child", stringHierarchyRegion, stringHierarchyRegion.Options, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());

        Assert.Same(parent, stringHierarchyRegion.ParentRegion);
        Assert.Equal(parent.Options, stringHierarchyRegion.ParentOptions);
    }

    /// <summary>
    /// Test the ToString method.
    /// </summary>
    [Fact]
    public void ToString_ShouldOutputStringRep()
    {
        var nullRegion = new StringHierarchyRegion("base", null);
        var emptyRegion = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var subregionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "grandchild", new StringHierarchyRegion("grandchild", null) }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null, null, new HashSet<string> { "opt1", "opt2" }, null, subregionSubregions) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", emptyRegion, emptyRegion.Options, regionOptions, regionModifiers, regionSubregions);

        Assert.Equal("base", nullRegion.ToString());
        Assert.Equal("root", emptyRegion.ToString());
        Assert.Equal("[right,left] base | [mod1,mod2], [mod3,mod4]\n    [right,left] base, [opt1,opt2] child1\n        [right,left] base, [opt1,opt2] child1, grandchild\n    [right,left] base, child2", stringHierarchyRegion.ToString());
    }

    /// <summary>
    /// Test the TryGetSubregion method with various malformed or otherwise incorrect regions.
    /// </summary>
    [Fact]
    public void TryGetSubregion_ShouldOutputNull()
    {
        var parent = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, regionOptions, regionModifiers, regionSubregions);

        StringHierarchyRegion foundSubregion;
        var output = stringHierarchyRegion.TryGetSubregion("", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("reg1, , reg2", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("reg1,  reg2", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("opt1 opt2 reg1", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("opt1   reg1", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("base2", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("middle base", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("base", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("child1", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);


        output = stringHierarchyRegion.TryGetSubregion("left base | modSet1", out foundSubregion);

        Assert.False(output);
        Assert.Null(foundSubregion);
    }

    /// <summary>
    /// Test the TryGetSubregion method with region present in current level and region present in subregion.
    /// </summary>
    [Fact]
    public void TryGetSubregion_ShouldOutputRegion()
    {
        var parent = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var subregionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "grandchild", new StringHierarchyRegion("grandchild", null) }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null, null, new HashSet<string> { "opt1", "opt2" }, null, subregionSubregions) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, regionOptions, regionModifiers, regionSubregions);

        StringHierarchyRegion foundSubregion;
        var output = stringHierarchyRegion.TryGetSubregion("right base", out foundSubregion);

        Assert.True(output);
        Assert.Equal(stringHierarchyRegion, foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("left base, child2", out foundSubregion);

        Assert.True(output);
        Assert.Equal(regionSubregions["child2"], foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("left base, opt2 child1", out foundSubregion);

        Assert.True(output);
        Assert.Equal(regionSubregions["child1"], foundSubregion);

        output = stringHierarchyRegion.TryGetSubregion("left base, opt2 child1, grandchild", out foundSubregion);

        Assert.True(output);
        Assert.Equal(subregionSubregions["grandchild"], foundSubregion);
    }

    /// <summary>
    /// Test the AddSubregion method with new subregions.
    /// </summary>
    [Fact]
    public void AddSubregion_ShouldAddSubregion()
    {
        var parent = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, regionOptions, regionModifiers, regionSubregions);

        StringHierarchyRegion oldRegion;

        var errorSuccess = false;
        try
        {
            stringHierarchyRegion.AddSubregion(null, out oldRegion);
        }
        catch (ArgumentNullException ex) { errorSuccess = true; }
        Assert.True(errorSuccess);

        var newChildRegion = new StringHierarchyRegion("child3", null);

        stringHierarchyRegion.AddSubregion(newChildRegion, out oldRegion);

        Assert.Null(oldRegion);
        Assert.Same(newChildRegion, stringHierarchyRegion.Subregions["child3"]);
    }

    /// <summary>
    /// Test the AddSubregion method with duplicate subregions.
    /// </summary>
    [Fact]
    public void AddSubregion_ShouldReplaceSubregion()
    {
        var parent = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, regionOptions, regionModifiers, regionSubregions);

        StringHierarchyRegion oldRegion;

        stringHierarchyRegion.AddSubregion(regionSubregions["child2"], out oldRegion);

        Assert.Equal(regionSubregions["child2"], oldRegion);
        Assert.Same(regionSubregions["child2"], stringHierarchyRegion.Subregions["child2"]);

        var newChildRegion1 = new StringHierarchyRegion("child1", null);

        stringHierarchyRegion.AddSubregion(newChildRegion1, out oldRegion);

        Assert.Equal(regionSubregions["child1"], oldRegion);
        Assert.Same(newChildRegion1, stringHierarchyRegion.Subregions["child1"]);

        var newChildRegion2 = new StringHierarchyRegion("child2", null, null, regionOptions);

        stringHierarchyRegion.AddSubregion(newChildRegion2, out oldRegion);

        Assert.Equal(regionSubregions["child2"], oldRegion);
        Assert.Same(newChildRegion2, stringHierarchyRegion.Subregions["child2"]);
    }

    /// <summary>
    /// Test the DeepCopy method with retaining the parent reference.
    /// </summary>
    [Fact]
    public void DeepCopy_ShouldDeepCopy()
    {
        var nullRegion = new StringHierarchyRegion("base", null);
        var emptyRegion = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", emptyRegion, emptyRegion.Options, regionOptions, regionModifiers, regionSubregions);

        var nullRegionCopy = nullRegion.DeepCopy(true);

        Assert.Equivalent(nullRegion, nullRegionCopy, strict: true);
        Assert.NotEqual(nullRegion, nullRegionCopy);
        Assert.NotSame(nullRegion, nullRegionCopy);

        var emptyRegionCopy = emptyRegion.DeepCopy(true);

        Assert.Equivalent(emptyRegion, emptyRegionCopy, strict: true);
        Assert.NotEqual(emptyRegion, emptyRegionCopy);
        Assert.NotSame(emptyRegion, emptyRegionCopy);

        var stringHierarchyRegionCopy = stringHierarchyRegion.DeepCopy(true);

        Assert.Equal(stringHierarchyRegion.ToString(), stringHierarchyRegionCopy.ToString());
        Assert.Same(stringHierarchyRegion.ParentRegion, stringHierarchyRegionCopy.ParentRegion);
        Assert.Equal(stringHierarchyRegion.Options, stringHierarchyRegionCopy.Options);
        Assert.NotSame(stringHierarchyRegion.Options, stringHierarchyRegionCopy.Options);
        Assert.Equal(stringHierarchyRegion.Modifiers, stringHierarchyRegionCopy.Modifiers);
        Assert.NotSame(stringHierarchyRegion.Modifiers, stringHierarchyRegionCopy.Modifiers);
        // Testing Note: Due to the presence of parentRegion in each of the subregions and due to the deep copying of the subregions themselves, it is not feasible to check equality or equivalence of the subregions
        Assert.NotEqual(stringHierarchyRegion.Subregions, stringHierarchyRegionCopy.Subregions);
        Assert.NotSame(stringHierarchyRegion.Subregions, stringHierarchyRegionCopy.Subregions);
    }

    /// <summary>
    /// Test the DeepCopy method with retaining the parent reference.
    /// </summary>
    [Fact]
    public void DeepCopy_ShouldDeepCopyResetParent()
    {
        var nullRegion = new StringHierarchyRegion("base", null);
        var parent = new StringHierarchyRegion("root", nullRegion, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, regionOptions, regionModifiers, regionSubregions);

        var parentCopy = parent.DeepCopy(false);

        Assert.NotEqual(parent.ParentRegion, parentCopy.ParentRegion);
        parent.ParentRegion = null;
        Assert.Equal(parent.ParentRegion, parentCopy.ParentRegion);
        Assert.Equivalent(parent, parentCopy, strict: true);
        Assert.NotEqual(parent, parentCopy);
        Assert.NotSame(parent, parentCopy);

        var stringHierarchyRegionCopy = stringHierarchyRegion.DeepCopy(false);

        Assert.NotEqual(stringHierarchyRegion.ParentRegion, stringHierarchyRegionCopy.ParentRegion);
        stringHierarchyRegion.ParentRegion = null;
        Assert.Equal(stringHierarchyRegion.ParentRegion, stringHierarchyRegionCopy.ParentRegion);
        Assert.Equal(stringHierarchyRegion.ToString(), stringHierarchyRegionCopy.ToString());
        Assert.Same(stringHierarchyRegion.ParentRegion, stringHierarchyRegionCopy.ParentRegion);
        Assert.Equal(stringHierarchyRegion.Options, stringHierarchyRegionCopy.Options);
        Assert.NotSame(stringHierarchyRegion.Options, stringHierarchyRegionCopy.Options);
        Assert.Equal(stringHierarchyRegion.Modifiers, stringHierarchyRegionCopy.Modifiers);
        Assert.NotSame(stringHierarchyRegion.Modifiers, stringHierarchyRegionCopy.Modifiers);
        // Testing Note: Due to the presence of parentRegion in each of the subregions and due to the deep copying of the subregions themselves, it is not feasible to check equality or equivalence of the subregions
        Assert.NotEqual(stringHierarchyRegion.Subregions, stringHierarchyRegionCopy.Subregions);
        Assert.NotSame(stringHierarchyRegion.Subregions, stringHierarchyRegionCopy.Subregions);
    }

    /// <summary>
    /// Test the IsValidModifierSpec method with invalid modifier specs.
    /// </summary>
    [Fact]
    public void IsValidModifierSpec_ShouldOutputFalse()
    {
        var parent = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, regionOptions, regionModifiers, regionSubregions);

        var modSpec1 = "";
        var modSpec2 = "mod5";
        var modSpec3 = "mod1, mod2";
        var modSpec4 = "mod1, mod2, mod3";

        Assert.False(stringHierarchyRegion.IsValidModifierSpec(modSpec1));
        Assert.False(stringHierarchyRegion.IsValidModifierSpec(modSpec2));
        Assert.False(stringHierarchyRegion.IsValidModifierSpec(modSpec3));
        Assert.False(stringHierarchyRegion.IsValidModifierSpec(modSpec4));
    }

    /// <summary>
    /// Test the IsValidModifierSpec method with valid modifier specs.
    /// </summary>
    [Fact]
    public void IsValidModifierSpec_ShouldOutputTrue()
    {
        var parent = new StringHierarchyRegion("root", null, null, new HashSet<string>(), new Dictionary<string, HashSet<string>>(), new Dictionary<string, StringHierarchyRegion>());
        var regionOptions = new HashSet<string>() { "right", "left" };
        var regionModifiers = new Dictionary<string, HashSet<string>>
        {
            { "modSet1", new HashSet<string>() { "mod1", "mod2" } },
            { "modSet2", new HashSet<string>() { "mod3", "mod4" } }
        };
        var regionSubregions = new Dictionary<string, StringHierarchyRegion>
        {
            { "child1", new StringHierarchyRegion("child1", null) },
            { "child2", new StringHierarchyRegion("child2", null) }
        };

        var stringHierarchyRegion = new StringHierarchyRegion("base", parent, parent.Options, regionOptions, regionModifiers, regionSubregions);

        var modSpec1 = "mod3";
        var modSpec2 = "mod1, mod4";

        Assert.True(stringHierarchyRegion.IsValidModifierSpec(modSpec1));
        Assert.True(stringHierarchyRegion.IsValidModifierSpec(modSpec2));
    }
}
