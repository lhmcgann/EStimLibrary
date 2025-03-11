using Xunit;
using EStimLibrary.Extensions.SpatialModel.StringHierarchy;
using EStimLibrary.Core.SpatialModel;

namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy
{
    public class StringHierarchyLocationTests
    {
        /// Test the constructor StringHierarchyLocation(string[] regionSpec, string[] modifiers) with expected inputs.
        [Theory]
        [InlineData(new[] { "region1", "region2" }, new[] { "modifier1", "modifier2" })] // Normal case
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "ModifierX" })] // Case sensitivity check
        public void Constructor_WithRegionAndModifierArrays_ShouldInitializeCorrectlyWithExpectedInputs(
            string[] expectedRegions, string[] expectedModifiers)
        {
            var location = new StringHierarchyLocation(expectedRegions, expectedModifiers);
            Assert.Equal(expectedRegions, location.RegionSet);
            Assert.Equal(expectedModifiers, location.ModifierSet);
        }

        /// Test the constructor StringHierarchyLocation(string[] regionSpec, string[] modifiers) with empty input(s).
        [Theory]
        [InlineData(new[] { "region1" }, new string[0])] // Single region, no modifiers
        [InlineData(new string[0], new string[0])] // Both arrays empty
        public void Constructor_WithRegionAndModifierArrays_ShouldInitializeCorrectlyWithEmptyInputs(
            string[] expectedRegions, string[] expectedModifiers)
        {
            var location = new StringHierarchyLocation(expectedRegions, expectedModifiers);
            Assert.Equal(expectedRegions, location.RegionSet);
            Assert.Equal(expectedModifiers, location.ModifierSet);
        }

        /// Test the constructor StringHierarchyLocation(string fullModifiedRegion) with expected inputs.
        [Theory]
        [InlineData("region1, region2 | modifier1, modifier2", new[] { "region1", "region2" }, new[] { "modifier1", "modifier2" })]
        [InlineData("REGIONA, regionB | modifierX", new[] { "regiona", "regionb" }, new[] { "modifierx" })] // Mixed case
        public void Constructor_WithFullModifiedRegion_ShouldInitializeCorrectlyWithExpectedInputs(
            string fullSpec, string[] expectedRegions, string[] expectedModifiers)
        {
            var location = new StringHierarchyLocation(fullSpec);
            Assert.Equal(expectedRegions, location.RegionSet);
            Assert.Equal(expectedModifiers, location.ModifierSet);
        }

        /// Test the constructor StringHierarchyLocation(string fullModifiedRegion) with empty input(s).
        [Theory]
        [InlineData("region1", new[] { "region1" }, new string[0])] // Single region only
        public void Constructor_WithFullModifiedRegion_ShouldInitializeCorrectly(
            string fullSpec, string[] expectedRegions, string[] expectedModifiers)
        {
            var location = new StringHierarchyLocation(fullSpec);
            Assert.Equal(expectedRegions, location.RegionSet);
            Assert.Equal(expectedModifiers, location.ModifierSet);
        }

        /// Test the Name property of StringHierarchyLocation.
        [Fact]
        public void Name_ShouldReturnCorrectValue()
        {
            var location = new StringHierarchyLocation(new[] { "region1" }, new[] { "modifier1" });
            Assert.Equal("StringHierarchyLocation", location.Name);
        }

        /// Test the IsLocationCompatible method.
        [Theory]
        [InlineData(true)] // Should return true when the type is assignable
        public void IsLocationCompatible_ShouldReturnExpectedResult(bool expectedResult)
        {
            var location1 = new StringHierarchyLocation(new[] { "region1" }, new[] { "modifier1" });
            var location2 = new StringHierarchyLocation(new[] { "region2" }, new[] { "modifier2" });

            bool result = location1.IsLocationCompatible(location2);

            Assert.Equal(expectedResult, result);
        }

        /// Test the Equals method for value-based equality.
        [Theory]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1", "Modifier2" },
                    new[] { "RegionA", "RegionB" }, new[] { "Modifier1", "Modifier2" }, true)]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1", "Modifier2" },
                    new[] { "RegionA", "RegionB" }, new[] { "ModifierX" }, false)]
        public void Equals_ShouldReturnExpectedResult(
            string[] regionSet1, string[] modifierSet1, string[] regionSet2, string[] modifierSet2, bool expected)
        {
            var location1 = new StringHierarchyLocation(regionSet1, modifierSet1);
            var location2 = new StringHierarchyLocation(regionSet2, modifierSet2);

            Assert.Equal(expected, location1.Equals(location2));
        }

        /// Test the ToString method.
        [Theory]
        [InlineData(new[] { "RegionA", "RegionB" }, new[] { "Modifier1" }, "StringHierarchyLocation: RegionA, RegionB | Modifier1")]
        [InlineData(new[] { "Main" }, new string[] { }, "StringHierarchyLocation: Main")]
        [InlineData(new[] { "X", "Y", "Z" }, new[] { "North", "South" }, "StringHierarchyLocation: X, Y, Z | North, South")]
        public void ToString_ShouldReturnFullSpecRepresentation(
            string[] regionSet, string[] modifierSet, string expectedFullSpec)
        {
            var location = new StringHierarchyLocation(regionSet, modifierSet);
            var result = location.ToString();
            Assert.Equal(expectedFullSpec, result);
        }
    }
}
