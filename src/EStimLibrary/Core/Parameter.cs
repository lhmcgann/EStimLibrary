namespace EStimLibrary.Core;


public record Parameter(string Name, int SortIdx, IDataLimits DataLimits) :
    ISelectable, IComparable
{
    /// <summary>
    /// Compare another object to this Parameter.
    /// </summary>
    /// <param name="obj">The other object to compare.</param>
    /// <returns>Less than 0 if this instance precedes the other in the sort
    /// order. Zero if this instance and occurs in the same position as the
    /// other in the sort order. Greater than 0 if this instance follows the
    /// other in the sort order.</returns>
    public int CompareTo(object? obj)
    {
        // Sort this instance before the other if other is null.
        if (obj == null)
        {
            return 1;
        }

        int res;    // Comparison result

        // Cast the other object to this type.
        Parameter? otherParam = obj as Parameter;
        // If other is not of correct type, throw ArgumentException.
        if (otherParam == null)
        {
            throw new ArgumentException("Object is not a Parameter.");
        }
        // Else if correct type, first compare sort indices.
        else if ((res = this.SortIdx.CompareTo(otherParam.SortIdx)) == 0)
        {
            // If indices are the same, resulting comparison is by name.
            res = this.Name.CompareTo(otherParam.Name);
        }
        // Return the int comparison result.
        return res;
    }
}

