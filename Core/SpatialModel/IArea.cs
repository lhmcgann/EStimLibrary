namespace EStimLibrary.Core.SpatialModel;


public interface IArea : ISelectable
{
    bool IsLocationCompatible(ILocation location);
    bool IsAreaCompatible(IArea area);

    bool ContainsLocation(ILocation location);

    bool TryGetOverlap(IArea area,
        out IArea overlappingArea,
        out bool fullyContainsArea);
}

//public interface IArea<LocationType, AreaType> : ISelectable
//    where LocationType : ILocation<LocationType>
//    where AreaType : IArea<LocationType, AreaType>
//{
//    bool ContainsLocation(LocationType location);

//    bool TryGetOverlap(AreaType area,
//        out AreaType overlappingArea,
//        out bool fullyContainsArea);
//}

//public interface IArea<T> where T : ISpatialDataType
//{
//    bool IsLocationWithinArea(ILocation<T> location);

//    IArea<T> GetOverlap(IArea<T> area);
//}

//public interface IArea<TArea, TLocation>
//    where TArea : IArea<TArea, TLocation>
//    where TLocation : ILocation<TLocation>
//{
//    bool IsLocationWithinArea(TLocation location);

//    bool TryGetOverlap(TArea area, out TArea overlappingArea);
//}

//public interface IArea<TLocation, TArea>
//    where TLocation : ILocation<TLocation>
//    where TArea : IArea<TLocation, TArea>
//{
//    bool IsLocationWithinArea(TLocation location);

//    bool TryGetOverlap(TArea area, out TArea overlappingArea);
//}

//// An interface just to alias the full generic name.
//public interface IArea : ISpatialReferenceFrame<ISpatialDataType>.
//    IArea<ISpatialDataType>
//{

//}

//public interface IArea<out T, TArea>
//    where T : ISpatialDataType
//    where TArea : IArea<T, TArea>
//{
//    bool IsLocationWithinArea(ILocation<T> location);

//    bool TryGetOverlap(TArea area, out TArea overlappingArea);
//}