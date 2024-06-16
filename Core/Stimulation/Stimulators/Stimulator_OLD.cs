//using MathNet.Numerics.LinearAlgebra;
//using EStimLibrary.Core.Stimulation.Data;
//using EStimLibrary.Core.Stimulation.Functions;

//namespace EStimLibrary.Core.Stimulation.Stimulators;

//// This is an abstract class rather an interface because it provides a base
//// class implementation rather than extension capabilities. The abstract class
//// allows this Stimulator super class to have instantiable class fields.
//public abstract class StimulatorOLD : ISelectable, IIdentifiable
//{
//    // TODO: once actually have apps and things running both in and out of this
//    // assembly, re-discuss which access modifiers desired here, in derived
//    // classes, etc
//    public abstract string Name { get; }        // ISelectable
//    public int Id { get => this._id; }          // IIdentifiable

//    // Manager-given ID of the stimulator.
//    protected internal readonly int _id;
//    // Private field - only used by base class Stimulator methods - for base
//    // output ID (global context).
//    private readonly int _baseOutputId;
//    // Set of global output IDs allocated to this Stimulator.
//    // TODO: look at NumOutputs-related bugfix in testing branch
//    private readonly ReusableIdPool _globalOutputIdPool;
//    public SortedSet<int> GlobalOutputIds { get => this._globalOutputIdPool.Ids; }
//    private int[] _outputUses;

//    // Properties for: stim pulse params supported by the stimulator; number
//    // of total outputs; number of max, used, and free channels.
//    // Derived classes must implement the get() of the abstract properties.
//    // e.g., SpecificStimulator.NumOutputs get { return constNumOutputs; }.
//    protected internal abstract SortedSet<PhaseParam> PulseParamsAvailable
//    { get; }
//    protected internal abstract int NumOutputs { get; }
//    protected internal abstract int MaxNumChannels { get; }   // -1 == unlim'd.
//    protected internal byte NumUsedChannels
//    {
//        get
//        {
//            return (byte)this._channels.Keys.Count;
//        }
//    }
//    protected internal int NumFreeChannels
//    {
//        get
//        {
//            // Return minimum of -1 (unlimited channels) and remaining num
//            // channels. Max remaining num channels calculation with -1 so
//            // doesn't go below -1.
//            return Math.Min(this.MaxNumChannels,
//                Math.Max(-1, this.MaxNumChannels - this.NumUsedChannels));
//        }
//    }

//    // Dictionary mapping global to local channel IDs.
//    private Dictionary<byte, byte> _globalLocalChannelIdMap;
//    // Dictionary mapping local channel ID to corresponding Channel object.
//    private SortedDictionary<byte, StimThread> _channels;

//    // Private fields for the base class Stimulator to do resource management.
//    private readonly SortedSet<byte> _channelLocalIds;
//    private SortedSet<byte> _usedChannelLocalIds;
//    private SortedSet<byte> _freeChannelLocalIds
//    {
//        get
//        {
//            // Return all except the used channel IDs.
//            SortedSet<byte> channels = new(this._channelLocalIds);
//            channels.ExceptWith(this._usedChannelLocalIds);
//            return channels;
//        }
//    }

//    // A variable to store the function used to validate a stimulator's output
//    // config. See C# delegates and Stimulators.Functions.OutputConfigChecks.
//    protected internal abstract ValidateOutputConfigDelegate OutputConfigCheckFunction
//    { get; }
//    // A variable to store the function used to validate a stimulator's param
//    // data. See C# delegates and Stimulators.Functions.StimParamDataChecks.
//    protected internal abstract ValidateStimParamDataDelegate StimParamDataCheckFunction
//    { get; }

//    // A lock on the communicationn line for message sending.
//    private readonly object _commsLock = new();

//    // Fields for controlling stimulator modes.
//    private bool _pushConfigData;
//    private bool _pushStimParamData;
//    private bool _stimOn;

//    /// <summary>
//    /// Base initialization functionality for any Stimulator. Derived classes
//    /// must call this constructor in addition to their own construction
//    /// functionality.
//    /// </summary>
//    /// <param name="stimId">The ID of the Stimulator, given by the manager and
//    /// unique in this run-time system.</param>
//    /// <param name="baseOutputId">The base value to add each local output ID
//    /// to in order to get the corresponding global output ID. Global output IDs
//    /// will be assigned to the Stimulator in a block starting at this value.
//    /// </param>
//    protected StimulatorOLD(int stimId, int baseOutputId)
//    {
//        // Store stim ID.
//        this._id = stimId;
//        // Store base output ID.
//        this._baseOutputId = baseOutputId;

//        // Init set of global output IDs. Const after init.
//        this._globalOutputIdPool = new(this._baseOutputId, this.NumOutputs);

//        // Init array of used markings for each output.
//        this._outputUses = Enumerable.Repeat(0, this.NumOutputs).ToArray();

//        // TODO: refactor below
//        // Init set of local channel IDs. Const after init.
//        var _ = Enumerable.Range(               // Use a temp variable.
//            0, this.MaxNumChannels)
//            .Select(x => (byte)x).ToHashSet();
//        this._channelLocalIds = new SortedSet<byte>(_);

//        // Init empty Channel-related dicts.
//        this._usedChannelLocalIds = new();      // used local channel ID set.
//        this._globalLocalChannelIdMap = new();  // globalId:localId dict.
//        this._channels = new();                 // localId:Channel dict.

//        // Init "mode" booleans to false.
//        this._pushConfigData = this._pushStimParamData = this._stimOn = false;
//    }

//    /// <summary>
//    /// Try to convert a global channel ID to the corresponding local channel ID
//    /// in this Stimulator's context.
//    /// </summary>
//    /// <param name="globalChannelId">The global channel ID to validate.</param>
//    /// <param name="localChannelId">Output parameter. The corresponding local
//    /// channel ID.</param>
//    /// <returns>True if localChannelId is valid and can be used, False if not.
//    /// </returns>
//    private bool _TryConvertGlobalToLocalChannelId(byte globalChannelId,
//        out byte localChannelId)
//    {
//        // Try looking up the global channel ID in the global:local dictionary.
//        // Store looked-up value in output param and return the bool value.
//        return this._globalLocalChannelIdMap.TryGetValue(globalChannelId,
//            out localChannelId);
//    }

//    /// <summary>
//    /// Lookup the local channel IDs for the given global channel IDs.
//    /// </summary>
//    /// <param name="globalIds">The list of global channel IDs to look up. An
//    /// ID will be ignored if invalid.
//    /// </param>
//    /// <returns>A dictionary of requested global:local channel ID pairs.
//    /// </returns>
//    private Dictionary<byte, byte> _GetLocalChannelIds(List<byte> globalIds)
//    {
//        // Dictionary to fill with requested global:local pairs.
//        Dictionary<byte, byte> globalLocalIdMap = new();
//        // Find the local ID corresponding to each global ID.
//        foreach (var globalId in globalIds)
//        {
//            // Attempt to lookup the local ID.
//            if (this._TryConvertGlobalToLocalChannelId(globalId,
//                out byte localId))
//            {
//                // Add global:local ID pair to the output map.
//                globalLocalIdMap.Add(globalId, localId);
//            }
//            // Ignore ID if not found.
//        }
//        // Return the requested subset dictionary.
//        return globalLocalIdMap;
//    }

//    /// <summary>
//    /// Check if an output is in use by any Stimulator resources.
//    /// </summary>
//    /// <param name="globalOutputId">The global output ID of the output to
//    /// check.</param>
//    /// <returns>True if used, False if not or if lookup failed.</returns>
//    public bool IsOutputUsed(int globalOutputId)
//    {
//        return this._globalOutputIdPool.IsUsed(globalOutputId);
//        // Try getting the local output ID.
//        //return (this._globalOutputIdPool.TryGetLocalId(globalOutputId,
//        //            out int localOutputId) &&
//        //    this._outputUses[localOutputId] > 0);
//    }

//    /// <summary>
//    /// Check if there is at least 1 channel available for new use in this
//    /// Stimulator's resource pool.
//    /// </summary>
//    /// <returns>True if there is at least 1 channel available, False if not.
//    /// </returns>
//    public bool HasChannelAvailable()
//    {
//        // There is a channel free if count not down to 0, or if unlimited
//        // channels (negative number).
//        return this.NumFreeChannels != 0;
//    }

//    /// <summary>
//    /// Get a local ID for the next channel available in this Stimulator's
//    /// resource pool. Caller must ensure a channel is available, otherwise
//    /// this method will throw an error.
//    /// </summary>
//    /// <returns>The next available local channel ID which can be used to
//    /// create a new Channel.</returns>
//    private byte _GetNextAvailableChannelId()
//    {
//        // Sorted set, so just return 1st value. Assuming channel free (len>0)
//        return this._freeChannelLocalIds.ElementAt(0);
//    }

//    /// <summary>
//    /// Request a single new Channel be used in this Stimulator.
//    /// </summary>
//    /// <param name="globalChannelId">The global ID to give the new Channel if
//    /// creation is successful.</param>
//    /// <param name="requestedOutputConfig">A dictionary mapping global output
//    /// ID to its corresponding requested assignment value. Valid if all the
//    /// outputs are on this Stimulator and the assignments collectively create 
//    /// a valid assignment for the stimulator.
//    /// </param>
//    /// <param name="pulseParamsRequested">The pulse params requested to be
//    /// modulated by the new Channel. Valid if a subset of pulse params
//    /// available.</param>
//    /// <param name="channel">Output parameter. An ErrorChannel if unable to
//    /// grant the channel request. If successful, a new Channel with the given
//    /// global ID. Its number of outputs will be equal to that of the
//    /// Stimulator which may not be the number requested if fewer outputs were
//    /// requested. The pulse params set will be the subset of those requested
//    /// that this Stimulator can support.</param>
//    /// <returns>True if request successful and granted, False if failed.
//    /// </returns>
//    public bool RequestChannel(byte globalChannelId,
//        Dictionary<byte, OutputConfigEnum> requestedOutputConfig,
//        SortedSet<PhaseParam> pulseParamsRequested, out StimThread channel)
//    {
//        // 1) Validate there are Channels available.
//        if (!this.HasChannelAvailable())
//        {
//            channel = new ErrorChannel($"ERROR: No channels free on requested " +
//                $"stimulator '{this._id}'. Try reallocating.");
//            return false;
//        }

//        // 2) Validate this Stimulator can support the requested number of
//        // outputs.
//        int numOutputsRequested = requestedOutputConfig.Count;
//        if (numOutputsRequested > this.NumOutputs)
//        {
//            channel = new ErrorChannel($"ERROR: Over-requested outputs. " +
//                $"Stimulator '{this._id}' has {this.NumOutputs} but " +
//                $"{numOutputsRequested} requested.");
//            return false;
//        }

//        // 3) Validate at least some of the pulse params requested are supported
//        // by this Stimulator.
//        SortedSet<PhaseParam> pulseParamsUsed = new(pulseParamsRequested
//            .Intersect(this.PulseParamsAvailable));
//        // Error if no requested pulse params supported.
//        if (pulseParamsUsed.Count <= 0)
//        {
//            channel = new ErrorChannel($"ERROR: No requested pulse params " +
//                $"supported by stimulator '{this._id}'." +
//                $"\n\tRequested: " +
//                $"[{string.Join(",", pulseParamsRequested)}]" +
//                $"\n\tSupported: " +
//                $"[{string.Join(",", this.PulseParamsAvailable)}]");
//            return false;
//        }

//        // 4) Designate a new local channel ID.
//        byte localChannelId = this._GetNextAvailableChannelId();

//        // 5) Create a new Channel and store it in the output variable.
//        channel = new StimThread(globalChannelId, (byte)this.NumOutputs,
//            pulseParamsUsed, this.OutputConfigCheckFunction,
//            this.StimParamDataCheckFunction);

//        // _) Set the field changed event callbacks.
//        // TODO: pass these into the Channel constructor instead, but first
//        // need to figure out what their parameter type would be (some pre-
//        // existing EventX delegate?)
//        // TODO: also figure out nullability warning  vvv
//        channel.OutputConfigChanged += this.c_ChannelOutputConfigChanged;
//        channel.StimParamDataChanged += this.c_ChannelStimParamDataChanged;

//        // 6) Form and set the requested output config of the new Channel.
//        // a) Create the empty output config enum array (all UNUSED to start).
//        var outputConfig = new OutputConfigEnum[this.NumOutputs];
//        Array.Fill(outputConfig, OutputConfigEnum.UNUSED);
//        // b) Store each specified output assignment.
//        foreach (var globalId in requestedOutputConfig.Keys.ToList())
//        {
//            // Try converting to local ID.
//            if (this._globalOutputIdPool.TryGetLocalId(globalId, out int localId))
//            {
//                // Store assignment is valid ID.
//                outputConfig[localId] = requestedOutputConfig[globalId];
//            }
//            // If invalid ID, error indicating as much.
//            else
//            {
//                channel = new ErrorChannel($"ERROR: Invalid output ID '{globalId}'.");
//                return false;
//            }
//        }
//        // c) Attempt to set the output config of the Channel. Error if failed.
//        if (!channel.SetOutputConfig(outputConfig))
//        {
//            // Return error if output assignment is invalid.
//            channel = new ErrorChannel($"ERROR: Invalid output config: " +
//                $"[{string.Join(",", outputConfig)}]");
//            return false;
//        }

//        // 7) Propagate the new Channel assignment to the Stimulator itself.
//        if (!this.HW_AddChannel(localChannelId, channel.PulseParamsUsed,
//            channel.OutputConfig))
//        {
//            // Return error if Stimulator couldn't add channel.
//            channel = new ErrorChannel($"ERROR: Stimulator '{this._id}' " +
//                $"could not add channel.");
//            return false;
//        }

//        // 8) If Channel successfully created, set, and propagated, adjust
//        // internal fields.
//        this._channels.Add(localChannelId, channel);
//        this._globalLocalChannelIdMap.Add(globalChannelId, localChannelId);
//        this._usedChannelLocalIds.Add(localChannelId);

//        // 9) Return success.
//        return true;
//    }

//    /// <summary>
//    /// Free a channel from use.
//    /// </summary>
//    /// <param name="globalChannelId">The global ID of the channel to free.
//    /// </param>
//    /// <returns>True if succeeded, False if failed.</returns>
//    public bool FreeChannel(byte globalChannelId)
//    {
//        // Get the local channel ID. Only act if global channel ID valid.
//        if (this._TryConvertGlobalToLocalChannelId(globalChannelId,
//            out byte localChannelId))
//        {
//            // 1) Free the channel in Stimulator firmware.
//            // TODO: this should be done in the derived class, not here bc may be FW specific
//            // Build the UNUSED output assignment array.
//            OutputConfigEnum[] outputConfig = Enumerable.Repeat(
//                OutputConfigEnum.UNUSED, this.NumOutputs).ToArray();
//            // Reconfigure channel on stimulator with UNUSED output config.
//            if (this.HW_ReconfigureChannelOutputs(localChannelId,
//                outputConfig))
//            {
//                // 2) If the actual free succeeded, remove the channel from 
//                // internal fields.
//                this._channels.Remove(localChannelId);
//                this._globalLocalChannelIdMap.Remove(globalChannelId);
//                this._usedChannelLocalIds.Remove(localChannelId);

//                // Return success.
//                return true;
//            }
//        }

//        // Return failure (i.e., global ID invalid or FW free failed).
//        return false;
//    }

//    /// <summary>
//    /// Get an aggregate representation in matrix form of all output
//    /// assignments in this Stimulator.
//    /// </summary>
//    /// <returns>The matrix of (channel x output) output assignments (enum
//    /// values as ints). Local channel and output IDs used as row and column
//    /// indices, respectively.</returns>
//    public Matrix<int> GetOutputConfigMatrix()
//    {
//        // Aggregate all Channel output config arrays.
//        List<List<int>> allConfigs = new();
//        // Iter over Channels. Local ID ascending order ensured by SortedDict.
//        foreach (var (_, channel) in this._channels)
//        {
//            // Convert each enum value in the Channel's config to an int.
//            List<int> outputConfig = new();
//            foreach (var enumValue in channel.OutputConfig)
//            {
//                outputConfig.Add((int)enumValue);
//            }
//            // Store this Channel's config in the aggregate list.
//            allConfigs.Add(outputConfig);
//        }
//        // Return the matrix of this aggregate config data.
//        return Matrix<int>.Build.DenseOfRows(allConfigs);
//    }

//    /// <summary>
//    /// Get an aggregate representation in matrix form of all stim param data
//    /// for this Stimulator.
//    /// </summary>
//    /// <returns>The matrix of (channel x pulse param) stim param data values.
//    /// Local channel and output IDs used as row and column indices,
//    /// respectively.</returns>
//    public Matrix<double> GetStimParamDataMatrix()
//    {
//        // Aggregate all Channel output config arrays.
//        List<Vector<double>> allData = new();
//        // Iter over Channels. Local ID ascending order ensured by SortedDict.
//        foreach (var (_, channel) in this._channels)
//        {
//            // Store this Channel's stim data in the aggregate list.
//            allData.Add(channel.StimData);
//        }
//        // Return the matrix of this aggregate data.
//        return Matrix<double>.Build.DenseOfRowVectors(allData);
//    }


//    /// <summary>
//    /// A callback method for events marking a channel's output config has
//    /// changed. Propagate the change to the stimulator device if the channel
//    /// that raised the event is still active and has not been freed, and if the
//    /// Stimulator is in the correct mode to allow propagation to the device.
//    /// </summary>
//    /// <param name="sender">The channel object that raised the event.</param>
//    /// <param name="e">The channel output config changed event data.</param>
//    private void c_ChannelOutputConfigChanged(object sender,
//        StimThreadOutputConfigChangedEventArgs e)
//    {
//        // Safely propagate the output config to the stimulator device.
//        this.ReconfigureChannelOutputs(e.GlobalStimThreadId, e.OutputConfig);
//    }

//    /// <summary>
//    /// A callback method for events marking a channel's stim param data has
//    /// changed. Propagate the change to the correct stimulator if the channel
//    /// that raised the event is still active and has not been freed, and if the
//    /// Stimulator is in the correct mode to allow propagation to the device.
//    /// </summary>
//    /// <param name="sender">The channel object that raised the event.</param>
//    /// <param name="e">The channel stim param data changed event data.</param>
//    private void c_ChannelStimParamDataChanged(object sender,
//        StimThreadStimParamDataChangedEventArgs e)
//    {
//        //  Propagate the output config to the stimulator device.
//        this.UpdateStim(e.GlobalChannelId, e.PulseParamsUsed, e.StimParamData);
//    }


//    // NOTE: the only methods this abstracted Stimulator cares to define (and
//    // thus require of its inheriting subclasses) are those internal-facing. This
//    // becomes the contract with the outside world of what any stimulator will
//    // be able to do and provide. Each individual, specifically implemented
//    // subclass can take care of its own message/packet handling (e.g., format,
//    // check, and send msg, along with all the specific byte codes).

//    /// <summary>
//    /// Device-specific implementation to start using a channel that is
//    /// currently not used. The stimulator will perform whatever operations are
//    /// needed to support use of this new Channel.
//    /// </summary>
//    /// <param name="localChannelId">The local channel ID the stimulator should 
//    /// use to refer to its own new used channel.</param>
//    /// <param name="pulseParamsUsed">The set of pulse params this channel uses.
//    /// Must be a subset if this Stimulator's available pulse params, but may
//    /// not be the full set.</param>
//    /// <param name="outputConfig">The array of output config assignments.
//    /// </param>
//    /// <returns>True if channel added successfully, False if not.</returns>
//    protected abstract bool HW_AddChannel(byte localChannelId,
//        SortedSet<PhaseParam> pulseParamsUsed,
//        OutputConfigEnum[] outputConfig);

//    /// <summary>
//    /// Change the outputs configured under a given existing, in-use channel.
//    /// Data will be automatically propagated to the stimulator device itself
//    /// if this Stimulator is in the correct mode.
//    /// </summary>
//    /// <param name="globalChannelId">The global channel ID.</param>
//    /// <param name="outputConfig">The array of new output config assignments.
//    /// Data security against source data changes by other threads must be
//    /// ensured by the caller.
//    /// </param>
//    /// <returns>True if outputs of the channel reconfigured successfully,
//    /// False if not.</returns>
//    internal bool ReconfigureChannelOutputs(byte globalChannelId,
//        OutputConfigEnum[] outputConfig)
//    {
//        // Conjunctive AND (&&) condition evaluates clauses in order and
//        // terminates early upon failure.
//        bool success =
//            // 1) Try to lookup the local channel ID.
//            this._TryConvertGlobalToLocalChannelId(globalChannelId,
//                out byte localChannelId) &&
//            // 2) Make sure in correct "push to device" mode.
//            this._pushConfigData &&
//            // 3) Try to push output config to the stimulator device.
//            this.HW_ReconfigureChannelOutputs(localChannelId,
//                outputConfig);

//        // If successful, store the last sent data.
//        if (success)
//        {
//            this._channels[localChannelId]
//                .UpdateLastSentOutputConfig(outputConfig);
//        }

//        // Return the outcome.
//        return success;
//    }

//    /// <summary>
//    /// Device-specific implementation to change the outputs configured under an
//    /// existing, in-use channel.
//    /// </summary>
//    /// <param name="localChannelId">The local channel ID.</param>
//    /// <param name="outputConfig">The array of new output config assignments.
//    /// </param>
//    /// <returns>True if outputs of the channel reconfigured successfully,
//    /// False if not.</returns>
//    protected abstract bool HW_ReconfigureChannelOutputs(byte localChannelId,
//        OutputConfigEnum[] outputConfig);

//    // TODO: when to use these functions? need to expose via SMgr
//    internal void StartSendingCommands()
//    {
//        this._pushConfigData = this._pushStimParamData = true;
//    }
//    internal void StopSendingCommands()
//    {
//        this._pushConfigData = this._pushStimParamData = false;
//    }

//    // TODO: decide how to use the wrapper methods for start/stop stim. Consider
//    // introducing 'active', etc states in Stimulator and set/use them here?
//    internal void StartStim()
//    {
//        // TODO: should this set _pushStimParaData and/or _stimOn?
//        this._stimOn = this._pushStimParamData = true;
//    }
//    internal void StopStim()
//    {
//        // TODO: should this set _pushStimParaData and/or _stimOn?
//        this._stimOn = this._pushStimParamData = false;
//    }

//    protected abstract void HW_StartStim();
//    protected abstract void HW_StopStim();

//    /// <summary>
//    /// Change the stimulation parameter values on a given existing, in-use
//    /// channel. Data will be automatically propagated to the stimulator device
//    /// itself if this Stimulator is in the correct mode.
//    /// </summary>
//    /// <param name="globalChannelId">The global channel ID.</param>
//    /// <param name="pulseParamsUsed">The sorted set of pulse params used.
//    /// </param>
//    /// <param name="stimParamData">The vector of stim param data values,
//    /// ordered in alignment with the pulse params used. Data security against
//    /// source data changes by other threads must be ensured by the caller.
//    /// </param>
//    internal bool UpdateStim(byte globalChannelId,
//        SortedSet<PhaseParam> pulseParamsUsed, Vector<double> stimParamData)
//    {
//        // Conjunctive AND (&&) condition evaluates clauses in order and
//        // terminates early upon failure.
//        bool success =
//            // 1) Try to lookup the local channel ID.
//            this._TryConvertGlobalToLocalChannelId(globalChannelId,
//                out byte localChannelId) &&
//            // 2) Make sure in correct "push to device" mode.
//            this._pushStimParamData &&
//            // 3) Try to push stim param data to the stimulator device.
//            this.HW_UpdateStim(localChannelId, pulseParamsUsed, stimParamData);

//        // If successful, store the last sent data.
//        if (success)
//        {
//            this._channels[localChannelId]
//                .UpdateLastSentStimParamData(stimParamData);
//        }

//        // Return the outcome.
//        return success;
//    }

//    /// <summary>
//    /// Device-specific implementation to change stimulation parameter values
//    /// on a given channel.
//    /// </summary>
//    /// <param name="localChannelId">The the stimulator device context ID of
//    /// the channel to update.</param>
//    /// <param name="pulseParamsUsed">The set of pulse params this channel uses.
//    /// Must match the parameter set the stimulator device already has
//    /// configured for the channel.</param>
//    /// <param name="stimParamData">The new stimulation parameter values to set
//    /// on the given channel.</param>
//    protected abstract bool HW_UpdateStim(byte localChannelId,
//        SortedSet<PhaseParam> pulseParamsUsed, Vector<double> stimParamData);

//    // TODO: safety check(s). very much a work-in-progress. What does it take
//    // in? different safety check functions? string key based switch statement
//    // or does it accept the specific safety function as a delgate? what are the
//    // bounds applied and to what params,outputs and/or channels?
//    // protected abstract void ApplySafetyBounds();

//    /// <summary>
//    /// Wrapper method for sending messages to the stimulator device itself,
//    /// but while ensuring a lock on the communication line.
//    /// </summary>
//    /// <param name="data">The full byte array of data to send.</param>
//    protected void SendMessage(byte[] data)
//    {
//        lock (this._commsLock)
//        {
//            this.HW_SendMessage(data);
//        }
//    }

//    /// <summary>
//    /// Device-specific implementation to send a message to the stimulator
//    /// device itself.
//    /// </summary>
//    /// <param name="data">The full byte array of data to send.</param>
//    protected abstract void HW_SendMessage(byte[] data);

//}

