namespace EStimLibrary.Core;


public interface IFactory<ProductType>
{
    public string HelpMsg { get; }
    public Dictionary<string, IDataLimits> ParamLimits { get; }
    /// <summary>
    /// Try creating a product from a given set of param values.
    /// </summary>
    /// <param name="paramValues">The data values for each product parameter,
    /// keyed by parameter name as keyed in ParamLimits. A value must be given
    /// for each parameter. The object value is assumed to be castable as the
    /// correct type with no further data or type manipulation needed.</param>
    /// <param name="product">The output product if creation is successful. null
    /// if product creation was unsuccessful.</param>
    /// <param name="skipValueValidation">An optional parameter: perform only
    /// basic input validation and skip any deeper parameter value validation,
    /// e.g., if the data has already been validated. Default: false.</param>
    /// <returns>True if a product could be created, False if not.</returns>
    public bool TryCreate(Dictionary<string, object> paramValues,
        out ProductType product, bool skipValueValidation = false);
    //// Try creating product from I/O interaction.
    //public bool TryCreate(Action<string> displayOutput, Func<string> readInput);
}

