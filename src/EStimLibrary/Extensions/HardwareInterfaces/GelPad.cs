using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Extensions.HardwareInterfaces;


/// <summary>
/// A single gel pad electrode.
/// </summary>
public class GelPad : NeuralInterfaceHardware
{
    /// <summary>
    /// The name of this neural interface type.
    /// </summary>
    public override string Name => "Gel Pad";

    /// <summary>
    /// A gel pad represents a single contact.
    /// </summary>
    public override int NumContacts => 1;

    /// <summary>
    /// Create a gel pad.
    /// </summary>
    public GelPad() : base()
    {
    }
}

