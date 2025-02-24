using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using EStimLibrary.Core;
using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Core.Data;
using EStimLibrary.Core.Stimulation.Stimulators;

namespace EStimLibrary.UnitTests.Core
{
    /// <summary>
    /// Comprehensive unit tests for the <see cref="ModelManager"/> class.
    /// These tests verify the functionality related to adding and retrieving body models,
    /// saving and retrieving locations and areas, and localizing by location/area.
    /// </summary>
    public class ModelManagerTests
    {
        #region TryAddBodyModel Tests

        [Fact]
        public void TryAddBodyModel_WithValidBodyModel_ShouldReturnTrueAndAssignDefaultKey()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "TestModel" };

            // Act
            bool result = manager.TryAddBodyModel(bodyModel, out string key);

            // Assert
            Assert.True(result);
            Assert.Equal("TestModel", key);
        }

        [Fact]
        public void TryAddBodyModel_WithOverrideKey_ShouldReturnTrueAndAssignOverrideKey()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "TestModel" };

            // Act
            bool result = manager.TryAddBodyModel(bodyModel, out string key, "OverrideKey");

            // Assert
            Assert.True(result);
            Assert.Equal("OverrideKey", key);
        }

        #endregion

        #region _TryGetBodyModel Tests

        [Fact]
        public void TryGetBodyModel_WithValidKey_ShouldReturnTrueAndCorrectBodyModel()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "Model1" };
            manager.TryAddBodyModel(bodyModel, out string key);

            // Act
            bool result = manager._TryGetBodyModel(key, out IBodyModel retrieved);

            // Assert
            Assert.True(result);
            Assert.Equal(bodyModel, retrieved);
        }

        [Fact]
        public void TryGetBodyModel_WithInvalidKey_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var manager = new ModelManager();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => manager._TryGetBodyModel("NonExistent", out var _));
        }

        #endregion

        #region IsLocationInModel & IsAreaInModel Tests

        [Fact]
        public void IsLocationInModel_WhenAtLeastOneModelContainsLocation_ShouldReturnTrue()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "Model1", ReturnTrueForLocation = true };
            manager.TryAddBodyModel(bodyModel, out string key);
            ILocation location = new DummyLocation();

            // Act
            bool result = manager.IsLocationInModel(location);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsLocationInModel_WhenNoModelContainsLocation_ShouldReturnFalse()
        {
            // Arrange
            var manager = new ModelManager();
            var failingModel = new FailingBodyModel { Name = "FailModel" };
            manager.TryAddBodyModel(failingModel, out string key);
            ILocation location = new DummyLocation();

            // Act
            bool result = manager.IsLocationInModel(location);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsAreaInModel_WhenAtLeastOneModelContainsArea_ShouldReturnTrue()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "Model1", ReturnTrueForArea = true };
            manager.TryAddBodyModel(bodyModel, out string key);
            IArea area = new DummyArea();

            // Act
            bool result = manager.IsAreaInModel(area);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsAreaInModel_WhenNoModelContainsArea_ShouldReturnFalse()
        {
            // Arrange
            var manager = new ModelManager();
            var failingModel = new FailingBodyModel { Name = "FailModel" };
            manager.TryAddBodyModel(failingModel, out string key);
            IArea area = new DummyArea();

            // Act
            bool result = manager.IsAreaInModel(area);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region TrySaveLocation and TrySaveArea Tests

        [Fact]
        public void TrySaveLocation_WithValidModelAndLocation_ShouldReturnTrueAndGlobalId()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "Model1" };
            manager.TryAddBodyModel(bodyModel, out string key);
            ILocation location = new DummyLocation();

            // Act
            bool result = manager.TrySaveLocation(key, location, out int globalId);

            // Assert
            Assert.True(result);
            Assert.True(globalId >= 0);
        }

        [Fact]
        public void TrySaveLocation_WithInvalidModelKey_ShouldReturnFalseAndGlobalIdMinusOne()
        {
            // Arrange
            var manager = new ModelManager();
            ILocation location = new DummyLocation();

            // Act
            bool result = manager.TrySaveLocation("InvalidKey", location, out int globalId);

            // Assert
            Assert.False(result);
            Assert.Equal(-1, globalId);
        }

        [Fact]
        public void TrySaveArea_WithValidModelAndArea_ShouldReturnTrueAndGlobalId()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "Model1" };
            manager.TryAddBodyModel(bodyModel, out string key);
            IArea area = new DummyArea();

            // Act
            bool result = manager.TrySaveArea(key, area, out int areaId);

            // Assert
            Assert.True(result);
            Assert.True(areaId >= 0);
        }

        [Fact]
        public void TrySaveArea_WithInvalidModelKey_ShouldReturnFalseAndAreaIdMinusOne()
        {
            // Arrange
            var manager = new ModelManager();
            IArea area = new DummyArea();

            // Act
            bool result = manager.TrySaveArea("InvalidKey", area, out int areaId);

            // Assert
            Assert.False(result);
            Assert.Equal(-1, areaId);
        }

        #endregion

        #region TryRetrieveLocation and TryRetrieveArea Tests

        [Fact]
        public void TryRetrieveLocation_AfterSaving_ShouldReturnTrueAndCorrectLocation()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "Model1" };
            manager.TryAddBodyModel(bodyModel, out string key);
            ILocation location = new DummyLocation();
            manager.TrySaveLocation(key, location, out int globalId);

            // Act
            bool result = manager.TryRetrieveLocation(globalId, out string retrievedKey, out ILocation retrievedLocation);

            // Assert
            Assert.True(result);
            Assert.Equal(key, retrievedKey);
            Assert.NotNull(retrievedLocation);
        }

        [Fact]
        public void TryRetrieveLocation_WithInvalidGlobalId_ShouldReturnFalse()
        {
            // Arrange
            var manager = new ModelManager();

            // Act
            bool result = manager.TryRetrieveLocation(9999, out string retrievedKey, out ILocation retrievedLocation);

            // Assert
            Assert.False(result);
            Assert.Equal("", retrievedKey);
            Assert.Null(retrievedLocation);
        }

        [Fact]
        public void TryRetrieveArea_AfterSaving_ShouldReturnTrueAndCorrectArea()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel { Name = "Model1" };
            manager.TryAddBodyModel(bodyModel, out string key);
            IArea area = new DummyArea();
            manager.TrySaveArea(key, area, out int areaId);

            // Act
            bool result = manager.TryRetrieveArea(areaId, out string retrievedKey, out IArea retrievedArea);

            // Assert
            Assert.True(result);
            Assert.Equal(key, retrievedKey);
            Assert.NotNull(retrievedArea);
        }

        [Fact]
        public void TryRetrieveArea_WithInvalidGlobalId_ShouldReturnFalse()
        {
            // Arrange
            var manager = new ModelManager();

            // Act
            bool result = manager.TryRetrieveArea(9999, out string retrievedKey, out IArea retrievedArea);

            // Assert
            Assert.False(result);
            Assert.Equal("", retrievedKey);
            Assert.Null(retrievedArea);
        }

        #endregion

        #region TryLocalizeByLocation and TryLocalizeByArea Tests

        [Fact]
        public void TryLocalizeByLocation_WithValidLocation_ShouldReturnTrueAndLocalizationData()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel
            {
                Name = "Model1",
                LocalizationForLocationFully = new List<int> { 100 },
                LocalizationForLocationPartially = new List<int> { 200 }
            };
            manager.TryAddBodyModel(bodyModel, out string key);
            ILocation location = new DummyLocation();

            // Act
            bool result = manager.TryLocalizeByLocation(key, location, out LocalizationData localizationData);

            // Assert
            Assert.True(result);
            Assert.NotNull(localizationData);
            Assert.Contains(100, localizationData.AreasFullyContaining);
            Assert.Contains(200, localizationData.AreasPartiallyContaining);
        }

        [Fact]
        public void TryLocalizeByLocation_WithInvalidModelKey_ShouldReturnFalse()
        {
            // Arrange
            var manager = new ModelManager();
            ILocation location = new DummyLocation();

            // Act
            bool result = manager.TryLocalizeByLocation("InvalidKey", location, out LocalizationData localizationData);

            // Assert
            Assert.False(result);
            Assert.Null(localizationData);
        }

        [Fact]
        public void TryLocalizeByArea_WithValidArea_ShouldReturnTrueAndLocalizationData()
        {
            // Arrange
            var manager = new ModelManager();
            var bodyModel = new DummyBodyModel
            {
                Name = "Model1",
                LocalizationForAreaFully = new List<int> { 300 },
                LocalizationForAreaPartially = new List<int> { 400 }
            };
            manager.TryAddBodyModel(bodyModel, out string key);
            IArea area = new DummyArea();

            // Act
            bool result = manager.TryLocalizeByArea(key, area, out LocalizationData localizationData);

            // Assert
            Assert.True(result);
            Assert.NotNull(localizationData);
            Assert.Contains(300, localizationData.AreasFullyContaining);
            Assert.Contains(400, localizationData.AreasPartiallyContaining);
        }

        [Fact]
        public void TryLocalizeByArea_WithInvalidModelKey_ShouldReturnFalse()
        {
            // Arrange
            var manager = new ModelManager();
            IArea area = new DummyArea();

            // Act
            bool result = manager.TryLocalizeByArea("InvalidKey", area, out LocalizationData localizationData);

            // Assert
            Assert.False(result);
            Assert.Null(localizationData);
        }

        #endregion
    }

    #region Dummy Implementations for Testing

    // We use the production interfaces ILocation, IArea, and LocalizationData.
    // Ensure that the using directives reference EStimLibrary.Core.SpatialModel.

    public class DummyLocation : ILocation
    {
        public string Name => "DummyLocation";
        public bool IsLocationCompatible(ILocation location) => true;
    }

    public class DummyArea : IArea
    {
        public string Name => "DummyArea";
        public bool IsLocationCompatible(ILocation location) => true;
        public bool IsAreaCompatible(IArea area) => true;
        public bool ContainsLocation(ILocation location) => true;
        public bool TryGetOverlap(IArea area, out IArea overlappingArea, out bool fullyContainsArea)
        {
            overlappingArea = this;
            fullyContainsArea = true;
            return true;
        }
    }

    /// <summary>
    /// A dummy implementation of IBodyModel that simulates successful operations.
    /// </summary>
    public class DummyBodyModel : IBodyModel
    {
        public string Name { get; set; }

        public IFactory<ILocation> LocationFactory { get; init; } = null;
        public IFactory<IArea> AreaFactory { get; init; } = null;

        public IDataLimits LocationLimits { get; init; } = null;
        public IDataLimits AreaLimits { get; init; } = null;

        public bool IsLocationTypeCompatible(Type locationType) => true;
        public bool IsAreaTypeCompatible(Type areaType) => true;

        public bool IsLocationInModel(ILocation location) => ReturnTrueForLocation;
        public bool IsAreaInModel(IArea area) => ReturnTrueForArea;

        public Dictionary<int, ILocation> SavedLocations { get; } = new Dictionary<int, ILocation>();
        public Dictionary<int, IArea> SavedAreas { get; } = new Dictionary<int, IArea>();

        public bool ReturnTrueForLocation { get; set; } = false;
        public bool ReturnTrueForArea { get; set; } = false;

        public List<int> LocalizationForLocationFully { get; set; } = new List<int> { 0 };
        public List<int> LocalizationForLocationPartially { get; set; } = new List<int> { 0 };
        public List<int> LocalizationForAreaFully { get; set; } = new List<int> { 0 };
        public List<int> LocalizationForAreaPartially { get; set; } = new List<int> { 0 };

        public bool TrySaveLocation(ILocation location, out int localLocationId, out bool isNewLocationId)
        {
            localLocationId = 1;
            isNewLocationId = true;
            SavedLocations[localLocationId] = location;
            return true;
        }

        public bool TrySaveArea(IArea area, out int localAreaId, out bool isNewAreaId)
        {
            localAreaId = 2;
            isNewAreaId = true;
            SavedAreas[localAreaId] = area;
            return true;
        }

        public bool TryRetrieveLocation(int localLocationId, out ILocation location)
        {
            return SavedLocations.TryGetValue(localLocationId, out location);
        }

        public bool TryRetrieveArea(int localAreaId, out IArea area)
        {
            return SavedAreas.TryGetValue(localAreaId, out area);
        }

        public bool TryFindContainingAreas(ILocation location, out LocalizationData localContainingAreaIds)
        {
            localContainingAreaIds = new LocalizationData(LocalizationForLocationFully, LocalizationForLocationPartially);
            return true;
        }

        public bool TryFindContainingAreas(IArea area, out LocalizationData localContainingAreaIds)
        {
            localContainingAreaIds = new LocalizationData(LocalizationForAreaFully, LocalizationForAreaPartially);
            return true;
        }
    }

    /// <summary>
    /// A dummy implementation of IBodyModel that simulates failure for all operations.
    /// </summary>
    public class FailingBodyModel : IBodyModel
    {
        public string Name { get; set; }

        public IFactory<ILocation> LocationFactory { get; init; } = null;
        public IFactory<IArea> AreaFactory { get; init; } = null;

        public IDataLimits LocationLimits { get; init; } = null;
        public IDataLimits AreaLimits { get; init; } = null;

        public bool IsLocationTypeCompatible(Type locationType) => false;
        public bool IsAreaTypeCompatible(Type areaType) => false;

        public bool IsLocationInModel(ILocation location) => false;
        public bool IsAreaInModel(IArea area) => false;

        public Dictionary<int, ILocation> SavedLocations { get; } = new Dictionary<int, ILocation>();
        public Dictionary<int, IArea> SavedAreas { get; } = new Dictionary<int, IArea>();

        public bool TrySaveLocation(ILocation location, out int localLocationId, out bool isNewLocationId)
        {
            localLocationId = -1;
            isNewLocationId = false;
            return false;
        }

        public bool TrySaveArea(IArea area, out int localAreaId, out bool isNewAreaId)
        {
            localAreaId = -1;
            isNewAreaId = false;
            return false;
        }

        public bool TryRetrieveLocation(int localLocationId, out ILocation location)
        {
            location = null;
            return false;
        }

        public bool TryRetrieveArea(int localAreaId, out IArea area)
        {
            area = null;
            return false;
        }

        public bool TryFindContainingAreas(ILocation location, out LocalizationData localContainingAreaIds)
        {
            localContainingAreaIds = null;
            return false;
        }

        public bool TryFindContainingAreas(IArea area, out LocalizationData localContainingAreaIds)
        {
            localContainingAreaIds = null;
            return false;
        }
    }
}
#endregion