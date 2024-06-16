namespace EStimLibrary.Core;


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

/// <summary>
/// A limit structure for data that can have values within a continuous,
/// real-valued range.
/// </summary>
/// <param name="MinBound">The lower bound, inclusive.</param>
/// <param name="MaxBound">The upper bound, inclusive.</param>
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

/// <summary>
/// A limit structure for data that is itself a sequence of data values, each
/// element with its own subsequent data limits.
/// </summary>
/// <param name="OrderedElementNames">A list of string names for each element in
/// the sequence. The names must be unique and in the desired order.</param>
/// <param name="ElementLimits">The corresponding data limits for each element,
/// keyed by the element's string name which must be present in
/// OrderedElementNames.</param>
/// <param name="OptionalElements">An optional parameter: the string names of
/// any elements in the sequence that are themselves optional and don't have to
/// be included in an actual sequence data record. This set must only contain
/// strings found in OrderedElementNames. Value is null by default, i.e., if all
/// elements are required.</param>
public record SequenceDataLimits(List<string> OrderedElementNames,
    Dictionary<string, IDataLimits> ElementLimits,
    IEnumerable<string> OptionalElements = null) : IDataLimits
{
    public string Name => "Sequence Data Limits";
    // At bare minimum, data instances valid for further validation of this
    // against this sequence limitations are ordered lists of sequence element
    // name-value pairs.
    public Type ValidDataType => typeof(List<Tuple<string, object>>);
    public string Description
    {
        get
        {
            string description = "A sequence of data values, each with their " +
                $"own data limits, as follows:\n";
            // Add the description of each element's data limits.
            for (int i = 0; i < this.OrderedElementNames.Count; i++)
            {
                string elementName = this.OrderedElementNames[i];

                // Add a '*' to the beginning of the element description if it
                // is an optional element in the sequence.
                bool isOptional = this.OptionalElements is not null && this.OptionalElements.Contains(elementName);
                string optionalPrefix = isOptional ? "*" : "";

                description = description + $"\t{optionalPrefix}" +
                    $"Element {i + 1} = '{elementName}': " +
                    $"{this.ElementLimits[elementName].Description}\n";
            }
            return description;
        }
    }

    public bool IsValidDataValue(object value)
    {
        // Fail immediately if null or not a list of name-value pairs is given.
        if (value is null || value.GetType() != this.ValidDataType)
        {
            return false;
        }

        // Extract the sequence, a list of element name-value pairs.
        var sequence = (List<Tuple<string, object>>)value;

        // Iterate through each element name-value pair.
        int headerIndex = 0;
        for (int dataIndex = 0; dataIndex < sequence.Count; dataIndex++)
        {
            // Return failure if data values provided but no more expected
            // in the sequence.
            if (headerIndex >= this.OrderedElementNames.Count)
            {
                return false;
            }

            // Get the element name-value pair.
            (string givenName, object givenValue) = sequence[dataIndex];

            // Get the name of the element expected to be at this index.
            var expectedName = this.OrderedElementNames[headerIndex];

            // Check if the expected and given names match.
            if (!givenName.Equals(expectedName))
            {
                // If names don't match, check if the expected element is
                // optional. Simply go to next header index if so.
                if (this.OptionalElements is not null &&
                    this.OptionalElements.Contains(expectedName))
                {
                    // Decrement the data index so this value is checked again
                    // and not skipped.
                    dataIndex--;
                }
                // Otherwise, return failure: the sequence is missing an
                // element.
                else
                {
                    return false;
                }
            }

            // Otherwise, the names match, so get the data limits.
            var limits = this.ElementLimits[givenName];
            // Return failure if an invalid element value is given.
            if (!limits.IsValidDataValue(givenValue))
            {
                return false;
            }

            // Otherwise, it's valid data so far. Look at the next expected
            // value.
            headerIndex++;
        }

        // If made it to the end of the loop, all data valid. Return success.
        return true;
    }
}