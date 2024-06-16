namespace EStimLibrary.Core.Stimulation.Data;

/// <summary>
/// A simple struct to represent a single point in any stimulation waveform,
/// i.e., a (PW, PA) = (x, y) coordinate.
/// Used mainly in Phase.
/// </summary>
public struct StimPoint
{
    public double PW { get; }   // x-coord
    public double PA { get; }   // y-coord

    public StimPoint(double PW, double PA)
    {
        this.PW = PW;
        this.PA = PA;
    }
}

