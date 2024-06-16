using EStimLibrary.Core;


namespace EStimLibrary.Core.Stimulation.Functions;

/// <summary>
/// The function signature (i.e., delegate) that every output config validation
/// function must follow.
/// </summary>
/// <param name="outputConfig">The OutputConfiguration to check.</param>
/// <returns>True if the config is valid, False if not.</returns>
public delegate bool ValidateOutputConfigDelegate(
    OutputConfiguration outputConfig);

/// <summary>
/// A class of output config validation functions that all follow the correct
/// delegate signature.
/// </summary>
public static class OutputConfigChecks
{
    /// <summary>
    /// Validate that the output config contains at least 1 source and 1 sink.
    /// </summary>
    /// <param name="outputConfig">The OutputConfiguration to check.</param>
    /// <returns>True if the config is valid, False if not.</returns>
    public static bool BiphasicCheck(OutputConfiguration outputConfig)
    {
        // Validate data: return false(fail) if not at least 1 source and sink.
        return outputConfig.OutputAssignments.Values.Contains(
            Constants.OutputAssignment.SOURCE) &&
            outputConfig.OutputAssignments.Values.Contains(
                Constants.OutputAssignment.SINK);
    }
}