using EStimLibrary.Core;
using EStimLibrary.Core.Stimulation;
using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.UnitTests.Core.Stimulation;


/// <summary>
/// Unit tests for the ThreadConfigDataPerStimulator record.
/// </summary>
public class ThreadConfigDataPerStimulatorTest
{
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Initializes the test class with an output helper.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper.</param>
    public ThreadConfigDataPerStimulatorTest(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    #region Test Data Providers

    /// <summary>
    /// Provides test data for constructor tests.
    /// Each object[] contains:
    /// - globalStimId (int)
    /// - independentLeads (List<Lead>)
    /// - stimParamData (Dictionary<string, Tuple<IDataLimits, object>>)
    /// - modulatableStimParams (SortedSet<string>)
    /// </summary>
    public static IEnumerable<object[]> ConstructorTestData =>
        new List<object[]>
    {
        new object[]
        {
            100,
            new List<Lead>
            {
                new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 10 },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 3 },
                    new SortedSet<int> { 20 },
                    Constants.CurrentDirection.SINK)
            },
            new Dictionary<string, Tuple<IDataLimits, object>>
            {
                { "ParamA", Tuple.Create<IDataLimits,
                    object>(new StringDataLimits(), "InitialValueA") },
                { "ParamB", Tuple.Create<IDataLimits,
                    object>(new ContinuousDataLimits(0.0, 100.0), 50.0) }
            },
            new SortedSet<string> { "ParamA", "ParamB" }
        },
        new object[]
        {
            200,
            new List<Lead>
            {
                new Lead(new SortedSet<int> { 4 },
                    new SortedSet<int> { 40 },
                    Constants.CurrentDirection.SOURCE)
            },
            new Dictionary<string, Tuple<IDataLimits, object>>(),
            new SortedSet<string>() // Passing empty instead of null
        },
        new object[]
        {
            -500,
            new List<Lead>
            {
                new Lead(new SortedSet<int> { -3 },
                    new SortedSet<int> { -30 },
                    Constants.CurrentDirection.SINK)
            },
            new Dictionary<string, Tuple<IDataLimits, object>>(),
            new SortedSet<string>()
        }
    };

    #endregion

    #region Constructor Tests

    /// <summary>
    /// Test the constructor stores shallow copies of the passed-in data
    /// structures.
    /// </summary>
    [Theory]
    [MemberData(nameof(ConstructorTestData))]
    public void Constructor_ShouldStoreShallowCopies(
        int globalStimId,
        List<Lead> independentLeads,
        Dictionary<string, Tuple<IDataLimits, object>> stimParamData,
        SortedSet<string> modulatableStimParams)
    {
        // Arrange
        var modulatableStimParamsToPass = modulatableStimParams;

        // Act
        var threadConfig = new ThreadConfigDataPerStimulator(
            globalStimId,
            independentLeads,
            stimParamData,
            modulatableStimParamsToPass
        );

        // Assert
        Assert.Same(independentLeads, threadConfig.IndependentLeads);
        Assert.Same(stimParamData, threadConfig.StimParamData);
        Assert.Same(modulatableStimParamsToPass,
            threadConfig.ModulatableStimParams);
    }

    /// <summary>
    /// Test that modifying the original collections affects the record's
    /// properties, confirming shallow copy behavior.
    /// </summary>
    [Theory]
    [MemberData(nameof(ConstructorTestData))]
    public void Constructor_ShallowCopy_ModificationsReflectInRecord(
        int globalStimId,
        List<Lead> independentLeads,
        Dictionary<string, Tuple<IDataLimits, object>> stimParamData,
        SortedSet<string> modulatableStimParams)
    {
        // Arrange
        var modulatableStimParamsToPass = modulatableStimParams;

        var threadConfig = new ThreadConfigDataPerStimulator(
            globalStimId,
            independentLeads,
            stimParamData,
            modulatableStimParamsToPass
        );

        // Act
        // Modify IndependentLeads
        var newLead = new Lead(new SortedSet<int> { 5 },
            new SortedSet<int> { 50 }, Constants.CurrentDirection.SINK);
        independentLeads.Add(newLead);

        // Modify StimParamData
        bool initiallyHasEntries = stimParamData.Any();
        if (initiallyHasEntries)
        {
            var firstKey = stimParamData.Keys.First();
            stimParamData[firstKey] = Tuple.Create<IDataLimits,
                object>(new ContinuousDataLimits(0.0, 1.0), "NewValue");
        }
        stimParamData["ParamY"] = Tuple.Create<IDataLimits,
            object>(new ContinuousIntDataLimits(1, 10), 5);

        // Modify ModulatableStimParams
        modulatableStimParamsToPass.Add("ParamN");

        // Assert
        // IndependentLeads
        Assert.Contains(newLead, threadConfig.IndependentLeads);
        Assert.Equal(independentLeads.Count,
            threadConfig.IndependentLeads.Count());

        // StimParamData
        Assert.Contains("ParamY", threadConfig.StimParamData.Keys);
        Assert.Equal(5, threadConfig.StimParamData["ParamY"].Item2);
        if (initiallyHasEntries)
        {
            var firstKey = stimParamData.Keys.First();
            Assert.Equal("NewValue",
                threadConfig.StimParamData[firstKey].Item2);
        }

        // ModulatableStimParams
        Assert.Contains("ParamN", threadConfig.ModulatableStimParams);
        Assert.Equal(modulatableStimParamsToPass.Count,
            threadConfig.ModulatableStimParams.Count);
    }

    /// <summary>
    /// Tests that nested IDataLimits objects are shallowly copied.
    /// </summary>
    [Theory]
    [MemberData(nameof(ConstructorTestData))]
    public void Constructor_NestedIDataLimits_ShouldBeShallowCopied(
        int globalStimId,
        List<Lead> independentLeads,
        Dictionary<string, Tuple<IDataLimits, object>> stimParamData,
        SortedSet<string> modulatableStimParams)
    {
        // Arrange
        var modulatableStimParamsToPass = modulatableStimParams;

        var threadConfig = new ThreadConfigDataPerStimulator(
            globalStimId,
            independentLeads,
            stimParamData,
            modulatableStimParamsToPass
        );

        // Act
        if (stimParamData.Any())
        {
            var firstKey = stimParamData.Keys.First();
            stimParamData[firstKey] = Tuple.Create<IDataLimits,
                object>(new ContinuousDataLimits(0.0, 1.0), "NewValue");
        }

        // Assert
        if (stimParamData.Any())
        {
            var firstKey = stimParamData.Keys.First();
            Assert.IsType<ContinuousDataLimits>(
                threadConfig.StimParamData[firstKey].Item1);
            Assert.Equal("NewValue",
                threadConfig.StimParamData[firstKey].Item2);
        }
    }

    /// <summary>
    /// Test the constructor correctly initializes all properties with empty 
    /// collections when empty data structures are passed in.
    /// </summary>
    [Fact]
    public void Constructor_WithEmptyCollections_ShouldInitPropertiesCorrectly()
    {
        // Arrange
        var globalStimId = 800;
        var independentLeads = new List<Lead>();
        var stimParamData = new Dictionary<string,
            Tuple<IDataLimits, object>>();
        var modulatableStimParams = new SortedSet<string>();

        // Act
        var threadConfig = new ThreadConfigDataPerStimulator(
            globalStimId,
            independentLeads,
            stimParamData,
            modulatableStimParams
        );

        // Assert
        Assert.Empty(threadConfig.IndependentLeads);
        Assert.Empty(threadConfig.StimParamData);
        Assert.Empty(threadConfig.ModulatableStimParams);
    }

    /// <summary>
    /// Test the constructor correctly handles negative values by storing them
    /// as-is.
    /// </summary>
    [Theory]
    [MemberData(nameof(ConstructorTestData))]
    public void Constructor_WithNegativeValues_ShouldHandleGracefully(
        int globalStimId,
        List<Lead> independentLeads,
        Dictionary<string, Tuple<IDataLimits, object>> stimParamData,
        SortedSet<string> modulatableStimParams)
    {
        // Arrange
        var modulatableStimParamsToPass = modulatableStimParams;

        var threadConfig = new ThreadConfigDataPerStimulator(
            globalStimId,
            independentLeads,
            stimParamData,
            modulatableStimParamsToPass
        );

        // Act
        // No specific action; just verify stored values.

        // Assert
        Assert.Equal(globalStimId, threadConfig.GlobalStimId);
        foreach (var lead in independentLeads)
        {
            Assert.Contains(lead, threadConfig.IndependentLeads);
        }
        foreach (var param in stimParamData)
        {
            Assert.Contains(param.Key, threadConfig.StimParamData.Keys);
            Assert.Equal(param.Value.Item1,
                threadConfig.StimParamData[param.Key].Item1);
            Assert.Equal(param.Value.Item2,
                threadConfig.StimParamData[param.Key].Item2);
        }
        foreach (var param in modulatableStimParamsToPass)
        {
            Assert.Contains(param, threadConfig.ModulatableStimParams);
        }
    }

    #endregion
}

