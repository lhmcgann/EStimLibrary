using EStimLibrary.Core.Stimulation.Pulses;

namespace EStimLibrary.Core.Stimulation.Functions;

/// <summary>
/// The function signature (i.e., delegate) that a pulse check function must
/// follow.
/// </summary>
/// <param name="pulse">The stimulation Pulse to check.</param>
/// <returns>True if the pulse passes the check, False if not.</returns>
public delegate bool PulseCheckDelegate(Pulse pulse);

/// <summary>
/// A class of pulse check functions that all follow the correct delegate
/// signature.
/// </summary>
public static class PulseChecks
{
    // TODO: should these functions just be in specific Stimulator
    // implementations? bc comfort at least is somewhat dependent on stim params
    // used. Or put charge balance in BiphasicPulse? Or can it apply to any
    // pulse w/ 2+ phases, and then it intelligently checks across phases for
    // polarities and net charge?
    // TODO: also just make safety checks of this delegate rather than their own
    // delegate?
    // TODO: what about another delegate for functions that alter the data to
    // pass the check if it doesn't initially and then output the check-passing
    // data in an output param while still returning the bool?

    public static bool ChargeBalanceCheck(Pulse pulse)
    {
        // TODO: apply charge balancing logic given the current output config
        // and stim data values.
        // If changing the stim data values, call the SetStimParamData()
        // function with the new values so any set safeguarding and resource
        // locking is still performed.
        throw new NotImplementedException();
    }

    public static bool ComfortCheck(Pulse pulse)
    {
        // TODO: apply pulse param bounds given min threshold and max
        // comfortable bounds given the current output config.
        // If changing the stim data values, call the SetStimParamData()
        // function with the new values so any set safeguarding and resource
        // locking is still performed.
        throw new NotImplementedException();
    }
}

