using EStimLibrary.Core;


namespace EStimLibrary.Core.Stimulation;


public record OutputConfiguration(
    Dictionary<int, Constants.OutputAssignment> OutputAssignments)
{
    /// <summary>
    /// Get the output config array based on this output assignment.
    /// </summary>
    /// <param name="totalNumOutputs">The total number of outputs to include in
    /// the config array.</param>
    /// <param name="globalToLocalMap">A map of global output IDs (as currently
    /// contained in this OutputConfig) to local output IDs (i.e., the indexes
    /// in the output array). Must contain an element for each global ID in this
    /// current output assignment. Extra mappings will be ignored.</param>
    /// <param name="configArray">An output parameter: The full length output
    /// configuration array. Any outputs not assigned a value in this config
    /// will be marked as UNUSED.</param>
    /// <returns>True if an output config array could be made, false if not
    /// (e.g., globalToLocalMap does not contain enough information).</returns>
    public bool TryToArray(int totalNumOutputs,
        Dictionary<int, int> globalToLocalMap,
        out Constants.OutputAssignment[] configArray)
    {
        // Init the output config array with all UNUSED.
        configArray = CreateEmptyConfig(totalNumOutputs);

        // Fail early if ID map does not contain a mapping for each global ID.
        if (!this.OutputAssignments.Keys.All(
            globalId => globalToLocalMap.Keys.Contains(globalId)))
        {
            return false;
        }

        // Stuff the array with assignment values at the correct idxs per ID.
        foreach (var (globalId, value) in this.OutputAssignments)
        {
            configArray[globalToLocalMap[globalId]] = value;
        }

        // Return success.
        return true;
    }

    public static Constants.OutputAssignment[] CreateEmptyConfig(
        int numOutputs)
    {
        var unusedConfig = new Constants.OutputAssignment[numOutputs];
        Array.Fill(unusedConfig, Constants.OutputAssignment.UNUSED);
        return unusedConfig;
    }
}

