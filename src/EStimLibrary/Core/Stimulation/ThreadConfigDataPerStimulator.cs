using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Core.Data;


namespace EStimLibrary.Core.Stimulation;


public record ThreadConfigDataPerStimulator(int GlobalStimId,
    IEnumerable<Lead> IndependentLeads,
    Dictionary<string, Tuple<IDataLimits, object>> StimParamSpecs,
    // TODO: edit once have sorted how will resolve StimParams and enum stuff.
    SortedSet<string> ModulatableStimParams);

