using EStimLibrary.Core;
using EStimLibrary.Core.Data;
using EStimLibrary.Extensions.Data;
using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;


/// <summary>
/// A factory to create StringHierarchyLocation objects.
/// </summary>
public class StringHierarchyLocationFactory :
    IFactory<ILocation>
{
    /// <summary>
    /// The base string hierarchy region within which valid 
    /// StringHierarchyLocations can be created.
    /// </summary>
    private readonly StringHierarchyRegion _baseRegion;

    /// <summary>
    /// The help message describing the dynamic data validation performed.
    /// Will contain a description of valid StringHierarchyLocations based on
    /// body models configured at run-time.
    /// </summary>
    public string HelpMsg { get; init; }

    /// <summary>
    /// The dictionary containing data validation objects for parameters
    /// passed into the factory create method. Contains a single parameter
    /// "fullSpec" which must be a valid full string location spec within the
    /// base region.
    /// </summary>
    public Dictionary<string, IDataLimits> ParamLimits { get; init; }

    /// <summary>
    /// Create an instance of the factory using the supplied base region to 
    /// initialize help and data validation properties.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if the base region is
    /// null.</exception>
    public StringHierarchyLocationFactory(
        StringHierarchyRegion baseModelRegion)
    {
        this._baseRegion = baseModelRegion;

        this.HelpMsg = $"A StringHierarchyLocation can be built from one of " +
            $"the following path specs, selecting one option from any list " +
            $"in [] and excluding the []:\n{baseModelRegion.ToString()}";

        this.ParamLimits = new() {
            {"fullSpec",
                new DynamicDataLimits<string>(this._LocationSpecCheckFunction,
                this.HelpMsg) } };
    }

    /// <summary>
    /// The validation function used by the factory create method to ensure
    /// the supplied location spec - region spec and modifiers - is valid 
    /// within the provided base region.
    /// </summary>
    /// <param name="fullSpec">The full location spec to validate. Assumed to
    /// be not null.</param>
    /// <returns>True if valid location spec, false if not.</returns>
    private bool _LocationSpecCheckFunction(string fullSpec)
    {
        // TryGetSubregion and IsValidModifiers inherently validate those spec
        // parts, so split manually rather than call ParseSpec.
        var parts = fullSpec.Split(
            StringHierarchySpec.REGIONS_MODIFIERS_DELIMITER);

        return parts.Length > 0 && parts.Length < 3 &&
            // First try to navigate this model to the specified region.
            this._baseRegion.TryGetSubregion(parts[0], out var subregion) &&
            // Then - if any given - check if the modifiers valid in the model.
            ((parts.Length > 1) ?
                subregion.IsValidModifierSpec(parts[1], out _) : true);
    }

    /// <summary>
    /// Create a StringHierarchyLocation object from the provided parameter
    /// values.
    /// </summary>
    /// <param name="paramValues">The parameters to pass into the
    /// StringHierarchyLocation constructor. This must adhere to the 
    /// ParamLimits of this factory for successful creation, i.e., include a
    /// single element keyed "fullSpec" which is a full string location spec 
    /// valid within this factory's base region.</param>
    /// <param name="product">An output parameter: the StringHierarchyLocation
    /// if created, else null.</param>
    /// <param name="skipValueValidation">An optional parameter: perform only
    /// basic input validation and skip any deeper parameter value validation,
    /// e.g., if the data has already been validated. Default: false.</param>
    /// <returns>True if a StringHierarchyLocation could be created, False if 
    /// not.</returns>
    public bool TryCreate(Dictionary<string, object> paramValues,
        out ILocation? product, bool skipValueValidation = false)
    {
        // Get the params provided for product creation. This factory only
        // requires one parameter value for "fullSpec" which must follow the
        // DynamicDataLimits in this factory's ParamLimits
        bool valid = paramValues.TryGetValue("fullSpec", out object? value);

        // Init the product to null in case of failed creation.
        product = null;

        // Skip param value validation if requested.
        if (!skipValueValidation)
        {
            valid = this.ParamLimits["fullSpec"].IsValidDataValue(value!);
        }

        // Create and return the product if param values valid.
        if (valid)
        {
            product = new StringHierarchyLocation((string)value!);
        }
        return valid;
    }
}