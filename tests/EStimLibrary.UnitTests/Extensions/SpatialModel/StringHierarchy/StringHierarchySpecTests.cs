using Xunit;
using EStimLibrary.Extensions.SpatialModel.StringHierarchy;

namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy
{
    public class StringHierarchySpecTests
    {
        private readonly ITestOutputHelper _output;

        public StringHierarchySpecTests(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
        }

        /// <summary>
        /// Test the default constructor of StringHierarchySpec.
        /// </summary>
        [Fact]
        public void testParseRegionSpec()
        {
            testOutput=ParseRegionSpec("testtesttesttesttesttest");
            Assert.NotNull(testOutput);
        }

        
    }
}
