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
    /// The parent region of this region.
    /// </summary>
    public StringHierarchyRegion ParentRegion { get; set; }
    /// <summary>
    /// The all possible option strings through the parent tree to get to this
    /// region.
    /// </summary>
    public HashSet<string> ParentOptions { get; set; }
    /// <summary>
    /// Options of the base region that can be selected, e.g., [left, right].
    /// Ensured to be non-null. Empty if none.
    /// </summary>
    public HashSet<string> Options { get; set; }
    public bool HasOptions => this.Options.Count > 0;
    public List<string> OptionedRegionNames
    {
        get
        {
            // If there are options, return all option+region names.
            if (this.HasOptions)
            {
                return this.Options.Select(
                    option => $"{option}{StringHierarchySpec.OPTION_REGION_DELIMITER}" +
                    $"{this.BaseName}").ToList();
            }
            // Else, add include the base name.
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
    public bool HasModifiers => this.Modifiers.Count > 0;
    private Dictionary<string, HashSet<string>> _modifiers;
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
    public bool HasSubregions => this.Subregions.Count > 0;
    public bool IsLeaf => this.Subregions.Count == 0;

    /// <summary>
    /// Set of local IDs of saved locations with a region spec to this region.
    /// </summary>
    public SortedSet<int> SavedLocations { get; set; }
    /// <summary>
    /// Set of local IDs of saved areas with a region spec to this region.
    /// </summary>
    public SortedSet<int> SavedAreas { get; set; }

    // Deep copies made of data structs but NOT of any referenced
    // StringHierarchyRegions. References retained to those objects.
    public StringHierarchyRegion(string baseName, StringHierarchyRegion parent,
        HashSet<string> parentOptions = null,
        HashSet<string> options = null,
        Dictionary<string, HashSet<string>> modifiers = null,
        Dictionary<string, StringHierarchyRegion> subregions = null)
    {
        this.BaseName = baseName;
        this.ParentRegion = parent;
        // Deep copy options and modifier structs.
        this.ParentOptions = (parentOptions is not null) ? new(parentOptions) :
            new();
        this.Options = (options is not null) ? new(options) : new();
        this.Modifiers = (modifiers is not null) ? modifiers.ToDictionary(
            kvp => kvp.Key, kvp => new HashSet<string>(kvp.Value)) :
            new();
        // Copy the dictionary but keep references to same subregion objects.
        this.Subregions = (subregions is not null) ? new(subregions) : new();

        this.SavedLocations = new();
        this.SavedAreas = new();
    }

    /// <summary>
    /// Add a given region as a subregion of this region. Replaces stored
    /// subregion of the same subregion base name if exists but does not alter
    /// any existing subregion. Sets the parent reference of the added subregion
    /// to be this region.
    /// </summary>
    /// <param name="subregion">The subregion to add. Shallow copied.
    /// Parent reference set.</param>
    /// <param name="existingSubregion">An output parameter: the replaced but
    /// unaltered existing subregion if any, else null.</param>
    public void AddSubregion(StringHierarchyRegion subregion,
        out StringHierarchyRegion? existingSubregion)
    {
        // Fill the out parameter with the existing subregion if exists.
        if (this.Subregions.TryGetValue(subregion.BaseName,
            out existingSubregion))
        {
            // Overwrite the stored subregion with this basename.
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

    // TODO: TEST THE HECK OUT OF THIS
    /// <summary>
    /// Try to get a given subregion of this region.
    /// </summary>
    /// <param name="regionSpec">The specified region to search for, given as a 
    /// string sequence of appropriately delimited option-region names.</param>
    /// <param name="foundSubregion">An output parameter: the searched
    /// subregion if found, null if not.</param>
    /// <returns>True if the subregion could be found, False if not.</returns>
    public bool TryGetSubregion(string regionSpec,
        out StringHierarchyRegion foundSubregion)
    {
        // Split full linked name into sequence of option+region names.
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
                if (foundSubregion.OptionedRegionNames.Contains(
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
            // Else try to find matching subregion.
            else
            {
                // Fail if search region found in subregion, else search for next
                // item in that subregion's subregions.
                if (!(foundSubregion.Subregions.TryGetValue(searchBaseName,
                    out foundSubregion) && (searchOption.Equals("") ||
                    foundSubregion.Options.Contains(searchOption))))
                {
                    foundSubregion = null;
                    return false;
                }
            }
        }

        // If made it here, subregion is found. Fill output param and return.
        return true;
    }

    public bool IsValidModifierSpec(string modifierSpec)
    {
        // Split into modifier set.
        var modifierSet = StringHierarchySpec.ParseModifierSpec(modifierSpec);
        // Sort modifiers by their frequency across axis option sets, ascending
        // order so duplicate modifier values (e.g., "center" as a valid value
        // on two axes) doesn't use the only axis another modifier value may be
        // valid for.
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

    // TODO: TEST!!!
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
        var newRegion = new StringHierarchyRegion(this.BaseName, parentRegion,
            options: this.Options, modifiers: this.Modifiers);

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

    public override string ToString()
    {
        return s_BuildSpecOptionsString("", 0, this);
    }

    private static string s_BuildSpecOptionsString(string parentRegionSpec,
        int indentLevel, StringHierarchyRegion region)
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
            subregionStrings.Add(s_BuildSpecOptionsString(regionSpec,
                indentLevel + 1, subregion));
        }

        // Return the single string.
        return string.Join('\n', subregionStrings);
    }
}

