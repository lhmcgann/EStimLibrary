using EStimLibrary.Core.Stimulation.Data;
using EStimLibrary.Core.Stimulation.Functions;
using EStimLibrary.Core.Stimulation.Trains;


namespace EStimLibrary.Core.Stimulation.Stimulators;


// This is an abstract class rather than an interface because it provides a base
// class implementation rather than extension capabilities. The abstract class
// allows this Stimulator super class to have instantiable class fields.
public abstract class Stimulator : ISelectable, IIdentifiable
{
    // TODO: once actually have apps and things running both in and out of this
    // assembly, re-discuss which access modifiers desired here vs in derived
    // classes, etc
    public abstract string Name { get; }    // ISelectable
    // Manager-given ID of the stimulator.
    public int Id { get; internal set; }    // IIdentifiable

    // TODO: redo how these are structured, implemented, used, etc w/ stim data
    // structs and stim param limit stuff
    // Data check functions.
    // A variable to store the function used to validate a stimulator's output
    // configs. See C# delegates and Stimulators.Functions.OutputConfigChecks.
    protected abstract ValidateOutputConfigDelegate OutputConfigCheckFunction
    { get; }
    // A variable to store the function used to validate a stimulator's param
    // data. See C# delegates and Stimulators.Functions.StimParamDataChecks.
    protected abstract ValidateStimParamDataDelegate StimParamDataCheckFunction
    { get; }

    // Properties for: stim pulse params supported by the stimulator; number
    // of total outputs; max number of output configs allowed.
    // Derived classes must implement the get() of the abstract properties.
    // e.g., SpecificStimulator.NumOutputs get { return constNumOutputs; }.

    public abstract Dictionary<string, Tuple<IDataLimits, object>> StimParamData
    { get; }
    public abstract SortedSet<string> ModulatableStimParams
    { get; }
    // Fixed-value params must be all params available that aren't modulatable.
    public SortedSet<string> FixedStimParams =>
        new(this._StimParamsAvailable.Except(this.ModulatableStimParams));
    //public abstract Dictionary<StimParamType, double> FixedStimParamValues
    //{ get; }
    // Essentially a validation check on stim param specification after
    // Stimulator construction.
    public bool ValidStimParamSpecification =>
        // a) at least the base stim params are included in the enum.
        BaseStimParams.ParamOrderIndices.Keys.All(
            this.StimParamData.Keys.Contains) &&
        // b) all available parameters are from the same enum
        this.StimParamData.Keys.Select(param => param.GetType())
            .Distinct().Count() == 1;

    // Instantiated in constructor based on the keys in StimParamData.
    protected readonly SortedSet<string> _StimParamsAvailable;
    public SortedSet<string> StimParamsAvailable => this._StimParamsAvailable;
    //new SortedSet<Enum>((Enum[])Enum.GetValues(typeof(StimParamType)));
    // TODO: look at NumOutputs-related bugfix in testing branch
    public abstract int NumOutputs { get; }
    public abstract int MaxNumOutputConfigs { get; }
    // Local output ID pool. TODO: if/when to mark as used?
    protected ReusableIdPool _OutputIds;
    // Array to mark uses of each output, indexed by local output ID.
    private int[] _outputUses;

    // Data structs regarding output configs themselves.
    private readonly ReusableIdPool _globalOutputConfigIdPool;
    // Dictionary storing full-length config arrays themselves, len<=maxNum,
    // keyed by output config ID (local only, so [0, maxNum)).
    private Dictionary<int, Constants.OutputAssignment[]> _outputConfigs;

    // A lock on the communicationn line for message sending.
    private readonly object _commsLock = new();

    // Fields for controlling stimulator modes. TODO: this overall. enum?
    private bool _pushConfigData;
    private bool _pushStimParamData;
    private bool _stimOn;

    /// <summary>
    /// Base initialization functionality for any Stimulator. Derived classes
    /// must call this constructor in addition to their own construction
    /// functionality.
    /// </summary>
    /// <param name="baseOutputConfigId">The base value to add each local output
    /// config ID to in order to get the corresponding global output config ID.
    /// Global output config IDs will be assigned to the Stimulator in a block
    /// starting at this value.
    /// </param>
    protected Stimulator()//, int baseOutputConfigId)
    {
        // Init stim ID as not set yet.
        this.Id = -1;

        // Store the param options.
        this._StimParamsAvailable = new(this.StimParamData.Keys);

        // Init empty output config dict and array of used markings per output.
        this._outputConfigs = new();
        this._outputUses = Enumerable.Repeat(0, this.NumOutputs).ToArray();
        this._OutputIds = new(0, this.NumOutputs);

        // Init "mode" booleans to false.
        this._pushConfigData = this._pushStimParamData = this._stimOn = false;
    }

    /// <summary>
    /// Verify that a given group of outputs can be wired in a single lead.
    /// </summary>
    /// <param name="localOutputIds">The list of outputs proposed to be wired
    /// by a single lead. Local IDs, assumed to be valid for this Stimulator.
    /// </param>
    /// <returns></returns>
    public abstract bool IsValidOutputWiring(IEnumerable<int> localOutputIds);

    public bool IsValidParamValue(string stimParam, object paramValue)
    {
        var (paramLims, defaultVal) = this.StimParamData[stimParam];
        return paramLims.IsValidDataValue(paramValue);
    }

    // MAIN UPDATE STIM METHOD PASSED TO EACH THREAD DURING CONFIG AND CALLED
    // BY TRANSDUCER ON UPDATE
    public bool UpdateStim(object state)
    {
        // TODO: there is probably a better way to do this global-local output
        // conversion, but I don't know how right now. ALSO depends on if
        // allowing any new output config to come in, or if having to pass a
        // selector byte/ID if limited number of configs allowed.
        //var data = (Tuple<IEnumerable<Train>, Dictionary<int, int>>)state;
        var data = ((IEnumerable<Dictionary<string, object>>,
            Dictionary<int, Constants.OutputAssignment>))state;
        var trainsParams = data.Item1;
        var localOutputAssignments = data.Item2;
        return this.HW_UpdateStim(trainsParams, localOutputAssignments);
    }
    //protected abstract bool HW_UpdateStim(IEnumerable<Train> stimTrains,
    //    Dictionary<int, int> globalToLocalOutputIds);
    protected abstract bool HW_UpdateStim(
        IEnumerable<Dictionary<string, object>> trainsParams,
        Dictionary<int, Constants.OutputAssignment> localOutputAssignments);

    // TODO: when to mark outputs as used vs free? in UpdateStim? but only if
    // the HW_UpdateStim returns true, i.e., data passes checks and able to
    // send to stim HW? How keep track of all configs across stim updates
    // though, bc output being used is across the whole scope...
    /// <summary>
    /// Check if an output is in use by any Stimulator resources.
    /// </summary>
    /// <param name="localOutputId">The local output ID of the output to
    /// check.</param>
    /// <returns>True if used, False if not or if lookup failed.</returns>
    public bool IsOutputUsed(int localOutputId)
    {
        return this._OutputIds.IsUsed(localOutputId);
        // Try getting the local output ID.
        //return (this._globalOutputIdPool.TryGetLocalId(globalOutputId,
        //            out int localOutputId) &&
        //    this._outputUses[localOutputId] > 0);
    }

    /// <summary>
    /// Set default output configurations in the hardware.
    /// </summary>
    /// <param name="defaultConfig">A dictionary representing the output config
    /// set by default. Keys are global output IDs. Value are the full-length
    /// config array corresponding to that output.</param>
    /// <returns></returns>
    public abstract bool SetDefaultOutputConfigs(
        out Dictionary<int, Constants.OutputAssignment[]> defaultConfig);


    // NOTE: the only methods this abstracted Stimulator cares to define (and
    // thus require of its inheriting subclasses) are those internal-facing. This
    // becomes the contract with the outside world of what any stimulator will
    // be able to do and provide. Each individual, specifically implemented
    // subclass can take care of its own message/packet handling (e.g., format,
    // check, and send msg, along with all the specific byte codes).


    // TODO: when to use these functions? need to expose via SMgr
    internal void StartSendingCommands()
    {
        this._pushConfigData = this._pushStimParamData = true;
    }
    internal void StopSendingCommands()
    {
        this._pushConfigData = this._pushStimParamData = false;
    }

    // TODO: decide how to use the wrapper methods for start/stop stim. Consider
    // introducing 'active', etc states in Stimulator and set/use them here?
    internal void StartStim()
    {
        this.HW_StartStim();
        // TODO: should this set _pushStimParaData and/or _stimOn?
        this._stimOn = this._pushStimParamData = true;
    }
    internal void StopStim()
    {
        // TODO: should this set _pushStimParaData and/or _stimOn?
        this._stimOn = this._pushStimParamData = false;
        this.HW_StopStim();
    }

    protected abstract void HW_StartStim();
    protected abstract void HW_StopStim();

    // TODO: safety check(s). very much a work-in-progress. What does it take
    // in? different safety check functions? string key based switch statement
    // or does it accept the specific safety function as a delgate? what are the
    // bounds applied and to what params,outputs and/or channels?
    // protected abstract void ApplySafetyBounds();

    /// <summary>
    /// Wrapper method for sending messages to the stimulator device itself,
    /// but while ensuring a lock on the communication line.
    /// </summary>
    /// <param name="data">The full byte array of data to send.</param>
    protected void SendMessage(byte[] data)
    {
        lock (this._commsLock)
        {
            this.HW_SendMessage(data);
        }
    }

    /// <summary>
    /// Device-specific implementation to send a message to the stimulator
    /// device itself.
    /// </summary>
    /// <param name="data">The full byte array of data to send.</param>
    protected abstract void HW_SendMessage(byte[] data);

}

