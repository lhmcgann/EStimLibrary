using MathNet.Numerics.LinearAlgebra;

using EStimLibrary.Core.Stimulation.Data;
using EStimLibrary.Core.Stimulation.Phases;


namespace EStimLibrary.Extensions.Stimulation.Phases;


// Value Object: immutable, lacks unique ID
/// <summary>
/// A single stimulation phase that is constructed from parameters. Derived
/// classes implement specific parameterized phase shape types, each with their
/// own specific enum of phase parameters.
/// A Phase is immutable once created and is not uniquely identifiable.
/// </summary>
/// <typeparam name="PhaseParamType">The enum type of specific phase parameters.
/// TODO: ask Jeremy if different shaped phases will still be used just w/ PA,PW
/// scaling rather than multiple (new) params, i.e., all base shapes saved on
/// FW, then just selected and scaled
/// </typeparam>
public abstract record ParameterizedPhaseData<PhaseParamType> : IPhaseData
    where PhaseParamType : Enum
{
    #region Properties
    // An ISelectable property so this IPhaseData type can be shown in and 
    // selected from, e.g., in a drop-down menu.
    public abstract string Name { get; }

    // IPhaseData properties.
    public int Polarity { get; init; }
    public abstract double PhaseWidth { get; }
    public abstract List<StimPoint> StimPoints { get; }

    // Specific ParameterizedPhaseData properties.
    public SortedSet<PhaseParamType> ParamKeys => new(
        (IEnumerable<PhaseParamType>)Enum.GetValues(typeof(PhaseParamType)));
    public int NumParams => Enum.GetValues(typeof(PhaseParamType)).Length;

    // Data values of parameters, same as order as listed in PhaseParamsUsed.
    // These are always non-polarized data values.
    public Vector<double> PhaseParamValueVector { get; init; }
    public double[] PhaseParamValueArray =>
        this.PhaseParamValueVector.ToArray();
    #endregion


    public ParameterizedPhaseData(Vector<double> paramData)
    {
        // First check if param values valid, based on derived phase
        // implementation. Throw exception if not.
        if (!this._ValidPhaseParamValues(paramData, out string paramErrorMsg))
        {
            throw new ArgumentException($"{this.Name}Phase construction " +
                $"error: {paramErrorMsg}");
        }

        // Determine and store the polarity of the given phase data, based on
        // derived phase implementation.
        this.Polarity = this._DeterminePolarityFromParamValues(paramData);

        // Init the vector of param data values, length equal to params used.
        this.PhaseParamValueVector = Vector<double>.Build.Dense(
            this.NumParams);
        // Story a deepcopy of the param data values, up to n values.
        paramData.CopySubVectorTo(this.PhaseParamValueVector, 0, 0,
            this.NumParams);
    }


    #region IPhaseData Methods
    // Make derived classes implement them.
    public abstract IPhaseData CreateOppositePolarityPhaseData();
    public abstract IPhaseData CreateResizedPhase(double amplitudeScale,
        double chargeError = 0.0);
    public abstract double CalculateCharge();
    #endregion


    #region Abstract ParameterizedPhaseData Methods
    /// <summary>
    /// Validate the given phase param data values with respect to the
    /// constraints of the specific Phase type.
    /// </summary>
    /// <param name="paramDataVector">The Vector of phase param data values.
    /// Must be in an order consistent with the phase param order in
    /// PhaseParamsUsed. Extra values are ignored. This parameter is not mutated.
    /// </param>
    /// <param name="errorMsg">A string output parameter for any error messages.
    /// Will be an empty string if the method returns true.
    /// </param>
    /// <returns>True if valid, False if not.</returns>
    protected abstract bool _ValidPhaseParamValues(
        Vector<double> paramDataVector, out string errorMsg);

    /// <summary>
    /// Determine the polarity of the given phase param data values.
    /// </summary>
    /// <param name="paramDataVector">A Vector of the phase param data values,
    /// given in the order of the Phase's phase params.</param>
    /// <returns>The polarity of the phase.</returns>
    protected abstract int _DeterminePolarityFromParamValues(
        Vector<double> paramDataVector);
    #endregion
}

