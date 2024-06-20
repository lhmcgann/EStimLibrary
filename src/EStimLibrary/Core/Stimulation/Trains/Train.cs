using EStimLibrary.Core.Stimulation.Patterns;


namespace EStimLibrary.Core.Stimulation.Trains;


/// <summary>
/// A stimulation train is the full time course of a stimulation pattern
/// repeating at a frequency (1/patternPeriod), perhaps being modulated by a
/// certain function. A train can run indefinitely or for a fixed duration,
/// specified by a set number of pattern repeats. TODO: duration-specification
/// instead? or leave that to the calling context to do math real quick...
/// </summary>
/// <param name="BasePattern">The pattern on which to base further iterations,
/// e.g., the base pattern for a (TODO) modulation function.</param>
/// <param name="NumPatternRepeats">The number of pattern (period) repeats. May
/// be INFINITE (-1).</param>
public abstract record Train(Pattern BasePattern, int NumPatternRepeats)
{
}

