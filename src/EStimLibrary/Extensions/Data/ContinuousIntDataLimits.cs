using EStimLibrary.Core.Data;


namespace EStimLibrary.Extensions.Data;


/// <summary>
/// A limit structure for data that can have values within a continuous,
/// integer-valued range.
/// </summary>
/// <param name="MinBound">The lower bound, inclusive.</param>
/// <param name="MaxBound">The upper bound, inclusive.</param>
public record ContinuousIntDataLimits(int MinBound, int MaxBound) :
    IDataLimits
{
    public string Name => "Continuous Integer Data Limits";
    public Type ValidDataType => typeof(int);
    public string Description => $"{this.Name}: [{this.MinBound}, " +
        $"{this.MaxBound}]";

    public bool IsValidDataValue(object value)
    {
        return value is not null &&
            value.GetType() == this.ValidDataType &&
            this.MinBound <= (int)value &&
            (int)value <= this.MaxBound;
    }
}