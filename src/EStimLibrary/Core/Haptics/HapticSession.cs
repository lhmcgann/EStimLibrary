using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Core.Stimulation;


namespace EStimLibrary.Core.Haptics;


/// <summary>
/// A HapticSession is the main entry point into active use of this library.
/// A session must be created in any application using this library, and all
/// selection and configuration steps will be calls to the session. The session
/// also controls overall starting and stopping of any live running of the
/// library internals.
/// </summary>
public class HapticSession
{
    private ModelManager _modelManager;
    // TODO: if spatial type enforcing in the Transducer, then can only have 1
    // spatial "data type" per session (bc rn, only 1 transducer per session).
    // --> does it make sense to have transducers per saved body model then?
    // or per saved percept area? but then goes back to transducer-specific
    // scheme of area/thread picking, but did we decide that areas should be
    // defined s.t. non-overlapping anyway?
    private HapticTransducer _transducer;
    private bool _transducerInstantiated;
    private NeuralInterfaceManager _neuralInterfaceManager;
    private StimulatorManager _stimManager;
    private LeadManager _leadManager;
    // {areaId as given by ModelManager : pool of threads with contact pools
    // that reach that area }
    // {globalAreaId: stimThread}
    private Dictionary<int, StimThread> _perceptMap;
    // {globalContactId : globalLocationId}
    private Dictionary<int, int> _contactPlacements;

    // TODO: should also have a full location-contact-output map(s).

    /// <summary>
    /// The global contact IDs that currently exist in this session.
    /// </summary>
    public SortedSet<int> ContactIds =>
        this._neuralInterfaceManager.ContactIds;
    /// <summary>
    /// The global contact IDs that are currently wired to one or more leads.
    /// </summary>
    public SortedSet<int> WiredContacts => this._leadManager._WiredContacts;
    /// <summary>
    /// The global contact IDs that are currently not wired to any leads.
    /// </summary>
    public SortedSet<int> UnwiredContacts => new(this.ContactIds.Intersect(
        this.WiredContacts));
    public Dictionary<int, SortedSet<int>> ContactsPerInterface =>
        this._neuralInterfaceManager.ContactsPerInterface();

    /// <summary>
    /// The global output IDs that currently exist in this session.
    /// </summary>
    public SortedSet<int> OutputIds => this._stimManager.OutputIds;
    /// <summary>
    /// The global output IDs that are currently wired to one or more leads.
    /// </summary>
    public SortedSet<int> WiredOutputs => this._leadManager._WiredOutputs;
    /// <summary>
    /// The global output IDs that are currently not wired to any leads.
    /// </summary>
    public SortedSet<int> UnwiredOutputs => new(this.OutputIds.Intersect(
        this.WiredOutputs));
    public Dictionary<int, SortedSet<int>> OutputsPerStimulator =>
        this._stimManager.OutputsPerStimulator;

    public SortedSet<int> LeadIds => new(this._leadManager.Resources.Keys);
    public Dictionary<int, Lead> Leads => this._leadManager.Resources;

    public List<string> BodyModelKeys => this._modelManager.BodyModelKeys;

    public HapticSession()
    {
        this._transducer = null;
        this._transducerInstantiated = false;

        this._modelManager = new();
        this._neuralInterfaceManager = new();
        this._stimManager = new();
        this._leadManager = new();
        this._perceptMap = new();
        this._contactPlacements = new();
    }

    #region Configuration and Related Accessor Methods
    #region Body Model Config
    public bool TryAddBodyModel(IBodyModel bodyModel,
        out string bodyModelKey, string nonNameBodyModelKey = "")
    {
        return this._modelManager.TryAddBodyModel(bodyModel, out bodyModelKey,
            nonNameBodyModelKey);
    }

    public bool TryGetBodyModel(string bodyModelKey, out IBodyModel bodyModel)
    {
        return this._modelManager._BodyModels.TryGetValue(bodyModelKey,
            out bodyModel);
    }
    #endregion

    #region NI Config
    public SortedSet<int> AddInterface(Type interfaceType,
        object[] interfaceSpecificParams, out int globalInterfaceId)
    {
        var contactIds = this._neuralInterfaceManager
            .CreateAndRegisterNeuralInterface(interfaceType,
            interfaceSpecificParams, out globalInterfaceId);

        // TODO?

        return contactIds;
    }

    public bool IsValidContactId(int contactId)
    {
        return this._neuralInterfaceManager.IsValidContactId(contactId);
    }
    #endregion

    #region Contact Placement Config
    /// <summary>
    /// Place the given contact at a specific location in a body model.
    /// Overwrite any existing placement.
    /// </summary>
    /// <param name="contactId">The int ID of the contact to place.</param>
    /// <param name="bodyModelKey">The string key of the loaded body model on
    /// which the given location lies.</param>
    /// <param name="location">The location at which to place the contact.
    /// </param>
    /// <returns>True if contact placed successfully. False if given contact ID
    /// or spatial information is invalid.</returns>
    public bool TryPlaceOrMoveContact(int contactId, string bodyModelKey,
        ILocation location)
    {
        // Check if contact ID and spatial info are valid.
        if (this.IsValidContactId(contactId) &&
            this._modelManager.TrySaveLocation(bodyModelKey, location,
            out int globalLocationId))
        {
            // Store the contact placement. This [] syntax adds or overwrites.
            this._contactPlacements[contactId] = globalLocationId;
            return true;
        }

        // Error return.
        return false;

        //return (this._modelManager.IsLocationInModel(location) &&
        //    this._interfaceConfig.PlaceContact(contactId, location));
    }

    /// <summary>
    /// Check if a contact has been given a specific placement location.
    /// </summary>
    /// <param name="contactId">The int id of the contact in question.</param>
    /// <returns>True if contact id is valid for this group and contact is
    /// placed. False if invalid or not placed.</returns>
    public bool IsContactPlaced(int contactId)
    {
        return this._contactPlacements.ContainsKey(contactId);
    }

    public bool TryGetContactPlacement(int contactId, out string bodyModelKey,
        out ILocation location)
    {
        // Check if contact ID is placed (doubles as an ID validity check).
        if (this.IsContactPlaced(contactId))
        {
            // Get the key info of the location at which the contact is placed.
            var globalLocationId = this._contactPlacements[contactId];
            // Get the actual location at which it was placed.
            this._modelManager.TryRetrieveLocation(globalLocationId,
                out bodyModelKey, out location);
            // Return success.
            return true;
        }
        // Return failure.
        bodyModelKey = "";
        location = null;
        return false;
    }

    public bool TryUnplaceContact(int contactId, out string bodyModelKey,
        out ILocation location)
    {
        // Return if can 1) find the contact placement, and 2) remove it.
        return (this.TryGetContactPlacement(contactId, out bodyModelKey,
            out location) &&
            this._contactPlacements.Remove(contactId)); // Remove the placement.
    }

    #region Placement TODOs
    // TODO: these functions need to be refactored; decide if needed anyway
    ///// <summary>
    ///// Create a dictionary mapping output IDs to the locations they end up
    ///// connecting to.
    ///// </summary>
    ///// <returns>The dictionary of byte output IDs (as given by
    ///// StimulatorManager) to the ILocation objects they lead to.</returns>
    //public Dictionary<byte, ILocation> GenerateOutputLocationMap()
    //{
    //    // Create empty dictionary to return.
    //    Dictionary<byte, ILocation> outputLocationMap = new();

    //    // Iterate through contact IDs in internal dicts to find
    //    // output-location pairs.
    //    foreach (var (contactId, outputId) in this.ContactOutputMap)
    //    {
    //        outputLocationMap.Add(outputId,
    //            this.ContactLocationMap[contactId]);
    //    }

    //    return outputLocationMap;
    //}

    ///// <summary>
    ///// Create a dictionary mapping specific locations to the IDs of the
    ///// stimulator outputs that stimulate those locations.
    ///// </summary>
    ///// <returns>The dictionary of specific ILocation objects to the byte output
    ///// IDs (as given by StimulatorManager) that stimulate those locations.
    ///// </returns>
    //public Dictionary<ILocation, byte> GenerateLocationOutputMap()
    //{
    //    // Create empty dictionary to return.
    //    Dictionary<ILocation, byte> locationOutputMap = new();

    //    // Iterate through contact IDs in internal dicts to find
    //    // location-output pairs.
    //    foreach (var (contactId, location) in this.ContactLocationMap)
    //    {
    //        locationOutputMap.Add(location,
    //            this.ContactOutputMap[contactId]);
    //    }

    //    return locationOutputMap;
    //}
    #endregion
    #endregion

    #region Stimulator Config
    public bool TryAddStimulator(Type stimulatorType,
        object[] stimSpecificParams, out int globalStimId,
        out SortedSet<int> globalOutputIds)
    {
        // First try creating the stimulator of the given type and params.
        if (!this._stimManager.TryCreateAndRegisterStimulator(
            stimulatorType, stimSpecificParams, out globalStimId,
            out globalOutputIds))
        {
            // Fail early
            return false;
        }

        // TODO?

        // Return success.
        return true;
    }

    public bool IsValidOutputId(int outputId)
    {
        return this._stimManager.IsValidOutputId(outputId);
    }
    #endregion

    #region Wiring Config
    public void AddCable(Type cableHwType, object[] cableSpecificParams)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Wire specific contacts to specific outputs via the given Lead. Leads
    /// must meet the following criteria to be valid:
    /// 1) Include at least one contact and one output.
    /// 2) Include only valid contact and output IDs.
    /// 3) Include outputs from only a single stimulator.
    /// </summary>
    /// <param name="lead">The lead to add.</param>
    /// <param name="leadId">The int ID assigned to the given Lead if added
    /// successfully.</param>
    /// <returns>True if Lead added successfully, False if the requested Lead
    /// is invalid for any reason and could not be added.</returns>
    public bool TryAddLead(Lead lead, out int leadId)
    {
        // 1) Check that the requested lead has 1+ contact and 1+ output.
        if (!(lead.ContactSet.Count > 0 && lead.OutputSet.Count > 0))
        {
            // Fail early.
            leadId = -1;
            return false;
        }

        // 2) Check that the requested lead has all valid contact IDs.
        // (Output IDs will be inherently checked in the next step)
        // Copy ContactSet bc was getting error saying enumerator (in the foreach
        // loop) was being modified after instantiation. Idk how or where though --> TODO
        SortedSet<int> contactSet = new(lead.ContactSet);
        foreach (var contactId in contactSet)
        {
            if (!this.IsValidContactId(contactId))
            {
                // Fail early.
                leadId = -1;
                return false;
            }
        }

        // 3) Check that the requested lead only involves one stimulator.
        // a) Get IDs of all involved stimulators.
        HashSet<int> involvedStimulators = new();
        // Copy OutputSet bc was getting error saying enumerator (in the foreach
        // loop) was being modified after instantiation. Idk how or where though --> TODO
        SortedSet<int> outputSet = new(lead.OutputSet);
        foreach (var outputId in outputSet)
        {
            // Try to get the stimulator ID associated with the output ID.
            // Returns false if the output ID is invalid.
            if (!this._stimManager.TryGetStimulatorOfOutput(outputId,
                out int stimId))
            {
                // Fail early if invalid output ID.
                leadId = -1;
                return false;
            }
            // Else, store stim ID.
            involvedStimulators.Add(stimId);
        }
        // b) Return early if more than 1 stim ID was found.
        if (involvedStimulators.Count > 1)
        {
            // Fail early.
            leadId = -1;
            return false;
        }

        // 4) Make sure the output wiring is valid for the stimulator.
        // Get the stimluator itself.
        this._stimManager.TryGetResource(involvedStimulators.First(), out var stim);
        // Get the local output IDs of this lead.
        var localOutputIds = this._stimManager.GetGlobalLocalOutputIdMap(
            lead.OutputSet).Values;
        // Check if it's a valid group.
        if (!stim.IsValidOutputWiring(localOutputIds))
        {
            // Fail early.
            leadId = -1;
            return false;
        }

        // 5) Try adding the lead (ID asgt, internal data changes).
        // TODO: should there be any deeper (more meaningful) lead validation?
        return this._leadManager.TryAddLead(lead, out leadId);
    }
    #endregion

    #region Percept Mapping Config
    // TODO: Creates StimThread per lead pool, configures dataChanged callback
    // functions to go to the Stimulators themselves, thru StimMgr or not
    // Must also validate the pool: 2 indp leads min per stim
    // TODO: at some point need to manage overlapping contact/lead pools
    // --> SMgr? Stimulator? Make Leads Identifiable and Claimable?
    /// <summary>
    /// Try to add a lead set to this session as covering a specific reachable
    /// haptic area.
    /// </summary>
    /// <param name="leadIds">The requested set of global global IDs.</param>
    /// <param name="bodyModelKey">The string key of the loaded body model on
    /// which the given area lies.</param>
    /// <param name="reachableHapticArea">The area on the body within which
    /// the requested lead set can elicit haptic percepts. This area should
    /// be the largest area on which haptic percepts can potentially be evoked
    /// by the given lead set. Adding or removing another independent lead
    /// would increase or decrease, respectively, the size of the reachable
    /// area.</param>
    /// <param name="firstInvalidLeadId">An output parameter: the ID of the
    /// first invalid lead ID. Only relevant if method returns false. -1 if all
    /// lead IDs valid and the method failed for some other reason.</param>
    /// <returns>T/F if the requested lead set and spatial info are valid and a
    /// StimThread could be created.</returns>
    public bool TryMapLeadPool(SortedSet<int> leadIds, string bodyModelKey,
        IArea reachableHapticArea, out int firstInvalidLeadId)
    {
        // 1) Check if all lead IDs are valid, getting the Leads if they are.
        bool validIds = true;
        List<Lead> leads = new();
        // TODO: same mod while iterating error, only in debug
        foreach (int leadId in leadIds.ToList())
        {
            if (this._leadManager.Resources.TryGetValue(leadId, out Lead lead))
            {
                // Store the lead if ID was valid.
                leads.Add(lead);
            }
            // Otherwise, quit early, reporting the invalid ID.
            else
            {
                firstInvalidLeadId = leadId;
                return false;
            }
        }
        // If get here, indicate all lead IDs valid.
        firstInvalidLeadId = -1;

        // 2) Make sure there are 2+ independent leads per stimulator involved.
        // a) Sort leads by stimulator.
        Dictionary<int, HashSet<Lead>> leadsPerStimulator = new();
        foreach (var lead in leads)
        {
            // Determine which stimulator this lead connects to. Lookup by any
            // output ID since a lead must contain only outputs on the same
            // stimulator. Ignore bool return: outputs in saved lead are valid.
            this._stimManager.TryGetStimulatorOfOutput(
                lead.OutputSet.ToArray()[0], out int stimId);
            // Add the lead to the leads-per-stim dictionary.
            if (!leadsPerStimulator.TryGetValue(stimId, out var leadSet))
            {
                leadSet = new();
                leadsPerStimulator.Add(stimId, leadSet);
            }
            //leadsPerStimulator.GetValueOrDefault(stimId).Add(lead);
            leadSet.Add(lead);
        }
        // b) Make sure there exists a pair of independent leads per stimulator.
        foreach (var (stimId, leadSet) in leadsPerStimulator)
        {
            // Return false early if one stimulator does not have an independent
            // Lead pair with opposite current directions.
            if (!Lead.IndependentLeadsExist(leadSet,
                checkCurrentDirection: true))
            {
                return false;
            }
        }

        // 3) Try to save the given area.
        // Validates bodyModelKey+area. Saves area and outputs designated area
        // ID upon success.
        if (!this._modelManager.TrySaveArea(bodyModelKey, reachableHapticArea,
            out int globalAreaId))      // Saved area ID
        {
            // Error return if invalid spatial info.
            return false;
        }

        // 4) Fail if the area has already been mapped to a different lead set.
        if (this._perceptMap.TryGetValue(globalAreaId,
            out StimThread mappedThread) &&
            leadsPerStimulator != mappedThread.LeadsPerStimulator)
        {
            return false;
        }

        // 5) Create the new associated stim thread for this lead pool.
        // a) Create each thread config data struct per stimulator involved.
        Dictionary<int, ThreadConfigDataPerStimulator> allConfigData = new();
        foreach (var (stimId, stimLeads) in leadsPerStimulator)
        {
            // Get the Stimulator itself. Assuming stim ID is valid.
            this._stimManager.TryGetStimulator(stimId, out var stimulator);
            // Create config data: [stimId, leads, availableParams]
            ThreadConfigDataPerStimulator data = new(stimId, stimLeads,
                stimulator.StimParamData, stimulator.ModulatableStimParams);
            // Add to total set of config data.
            allConfigData.Add(stimId, data);
        }
        // b) Create the stim thread itself with all config data and the update
        // event callback method for when stim data is changed.
        StimThread stimThread = new(allConfigData,
            this._stimManager.UpdateStim);
        // c) Save the thread as mapped to the given area.
        this._perceptMap.Add(globalAreaId, stimThread);

        //// 5) Add the new StimThread to the StimThreadPool associated with the
        //// given reachable haptic area. If the area is not already covered,
        //// create a new StimThreadPool with the new StimThread for requested
        //// (full valid) contact pool. Otherwise, just add the new stim thread
        //// to the existing StimThreadPool.
        //if (!this._areaIdToStimThreadPoolMap.TryGetValue(globalAreaId,
        //    out StimThreadPool localizedThreadPool))
        //{
        //    // If doesn't exist already, create new StimThreadPool.
        //    localizedThreadPool = new();
        //    // Add the new pool to the (modelStrKey+areaId)-based map.
        //    this._areaIdToStimThreadPoolMap.Add(globalAreaId,
        //        localizedThreadPool);
        //}
        //// Add the new thread to the area-mapped pool.
        //localizedThreadPool.AddStimThread(stimThread);

        // Return success.
        return true;
    }
    #endregion

    #region Transducer Config
    // TODO: who is responsible for instantiating the transducer? make
    // transducer the generic parameter of HapticSession? Or allow multiple
    // transducers per session, i.e., that can be assigned per some large area
    // rather than a single transducer that is expected to handle each area
    // according to how it wishes?
    public void SetTransducer(HapticTransducer transducer)
    {
        this._transducer = transducer;  // TODO: deep copy?
        this._transducerInstantiated = true;
    }
    #endregion
    #endregion

    #region Run-Time Methods
    public bool Start()
    {
        // First make sure everything is configured that needs to be.
        if (!this._transducerInstantiated)  // TODO: what other config checks?
        {
            // Fail to start the session if not.
            return false;
        }

        // Start all Stimulators.
        this._stimManager.StartStimulationOnAll();

        // TODO: what else?

        // Return successful session start.
        return true;
    }

    public void Stop()
    {
        // Stop all Stimulators.
        this._stimManager.StopStimulationOnAll();

        // TODO: what else?
    }

    /// <summary>
    /// Instigate the threaded processing of a new haptic event in this session.
    /// </summary>
    /// <param name="hapticEvent">The triggering haptic event.</param>
    /// TODO: ok to have the programmatic thread be here rather than calling
    /// context? bc how is calling context allowing multiple events at a time?
    /// If each triggering event is already on its own thread, then don't need
    /// to make a new one here
    public void AddEvent(HapticEvent hapticEvent)
    {
        //ThreadPool.QueueUserWorkItem(this._AddEvent, hapticEvent);
        this._AddEvent(hapticEvent);
    }

    protected void _AddEvent(object state)
    {
        // 0) Cast state object to HapticEvent bc guaranteed to be that type.
        var hapticEvent = (HapticEvent)state;

        // 1) Localize event --> LocalizationData: ids of relevant areas
        bool validEvent = this.LocalizeEvent(hapticEvent,
            out LocalizationData relevantAreaIds);

        // 2) Get the actual areas.
        // TODO: would it be easier to just store the saved IArea itself in the
        // StimThread rather than doing this whole lookup process again?
        Dictionary<int, IArea> fullyContainingAreas = new();
        foreach (var globalAreaId in relevantAreaIds.AreasFullyContaining)
        {
            this._modelManager.TryRetrieveArea(globalAreaId, out var _,
                out var relevantArea);
            fullyContainingAreas.Add(globalAreaId, relevantArea);
        }
        Dictionary<int, IArea> partiallyContainingAreas = new();
        foreach (var globalAreaId in relevantAreaIds.AreasPartiallyContaining)
        {
            this._modelManager.TryRetrieveArea(globalAreaId, out var _,
                out var relevantArea);
            partiallyContainingAreas.Add(globalAreaId, relevantArea);
        }

        // 3) Get all areaId-associated StimThreads
        // TODO: ID-based operations will be affected by changes enabling haptic
        // events to span >1 model (per the comment by LocalizeEvent() below).
        var relevantThreads = relevantAreaIds.AreasFullyContaining
            .Union(relevantAreaIds.AreasPartiallyContaining)
            // Keep the model-local area/pool ID.
            .ToDictionary(id => id,
            // Get the StimThread keyed by the globalAreaId.
            id => this._perceptMap[id]);

        // 4) Call transducer method.
        this._transducer.TransduceHapticEvent(hapticEvent, fullyContainingAreas,
            partiallyContainingAreas, relevantThreads);
    }

    /// <summary>
    /// Localize a haptic event to relevant saved body model areas.
    /// TODO: refactor so an event can span more than one body model. will
    /// probably have to come up with a better way than string keying to
    /// unify/merge body models
    /// </summary>
    /// <param name="hapticEvent">The event to localize. It must contain a
    /// valid body model string key, as well as a valid location or area,
    /// depending on its LocalizeByArea property.</param>
    /// <param name="locData">An out parameter. The event localization data,
    /// containing IDs of areas fully and partially containing the given event.
    /// IDs are local to the body model keyed by the event. This data is only
    /// valid if the method returns true.</param>
    /// <returns>True if the event could be fully localized, False if any
    /// spatial element could not be localized and the event could not.be
    /// processed. TODO: change so still partially processes if partial
    /// localization success?</returns>
    public bool LocalizeEvent(HapticEvent hapticEvent,
        out LocalizationData locData)
    {
        bool success = true;
        locData = new(new List<int>(), new List<int>());
        // Localize by area or location.
        if (hapticEvent.LocalizeByArea)
        {
            // TODO: check event.area is not null or empty?
            foreach (var (bodyModelKey, areas) in hapticEvent.Areas)
            {
                // Quit if could not localize the previous area.
                if (!success)
                {
                    break;
                }
                foreach (var area in areas)
                {
                    // Stop if could not localize the area.
                    if (!(success = this._modelManager.TryLocalizeByArea(
                        bodyModelKey, area, out var someLocData)))
                    {
                        break;
                    }
                    // Otherwise merge this area's  localization restults.
                    else
                    {
                        locData = locData.Merge(someLocData);
                    }
                }
            }
        }
        // Else, localize by location
        else
        {
            // TODO: check event.location is not null or empty?
            foreach (var (bodyModelKey, locs) in hapticEvent.Locations)
            {
                // Quit if could not localize the previous location.
                if (!success)
                {
                    break;
                }
                foreach (var location in locs)
                {
                    // Stop if could not localize the location.
                    if (!(success = this._modelManager.TryLocalizeByLocation(
                        bodyModelKey, location, out var someLocData)))
                    {
                        break;
                    }
                    // Otherwise merge this area's  localization restults.
                    else
                    {
                        locData = locData.Merge(someLocData);
                    }
                }
            }
        }
        return success;
        /**
         * rule: 
         LocalizationData
            areasPartiallyContaining
            areasFullyContaining
         */
    }
    #endregion
}

/**
 TODO: MAIN CONFIG SEQUENCE
need PerceptAreas and associated running Threads, basically

manually (this for now) or smart-gen (i.e., from experimental data) the PerceptAreas/ThreadPools, Percepts/ContactPools/Threads mappings
let output configs and param data be up to a matrix (for now)
1 PerceptArea/Percept (and thus ContactPools/Threads) per finger
    limit: ContactPool can only contain contacts wired to same stimulator? OR just must contain at least 2 contacts per stimulator involved (bc otherwise no closed circuit)
via Contact-Output map, get set of outputs involved
    (split by stimulator involved)
***"matrix gen" params per output config***
stim immediately

 TODO: MAIN CALLBACK SEQUENCE
1) HapticEvent(quality, intensity, spatial) --> LocalizeHapticEvent --> {Percepts p | HapticEvent.SpatialInfo is a subset of - or best approximated by - p.SpatialInfo}
    ^^^ TODO: distance metric between SpatialInfos so can sort based on shortest?
    Could be a good Percept sort/prioritization/selection alg, and later could
    include other factors like if wanted to prioritize quality/intensity over location
2) Choose Percept(s?) want to use (based on quality, intensity, etc (@more general selection alg?^^^)
3) Get the association Thread/ContactPool > OutputConfigs available/used to elicit desired percept
    Maybe just possible configs for now? Then a matrix decides which one(s) to use?
4) Generate trains of pulses w/ spc params on spc configs --> send a single stream of info to HW
    TODO: how is this info being communicated? ahead of time? bc want FW/HW not MW doing timing?
        or just saying "go now until changed"?
    TODO: StimThread should have a method: BuildPulseSequence --> Stimulator has a method: BuildMessageSequence and SendMessageSequence
    --> Should Stimulator then have a msg queue? how to deal w/ timing though? esp across threads?

POSSIBLE REV1
Stimulator has pulses per output config
any time pulse data or output config data changes, send msg
the thread that gets launched to send this data should:
    recognize which data has changed and send only that data
    make a copy of the changed data so if it's changed while the send thread is waiting, old change not lost (or this could be a setting: save and send or overwrite w/ most recent changes, i.e., whatever is in the data struct a the time, but then could end up in situation where data changing too fast that no "real"/"action" data ever gets sent)
    waits for the comms lock
    sends
by this means, Thread could just keep changing the data one-by-one, BUT (TODO) back to timing: how to do more granular timing control? I.e., schedule for future?
 */

