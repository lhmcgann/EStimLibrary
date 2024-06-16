using EStimLibrary.Core;
using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;


public class StringHierarchyBodyModel : IBodyModel
{
    public StringHierarchyRegion BaseRegion { get; init; }
    //public string Name => this.BaseRegion.BaseName; // IBodyModel > ISelectable.
    public string Name { get; init; }
    private ResourceManager<ILocation> _savedLocations;
    private ResourceManager<IArea> _savedAreas;

    /// <summary>
    /// Create a string hierarchy body model, starting from a base region.
    /// </summary>
    /// <param name="baseRegion">The region node that is the root of the nodal
    /// graph for this body model. Shallow copy saved.</param>
    public StringHierarchyBodyModel(StringHierarchyRegion baseRegion,
        string modelName)
    {
        // Intentional pass-by-reference.
        this.BaseRegion = baseRegion;
        // Store the name of the body model which may be different than the
        // region base name.
        this.Name = modelName;
        // Init private resource pools for saved locations and areas.
        this._savedLocations = new(initialNumResourceIds: 10);
        this._savedAreas = new(initialNumResourceIds: 10);
        // Init Factory.
        //this._specFactory = new(this.BaseRegion);
        this.LocationFactory =
            new StringHierarchyLocationFactory(this.BaseRegion);
        this.AreaFactory = new StringHierarchyAreaFactory(this.BaseRegion);
    }

    #region Factories
    // TODO: Is there a better way using generics to do these factories since
    // they are the exact same thing?
    //private readonly StringHierarchySpecFactory _specFactory;
    //public IFactory<ILocation> LocationFactory => (IFactory<ILocation>)
    //    this._specFactory;
    //public IFactory<IArea> AreaFactory => (IFactory<IArea>)this._specFactory;
    // new StringHierarchySpecFactory<StringHierarchyLocation>(this.BaseRegion);
    public IFactory<ILocation> LocationFactory { get; init; }
    public IFactory<IArea> AreaFactory { get; init; }
    // Limits and validation checks on whole, pre-existing location and area
    // value objects.
    // TODO: delete from IBodyModel and other implementing classes
    public IDataLimits LocationLimits => null;
    public IDataLimits AreaLimits => null;
    #endregion

    #region Basic Validation Methods
    public bool IsLocationTypeCompatible(Type locationType)
    {
        return typeof(StringHierarchyLocation).IsAssignableFrom(locationType);
    }

    public bool IsAreaTypeCompatible(Type areaType)
    {
        return typeof(StringHierarchyArea).IsAssignableFrom(areaType);
    }

    protected bool _IsLocationValid(ILocation location)
    {
        // TODO: exceptions or bool return or something else?
        return location is not null &&
            this.IsLocationTypeCompatible(location.GetType());
        //// Run-time type checks.
        //if (location is null)
        //{
        //    throw new ArgumentNullException(
        //        $"StringHierarchyBodyModel._IsLocationValid: location is " +
        //        $"null.");
        //}
        //if (!this.IsLocationTypeCompatible(location.GetType()))
        //{
        //    throw new ArgumentException(
        //        $"StringHierarchyBodyModel._IsLocationValid: location is of" +
        //        $"incorrect type: {location.GetType()}.");
        //}
    }

    protected bool _IsAreaValid(IArea area)
    {
        // TODO: exceptions or bool return or something else?
        return area is not null &&
            this.IsAreaTypeCompatible(area.GetType());
        //// Run-time type checks.
        //if (area is null)
        //{
        //    throw new ArgumentNullException(
        //        $"StringHierarchyBodyModel._IsAreaValid: area is " +
        //        $"null.");
        //}
        //if (!this.IsAreaTypeCompatible(area.GetType()))
        //{
        //    throw new ArgumentException(
        //        $"StringHierarchyBodyModel._IsAreaValid: area is of" +
        //        $"incorrect type: {area.GetType()}.");
        //}
    }

    public bool IsLocationInModel(ILocation location)
    {
        return this._IsLocationValid(location) &&
            this._IsPathSpecInModel((StringHierarchySpec)location, out _);
    }

    public bool IsAreaInModel(IArea area)
    {
        return this._IsAreaValid(area) &&
            this._IsPathSpecInModel((StringHierarchySpec)area, out _);
    }

    protected bool _IsPathSpecInModel(StringHierarchySpec pathSpec,
        out StringHierarchyRegion region)
    {
        // First try to navigate this model to the specified region.
        return this.BaseRegion.TryGetSubregion(pathSpec.RegionSpec,
            out region) &&
        // Then check if the specified modifiers are valid in that region.
            region.IsValidModifierSpec(pathSpec.ModifierSpec);
    }
    #endregion

    #region Saved Location and Area Management and Use
    public Dictionary<int, ILocation> SavedLocations =>
        this._savedLocations.Resources;
    public Dictionary<int, IArea> SavedAreas => this._savedAreas.Resources;

    public bool TrySaveLocation(ILocation location,
        out int localLocationId, out bool isNewLocationId)
    {
        // Assume location doesn't already exist. Change if found.
        isNewLocationId = true;

        // Fail early if: a) can't get new ID, b) location invalid or c) not in
        // this model.
        if (!this._savedLocations.TryGetNextAvailableId(out localLocationId) ||
            !this._IsLocationValid(location) ||
            !this._IsPathSpecInModel((StringHierarchySpec)location,
                out var region))
        {
            return false;
        }

        // Check if the location is already saved. Get the int ID if so.
        foreach (var savedId in region.SavedLocations)
        {
            // Get the location itself.
            this._savedLocations.TryGetResource(savedId, out var savedLocation);
            // If location already saved, reuse.
            if (location.Equals(savedLocation))
            {
                isNewLocationId = false;
                localLocationId = savedId;
                break;  // Don't look at any more locations.
            }
        }
        // If location not already saved, add with new ID.
        if (isNewLocationId)
        {
            // Add actual location spec to the recource pool.
            this._savedLocations.TryAddResource(localLocationId, location);
            // Store the local loc ID in the region. Only used in this method.
            region.SavedLocations.Add(localLocationId);
        }

        // Success in either case.
        return true;
    }

    public bool TrySaveArea(IArea area, out int localAreaId,
        out bool isNewAreaId)
    {
        // Assume area doesn't already exist. Change if found.
        isNewAreaId = true;

        // Fail early if: a) can't get new ID, b) area invalid or c) not in
        // this model.
        if (!this._savedAreas.TryGetNextAvailableId(out localAreaId) ||
            !this._IsAreaValid(area) ||
            !this._IsPathSpecInModel((StringHierarchySpec)area,
                out var region))
        {
            return false;
        }

        // Check if the area is already saved. Get the int ID if so.
        foreach (var savedId in region.SavedAreas)
        {
            // Get the area itself.
            this._savedAreas.TryGetResource(savedId, out var savedArea);
            // If area already saved, reuse.
            if (area.Equals(savedArea))
            {
                isNewAreaId = false;
                localAreaId = savedId;
                break;  // Don't look at any more areas.
            }
        }
        // If area not already saved, add with new ID.
        if (isNewAreaId)
        {
            // Add actual area spec to the recource pool.
            this._savedAreas.TryAddResource(localAreaId, area);
            // Store the local area ID in the region. Only used in this method.
            region.SavedAreas.Add(localAreaId);
        }

        // Success in either case.
        return true;
    }

    public bool TryRetrieveLocation(int localLocationId,
        out ILocation location)
    {
        return this.SavedLocations.TryGetValue(localLocationId, out location);
    }

    public bool TryRetrieveArea(int localAreaId, out IArea area)
    {
        return this.SavedAreas.TryGetValue(localAreaId, out area);
    }

    public bool TryFindContainingAreas(ILocation location,
        out LocalizationData localContainingAreaIds)
    {
        // Set default output to empty.
        localContainingAreaIds = new(new List<int>(), new List<int>());

        // Try to find containing areas. Use general path spec search.
        return this._IsLocationValid(location) &&
            this._TryFindContainingAreas((StringHierarchySpec)location,
            out localContainingAreaIds);
    }

    public bool TryFindContainingAreas(IArea area,
        out LocalizationData localContainingAreaIds)
    {
        // Set default output to empty.
        localContainingAreaIds = new(new List<int>(), new List<int>());

        // Try to find containing areas. Use general path spec search.
        return this._IsAreaValid(area) &&
            this._TryFindContainingAreas((StringHierarchySpec)area,
            out localContainingAreaIds);
    }

    // TODO: TEST THE HECK OUT OF THIS
    protected bool _TryFindContainingAreas(StringHierarchySpec pathSpec,
        out LocalizationData localContainingAreaIds)
    {
        // Return early with empty data if area not valid in this model.
        if (!this._IsPathSpecInModel(pathSpec,
            out StringHierarchyRegion region))
        {
            localContainingAreaIds = new(new List<int>(), new List<int>());
            return false;
        }

        // Lists to store the IDs of the saved areas fully and partially
        // containing the given test area.
        List<int> fullyContainingAreaIds = new();
        List<int> partiallyContainingAreaIds = new();

        var currentRegion = region;
        while (currentRegion is not null)
        {
            // Look through all the saved areas at this region.
            foreach (var areaId in currentRegion.SavedAreas)
            {
                // Get the saved area path spec.
                var savedArea = (StringHierarchySpec)this.SavedAreas[areaId];
                // See if the saved area contains or at least overlaps the area.
                bool overlaps = savedArea.TryGetOverlap(pathSpec, out _,
                    out bool contains);
                // Add to the according list, if any.
                if (contains)
                {
                    fullyContainingAreaIds.Add(areaId);
                }
                else if (overlaps)
                {
                    partiallyContainingAreaIds.Add(areaId);
                }
            }
            // Move up to the parent region.
            currentRegion = currentRegion.ParentRegion;
        }

        // Store output data.
        localContainingAreaIds = new LocalizationData(fullyContainingAreaIds,
            partiallyContainingAreaIds);
        return true;
    }
    #endregion
}

