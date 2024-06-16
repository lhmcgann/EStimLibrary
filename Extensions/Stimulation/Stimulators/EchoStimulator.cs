using EStimLibrary.Core.Stimulation.Stimulators;
using EStimLibrary.Core.Stimulation.Data;
using EStimLibrary.Core;
using EStimLibrary.Core.Stimulation.Functions;
using EStimLibrary.Core.Stimulation.Trains;
using EStimLibrary.Extensions.Stimulation.Phases;
using Newtonsoft.Json.Linq;


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

    // Use example limits and defaults for all base stim params.
    public override Dictionary<string, Tuple<IDataLimits, object>>
        StimParamData => BaseStimParams.ExampleParamData;
    // Params that can be dynamically modulated
    public override SortedSet<string> ModulatableStimParams => new()
    {
        BaseStimParams.PA,
        BaseStimParams.PW
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

