using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Core.Stimulation.Trains;


namespace EStimLibrary.Core.Stimulation;


/*
 * Each lead is only connected to 1 Stimulator
 * 
 * Each StimThread pertains to a single ContactPool set at construction.
 * The ContactPool may correspond to outputs on multiple Stimulators based on 
 * the Lead wiring, but there will be at least 2 independent Leads per involved 
 * Stimulator, per ContactPool validation.
 * The ContactPool may be wired by overlapping Leads, meaning some Leads may 
 * contain some of the same outputs and/or contacts.
 * Any two or more independent Leads wired to the same stimulator can be in a 
 * single output configuration, i.e., such that there is at least one output set 
 * to act as a current source, and at least one output set to act as a current 
 * sink.
 * TODO: how to get to sendable output configurations? what is allowed as an 
 * output configuration?
 * TODO: output configurations and pulse (and phase; TODO: and train? but trains
 * aren't within a StimThread's scope...) params should pertain to each 
 * Stimulator.
 * **/

// TODO: move this delegate to be with the other delegates somewhere?
public delegate void StimDataUpdateHandler(StimThread stimThread);


public class StimThread : Claimable
{
    // TODO: when/how to use the locking mechanisms offered by being a
    // Claimable? since don't want to be copying data everytime something is
    // changed, stim data stored as objects --> how to lock at all levels (bc if
    // objects, they're used via references which can be accessed elsewhere)?
    // OR do we just concede and make all stim data records so copied on every
    // data change (even though that's highly inefficient and not what records
    // are designed for)?

    // The event to trigger when stim data in this thread is updated.
    public event StimDataUpdateHandler StimDataUpdated;

    //public ContactPool ContactPool { get; init; }
    // { stimId: {[leads], [availableParams]} }
    public Dictionary<int, ThreadConfigDataPerStimulator> PerStimulatorConfigs
    { get; init; }
    // { stimId: [pulseTrains] }    TODO: delete if forego stim data structs
    public Dictionary<int, IEnumerable<Train>> PulseTrainsPerStimulator
    { get; set; }
    // {stimId: [train1: {param: value}, train2: {param: value}] }
    public Dictionary<int, IEnumerable<Dictionary<string, object>>> TrainParamsPerStimulator
    { get; set; }


    // TODO: are these properties needed?
    // {stimId: {Leads}}
    public Dictionary<int, HashSet<Lead>> LeadsPerStimulator =>
        this.PerStimulatorConfigs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IndependentLeads.ToHashSet());
    // {stimId: {globalOutputIds}}
    // NOTE: this informatino is what the transducer can use to set
    // OutputConfigs
    public Dictionary<int, SortedSet<int>> OutputsPerStimulator =>
        this.PerStimulatorConfigs.ToDictionary(
            kvp => kvp.Key,
            // Get all outputs per stim. 1) Get all relevant leads of this stim.
            kvp => kvp.Value.IndependentLeads
            // 2) Get the output sets of each lead.
            .Select(lead => lead.OutputSet)
            // 3) Aggregate all per-lead output sets into one set per stim.
            .Aggregate((outputsPerStim, outputsPerLead) =>
                new(outputsPerStim.Union(outputsPerLead))));
    // {globalOutputIds}
    public SortedSet<int> AllOutputs => this.OutputsPerStimulator
        .Select(kvp => kvp.Value)
        .Aggregate((allOutputs, perStimOutputs) =>
            new(allOutputs.Union(perStimOutputs)));


    public StimThread(//SortedSet<int> contactPool,
        Dictionary<int, ThreadConfigDataPerStimulator> perStimConfigData,
        StimDataUpdateHandler updateCallback)
    {
        //this.ContactPool = new(contactPool);
        this.PerStimulatorConfigs = new(perStimConfigData); // TODO: deep copy?
        this.PulseTrainsPerStimulator = new();
        this.TrainParamsPerStimulator = new();

        // Set up the callback method for the stim data changed event.
        this.StimDataUpdated += updateCallback;
    }

    // TODO: have stim data and output config validation functions here or just
    // in the Stimulator?
    //// {stimId: ValidateOutputConfigDelegate}
    //public Dictionary<int, ValidateOutputConfigDelegate>
    //    OutputConfigCheckFunctionsPerStimulator
    //{ get; init; }
    //// {stimId: ValidateStimParamDataDelegate}
    //public Dictionary<int, ValidateStimParamDataDelegate>
    //    StimDataCheckFunctionsPerStimulator
    //{ get; init; }


    // TODO: need this or not? depends on resolution of overall enum/stimParams
    // problem
    //public SortedSet<PhaseParam> PulseParamsUsed { get; private set; }


    // TODO: params to select subsets of per-stim train groups or even indv
    // trains?
    public void SendUpdatedStimData()
    {
        // 2) once get some confirmation(s) the data is sent to HW, lock
        // the lastSentPulse
        // 3) copy newPulse to lastSentPulse
        // 4) release the lastSentPulse lock

        // TODO: lock trains until receive confirmation from Stim that deep
        // copy of data made? and/or will whatever context is calling this
        // method already have a lock on this thread / deeper data levels?

        // Raise the data changed event and invoke the callback, passing this
        // StimThread as a parameter to the configured event callback function.
        this.StimDataUpdated?.Invoke(this);
    }

    //// TODO: actually use callbacks upon data change, or use like what is
    ///above: pre-stored "callback" functions that are explicitly called rather
    ///than automatically triggered
    ///if do decide to use, need to re-determine should be initialized (check
    //// old Channel constructor...)
    //// Event handlers for when output config and stim param data fields changed.
    //public event StimThreadOutputConfigChangedEventHandler OutputConfigChanged;
    //public event StimThreadStimParamDataChangedEventHandler StimParamDataChanged;
    //// Declare an EventArgs for the output config changed event callback.
    //StimThreadOutputConfigChangedEventArgs eventArgs;
    //    // Raise an event marking this StimThread's output config was changed.
    //    this.OutputConfigChanged?.Invoke(this, eventArgs);
}

