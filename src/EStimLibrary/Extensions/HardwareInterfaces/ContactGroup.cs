using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Extensions.HardwareInterfaces;


/// <summary>
/// A generic neural interface containing 1+ contacts.
/// </summary>
public class ContactGroup : NeuralInterfaceHardware
{
    /// <summary>
    /// The name of this neural interface type.
    /// </summary>
    public override string Name => "Contact Group";

    /// <summary>
    /// The integer number of contacts in this neural interface.
    /// </summary>
    public override int NumContacts => this._NumContacts;
    protected int _NumContacts;

    /// <summary>
    /// Create a contact group with a positive integer number of contacts.
    /// </summary>
    /// <param name="numContacts">Number of contacts.</param>
    /// <exception cref="ArgumentException">Invalid argument, numContact must be
    /// positive.</exception>
    public ContactGroup(int numContacts) : base()
    {
        if (numContacts > 0)
        {
            this._NumContacts = numContacts;
        }
        else
        {
            throw new ArgumentException("Number of contacts must be positive");
        }
    }
}

