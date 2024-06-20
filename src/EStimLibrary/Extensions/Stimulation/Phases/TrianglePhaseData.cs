using MathNet.Numerics.LinearAlgebra;

using EStimLibrary.Core.Stimulation.Data;
using EStimLibrary.Core.Stimulation.Phases;


namespace EStimLibrary.Extensions.Stimulation.Phases;


public record TrianglePhaseData : ParameterizedPhaseData<
    TrianglePhaseData.TrianglePhaseParam>
{
    // An ISelectable property so this IPhase type can be shown in and selected
    // from, e.g., a drop-down menu.
    public override string Name => "Triangle";
    protected StimPoint[] _StimPoints = new StimPoint[3];

    /// <summary>
    /// The phase param enum specific to this Phase type.
    /// </summary>
    public enum TrianglePhaseParam
    {
        PW1,// phase width 1-- the width (us) of "rising" part, 0+ only
        PA, // phase amplitude-- the peak triangle amplitude (mA), +0-
        PW2 // phase width 2-- the width (us) of the "falling" part, 0+ only
    }

    public TrianglePhaseData(Vector<double> paramData) : base(paramData)
    {
        // Create 3 stim points to represent this triangle phase.
        StimPoint startingPt = new(PW: 0.0, PA: 0.0);
        StimPoint midPt = new(PW: paramData[(int)TrianglePhaseParam.PW1],
            PA: paramData[(int)TrianglePhaseParam.PA]);
        StimPoint endingPt = new(PW: paramData[(int)TrianglePhaseParam.PW1] +
            paramData[(int)TrianglePhaseParam.PW2], PA: 0.0);

        this._StimPoints[0] = startingPt;
        this._StimPoints[1] = midPt;
        this._StimPoints[2] = endingPt;

        // TODO: Any additional constructor logic specific to TrianglePhase?
    }


    #region IPhaseData Implementation
    public override double PhaseWidth => this._StimPoints[2].PW;
    public override List<StimPoint> StimPoints => new(this._StimPoints);

    public override IPhaseData CreateOppositePolarityPhaseData()
    {
        double[] newParamData = (double[])this.PhaseParamValueArray.Clone();
        newParamData[(int)TrianglePhaseParam.PA] *= -1; // just flip PA sign
        return new TrianglePhaseData(Vector<double>.Build.DenseOfArray(
            newParamData));
    }

    public override IPhaseData CreateResizedPhase(double amplitudeScale,
        double chargeError = 0)
    {
        throw new NotImplementedException();
    }

    public override double CalculateCharge()
    {
        // Charge is just area of a triangle.
        double base1 = this.PhaseParamValueArray[(int)TrianglePhaseParam.PW1];
        double height = this.PhaseParamValueArray[(int)TrianglePhaseParam.PA];
        double base2 = this.PhaseParamValueArray[(int)TrianglePhaseParam.PW2];
        return (base1 + base2) * height / 2.0;
    }
    #endregion


    #region ParameterizedPhaseData Implementation
    /// <summary>
    /// Validate the given phase param data values with respect to the
    /// constraints of the specific Phase type.
    /// </summary>
    /// <param name="paramDataVector">The Vector of phase param data values.
    /// Must be in an order consistent with the phase param order in
    /// PhaseParamsUsed. Extra values are ignored. This parameter is not
    /// mutated.</param>
    /// <param name="errorMsg">A string output parameter for any error
    /// messages. Will be an empty string if the method returns true.
    /// </param>
    /// <returns>True if valid, False if not.</returns>
    protected override bool _ValidPhaseParamValues(
        Vector<double> paramDataVector, out string errorMsg)
    {
        double[] paramDataArray = paramDataVector.ToArray();

        // TODO: value bounds on PA and PW?

        // PWs >= 0
        if (!(paramDataArray[(int)TrianglePhaseParam.PW1] >= 0.0))
        {
            errorMsg = $"{TrianglePhaseParam.PW1} must be positive.";
            return false;
        }
        if (!(paramDataArray[(int)TrianglePhaseParam.PW2] >= 0.0))
        {
            errorMsg = $"{TrianglePhaseParam.PW2} must be positive.";
            return false;
        }

        // Output no error msg and return success if all data valid.
        errorMsg = "";
        return true;
    }

    protected override int _DeterminePolarityFromParamValues(
        Vector<double> paramDataVector)
    {
        double[] paramDataArray = paramDataVector.ToArray();
        int polarity = Math.Sign(paramDataArray[(int)TrianglePhaseParam.PA]);
        return (polarity < 0) ? -1 : 1; // return only -1 or +1, +1 if sign 0
    }
    #endregion
}