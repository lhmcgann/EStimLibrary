using EStimLibrary.Core.Stimulation;
using EStimLibrary.Core.Stimulation.Data;
using EStimLibrary.Core.Haptics;
using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Core;


namespace EStimLibrary.Extensions.Haptics;


public class ClassicDirectTransducer : HapticTransducer
{
    public override string Name => "ClassicDirectModulation";

    public string ModulatedParam { get; init; }

    // TODO: make this a factory create somehow? at least other validation of
    // input modulatable phase param?
    public ClassicDirectTransducer(string modParam)
    {
        // TODO: adjust this validation to be flexible to user-input param
        // lists.
        // TODO: how to also factor in stimulator-specific modulation abilities?
        // e.g., even if valid param name, the stimulator used for a given event
        // may not be able to mod it...
        if (!BaseStimParams.ParamOrderIndices.Keys.Contains(modParam))
        {
            throw new ArgumentException($"{this.Name} Constructor Error: " +
                $"{modParam} is not a valid stim param.");
        }
        this.ModulatedParam = modParam;
    }

    /// <summary>
    /// Transduce a haptic event into stimulation data.
    /// Make the desired stim data updates given a haptic event, the relevant
    /// spatial info, and the associated stim thread pool(s).
    /// </summary>
    /// <param name="hapticEvent">The haptic event to transduce.</param>
    /// <param name="fullyContainingAreas">The ID-keyed areas fully containing
    /// the event. IDs are local to the body model on which the event happened.
    /// </param>
    /// <param name="partiallyContainingAreas">The ID-keyed areas partially
    /// containing the event. IDs are local to the body model on which the event
    /// happened.</param>
    /// <param name="localizedThreads">The StimThreads associated with
    /// each ID-keyed area given in fullyContainingAreas and
    /// partiallyContainingAreas. This must contain an entry for each area in
    /// those two params.</param>
    /// <returns>The StimThreads actually used, i.e., in which stim data was
    /// changed.</returns>
    protected override IEnumerable<StimThread> _TransduceHapticEvent(
        HapticEvent hapticEvent,
        Dictionary<int, IArea> fullyContainingAreas,
        Dictionary<int, IArea> partiallyContainingAreas,
        Dictionary<int, StimThread> localizedThreads)
    {
        // P should be the first value in the haptic param vector. Even if not,
        // i.e., P not included at all (otherwise would be first), use the first
        // value.
        // Should be between 0 and 1
        double modValue = hapticEvent.HapticParamData[0];

        // StimThread properties:
        // PerStimulatorConfigs (MAIN constructor input; next 2 derive from it)
        //  Dict<stimId, ThreadConfig>
        //  where ThreadConfig:
        //      stimId,
        //      IEnum<Lead> IndpLeads,
        //      Dictionary<string, Tuple<IDataLimits, object>> StimParamData
        //      SortedSet<string> ModulatableStimParams
        // OutputsPerStimulator
        // LeadsPerStimulator

        // TODO: Per StimThread, should namely set PulseTrainsPerStimulator
        //  Dict<stimId, IEnum<Train>>

        List<StimThread> stimThreadsUsed = new();

        // Only consider areas (and associated threads) fully containing the
        //  event unless there are none, then use the partials.
        var relevantAreas = (fullyContainingAreas.Count > 0) ?
            fullyContainingAreas : partiallyContainingAreas;
        foreach (var (areaId, area) in relevantAreas)
        {
            // Lookup the corresponding stim thread.
            var stimThread = localizedThreads[areaId];

            // Add the stim thread to the list of those used.
            stimThreadsUsed.Add(stimThread);

            // Create a stim train from Stimulator-specific default values,
            // except for a modulated pulse param.
            foreach (var (stimId, threadConfig) in
                stimThread.PerStimulatorConfigs)
            {
                // Extract per-stimulator info.
                var (_, indpLeads, stimParamData, modulatableStimParams) =
                    threadConfig;

                // TODO: is this violating coherence, etc OOP rules? should this
                //  be relegated to each stimulator's UpdateStim call? So only
                //  stimulator has to know its own limits? but then how to limit
                //  transducer params options by stim limits... larger design
                //  dilemma of single "omniscient" transducer or one per thread?
                //  Per train?
                // Skip this stimulator if:
                //  param specified is not modulatable,
                //  param data cannot be retrieved
                //  param is not a continuous data type
                if (!modulatableStimParams.Contains(this.ModulatedParam) ||
                    !stimParamData.TryGetValue(this.ModulatedParam, out var paramDataTuple) ||
                    paramDataTuple.Item1.GetType() != typeof(ContinuousDataLimits))
                {
                    continue;
                }

                // Get the scaled value of the modulated param.
                var paramLimits = (ContinuousDataLimits)paramDataTuple.Item1;
                var paramValue = Utils.ScaleValue(modValue,
                    paramLimits.MinBound, paramLimits.MaxBound);

                // Get default values for the remaining params.
                // Resulting dictionary: {paramName (str): defaultVal (object)}
                var paramValues = stimParamData
                    .Where(kvp => kvp.Key != this.ModulatedParam)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Item2);

                // Add the modulated param value.
                paramValues.Add(this.ModulatedParam, paramValue);

                // Store these per-stimulator train params in the stim thread.
                stimThread.TrainParamsPerStimulator[stimId] =
                    new List<Dictionary<string, object>>() { paramValues };

                // TODO: do I need the data structs at all? Or can I leave as
                // dictionary of values? Bc otherwise would need to decode,
                // stuff structs, then stimulators unstuff and re-encode...

                // Generate the train for this stimulator.
                // Base stim params include: PA, PW, PhaseShape, IPD,
                //  AnodeRatio, AnodeFirst, Period, FixedRepeats.
                // Phase params: OutputConfiguration OutputConfig,
                //  IPhaseData PhaseData
                // IPhaseData is ISelectable
                // BiphasicPulse params: Phase givenPhase, double ipd,
                //  double anodeRatio, bool anodeFirst
            }
        }

        //throw new NotImplementedException();
        return stimThreadsUsed;
    }
}

