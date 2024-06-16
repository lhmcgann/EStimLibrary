using EStimLibrary.Core.Stimulation.Trains;
using EStimLibrary.Core.Stimulation.Patterns;
using EStimLibrary.Core;


namespace EStimLibrary.Extensions.Stimulation.Trains;


public record InfiniteTrain : Train
{
    public InfiniteTrain(Pattern basePattern) :
        base(basePattern, Constants.POS_INFINITY)
    {
    }
}

