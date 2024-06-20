using EStimLibrary.Extensions.Stimulation.Phases;
using EStimLibrary.Core;


namespace EStimLibrary.Core.Stimulation.Data;

// TODO: DELETE once implemented w/ individual enums
// TODO: OR make these Flags so can use bit arithmetic and flagging principles,
// along with the SortedSet<enum> list of params available or used, to mark
// simultaneously when and which parameters are changed

// TODO: describe each param more as a flag for something the stimulator can do,
// rather than as a data value, even though the two may correspond

//public enum BaseStimParam
//{
//    // Phase Params
//    PA,         // phase amplitude-- the amplitude in mA of the cathodic pulse phase
//    PW,         // phase width-- the width in us of the cathodic pulse phase
//    PhaseShape, // phase shape-- refers to the shape of each phase of the pulse (default is rectangular)

//    // Pulse Params
//    IPD,        // interphase delay-- the time in us between two phases of a pulse
//    AnodeRatio, // anode ratio-- the multiplier of the anodic phase's PW/divisor of the PA for charge balance. ex: AR = 2 -> anodic phase is twice the PW and half the PA of the cathodic phase
//    AnodeFirst, // anode first-- binary that indicates that the anodic phase of the biphasic pulse should go first (default 0 is cathode-first)

//    // Pattern Params
//    Period,     // pattern period-- the period in ms of a pattern; all pulses (and the delays between them) in the pattern must fit within this time window; classically 1/PF given a pattern with only 1 pulse

//    // Train Params
//    FixedRepeats   // number of repeats-- the number of pattern/period repeats this train should run for
//}

/// <summary>
/// A static class of *Extension Methods* to check which sub-group a stim param
/// falls under.
/// Ref.: https://stackoverflow.com/questions/9299279/how-to-group-enum-values
/// </summary>
public static class BaseStimParams
{
    public const string PA = "PA";
    public const string PW = "PW";
    public const string PhaseShape = "PhaseShape";
    public const string IPD = "IPD";
    public const string AnodeRatio = "AnodeRatio";
    public const string AnodeFirst = "AnodeFirst";
    public const string Period = "Period";
    public const string FixedRepeats = "FixedRepeats";

    public static Dictionary<string, int> ParamOrderIndices = new()
    {
        {PA, 0 },
        {PW, 1 },
        {PhaseShape, 2 },
        {IPD, 3 },
        {AnodeRatio, 4 },
        {AnodeFirst, 5 },
        {Period, 6 },
        {FixedRepeats, 7 }
    };

    // TODO: put in the actual limits for each param; rn just semi-dummy values
    public static Dictionary<string, Tuple<IDataLimits, object>>
        ExampleParamData => new()
    {
        // Phase amplitude in mA
        { PA, new(
            new ContinuousDataLimits(0.0, 100),
            12.0)},
        // Phase width in us
        { PW, new(
            new ContinuousDataLimits(1.0, 250.0),
            4.0)},
        // Phase shape
        { PhaseShape, new(
            new FixedOptionDataLimits<Type>(new()
            {
                typeof(SquarePhaseData)
            }),
            typeof(SquarePhaseData))},
        // Inter-phase delay in us
        { IPD, new(
            new ContinuousDataLimits(1.0, 10.0),
            1.0)},
        { AnodeRatio, new(
            new ContinuousDataLimits(0.0, 10.0),
            8.0)},
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
            1/60.0)},
        { FixedRepeats, new(
            new ContinuousIntDataLimits(0, Constants.POS_INFINITY),
            10)}
    };

    //public const int FirstPhaseParamIdx = (int)BaseStimParam.PA;
    //public const int FirstPulseParamIdx = (int)BaseStimParam.IPD;
    //public const int FirstPatternParamIdx = (int)BaseStimParam.Period;
    //public const int FirstTrainParamIdx = (int)BaseStimParam.FixedRepeats;

    public static bool IsPhaseParam(string param)
    {
        switch (param)
        {
            case BaseStimParams.PA:
            case BaseStimParams.PW:
            case BaseStimParams.PhaseShape:
                return true;

            default:
                return false;
        }
    }

    public static bool IsPulseParam(string param)
    {
        switch (param)
        {
            case BaseStimParams.IPD:
            case BaseStimParams.AnodeRatio:
            case BaseStimParams.AnodeFirst:
                return true;

            default:
                return false;
        }
    }

    public static bool IsPatternParam(string param)
    {
        switch (param)
        {
            case BaseStimParams.Period:
                return true;

            default:
                return false;
        }
    }

    public static bool IsTrainParam(string param)
    {
        switch (param)
        {
            case BaseStimParams.FixedRepeats:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Sort parameter string keys by paired integer value.
    /// </summary>
    /// <param name="paramIndices">The parameter key: order index pairs.</param>
    /// <returns>An ordered list of parameter keys.</returns>
    public static List<string> SortParams(Dictionary<string, int> paramIndices)
    {
        return paramIndices.OrderBy(pair => pair.Value)
            .Select(pair => pair.Key).ToList();
    }
}
