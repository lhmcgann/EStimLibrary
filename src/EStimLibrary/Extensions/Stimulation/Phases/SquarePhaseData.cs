using MathNet.Numerics.LinearAlgebra;

using EStimLibrary.Core.Stimulation.Data;
using EStimLibrary.Core.Stimulation.Phases;


namespace EStimLibrary.Extensions.Stimulation.Phases;


public record SquarePhaseData :
    ParameterizedPhaseData<SquarePhaseData.SquarePhaseParam>
{
    // An ISelectable property so this IPhase type can be shown in and selected
    // from, e.g., a drop-down menu.
    public override string Name => "Square";
    protected StimPoint[] _StimPoints = new StimPoint[2];

    /// <summary>
    /// The phase param enum specific to this Phase type.
    /// </summary>
    public enum SquarePhaseParam
    {
        PA, // phase amplitude-- the amplitude in mA of the square phase, +0-
        PW  // phase width-- the width in us of the square phase, + only
    }

    public SquarePhaseData(Vector<double> paramData) : base(paramData)
    {
        // Create 2 stim points to represent the corners of this square phase.
        StimPoint leadingEdge = new(PW: 0.0,
            PA: paramData[(int)SquarePhaseParam.PA]);

        StimPoint fallingEdge = new(PW: paramData[(int)SquarePhaseParam.PW],
            PA: paramData[(int)SquarePhaseParam.PA]);

        this._StimPoints[0] = leadingEdge;
        this._StimPoints[1] = fallingEdge;
        // TODO: Any additional constructor logic specific to SquarePhase?
    }


    #region IPhaseData Implementation
    public override double PhaseWidth => this._StimPoints[1].PW;
    public override List<StimPoint> StimPoints => new(this._StimPoints);

    public override IPhaseData CreateOppositePolarityPhaseData()
    {
        double[] newParamData = (double[])this.PhaseParamValueArray.Clone();
        newParamData[(int)SquarePhaseParam.PA] *= -1;   // just flip PA sign
        return new SquarePhaseData(Vector<double>.Build.DenseOfArray(
            newParamData));
    }

    public override IPhaseData CreateResizedPhase(double amplitudeScale,
        double chargeError = 0.0)
    {
        // Scale PA up and PW down
        double newPA = this.PhaseParamValueArray[(int)SquarePhaseParam.PA] *
            amplitudeScale;
        double newPW = this.PhaseParamValueArray[(int)SquarePhaseParam.PW] /
            amplitudeScale;

        // Store new values in correct order
        double[] newParamValues = new double[this.NumParams];
        newParamValues[(int)SquarePhaseParam.PA] = newPA;
        newParamValues[(int)SquarePhaseParam.PW] = newPW;

        // Create new SquarePhase with the same polarity but new param values
        return new SquarePhaseData(Vector<double>.Build.DenseOfArray(
            newParamValues));
    }

    public override double CalculateCharge()
    {
        // Charge is just area of a rectangle.
        double baseVal = this.PhaseParamValueArray[(int)SquarePhaseParam.PW];
        double height = this.PhaseParamValueArray[(int)SquarePhaseParam.PA];
        return baseVal * height;
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
    /// mutated. </param>
    /// <param name="errorMsg">A string output parameter for any error
    /// messages. Will be an empty string if the method returns true.
    /// </param>
    /// <returns>True if valid, False if not.</returns>
    protected override bool _ValidPhaseParamValues(
        Vector<double> paramDataVector, out string errorMsg)
    {
        double[] paramDataArray = paramDataVector.ToArray();

        // TODO: value bounds on PA and PW?

        // PW > 0
        if (!(paramDataArray[(int)SquarePhaseParam.PW] > 0.0))
        {
            errorMsg = $"{SquarePhaseParam.PW} must be greater than 0.";
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
        int polarity = Math.Sign(paramDataArray[(int)SquarePhaseParam.PA]);
        return (polarity < 0) ? -1 : 1; // return only -1 or +1, +1 if sign 0
    }
    #endregion
}

