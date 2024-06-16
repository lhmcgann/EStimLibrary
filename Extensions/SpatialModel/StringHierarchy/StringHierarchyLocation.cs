using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;


public record StringHierarchyLocation : StringHierarchySpec, ILocation
{
    public StringHierarchyLocation(string[] regionSpec, string[] modifiers) :
        base(regionSpec, modifiers)
    {
    }
    public StringHierarchyLocation(string fullModifiedRegion) :
        base(fullModifiedRegion)
    {
    }

    #region ILocation Implementation
    public string Name => "StringHierarchyLocation";   // ISelectable

    public bool IsLocationCompatible(ILocation location)
    {
        return typeof(StringHierarchyLocation).IsAssignableFrom(
            location.GetType());
    }

    // the Equals method is overridden by the inherent StringHierarchySpec
    // Equals method; a method inherent to any class/object --> just make sure
    // it's value-based! e.g., implementation type is a record, or overrides the
    // Equals() method
    #endregion ILocation Methods

    public override string ToString()
    {
        return $"{this.Name}: {base.ToString()}";
    }
}

