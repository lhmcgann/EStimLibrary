namespace EStimLibrary.Core.SpatialModel;


public class ModelManager
{
    internal Dictionary<string, IBodyModel> _BodyModels;
    // Num also used as next available index.
    public int NumModels => this._BodyModels.Count;
    public List<string> BodyModelKeys => this._BodyModels.Keys.ToList();

    protected ReusableIdPool _GlobalLocationIdPool;
    protected Dictionary<int, SpatialId> _GlobalToLocalLocationIds;
    protected Dictionary<SpatialId, int> _LocalToGlobalLocationIds;
    protected ReusableIdPool _GlobalAreaIdPool;
    protected Dictionary<int, SpatialId> _GlobalToLocalAreaIds;
    protected Dictionary<SpatialId, int> _LocalToGlobalAreaIds;

    public ModelManager()
    {
        this._BodyModels = new();

        this._GlobalLocationIdPool = new();
        this._GlobalToLocalLocationIds = new();
        this._LocalToGlobalLocationIds = new();

        this._GlobalAreaIdPool = new();
        this._GlobalToLocalAreaIds = new();
        this._LocalToGlobalAreaIds = new();
    }

    /// <summary>
    /// Try to add a new constructed body model to this model manager.
    /// </summary>
    /// <param name="bodyModel">The body model to add.</param>
    /// <param name="bodyModelKey">An output parameter. The string key used to
    /// identify the model within this manager.</param>
    /// <param name="nonNameBodyModelKey">Optional parameter: the string to key
    /// the new model by if different from the model name which is used by
    /// default.</param>
    /// <returns>True if the model could be added, False if not (e.g., a model
    /// of the same key already exists).
    /// </returns>
    public bool TryAddBodyModel(IBodyModel bodyModel, out string bodyModelKey,
        string nonNameBodyModelKey = "")
    {
        // Output the body model key regardless of successful add or not.
        // If no default-override key is given, use the model name.
        bodyModelKey = (nonNameBodyModelKey == "") ? bodyModel.Name :
            nonNameBodyModelKey;

        // If body model not already stored under the same key, add new model.
        if (!this._BodyModels.Keys.Contains(bodyModelKey))
        {
            // Store in dict of {key: model}
            this._BodyModels.Add(bodyModelKey, bodyModel);
            return true;
        }
        return false;
    }

    protected bool _TryGetBodyModel(string modelKey, out IBodyModel bodyModel)
    {
        return this._BodyModels.TryGetValue(modelKey, out bodyModel);
    }

    // TODO: keep these two methods? if so, leave as are - iterating through all
    // body models - or require additional parameter of model key?
    public bool IsLocationInModel(ILocation location)
    {
        // Check if location is contained in one model.
        foreach (var (modelKey, bodyModel) in this._BodyModels)
        {
            if (bodyModel.IsLocationInModel(location))
            {
                return true;
            }
        }
        // If not found
        return false;
    }

    public bool IsAreaInModel(IArea area)
    {
        // Check if area is contained in one model.
        foreach (var (modelKey, bodyModel) in this._BodyModels)
        {
            if (bodyModel.IsAreaInModel(area))
            {
                return true;
            }
        }
        // If not found
        return false;
    }

    // TODO: have a true ModelManager global list of IDs? or just IDs per body
    // model key? otherwise calling context must keep track of IDs per body
    // model key.

    protected static int _GetNextAvailableId(ReusableIdPool targetIdPool)
    {
        // Get the next available stim ID, or increase the pool max if none.
        int id;
        while (!targetIdPool.TryGetNextFreeId(out id))
        {
            targetIdPool.IncrementNumIds(1);
        }
        return id;
    }

    /// <summary>
    /// Try to save a specific location for ID lookup later.
    /// </summary>
    /// <param name="bodyModelKey">The string keying the specific body model in
    /// which the given location lies.</param>
    /// <param name="location">The location to save.</param>
    /// <param name="globalLocationId">A global ID designated to the location if
    /// saved successfully. The ID can be used to lookup the location within
    /// this model manager. It may be the ID of a location already saved if the
    /// given location is an exact match.</param>
    /// <returns>True if successfully saved or pre-existing, False if not
    /// (e.g., body model key or location invalid).</returns>
    public bool TrySaveLocation(string bodyModelKey, ILocation location,
        out int globalLocationId)
    {
        // Check if valid model key and successful location save.
        if (this._TryGetBodyModel(bodyModelKey, out var bodyModel) &&
            bodyModel.TrySaveLocation(location, out int localLocationId,
            out bool newLocationId))
        {
            // Create the local ID struct.
            var spatialId = new SpatialId
            {
                BodyModelKey = bodyModelKey,
                LocalId = localLocationId
            };

            // If pre-existing location, just return the existing global ID.
            if (!newLocationId)
            {
                globalLocationId = this._LocalToGlobalLocationIds[spatialId];
            }
            // Else, get a new global ID and update internal ID tracking data
            // structures.
            else
            {
                // Get the next available manager-global ID for a new saved
                // location.
                globalLocationId = _GetNextAvailableId(
                    this._GlobalLocationIdPool);
                // Use the new global ID.
                this._GlobalLocationIdPool.UseId(globalLocationId);
                // Store the global ID to local ID mapping, and vice versa.
                this._GlobalToLocalLocationIds.Add(globalLocationId, spatialId);
                this._LocalToGlobalLocationIds.Add(spatialId, globalLocationId);
            }

            // Return valid/success.
            return true;
        }
        else
        {
            // Return failure.
            globalLocationId = -1;
            return false;
        }
    }

    /// <summary>
    /// Try to save a specific area for ID lookup later.
    /// </summary>
    /// <param name="bodyModelKey">The string keying the specific body model in
    /// which the given area lies.</param>
    /// <param name="area">The area to save.</param>
    /// <param name="locationId">A global ID designated to the area if saved
    /// successfully. The ID can be used to lookup the area within this model
    /// manager. It may be the ID of an area already saved if the given area is
    /// an exact match.</param>
    /// <returns>True if successfully saved or pre-existing, False if not
    /// (e.g., area invalid).</returns>
    public bool TrySaveArea(string bodyModelKey, IArea area, out int areaId)
    {
        // Check if valid model key and successful area save.
        if (this._TryGetBodyModel(bodyModelKey, out var bodyModel) &&
            bodyModel.TrySaveArea(area, out int localAreaId,
            out bool newAreaId))
        {
            // Create the local ID struct.
            var spatialId = new SpatialId
            {
                BodyModelKey = bodyModelKey,
                LocalId = localAreaId
            };

            // If pre-existing area, just return the existing global ID.
            if (!newAreaId)
            {
                areaId = this._LocalToGlobalLocationIds[spatialId];
            }
            // Else, get a new global ID and update internal ID tracking data
            // structures.
            else
            {
                // Get the next available manager-global ID for a new saved
                // area.
                areaId = _GetNextAvailableId(this._GlobalAreaIdPool);
                // Use the new global ID.
                this._GlobalAreaIdPool.UseId(areaId);
                // Store the global ID to local ID mapping, and vice versa.
                this._GlobalToLocalAreaIds.Add(areaId, spatialId);
                this._LocalToGlobalAreaIds.Add(spatialId, areaId);
            }

            // Return valid/success.
            return true;
        }
        else
        {
            // Return failure.
            areaId = -1;
            return false;
        }
    }

    public bool TryRetrieveLocation(int globalLocationId,
        out string bodyModelKey, out ILocation location)
    {
        // Check if the global ID is a valid saved ID.
        if (this._GlobalLocationIdPool.IsUsed(globalLocationId))
        {
            // Get the local ID info.
            var localSpatialId =
                this._GlobalToLocalLocationIds[globalLocationId];
            bodyModelKey = localSpatialId.BodyModelKey;
            // Get the body model to search in.
            this._TryGetBodyModel(bodyModelKey, out var bodyModel);
            // Try to retrieve the location.
            return bodyModel.TryRetrieveLocation(localSpatialId.LocalId,
                out location);
        }
        else
        {
            // Return failure.
            bodyModelKey = "";
            location = null;
            return false;
        }
    }

    public bool TryRetrieveArea(int globalAreaId, out string bodyModelKey,
        out IArea area)
    {
        // Check if the global ID is a valid saved ID.
        if (this._GlobalAreaIdPool.IsUsed(globalAreaId))
        {
            // Get the local ID info.
            var localSpatialId =
                this._GlobalToLocalLocationIds[globalAreaId];
            bodyModelKey = localSpatialId.BodyModelKey;
            // Get the body model to search in.
            this._TryGetBodyModel(bodyModelKey, out var bodyModel);
            // Try to retrieve the area.
            return bodyModel.TryRetrieveArea(localSpatialId.LocalId,
                out area);
        }
        else
        {
            // Return failure.
            bodyModelKey = "";
            area = null;
            return false;
        }
    }

    /// <summary>
    /// Try to look up all areas fully or partially containing the given
    /// location.
    /// </summary>
    /// <param name="bodyModelKey">The string key of the specific body model to
    /// look at.</param>
    /// <param name="targetLocation">The location to try localizing.</param>
    /// <param name="localizationData">Contains global IDs of areas fully and
    /// partially containing the given location. Will contain invalid data,
    /// possibly null, if this method returns False.</param>
    /// <returns>True if the location can be localized, False if not (e.g., body
    /// model key or location were invalid).</returns>
    public bool TryLocalizeByLocation(string bodyModelKey,
        ILocation targetLocation, out LocalizationData globalLocalizationData)
    {
        // Check if model key and area are valid. Output localization data if
        // both valid.
        if (this._TryGetBodyModel(bodyModelKey, out var bodyModel) &&
            bodyModel.TryFindContainingAreas(targetLocation,
            out var localLocalizationData))
        {
            // Convert to global IDs.
            var globalFullyContainingIds = _GetGlobalIds(
                this._LocalToGlobalLocationIds, bodyModelKey,
                localLocalizationData.AreasFullyContaining);
            var globalPartiallyContainingIds = _GetGlobalIds(
                this._LocalToGlobalLocationIds, bodyModelKey,
                localLocalizationData.AreasPartiallyContaining);

            // Return success w/ global localization data.
            globalLocalizationData = new(globalFullyContainingIds,
                globalPartiallyContainingIds);
            return true;
        }
        else
        {
            // Return failure.
            globalLocalizationData = null;
            return false;
        }
    }

    /// <summary>
    /// Try to look up all areas fully or partially containing the given area.
    /// </summary>
    /// <param name="bodyModelKey">The string key of the specific body model to
    /// look at.</param>
    /// <param name="targetArea">The area to try localizing.</param>
    /// <param name="globalLocalizationData">Contains global IDs of areas fully
    /// and partially containing the given area. Will contain invalid data,
    /// possibly null, if this method returns False.</param>
    /// <returns>True if the area can be localized, False if not (e.g., body
    /// model key or area were invalid).</returns>
    public bool TryLocalizeByArea(string bodyModelKey, IArea targetArea,
        out LocalizationData globalLocalizationData)
    {
        // Check if model key and area are valid. Output localization data if
        // both valid.
        if (this._TryGetBodyModel(bodyModelKey, out var bodyModel) &&
            bodyModel.TryFindContainingAreas(targetArea,
            out var localLocalizationData))
        {
            // Convert to global IDs.
            var globalFullyContainingIds = _GetGlobalIds(
                this._LocalToGlobalAreaIds, bodyModelKey,
                localLocalizationData.AreasFullyContaining);
            var globalPartiallyContainingIds = _GetGlobalIds(
                this._LocalToGlobalAreaIds, bodyModelKey,
                localLocalizationData.AreasPartiallyContaining);

            // Return success w/ global localization data.
            globalLocalizationData = new(globalFullyContainingIds,
                globalPartiallyContainingIds);
            return true;
        }
        else
        {
            // Return failure.
            globalLocalizationData = null;
            return false;
        }
    }

    protected static List<int> _GetGlobalIds(
        Dictionary<SpatialId, int> localToGlobalMap,
        string bodyModelKey, IEnumerable<int> localIds)
    {
        List<int> globalIds = new();
        foreach (int localId in localIds)
        {
            var spatialId = new SpatialId
            {
                BodyModelKey = bodyModelKey,
                LocalId = localId
            };
            // Assumes body model string and local ID are a valid pair and exist
            // in the map. Ok because this should only be used internally, i.e.,
            // with data passed in by the Model Manager, so should be valid.
            globalIds.Add(localToGlobalMap[spatialId]);
        }
        return globalIds;
    }
}

