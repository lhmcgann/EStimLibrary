using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Extensions.HardwareInterfaces;


public class PrintedRing : NeuralInterfaceHardware
{
    public override string Name => "3D Printed Ring";

    public override int NumContacts => 2;

    public PrintedRing() : base()
    {
    }
}

