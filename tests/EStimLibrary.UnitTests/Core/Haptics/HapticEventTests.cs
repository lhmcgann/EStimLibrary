using EStimLibrary.Core.Haptics;
using EStimLibrary.Core.SpatialModel;


namespace EStimLibrary.UnitTests.Core.Haptics;


public class HapticEventTests
{
    private readonly ITestOutputHelper _output;

    // Test class constructor creates an output helper so can write console
    // output.
    public HapticEventTests(ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    // Test method naming convention: LibClassMethodName_ScenarioShouldExpectn

    /// <summary>
    /// Test record initialization.
    /// </summary>
    [Fact]
    public void Constuctor_ShouldInit()
    {
        var timestamp = DateTime.Now;
        var locations = new Dictionary<string, IEnumerable<ILocation>>();
        var areas = new Dictionary<string, IEnumerable<IArea>>();
        var hapticParams = new Dictionary<HapticParam, double>();
        hapticParams.Add(HapticParam.P, 2.5);
        hapticParams.Add(HapticParam.dP, 1.0);
        var hapticEvent = new HapticEvent(timestamp, locations, areas, hapticParams);
        Assert.NotNull(hapticEvent);
        Assert.Equal(timestamp, hapticEvent.Timestamp);
        Assert.Equal(locations, hapticEvent.Locations);
        Assert.Equal(areas, hapticEvent.Areas);
        Assert.Equal(hapticParams, hapticEvent.HapticParamData);
        Assert.True(hapticEvent.LocalizeByArea);
    }

    /// <summary>
    /// Test record copying.
    /// </summary>
    [Fact]
    public void With_ShouldCopy()
    {
        var timestamp = DateTime.Now;
        var locations = new Dictionary<string, IEnumerable<ILocation>>();
        var areas = new Dictionary<string, IEnumerable<IArea>>();
        var hapticParams = new Dictionary<HapticParam, double>();
        hapticParams.Add(HapticParam.P, 2.5);
        hapticParams.Add(HapticParam.dP, 1.0);
        var hapticEvent1 = new HapticEvent(timestamp, locations, areas, hapticParams);
        var hapticEvent2 = hapticEvent1 with { };
        var hapticEvent3 = hapticEvent2 with { Timestamp = DateTime.Now };
        var hapticEvent4 = hapticEvent3 with { Areas = new Dictionary<string, IEnumerable<IArea>>() };

        Assert.Equal(hapticEvent1, hapticEvent2);
        Assert.NotEqual(hapticEvent2, hapticEvent3);
        Assert.NotEqual(hapticEvent3, hapticEvent4);
    }
}