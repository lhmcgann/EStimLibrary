using EStimLibrary.Core.Stimulation.Trains;
using EStimLibrary.Core.Stimulation.Patterns;


namespace EStimLibrary.Extensions.Stimulation.Trains;


public abstract record DurationTrain : Train
{
    /// <summary>
    /// The duration of the whole train, in ms. The base pattern of this train
    /// will repeat for as many full iterations as possible within the given
    /// duration. No partial pattern iterations will be executed.
    /// </summary>
    public double Duration { get; init; }

    public DurationTrain(Pattern basePattern, double duration) :
        base(basePattern, _GetMaxNumIterations(basePattern, duration))
    {
        // TODO: error if Max iters = 0 --> do that in base Train though
        this.Duration = duration;
    }

    private static int _GetMaxNumIterations(Pattern pattern, double duration)
    {
        return (int)Math.Floor(duration / pattern.Duration);
    }
}

