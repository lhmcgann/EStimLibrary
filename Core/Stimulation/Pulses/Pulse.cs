using EStimLibrary.Core.Stimulation.Phases;


namespace EStimLibrary.Core.Stimulation.Pulses;


/// <summary>
/// A Pulse is a sequence of Phases separated by inter-phase delays, or IPDs,
/// additionally parameterized by other specified PulseParams.
/// TODO: use said PulseParams or StimParams or whatever?
/// </summary>
/// NOTE: this primary/default constructor should not be called, instead call
/// the custom constructor declared below. The _validated parameter in this
/// constructor is to help enforce this. The custom constructor will pass this
/// parameter as 'true'. The primary constructor is still declared here though
/// to enable the built-in, auto-generated implementations of methods related to
/// equality checking and deconstruction. Pulse instances will be compared
/// for value-based equality based on the parameters listed in this primary
/// constructor.
public record Pulse(List<Phase> Phases, List<double> IPDs,
    bool _validated)
{
    public int NumPhases { get => this.Phases.Count; }

    public double PulseWidth
    {
        get
        {
            // Start by getting the first phase's width.
            var pulseWidth = this.Phases[0].PhaseData.PhaseWidth;
            // If there are more phases, add the preceding IPD, then its width.
            for (int i = 1; i < this.NumPhases; i++)
            {
                pulseWidth += this.IPDs[i] +
                    this.Phases[i].PhaseData.PhaseWidth;
            }
            return pulseWidth;
        }
    }

    /// <summary>
    /// Create a new Pulse from a series of Phases and IPDs.
    /// </summary>
    /// <param name="phases">The Phases in desired sequence order. Deep copies
    /// ensured.</param>
    /// <param name="ipds">The IPD values (us) in desired sequence order. Should
    /// be n-1 IPD values compared to phases. Any extra IPDs will be ignored.
    /// </param>
    public Pulse(IEnumerable<Phase> phases, IEnumerable<double> ipds) :
        this(new(phases), new(ipds.Take(phases.Count() - 1)), _validated: true)
    {
        // TODO: how to ensure true deep copy of Phase objects? make their type
        // something that's pass-by-value not reference? is having them be
        // records sufficient?
    }
}

