namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;


/// <summary>
/// StringHierarchyRegions are nodes in a nodal graph construction of a string
/// hierarchy body model.
/// </summary>
public class StringHierarchyRegion
{
    /// <summary>
    /// The base name of this region, e.g., hand.
    /// </summary>
    public string BaseName { get; init; }

    /// <summary>
    /// The parent region of this region. Null if this region is the root 
    /// region.
    /// </summary>
    public StringHierarchyRegion? ParentRegion { get; set; }

    /// <summary>
    /// All region options specified through the path through the parent tree
    /// to this region.
    /// </summary>
    public HashSet<string> ParentOptions { get; set; }
    /// <summary>
    /// Options of this base region that can be selected, e.g., [left, right].
    /// Ensured to be non-null. Empty if none.
    /// </summary>
    public HashSet<string> Options { get; set; }
    /// <summary>
    /// Whether or not there are options that must be specified at this base 
    /// region.
    /// </summary>
    public bool HasOptions => this.Options.Count > 0;
    /// <summary>
    /// All optioned region names that can be specified at this base region.
    /// </summary>
    public List<string> OptionedRegionNames
    {
        get
        {
            // If there are options, return all option+region names.
            if (this.HasOptions)
            {
                return this.Options.Select(
                    option => $"{option}" +
                        $"{StringHierarchySpec.OPTION_REGION_DELIMITER}" +
                        $"{this.BaseName}").ToList();
            }
            // Else, just include the base name.
            else
            {
                return new() { this.BaseName };
            }
        }
    }
    
    /// <summary>
    /// Directional modifiers that could be applied to this region. Dictionary
    /// keyed by directional axis name. Value set contains possible modifiers
    /// along that axis. Ensured to be non-null. Empty if none.
    /// </summary>
    public Dictionary<string, HashSet<string>> Modifiers
    {
        get => _modifiers;
        set
        {
            _modifiers = value;
            _UpdateFrequencyDict();
        }
    }
    /// <summary>
    /// Whether or not there are directional modifiers specified at this base 
    /// region.
    /// </summary>
    public bool HasModifiers => this.Modifiers.Count > 0;
    /// <summary>
    /// Directional modifiers that could be applied to this region. Dictionary
    /// keyed by directional axis name. Value set contains possible modifier
    /// values along that axis. Ensured to be non-null. Empty if none. Accessed
    /// by the Modifiers property.
    /// </summary>
    private Dictionary<string, HashSet<string>> _modifiers = new();
    /// <summary>
    /// Dictionary of how frequently a modifier value occurs across all 
    /// specified modifier axes. Keyed by modifier value. Integer value is how
    /// many axes contain that modifier value. Used when parsing string 
    /// hierarchy modifier specs to handle axes with the same modifier value,
    /// e.g., x: center, y: center. Updated by the Mofidiers property setter.
    /// </summary>
    private Dictionary<string, int> _modifierFrequencyDict = new();

    /// <summary>
    /// The subregions of this region, keyed by string name. Ensured to be
    /// non-null. Empty if none, meaning this region is a "leaf" in the nodal
    /// graph.
    /// </summary>
    public Dictionary<string, StringHierarchyRegion> Subregions
    {
        get;
        protected set;
    }
    /// <summary>
    /// Whether or not this region has subregions.
    /// </summary>
    public bool HasSubregions => this.Subregions.Count > 0;
    /// <summary>
    /// Whether or not this region is a "leaf" in the nodal graph, i.e., has no
    /// subregions.
    /// </summary>
    public bool IsLeaf => this.Subregions.Count == 0;

    /// <summary>
    /// Set of local IDs of saved locations with a region spec to this region.
    /// </summary>
    public SortedSet<int> SavedLocations { get; set; }
    /// <summary>
    /// Set of local IDs of saved areas with a region spec to this region.
    /// </summary>
    public SortedSet<int> SavedAreas { get; set; }

    /// <summary>
    /// Create a new StringHierarchyRegion with the given base name and parent
    /// region. Optionally provide parent options, region options, modifiers,
    /// and subregions. Ensures all properties non-null after construction.
    /// Deep copies made of collections passed in but NOT of any referenced 
    /// StringHierarchyRegions. References retained to those objects.
    /// </summary>
    /// <param name="baseName">The base name of this region, e.g., "hand".
    /// </param>
    /// <param name="parent">The parent region of this region (default: null, 
    /// meaning root region).</param>
    /// <param name="parentOptions">The specification of option values passed
    /// to this region by the parent region (default: empty).</param>
    /// <param name="options">The possible option values for this region 
    /// (default: empty)</param>
    /// <param name="modifiers">The directional modifiers that can be specified 
    /// at this region (default: empty). Key by modifier axis name. Valued by 
    /// set of possible modifier values along that directional axis.</param>
    /// <param name="subregions">The subregions of this region, if any 
    /// (default: empty). If none, this region is a "leaf" in the nodal graph.
    /// </param>
    public StringHierarchyRegion(string baseName,
        StringHierarchyRegion? parent = null,
        // Note: the ! alleviates null warning, promising the value will be set
        // to not-null in the constructor.
        HashSet<string> parentOptions = null!,
        HashSet<string> options = null!,
        Dictionary<string, HashSet<string>> modifiers = null!,
        Dictionary<string, StringHierarchyRegion> subregions = null!)
    {
        // Store base name and reference to parent region.
        this.BaseName = baseName;
        this.ParentRegion = parent;

        // Deep copy options and modifier collection structs.
        this.ParentOptions = (parentOptions is not null) ? new(parentOptions) :
            new();
        this.Options = (options is not null) ? new(options) : new();
        this.Modifiers = (modifiers is not null) ? modifiers.ToDictionary(
            kvp => kvp.Key, kvp => new HashSet<string>(kvp.Value)) :
            new();
        // Copy the dictionary but keep references to same subregion objects.
        this.Subregions = (subregions is not null) ? new(subregions) : new();

        // Initialize sets noting the IDs of saved locations and areas that 
        // point to this region.
        this.SavedLocations = new();
        this.SavedAreas = new();
    }

    /// <summary>
    /// Add a given region as a subregion of this region. Replaces stored
    /// subregion of the same subregion base name if exists but does not alter
    /// any existing subregion. Sets the parent reference of the added subregion
    /// to be this region.
    /// </summary>
    /// <param name="subregion">The subregion to add. Shallow copied. Parent
    /// reference set.</param>
    /// <param name="existingSubregion">An output parameter: the replaced but
    /// unaltered existing subregion of the same base name if any, else null.
    /// </param>
    /// <exception cref = "ArgumentNullException">The provided subregion
    /// argument is null.</exception>
    public void AddSubregion(StringHierarchyRegion subregion,
        out StringHierarchyRegion? existingSubregion)
    {
        // Fill the output parameter with the existing subregion if exists.
        if (this.Subregions.TryGetValue(subregion.BaseName,
            out existingSubregion))
        {
            // Replace the existing subregion at this basename with the new
            // one.
            this.Subregions[subregion.BaseName] = subregion;
        }
        // Else add the new subregion keyed by its basename.
        else
        {
            this.Subregions.Add(subregion.BaseName, subregion);
        }

        // Change the parent of the newly added subregion to be this region.
        subregion.ParentRegion = this;
    }

    /// <summary>
    /// Try to get a given subregion of this region.
    /// </summary>
    /// <param name="regionSpec">The string specification of the region to 
    /// search for, given as a string sequence of appropriately delimited 
    /// option-region names.</param>
    /// <param name="foundSubregion">An output parameter: the searched
    /// subregion if found, null if not.</param>
    /// <returns>True if the subregion could be found, False if not.</returns>
    public bool TryGetSubregion(string regionSpec,
        out StringHierarchyRegion? foundSubregion)
    {
        // Split full region spec into sequence of option+region names.
        var regionSet = StringHierarchySpec.ParseRegionSpec(regionSpec);

        // Navigate the nodal graph to find the region.
        // Start searching in this region.
        foundSubregion = this;
        // Bool to indicate if current search iteration should check against
        // the current region or subregions.
        bool searchCurrentRegion = true;
        // Start with the first option+region name in the given full name.
        foreach (var optionedRegionName in regionSet)
        {
            // Split search name into base name and option. Fail if
            // invalid format.
            if (!StringHierarchySpec.TryParseOptionedRegionName(
                optionedRegionName, out var searchBaseName,
                out var searchOption))
            {
                foundSubregion = null;
                return false;
                /// <exception cref="ArgumentException">The given full region name is
                /// improperly formatted and cannot be parsed.Should exactly contain 0 or 1
                /// option string elements and 1 base name element.</exception>
                //// TODO: put in error code or prefix or something
                //throw new ArgumentException($"ERROR: '{optionedRegionName}' is an " +
                //    $"invalid string hierarchy region name. Must be a 'baseName' " +
                //    $"or 'option{OPTION_REGION_DELIMITER}baseName'.");
            }

            // If searching in current region, compare option+region names.
            if (searchCurrentRegion)
            {
                // If search name found, look for next item in subregions.
                if (foundSubregion!.OptionedRegionNames.Contains(
                    optionedRegionName))
                {
                    searchCurrentRegion = false;
                }
                // Else, fail.
                else
                {
                    foundSubregion = null;
                    return false;
                }
            }
            // Else search subregion for the option+region name.
            else
            {
                // Check if search basename is in subregions.
                // Sets foundSubregion to the subregion if found.
                bool viableSubregion = foundSubregion!.Subregions.TryGetValue(
                    searchBaseName, out foundSubregion);
                
                // Fail if search basename not found in subregions or search 
                // option exists and not found in subregion matching search 
                // basename.
                if (!(viableSubregion &&
                    (searchOption.Equals("") ||
                    foundSubregion!.Options.Contains(searchOption))))
                {
                    foundSubregion = null;
                    return false;
                }

                // Else, search continues in located matching subregion.
            }
        }

        // If made it here, subregion is found. Output param already filled.
        // Return success.
        return true;
    }

    /// <summary>
    /// Check if a given modifier specification is valid within this region.
    /// Valid if:
    ///     - formatted (delimited) correctly
    ///     - all modifiers values are found in the possible value sets of the 
    ///       modifier axes of this region
    ///     - each modifier axis is used at most once
    /// </summary>
    /// <param name="modifierSpec">The modifier specification to check.</param>
    /// <returns>T/F if the spec is valid in this region.</returns>
    public bool IsValidModifierSpec(string modifierSpec)
    {
        // Parse modifier spec into a set of modifier values.
        var modifierSet = StringHierarchySpec.ParseModifierSpec(modifierSpec);
        // Sort modifiers by their frequency across axis option sets, ascending
        // order so duplicate modifier values (e.g., "center" as a valid value
        // on two axes) doesn't use the only axis another modifier value may be
        // valid for. I.e., duplicate modifier values "used" for the most 
        // restricted axis first.
        // TODO: FIX THIS!!!
        var sortedModifierSet = modifierSet.OrderBy(modifier =>
            this._modifierFrequencyDict.ContainsKey(modifier) ?
            this._modifierFrequencyDict[modifier] : 0)
            .ToList();

        // Foreach modifier, search for it in the value set of all unused
        // modifier axes.
        List<string> unusedAxes = new(this.Modifiers.Keys);

        foreach (var modifier in modifierSet)
        {
            bool found = false;
            string usedAxis;
            for (int i = 0; i < unusedAxes.Count; i++)
            {
                usedAxis = unusedAxes[i];
                // If this axis contains the modifier, mark the modifier as
                // found and the axis as used.
                if (this.Modifiers[usedAxis].Contains(modifier))
                {
                    found = true;
                    unusedAxes.Remove(usedAxis);
                    break;
                }
            }
            // If modifier value could not be found in remaining axes, fail.
            if (!found)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Update the frequency dictionary of modifier values across all axes
    /// based on current Modifiers.
    /// </summary>
    private void _UpdateFrequencyDict()
    {
        // Clear the frequency dictionary
        this._modifierFrequencyDict.Clear();

        // Flatten the value sets and update the frequency dictionary.
        foreach (var set in _modifiers.Values)
        {
            foreach (var item in set)
            {
                if (this._modifierFrequencyDict.ContainsKey(item))
                {
                    this._modifierFrequencyDict[item]++;
                }
                else
                {
                    this._modifierFrequencyDict[item] = 1;
                }
            }
        }
    }

    /// <summary>
    /// Create a deep copy of the whole subtree starting at this region.
    /// </summary>
    /// <param name="retainParentReference">True (default) to keep the parent
    /// reference of this region in the deep copy, False to reset the deep
    /// copy's parent region to null.</param>
    /// <returns>The deep copy of this region, linking to deep copies of all
    /// subregions.</returns>
    public StringHierarchyRegion DeepCopy(bool retainParentReference = true)
    {
        var parentRegion = (retainParentReference) ? this.ParentRegion : null;
        // Create new region. Inherently deep copies options and modifiers.
        var newRegion = new StringHierarchyRegion(this.BaseName, parent: parentRegion!,
            parentOptions: this.ParentOptions, options: this.Options, 
            modifiers: this.Modifiers);

        // Add deep copies of all subregions.
        foreach (var (_, subregion) in this.Subregions)
        {
            // Create deep copy of subregion without keeping parent reference.
            // Add deep copied subregion to new region, setting parent ref.
            newRegion.AddSubregion(subregion.DeepCopy(
                retainParentReference: false), out _);
        }

        return newRegion;
    }

    /// <summary>
    /// Create a string representation of this region and all subregions.
    /// Override default behavior to provide a more detailed string output.
    /// </summary>
    /// <returns>A string representation of this region.</returns>
    public override string ToString()
    {
        return s_BuildSpecOptionsString(this);
    }

    /// <summary>
    /// Create a string representation of the given region, recursing to contain
    /// all subregions.
    /// </summary>
    /// <param name="region">The region to build a string representation of.
    /// </param>
    /// <param name="parentRegionSpec">The string specification of the parent 
    /// region (default: empty string, assuming given region is a root).
    /// </param>
    /// <param name="indentLevel">Depth in the tree and thus number of indents
    /// to included in the string output (default: 0, assuming given region is 
    /// a root).</param>
    /// <returns>A printable string representation of the given region.
    /// </returns>
    private static string s_BuildSpecOptionsString(StringHierarchyRegion region,
        string parentRegionSpec = "", int indentLevel = 0)
    {
        // Output: [prev regionSpec], [options] baseName | [mod1Options], ...

        // List of comma-separated options: option1, option2, ...
        var options = $"{string.Join(',', region.Options)}";
        // This region's spec with options list [] if any, else just base name.
        var localRegionSpec = (options.Length != 0) ?
            $"[{options}]" + StringHierarchySpec.OPTION_REGION_DELIMITER +
            region.BaseName :
            region.BaseName;
        // Full region spec, including parent spec if any, else just local spec.
        var regionSpec = (parentRegionSpec.Length != 0) ?
            $"{parentRegionSpec}, {localRegionSpec}" :
            localRegionSpec;

        // Build the modifier spec.
        List<string> modifierLists = new();
        foreach (var (_, modifierSet) in region.Modifiers)
        {
            modifierLists.Add($"[{string.Join(',', modifierSet)}]");
        }
        var modifierSpec = string.Join(StringHierarchySpec.MODIFIERS_DELIMITER,
            modifierLists);

        // The full line for this region: {indent}{fullPathSpec}{DELIM}{mods}
        var indent = new string(' ', indentLevel * 4);
        var fullSpec = $"{indent}{regionSpec}" +
            ((modifierSpec.Length == 0) ? "" :
            $"{StringHierarchySpec.REGIONS_MODIFIERS_DELIMITER}{modifierSpec}");

        // Recursively get subregion spec strings. Init final list w/ this spec.
        List<string> subregionStrings = new() { fullSpec };
        foreach (var (_, subregion) in region.Subregions)
        {
            subregionStrings.Add(s_BuildSpecOptionsString(subregion, 
                parentRegionSpec: regionSpec,
                indentLevel: indentLevel + 1));
        }

        // Return the single string.
        return string.Join('\n', subregionStrings);
    }
}

