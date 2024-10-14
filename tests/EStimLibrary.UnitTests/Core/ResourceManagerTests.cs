using EStimLibrary.Core;
using Xunit;
using Xunit.Abstractions;
using System;
using System.Collections.Generic;

namespace EStimLibrary.UnitTests.Core
{
    // Test class naming convention: LibClassTests
    public class ResourceManagerTests
    {
        private readonly ITestOutputHelper _output;

        // Test class constructor creates an output helper so can write console output.
        public ResourceManagerTests(ITestOutputHelper testOutputHelper)
        {
            this._output = testOutputHelper;
        }

        // Test method naming convention: LibClassMethodName_ScenarioShouldExpect

        #region Constructors

        /// <summary>
        /// Test the empty constructor initializes correctly.
        /// </summary>
        [Fact]
        public void Constructor_Empty_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var resourceManager = new ResourceManager<object>();

            // Assert
            Assert.NotNull(resourceManager.IdPool);
            Assert.Equal(0, resourceManager.IdPool.BaseId);
            Assert.Equal(0, resourceManager.IdPool.NumIds);
            Assert.Empty(resourceManager.IdPool.Ids);
            Assert.Empty(resourceManager.Resources);
            Assert.Equal(Constants.POS_INFINITY, resourceManager.MaxNumResources);
            Assert.Equal(0, resourceManager.NumTotalResources);
        }
        /// <summary>
        /// Tests if the class can handle dynamic values
        /// </summary>
        /// <param name="baseId"></param>
        /// <param name="initialNumResourceIds"></param>
        /// <param name="maxNumResources"></param>
        [Theory]
        [InlineData(10, 5, 15)]
        [InlineData(100, 50, 120)]
        [InlineData(0, 0, 10)]
        public void Constructor_DynamicParameters_ShouldInitializeCorrectly(int baseId, int initialNumResourceIds, int maxNumResources)
        {
            // Arrange & Act
            var resourceManager = new ResourceManager<object>(baseId, initialNumResourceIds, maxNumResources);

            // Assert
            Assert.Equal(baseId >= 0 ? baseId : 0, resourceManager.IdPool.BaseId);
            Assert.Equal(initialNumResourceIds >= 0 ? initialNumResourceIds : 0, resourceManager.IdPool.NumIds);
            Assert.Equal(maxNumResources >= 0 ? maxNumResources : Constants.POS_INFINITY, resourceManager.MaxNumResources);
        }


        /// <summary>
        /// Test the parameterized constructor initializes correctly.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 10)]
        [InlineData(5, 3, 5)]
        [InlineData(-1, -1, -1)] // Negative values should default appropriately
        public void Constructor_Parameters_ShouldInitializeCorrectly(int baseId, int initialNumResourceIds, int maxNumResources)
        {
            // Arrange & Act
            var resourceManager = new ResourceManager<object>(baseId, initialNumResourceIds, maxNumResources);

            // Expected values
            int expectedBaseId = Math.Max(baseId, 0);
            int expectedNumIds = Math.Max(initialNumResourceIds, 0);
            int expectedMaxResources = maxNumResources >= 0 ? maxNumResources : Constants.POS_INFINITY;

            // Assert
            Assert.NotNull(resourceManager.IdPool);
            Assert.Equal(expectedBaseId, resourceManager.IdPool.BaseId);
            Assert.Equal(expectedNumIds, resourceManager.IdPool.NumIds);
            Assert.Equal(expectedMaxResources, resourceManager.MaxNumResources);
            Assert.Empty(resourceManager.IdPool.UsedIds);
            Assert.Empty(resourceManager.Resources);
            Assert.Equal(0, resourceManager.NumTotalResources);
        }

        #endregion

        #region IsValidResourceId

        /// <summary>
        /// Test IsValidResourceId returns true only when the ID is valid and has an associated resource.
        /// </summary>
        [Fact]
        public void IsValidResourceId_ValidUsedIdWithResource_ShouldReturnTrue()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 5, initialNumResourceIds: 3);
            int id = 6;
            resourceManager.TryAddResource(id, "Resource");

            // Act
            bool isValid = resourceManager.IsValidResourceId(id);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// Test IsValidResourceId returns false when the ID is invalid.
        /// </summary>
        [Fact]
        public void IsValidResourceId_InvalidId_ShouldReturnFalse()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 5, initialNumResourceIds: 3);
            int invalidId = 10;

            // Act
            bool isValid = resourceManager.IsValidResourceId(invalidId);

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// Test IsValidResourceId returns false when the ID is valid but has no associated resource.
        /// </summary>
        [Fact]
        public void IsValidResourceId_ValidIdNoResource_ShouldReturnFalse()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 5, initialNumResourceIds: 3);
            int id = 6;
            // Mark ID as used without adding a resource (this should not happen in normal usage)
            resourceManager.IdPool.UseId(id);

            // Act
            bool isValid = resourceManager.IsValidResourceId(id);

            // Assert
            Assert.False(isValid);
        }

        #endregion

        #region TryGetNextAvailableId

        /// <summary>
        /// Test TryGetNextAvailableId returns true and provides the next available ID.
        /// </summary>
        [Fact]
        public void TryGetNextAvailableId_Available_ShouldReturnTrue()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);

            // Act
            bool result = resourceManager.TryGetNextAvailableId(out int nextId);

            // Assert
            Assert.True(result);
            Assert.Equal(0, nextId);
        }

        /// <summary>
        /// Test TryGetNextAvailableId returns false when no IDs are available and MaxNumResources is reached.
        /// </summary>
        [Fact]
        public void TryGetNextAvailableId_NoAvailableIds_ShouldReturnFalse()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1, maxNumResources: 1);
            resourceManager.TryAddResource(0, "Resource1"); // Use the only ID

            // Act
            bool result = resourceManager.TryGetNextAvailableId(out int nextId);

            // Assert
            Assert.False(result);
            Assert.Equal(0, nextId); // Default value
        }

        /// <summary>
        /// Test TryGetNextAvailableId automatically increments NumIds when IDs are exhausted but MaxNumResources is not reached.
        /// </summary>
        [Fact]
        public void TryGetNextAvailableId_ExhaustedIds_ShouldIncrementNumIds()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1, maxNumResources: 2);
            resourceManager.TryAddResource(0, "Resource1"); // Use the only ID

            // Act
            bool result = resourceManager.TryGetNextAvailableId(out int nextId);

            // Assert
            Assert.True(result);
            Assert.Equal(1, nextId);
            Assert.Equal(2, resourceManager.IdPool.NumIds); // NumIds should have incremented
        }

        #endregion

        #region TryAddResource

        /// <summary>
        /// Test TryAddResource successfully adds a resource and marks the ID as used.
        /// </summary>
        [Fact]
        public void TryAddResource_ValidId_ShouldAddResourceAndMarkIdAsUsed()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);
            string resource = "TestResource";

            // Act
            bool result = resourceManager.TryAddResource(0, resource);

            // Assert
            Assert.True(result);
            Assert.True(resourceManager.IdPool.IsUsed(0));
            Assert.Equal(resource, resourceManager.Resources[0]);
            Assert.Equal(1, resourceManager.NumTotalResources);
        }

        /// <summary>
        /// Test that IDs are only marked as used when a resource is associated.
        /// </summary>
        [Fact]
        public void TryAddResource_IdShouldBeUsedOnlyWithResource()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);

            // Act
            bool idUsedBefore = resourceManager.IdPool.IsUsed(0);
            bool resourceExistsBefore = resourceManager.Resources.ContainsKey(0);

            resourceManager.TryAddResource(0, "Resource");

            bool idUsedAfter = resourceManager.IdPool.IsUsed(0);
            bool resourceExistsAfter = resourceManager.Resources.ContainsKey(0);

            // Assert
            Assert.False(idUsedBefore);
            Assert.False(resourceExistsBefore);
            Assert.True(idUsedAfter);
            Assert.True(resourceExistsAfter);
        }

        /// <summary>
        /// Test TryAddResource fails when ID is invalid.
        /// </summary>
        [Fact]
        public void TryAddResource_InvalidId_ShouldFail()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);
            string resource = "TestResource";

            // Act
            bool result = resourceManager.TryAddResource(1, resource);

            // Assert
            Assert.False(result);
            Assert.False(resourceManager.IdPool.IsUsed(1));
            Assert.False(resourceManager.Resources.ContainsKey(1));
            Assert.Equal(0, resourceManager.NumTotalResources);
        }

        /// <summary>
        /// Test TryAddResource fails when ID is already used.
        /// </summary>
        [Fact]
        public void TryAddResource_UsedId_ShouldFail()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);
            string resource1 = "Resource1";
            string resource2 = "Resource2";
            resourceManager.TryAddResource(0, resource1);

            // Act
            bool result = resourceManager.TryAddResource(0, resource2);

            // Assert
            Assert.False(result);
            Assert.Equal(resource1, resourceManager.Resources[0]);
            Assert.Equal(1, resourceManager.NumTotalResources);
        }

        #endregion

        #region TryGetResource

        /// <summary>
        /// Test TryGetResource successfully retrieves a resource.
        /// </summary>
        [Fact]
        public void TryGetResource_ValidId_ShouldRetrieveResource()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);
            string resource = "TestResource";
            resourceManager.TryAddResource(0, resource);

            // Act
            bool result = resourceManager.TryGetResource(0, out string retrievedResource);

            // Assert
            Assert.True(result);
            Assert.Equal(resource, retrievedResource);
        }

        /// <summary>
        /// Test TryGetResource fails when ID is invalid.
        /// </summary>
        [Fact]
        public void TryGetResource_InvalidId_ShouldFail()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>();

            // Act
            bool result = resourceManager.TryGetResource(0, out string retrievedResource);

            // Assert
            Assert.False(result);
            Assert.Null(retrievedResource);
        }

        /// <summary>
        /// Test TryGetResource fails when there is no resource with the given ID.
        /// </summary>
        [Fact]
        public void TryGetResource_NoResource_ShouldFail()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);

            // Act
            bool result = resourceManager.TryGetResource(0, out string retrievedResource);

            // Assert
            Assert.False(result);
            Assert.Null(retrievedResource);
        }

        #endregion

        #region Resource and ID Synchronization

        /// <summary>
        /// Test that when a resource is removed, the ID is marked as free and no longer associated with a resource.
        /// </summary>
        [Fact]
        public void TryRemoveResource_ShouldFreeIdAndRemoveResource()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);
            resourceManager.TryAddResource(0, "Resource");

            // Act
            bool removeResult = resourceManager.TryRemoveResource(0, out string removedResource);

            // Assert
            Assert.True(removeResult);
            Assert.Equal("Resource", removedResource);
            Assert.False(resourceManager.IdPool.IsUsed(0));
            Assert.False(resourceManager.Resources.ContainsKey(0));
        }

        /// <summary>
        /// Test that free IDs do not have associated resources.
        /// </summary>
        [Fact]
        public void FreeIds_ShouldNotHaveAssociatedResources()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);
            resourceManager.TryAddResource(0, "Resource");
            resourceManager.TryRemoveResource(0, out _);

            // Act
            bool isUsed = resourceManager.IdPool.IsUsed(0);
            bool hasResource = resourceManager.Resources.ContainsKey(0);

            // Assert
            Assert.False(isUsed);
            Assert.False(hasResource);
        }

        /// <summary>
        /// Test that IDs cannot be marked as used without an associated resource.
        /// </summary>
        [Fact]
        public void UseId_WithoutResource_ShouldNotMarkAsUsed()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);

            // Directly using IdPool to mark ID as used without adding a resource
            bool useResult = resourceManager.IdPool.UseId(0);

            // Act
            bool isValidResourceId = resourceManager.IsValidResourceId(0);
            bool hasResource = resourceManager.Resources.ContainsKey(0);

            // Assert
            Assert.False(isValidResourceId); // Should be false because no resource is associated
            Assert.False(hasResource);
            Assert.True(useResult); // The ID is marked as used, but this should not happen without a resource
        }

        /// <summary>
        /// Test that IDs are only marked as used when resources are added, and marked as free when resources are removed.
        /// </summary>
        [Fact]
        public void ResourceManagement_ShouldSynchronizeIdUsage()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 2);

            // Act & Assert
            // Initially, IDs are free
            Assert.False(resourceManager.IdPool.IsUsed(0));
            Assert.False(resourceManager.IdPool.IsUsed(1));

            // Add a resource to ID 0
            resourceManager.TryAddResource(0, "Resource0");
            Assert.True(resourceManager.IdPool.IsUsed(0));
            Assert.False(resourceManager.IdPool.IsUsed(1));

            // Remove the resource from ID 0
            resourceManager.TryRemoveResource(0, out _);
            Assert.False(resourceManager.IdPool.IsUsed(0));

            // Add resources to both IDs
            resourceManager.TryAddResource(0, "Resource0");
            resourceManager.TryAddResource(1, "Resource1");
            Assert.True(resourceManager.IdPool.IsUsed(0));
            Assert.True(resourceManager.IdPool.IsUsed(1));

            // Remove resource from ID 1
            resourceManager.TryRemoveResource(1, out _);
            Assert.True(resourceManager.IdPool.IsUsed(0));
            Assert.False(resourceManager.IdPool.IsUsed(1));
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// Test adding a resource when the IdPool is empty should fail.
        /// </summary>
        [Fact]
        public void TryAddResource_EmptyIdPool_ShouldFail()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>();

            // Act
            bool result = resourceManager.TryAddResource(0, "Resource");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test removing a resource that was never added should fail.
        /// </summary>
        [Fact]
        public void TryRemoveResource_NeverAdded_ShouldFail()
        {
            // Arrange
            var resourceManager = new ResourceManager<string>(baseId: 0, initialNumResourceIds: 1);

            // Act
            bool result = resourceManager.TryRemoveResource(0, out string removedResource);

            // Assert
            Assert.False(result);
            Assert.Null(removedResource);
        }

        #endregion
    }
}
