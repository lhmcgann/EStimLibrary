using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Extensions.HardwareInterfaces;


public class XPrizeGlove : NeuralInterfaceHardware
{
    public override string Name => "XPrize Glove";

    public override int NumContacts => 5;

    public XPrizeGlove() : base()
    {
    }
}

