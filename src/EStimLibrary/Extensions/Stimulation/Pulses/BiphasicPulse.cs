using EStimLibrary.Core.Stimulation.Phases;
using EStimLibrary.Core.Stimulation.Pulses;
using EStimLibrary.Core;


namespace EStimLibrary.Extensions.Stimulation.Pulses;


/// <summary>
/// A biphasic pulse.
/// </summary>
public record BiphasicPulse : Pulse
{
    // TODO: actually keep these properties, or just accept their values at
    // construction to create the correct Phases for the generic Pulse?
    public double AnodeRatio { get; init; }
    public bool AnodeFirst { get; init; }
    public int AnodeIdx => Constants.GetAnodeIdx(this.AnodeFirst);

    /// <summary>
    /// Create a new BiphasicPulse from 2 existing phases and an IPD, factoring
    /// in pulse parameters.
    /// </summary>
    /// <param name="phases">The two Phases for this biphasic pulse. Deep copies
    /// ensured.</param>
    /// <param name="ipd">The inter-phase delay (us).
    /// <param name="anodeRatio">TODO: explanation. Optional parameter, default
    /// is BiphasicPulse.DEFAULT_ANODE_RATIO.</param>
    /// <param name="scalingChargeEpsilon">The acceptable error in phase charge
    /// after any scaling, e.g., due to anode ratio. Optional parameter, default
    /// is IPhaseData.DEFAULT_CHARGE_ERROR.</param>
    /// <param name="anodeFirst">T/F whether to have the anodic phase first.
    /// Optional parameter, default false, i.e., cathodic-first pulse.</param>
    /// </param>
    public BiphasicPulse(List<Phase> phases, double ipd,
        double anodeRatio = Constants.DEFAULT_ANODE_RATIO,
        double scalingChargeEpsilon = Constants.DEFAULT_CHARGE_ERROR,
        bool anodeFirst = false) :
        base(OrderAndScalePhases(phases, anodeRatio, scalingChargeEpsilon,
                                anodeFirst),
            new List<double>() { ipd })
    {
        this.AnodeFirst = anodeFirst;
        this.AnodeRatio = anodeRatio;
    }

    /// <summary>
    /// Create a shape-symmetric biphasic pulse from 1 base phase and an IPD,
    /// factoring in pulse parameters.
    /// </summary>
    /// <param name="givenPhase">A preconstructed Phase from which to create 
    /// the opposite polarity Phase of the *same shape*. The two phases will be
    /// stored in order depending on their polarities and the parameter
    /// anodeFirst.</param>
    /// <param name="ipd">The inter-phase delay (us).
    /// <param name="anodeRatio">TODO: explanation. Optional parameter, default
    /// is BiphasicPulse.DEFAULT_ANODE_RATIO.</param>
    /// <param name="scalingChargeEpsilon">The acceptable error in phase charge
    /// after any scaling, e.g., due to anode ratio. Optional parameter, 
    /// default is IPhaseData.DEFAULT_CHARGE_ERROR.</param>
    /// <param name="anodeFirst">T/F whether to have the anodic phase first.
    /// Optional parameter, default false, i.e., cathodic-first pulse.</param>
    public BiphasicPulse(Phase givenPhase, double ipd,
        double anodeRatio = Constants.DEFAULT_ANODE_RATIO,
        double scalingChargeEpsilon = Constants.DEFAULT_CHARGE_ERROR,
        bool anodeFirst = false) :
        base(GeneratePhases(givenPhase, anodeRatio, scalingChargeEpsilon,
                            anodeFirst),
            new List<double>() { ipd })
    {
        this.AnodeFirst = anodeFirst;
        this.AnodeRatio = anodeRatio;
    }

    /// <summary>
    /// Helper constructor function to generate the second phase of the
    /// biphasic pulse, scale the phases correctly depending on anode ratio,
    /// and order the phases correctly based on the anode-first selection.
    /// </summary>
    /// <param name="givenPhase">The base phase from which to create the other
    /// phase of opposite polarity.</param>
    /// <param name="anodeRatio">TODO: explanation</param>
    /// <param name="scalingChargeEpsilon">The acceptable error in phase charge
    /// after scaling by anode ratio.</param>
    /// <param name="anodeFirst">T/F whether to have the anodic phase first.
    /// </param>
    /// <returns>The list of 2 appropriately scaled phases in the correct
    /// polarity order.</returns>
    protected static IEnumerable<Phase> GeneratePhases(Phase givenPhase,
        double anodeRatio, double scalingChargeEpsilon, bool anodeFirst)
    {
        // Create opposite-polarity phase.
        Phase otherPhase = givenPhase.CreateOppositePolarityPhase();

        // Return ordered and scaled phases.
        return OrderAndScalePhases(new List<Phase>() { givenPhase, otherPhase },
            anodeRatio, scalingChargeEpsilon, anodeFirst);
    }

    /// <summary>
    /// Constructor helper function. Order the 2 phases according to phase
    /// polarity and the anode-first pulse parameter. Scale the anodic phase
    /// by the anode ratio, maintaining the same phase charge to within a
    /// certain epsilon.
    /// </summary>
    /// <param name="phases">The two phases of this pulse, assumed to be of
    /// opposite polarity.</param>
    /// <param name="anodeRatio">TODO: explanation</param>
    /// <param name="scalingChargeEpsilon">The acceptable error in phase charge
    /// after scaling by anode ratio.</param>
    /// <param name="anodeFirst">T/F whether to have the anodic phase first.
    /// </param>
    /// <returns></returns>
    protected static List<Phase> OrderAndScalePhases(List<Phase> phases,
        double anodeRatio, double scalingChargeEpsilon, bool anodeFirst)
    {
        // Store phases in correct order depending on anodeFirst bool.
        List<Phase> orderedPhases = OrderPhases(phases, anodeFirst,
            out int anodeIdx);

        // Scale the anodic phase.
        orderedPhases[anodeIdx] = orderedPhases[anodeIdx]
            .CreateResizedPhase(anodeRatio, scalingChargeEpsilon);

        return orderedPhases;
    }

    /// <summary>
    /// Constructor helper function. Order the 2 phases according to phase
    /// polarity and the anode-first pulse parameter.
    /// </summary>
    /// <param name="phases">The two phases of this pulse, assumed to be of
    /// opposite polarity. TODO: should this assumption be enforced?</param>
    /// <param name="anodeFirst">T/F whether to have the anodic phase first.
    /// </param>
    /// <param name="anodeIdx">An output parameter. The integer index in the
    /// returned list at which the anodic phase was placed.</param>
    /// <returns>A list of the two opposite polarity phases, now correctly
    /// ordered based on polarity and pulse params. If the phases are given in
    /// the correct order, the argument list will be returned.</returns>
    protected static List<Phase> OrderPhases(List<Phase> phases,
        bool anodeFirst, out int anodeIdx)
    {
        // Determine which phase is cathodic vs anodic, and store in temp vars.
        anodeIdx = (phases[0].PhaseData.Polarity == Constants.ANODIC_POLARITY)
            ? 0 : 1;
        int cathodeIdx = (anodeIdx == 0) ? 1 : 0;

        // Determine what the order should be.
        int desiredAnodeIdx = Constants.GetAnodeIdx(anodeFirst);

        // Put phases in correct order.
        if (anodeIdx == desiredAnodeIdx)
        {
            return phases;
        }
        // Reorder: put anode and cathode at correct indices.
        var orderedPhases = new List<Phase>(2);
        orderedPhases[desiredAnodeIdx] = phases[anodeIdx];
        orderedPhases[anodeIdx] = phases[cathodeIdx];

        // Correct output variable and return.
        anodeIdx = desiredAnodeIdx;
        return orderedPhases;
    }

    /// <summary>
    /// Check if the pulse is charge balanced.
    /// </summary>
    /// <param name="epsilon">The error within which the charge of the phases of
    /// this pulse must be equal (default 0.0).</param>
    /// <returns>T/F if phases charge balanced within epsilon.</returns>
    public bool IsChargeBalanced(double epsilon = 0.0)
    {
        return Math.Abs(this.Phases[0].PhaseData.CalculateCharge() -
            this.Phases[1].PhaseData.CalculateCharge()) <= epsilon;
    }

    /// <summary>
    /// Adjust phases to be charge balanced.
    /// TODO: accept some param-changing priority scheme/Strategy to determine
    /// what gets adjusted
    /// </summary>
    /// <param name="epsilon">The error within which the charge of the phases of
    /// this pulse must become equal (default 0.0).</param>
    /// <exception cref="NotImplementedException"></exception>
    public void EnforceChargeBalance(double epsilon = 0.0)
    {
        throw new NotImplementedException();
    }
}

