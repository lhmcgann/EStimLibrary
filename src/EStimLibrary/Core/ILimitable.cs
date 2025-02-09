using EStimLibrary.Core.Data;


namespace EStimLibrary;


public interface ILimitable
{
    public IDataLimits Limits { get; }
}

