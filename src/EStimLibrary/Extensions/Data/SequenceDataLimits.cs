using EStimLibrary.Core.Data;


namespace EStimLibrary.Extensions.Data;


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