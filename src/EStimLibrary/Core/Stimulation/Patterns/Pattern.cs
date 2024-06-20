using EStimLibrary.Core.Stimulation.Phases;
using EStimLibrary.Core.Stimulation.Pulses;


namespace EStimLibrary.Core.Stimulation.Patterns;


/// <summary>
/// A stimulation pattern is a sequence of stimulation pulses within a single
/// period. The pattern can be modulated by a function over iterations (periods)
/// of the pattern, which is then considered to be a stimulation or pulse train.
/// Pulses within the pattern period are specified by the stimulation pulse
/// itself and a time delay from the beginning of the period at which the pulse
/// starts. Pulses in a single pattern must have a positive, non-zero
/// inter-pulse interval (IPI) separating them. All pulses must fit within the
/// pattern period, so total pulse widths (all phase widths and IPDs considered)
/// and pulse delays from the start of the pattern must be valued accordingly.
/// </summary>
/// NOTE: this primary/default constructor should not be called, instead call
/// the custom constructor declared below. The _validated parameter in this
/// constructor is to help enforce this. The custom constructor will pass this
/// parameter as 'true'. The primary constructor is still declared here though
/// to enable the built-in, auto-generated implementations of methods related to
/// equality checking and deconstruction. Pattern instances will be compared
/// for value-based equality based on the parameters listed in this primary
/// constructor.
public record Pattern(double Period, List<Pulse> Pulses, List<double> Delays,
    bool _validated)
{
    public const double MIN_IPI = 1.0; // us

    /// <summary>
    /// The number of pulses in this pattern, i.e., in one period.
    /// </summary>
    public int NumPulses => this.Pulses.Count;
    public double Duration { get; init; }

    /// <summary>
    /// Create a stimulation pulse pattern with parameter validation.
    /// Check that the pulses in the requested pattern are delayed in a valid
    /// manner, i.e., pulses are separated by the given minimum IPI, and all
    /// pulses fit within the requested pattern period. Construction will fail
    /// if these conditions are not met.
    /// </summary>
    /// <param name="period">The period of this pulse pattern in ms. The
    /// inverse is equal to the traditional "pulse frequency" (PF) parameter.
    /// </param>
    /// <param name="pulses">The pulses of this pattern in sequential order.
    /// </param>
    /// <param name="delays">The delays (in ms) from the start of the period
    /// (t=0) at which the corresponding pulse starts. If there are N pulses
    /// given, there should also be at least N delay values given. Any extra
    /// delays will be ignored.</param>
    /// <param name="minIPI">Optional parameter: the minimum IPI between pulses
    /// (in us), default: MIN_IPI.</param>
    /// <exception cref="ArgumentException">Pulses not separated enough or
    /// total pattern duration exceeds specified pattern period.</exception>
    public Pattern(double period, List<Pulse> pulses, List<double> delays,
        double minIPI = MIN_IPI) :
        this(period, new(pulses), new(delays.Take(pulses.Count)),
            _validated: true)
    {
        // TODO: deep copies ensured by Pulses being records?

        this.Duration = 0.0;

        for (int i = 0; i < this.NumPulses; i++)
        {
            // Get the next pulse in the pattern and its delay from t=0.
            var pulse = this.Pulses[i];
            var delay = this.Delays[i];
            // Calculate the time delay from the end of the previous pulse to
            // the start of this next pulse, i.e., the inter-pulse interval.
            var IPI = delay - this.Duration; // units: ms
            // Return early indicating failure if this next pulse is not
            // separated from the previous pulse by a large enough IPI.
            if (IPI * 1000 < minIPI)  // Check IPI in us.
            {
                throw new ArgumentException($"PATTERN CONSTRUCTION ERROR: " +
                    $"Pulse {i} is not separated from the previous pulse by " +
                    $">= min IPI {minIPI} us.");
            }
            // Add the inter-pulse interval and new pulse width to the duration.
            this.Duration += IPI + pulse.PulseWidth;
        }

        // Error if total pattern duration invalid. Units: ms
        if (this.Duration > this.Period)
        {
            throw new ArgumentException($"PATTERN CONSTRUCTION ERROR: " +
                $"Pattern duration {this.Duration} ms exceeds " +
                $"specified pattern period: {this.Period} ms.");
        }
    }
}

