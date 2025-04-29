using MathNet.Numerics.LinearAlgebra;
using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.Core.Haptics;


/// <summary>
/// A record for a haptic event that occurs during live interaction with
/// environment objects.
/// </summary>
/// <param name="Timestamp">The datetime at which the event occurred.</param>
/// <param name="BodyModelKey">The string selected during config that keys the
/// body model (also selected at config) on which this event occurred.</param>
/// <param name="Locations">The dictionary of locations per body model at which
/// this event occurred. Key is the string selected during config that keys the
/// body model on which the listed locations are found. Locations must be of a
/// type conforming to the body model. Parameter may be null or contain
/// otherwise invalid data if LocalizeByArea is true.
/// </param>
/// <param name="Areas">The dictionary of areas per body model at which this
/// event occurred. Key is the string selected during config that keys the body
/// model on which the listed areas are found. Areas must be of a type 
/// conforming to the body model. Parameter may be null or contain otherwise
/// invalid data if LocalizeByArea is false.
/// </param>
/// <param name="HapticParamData">The dictionary of parameters for the haptic
/// event. Key is the haptic param label (see HapticParamEnum).</param>
/// <param name="LocalizeByArea">True if this event should be localized on the
/// body model using this event's Area, False if this event should be localized
/// on the body model using this event's Location.</param>
public record HapticEvent(DateTime Timestamp,
    Dictionary<string, IEnumerable<ILocation>> Locations,
    Dictionary<string, IEnumerable<IArea>> Areas,
    Dictionary<HapticParam, double> HapticParamData,
    bool LocalizeByArea = true);