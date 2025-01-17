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

    public override string ToString()
    {
        return $"{this.Name}: {base.ToString()}";
    }
    #endregion IArea Implementation
}

