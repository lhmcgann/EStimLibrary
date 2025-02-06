using EStimLibrary.Core;
using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.Extensions.SpatialModel.StringHierarchy;


/// <summary>
/// StringHierarchyLocationFactories are objects used to create instances of
/// the StringHierarchyLocation class.
/// </summary>
public class StringHierarchyLocationFactory :
    IFactory<ILocation>
{
    /// <summary>
    /// The base region to create StringHierarchyLocations from.
    /// </summary>
    private readonly StringHierarchyRegion _baseRegion;

    /// <summary>
    /// The help message describing the dynamic data validation performed.
    /// </summary>
    public string HelpMsg { get; init; }

    /// <summary>
    /// The dictionary containing data validation objects for parameters
    /// passed into the factory create method.
    /// </summary>
    public Dictionary<string, IDataLimits> ParamLimits { get; init; }

    /// <summary>
    /// The constructor, which creates an instance of the factory with the
    /// supplied base region and initializes validation.
    /// </summary>
    /// <param name="baseModelRegion">The base region to use.</param>
    public StringHierarchyLocationFactory(
        StringHierarchyRegion baseModelRegion)
    {
        this._baseRegion = baseModelRegion;

        this.HelpMsg = $"A StringHierarchyLocation can be built from one of " +
            $"the following path specs, selecting one option from any list " +
            $"in [] and excluding the []:\n{baseModelRegion.ToString()}";

        this.ParamLimits = new() {
            {"fullSpec",
                new DynamicDataLimits<string>(this.LocationSpecCheckFunction,
                this.HelpMsg) } };

    }

    /// <summary>
    /// The validation function used by the factory create method to ensure
    /// the supplied location spec is valid for the provided base region.
    /// The fullspec must provide a single region spec which is a subregion
    /// of the base region, including at most one set of modifiers.
    /// </summary>
    /// <param name="fullSpec">The location spec to validate.</param>
    /// <returns></returns>
    private bool LocationSpecCheckFunction(string fullSpec)
    {
        var parts = fullSpec.Split(
            StringHierarchySpec.REGIONS_MODIFIERS_DELIMITER);

        return parts.Length > 0 && parts.Length < 3 &&
            // First try to navigate this model to the specified region.
            this._baseRegion.TryGetSubregion(parts[0], out var subregion) &&
            // Then - if any given - check if the modifiers valid in the model.
            ((parts.Length > 1) ?
            subregion.IsValidModifierSpec(parts[1]) : true);
    }

    /// <summary>
    /// The factory create method used to create StringHierarchyLocation
    /// instances. 
    /// </summary>
    /// <param name="paramValues">The parameters to pass into the
    /// StringHierarchyLocation constructor. This must include a valid location
    /// spec with key "fullspec".</param>
    /// <param name="product">An output parameter: the StringHierarchyLocation
    /// if created, else null.</param>
    /// <param name="skipValueValidation">Whether to skip validation with
    /// LocationSpecCheckFunction.</param>
    /// <returns></returns>
    public bool TryCreate(Dictionary<string, object> paramValues,
        out ILocation product, bool skipValueValidation = false)
    {
        bool valid = paramValues.TryGetValue("fullSpec", out object value);
        product = null;
        if (!skipValueValidation)
        {
            valid = this.ParamLimits["fullSpec"].IsValidDataValue(value);
        }
        if (valid)
        {
            product = new StringHierarchyLocation((string)value);
        }
        return valid;
    }
}