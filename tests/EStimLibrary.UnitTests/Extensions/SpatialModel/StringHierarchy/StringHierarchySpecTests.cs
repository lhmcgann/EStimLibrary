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
        /// TryParseOptions(string options, out string[] parsedOptions) --- DONE
        /// RegionSetSharesPath(StringHierarchySpec other, out string[] sharedRegionSet) --- DONE
        ///   NOTE: RegionSetOverlaps and ModifiersAllowOverlap were removed; replaced by RegionSetSharesPath.
        /// TryGetOverlap(StringHierarchySpec other, out string overlappingRegion, out bool contains) --- TODO: MOVE TO BodyModel TESTING
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
        /// spec, where the input is well-formed or tolerantly parsed.
        /// </summary>
        [Theory]
        // Standard case with regions and modifiers
        [InlineData("region1, region2 | modifier1, modifier2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        // Case insensitivity: should convert to lowercase
        [InlineData("Region1, REGION2 | Modifier1, MODIFIER2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        // Single region and modifier
        [InlineData("region1 | modifier1",
                    new[] { "region1" },
                    new[] { "modifier1" })]
        // Tolerant: missing space after commas
        [InlineData("region1,region2 | modifier1,modifier2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        // Tolerant: extra spaces around delimiters
        [InlineData("  region1  ,  region2  |  modifier1  ,  modifier2  ",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        // Tolerant: no spaces around pipe
        [InlineData("region1, region2|modifier1, modifier2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        public void ConstructorFullSpec_ValidInputs_ShouldInit(string fullSpec,
            string[] expectedRegions, string[] expectedModifiers)
        {
            // Build the spec and make sure sets parsed correctly
            var stringHierarchySpec = new StringHierarchySpec(fullSpec);
            Assert.Equal(expectedRegions, stringHierarchySpec.RegionSet);
            Assert.Equal(expectedModifiers, stringHierarchySpec.ModifierSet);

            // Verify the canonical RegionSpec and ModifierSpec use expected
            // delimiter format: ", " between tokens, " | " between halves.
            Assert.Equal(string.Join(", ", expectedRegions),
                stringHierarchySpec.RegionSpec);
            Assert.Equal(string.Join(", ", expectedModifiers),
                stringHierarchySpec.ModifierSpec);
        }

        /// <summary>
        /// Test the constructor StringHierarchySpec(string fullSpec) with valid
        /// inputs that have at least one region but no modifiers.
        /// </summary>
        [Theory]
        // Only a region, no pipe, no modifiers
        [InlineData("region1",
                    new[] { "region1" },
                    new string[0])]
        // Trailing pipe with no content: modifier set should be empty (not [""])
        [InlineData("region1 | ",
                    new[] { "region1" },
                    new string[0])]
        public void ConstructorFullSpec_NoModifiers_ShouldInit(string fullSpec,
            string[] expectedRegions, string[] expectedModifiers)
        {
            var stringHierarchySpec = new StringHierarchySpec(fullSpec);
            Assert.Equal(expectedRegions, stringHierarchySpec.RegionSet);
            Assert.Equal(expectedModifiers, stringHierarchySpec.ModifierSet);
        }

        /// <summary>
        /// Test that the constructor StringHierarchySpec(string fullSpec) throws
        /// an ArgumentException when the parsed region set is empty.
        /// </summary>
        [Theory]
        [InlineData("")]              // Empty string: no tokens
        [InlineData("   ")]           // Whitespace only: no tokens
        [InlineData(" | modifier1")] // Region part is empty
        [InlineData(" | ")]           // Both parts empty
        public void ConstructorFullSpec_NoRegions_ShouldThrow(string fullSpec)
        {
            Assert.Throws<ArgumentException>(() =>
                new StringHierarchySpec(fullSpec));
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
        // Case insensitivity check: should convert to lowercase
        [InlineData(new[] { "Region1", "Region2" }, new[] { "Modifier1" })]
        public void ConstructorTuple_ValidInputs_ShouldInit(
            string[] inputRegions, string[] inputModifiers)
        {
            // Keep original case in input arrays
            var tuple = (inputRegions, inputModifiers);
            // Now convert to lowercase expected values for correct comparison
            var expectedRegions = inputRegions.Select(r => r.ToLower()).ToArray();
            var expectedModifiers = inputModifiers.Select(m => m.ToLower()).ToArray();

            var stringHierarchySpec = new StringHierarchySpec(tuple);
            Assert.Equal(expectedRegions, stringHierarchySpec.RegionSet);
            Assert.Equal(expectedModifiers, stringHierarchySpec.ModifierSet);
        }

        /// <summary>
        /// Test that null region array throws exception.
        /// </summary>
        [Fact]
        public void Constructor_NullRegionArray_ShouldThrow()
        {
            // null region array normalizes to empty → throws
            Assert.Throws<ArgumentException>(() =>
                new StringHierarchySpec(null, new[] { "modifier1" }));
        }

        /// <summary>
        /// Test that empty region array throws exception.
        /// </summary>
        [Fact]
        public void Constructor_EmptyRegionArray_ShouldThrow()
        {
            // empty region array → throws
            Assert.Throws<ArgumentException>(() =>
                new StringHierarchySpec(new string[0], new[] { "modifier1" }));
        }

        /// <summary>
        /// Test that region array with only null or whitespace elements throws
        /// exception.
        /// </summary>
        [Fact]
        public void Constructor_RegionArrayWithOnlyNullOrWhitespaceElements_ShouldThrow()
        {
            // Array literal with null elements can't go in [InlineData], so tested here.
            // All elements normalize to empty → throws.
            Assert.Throws<ArgumentException>(() =>
                new StringHierarchySpec(new string[] { null!, "", "   " }, new[] { "modifier1" }));
        }

        /// <summary>
        /// Test that null modifier array is treated as empty.
        /// </summary>
        [Fact]
        public void Constructor_NullModifierArray_ShouldTreatAsEmpty()
        {
            var spec = new StringHierarchySpec(new[] { "region1" }, (string[])null);
            Assert.Equal(new[] { "region1" }, spec.RegionSet);
            Assert.Empty(spec.ModifierSet);
        }

        [Fact]
        public void Constructor_ArraysWithNullOrEmptyElements_ShouldDropThem()
        {
            // null and empty string elements are dropped; remaining valid regions survive
            var spec = new StringHierarchySpec(
                new[] { "region1", null, "", "region2" },
                new[] { null, "modifier1", "" });
            Assert.Equal(new[] { "region1", "region2" }, spec.RegionSet);
            Assert.Equal(new[] { "modifier1" }, spec.ModifierSet);
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
        // Case insensitivity for regions and modifiers
        [InlineData("Region1, REGION2 | Modifier1, MODIFIER2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        // Extra spaces around delimeters
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
        // Tolerant: comma without space
        [InlineData("region1,region2 | modifier1,modifier2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        // Tolerant: no spaces around pipe
        [InlineData("region1, region2|modifier1, modifier2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        // Tolerant: content after second pipe is ignored
        [InlineData("region1, region2 | modifier1, modifier2 | extra, stuff",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]

        public void ParseFullSpec_ValidInputs_ShouldParse(string fullSpec, string[] 
            expectedRegions, string[] expectedModifiers)
        {
            var (regionSet, modifierSet) = StringHierarchySpec.ParseFullSpec(fullSpec);
            Assert.Equal(expectedRegions, regionSet);
            Assert.Equal(expectedModifiers, modifierSet);
        }

        /// <summary>
        /// Test the method ParseFullSpec with empty, missing-part, or whitespace-only
        /// inputs. Empty tokens are filtered; trailing delimeter yields empty 
        /// modifier set.
        /// </summary>
        [Theory]
        // Only regions, no pipe, no modifiers
        [InlineData("region1, region2, region3",
                    new[] { "region1", "region2", "region3" },
                    new string[0])]
        // Leading pipe: region part is empty, region set is empty
        [InlineData(" | modifier1, modifier2",
                    new string[0],
                    new[] { "modifier1", "modifier2" })]
        // Empty input: both sets empty
        [InlineData("",
                    new string[0],
                    new string[0])]
        // Trailing pipe with no modifier content: modifier set empty (not [""])
        [InlineData("region1 | ",
                    new[] { "region1" },
                    new string[0])]
        // Empty comma-separated tokens are filtered
        [InlineData("region1, , region2 | modifier1, , modifier2",
                    new[] { "region1", "region2" },
                    new[] { "modifier1", "modifier2" })]
        public void ParseFullSpec_EmptyInputs_ShouldParse(string fullSpec,
            string[] expectedRegions, string[] expectedModifiers)
        {
            var (regionSet, modifierSet) = StringHierarchySpec.ParseFullSpec(fullSpec);
            Assert.Equal(expectedRegions, regionSet);
            Assert.Equal(expectedModifiers, modifierSet);
        }

        /// <summary>
        /// Test the method ParseFullSpec with tolerant/malformed inputs that previously
        /// produced wrong results. All cases now parse correctly with the tolerant
        /// implementation.
        /// </summary>
        [Theory]
        // Comma without space: still splits correctly
        [InlineData(
                "region1,region2,region3 | modifier1,modifier2",
                new[] { "region1", "region2", "region3" },
                new[] { "modifier1", "modifier2" })]
        // Extra spaces and double commas: empty tokens are filtered
        [InlineData(
                "  region1  , ,  region2  |  modifier1  , ,  modifier2  ",
                new[] { "region1", "region2" },
                new[] { "modifier1", "modifier2" })]
        // Second pipe: content after second | is ignored
        [InlineData(
                "region1, region2 | modifier1, modifier2 | extra, stuff",
                new[] { "region1", "region2" },
                new[] { "modifier1", "modifier2" })]
        // No spaces around pipe
        [InlineData(
                "region1,region2|modifier1,modifier2",
                new[] { "region1", "region2" },
                new[] { "modifier1", "modifier2" })]
        public void ParseFullSpec_TolerantInputs_ShouldParse(string fullSpec, string[]
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
        // Empty input: empty token is filtered, result is empty array
        [InlineData("", new string[] { })]
        // Case insensitivity
        [InlineData("Region1, REGION2, ReGiOn3", new[] { "region1", "region2", "region3" })]
        // Extra spaces around commas
        [InlineData(" region1 , region2 ,  region3 ", new[] { "region1", "region2", "region3" })]
        // Special characters
        [InlineData("region1, region@#$, region123", new[] { "region1", "region@#$", "region123" })]
        // Duplicate regions: preserved as-is (no deduplication)
        [InlineData("region1, region2, region1", new[] { "region1", "region2", "region1" })]
        // Tolerant: comma without space
        [InlineData("region1,region2,region3", new[] { "region1", "region2", "region3" })]
        // Tolerant: double comma (empty middle token filtered)
        [InlineData("region1, , region2", new[] { "region1", "region2" })]
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
        [InlineData("", new string[] { })] // Empty input: filtered to empty array
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
        // Empty input: empty token is filtered, result is empty array
        [InlineData("", new string[] { })]
        // Case insensitivity
        [InlineData("Modifier1, MODIFIER2, mODiFier3", new[] { "modifier1", "modifier2", "modifier3" })]
        // Extra spaces around commas
        [InlineData(" modifier1 , modifier2 ,  modifier3 ", new[] { "modifier1", "modifier2", "modifier3" })]
        // Special characters
        [InlineData("modifier1, modifier@#$, modifier123", new[] { "modifier1", "modifier@#$", "modifier123" })]
        // Duplicate modifiers: preserved as-is (no deduplication)
        [InlineData("modifier1, modifier2, modifier1", new[] { "modifier1", "modifier2", "modifier1" })]
        // Tolerant: comma without space
        [InlineData("modifier1,modifier2,modifier3", new[] { "modifier1", "modifier2", "modifier3" })]
        // Tolerant: double comma (empty middle token filtered)
        [InlineData("modifier1, , modifier2", new[] { "modifier1", "modifier2" })]
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
        [InlineData("", new string[] { })] // Empty input: filtered to empty array
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
        /// The last whitespace-separated token is always the base name; all
        /// preceding tokens are options (joined by a single space). Any number
        /// of options is allowed.
        /// </summary>
        [Theory]
        // One option + base name
        [InlineData("option1 regionName", "regionName", "option1", true)]
        // Two options + base name: returns base = last token, options = rest joined
        [InlineData("option1 option2 regionName", "regionName", "option1 option2", true)]
        // No option, just base name
        [InlineData("regionName", "regionName", "", true)]
        // Extra internal and edge spaces: collapsed
        [InlineData("  optionX   regionY  ", "regionY", "optionX", true)]
        // Trailing space only: single token = base name
        [InlineData("regionOnly ", "regionOnly", "", true)]
        // Single token with no spaces is treated as base name only
        [InlineData("option1", "option1", "", true)]
        // Edge whitespace trimmed
        [InlineData("     regionName        ", "regionName", "", true)]
        // Multiple internal spaces collapsed
        [InlineData("option1     option2 ", "option2", "option1", true)]
        // Real-world multi-word region name: "left index finger"
        [InlineData("left index finger", "finger", "left index", true)]
        // Three options + base name
        [InlineData("a b c base", "base", "a b c", true)]
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
        /// Test the RegionSetSharesPath method. Uses non-empty inputs.
        /// </summary>
        [Theory]
        // Full shared path: both region sets are identical
        [InlineData(new[] { "a", "b", "c" }, new[] { "a", "b", "c" }, true, new[] { "a", "b", "c" })]
        // Partial match: this spec is longer; shared prefix = shorter length
        [InlineData(new[] { "a", "b", "c" }, new[] { "a", "b" }, true, new[] { "a", "b" })]
        // Partial match: other spec is longer; shared prefix = this length
        [InlineData(new[] { "a", "b" }, new[] { "a", "b", "c" }, true, new[] { "a", "b" })]
        // Only first element is shared
        [InlineData(new[] { "a", "b" }, new[] { "a", "x" }, true, new[] { "a" })]
        // No shared prefix: first elements differ
        [InlineData(new[] { "a", "b" }, new[] { "x", "y" }, false, new string[0])]
        // Single-element sets that match
        [InlineData(new[] { "a" }, new[] { "a" }, true, new[] { "a" })]
        public void RegionSetSharesPath_ValidInputs_ShouldFindSharedPath(
            string[] firstRegionSet, string[] secondRegionSet,
            bool expectedResult, string[] expectedSharedRegionSet)
        {
            var spec1 = new StringHierarchySpec((firstRegionSet, new string[0]));
            var spec2 = new StringHierarchySpec((secondRegionSet, new string[0]));
            var result = spec1.RegionSetSharesPath(spec2, out var sharedRegionSet);
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedSharedRegionSet, sharedRegionSet);
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
        /// Test the Equals method. Uses null as "other".
        /// </summary>
        [Fact]
        public void Equals_NullOther_ShouldReturnFalse()
        {
            var spec1 = new StringHierarchySpec(new[] { "RegionA", "RegionB" }, new[] { "Modifier1" });
            StringHierarchySpec? spec2 = null;
            Assert.False(spec1.Equals(spec2));
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
        /// Test the GetHashCode method with modifier edge cases: no-modifier specs
        /// and modifier-order independence.
        /// </summary>
        [Theory]
        // Same region, both with no modifiers: equal hash
        [InlineData(new[] { "region1" }, new string[0],
                    new[] { "region1" }, new string[0],
                    true)]
        // Same region, one has modifiers the other does not: different hash
        [InlineData(new[] { "region1" }, new[] { "modifier1" },
                    new[] { "region1" }, new string[0],
                    false)]
        // Different region, same modifiers: different hash
        [InlineData(new[] { "region1" }, new[] { "modifier1" },
                    new[] { "region2" }, new[] { "modifier1" },
                    false)]
        public void GetHashCode_ModifierEdgeCases_ShouldReturnConsistentHashCodes(
            string[] regionSet1, string[] modifierSet1,
            string[] regionSet2, string[] modifierSet2,
            bool expectEqualHashCodes)
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
