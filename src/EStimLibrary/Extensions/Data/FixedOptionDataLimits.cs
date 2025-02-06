using EStimLibrary.Core.Data;


namespace EStimLibrary.Extensions.Data;


/// <summary>
/// A limit structure for data that can only have values from a specific list.
/// </summary>
/// <typeparam name="DataType">The data type of the specific options for the
/// associated data.</typeparam>
/// <param name="DataOptions">The specific values the associated data can be.
/// </param>
public record FixedOptionDataLimits<DataType>(
    SortedSet<DataType> DataOptions) : IDataLimits
{
    public string Name => "Fixed Option Data Limits";
    public Type ValidDataType => typeof(DataType);
    public string Description => $"Data of type '{typeof(DataType)}' with " +
        $"discrete options:\n\t" +
        string.Join($"\n\t", this.DataOptions);

    public bool IsValidDataValue(object value)
    {
        return value is not null &&
            value.GetType() == this.ValidDataType &&
            this.DataOptions.Contains((DataType)value);
    }
}