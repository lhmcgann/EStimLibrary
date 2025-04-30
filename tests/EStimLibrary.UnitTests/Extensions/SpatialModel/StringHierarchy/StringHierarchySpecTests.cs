using Xunit;
using EStimLibrary.Extensions.SpatialModel.StringHierarchy;


namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy;

    public class StringHierarchySpecTests
    {
        private readonly ITestOutputHelper _output;

    public StringHierarchySpecTests(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    /// Tests:
    /// StringHierarchySpec(string fullSpec) --- DONE
    /// StringHierarchySpec((string[] RegionSet, string[] ModifierSet) tuple) --- DONE
    /// ParseFullSpec(string fullSpec) --- DONE
    /// ParseRegionSpec(string regionSpec) --- DONE
    /// ParseModifierSpec(string modifierSpec) --- DONE
    /// JoinFullSpec(string[] regionSet, string[] modifierSet) --- DONE
    /// JoinRegionSet(string[] regionSet) --- DONE
    /// JoinModifierSet(string[] modifierSet) --- DONE
    /// TryParseOptionedRegionName(string optionedRegionName, out string baseName, out string options) --- DONE
    /// RegionSetOverlaps(StringHierarchySpec other, out string[] sharedRegionSet) --- DEFUNCT
    /// ModifiersAllowOverlap(StringHierarchySpec other, out string[] commonModifiers) --- DEFUNCT
    /// TryGetOverlap(StringHierarchySpec other, out string overlappingRegion, out bool contains) --- MOVE TO BodyModel TESTING
    /// Equals(StringHierarchySpec? other) --- DONE
    /// GetHashCode() --- DONE
    /// ToString() --- DONE

    /// <summary>
    /// Test the static example spec is correct.
    /// </summary>
    [Fact]
    public void ExamplePath_ShouldBeCorrect()
    {
        Assert.Equal("option1 region1, no-option-region2, ... | modifier, ...",
            StringHierarchySpec.ExamplePath);
    }

    /// <summary>
    /// Test the constructor StringHierarchySpec that takes the full string 
    /// spec, where the input field is not empty.
    /// </summary>
    [Theory]
    // Case with regions and modifiers
    [InlineData("region1, region2 | modifier1, modifier2",
                new[] { "region1", "region2" },
                new[] { "modifier1", "modifier2" })]
    // Case with only one region
    [InlineData("region1",
                new[] { "region1" },
                new string[0])]
    // Case sensitivity
    [InlineData("Region1, REGION2 | Modifier1, MODIFIER2",
                new[] { "region1", "region2" },
                new[] { "modifier1", "modifier2" })]
    // Single region and modifier
    [InlineData("region1 | modifier1",
                new[] { "region1" },
                new[] { "modifier1" })]
    public void ConstructorFullSpec_ValidInputs_ShouldInit(string fullSpec, 
        string[] expectedRegions, string[] expectedModifiers)
    {
        // Build the spec and make sure sets parsed correctly
        var stringHierarchySpec = new StringHierarchySpec(fullSpec);
        Assert.Equal(expectedRegions, stringHierarchySpec.RegionSet);
        Assert.Equal(expectedModifiers, stringHierarchySpec.ModifierSet);

        // Make sure partial string specs are correct
        var specHalves = fullSpec.Split(
            StringHierarchySpec.REGIONS_MODIFIERS_DELIMITER);
        Assert.Equal(specHalves[0].Trim(), stringHierarchySpec.RegionSpec);
        Assert.Equal(specHalves[1].Trim(), stringHierarchySpec.ModifierSpec);
    }

    /// <summary>
    /// Test the constructor StringHierarchySpec(string fullSpec), where the
    /// input field is empty.
    /// </summary>
    [Theory]
    [InlineData("",
                new[] { "" },
                new string[0])] // Case with empty input
    public void ConstructorFullSpec_EmptyInputs_ShouldInit(string fullSpec, 
        string[] expectedRegions, string[] expectedModifiers)
    {
        var stringHierarchySpec = new StringHierarchySpec(fullSpec);
        Assert.Equal(expectedRegions, stringHierarchySpec.RegionSet);
        Assert.Equal(expectedModifiers, stringHierarchySpec.ModifierSet);
    }

    /// <summary>
    /// Test the constructor StringHierarchySpec that takes the tuple of region
    /// and modifier string sets.
    /// </summary>
    [Theory]
    // Normal case
    [InlineData(new[] { "region1", "region2" }, 
        new[] { "modifier1", "modifier2" })]
    // Single region, no modifiers
    [InlineData(new[] { "region1" }, new string[0])]
    // Both arrays empty
    [InlineData(new string[0], new string[0])]
    // Case sensitivity check
    [InlineData(new[] { "Region1", "Region2" }, new[] { "Modifier1" })]
    public void ConstructorTuple_ValidInputs_ShouldInit(
        string[] expectedRegions, string[] expectedModifiers)
    {
        var tuple = (expectedRegions, expectedModifiers);
        var stringHierarchySpec = new StringHierarchySpec(tuple);
        Assert.Equal(expectedRegions, stringHierarchySpec.RegionSet);
        Assert.Equal(expectedModifiers, stringHierarchySpec.ModifierSet);
    }

        /// <summary>
        /// Test the method ParseFullSpec. This requires the methods ParseRegionSpec and 
        /// ParseModifierSpec to function correctly as well. Assumes that fullspec includes 
        /// regions and modifiers.
        /// </summary>
        [Theory]
        // Standard case
        [InlineData("region1, region2, region3 | modifier1, modifier2",
                    new[] { "region1", "region2", "region3" },
                    new[] { "modifier1", "modifier2" })] 
        // Case sensitivity for regions and modifiers
        [InlineData("Region1, REGION2 | Modifier1, MODIFIER2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })] 
        // Extra spaces in regions and modifiers
        [InlineData("region1 , region2 , region3   | modifier1 , modifier2 ",
                    new[] { "region1", "region2", "region3" },
                    new[] { "modifier1", "modifier2" })] 
        // Special characters in regions and modifiers
        [InlineData("region1, region2, region@#$ | modifier@#$, modifier123",
                    new[] { "region1", "region2", "region@#$" },
                    new[] { "modifier@#$", "modifier123" })] 
        // Single region with multiple modifiers
        [InlineData("region1 | modifier1, modifier2, modifier3",
                    new[] { "region1" },
                    new[] { "modifier1", "modifier2", "modifier3" })] 

        public void ParseFullSpec_ValidInputs_ShouldParse(string fullSpec, string[] 
            expectedRegions, string[] expectedModifiers)
        {
            var (regionSet, modifierSet) = StringHierarchySpec.ParseFullSpec(fullSpec);
            Assert.Equal(expectedRegions, regionSet);
            Assert.Equal(expectedModifiers, modifierSet);
        }

        /// <summary>
        /// Test the method ParseFullSpec. This requires the methods ParseRegionSpec and 
        /// ParseModifierSpec to function correctly as well. Assumes that fullspec is at 
        /// least partly empty (no regions and/or fields).
        /// </summary>
        [Theory]
        // Case with only regions, no modifiers
        [InlineData("region1, region2, region3",
                    new[] { "region1", "region2", "region3" },
                    new string[0])] 
        // Case with only modifiers, no regions
        [InlineData(" | modifier1, modifier2",
                    new[] { "" },
                    new[] { "modifier1", "modifier2" })] 
        // Empty input
        [InlineData("",
                    new[] { "" },
                    new string[0])] 
        public void ParseFullSpec_EmptyInputs_ShouldParse(string fullSpec, 
            string[] expectedRegions, string[] expectedModifiers)
        {
            var (regionSet, modifierSet) = StringHierarchySpec.ParseFullSpec(fullSpec);
            Assert.Equal(expectedRegions, regionSet);
            Assert.Equal(expectedModifiers, modifierSet);
        }

        /// <summary>
        /// Test the method ParseFullSpec. This requires the methods ParseRegionSpec and 
        /// ParseModifierSpec to function correctly as well. Assumes that fullspec includes 
        /// regions and modifiers. Uses invalid inputs.
        /// </summary>
        [Theory]
        [InlineData(
                "region1||region2||| region3 | modifier1|| modifier2",
                new[] { "region1", "region2", "region3" },
                new[] { "modifier1", "modifier2" })]
        [InlineData(
                "|| region1 , , region2 | | mod1 , , mod2 ||",
                new[] { "region1", "region2" },
                new[] { "mod1", "mod2" })]
        [InlineData("||||", new[] { "" }, new string[0])]


        public void ParseFullSpec_InvalidInputs_ShouldParse(string fullSpec, string[] 
            expectedRegions, string[] expectedModifiers)
        {
            var (regionSet, modifierSet) = StringHierarchySpec.ParseFullSpec(fullSpec);
            Assert.Equal(expectedRegions, regionSet);
            Assert.Equal(expectedModifiers, modifierSet);
        }

        /// <summary>
        /// Test the method ParseRegionSpec. Uses non-empty inputs only.
        /// </summary>
        [Theory]
        [InlineData("region1, region2, region3", new[] { "region1", "region2", "region3" })]
        [InlineData("region1", new[] { "region1" })]
        // Empty input
        [InlineData("", new string[] { "" })] 
        // Case insensitivity
        [InlineData("Region1, REGION2, ReGiOn3", new[] { "region1", "region2", "region3" })] 
        // Extra spaces
        [InlineData(" region1 , region2 ,  region3 ", new[] { "region1", "region2", "region3" })] 
        // Special characters
        [InlineData("region1, region@#$, region123", new[] { "region1", "region@#$", "region123" })] 
        // Duplicate regions
        [InlineData("region1, region2, region1", new[] { "region1", "region2", "region1" })] 
        public void ParseRegionSpec_ValidInputs_ShouldParseRegions(string input, 
            string[] expectedOutput)
        {
            var result = StringHierarchySpec.ParseRegionSpec(input);
            Assert.Equal(expectedOutput, result);
        }

        /// <summary>
        /// Test the method ParseRegionSpec. Uses empty inputs only.
        /// </summary>
        [Theory]
        [InlineData("", new string[] { "" })] // Empty input
        public void ParseRegionSpec_EmptyInputs_ShouldParseRegions(string input, 
            string[] expectedOutput)
        {
            var result = StringHierarchySpec.ParseRegionSpec(input);
            Assert.Equal(expectedOutput, result);
        }

        /// <summary>
        /// Test the method ParseModifierSpec. Uses non-empty inputs only.
        /// </summary>
        [Theory]
        [InlineData("modifier1, modifier2, modifier3", new[] { "modifier1", "modifier2", "modifier3" })]
        [InlineData("modifier1", new[] { "modifier1" })]
        // Case insensitivity
        [InlineData("Modifier1, MODIFIER2, mODiFier3", new[] { "modifier1", "modifier2", "modifier3" })] 
        // Extra spaces
        [InlineData(" modifier1 , modifier2 ,  modifier3 ", new[] { "modifier1", "modifier2", "modifier3" })] 
        // Special characters
        [InlineData("modifier1, modifier@#$, modifier123", new[] { "modifier1", "modifier@#$", "modifier123" })] 
        // Duplicate regions
        [InlineData("modifier1, modifier2, modifier1", new[] { "modifier1", "modifier2", "modifier1" })] 
        public void ParseModifierSpec_ValidInputs_ShouldParseModifiers(string input, 
            string[] expectedOutput)
        {
            var result = StringHierarchySpec.ParseModifierSpec(input);
            Assert.Equal(expectedOutput, result);
        }

        /// <summary>
        /// Test the method ParseModifierSpec. Uses empty inputs only.
        /// </summary>
        [Theory]
        [InlineData("", new string[] { "" })] // Empty input
        public void ParseModifierSpec_EmptyInputs_ShouldParseModifiers(string input, 
            string[] expectedOutput)
        {
            var result = StringHierarchySpec.ParseModifierSpec(input);
            Assert.Equal(expectedOutput, result);
        }

        /// <summary>
        /// Test the JoinFullSpec method. Uses non-empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new string[] { "region1", "region2" }, new string[] { "modifier1", 
            "modifier2" }, "region1, region2 | modifier1, modifier2")]
        [InlineData(new string[] { "region1" }, new string[] { "modifier1" }, 
            "region1 | modifier1")]
        [InlineData(new string[] { "region1" }, new string[] { }, "region1")]
        [InlineData(new string[] { }, new string[] { }, "")]
        public void JoinFullSpec_ValidInputs_ShouldJoinRegionsAndModifiers(string[] 
            regions, string[] modifiers, string expected)
        {
            var result = StringHierarchySpec.JoinFullSpec(regions, modifiers);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Test the JoinFullSpec method. Uses empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new string[] { "region1" }, new string[] { }, "region1")]
        [InlineData(new string[] { }, new string[] { }, "")]
        public void JoinFullSpec_EmptyInputs_ShouldJoinRegionsAndModifiers(string[] 
            regions, string[] modifiers, string expected)
        {
            var result = StringHierarchySpec.JoinFullSpec(regions, modifiers);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Test the JoinRegionSet method. Uses non-empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new string[] { "region1", "region2", "region3" }, 
            "region1, region2, region3")]
        [InlineData(new string[] { "region1" }, "region1")]
        public void JoinRegionSet_ValidInputs_ShouldJoinRegions(string[] regions, 
            string expected)
        {
            var result = StringHierarchySpec.JoinRegionSet(regions);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Test the JoinRegionSet method. Uses empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new string[] { }, "")]
        public void JoinRegionSet_EmptyInputs_ShouldJoinRegions(string[] regions, 
            string expected)
        {
            var result = StringHierarchySpec.JoinRegionSet(regions);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Test the JoinModifierSet method. Uses non-empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new string[] { "modifier1", "modifier2", "modifier3" }, 
            "modifier1, modifier2, modifier3")]
        [InlineData(new string[] { "modifier1" }, "modifier1")]
        public void JoinModifierSet_ValidInputs_ShouldJoinRegions(string[]
            modifiers, string expected)
        {
            var result = StringHierarchySpec.JoinModifierSet(modifiers);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Test the JoinModifierSet method. Uses empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new string[] { }, "")]
        public void JoinModifierSet_EmptyInputs_ShouldJoinRegions(string[] 
            modifiers, string expected)
        {
            var result = StringHierarchySpec.JoinModifierSet(modifiers);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Test the TryParseOptionedRegionName method. Uses non-empty inputs.
        /// </summary>
        [Theory]
        [InlineData("option1 regionName", "regionName", "option1", true)]
        [InlineData("option1 option2 regionName", "", "", false)]
        [InlineData("regionName", "regionName", "", true)]
        [InlineData("     regionName        ", "regionName", "", true)]
        [InlineData("option1     option2 ", "option2", "option1", true)]
        public void TryParseOptionedRegionName_ValidInputs_ShouldParse(string 
            input, string expectedBaseName, string expectedOptions, bool expectedResult)
        {
            var result = StringHierarchySpec.TryParseOptionedRegionName(input, out var 
                baseName, out var options);
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedBaseName, baseName);
            Assert.Equal(expectedOptions, options);
        }

        /// <summary>
        /// Test the TryParseOptionedRegionName method. Uses empty inputs.
        /// </summary>
        [Theory]
        [InlineData("", "", "", false)]
        [InlineData("   ", "", "", false)]
        public void TryParseOptionedRegionName_EmptyInputs_ShouldParse(string input, 
            string expectedBaseName, string expectedOptions, bool expectedResult)
        {
            var result = StringHierarchySpec.TryParseOptionedRegionName(input, out var 
                baseName, out var options);
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedBaseName, baseName);
            Assert.Equal(expectedOptions, options);
        }

        #region Spec Overlap Methods
        // TODO: move these tests to StringHierarchyBodyModel once move on to testing that class
        ///// <summary>
        ///// Test the RegionSetOverlaps method. Uses non-empty inputs.
        ///// </summary>
        //[Theory]
        //[InlineData(new[] { "region1", "region2", "region3" }, new[] { "region1", "region2", "region3" }, 
        //    true, new[] { "region1", "region2", "region3" })] // Full overlap
        //[InlineData(new[] { "region1", "region2", "region3" }, new[] { "region1", "region2" }, true, 
        //    new[] { "region1", "region2" })] // Partial overlap
        //[InlineData(new[] { "region1", "region2" }, new[] { "region1", "regionX" }, true, 
        //    new[] { "region1" })] // Partial overlap to first element
        //[InlineData(new[] { "region1", "region2" }, new[] { "regionX", "regionY" }, false, 
        //    new string[0])] // No overlap
        //[InlineData(new string[0], new[] { "region1", "region2" }, false, new string[0])] // One empty RegionSet
        //[InlineData(new string[0], new string[0], false, new string[0])] // Both empty RegionSet
        //public void RegionSetOverlaps_ShouldCorrectlyOverlapWithExpectedInputs(string[] firstRegionSet, 
        //     string[] secondRegionSet, bool expectedResult, string[] expectedSharedRegionSet)
        //{
        //    var spec1 = new StringHierarchySpec((firstRegionSet, new string[0]));
        //    var spec2 = new StringHierarchySpec((secondRegionSet, new string[0]));
        //    var result = spec1.RegionSetOverlaps(spec2, out var sharedRegionSet);
        //    Assert.Equal(expectedResult, result);
        //    Assert.Equal(expectedSharedRegionSet, sharedRegionSet);
        //}

        ///// <summary>
        ///// Test the RegionSetOverlaps method. Uses empty inputs.
        ///// </summary>
        //[Theory]
        //[InlineData(new string[0], new[] { "region1", "region2" }, false, new string[0])] // One empty RegionSet
        //[InlineData(new string[0], new string[0], false, new string[0])] // Both empty RegionSet
        //public void RegionSetOverlaps_ShouldCorrectlyOverlapWithEmptyInputs(string[] firstRegionSet, 
        // string[] secondRegionSet, bool expectedResult, string[] expectedSharedRegionSet)
        //{
        //    var spec1 = new StringHierarchySpec((firstRegionSet, new string[0]));
        //    var spec2 = new StringHierarchySpec((secondRegionSet, new string[0]));
        //    var result = spec1.RegionSetOverlaps(spec2, out var sharedRegionSet);
        //    Assert.Equal(expectedResult, result);
        //    Assert.Equal(expectedSharedRegionSet, sharedRegionSet);
        //}

        ///// <summary>
        ///// Test the ModifiersAllowOverlap method. Uses non-empty inputs.
        ///// </summary>
        //[Theory]
        //[InlineData(new[] { "modifier1", "modifier2", "modifier3" }, 
        //    new[] { "modifier1", "modifier2", "modifier3" }, true, new[] { "modifier1", "modifier2", "modifier3" })] // Full overlap
        //[InlineData(new[] { "modifier1", "modifier2" }, new[] { "modifier1", "modifier3" }, 
        //    false, new[] { "modifier1" })] // Partial overlap
        //[InlineData(new[] { "modifier1", "modifier2" }, new[] { "modifierX", "modifierY" }, 
        //    false, new string[0])] // No overlap
        ////[InlineData(new string[0], new[] { "mod1", "mod2" }, false, new string[0])] // One empty ModifierSet
        ////[InlineData(new string[0], new string[0], false, new string[0])] // Both empty ModifierSet
        //public void ModifiersAllowOverlap_ShouldCorrectlyOverlapWithExpectedInputs(
        //    string[] firstModifierSet, string[] secondModifierSet, bool expectedResult, string[] expectedCommonModifiers)
        //{
        //    var spec1 = new StringHierarchySpec((new string[0], firstModifierSet));
        //    var spec2 = new StringHierarchySpec((new string[0], secondModifierSet));
        //    var result = spec1.ModifiersAllowOverlap(spec2, out var commonModifiers);
        //    Assert.Equal(expectedResult, result);
        //    Assert.Equal(expectedCommonModifiers, commonModifiers);
        //}

        ///// <summary>
        ///// Test the ModifiersAllowOverlap method. Uses empty inputs.
        ///// </summary>
        //[Theory]
        //[InlineData(new string[0], new[] { "modifier1", "modifier2" }, 
        //    true, new string[0])] // One empty ModifierSet
        //[InlineData(new string[0], new string[0], true, new string[0])] 
        //    // Both empty ModifierSet
        //public void ModifiersAllowOverlap_ShouldCorrectlyOverlapWithEmptyInputs(
        //    string[] firstModifierSet, string[] secondModifierSet, bool expectedResult, 
        //    string[] expectedCommonModifiers)
        //{
        //    var spec1 = new StringHierarchySpec((new string[0], firstModifierSet));
        //    var spec2 = new StringHierarchySpec((new string[0], secondModifierSet));
        //    var result = spec1.ModifiersAllowOverlap(spec2, out var commonModifiers);
        //    Assert.Equal(expectedResult, result);
        //    Assert.Equal(expectedCommonModifiers, commonModifiers);
        //}

    ///// <summary>
    ///// Test the TryGetOverlap method. Uses inputs that should lead to a succesful overlap.
    ///// </summary>
    //[Theory]
    //[InlineData(
    //new[] { "region1", "region2" }, new[] { "modifier1", "modifier2" },
    //new[] { "region1", "region2" }, new[] { "modifier1", "modifier2" },
    //true, true, "region1, region2 | modifier1, modifier2"
    //)] // Complete containment

    //[InlineData(
    //new[] { "region1" }, new[] { "modifier1" },
    //new[] { "region1", "region2" }, new[] { "modifier1", "modifier2" },
    //true, true, "region1, region2 | modifier1, modifier2"
    //)] // Partial overlap

    //public void TryGetOverlap_ShouldIdentifyOverlapIsPossible(
    //    string[] firstRegionSet, string[] firstModifierSet,
    //    string[] secondRegionSet, string[] secondModifierSet,
    //    bool expectedOverlap, bool expectedContains, string expectedOverlappingRegion)
    //{
    //    var spec1 = new StringHierarchySpec((firstRegionSet, firstModifierSet));
    //    var spec2 = new StringHierarchySpec((secondRegionSet, secondModifierSet));
    //    var result = spec1.TryGetOverlap(spec2, out var overlappingRegion, out var contains);
    //    Assert.Equal(expectedOverlap, result);
    //    Assert.Equal(expectedContains, contains);
    //    Assert.Equal(expectedOverlappingRegion, overlappingRegion);
    //}

    ///// <summary>
    ///// Test the TryGetOverlap method. Uses inputs that should lead to an unsuccesful overlap.
    ///// </summary>
    //[Theory]
    //[InlineData(
    //new[] { "regionA", "regionB" }, new[] { "modifierX", "modifierY" },
    //new[] { "region1", "region2" }, new[] { "modifier1", "modifier2" },
    //false, false, ""
    //)] // No overlap

    //[InlineData(
    //new[] { "region1" }, new[] { "modifier1", "modifier2" },
    //new[] { "region1" }, new[] { "modifier3", "modifier4" },
    //false, false, ""
    //)] // Region-only overlap

    //[InlineData(
    //new string[0], new string[0],
    //new string[0], new string[0],
    //false, false, ""
    //)] // Empty sets

        //public void TryGetOverlap_ShouldIdentifyOverlapIsNotPossible(
        //    string[] firstRegionSet, string[] firstModifierSet,
        //    string[] secondRegionSet, string[] secondModifierSet,
        //    bool expectedOverlap, bool expectedContains, string expectedOverlappingRegion)
        //{
        //    var spec1 = new StringHierarchySpec((firstRegionSet, firstModifierSet));
        //    var spec2 = new StringHierarchySpec((secondRegionSet, secondModifierSet));
        //    var result = spec1.TryGetOverlap(spec2, out var overlappingRegion, out var contains);
        //    Assert.Equal(expectedOverlap, result);
        //    Assert.Equal(expectedContains, contains);
        //    Assert.Equal(expectedOverlappingRegion, overlappingRegion);
        //}
        #endregion Spec Overlap Methods

        /// <summary>
        /// Test the TryParseOptionedRegionName method. Uses non-empty inputs.
        /// </summary>
        [Theory]
        [InlineData("option1 regionName", "regionName", "option1", true)] 
        [InlineData("option1 option2 regionName", "", "", false)] 
        [InlineData("regionName", "regionName", "", true)] 
        [InlineData("  optionX   regionY  ", "regionY", "optionX", true)]
        [InlineData("regionOnly ", "regionOnly", "", true)] 
        [InlineData("option1", "option1", "", true)] 
        public void TryParseOptionedRegionName_ValidInputs_ShouldParseOptionedRegionName(
            string input, string expectedBaseName, string expectedOption, bool expectedResult){
            var result = StringHierarchySpec.TryParseOptionedRegionName(input, out var baseName, out var option);
    
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedBaseName, baseName);
            Assert.Equal(expectedOption, option);
        }

        /// <summary>
        /// Test the TryParseOptionedRegionName method. Uses empty inputs.
        /// </summary>
        [InlineData("", "", "", false)] 
        [InlineData("   ", "", "", false)] 
        public void TryParseOptionedRegionName_EmptyInputs_ShouldParseOptionedRegionName(
            string input, string expectedBaseName, string expectedOption, bool expectedResult){
            var result = StringHierarchySpec.TryParseOptionedRegionName(input, out var baseName, out var option);
    
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedBaseName, baseName);
            Assert.Equal(expectedOption, option);
        }

    /// <summary>
    /// Test the TryParseOptions method. Uses non-empty inputs.
    /// </summary>
    [Theory]
    // Single option
    [InlineData("option1", new[] { "option1" }, true)] 
    // Trimmed single option
    [InlineData("  option1  ", new[] { "option1" }, true)] 
    // Two options
    [InlineData("option1 option2", new[] { "option1", "option2" }, true)] 
    // Multiple options with extra spaces
    [InlineData("  option1   option2  option3  ", new[] { "option1", "option2", "option3" }, true)] 
    public void TryParseOptions_ValidInputs_ShouldParseOptions(string input, 
        string[] expectedOptions, bool expectedResult)
    {
        var result = StringHierarchySpec.TryParseOptions(input, out var parsedOptions);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedOptions, parsedOptions);
    }

    /// <summary>
    /// Test the TryParseOptions method. Uses empty inputs.
    /// </summary>
    [Theory]
    // Empty input
    [InlineData("", new string[] { }, false)] 
    // Whitespace-only input
    [InlineData("   ", new string[] { }, false)] 
    public void TryParseOptions_EmptyInputs_ShouldParseOptions(string input, 
        string[] expectedOptions, bool expectedResult)
    {
        var result = StringHierarchySpec.TryParseOptions(input, out var parsedOptions);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedOptions, parsedOptions);
    }


        /// <summary>
        /// Test the Equals method. Uses non-empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1", "Modifier2" },
            new[] { "RegionA", "RegionB" }, new[] { "Modifier2", "Modifier1" }, true)]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1", "Modifier2" },
            new[] { "RegionA", "RegionB" }, new[] { "Modifier1" }, false)]
        [InlineData(new[] { "RegionA" }, new[] { "Modifier1" },
            new[] { "RegionA" }, new[] { "Modifier1" }, true)]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1" },
            new[] { "RegionA", "RegionC" }, new[] { "Modifier1" }, false)]
        [InlineData(new[] { "RegionA" }, new[] { "Modifier1", "Modifier2" },
            new[] { "RegionA" }, new[] { "Modifier2", "Modifier1" }, true)]
        public void Equals_ValidInputs_ShouldReturnExpectedResult(
            string[] regionSet1, string[] modifierSet1,
        string[] regionSet2, string[] modifierSet2, bool expected)
        {
            var spec1 = new StringHierarchySpec(regionSet1, modifierSet1);
            var spec2 = regionSet2 != null ? new StringHierarchySpec(regionSet2, modifierSet2) : null;
            bool result = spec1.Equals(spec2);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Test the Equals method. Uses empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1" },
            new[] { "" }, new[] { "" }, false)]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1" },
            null, null, false)]

        public void Equals_EmptyInputs_ShouldReturnExpectedResult(
            string[] regionSet1, string[] modifierSet1,
        string[] regionSet2, string[] modifierSet2, bool expected)
        {
            var spec1 = new StringHierarchySpec(regionSet1, modifierSet1);
            var spec2 = regionSet2 != null ? new StringHierarchySpec(regionSet2, modifierSet2) : null;
            bool result = spec1.Equals(spec2);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Test the Equals method. Uses null as "other".
        /// </summary>
        [Fact]
        public void Equals_NullOtherSpec_ShouldReturnFalse()
        {
            var spec1 = new StringHierarchySpec(new[] { "region1" }, new[] { "modifier1" });
            StringHierarchySpec spec2 = null;

            bool result = spec1.Equals(spec2);

            Assert.False(result);
        }

        /// <summary>
        /// Test the GetHashCode method. Uses non-empty inputs.
        /// </summary>
        [Theory]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1", "Modifier2" },
            new[] { "RegionA", "RegionB" }, new[] { "Modifier2", "Modifier1" }, true)]
        [InlineData(new[] { "RegionA" }, new[] { "Modifier1" },
            new[] { "RegionA" }, new[] { "Modifier1" }, true)]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1" },
            new[] { "RegionA", "RegionC" }, new[] { "Modifier1" }, false)]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1", "Modifier2" },
            new[] { "RegionB" }, new[] { "Modifier1", "Modifier2" }, false)]
        public void GetHashCode_ValidInputs_ShouldReturnConsistentHashCodesForEqualObjects(
            string[] regionSet1, string[] modifierSet1,
            string[] regionSet2, string[] modifierSet2, bool expectEqualHashCodes)
        {
            var spec1 = new StringHierarchySpec(regionSet1, modifierSet1);
            var spec2 = new StringHierarchySpec(regionSet2, modifierSet2);
            int hash1 = spec1.GetHashCode();
            int hash2 = spec2.GetHashCode();
            if (expectEqualHashCodes)
            {
                Assert.Equal(hash1, hash2);
            }
            else
            {
                Assert.NotEqual(hash1, hash2);
            }
        }

        /// <summary>
        /// Test the GetHashCode method. Uses empty inputs.
        /// </summary>
        [Theory]
        // Empty vs empty
        [InlineData(new string[0], new string[0], new string[0], new string[0], true)] 
        // Non-empty vs empty
        [InlineData(new[] { "region1" }, new[] { "modifier1" }, new string[0], new string[0], false)] 
        // Empty vs non-empty
        [InlineData(new string[0], new string[0], new[] { "region1" }, new[] { "modifier1" }, false)] 
        //[InlineData(new[] { "region1" }, new[] { "modifier1" }, (string[])null, (string[])null, false)] 

        public void GetHashCode_EmptyInputs_ShouldReturnConsistentHashCodesForEqualObjects(
            string[] regionSet1, string[] modifierSet1,
            string[] regionSet2, string[] modifierSet2, bool expectEqualHashCodes)
        {
            var spec1 = new StringHierarchySpec(regionSet1, modifierSet1);
            var spec2 = new StringHierarchySpec(regionSet2, modifierSet2);
            int hash1 = spec1.GetHashCode();
            int hash2 = spec2.GetHashCode();
            if (expectEqualHashCodes)
            {
                Assert.Equal(hash1, hash2);
            }
            else
            {
                Assert.NotEqual(hash1, hash2);
            }
        }

        /// <summary>
        /// Test the GetHashCode method. Uses null as "other".
        /// </summary>
        [Fact]
        public void GetHashCode_OtherSpecIsNull_ShouldReturnFalse()
        {
            var spec1 = new StringHierarchySpec(new[] { "region1" }, new[] { "modifier1" });
            StringHierarchySpec spec2 = null;

            bool result = spec1.Equals(spec2);

            Assert.False(result);
        }



        /// <summary>
        /// Test the ToString method. 
        /// </summary>
        [Theory]
        [InlineData(new[] 
            { "RegionA", "RegionB" }, new[] { "Modifier1" }, "regiona, regionb | modifier1")]
        [InlineData(new[] { "main" }, new string[] { }, "main")]
        [InlineData(new[] { "x", "Y", "Z" }, new[] { "North", "South" }, "x, y, z | north, south")]
        public void ToString_ShouldReturnFullSpecRepresentation(
        string[] regionSet, string[] modifierSet, string expectedFullSpec)
        {
            var spec = new StringHierarchySpec(regionSet, modifierSet);
            var result = spec.ToString();
            Assert.Equal(expectedFullSpec, result);
        }
    }
