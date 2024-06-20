using EStimLibrary.Core.Stimulation.Phases;
using EStimLibrary.Core.Stimulation.Pulses;


namespace EStimLibrary.Extensions.Stimulation.Pulses;


public record MonophasicPulse : Pulse
{
    public Phase Phase { get; init; }

    public MonophasicPulse(Phase phase) :
        base(new List<Phase>() { phase }, new List<double>())
    {
        this.Phase = phase;
    }
}

