using EStimLibrary.Core.Stimulation.Stimulators;
using EStimLibrary.Core.Stimulation.Data;

//using ID = int; // not supported in this verison :(


namespace EStimLibrary.Core.Stimulation;


public class StimulatorManager : ResourceManager<Stimulator>
{
    //// Object to inspect the stack trace when handling/creating error messages.
    //private readonly StackTrace _stackTrace = new();

    // Some key properties are inherited from the parent class ResourceManager.
    // The Stimulator objects themselves are stored in Resources.
    // The ID pool is IdPool.
    // If a managed Stimulator resource exists, its ID should be marked as Used.
    // Free IDs can exist in the pool, e.g., if a Stimulator was added then
    // removed, but there must be no managed resource in Resources under the ID.

    // Private data structures specific to stimulator management.
    private Dictionary<string, SortedSet<int>> _stimulatorsWithAbilities;


    /// <summary>
    /// The global output IDs that currently exist in this session.
    /// </summary>
    // NOTE: They are only those marked as "used" by the StimMgr since some
    // IDs may have been removed, leaving non-consecutive IDs in the pool
    // overall, so only the "used" IDs are live.
    public SortedSet<int> OutputIds => this._OutputIdPool.UsedIds;
    protected ReusableIdPool _OutputIdPool { get; set; }
    protected Dictionary<int, int> _OutputStimulatorIdMap;
    protected Dictionary<int, ReusableIdPool> _OutputsPerStimulatorUsage
    { get; private set; }
    // TODO: use the 'Used' marking on these ^ smaller output ID pools to
    // represent outputs actually being used in an output config
    public Dictionary<int, SortedSet<int>> OutputsPerStimulator =>
        this._OutputsPerStimulatorUsage
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Ids);

    // Properties to relay total stimulator and output counts.
    public int NumTotalStimulators => this.NumTotalResources;
    // NOTE: The "total" *live* output IDs are only those marked as "used" by
    // the StimMgr. This is to account for stimulator (& subsequent output ID)
    // removal which would leave Stim and outputIDs in existence in the pool,
    // just not presently used.
    public int NumTotalOutputs => this.OutputIds.Count;

    public StimulatorManager() : base()
    {
        // Init empty dictionaries, resource maps, etc.
        this._stimulatorsWithAbilities = new();
        this._OutputIdPool = new();
        this._OutputStimulatorIdMap = new();
        this._OutputsPerStimulatorUsage = new();

        // Create stimulator ability dict with empty sets for all stim params.
        foreach (string p in BaseStimParams.ParamOrderIndices.Keys)
        {
            this._stimulatorsWithAbilities.Add(p, new());
        }
    }

    /// <summary>
    /// Create and register a stimulator of the given type with this stimulator
    /// manager.
    /// </summary>
    /// <param name="stimulatorType">The specific Type of Stimulator desired.
    /// </param>
    /// <param name="stimSpecificParams">Any additional constructor arguments
    /// needed for the requested stimulatorType, beyond base Stimulator args.
    /// </param>
    /// <param name="globalStimId">Output parameter. The unique ID assigned to 
    /// the given stimulator in the management system.</param>
    /// <returns>True if a Stimulator of the given type could be created with
    /// the given arguments, False if not. TODO: use generics and reflection
    /// rather than run-time type-checking?</returns>
    public bool TryCreateAndRegisterStimulator(Type stimulatorType,
        object[] stimSpecificParams, out int globalStimId,
        out SortedSet<int> globalOutputIds)
    {
        // 0) Check stimType is valid derived class, throw exception if not.
        if (!(typeof(Stimulator).IsAssignableFrom(stimulatorType)
             && !stimulatorType.IsInterface && !stimulatorType.IsAbstract))
        {
            // Fail early.
            globalStimId = -1;
            globalOutputIds = null;
            return false;
        }

        // 1) Try to generate this new stimulator's new managed ID.
        if (!this.TryGetNextAvailableId(out globalStimId))
        {
            // Fail early.
            globalStimId = -1;
            globalOutputIds = null;
            return false;
        }

        // 2) If successful, create the new stimulator of the given type.
        // 2a) Create the Stimulator (dynamic runtime type determination)
        dynamic stim = Activator.CreateInstance(stimulatorType,
            stimSpecificParams);
        // 2b) Set the global ID of the stimulator.
        stim.Id = globalStimId;

        // 3) Add this stimulator as a managed resource with its new given ID.
        this.TryAddResource(globalStimId, stim);

        // 4) Add the new stim ID to the set of each pulse param it supports.
        foreach (string p in stim.ModulatableStimParams)
        {
            this._stimulatorsWithAbilities[p].Add(globalStimId);
        }

        // 5) Generate the new output IDs (inherently marks them as used) and
        // add them to internal structs.
        globalOutputIds = this._UseOutputs((int)stim.NumOutputs);
        // Enable outputID --> stimID lookup.
        foreach (var globalOutputId in globalOutputIds)
        {
            this._OutputStimulatorIdMap.Add(globalOutputId, globalStimId);
        }
        // Enable stimID --> {outputIDs} lookup.
        this._OutputsPerStimulatorUsage.Add(globalStimId, new ReusableIdPool(
            globalOutputIds.First(), globalOutputIds.Count));

        // Return success.
        return true;
    }

    /// <summary>
    /// Use the given number of outputs to the global pool, claiming existing
    /// IDs or adding new ones as needed.
    /// </summary>
    /// <param name="numOutputs">The number of outputs to use.</param>
    /// <returns>The set of outpus IDs used.</returns>
    protected SortedSet<int> _UseOutputs(int numOutputs)
    {
        // Init a output ID set to fill in and then return.
        SortedSet<int> outputIds = new();

        // Generate n new output IDs.
        for (int i = 0; i < numOutputs; i++)
        {
            // Generate a new output ID.
            int outputId = this._GetNextAvailableOutputId();
            // Mark the new ID as "used" so another GetNext call doesn't get the
            // same ID.
            this._OutputIdPool.UseId(outputId);
            // Add the new output ID to the returned set.
            outputIds.Add(outputId);
        }
        // Return the new set of output IDs.
        return outputIds;
    }

    /// <summary>
    /// Get the next free global output ID, increasing the pool ID count if
    /// none are immediately free.
    /// </summary>
    /// <returns>The next free global output ID.</returns>
    protected int _GetNextAvailableOutputId()
    {
        // Get the next available ID. If none and if still haven't hit hard max
        // limit on resource count, increase the ID pool max and try again, else
        // return failure.
        int globalOutputId;
        while (!this._OutputIdPool.TryGetNextFreeId(out globalOutputId))
        {
            // Else increment the number of IDs in the pool and try again.
            this._OutputIdPool.IncrementNumIds(1);
        }
        return globalOutputId;
    }

    /// <summary>
    /// Check if a global output ID is valid among currently registered
    /// stimulators.
    /// </summary>
    /// <param name="outputId">The global int output ID.</param>
    /// <returns>True or false.</returns>
    public bool IsValidOutputId(int outputId)
    {
        return this.OutputIds.Contains(outputId);
    }

    public bool IsValidStimId(int stimId)
    {
        return this.IsValidResourceId(stimId);
    }

    public bool TryGetStimulator(int stimId, out Stimulator stimulator)
    {
        return this.TryGetResource(stimId, out stimulator);
    }

    public bool TryRemoveStimulator(int stimId, out Stimulator removedStim,
        out SortedSet<int> globalOutputIds)
    {
        // Try removing the {stimId: Stimulator} and {stimId: {outputIds}} data
        // entries. Use these as data getters and ID validation as well. The
        // TryRemove also marks the ID as free upon successful removal.
        if (!this.TryRemoveResource(stimId, out removedStim) ||
            !this._OutputsPerStimulatorUsage.Remove(stimId, out var outputIdPool))
        {
            globalOutputIds = new();
            return false;
        }

        // Remove all stimulator output entries.
        globalOutputIds = outputIdPool.Ids;
        foreach (var outputId in globalOutputIds)
        {
            // Remove the ID from the outputID->stimId map.
            this._OutputStimulatorIdMap.Remove(outputId);
            // Mark the integer ID itself as free to use for a new Stimulator.
            this._OutputIdPool.FreeId(outputId);
        }

        // Return success.
        return true;
    }

    /// <summary>
    /// Find all registered stimulators that can support (modulate) the given
    /// pulse params.
    /// </summary>
    /// <param name="abilities">The set of pulse params requested.</param>
    /// <returns>The set of IDs of stimulators able to support the requested
    /// pulse params. May be an empty set if no stimulators support the
    /// requested parameters.</returns>
    public SortedSet<int> FindStimulatorsWithAbilities(
        SortedSet<string> abilities)
    {
        // 1) Init a set with all registered stimulator IDs.
        SortedSet<int> ableStimulators = new(this.IdPool.UsedIds);

        // 2) Only keep the stimulators that can support all requested params.
        // Iterate over each parameter requested.
        foreach (var param in abilities)
        {
            // Get the set of stim IDs that support the parameter.
            var stimSet = this._stimulatorsWithAbilities[param];
            // Only keep the stim IDs that support each parameter.
            ableStimulators.IntersectWith(stimSet);     // in-place operation
        }

        // 3) Return the set of IDs of stimulators that support all requested
        // abilities.
        return ableStimulators;
    }

    public bool TryGetStimulatorOfOutput(int globalOutputId, out int stimId)
    {
        return this._OutputStimulatorIdMap.TryGetValue(globalOutputId,
            out stimId);
    }

    public bool TryGetStimulatorsOfOutputs(IEnumerable<int> outputIds,
        out SortedSet<int> stimIds)
    {
        stimIds = new();
        foreach (var outputId in outputIds)
        {
            if (!this._OutputStimulatorIdMap.TryGetValue(outputId,
                out int stimId))
            {
                return false;
            }
            stimIds.Add(stimId);
        }
        return false;
    }

    public bool OrganizeOutputsPerStimulator(IEnumerable<int> outputIds,
        out Dictionary<int, SortedSet<int>> outputsPerStim)
    {
        // Get IDs of stimulators the outputs belong to, false if invalid IDs.
        if (!this.TryGetStimulatorsOfOutputs(outputIds, out var relevantStimIds))
        {
            outputsPerStim = new();
            return false;
        }

        // Sort outputs by stim ID, keeping only requested outputs in the sets.
        outputsPerStim = this._OutputsPerStimulatorUsage
            // Keep relevant Stimulators only.
            .Where(item => relevantStimIds.Contains(item.Key))
            .ToDictionary(kvp => kvp.Key,
                // Keep relevant output IDs only.
                kvp => new SortedSet<int>(kvp.Value.Ids.Intersect(outputIds)));
        return true;
    }

    /// <summary>
    /// Check if an output is used by any current output configuration.
    /// </summary>
    /// <param name="globalOutputId">The global output ID of the output to
    /// check.</param>
    /// <returns>True if output is used, False if not or if the lookup failed.
    /// </returns>
    public bool IsOutputUsed(int globalOutputId)
    {
        // Check output ID is valid first.
        return (this.IsValidOutputId(globalOutputId) &&
            // Getting the output's stimulator and ask if the output is used.
            this.Resources[this._OutputStimulatorIdMap[globalOutputId]]
            .IsOutputUsed(globalOutputId));
    }

    /// <summary>
    /// Get the list of local output IDs corresponding to the given global IDs.
    /// </summary>
    /// <param name="globalOutputIds">The global output IDs to lookup. Assumed
    /// to be valid.</param>
    /// <returns>A dictinoary mapping the requested global output IDs to
    /// corresponding local IDs.</returns>
    internal Dictionary<int, int> GetGlobalLocalOutputIdMap(IEnumerable<int> globalOutputIds)
    {
        return this._OutputIdPool.GetGlobalLocalMap()
            .Where(kvp => globalOutputIds.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        //.Select(kvp => kvp.Value);
    }

    // Follows the "void StimDataUpdateHandler(StimThread)" delegate.
    public void UpdateStim(StimThread stimThread)
    {
        // Send updated stim trains to each stimulator involved in the thread.
        //foreach (var (stimId, trainSet) in stimThread.PulseTrainsPerStimulator)
        foreach (var (stimId, trainsParams) in stimThread.TrainParamsPerStimulator)
        {
            // Try getting the stimulator itself.
            if (this.TryGetResource(stimId, out var stim))
            {
                // Get map of all global to local output IDs. So stimulator can
                // generate actual output config arrays from stim data.
                var globalToLocalOutputIds =
                    this._OutputsPerStimulatorUsage[stimId].GetGlobalLocalMap();

                // Specify the current direction for each output based on
                // default lead designation.
                Dictionary<int, Constants.OutputAssignment>
                    localOutputAssignments = new();
                foreach (var lead in stimThread.LeadsPerStimulator[stimId])
                {
                    foreach (var globalOutputId in lead.OutputSet)
                    {
                        int localOutputId =
                            globalToLocalOutputIds[globalOutputId];
                        localOutputAssignments.Add(localOutputId,
                            // NOTE: this conversion only works bc the two
                            // enums ultimately have the same int values.
                            (Constants.OutputAssignment)lead.CurrentDirection);
                    }
                }

                // Send the local-ID'd data to the stimulator. Call the update
                // method in a new programmatic thread. The per-Stim method
                // should do config and data checks before sending data.
                // TODO: that method should release the data lock claimed
                // earlier (in StimThread? per train? per thread? per lead?)
                // Note: when the UpdateStim method gets called, state will have
                // the value of Tuple(trains, globalToLocalOutputIds).
                //ThreadPool.QueueUserWorkItem(state => stim.UpdateStim(state),
                //    (trains, globalToLocalOutputIds));
                stim.UpdateStim((trainsParams, localOutputAssignments));
            }
            // Do nothing if invalid stim ID. TODO: how best to indicate this
            // failure somehow? or trust all valid IDs bc shouldn't allow thread
            // to be created w/ invalid stimIDs?
        }
    }


    // TODO: implement the modes described/alluded to in the doc comments of the
    // methods below (e.g., live stim vs not, but still accepting other updates,
    // as in a config or paused mode or something?)

    /// <summary>
    /// Start active stimulation on all registered stimulators. Stim param data
    /// variables will be pushed to the stimulator devices themselves, and the
    /// devices will act on those data. The order in which stimulator devices
    /// start stimulation is not fixed not guaranteed to be consistent, but
    /// execution of the start commands is made to be as simultaneous as
    /// possible.
    /// </summary>
    public void StartStimulationOnAll()
    {
        // Start stimulation on each registered stimulator in a separate thread.
        foreach (var (_, stim) in this.Resources)
        {
            //ThreadPool.QueueUserWorkItem(_ => stim.StartStim());
            stim.StartStim();
        }
    }

    /// <summary>
    /// Start active stimulation on a given stimulator. Stim param data will be
    /// pushed to the stimulator device itself, and the device will act on that
    /// data.
    /// </summary>
    /// <param name="stimId">The manager-given unique ID of the stimulator.
    /// </param>
    public bool StartStimulationOnOne(int stimId)
    {
        // Try getting the stimulator itself.
        if (this.TryGetResource(stimId, out var stimulator))
        {
            // TODO: have the per-stim Start() method return a bool and wait
            // until that returns before returning from here? then also return
            // that method's return rather than just 'true'?
            stimulator.StartStim();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Stop active stimulation on all registered Stimulators. Stim param data
    /// variables awaiting a push to the hardware may still be updated by
    /// external contexts, but no values will be propagated to the stimulator
    /// devices themselves, and the devices will cease all active stimulation
    /// despite prior commands. The last stim data values will be retained by
    /// the system but may or may not be retained by the stimulator devices
    /// themselves. Configuration changes are unaffected by this command and may
    /// still be updated and pushed to the stimulator devices. The order in
    /// which stimulator devices stop stimulation is not fixed nor guaranteed to
    /// be consistent, but execution of the stop commands is made to be as rapid
    /// and simultaneous as possible.
    /// </summary>
    public void StopStimulationOnAll()
    {
        // Stop stimulation on each registered stimulator in a separate thread.
        foreach (var (_, stim) in this.Resources)
        {
            //ThreadPool.QueueUserWorkItem(_ => stim.StopStim());
            stim.StopStim();
        }
    }

    /// <summary>
    /// Stop active stimulation on a given Stimulator. Stim param data
    /// variables awaiting a push to the hardware may still be updated by
    /// external contexts, but no values will be propagated to the stimulator
    /// itself, and the device will cease all active stimulation despite prior
    /// commands. The last stim data values will be retained by the system but
    /// may or may not be retained by the stimulator device itself.
    /// Configuration changes are unaffected by this command and may still be
    /// updated and pushed to the stimulator device.
    /// </summary>
    /// <param name="stimId">The manager-given unique ID of the stimulator.
    /// </param>
    public bool StopStimulationOnOne(int stimId)
    {
        // Try getting the stimulator itself.
        if (this.TryGetResource(stimId, out var stimulator))
        {
            // TODO: have the per-stim Stop() method return a bool and wait
            // until that returns before returning from here? then also return
            // that method's return rather than just 'true'?
            stimulator.StopStim();
            return true;
        }
        return false;
    }
}

