using Xunit;
using EStimLibrary.Extensions.SpatialModel;

namespace EStimLibrary.UnitTests.Extensions.SpatialModel
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
        public void DefaultConstructor_ShouldInitializeProperties()
        {
            var spec = new StringHierarchySpec();

            Assert.NotNull(spec);
        }

        
    }
}
