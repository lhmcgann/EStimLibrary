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
    /// <summary>
    /// Merge this LocalizationData object with another by taking the set union
    /// per property. Neither initial object should be modified, and the product
    /// object should be a new object.
    /// </summary>
    /// <param name="other">The other LocalizationData object to merge with this
    /// one.</param>
    /// <returns>A new LocalizationData object with the merged (set union)
    /// properties of the two initial LocalizationData objects.</returns>
    public LocalizationData Merge(LocalizationData other)
    {
        var allAreasFullyContaining = this.AreasFullyContaining
            .Union(other.AreasFullyContaining);
        var allAreasPartiallyContaining = this.AreasPartiallyContaining
            .Union(other.AreasPartiallyContaining);
        return new(allAreasFullyContaining, allAreasPartiallyContaining);
    }
}

