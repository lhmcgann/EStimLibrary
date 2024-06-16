namespace EStimLibrary.Core.SpatialModel;

public struct SpatialId
{
    /// <summary>
    /// The string keying the body model this ID'd spatial element belongs to.
    /// </summary>
    public string BodyModelKey;
    /// <summary>
    /// The int ID local to the keyed body model and assigned to this ID'd
    /// spatial element when the element is saved within the model.
    /// </summary>
    public int LocalId;
}

