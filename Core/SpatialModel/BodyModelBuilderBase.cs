namespace EStimLibrary.Core.SpatialModel;


public abstract class BodyModelBuilderBase : ISelectable, IFactory<IBodyModel>
{
    #region Abstract Elements
    // ISelectable property, passed to specific implementations.
    public abstract string Name { get; }

    public abstract List<string> AvailableModelNames { get; init; }

    public abstract bool TryCreate(string modelName,
        out IBodyModel bodyModel);
    #endregion

    #region IFactory<IBodyModel> Implementation
    // The defined param name string key for the IFactory.ParamLimits dict.
    protected static string s_modelNameParamKey = "fullModelName";

    public string HelpMsg => $"This is a {this.Name} which you can use to " +
        $"factory-create any one of the pre-loaded body models. All you " +
        $"need to provide is a value for the name of the desired body " +
        $"model, within the following limitations: " +
        $"{this.ParamLimits[s_modelNameParamKey].Description}";

    public Dictionary<string, IDataLimits> ParamLimits => new()
    {
        {s_modelNameParamKey, new FixedOptionDataLimits<string>(
            new(this.AvailableModelNames)) }
    };

    public bool TryCreate(Dictionary<string, object> paramValues,
        out IBodyModel bodyModel, bool skipValueValidation = false)
    {
        // Try to get the model name value.
        if (!paramValues.TryGetValue(s_modelNameParamKey,
            out object modelName) ||
            modelName.GetType() != typeof(string))
        {
            throw new ArgumentException($": Must supply a " +
                $"'{s_modelNameParamKey}' param of type " +
                $"'{this.ParamLimits[s_modelNameParamKey]}'");
        }

        string fullModelName = (string)modelName;

        // Implementation-specific body model build.
        return this.TryCreate(fullModelName, out bodyModel);
    }
    #endregion
}

