using EStimLibrary.Core.Stimulation.Stimulators;
using EStimLibrary.Core;
using EStimLibrary.Core.Stimulation.Functions;
using EStimLibrary.Core.Data;
using EStimLibrary.Extensions.Data;


namespace EStimLibrary.Extensions.Stimulation.Stimulators;


public class EchoStimulator : Stimulator
{
    protected readonly string _Port;
    protected readonly int _NumOutputs;


    public EchoStimulator(string port, int numOutputs)
    {
        this._Port = port;
        this._NumOutputs = numOutputs;
    }

    public override string Name => "Stimulator that just Echoes to Console";

    public override int NumOutputs => this._NumOutputs;
    // Only allow half as many configs as there are outputs.
    public override int MaxNumOutputConfigs => this._NumOutputs / 2;

    #region Convenience StimParam String Name Defines
    protected const string PA = "PA";
    protected const string PW = "PW";
    protected const string StimPhaseShape = "StimPhaseShape";
    protected const string RechargePhaseShape = "RechargePhaseShape";
    protected const string IPD = "IPD";
    protected const string AnodeRatio = "AnodeRatio";
    protected const string AnodeFirst = "AnodeFirst";
    protected const string Period = "Period";
    #endregion

    /// <summary>
    /// Example stimulation parameter specification.
    /// {"paramNameOrKey": (dataLimitsObject, defaultOrFixedValue), ...}
    /// </summary>
    public override Dictionary<string, Tuple<IDataLimits, object>>
        StimParamSpecs => new()
    {
        // Phase amplitude in mA
        { PA, new(
            new ContinuousDataLimits(0.0, 10.0),
            0.0)},
        // Phase width in us
        { PW, new(
            new ContinuousDataLimits(0.0, 250.0),
            0.0)},
        // Activation/stimulation phase shape
        { StimPhaseShape, new(
            new FixedOptionDataLimits<string>(new()
            {
                "square",
                "sine",
                "triangle"
            }),
            "sine")},
        // Recharge phase shape
        { RechargePhaseShape, new(
            new FixedOptionDataLimits<string>(new()
            {
                "square",
                "sine",
                "triangle"
            }),
            "sine")},
        // Inter-phase delay in us
        { IPD, new(
            new ContinuousDataLimits(0.0, 150.0),
            100.0)},
        { AnodeRatio, new(
            new ContinuousDataLimits(0.0, 12.0),
            12.0)},
        { AnodeFirst, new(
            new FixedOptionDataLimits<int>(new()
            {
                Constants.ANODE_FIRST,
                Constants.ANODE_SECOND
            }),
            Constants.ANODE_SECOND)},
        // Pulse period in s (1/Hz)
        { Period, new(
            new ContinuousDataLimits(0.0, 1/250.0),
            1/100.0)}
    };

    /// <summary>
    /// Example set of specified parameters that can be dynamically modulated.
    /// </summary>
    public override SortedSet<string> ModulatableStimParams => new()
    {
        PA,
        PW
    };

    #region TODO
    protected override ValidateOutputConfigDelegate
        OutputConfigCheckFunction => throw new NotImplementedException();

    protected override ValidateStimParamDataDelegate
        StimParamDataCheckFunction => throw new NotImplementedException();

    public override bool SetDefaultOutputConfigs(out Dictionary<int,
        Constants.OutputAssignment[]> defaultConfig)
    {
        throw new NotImplementedException();
    }
    #endregion

    public override bool IsValidOutputWiring(IEnumerable<int> localOutputIds)
    {
        return true;
    }

    protected override void HW_SendMessage(byte[] data)
    {
        Console.WriteLine($"EchoStimulator.HW_SendMessage: byte[] = {data}");
    }

    protected override void HW_StartStim()
    {
        Console.WriteLine("EchoStimulator.HW_StartStim");
    }

    protected override void HW_StopStim()
    {
        Console.WriteLine("EchoStimulator.HW_StopStim");
    }

    // protected override bool HW_UpdateStim(IEnumerable<Train> stimTrains,
    //  Dictionary<int, int> globalToLocalOutputIds)
    protected override bool HW_UpdateStim(
        IEnumerable<Dictionary<string, object>> trainsParams,
        Dictionary<int, Constants.OutputAssignment> localOutputAssignments)
    {
        var trainsParamsStrs = trainsParams.Select(
            d => string.Join("\n\t\t", d));
        Console.WriteLine($"\n\nEchoStimulator.HW_UpdateStim\n\t" +
            $"Outputs: {string.Join("\n\t\t", localOutputAssignments)}\n\n\t" +
            $"Params: {string.Join("\n\tTrain:", trainsParamsStrs)}");
        return true;
    }
}

