namespace EStimLibrary.Core.HardwareInterfaces;


public abstract class NeuralInterfaceHardware : ISelectable, IIdentifiable
{
    public abstract string Name { get; }    // ISelectable
    // Manager-given ID of the stimulator.
    public int Id { get; internal set; }    // IIdentifiable

    public abstract int NumContacts { get; }

    protected NeuralInterfaceHardware()
    {
        // Init stim ID as not set yet.
        this.Id = -1;
    }
}

