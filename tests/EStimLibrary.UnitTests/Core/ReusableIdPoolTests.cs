using EStimLibrary.Core;
using Xunit;
using Xunit.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EStimLibrary.UnitTests.Core
{
    // Test class naming convention: LibClassTests
    public class ReusableIdPoolTests
    {
        private readonly ITestOutputHelper _output;

        // Test class constructor creates an output helper so can write console output.
        public ReusableIdPoolTests(ITestOutputHelper testOutputHelper)
        {
            this._output = testOutputHelper;
        }

        // Test method naming convention: LibClassMethodName_ScenarioShouldExpectn

        #region Constructors

        /// <summary>
        /// Test the empty constructor.
        /// </summary>
        [Fact]
        public void EmptyConstructor_ShouldInitEmpty()
        {
            var reusableIdPool = new ReusableIdPool();

            Assert.Equal(0, reusableIdPool.BaseId);
            Assert.Equal(0, reusableIdPool.NumIds);
            Assert.Empty(reusableIdPool.Ids);
            Assert.Equal(0, reusableIdPool.NumUsedIds);
            Assert.Empty(reusableIdPool.UsedIds);
            Assert.Equal(0, reusableIdPool.NumFreeIds);
            Assert.Empty(reusableIdPool.FreeIds);
        }

        /// <summary>
        /// Test the parameterized constructor with different data values.
        /// </summary>
        [Theory]
        // Test expected valid (non-negative) parameter values.
        [InlineData(0, 0, new int[0])]
        [InlineData(0, 1, new int[] { 0 })]
        [InlineData(0, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [InlineData(1, 0, new int[0])]
        [InlineData(1, 1, new int[] { 1 })]
        [InlineData(2, 9, new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
        // Test negative parameter values default to 0 before generating ID set.
        [InlineData(-1, 0, new int[0])]
        [InlineData(-3, 3, new int[] { 0, 1, 2 })]
        [InlineData(0, -1, new int[0])]
        [InlineData(3, -3, new int[0])]
        [InlineData(-5, -4, new int[0])]
        public void Constructor_ShouldInitIdSets(int baseId, int numIds, int[] expectedIds)
        {
            // Init new ReusableIdPool
            var reusableIdPool = new ReusableIdPool(baseId, numIds);

            // Set expected values.
            int expectedBaseId = Math.Max(baseId, 0);
            int expectedNumIds = Math.Max(numIds, 0);
            SortedSet<int> expectedIdSet = new(expectedIds);
            this._output.WriteLine($"\nBase Id: {expectedBaseId}\nNum Ids: {expectedNumIds}");

            // Test ReusableIdPool properties are initialized correctly.
            Assert.Equal(expectedBaseId, reusableIdPool.BaseId);
            Assert.Equal(expectedNumIds, reusableIdPool.NumIds);
            Assert.Equal(expectedIdSet, reusableIdPool.Ids);
            Assert.Equal(0, reusableIdPool.NumUsedIds);
            Assert.Empty(reusableIdPool.UsedIds);
            Assert.Equal(expectedNumIds, reusableIdPool.NumFreeIds);
            Assert.Equal(expectedIdSet, reusableIdPool.FreeIds);
        }

        /// <summary>
        /// Test constructor with maximum integer values.
        /// </summary>
        [Fact]
        public void Constructor_MaxValues_ShouldInitializeCorrectly()
        {
            int baseId = int.MaxValue - 10;
            int numIds = 10;
            var reusableIdPool = new ReusableIdPool(baseId, numIds);

            Assert.Equal(baseId, reusableIdPool.BaseId);
            Assert.Equal(numIds, reusableIdPool.NumIds);
            Assert.Equal(numIds, reusableIdPool.Ids.Count);
            Assert.Contains(baseId + 9, reusableIdPool.Ids);
        }

        /// <summary>
        /// Test constructor with baseId greater than int.MaxValue - numIds.
        /// </summary>
        [Fact]
        public void Constructor_BaseIdExceedsMaxInt_ShouldHandleOverflow()
        {
            int baseId = int.MaxValue - 5;
            int numIds = 10; // Adjusted numIds so baseId + numIds - 1 <= int.MaxValue
            int expectedNumIds = int.MaxValue - baseId + 1;
            var reusableIdPool = new ReusableIdPool(baseId, numIds);

            Assert.Equal(baseId, reusableIdPool.BaseId);
            Assert.Equal(expectedNumIds, reusableIdPool.NumIds);
            Assert.Equal(expectedNumIds, reusableIdPool.Ids.Count);
            Assert.Contains(baseId + expectedNumIds - 1, reusableIdPool.Ids); // Should contain int.MaxValue
        }

        #endregion

        #region IncrementNumIds

        /// <summary>
        /// Test IncrementNumIds adjusts NumIds correctly.
        /// </summary>
        [Theory]
        [InlineData(5, 5, 3, 8)]
        [InlineData(0, 0, 5, 5)]
        public void IncrementNumIds_ShouldIncreaseNumIds(int baseId, int initialNumIds, int increment, int expectedNumIds)
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(baseId, initialNumIds);

            // Act
            int newNumIds = reusableIdPool.IncrementNumIds(increment);

            // Assert
            Assert.Equal(expectedNumIds, newNumIds);
            Assert.Equal(expectedNumIds, reusableIdPool.NumIds);

            // Verify that Ids are updated correctly
            var expectedIds = new SortedSet<int>(Enumerable.Range(Math.Max(baseId, 0), expectedNumIds));
            Assert.Equal(expectedIds, reusableIdPool.Ids);
        }

        /// <summary>
        /// Test IncrementNumIds does not allow negative increments.
        /// </summary>
        [Fact]
        public void IncrementNumIds_MaxNegativeIncrement_ShouldReturn()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 5);

            // Act & Assert
            Assert.Equal(2, reusableIdPool.IncrementNumIds(-3));
        }

        #endregion

        #region ResetNumIds

        /// <summary>
        /// Test ResetNumIds adjusts NumIds correctly.
        /// </summary>
        [Theory]
        [InlineData(5, 4, 3, 3)]
        [InlineData(0, 5, 2, 2)]
        public void ResetNumIds_ShouldAdjustNumIds(int baseId, int initialNumIds, int newNumIds, int expectedNumIds)
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(baseId, initialNumIds);

            // Act
            int resultNumIds = reusableIdPool.ResetNumIds(newNumIds);

            // Assert
            Assert.Equal(expectedNumIds, resultNumIds);
            Assert.Equal(expectedNumIds, reusableIdPool.NumIds);

            // Verify that Ids are updated correctly
            var expectedIds = new SortedSet<int>(Enumerable.Range(Math.Max(baseId, 0), expectedNumIds));
            Assert.Equal(expectedIds, reusableIdPool.Ids);
        }

        /// <summary>
        /// Test ResetNumIds does not allow negative values.
        /// </summary>
        [Fact]
        public void ResetNumIds_MaxNegativeValue_ShouldThrowException()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 5);
            int reset = -3;
            SortedSet<int> expectedNumIds = new SortedSet<int> { };
            // Act & Assert
            Assert.Equal(0, reusableIdPool.ResetNumIds(reset));
            for (int i = 0; i < 5; i++) {
                Assert.Equal(expectedNumIds, reusableIdPool.UsedIds);
            }
            
            //Assert.Throws<ArgumentException>(() => reusableIdPool.ResetNumIds(int.MaxValue-1));
        }

        #endregion

        #region IsValidId

        /// <summary>
        /// Test IsValidId for various IDs.
        /// </summary>
        [Theory]
        [InlineData(5, 5, 5, true)]
        [InlineData(5, 5, 9, true)]
        [InlineData(5, 5, 10, false)]
        [InlineData(0, 0, 0, false)]
        [InlineData(0, 1, 0, true)]
        [InlineData(0, 1, -1, false)]
        public void IsValidId_ShouldReturnExpectedResult(int baseId, int numIds, int idToTest, bool expectedResult)
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(baseId, numIds);

            // Act
            bool isValid = reusableIdPool.IsValidId(idToTest);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        #endregion

        #region TryGetLocalId

        /// <summary>
        /// Test TryGetLocalId returns correct local ID for valid global IDs.
        /// </summary>
        [Fact]
        public void TryGetLocalId_ValidGlobalId_ShouldReturnLocalId()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            int localId;

            // Act & Assert
            Assert.True(reusableIdPool.TryGetLocalId(5, out localId));
            Assert.Equal(0, localId);

            Assert.True(reusableIdPool.TryGetLocalId(8, out localId));
            Assert.Equal(3, localId);
        }

        /// <summary>
        /// Test TryGetLocalId returns false for invalid global IDs.
        /// </summary>
        [Fact]
        public void TryGetLocalId_InvalidGlobalId_ShouldReturnFalse()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            int localId;

            // Act & Assert
            Assert.False(reusableIdPool.TryGetLocalId(4, out localId));
            Assert.Equal(-1, localId);

            Assert.False(reusableIdPool.TryGetLocalId(9, out localId));
            Assert.Equal(4, localId); // Since localId = globalId - BaseId
        }

        #endregion

        #region TryGetGlobalId

        /// <summary>
        /// Test TryGetGlobalId returns correct global ID for valid local IDs.
        /// </summary>
        [Fact]
        public void TryGetGlobalId_ValidLocalId_ShouldReturnGlobalId()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            int globalId;

            // Act & Assert
            Assert.True(reusableIdPool.TryGetGlobalId(0, out globalId));
            Assert.Equal(5, globalId);

            Assert.True(reusableIdPool.TryGetGlobalId(3, out globalId));
            Assert.Equal(8, globalId);
        }

        /// <summary>
        /// Test TryGetGlobalId returns false for invalid local IDs.
        /// </summary>
        [Fact]
        public void TryGetGlobalId_InvalidLocalId_ShouldReturnFalse()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            int globalId;

            // Act & Assert
            Assert.False(reusableIdPool.TryGetGlobalId(-1, out globalId));
            Assert.Equal(4, globalId); // Since globalId = localId + BaseId

            Assert.False(reusableIdPool.TryGetGlobalId(4, out globalId));
            Assert.Equal(9, globalId);
        }

        #endregion

        #region GetGlobalLocalMap

        /// <summary>
        /// Test GetGlobalLocalMap returns correct mapping.
        /// </summary>
        [Fact]
        public void GetGlobalLocalMap_ShouldReturnCorrectMapping()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);

            // Act
            var map = reusableIdPool.GetGlobalLocalMap();

            // Assert
            var expectedMap = new Dictionary<int, int>
                {
                    { 5, 0 },
                    { 6, 1 },
                    { 7, 2 },
                    { 8, 3 }
                };
            Assert.Equal(expectedMap, map);
        }

        #endregion

        #region UseId

        /// <summary>
        /// Test UseId marks ID as used.
        /// </summary>
        [Fact]
        public void UseId_ShouldMarkIdAsUsed()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);

            // Act
            bool result = reusableIdPool.UseId(6);

            // Assert
            Assert.True(result);
            Assert.Contains(6, reusableIdPool.UsedIds);
            Assert.DoesNotContain(6, reusableIdPool.FreeIds);
            Assert.True(reusableIdPool.IsUsed(6));
            Assert.True(reusableIdPool.IsValidId(6));
        }

        /// <summary>
        /// Test UseId with invalid ID throws exception.
        /// </summary>
        [Fact]
        public void UseId_InvalidId_ShouldThrowException()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);

            // Act & Assert
            Assert.False(reusableIdPool.UseId(9));
        }

        /// <summary>
        /// Test UseId with already used ID does not add duplicate.
        /// </summary>
        [Fact]
        public void UseId_AlreadyUsedId_ShouldNotAddDuplicate()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            reusableIdPool.UseId(6);

            // Act
            bool result = reusableIdPool.UseId(6);

            // Assert
            Assert.True(result);
            Assert.Single(reusableIdPool.UsedIds, 6);
        }

        #endregion

        #region FreeId

        /// <summary>
        /// Test FreeId marks ID as free.
        /// </summary>
        [Fact]
        public void FreeId_ShouldMarkIdAsFree()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            reusableIdPool.UseId(6);

            // Act
            bool result = reusableIdPool.FreeId(6);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(6, reusableIdPool.UsedIds);
            Assert.Contains(6, reusableIdPool.FreeIds);
            Assert.False(reusableIdPool.IsUsed(6));
        }

        /// <summary>
        /// Test FreeId with invalid ID returns false.
        /// </summary>
        [Fact]
        public void FreeId_InvalidId_ShouldReturnFalse()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);

            // Act
            bool result = reusableIdPool.FreeId(9);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test FreeId on an ID that is not used returns true.
        /// </summary>
        [Fact]
        public void FreeId_NotUsedId_ShouldReturnTrue()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);

            // Act
            bool result = reusableIdPool.FreeId(6);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(6, reusableIdPool.UsedIds);
            Assert.Contains(6, reusableIdPool.FreeIds);
        }

        #endregion

        #region IsUsed & IsFree

        /// <summary>
        /// Test IsUsed returns correct status.
        /// </summary>
        [Fact]
        public void IsUsed_ShouldReturnCorrectStatus()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            reusableIdPool.UseId(6);

            // Act & Assert
            Assert.True(reusableIdPool.IsUsed(6));
            Assert.False(reusableIdPool.IsUsed(5));
        }

        /// <summary>
        /// Test IsFree returns correct status.
        /// </summary>
        [Fact]
        public void IsFree_ShouldReturnCorrectStatus()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            reusableIdPool.UseId(6);

            // Act & Assert
            Assert.False(reusableIdPool.IsFree(6));
            Assert.True(reusableIdPool.IsFree(5));
        }

        /// <summary>
        /// Test IsUsed with invalid ID returns false.
        /// </summary>
        [Fact]
        public void IsUsed_InvalidId_ShouldReturnFalse()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);

            // Act
            bool isUsed = reusableIdPool.IsUsed(9);

            // Assert
            Assert.False(isUsed);
        }

        /// <summary>
        /// Test IsFree with invalid ID returns false.
        /// </summary>
        [Fact]
        public void IsFree_InvalidId_ShouldReturnFalse()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);

            // Act
            bool isFree = reusableIdPool.IsFree(9);

            // Assert
            Assert.False(isFree);
        }

        #endregion

        #region TryGetNextFreeId

        /// <summary>
        /// Test TryGetNextFreeId returns lowest free ID.
        /// </summary>
        [Fact]
        public void TryGetNextFreeId_ShouldReturnLowestFreeId()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 4);
            reusableIdPool.UseId(5); // Use the first ID

            // Act
            bool result = reusableIdPool.TryGetNextFreeId(out int nextFreeId);

            // Assert
            Assert.True(result);
            Assert.Equal(6, nextFreeId);
        }

        /// <summary>
        /// Test TryGetNextFreeId with no free IDs returns false.
        /// </summary>
        [Fact]
        public void TryGetNextFreeId_NoFreeIds_ShouldReturnFalse()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 2);
            reusableIdPool.UseId(5);
            reusableIdPool.UseId(6);

            // Act
            bool result = reusableIdPool.TryGetNextFreeId(out int nextFreeId);

            // Assert
            Assert.False(result);
            Assert.Equal(-1, nextFreeId);
        }

        #endregion

        #region GetSubset

        /// <summary>
        /// Test GetSubset returns correct subset.
        /// </summary>
        [Fact]
        public void GetSubset_ShouldReturnCorrectSubset()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 5);

            // Act
            var subset = reusableIdPool.GetSubset(6, 2);

            // Assert
            var expectedSubset = new SortedSet<int> { 6, 7 };
            Assert.Equal(expectedSubset, subset);
        }

        /// <summary>
        /// Test GetSubset with invalid startId returns empty set.
        /// </summary>
        [Fact]
        public void GetSubset_InvalidStartId_ShouldReturnEmptySet()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 5);

            // Act
            var subset = reusableIdPool.GetSubset(10, 2);

            // Assert
            Assert.Empty(subset);
        }

        /// <summary>
        /// Test GetSubset with numIds exceeding pool size.
        /// </summary>
        [Fact]
        public void GetSubset_NumIdsExceedingPool_ShouldReturnAvailableIds()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 5);

            // Act
            var subset = reusableIdPool.GetSubset(7, 5);

            // Assert
            var expectedSubset = new SortedSet<int> { 7, 8, 9 };
            Assert.Equal(expectedSubset, subset);
        }

        /// <summary>
        /// Test GetSubset with negative numIds returns empty set.
        /// </summary>
        [Fact]
        public void GetSubset_NegativeNumIds_ShouldReturnEmptySet()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(5, 5);

            // Act
            var subset = reusableIdPool.GetSubset(6, -2);

            // Assert
            Assert.Empty(subset);
        }

        #endregion

        #region Concurrency Tests (If Applicable)

        // Note: Since the original class is not thread-safe, these tests are illustrative.
        // Uncomment if thread safety is implemented.

        /*
        /// <summary>
        /// Test concurrent UseId operations.
        /// </summary>
        [Fact]
        public void UseId_ConcurrentUsage_ShouldHandleCorrectly()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(0, 1000);
            var idsToUse = Enumerable.Range(0, 1000).ToList();

            // Act
            Parallel.ForEach(idsToUse, id =>
            {
                reusableIdPool.UseId(id);
            });

            // Assert
            Assert.Equal(1000, reusableIdPool.NumUsedIds);
            Assert.Empty(reusableIdPool.FreeIds);
        }

        /// <summary>
        /// Test concurrent FreeId operations.
        /// </summary>
        [Fact]
        public void FreeId_ConcurrentUsage_ShouldHandleCorrectly()
        {
            // Arrange
            var reusableIdPool = new ReusableIdPool(0, 1000);
            var idsToUse = Enumerable.Range(0, 1000).ToList();
            foreach (var id in idsToUse)
            {
                reusableIdPool.UseId(id);
            }

            // Act
            Parallel.ForEach(idsToUse, id =>
            {
                reusableIdPool.FreeId(id);
            });

            // Assert
            Assert.Equal(0, reusableIdPool.NumUsedIds);
            Assert.Equal(1000, reusableIdPool.FreeIds.Count);
        }
        */

        #endregion
    }
}
