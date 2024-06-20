namespace EStimLibrary.Core;


public static class Constants
{
    public const int POS_INFINITY = int.MaxValue;
    public const int NEG_INFINITY = int.MinValue;

    public const double DEFAULT_CHARGE_ERROR = 0.0;

    public const int CATHODIC_POLARITY = -1;
    public const int ANODIC_POLARITY = 1;
    public const double DEFAULT_ANODE_RATIO = 0.8;

    public enum CurrentDirection
    {
        SOURCE = ANODIC_POLARITY,
        SINK = CATHODIC_POLARITY
    }

    public enum OutputAssignment
    {
        ANODE = ANODIC_POLARITY,    // Use an output as an anode.
        CATHODE = CATHODIC_POLARITY,// Use an output as a cathode.
        SOURCE = ANODE,     // Use an output as a current source (same as anode).
        UNUSED = 0,         // Do not use an output.
        SINK = CATHODE,     // Use an output as a current sink (same as cathode).
    }

    public const int ANODE_FIRST = 0;
    public const int ANODE_SECOND = 1;
    public static int GetAnodeIdx(bool anodeFirst)
    {
        return (anodeFirst) ? ANODE_FIRST : ANODE_SECOND;
    }

    //public static SortedSet<Enum> BaseStimParamsAvailable = new((Enum[])Enum.GetValues(typeof(BaseStimParam)));
}

