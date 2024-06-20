using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Core.Stimulation;


public record ThreadConfigDataPerStimulator(int GlobalStimId,
    IEnumerable<Lead> IndependentLeads,
    Dictionary<string, Tuple<IDataLimits, object>> StimParamData,
    // TODO: edit once have sorted how will resolve StimParams and enum stuff.
    SortedSet<string> ModulatableStimParams);

