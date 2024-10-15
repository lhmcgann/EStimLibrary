using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Extensions.HardwareInterfaces;


public class ContactGroup : NeuralInterfaceHardware
{
    public override string Name => "Contact Group";

    public override int NumContacts => this._NumContacts;
    protected int _NumContacts;

    public ContactGroup(int numContacts) : base()
    {
        if (numContacts > 0) this._NumContacts = numContacts;
        else throw new ArgumentException("Number of contacts must be positive");
    }
}

