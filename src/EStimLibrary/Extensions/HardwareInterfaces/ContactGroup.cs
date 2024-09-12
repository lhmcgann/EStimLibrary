using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Extensions.HardwareInterfaces;


public class ContactGroup : NeuralInterfaceHardware
{
    public override string Name => "Contact Group";

    public override int NumContacts => this._NumContacts;
    protected int _NumContacts;

    public ContactGroup(int numContacts) : base()
    {
        this._NumContacts = numContacts;
    }
}

