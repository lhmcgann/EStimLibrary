using EStimLibrary.Core;


namespace EStimLibrary.Core.SpatialModel;

//public interface IBodyModel<ILocation, IArea, BodyModelType> :
//    ISelectable
//    where ILocation : ILocation<ILocation>
//    where IArea : IArea<ILocation, IArea>
//    where BodyModelType : IBodyModel<ILocation, IArea, BodyModelType>


public interface IBodyModel : ISelectable//, IFactory<ILocation>, IFactory<IArea>
{
    #region Basic Validation Properties and Methods
    // Factories for assisted creation of locations and areas valid within
    // this model.
    public IFactory<ILocation> LocationFactory { get; init; }
    public IFactory<IArea> AreaFactory { get; init; }

    // Limits and validation checks on whole, pre-existing location and area
    // value objects.
    public IDataLimits LocationLimits { get; }
    public IDataLimits AreaLimits { get; }
    // Methods for derived type checks ONLY!
    bool IsLocationTypeCompatible(Type locationType);
    bool IsAreaTypeCompatible(Type areaType);
    // Methods for actual object value checks. Should return true if the value
    // is not null, of a compatible type (per methods above), and within the
    // data limits.
    bool IsLocationInModel(ILocation location);
    bool IsAreaInModel(IArea area);
    #endregion

    #region Localization-Related Methods
    /// <summary>
    /// A set of specific saved locations, only added to by TrySaveLocation().
    /// </summary>
    public Dictionary<int, ILocation> SavedLocations { get; }
    /// <summary>
    /// A set of specific saved areas, only added to by TrySaveArea().
    /// </summary>
    public Dictionary<int, IArea> SavedAreas { get; }
    /// <summary>
    /// Try to save a specific location within this model.
    /// </summary>
    /// <param name="location">The location to save.</param>
    /// <param name="localLocationId">An ID designated to the location if
    /// saved successfully. The ID can be used to lookup the location within
    /// this body model. It may be the ID of a location already saved if the
    /// given location is an exact match.</param>
    /// <param name="isNewLocationId">True if the location was not already
    /// saved and thus a new ID was created for it. False if location
    /// already saved and the output locationId is not new.</param>
    /// <returns>True if successfully saved or pre-existing, False if not
    /// (e.g., location invalid).</returns>
    bool TrySaveLocation(ILocation location, out int localLocationId,
        out bool isNewLocationId);
    /// <summary>
    /// Try to save a specific area within this model.
    /// </summary>
    /// <param name="area">The area to save.</param>
    /// <param name="localAreaId">An ID designated to the area if saved
    /// successfully. The ID can be used to lookup the area within this body
    /// model. It may be the ID of an area already saved if the given area
    /// is an exact match.</param>
    /// <param name="isNewAreaId">True if the area was not already saved and
    /// thus a new ID was created for it. False if area already saved and
    /// the output areaId is not new.</param>
    /// <returns>True if successfully saved or pre-existing, False if not
    /// (e.g., area invalid).</returns>
    bool TrySaveArea(IArea area, out int localAreaId, out bool isNewAreaId);
    /// <summary>
    /// Try to retrieve a previously saved location from this model.
    /// </summary>
    /// <param name="localLocationId">The ID of the location.</param>
    /// <param name="location">The location itself if found.</param>
    /// <returns>True if the requested location was found as previously
    /// saved, False if not (e.g., ID not found).</returns>
    bool TryRetrieveLocation(int localLocationId, out ILocation location);
    /// <summary>
    /// Try to retrieve a previously saved area from this model.
    /// </summary>
    /// <param name="localAreaId">The ID of the area.</param>
    /// <param name="area">The area itself if found.</param>
    /// <returns>True if the requested area was found as previously
    /// saved, False if not (e.g., ID not found).</returns>
    bool TryRetrieveArea(int localAreaId, out IArea area);
    /// <summary>
    /// Get all saved areas in this model that contain the given location.
    /// </summary>
    /// <param name="location">The location to query.</param>
    /// <param name="localContainingAreaIds">A LocalizationData record
    /// containing IDs of all saved areas completely and partially containing
    /// the given location.</param>
    /// <returns>True if the the queried location could be localized, False 
    /// if not (e.g., invalid area or no containing areas found).</returns>
    bool TryFindContainingAreas(ILocation location,
        out LocalizationData localContainingAreaIds);
    /// <summary>
    /// Get all saved areas in this model that contain the given area.
    /// </summary>
    /// <param name="area">The area to query.</param>
    /// <param name="localContainingAreaIds">A LocalizationData record
    /// containing IDs of all saved areas completely and partially containing
    /// the given area.</param>
    /// <returns>True if the the queried area could be localized, False if
    /// not (e.g., invalid area or no containing areas found).</returns>
    bool TryFindContainingAreas(IArea area,
        out LocalizationData localContainingAreaIds);
    #endregion
}


//public interface IBodyModel<T> where T : ISpatialDataType
//{
//    bool IsLocationInModel(ILocation<T> location);
//    bool IsAreaInModel(IArea<T> area);
//}

//public interface IBodyModel<T, TSpatialReferenceFrame>
//    where T : ISpatialDataType
//    where TSpatialReferenceFrame : ISpatialReferenceFrame<T, ILocation<T>, IArea<T, IArea>>
//{
//    bool IsLocationInModel(ILocation<T> location);
//    bool IsAreaInModel(IArea<T, ILocation<T>> area);
//}

//public interface IBodyModel<TLocation, TArea> : ISelectable // : IArea<TLocation, TArea>
//    where TLocation : ILocation<TLocation>
//    where TArea : IArea<TLocation, TArea>
//{

//    bool IsLocationInModel(TLocation location);
//    bool IsAreaInModel(TArea area);
//}

//// An interface just to alias the full generic name.
//public interface IBodyModel :
//    ISpatialReferenceFrame<ISpatialDataType>.IBodyModel<ISpatialDataType>
//{
//    bool TryRetrieveArea(int areaId, out IArea area);
//    bool TryRetrieveLocation(int locationId, out ILocation location);
//}