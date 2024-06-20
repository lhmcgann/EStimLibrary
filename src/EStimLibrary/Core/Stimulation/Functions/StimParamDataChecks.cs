using EStimLibrary.Core.Stimulation.Pulses;

namespace EStimLibrary.Core.Stimulation.Functions;

/// <summary>
/// The function signature (i.e., delegate) that every stim data validation
/// function must follow.
/// </summary>
/// <param name="pulse">The stimulation pulse to check.</param>
/// <returns>True if the data is valid, False if not.</returns>
public delegate bool ValidateStimParamDataDelegate(Pulse pulse);

/// <summary>
/// A class of stim param data validation functions that all follow the correct
/// delegate signature.
/// </summary>
public static class StimParamDataChecks
{
    /// <summary>
    /// Validate the data vector includes exactly one value for each pulse param.
    /// </summary>
    /// <param name="pulse">The stimulation pulse to check.</param>
    /// <returns>True if the data is valid, False if not.</returns>
    public static bool AllParamsValuedCheck(Pulse pulse)
    {
        //return stimData.Count == pulseParams.Count;
        throw new NotImplementedException();
    }
}

