using MathNet.Numerics.LinearAlgebra;
using EStimLibrary.Core;


namespace EStimLibrary.Core.Stimulation;


// A delegate for handling StimThread output config changed events.
public delegate void StimThreadOutputConfigChangedEventHandler(object sender,
    StimThreadOutputConfigChangedEventArgs e);

public class StimThreadOutputConfigChangedEventArgs : EventArgs
{
    private readonly byte _globalStimThreadId;
    private readonly Constants.OutputAssignment[] _outputConfig;

    // Properties to securely expose the private fields.
    public byte GlobalStimThreadId { get { return this._globalStimThreadId; } }
    public Constants.OutputAssignment[] OutputConfig
    { get { return this._outputConfig; } }

    /// <summary>
    /// Construct the event arguments containing global StimThread ID and output
    /// config data.
    /// </summary>
    /// <param name="globalStimThreadId">The global StimThread ID.</param>
    /// <param name="outputConfig">The output config array. A deep copy is made.
    /// </param>
    public StimThreadOutputConfigChangedEventArgs(byte globalStimThreadId,
        Constants.OutputAssignment[] outputConfig)
    {
        this._globalStimThreadId = globalStimThreadId;
        this._outputConfig = (Constants.OutputAssignment[])outputConfig.Clone();
    }
}

// A delegate for handling StimThread stim param data changed events.
public delegate void StimThreadStimParamDataChangedEventHandler(object sender,
    StimThreadStimParamDataChangedEventArgs e);

public class StimThreadStimParamDataChangedEventArgs : EventArgs
{
    // Properties.
    public byte GlobalStimThreadId { get; init; }
    public SortedSet<string> StimParamsUsed { get; init; }
    public Vector<double> StimParamData { get; init; }

    /// <summary>
    /// Construct the event arguments containing global StimThread ID and stim 
    /// param data.
    /// </summary>
    /// <param name="globalStimThreadId">The global StimThread ID.</param>
    /// <param name="stimParamsUsed">The stim params used. A deep copy is
    /// made.</param>
    /// <param name="stimParamData">The stim param data in the order consistent
    /// with pulse params used. A deep copy is made.
    /// </param>
    public StimThreadStimParamDataChangedEventArgs(byte globalStimThreadId,
        SortedSet<string> stimParamsUsed,
        Vector<double> stimParamData)
    {
        this.GlobalStimThreadId = globalStimThreadId;
        this.StimParamsUsed = new(stimParamsUsed);
        this.StimParamData = stimParamData.Clone();
    }
}

