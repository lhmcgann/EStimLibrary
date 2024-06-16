using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Extensions.HardwareInterfaces;


public class GelPad : NeuralInterfaceHardware
{
    public override string Name => "Gel Pad";

    public override int NumContacts => 1;

    public GelPad() : base()
    {
    }
}

