namespace EStimLibrary.Core.SpatialModel;

/// <summary>
/// A record to represent event localization results.
/// </summary>
/// <param name="AreasFullyContaining">The set of all areas completely
/// containing the event.</param>
/// <param name="AreasPartiallyContaining">The set of all areas only partially
/// containing the event.</param>
public record LocalizationData(IEnumerable<int> AreasFullyContaining,
    IEnumerable<int> AreasPartiallyContaining)
{
    public LocalizationData Merge(LocalizationData other)
    {
        var allAreasFullyContaining = this.AreasFullyContaining
            .Union(other.AreasFullyContaining);
        var allAreasPartiallyContaining = this.AreasPartiallyContaining
            .Union(other.AreasPartiallyContaining);
        return new(allAreasFullyContaining, allAreasPartiallyContaining);
    }
}

