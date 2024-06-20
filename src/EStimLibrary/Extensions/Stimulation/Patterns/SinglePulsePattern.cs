using EStimLibrary.Core.Stimulation.Patterns;
using EStimLibrary.Core.Stimulation.Pulses;


namespace EStimLibrary.Extensions.Stimulation.Patterns;


public record SinglePulsePattern : Pattern
{
    public SinglePulsePattern(double period, Pulse pulse, double delay,
        double minIPI = Pattern.MIN_IPI) :
        base(period, new() { pulse }, new() { delay }, minIPI)
    {
    }
}

