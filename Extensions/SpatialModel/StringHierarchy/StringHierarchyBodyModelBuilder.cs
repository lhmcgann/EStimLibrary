using Newtonsoft.Json.Linq;
using EStimLibrary.Core;
using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;


public class StringHierarchyBodyModelBuilder : BodyModelBuilderBase
{
    public override string Name => "StringHierarchyModelBuilder";// ISelectable
    // BodyModelBuilder property
    public override List<string> AvailableModelNames { get; init; }
    private Dictionary<string, StringHierarchyRegion> _availableBaseRegions
    { get; init; }

    protected HashSet<string> _RequiredModifiers;
    protected StringHierarchyRegion _RootRegion;
    protected const string ROOT_NAME = "root";

    // Everything in the json will be turned into lowercase.
    protected const string REQUIRED_MODIFIERS_KEY = "_requiredmodifiers";
    protected const string OPTIONS_KEY = "options";
    protected const string MODIFIERS_KEY = "modifiers";
    protected const string SUBREGIONS_KEY = "subregions";

    protected static string _ErrParse = "JSON Body Model Parse Error";
    protected static string _WarnParse = "JSON Body Model Parse Warning";
    protected static string _ErrBuild = "String Hierarchy Body Model Build Error";

    public StringHierarchyBodyModelBuilder(string jsonFilePath)
    {
        // Read the JSON file first. Throws exception if any errors.
        string jsonBodyDefinition = Utils.ReadJSON(jsonFilePath);

        // Parse the JSON string (make lowercase) into a JObject.
        JObject json = JObject.Parse(jsonBodyDefinition.ToLower());

        // Throw exception if the minimum of required modifiers and a region
        // definition are not given.
        var properties = json.Properties().ToList();
        if (properties.Count < 2)
        {
            throw new ArgumentException($"{_ErrParse}: Cannot parse " +
                $"{jsonFilePath}. Must include a '{REQUIRED_MODIFIERS_KEY}' " +
                $"property and at least one body region definition.");
        }

        // Get the required modifiers property, should be first property.
        var requiredModifierProperty = properties[0];
        // Throw exception if first property is not the required modifiers.
        if (!requiredModifierProperty.Name.Equals(REQUIRED_MODIFIERS_KEY))
        {
            throw new ArgumentException($"{_ErrParse}: Cannot parse " +
                $"{jsonFilePath}. First property must be " +
                $"'{REQUIRED_MODIFIERS_KEY}': [ \"string1\", ... ] ");
        }
        // Throw exception if not an array type for required modifiers.
        _CheckJSONPropertyType(requiredModifierProperty, JTokenType.Array);
        // If all defined correctly, store the required modifiers.
        this._RequiredModifiers = new(
            (requiredModifierProperty.Value as JArray).Values<string>());

        // Initialize class properties.
        this._availableBaseRegions = new();
        this._RootRegion = new(ROOT_NAME, null);

        // Iterate through the top-level body regions defined.
        foreach (var topRegionProperty in properties.Skip(1))
        {
            // Parse the top-level region.
            var region = _ParseJSONBodyRegion(this._RootRegion,
                topRegionProperty);
            // Store the region.
            this._RootRegion.Subregions.Add(region.BaseName, region);
        }
        // Fill the available model names property.
        this.AvailableModelNames = this._availableBaseRegions.Keys.ToList();

        // TODO: remove this print once done debugging
        Console.WriteLine(this._RootRegion);
    }

    // TODO: TEST THE HECK OUT OF THIS
    /// <summary>
    /// Parse the JSON body model definition into a nodal graph of string
    /// hierarchy regions.
    /// </summary>
    /// <param name="parentRegion">The previously parsed parent region to the
    /// body region to be returned by this method.</param>
    /// <param name="regionJson">The JSON property to try parsing into a body
    /// region node.</param>
    /// <param name="passedOptions">TODO</param>
    /// <param name="passedModifiers">A dictionary of the modifier axes and
    /// corresponding value sets to pass to this region from the parent region.
    /// Passed modifiers may be ignored if they are defined locally in this
    /// region. Neither this dictionary not the contained value sets shall be
    /// modified by this function call.</param>
    /// <returns>The 'root' node constructed from this JSON property.</returns>
    /// <exception cref="ArgumentException">TODO</exception>
    protected StringHierarchyRegion _ParseJSONBodyRegion(
        StringHierarchyRegion parentRegion,
        JProperty regionJson, HashSet<string> passedOptions = null,
        Dictionary<string, HashSet<string>> passedModifiers = null)

    {
        // Get the region name.
        string regionName = regionJson.Name;
        // Make sure the region body is another JObject. Throw exception if not.
        _CheckJSONPropertyType(regionJson, JTokenType.Object);
        // Get the body of the region definition.
        var details = (JObject)regionJson.Value;

        // Create the region.
        StringHierarchyRegion region = new(regionName, parentRegion,
            parentOptions: passedOptions);
        // Temp variable for subregions object, if any.
        JObject subregionsJson = null;

        // Extract the property values.
        foreach (var property in details.Properties())
        {
            switch (property.Name)
            {
                case OPTIONS_KEY:
                    // Throw exception if not given an array.
                    _CheckJSONPropertyType(property, JTokenType.Array);
                    // Extract and store the list of option sets.
                    region.Options = new((property.Value as JArray)
                        .Values<string>());
                    break;
                case MODIFIERS_KEY:
                    // Throw exception if not given a JObject.
                    _CheckJSONPropertyType(property, JTokenType.Object);
                    // Extract each modifier.
                    foreach (var modifierProperty in
                        ((JObject)property.Value).Properties())
                    {
                        // Throw exception if not given an array.
                        _CheckJSONPropertyType(modifierProperty,
                            JTokenType.Array);
                        // Print warning and skip if modifier name doesn't
                        // match one of the required ones.
                        if (!this._RequiredModifiers.Contains(
                            modifierProperty.Name))
                        {
                            Console.WriteLine($"{_WarnParse}: Modifier " +
                                $"'{modifierProperty.Name}' is not valid among " +
                                $"the set [{string.Join(',',
                                this._RequiredModifiers)}]. Skipping.");
                        }
                        else
                        {
                            // Store string modifier values in region dict.
                            region.Modifiers.Add(modifierProperty.Name,
                                new((modifierProperty.Value as JArray)
                                .Values<string>()));
                        }
                    }
                    break;
                case SUBREGIONS_KEY:
                    // Throw exception if not given a JObject.
                    _CheckJSONPropertyType(property, JTokenType.Object);
                    // Save the property value (subregions JObject) for later.
                    subregionsJson = (JObject)property.Value;
                    break;
                default:
                    // Skip any incorrect properties. Just inform the user.
                    Console.WriteLine($"{_WarnParse}: Did not recognize " +
                        $"'{regionName}' region's property: '{property.Name}'." +
                        $"Skipping.");
                    break;
            }
        }

        // Finish constructing the StringHierarchyRegion.

        // Store the passed modifiers in this region to fill out the required
        // modifier set. Ignore passed value set if local options defined.
        if (passedModifiers is not null)
        {
            foreach (var (modifier, valueSet) in passedModifiers)
            {
                // Do not pass value set if already exists.
                if (region.Modifiers.TryGetValue(modifier, out var localValues))
                {
                    // Alert the user.
                    Console.WriteLine($"{_WarnParse}: Region '{regionName}' " +
                        $"already has modifier '{modifier}' values:\n\t" +
                        $"[{string.Join(',', localValues)}]. Ignoring " +
                        $"passed values:\n\t[{string.Join(',', valueSet)}]");
                }
                // Else store the passed modifier value set (deep copy).
                else
                {
                    region.Modifiers.Add(modifier, new(valueSet));
                }
            }
        }

        // Throw exception if not all required modifiers are present.
        var missingModifiers = this._RequiredModifiers.Except(
            region.Modifiers.Keys);
        if (missingModifiers.Count() > 0)
        {

            throw new ArgumentException($"{_ErrParse}: Not all required " +
                $"modifiers [{string.Join(',', this._RequiredModifiers)}] " +
                $"defined for region '{regionName}'. Missing: " +
                $"[{string.Join(',', missingModifiers)}].");
        }

        // Build the new set of parent option strings.
        // Temp variables for readability.
        var set1 = region.ParentOptions;
        var set2 = region.Options;
        HashSet<string> propagatingOptions = (
            // Case where no parent or local options.
            set1.Count == 0 && set2.Count == 0) ? new() { } :
            // Cases where one of the sets empty. Return the other.
            (set1.Count == 0) ? set2 :
            (set2.Count == 0) ? set1 :
            // Case where both sets have options. Do Cartesian product.
            new(set1.SelectMany(o1 => set2, (o1, o2) =>
                $"{o1}{StringHierarchySpec.OPTION_REGION_DELIMITER}{o2}"));
        // Store each of those option strings as navigating to this region.
        // This is just for the body model builder to create models.
        // If no options, just store the base name.
        if (propagatingOptions.Count == 0)
        {
            this._availableBaseRegions.Add(region.BaseName, region);
        }
        foreach (var o in propagatingOptions)
        {
            this._availableBaseRegions.Add($"{o}" +
                $"{StringHierarchySpec.OPTION_REGION_DELIMITER}" +
                $"{region.BaseName}", region);
        }

        if (subregionsJson is not null)
        {
            // Make the recursive call for all subregions, passing modifiers.
            // the region's subregions dict will remain empty if no subregions.
            // Also store sub full region names.
            foreach (var subregionProperty in subregionsJson.Properties())
            {
                // Throw exception if not given a JObject.
                _CheckJSONPropertyType(subregionProperty, JTokenType.Object);
                // Recursive call.
                StringHierarchyRegion subregion = _ParseJSONBodyRegion(region,
                    subregionProperty, propagatingOptions,
                    region.Modifiers);
                // Store in this region's subregion dictionary.
                // Skip if the add fails (e.g., multiple subregions defined with
                // the same name).
                if (!region.Subregions.TryAdd(subregion.BaseName, subregion))
                {
                    Console.WriteLine($"{_WarnParse}: Could not add " +
                        $"subregion '{subregion.BaseName}' to region " +
                        $"'{regionName}' with current subregions: " +
                        $"[{string.Join(',', region.Subregions.Keys)}]. " +
                        $"Skipping.");
                }
            }
        }

        // Return the now fully filled region.
        return region;
    }

    protected static void _CheckJSONPropertyType(JProperty property,
        JTokenType expectedType)
    {
        var actualType = property.Value.Type;
        if (actualType != expectedType)
        {
            throw new ArgumentException($"{_ErrParse}: '{property.Name}' " +
                $"was not of type {expectedType}. Instead received " +
                $"{actualType}: {property.Value}");
        }
    }

    public static string GetTemplateJSONString()
    {
        // TODO: figure out how to use the protected class const variables here
        // The example/template JSON structure as a string.
        string exampleJsonString = @"{
            ""_requiredModifiers"": [ ""x"", ""y"", ""z"" ],
            ""baseBodyRegion1"": {
                ""options"": [ ""independentOptionA1"", ""independentOptionAN"" ],
                ""modifiers"": {
                    ""x"": [ ""medial"", ""central"", ""lateral"" ],
                    ""y"": [ ""superior"", ""inferior"" ],
                    ""z"": [ ""anterior"", ""posterior"" ]
                },
                ""subregions"": {
                    ""subregion1"": {
                        ""modifiers"": {
                            ""y"": [ ""proximal"", ""intermediate"", ""distal"" ]
                        }
                    },
                    ""subregionN"": {
                        ""modifiers"": {
                            ""y"": [ ""rostral"", ""middle"", ""caudal"" ]
                        }
                    }
                }
            },
            ""baseBodyRegionN"": {
            }
        }";

        //// TODO: Might be able to just directly return the above string, but
        //// unsure. Keep the steps below for now.

        //// Deserialize the JSON string into a dynamic object for pretty printing.
        //dynamic jsonObject = JsonConvert.DeserializeObject(exampleJsonString);

        //// Serialize the dynamic object to a formatted JSON string.
        //string formattedJsonString = JsonConvert.SerializeObject(jsonObject,
        //    Formatting.Indented);

        //return formattedJsonString;
        return exampleJsonString;
    }

    public override bool TryCreate(string parentOptionedRegionSpec,
        out IBodyModel bodyModel)
    {
        // Try locating the region. Throws exception if invalid name format.
        if (this._availableBaseRegions.TryGetValue(parentOptionedRegionSpec,
            out var region) && StringHierarchySpec.TryParseOptionedRegionName(
                parentOptionedRegionSpec, out _, out string optionStr))
        {
            // Build the body model if successful. Deep copy of subtree and null
            // parent reference to mark the base region as 'root' for the body
            // model.
            var baseModelRegion = region.DeepCopy(retainParentReference: false);
            // Overwrite the options to be only the option string selected.
            baseModelRegion.Options.Clear();
            baseModelRegion.Options.Add(optionStr);
            // Create the model with the specific model name.
            bodyModel = new StringHierarchyBodyModel(baseModelRegion,
                parentOptionedRegionSpec);
            // Return success.
            return true;
        }
        // Else fail: could not find the region to create a body model from.
        else
        {
            // TODO: print and return false or throw error?
            Console.WriteLine($"{_ErrBuild}: Could not find " +
                $"'{parentOptionedRegionSpec}'. Please select from:\n\t" +
                $"{string.Join("\n\t", this.AvailableModelNames)}");
            bodyModel = null;
            return false;
        }
    }
}

