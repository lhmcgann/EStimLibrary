using EStimLibrary.Core;
using EStimLibrary.Core.Stimulation.Data;


namespace EStimLibrary.Core.Stimulation.Phases;


public record Phase(OutputConfiguration OutputConfig, IPhaseData PhaseData)
{
    public Phase CreateOppositePolarityPhase()
    {
        // Create polarity-mirrored output config.
        var oppositeConfig = this.OutputConfig.OutputAssignments.ToDictionary(
            kvp => kvp.Key,
            kvp => (Constants.OutputAssignment)(((int)kvp.Value) * -1));

        // Create opposite polarity data.
        IPhaseData oppositeData = this.PhaseData
            .CreateOppositePolarityPhaseData();

        return new Phase(new(oppositeConfig), oppositeData);
    }

    /// <summary>
    /// Create a new Phase that has the same charge as this Phase but is scaled
    /// in amplitude, and thus width as well, by a given factor.
    /// </summary>
    /// <param name="amplitudeScale">TODO: explanation (rly it's for anode
    /// ratio use, but generalizing here)</param>
    /// <param name="scalingChargeEpsilon">The acceptable error in phase charge
    /// after scaling by anode ratio.</param>
    /// <returns></returns>
    public Phase CreateResizedPhase(double amplitudeScale,
        double scalingChargeEpsilon = Constants.DEFAULT_CHARGE_ERROR)
    {
        var scaledData = this.PhaseData.CreateResizedPhase(amplitudeScale,
            scalingChargeEpsilon);
        return new(this.OutputConfig, scaledData);
    }
}

