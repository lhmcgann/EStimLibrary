using EStimLibrary.Core.Data;


namespace EStimLibrary.Extensions.Data;


/// <summary>
/// A limit structure for data that can have values within a continuous,
/// real-valued range.
/// </summary>
/// <param name="MinBound">The lower bound, inclusive.</param>
/// <param name="MaxBound">The upper bound, inclusive.</param>
/// TODO: add resolution? and rounding scheme param? or leave rounding scheme
/// for elsewhere to do since this is just validation, not subsequent action...
public record ContinuousDataLimits(double MinBound, double MaxBound) :
    IDataLimits
{
    public string Name => "Continuous Data Limits";
    public Type ValidDataType => typeof(double);
    public string Description => $"{this.Name}: [{this.MinBound}, " +
        $"{this.MaxBound}]";

    public bool IsValidDataValue(object value)
    {
        return value is not null &&
            value.GetType() == this.ValidDataType &&
            this.MinBound <= (double)value &&
            (double)value <= this.MaxBound;
    }
}