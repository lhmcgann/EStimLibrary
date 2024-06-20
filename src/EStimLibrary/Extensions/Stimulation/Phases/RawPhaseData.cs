using EStimLibrary.Core.Stimulation.Data;
using EStimLibrary.Core.Stimulation.Phases;


namespace EStimLibrary.Extensions.Stimulation.Phases;


public record RawPhaseData(int Polarity, List<StimPoint> StimPoints) :
    IPhaseData
{
    public string Name => "RawPoints";

    public double PhaseWidth => throw new NotImplementedException();

    public double CalculateCharge()
    {
        throw new NotImplementedException();
    }

    public IPhaseData CreateOppositePolarityPhaseData()
    {
        throw new NotImplementedException();
    }

    public IPhaseData CreateResizedPhase(double amplitudeScale,
        double chargeError = 0)
    {
        throw new NotImplementedException();
    }
}

