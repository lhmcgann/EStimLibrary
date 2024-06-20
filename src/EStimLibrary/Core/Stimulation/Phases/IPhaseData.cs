using EStimLibrary.Core;
using EStimLibrary.Core.Stimulation.Data;


namespace EStimLibrary.Core.Stimulation.Phases;


public interface IPhaseData : ISelectable
{
    public int Polarity { get; }
    public double PhaseWidth { get; }
    // (PW, PA) coordinates that make up this phase. PW values should be
    // relative to t=0 being the start of the phase.
    public List<StimPoint> StimPoints { get; }

    /// <summary>
    /// Create a phase of the opposite polarity. Symmetry across time is
    /// specified by specific implementations.
    /// </summary>
    /// <returns>The new phase of opposite polarity.</returns>
    /// TODO: edit to make it possible to create a different type of phase w/
    /// opposite polarity and same charge? maybe not here, but fnxn elsewhere?
    IPhaseData CreateOppositePolarityPhaseData();

    /// <summary>
    /// Create a phase with with its amplitude scaled up by the given amount and
    /// the rest of the phase adjusted accordingly to yield the same charge (or
    /// within the given error of charge difference from the original phase).
    /// </summary>
    /// <param name="amplitudeScale">The decimal percent to scale this phase's
    /// amplitude by. TODO: bound 0.0-1.0?</param>
    /// <param name="chargeError">The error allowed between the original charge
    /// and the new phase's charge (default 0). TODO: units?</param>
    /// <returns></returns>
    IPhaseData CreateResizedPhase(double amplitudeScale,
        double chargeError = Constants.DEFAULT_CHARGE_ERROR);

    /// <summary>
    /// Calculate the charge of this phase.
    /// </summary>
    /// <returns>The charge of this phase. TODO: units?</returns>
    double CalculateCharge();
}

