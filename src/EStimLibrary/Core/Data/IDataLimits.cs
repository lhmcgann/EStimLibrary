namespace EStimLibrary.Core.Data;


/// <summary>
/// A structure to represent the limits on a specific associated parameter or
/// other data value, supplying data type information and bounding/limiting
/// validation.
/// </summary>
public interface IDataLimits : ISelectable
{
    /// <summary>
    /// The data type that is valid for the associated data.
    /// </summary>
    public Type ValidDataType { get; }
    /// <summary>
    /// A strign description of the limits this structure imposes on the
    /// associated data. Meant to be open-ended to accommodate listing discrete
    /// options, explaining bounds, etc. Basically a 'help' message.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// Validate a specific data value. Must be non-null and of the
    /// ValidDataType, in addition to any other validation checks.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>True if the value is valid, false if not.</returns>
    bool IsValidDataValue(object value);
}