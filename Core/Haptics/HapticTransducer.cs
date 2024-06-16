using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Core.Stimulation;


namespace EStimLibrary.Core.Haptics;


public abstract class HapticTransducer : ISelectable
{
    public abstract string Name { get; }    // ISelectable

    /// <summary>
    /// Transduce a haptic event into stimulation data and push the data changes
    /// to stimulator hardware. A new programmatic thread is created for each
    /// updated StimThread that needs to be pushed.
    /// </summary>
    /// <param name="hapticEvent">The haptic event to transduce.</param>
    /// <param name="fullyContainingAreas">The ID-keyed areas fully containing
    /// the event. IDs are local to the body model on which the event happened.
    /// </param>
    /// <param name="partiallyContainingAreas">The ID-keyed areas partially
    /// containing the event. IDs are local to the body model on which the event
    /// happened.</param>
    /// <param name="localizedThreads">The StimThreads associated with
    /// each ID-keyed area given in fullyContainingAreas and
    /// partiallyContainingAreas. This must contain an entry for each area in
    /// those two params.</param>
    public void TransduceHapticEvent(HapticEvent hapticEvent,
        Dictionary<int, IArea> fullyContainingAreas,
        Dictionary<int, IArea> partiallyContainingAreas,
        Dictionary<int, StimThread> localizedThreads)
    {
        // Actually update the StimThreadData
        var updatedStimThreads = this._TransduceHapticEvent(hapticEvent,
            fullyContainingAreas, partiallyContainingAreas,
            localizedThreads);

        // Push the stim data changes on each updated StimThread to stim HW.
        foreach (var stimThread in updatedStimThreads)
        {
            // Create a new programmatic thread for each StimThread update.
            //ThreadPool.QueueUserWorkItem(_ => stimThread.SendUpdatedStimData());
            stimThread.SendUpdatedStimData();
        }
    }

    /// <summary>
    /// Transduce a haptic event into stimulation data.
    /// Make the desired stim data updates given a haptic event, the relevant
    /// spatial info, and the associated stim thread pool(s).
    /// </summary>
    /// <param name="hapticEvent">The haptic event to transduce.</param>
    /// <param name="fullyContainingAreas">The ID-keyed areas fully containing
    /// the event. IDs are local to the body model on which the event happened.
    /// </param>
    /// <param name="partiallyContainingAreas">The ID-keyed areas partially
    /// containing the event. IDs are local to the body model on which the event
    /// happened.</param>
    /// <param name="localizedThreads">The StimThreads associated with
    /// each ID-keyed area given in fullyContainingAreas and
    /// partiallyContainingAreas. This must contain an entry for each area in
    /// those two params.</param>
    /// <returns>The StimThreads actually used, i.e., in which stim data was
    /// changed.</returns>
    protected abstract IEnumerable<StimThread> _TransduceHapticEvent(
        HapticEvent hapticEvent,
        Dictionary<int, IArea> fullyContainingAreas,
        Dictionary<int, IArea> partiallyContainingAreas,
        Dictionary<int, StimThread> localizedThreads);
}

