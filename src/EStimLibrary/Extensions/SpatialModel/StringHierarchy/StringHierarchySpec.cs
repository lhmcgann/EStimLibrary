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

    public StringHierarchySpec(string fullSpec) :
        this(ParseFullSpec(fullSpec))
    {
    }

    public StringHierarchySpec(
        (string[] RegionSet, string[] ModifierSet) tuple) :
        this(tuple.RegionSet, tuple.ModifierSet)
    {
    }

    /// <summary>
    /// Parse a string hierarchy specification into the region names and 
    /// modifier lists.
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

    public static string[] ParseRegionSpec(string regionSpec)
    {
        return regionSpec.Split(REGIONS_DELIMITER)
            .Select(s => s.ToLower().Trim())
            .ToArray();
    }

    public static string[] ParseModifierSpec(string modifierSpec)
    {
        return modifierSpec.Split(MODIFIERS_DELIMITER)
            .Select(s => s.ToLower().Trim())
            .ToArray();
    }

    /// <summary>
    /// Join a region specification sequence and a modifier set into a single
    /// full modified region string.
    /// </summary>
    /// <param name="regionSet">A string array containing the ordered
    /// specification of regions.</param>
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

    public static string JoinRegionSet(string[] regionSet)
    {
        return string.Join(REGIONS_DELIMITER, regionSet);
    }

    public static string JoinModifierSet(string[] modifierSet)
    {
        return string.Join(MODIFIERS_DELIMITER, modifierSet);
    }

    /// <summary>
    /// Parse the region option - if there is one - and base name from a full
    /// region name.
    /// </summary>
    /// <param name="optionedRegionName">The full region name to parse.</param>
    /// <param name="baseName">An output parameter: the base name upon success,
    /// else and empty string.</param>
    /// <param name="option">An output parameter: the string that contains the option if
    /// there is any, empty if not.</param>
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
        var nameElements = optionedRegionName.Split(OPTION_REGION_DELIMITER);

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

    /// <summary>
    /// Check if another spec's region spatially overlaps with this spec's
    /// region and determine the region of overlap.
    /// </summary>
    /// <param name="other">The other StringHierarchySpec with which to check
    /// for overlap.</param>
    /// <param name="regionSetOfOverlap">The region of overlap specification as
    /// an ordered array of string region names. Ignore if no overlap found.
    /// </param>
    /// <returns>T/F if an overlapping region was found.</returns>
    public bool RegionSetOverlaps(StringHierarchySpec other,
        out string[] regionSetOfOverlap)
    {
        // Overlaps if all elements of shortest sequence match longer sequence.
        var minLength = Math.Min(this.RegionSet.Length,
            other.RegionSet.Length);

        // Get the index of the first differing element.
        var endIndex = Enumerable.Range(0, minLength).FirstOrDefault(
            i => !this.RegionSet[i].Equals(other.RegionSet[i]),
            minLength); // Idx = min length if no differences found.

        // DIFFERENT: Get spatial region of overlap (smaller region).
        regionSetOfOverlap = (this.RegionSet.Length <= other.RegionSet.Length) ?
            this.RegionSet : other.RegionSet;

        // Return T/F that any similar elements were found.
        return endIndex > 0;
    }

    /// <summary>
    /// Check if another spec's region shares any parent path spec.
    /// </summary>
    /// <param name="other">The other StringHierarchySpec with which to check
    /// for a shared path.</param>
    /// <param name="regionSetOfOverlap">The shared parent region path spec.
    /// Ignore if no shared path hierarchy is found.
    /// </param>
    /// <returns>T/F if a shared parent path is found.</returns>
    public bool RegionSetSharesPath(StringHierarchySpec other,
        out string[] sharedRegionSet)
    {
        // Shares path if first differing element is not the first one.

        // Only search up to the length of the shorter path.
        var minLength = Math.Min(this.RegionSet.Length,
            other.RegionSet.Length);

        // Get the index of the first differing element.
        var endIndex = Enumerable.Range(0, minLength).FirstOrDefault(
            i => !this.RegionSet[i].Equals(other.RegionSet[i]),
            minLength); // Idx = min length if no differences found.

        // DIFFERENT: Get the shared path spec (up to first differing idx).
        sharedRegionSet = this.RegionSet.Take(endIndex).ToArray();
        // Return T/F that any similar elements were found.
        return endIndex > 0;
    }

    public bool ModifiersAllowOverlap(StringHierarchySpec other,
        out string[] commonModifiers)
    {
        // Overlaps if all modifiers in shorter set are in longer set.
        string[] lessSpecificModSet; // Temp variables to store mod set references.
        string[] moreSpecificModSet;
        if (this.ModifierSet.Length < other.ModifierSet.Length)
        {
            lessSpecificModSet = this.ModifierSet;
            moreSpecificModSet = other.ModifierSet;
        }
        else
        {
            lessSpecificModSet = other.ModifierSet;
            moreSpecificModSet = this.ModifierSet;
        }
        commonModifiers = lessSpecificModSet.Intersect(moreSpecificModSet)
            .ToArray();
        //bool modifiersOverlap = shorterModSet.All(m => longerModSet.Contains(m));
        return commonModifiers.Length == lessSpecificModSet.Length;
    }

    public bool TryGetOverlap(StringHierarchySpec other,
        out string overlappingRegion, out bool contains)
    {
        // Check if the region specs actually have a common sequence.
        bool RegionSetsOverlap = this.RegionSetOverlaps(other,
            out var overlappingRegionSet);
        // This contains other if all this's elements in other.
        bool containsRegionSet = RegionSetsOverlap &&
            overlappingRegionSet.Length == this.RegionSet.Length;

        // Overlaps if all modifiers in shorter set are in longer set.
        bool modifiersOverlap = this.ModifiersAllowOverlap(other,
            out var commonModifiers);
        // Contains if all this's modifiers shared.
        bool contiansModifiers = commonModifiers.Length ==
            this.ModifierSet.Length;

        // Determine overall overlap and containment.
        bool overlaps = RegionSetsOverlap && modifiersOverlap;
        contains = containsRegionSet && contiansModifiers;

        // Determine overlapping region: take most specific region and mods.
        overlappingRegion = overlaps ?
            // Region spec: other if contained (more specific), else this.
            JoinFullSpec(containsRegionSet ? other.RegionSet : this.RegionSet,
                this.ModifierSet.Union(other.ModifierSet).ToArray())
            : string.Empty;     // Empty if no overlap.

        return overlaps;
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
}

