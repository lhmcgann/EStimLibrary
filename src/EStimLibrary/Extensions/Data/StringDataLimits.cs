using EStimLibrary.Core.Data;


namespace EStimLibrary.Extensions.Data;


public record StringDataLimits : IDataLimits
{
    public string Name => "String Data Limits";

    public Type ValidDataType => typeof(string);
    public string Description => "Any string-type data.";
    public bool IsValidDataValue(object value)
    {
        return value.GetType() == this.ValidDataType;
    }
}