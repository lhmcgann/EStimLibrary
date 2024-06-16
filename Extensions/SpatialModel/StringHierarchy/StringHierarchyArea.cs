using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;


public record StringHierarchyArea : StringHierarchySpec, IArea
{
    public StringHierarchyArea(string[] regionSpec, string[] modifiers) :
        base(regionSpec, modifiers)
    {
    }
    public StringHierarchyArea(string fullModifiedRegion) :
        base(fullModifiedRegion)
    {
    }

    #region IArea Implementation
    public string Name => "StringHierarchyArea";   // ISelectable

    public bool IsLocationCompatible(ILocation location)
    {
        return typeof(StringHierarchyLocation).IsAssignableFrom(
            location.GetType());
    }

    public bool IsAreaCompatible(IArea area)
    {
        return typeof(StringHierarchyArea).IsAssignableFrom(area.GetType());
    }

    public bool ContainsLocation(ILocation location)
    {
        // Run-time type checks.
        if (location is null)
        {
            throw new ArgumentNullException(
                $"StringHierarchyArea.ContainsLocation: location is null.");
        }
        if (!this.IsLocationCompatible(location))
        {
            throw new ArgumentException(
                $"StringHierarchyArea.ContainsLocation: location is of" +
                $"incorrect type: {location.GetType()}.");
        }

        // Uses the parent implementation of TryGetOverlap.
        bool overlaps = this.TryGetOverlap((StringHierarchySpec)location,
            out _, out bool contains);
        return contains;
    }

    public bool TryGetOverlap(IArea area, out IArea overlappingArea,
        out bool fullyContainsArea)
    {
        // Run-time type checks.
        if (area is null)
        {
            throw new ArgumentNullException(
                $"StringHierarchyArea.TryGetOverlap: other area is null.");
        }
        if (!this.IsAreaCompatible(area))
        {
            throw new ArgumentException(
                $"StringHierarchyArea.TryGetOverlap: other area is of" +
                $"incorrect type: {area.GetType()}.");
        }

        // Uses the parent implementation of TryGetOverlap.
        bool overlaps = this.TryGetOverlap((StringHierarchySpec)area,
            out string overlap, out fullyContainsArea);
        overlappingArea = new StringHierarchyArea(overlap);
        return overlaps;

    }
    #endregion IArea Methods

    public override string ToString()
    {
        return $"{this.Name}: {base.ToString()}";
    }
}

