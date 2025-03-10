namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;


public record StringHierarchySpec(string[] RegionSet, string[] ModifierSet)
{
    public string FullSpec => JoinFullSpec(this.RegionSet, this.ModifierSet);
    public string RegionSpec => JoinRegionSet(this.RegionSet);
    public string ModifierSpec => JoinModifierSet(this.ModifierSet);

    public const char OPTION_REGION_DELIMITER = ' ';
    public const string REGIONS_DELIMITER = ", ";
    public const string REGIONS_MODIFIERS_DELIMITER = " | ";
    public const string MODIFIERS_DELIMITER = REGIONS_DELIMITER;

    public static string ExamplePath = $"option1{OPTION_REGION_DELIMITER}" +
        $"region1{REGIONS_DELIMITER}no-option-region2{REGIONS_DELIMITER}..." +
        $"{REGIONS_MODIFIERS_DELIMITER}modifier{MODIFIERS_DELIMITER}...";

    /// <summary>
    /// Create a new StringHierarchySpec from a full modified region string 
    /// specification. Only syntactical parsing occurs, no semantic validation.
    /// All string components will be converted to lowercase and trimmed of 
    /// edge whitespace.
    /// </summary>
    /// <param name="fullSpec">The full string specification, delimited 
    /// appropriately.</param>
    public StringHierarchySpec(string fullSpec) :
        this(ParseFullSpec(fullSpec))
    {
    }

    /// <summary>
    /// Create a new StringHierarchySpec from an optioned region sequence and a
    /// modifier set. Only syntactical parsing occurs, no semantic validation.
    /// All string components will be converted to lowercase and trimmed of 
    /// edge whitespace.
    /// </summary>
    /// <param name="tuple">First element is a string array containing the 
    /// ordered optioned region names to include in the spec. Second element is
    /// a string array of the unique string directional modifiers.</param>
    public StringHierarchySpec(
        (string[] RegionSet, string[] ModifierSet) tuple) :
        this(tuple.RegionSet, tuple.ModifierSet)
    {
    }

    /// <summary>
    /// Parse a string hierarchy specification into the optioned region names 
    /// and modifier lists according to the x_DELIMITERS, lowercasing and 
    /// trimming.
    /// </summary>
    /// <param name="fullSpec">The full modified region string specification.
    /// </param>
    /// <returns>A tuple of string arrays, the first containing the string
    /// sequence of the region specification, the second containing the set of
    /// directional modifiers. The latter may be empty if no modifiers are
    /// included.</returns>
    public static (string[] regionSet, string[] modifierSet) ParseFullSpec(
        string fullSpec)
    {
        var parts = fullSpec.Split(REGIONS_MODIFIERS_DELIMITER);
        var regionSet = ParseRegionSpec(parts[0]);
        var modifierSet = parts.Length > 1 ? ParseModifierSpec(parts[1]) :
            new string[0];

        return (regionSet, modifierSet);
    }

    /// <summary>
    /// Parse a region specification string into a string array of optioned
    /// region names, converted to lower case and trimmed of edge whitespace.
    /// </summary>
    /// <param name="regionSpec">The string region specification, i.e., the 
    /// first half of a full string specification.</param>
    /// <returns>The string array of optioned region names, parsed by the 
    /// REGIONS_DELIMITER, converted to lowercase and trimmed of edge 
    /// whitespace.</returns>
    public static string[] ParseRegionSpec(string regionSpec)
    {
        return regionSpec.Split(REGIONS_DELIMITER)
            .Select(s => s.ToLower().Trim())
            .ToArray();
    }

    /// <summary>
    /// Parse a modifier specification string into a string array of modifiers,
    /// converted to lower case and trimmed of edge whitespace.
    /// </summary>
    /// <param name="modifierSpec">The string modifier specification, i.e., the
    /// second half of a full string specification.</param>
    /// <returns>The string array of modifiers, parsed by the 
    /// MODIFIERS_DELIMITER, converted to lowercase and trimmed of edge 
    /// whitespace.</returns>
    public static string[] ParseModifierSpec(string modifierSpec)
    {
        return modifierSpec.Split(MODIFIERS_DELIMITER)
            .Select(s => s.ToLower().Trim())
            .ToArray();
    }

    /// <summary>
    /// Join a region specification sequence and a modifier set into a single
    /// full modified region string specification.
    /// </summary>
    /// <param name="regionSet">A string array containing the ordered
    /// specification of optioned regions.</param>
    /// <param name="modifierSet">A string array containing the unique set of
    /// directional modifiers applied to the region specification.</param>
    /// <returns>The full modified region string.</returns>
    public static string JoinFullSpec(string[] regionSet, string[] modifierSet)
    {
        var regionSpec = JoinRegionSet(regionSet);
        var modifierSpec = JoinModifierSet(modifierSet);
        return (modifierSpec.Length != 0) ?
            string.Join(REGIONS_MODIFIERS_DELIMITER, new[] { regionSpec,
                modifierSpec }) :
            regionSpec;
    }

    /// <summary>
    /// Join a sequence of optioned region string names into a single string,
    /// separated by the REGIONS_DELIMITER.
    /// Inverse of ParseRegionSpec, although no case conversion or trimming.
    /// </summary>
    /// <param name="regionSet">The ordered array of optioned region string 
    /// names.</param>
    /// <returns>The single joined string region spec.</returns>
    public static string JoinRegionSet(string[] regionSet)
    {
        return string.Join(REGIONS_DELIMITER, regionSet);
    }

    /// <summary>
    /// Join a set of directional modifiers into a single string modifier spec,
    /// separated by the MODIFIERS_DELIMITER.
    /// Inverse of ParseModifierSpec, although no case conversion or trimming.
    /// </summary>
    /// <param name="modifierSet">The string array of unique modifiers.</param>
    /// <returns>The single join string modifier spec.</returns>
    public static string JoinModifierSet(string[] modifierSet)
    {
        return string.Join(MODIFIERS_DELIMITER, modifierSet);
    }

    /// <summary>
    /// Parse the region option - if there is one - and base name from a full
    /// optioned region name.
    /// </summary>
    /// <param name="optionedRegionName">The full optioned region name to parse.
    /// </param>
    /// <param name="baseName">An output parameter: the whitesapce trimmed base 
    /// name upon success, else an empty string.</param>
    /// <param name="option">An output parameter: the whitespace trimmed option 
    /// name if there is any, else an empty string.</param>
    /// <returns>True if valid parse, False if not.</returns>
    public static bool TryParseOptionedRegionName(string optionedRegionName,
        out string baseName, out string option)
    {
        // Check if input is empty
        if (string.IsNullOrWhiteSpace(optionedRegionName))
        {
            baseName = option = "";
            return false;
        }

        // Split name elements by OPTION_REGION_DELIMITER
        var nameElements = optionedRegionName
            .Split(OPTION_REGION_DELIMITER, 
                StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .ToArray();

        // Fail if more than 2 elements are present
        if (nameElements.Length > 2)
        {
            baseName = option = "";
            return false;
        }

        // Process one or two elements
        baseName = nameElements[^1].Trim();
        option = nameElements.Length == 2 ? nameElements[0].Trim() : "";
        return true;
    }

    public virtual bool Equals(StringHierarchySpec? other)
    {
        // Note: virtual so can be overridden in derived classes if needed.

        if (other is null)
        {
            return false;
        }

        // Check if RegionSet arrays are the same reference or have the same
        // elements in the same order.
        if (!this.RegionSet.SequenceEqual(other.RegionSet))
        {
            return false;
        }

        // Check if ModifierSet arrays have the same elements, irrespective of
        // order.
        if (!this.ModifierSet.OrderBy(x => x).SequenceEqual(
            other.ModifierSet.OrderBy(x => x)))
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        // Combine hash codes of RegionSet and sorted ModifierSet.
        int hashRegionSet = this.RegionSet != null ?
            this.RegionSet.Aggregate(0,
                (hash, item) => hash ^ item.GetHashCode()) :
            0;
        int hashModifierSet = this.ModifierSet != null ?
            this.ModifierSet.OrderBy(x => x).Aggregate(0,
                (hash, item) => hash ^ item.GetHashCode()) :
            0;

        return hashRegionSet ^ hashModifierSet;
    }

    public override string ToString()
    {
        return this.FullSpec;
    }


    #region OLD Spec Overlap Methods
    // Incorrect for the intended function of spec overlap, but leaving here
    // for now since may be useful for other things later.


    // TODO: decide to delete or not once new implementation tested
    /// <summary>
    /// Check if another spec's region shares any parent path spec.
    /// </summary>
    /// <param name="other">The other StringHierarchySpec with which to check
    /// for a shared path.</param>
    /// <param name="regionSetOfOverlap">The shared parent region path spec.
    /// Ignore if no shared path hierarchy is found.
    /// </param>
    /// <returns>T/F if a shared parent path is found.</returns>
    // public bool RegionSetSharesPath(StringHierarchySpec other,
    //     out string[] sharedRegionSet)
    // {
    //     // Shares path if first differing element is not the first one.

    //     // Only search up to the length of the shorter path.
    //     var minLength = Math.Min(this.RegionSet.Length,
    //         other.RegionSet.Length);

    //     // Get the index of the first differing element.
    //     var endIndex = Enumerable.Range(0, minLength).FirstOrDefault(
    //         i => !this.RegionSet[i].Equals(other.RegionSet[i]),
    //         minLength); // Idx = min length if no differences found.

    //     // DIFFERENT: Get the shared path spec (up to first differing idx).
    //     sharedRegionSet = this.RegionSet.Take(endIndex).ToArray();
    //     // Return T/F that any similar elements were found.
    //     return endIndex > 0;
    // }

    // TODO: delete or rename once new implementation tested
    //public bool ModifiersAllowOverlap_OLD(StringHierarchySpec other,
    //    out string[] commonModifiers)
    //{
    //    // Overlaps if all modifiers in shorter set are in longer set.
    //    string[] lessSpecificModSet; // Temp variables to store mod set references.
    //    string[] moreSpecificModSet;
    //    if (this.ModifierSet.Length < other.ModifierSet.Length)
    //    {
    //        lessSpecificModSet = this.ModifierSet;
    //        moreSpecificModSet = other.ModifierSet;
    //    }
    //    else
    //    {
    //        lessSpecificModSet = other.ModifierSet;
    //        moreSpecificModSet = this.ModifierSet;
    //    }
    //    commonModifiers = lessSpecificModSet.Intersect(moreSpecificModSet)
    //        .ToArray();
    //    //bool modifiersOverlap = shorterModSet.All(m => longerModSet.Contains(m));
    //    return commonModifiers.Length == lessSpecificModSet.Length;
    //}
    #endregion OLD Spec Overlap Methods
}

