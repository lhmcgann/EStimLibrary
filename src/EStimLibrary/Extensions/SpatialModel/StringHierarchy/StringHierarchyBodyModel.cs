using System.Security.AccessControl;
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

    protected bool _IsLocationValid(ILocation location)
    {
        // TODO: exceptions or bool return or something else?
        return location is not null &&
            typeof(StringHierarchyLocation).IsAssignableFrom(location.GetType());
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
            typeof(StringHierarchyArea).IsAssignableFrom(area.GetType());
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

    protected bool _IsPathSpecInModel(StringHierarchySpec pathSpec,
        out StringHierarchyRegion region)
    {
        // First try to navigate this model to the specified region.
        return this.BaseRegion.TryGetSubregion(pathSpec.RegionSpec,
            out region) &&
        // Then check if the specified modifiers are valid in that region.
            region.IsValidModifierSpec(pathSpec.ModifierSpec, out _);
    }

    /// <summary>
    /// Determine if a location is contained in an area.
    /// </summary>
    /// <param name="location">The location to check.</param>
    /// <param name="area">The area to check within.</param>
    /// <returns>True if the location is contained within the area. False if not
    /// or if either the location or area is not valid in this body model.
    /// </returns>
    public bool IsLocationInArea(ILocation location, IArea area)
    {
        // Make sure both areas are non-null and valid types. (protected method
        // called next will check if in model).
        if (!_IsLocationValid(location) || !_IsAreaValid(area))
        {
            return false;
        }

        // Find if the specs overlap.
        _TryGetSpecOverlap((StringHierarchySpec)area,
            (StringHierarchySpec)location, out _, out bool aContainsB);

        // Return the output
        return aContainsB;
    }

    /// <summary>
    /// Try to find the overlap between two areas.
    /// </summary>
    /// <param name="areaA">One area.</param>
    /// <param name="areaB">Another area.</param>
    /// <param name="overlappingArea">An output area representing the area of
    /// overlap if found. Null if no overlap found.</param>
    /// <param name="aFullyContainsB">An output boolean indicating if areaA
    /// fully contains areaB.</param>
    /// <returns>T/F if area A and B overlap at all.</returns>
    public bool TryGetOverlap(IArea areaA, IArea areaB,
        out IArea? overlappingArea, out bool aFullyContainsB)
    {
        // Default (fail case) output variable values.
        overlappingArea = null;
        aFullyContainsB = false;

        // Make sure both areas are non-null and valid types. (protected method
        // called next will check if in model).
        if (!_IsAreaValid(areaA) || !_IsAreaValid(areaB))
        {
            return false;
        }

        // Find if the specs overlap.
        bool overlaps = _TryGetSpecOverlap((StringHierarchySpec)areaA,
            (StringHierarchySpec)areaB, out StringHierarchySpec? overlapSpec,
            out aFullyContainsB);

        // Stuff overlapping area output variable if not null.
        if (overlapSpec != null)
        {
            overlappingArea = (StringHierarchyArea)overlapSpec;
        }

        // Return the output
        return overlaps;
    }

    protected bool _TryGetSpecOverlap(StringHierarchySpec specA,
        StringHierarchySpec specB, out StringHierarchySpec? overlapSpec,
        out bool aContainsB)
    {
        // Default (fail case) output param values.
        overlapSpec = null;
        aContainsB = false;

        // Variables for mod mapping dictionaries.
        Dictionary<string, string> modAMapping = new();
        Dictionary<string, string> modBMapping = new();

        // Locate the regions these specs map to.
        bool valid = this.BaseRegion.TryGetSubregion(specA.RegionSpec,
            out var regionA) &&
            // Then check if the specified modifiers are valid in that region.
            regionA.IsValidModifierSpec(specA.ModifierSpec, out modAMapping);
        valid &= this.BaseRegion.TryGetSubregion(specB.RegionSpec,
            out var regionB) &&
            // Then check if the specified modifiers are valid in that region.
            regionB.IsValidModifierSpec(specB.ModifierSpec, out modBMapping);

        // Fail early if a spec is not in this model.
        if (!valid)
        {
            return false;
        }

        // Check if regions overlap (i.e., if a region is a subregion of the
        // other)
        bool regionSetsOverlap = _RegionSetsOverlap(specA, specB,
            out var overlappingRegionSet);

        // Check modifier sets for overlap.
        // Loop through shorter modifier set. If any mod values collide, fail.
        Dictionary<string, string> lessSpecificModMap; // Temp variables to store mod map references.
        Dictionary<string, string> moreSpecificModMap;
        if (modAMapping.Count <= modBMapping.Count)
        {
            lessSpecificModMap = modAMapping;
            moreSpecificModMap = modBMapping;
            // Possible for specA to fully contain specB. Need further analysis.
            aContainsB = true;    // Potential. Changed if mods don't overlap.
        }
        else
        {
            lessSpecificModMap = modBMapping;
            moreSpecificModMap = modAMapping;
            // Impossible for specA to fully contain specB if is more specific (i.e., more modifiers)
        }
        // Check for same-axis modifier value collisions.
        bool modifiersOverlap = true;
        foreach (var (axis, modifier) in lessSpecificModMap)
        {
            bool otherHasMod = moreSpecificModMap.TryGetValue(axis,
                out var otherMod);
            // If both sets have a modifier on that axis & values collide, fail.
            if (otherHasMod && !modifier.Equals(otherMod))
            {
                modifiersOverlap = false;
                break;
            }
            // Else, continue.
        }

        // Contains if all specA's modifiers shared.
        aContainsB &= modifiersOverlap;

        // Determine overall overlap and containment.
        bool overlaps = regionSetsOverlap && modifiersOverlap;

        // Determine overlapping region: take most specific region and mods.
        overlapSpec = overlaps ?
            // Overlapping spec: the most specific sets.
            new StringHierarchySpec(
                overlappingRegionSet,
                moreSpecificModMap.Values.ToArray())
            : null;     // Empty if no overlap.

        return overlaps;
    }

    /// <summary>
    /// Check if two specs' regions spatially overlap and determine the specific
    /// region of overlap.
    /// </summary>
    /// <param name="specA">One StringHierarchySpec with which to check for
    /// overlap.</param>
    /// <param name="specB">Another StringHierarchySpec with which to check for
    /// overlap.</param>
    /// <param name="regionSetOfOverlap">The region of overlap specification as
    /// an ordered array of string region names. Ignore if no overlap found.
    /// </param>
    /// <returns>T/F if an overlapping region was found.</returns>
    protected bool _RegionSetsOverlap(StringHierarchySpec specA,
        StringHierarchySpec specB,
        out string[] regionSetOfOverlap)
    {
        // Overlaps if all elements of shortest sequence match longer sequence.
        var minLength = Math.Min(specA.RegionSet.Length,
            specB.RegionSet.Length);

        // Get the index of the first differing element.
        var endIndex = Enumerable.Range(0, minLength).FirstOrDefault(
            i => !specA.RegionSet[i].Equals(specB.RegionSet[i]),
            minLength); // Idx = min length if no differences found.

        // DIFFERENT: Get spatial region of overlap (more specific region).
        regionSetOfOverlap = (specA.RegionSet.Length >= specB.RegionSet.Length) ?
            specA.RegionSet : specB.RegionSet;

        // Return T/F that any similar elements were found.
        return endIndex > 0;
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
            this._IsPathSpecInModel((StringHierarchySpec)location,
                out var region) &&
            this._TryFindContainingAreas((StringHierarchySpec)location,
                region, out localContainingAreaIds);
    }

    public bool TryFindContainingAreas(IArea area,
        out LocalizationData localContainingAreaIds)
    {
        // Set default output to empty.
        localContainingAreaIds = new(new List<int>(), new List<int>());

        // Try to find containing areas. Use general path spec search.
        return this._IsAreaValid(area) &&
            this._IsPathSpecInModel((StringHierarchySpec)area,
                out var region) &&
            this._TryFindContainingAreas((StringHierarchySpec)area,
                region, out localContainingAreaIds);
    }

    // TODO: TEST THE HECK OUT OF THIS
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pathSpec">Path spec to try finding a container for. Assumes
    /// path spec is valid within this model.</param>
    /// <param name="localContainingAreaIds">The local IDs of the areas fully
    /// and partially containing the given path spec, empty if none found.
    /// </param>
    /// <returns>T/F if any containing areas found.</returns>
    protected bool _TryFindContainingAreas(StringHierarchySpec pathSpec,
        StringHierarchyRegion region,
        out LocalizationData localContainingAreaIds)
    {
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
                bool overlaps = _TryGetSpecOverlap(savedArea,
                    pathSpec, out _, out bool contains);
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

        // Return if there were any containing areas found.
        return (fullyContainingAreaIds.Count + partiallyContainingAreaIds.Count)
            > 0;
    }
    #endregion
}

