using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Extensions.HardwareInterfaces;


/// <summary>
/// The Adjustable Contact Electrode (ACE).
/// </summary>
public class ACE : NeuralInterfaceHardware
{
    #region NeuralInterfaceHardware Implementation
    public override string Name => "ACE";

    public override int NumContacts => this._NumContacts;
    #endregion
    protected int _NumContacts;

    /// <summary>
    /// Create a new representation of a real physical ACE interface.
    /// </summary>
    /// <param name="numContacts">The number of contacts on the ACE to
    /// represent.</param>
    public ACE(int numContacts) : base()
    {
        this._NumContacts = numContacts;
    }
}

