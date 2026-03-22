using System.Collections.Immutable;

namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;

public record StringHierarchySpec(string[] regionSet, string[] modifierSet)
{
    /// <summary>
    /// The ordered sequence of optioned region names in this spec, normalized to
    /// lowercase with edge whitespace trimmed. Null input arrays or null/empty
    /// elements are silently dropped.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown during construction if no
    /// non-empty region remains after normalization.</exception>
    public string[] RegionSet { get; } = _ValidateNonEmpty(
        (regionSet ?? Array.Empty<string>())
            .Where(r => r != null)
            .Select(r => r.Trim().ToLower())
            .Where(r => r.Length > 0)
            .ToArray());
    
    /// <summary>
    /// The set of directional modifiers in this spec, normalized to lowercase
    /// with edge whitespace trimmed. May be empty. Null input arrays or
    /// null/empty elements are silently dropped.
    /// </summary>
    public string[] ModifierSet { get; } = (modifierSet ?? Array.Empty<string>())
        .Where(m => m != null)
        .Select(m => m.Trim().ToLower())
        .Where(m => m.Length > 0)
        .ToArray();
    public string FullSpec => JoinFullSpec(this.RegionSet, this.ModifierSet);
    public string RegionSpec => JoinRegionSet(this.RegionSet);
    public string ModifierSpec => JoinModifierSet(this.ModifierSet);

    // Throws if the normalized region set is empty; used by RegionSet initializer.
    private static string[] _ValidateNonEmpty(string[] regions)
    {
        if (regions.Length == 0)
            throw new ArgumentException(
                "A StringHierarchySpec requires at least one non-empty region.",
                "regionSet");
        return regions;
    }

    /// <summary>
    /// Delimiter character separating region option(s) from the base region 
    /// name.
    /// </summary>
    public const char OPTION_REGION_DELIMITER = ' ';
    /// <summary>
    /// Delimiter used to <b>join</b> region or modifier names into their spec
    /// strings (output). Input parsing splits on <c>,</c> alone, so surrounding
    /// whitespace is tolerated and empty tokens are filtered out.
    /// </summary>
    public const char REGIONS_DELIMITER = ',';
    /// <summary>
    /// Delimiter used to <b>join</b> the region spec and modifier spec into a
    /// full spec string (output). Input parsing splits on the first <c>|</c>
    /// character regardless of surrounding whitespace; content after a second
    /// <c>|</c> is ignored.
    /// </summary>
    public const char REGIONS_MODIFIERS_DELIMITER = '|';
    /// <inheritdoc cref="REGIONS_DELIMITER"/>
    public const char MODIFIERS_DELIMITER = REGIONS_DELIMITER;

    /// <summary>
    /// An example of a full string specification demonstrating the expected
    /// format: optioned region names joined by <see cref="REGIONS_DELIMITER"/>,
    /// optionally followed by <see cref="REGIONS_MODIFIERS_DELIMITER"/> and
    /// modifiers joined by <see cref="MODIFIERS_DELIMITER"/>.
    /// </summary>
    public static readonly string ExamplePath = $"option1{OPTION_REGION_DELIMITER}" +
        $"region1{REGIONS_DELIMITER} no-option-region2{REGIONS_DELIMITER} ..." +
        $" {REGIONS_MODIFIERS_DELIMITER} modifier{MODIFIERS_DELIMITER} ...";

    /// <summary>
    /// Create a new StringHierarchySpec from a full modified region string 
    /// specification. Only syntactical parsing occurs, no semantic validation.
    /// All string components will be converted to lowercase and trimmed of 
    /// edge whitespace.
    /// </summary>
    /// <param name="fullSpec">The full string specification, delimited 
    /// appropriately.</param>
    /// <exception cref="ArgumentException">Thrown if no non-empty region is
    /// found after parsing.</exception>
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
    /// <exception cref="ArgumentException">Thrown if the region set is empty
    /// after dropping null/whitespace-only elements.</exception>
    public StringHierarchySpec(
        (string[] RegionSet, string[] ModifierSet) tuple) :
        this(tuple.RegionSet, tuple.ModifierSet)
    {
    }

    /// <summary>
    /// Parse a full string hierarchy specification into the optioned region
    /// names and modifier list. Tolerant of whitespace variations and malformed
    /// input: splits on the first <c>|</c> character (no surrounding spaces
    /// required), ignores everything after a second <c>|</c>, and delegates to
    /// <see cref="ParseRegionSpec"/> and <see cref="ParseModifierSpec"/> which
    /// split on <c>,</c> and filter empty tokens. Returns empty arrays if the
    /// input is null or whitespace-only.
    /// </summary>
    /// <param name="fullSpec">The full modified region string specification.
    /// </param>
    /// <returns>A tuple of string arrays: the first is the ordered optioned
    /// region no non-empty modifiers are present.</returns>
    public static (string[] regionSet, string[] modifierSet) ParseFullSpec(
        string fullSpec)
    {
        if (string.IsNullOrWhiteSpace(fullSpec))
            return (Array.Empty<string>(), Array.Empty<string>());

        // Split on the first '|' only; content after a second '|' is ignored.
        var delimIdx = fullSpec.IndexOf(REGIONS_MODIFIERS_DELIMITER);
        string regionPart, modifierPart;
        if (delimIdx < 0)
        {
            regionPart = fullSpec;
            modifierPart = string.Empty;
        }
        else
        {
            regionPart = fullSpec[..delimIdx];
            var modifierRaw = fullSpec[(delimIdx + 1)..];
            var secondDelimIdx = modifierRaw.IndexOf(REGIONS_MODIFIERS_DELIMITER);
            modifierPart = secondDelimIdx >= 0
                ? modifierRaw[..secondDelimIdx]
                : modifierRaw;
        }

        return (ParseRegionSpec(regionPart), ParseModifierSpec(modifierPart));
    }

    /// <summary>
    /// Parse a region specification string into a string array of optioned
    /// region names, converted to lower case and trimmed of edge whitespace.
    /// </summary>
    /// <param name="regionSpec">The string region specification, i.e., the 
    /// first half of a full string specification.</param>
    /// <returns>The string array of optioned region names, split on <c>,</c>
    /// (whitespace around it tolerated), converted to lowercase, trimmed, and
    /// with any empty tokens removed.</returns>
    public static string[] ParseRegionSpec(string regionSpec)
    {
        return regionSpec.Split(REGIONS_DELIMITER)
            .Select(s => s.Trim().ToLower())
            .Where(s => s.Length > 0)
            .ToArray();
    }

    /// <summary>
    /// Parse a modifier specification string into a string array of modifiers,
    /// converted to lower case and trimmed of edge whitespace.
    /// </summary>
    /// <param name="modifierSpec">The string modifier specification, i.e., the
    /// second half of a full string specification.</param>
    /// <returns>The string array of modifiers, split on <c>,</c> (whitespace
    /// around it tolerated), converted to lowercase, trimmed, and with any
    /// empty tokens removed.</returns>
    public static string[] ParseModifierSpec(string modifierSpec)
    {
        return modifierSpec.Split(MODIFIERS_DELIMITER)
            .Select(s => s.Trim().ToLower())
            .Where(s => s.Length > 0)
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
            $"{regionSpec} {REGIONS_MODIFIERS_DELIMITER} {modifierSpec}" :
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
        return string.Join($"{REGIONS_DELIMITER} ", regionSet);
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
        return string.Join($"{MODIFIERS_DELIMITER} ", modifierSet);
    }

    /// <summary>
    /// Parse zero or more option tokens and the base name from a full optioned
    /// region name. The last whitespace-separated token is the base name; all
    /// preceding tokens are options. Extra/multiple whitespace between tokens
    /// are collapsed. Returns false only if the input is null or whitespace-only.
    /// </summary>
    /// <param name="optionedRegionName">The full optioned region name to parse.
    /// </param>
    /// <param name="baseName">An output parameter: the last whitespace-separated
    /// token upon success. An empty string upon failure.</param>
    /// <param name="options">An output parameter: all preceding tokens joined
    /// by a single space (e.g., <c>"left index"</c> for input
    /// <c>"left index finger"</c>), or an empty string if there are none.
    /// </param>
    /// <returns>True if at least one token is present; false for empty or
    /// whitespace-only input.</returns>
    public static bool TryParseOptionedRegionName(string optionedRegionName,
        out string baseName, out string options)
    {
        // Check if input is empty
        if (string.IsNullOrWhiteSpace(optionedRegionName))
        {
            baseName = options = "";
            return false;
        }

        // Split by OPTION_REGION_DELIMITER, collapsing multiple.
        var nameElements = optionedRegionName
            .Split(OPTION_REGION_DELIMITER,
                StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Where(e => e.Length > 0)
            .ToArray();

        // Fail if no tokens are found after splitting and filtering.
        if (nameElements.Length == 0)
        {
            baseName = options = "";
            return false;
        }

        // Last token is the base region name; all preceding tokens are options.
        baseName = nameElements[^1];
        options = string.Join(OPTION_REGION_DELIMITER, nameElements[..^1]);
        return true;
    }

    /// <summary>
    /// Attempts to parse an options string into an array of individual options.
    /// </summary>
    /// <param name="options">The string containing options separated by OPTION_REGION_DELIMITER.</param>
    /// <param name="parsedOptions">An output parameter that contains the parsed array of option names.</param>
    /// <returns>True if at least one valid option is found; otherwise, false.</returns>
    public static bool TryParseOptions(string options, out string[] parsedOptions)
    {
        // If the input is null or empty, return false with an empty array
        if (string.IsNullOrWhiteSpace(options))
        {
            parsedOptions = Array.Empty<string>();
            return false;
        }

        // Split options by OPTION_REGION_DELIMITER, remove empty elements, and trim each part
        parsedOptions = options
            .Split(OPTION_REGION_DELIMITER, StringSplitOptions.RemoveEmptyEntries)
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrEmpty(option)) // Ensure no empty options remain
            .ToArray();

        // Return true if at least one valid option is found, false otherwise
        return parsedOptions.Length > 0;
    }


    // TODO: decide to delete or not once new overlap implementation tested in bodymodel
    /// <summary>
    /// Check if another spec's region shares any parent path spec.
    /// </summary>
    /// <param name="other">The other StringHierarchySpec with which to check
    /// for a shared path.</param>
    /// <param name="sharedRegionSet">The shared parent region path spec.
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

    // TODO: delete or rename once new overlap implementation tested in bodymodel
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

    /// <summary>
    /// Serves as the default hash function.
    /// Computes a combined hash code based on the exact sequence of the <see cref="RegionSet"/> 
    /// and the order-independent elements of the <see cref="ModifierSet"/>.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var item in this.RegionSet)
        {
            hash.Add(item);
        }

        foreach (var item in this.ModifierSet.OrderBy(x => x))
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether the specified <see cref="StringHierarchySpec"/> is equal to the current object.
    /// Equality requires an exact sequence match for the <see cref="RegionSet"/> and an 
    /// order-independent match for the <see cref="ModifierSet"/>.
    /// </summary>
    /// <param name="other">The <see cref="StringHierarchySpec"/> to compare with the current object.</param>
    /// <returns>
    /// <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool Equals(StringHierarchySpec? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.RegionSet.SequenceEqual(other.RegionSet) &&
            this.ModifierSet.OrderBy(x => x).SequenceEqual(other.ModifierSet.OrderBy(x => x));
    }

    public override string ToString()
    {
        return this.FullSpec;
    }


}

