using EStimLibrary.Core.Data;


namespace EStimLibrary.Extensions.Data;


/// <summary>
/// A limit structure for data with run-time-dependent limits or some other form
/// of dynamic validation.
/// </summary>
/// <typeparam name="DataType">The data type of the associated data.</typeparam>
/// <param name="CheckFunction">The dynamic validation function that is called
/// within the IsValidDataValue() method of this limits record. The function
/// must take in the data value (of the correct data type) and return a boolean
/// indicating if the data value is valid at the time when the function is 
/// called.</param>
/// <param name="Description">The string description of the dynamic validation.
/// Will be used as the overall description for the limits imposed.</param>
public record DynamicDataLimits<DataType>(
    Func<DataType, bool> CheckFunction, string Description) : IDataLimits
{
    public string Name => "Dynamic Data Limits";
    public Type ValidDataType => typeof(DataType);

    public bool IsValidDataValue(object value)
    {
        return value is not null &&
            value.GetType() == this.ValidDataType &&
            this.CheckFunction((DataType)value);
    }
}