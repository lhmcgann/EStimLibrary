using EStimLibrary.Core;
using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Core.Stimulation;
using EStimLibrary.Core.Stimulation.Data;


namespace EStimLibrary.UnitTests.Core.Stimulators;


/// <summary>
/// A class to test StimThread.cs
/// </summary>
public class StimThreadTests
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="perStimConfigData"> </param>
    /// <param name="updateCallback"> The function we are adding to the StimThread.
    /// This will be a dummy function to test that it gets initialized</param>
    [Theory]
    [MemberData(nameof(ConstructorTestData))]
    public void Constructor_ShouldInitializeStimThreadCorrectly(
        Dictionary<int, ThreadConfigDataPerStimulator> perStimConfigData)
    {
        var originalDataShallow = 
            perStimConfigData;
        var originalDataDeep = 
            DeepCopyHelper.DeepCopyPerStimConfigData(perStimConfigData);
        StimThread stimThread = new StimThread(perStimConfigData, thread => { });
        Assert.Equal(originalDataShallow, stimThread.PerStimulatorConfigs);
        Assert.Empty(stimThread.PulseTrainsPerStimulator);
        Assert.Empty(stimThread.TrainParamsPerStimulator);
        Assert.True(CompareConfigDataPerStimulator(originalDataDeep, 
            stimThread.PerStimulatorConfigs));
    }

    public static IEnumerable<object[]> ConstructorTestData()
    {
        return new List<object[]>
        {
            new object[]
            {
                new Dictionary<int, ThreadConfigDataPerStimulator>
                {
                    { 0, new ThreadConfigDataPerStimulator(
                            0, 
                            new List<Lead>
                            {
                                new Lead(
                                    new SortedSet<int> { 1 },
                                    new SortedSet<int> { 1 },
                                    Constants.CurrentDirection.SINK
                                ),
                                new Lead(
                                    new SortedSet<int> { 2 },
                                    new SortedSet<int> { 2 },
                                    Constants.CurrentDirection.SOURCE
                                )
                            }, 
                            BaseStimParams.ExampleParamData, 
                        new SortedSet<string>(BaseStimParams.ParamOrderIndices.Keys)
                        {
                            BaseStimParams.PA,         
                            BaseStimParams.PW,         
                            BaseStimParams.IPD,        
                            BaseStimParams.AnodeRatio
                        })
                    }
                },
            }
        }
        ;
    }
    
    
    

    private static bool CompareConfigDataPerStimulator(Dictionary<int, ThreadConfigDataPerStimulator> x, 
        Dictionary<int, ThreadConfigDataPerStimulator> y)
    {
        foreach (var kvp in x)
        {
            if (!y.ContainsKey(kvp.Key)) return false;

            var threadConfigDataX = kvp.Value;
            var threadConfigDataY = y[kvp.Key];

            if (!ThreadConfigDataEquals(threadConfigDataX, threadConfigDataY)) return false;
        }
        return true;
    }
    
    private static bool ThreadConfigDataEquals(ThreadConfigDataPerStimulator x, ThreadConfigDataPerStimulator y)
    {
        return x.GlobalStimId == y.GlobalStimId
               && LeadListEqual(x.IndependentLeads, y.IndependentLeads)
               && StimParamDataEquals(x.StimParamData, y.StimParamData)
               && x.ModulatableStimParams.SetEquals(y.ModulatableStimParams);
    }
    
    private static bool LeadListEqual(IEnumerable<Lead> x, IEnumerable<Lead> y)
    {
        var xList = x.ToList();
        var yList = y.ToList();

        if (xList.Count != yList.Count) return false;

        for (int i = 0; i < xList.Count; i++)
        {
            if (!LeadEquals(xList[i], yList[i])) return false;
        }

        return true;
    }

    private static bool LeadEquals(Lead x, Lead y)
    {
        return x.ContactSet.SetEquals(y.ContactSet)
               && x.OutputSet.SetEquals(y.OutputSet)
               && x.CurrentDirection == y.CurrentDirection;
    }
    
    private static bool StimParamDataEquals(Dictionary<string, Tuple<IDataLimits, object>> x, Dictionary<string, Tuple<IDataLimits, object>> y)
    {
        if (x.Count != y.Count) return false;

        foreach (var kvp in x)
        {
            if (!y.ContainsKey(kvp.Key)) return false;

            var tupleX = kvp.Value;
            var tupleY = y[kvp.Key];

            if (!tupleX.Item1.Equals(tupleY.Item1) || !object.Equals(tupleX.Item2, tupleY.Item2)) return false;
        }

        return true;
    }
    
    private static class DeepCopyHelper
    {
        public static Dictionary<int, ThreadConfigDataPerStimulator> DeepCopyPerStimConfigData(
            Dictionary<int, ThreadConfigDataPerStimulator> original)
        {
            var copy = new Dictionary<int, ThreadConfigDataPerStimulator>();

            foreach (var kvp in original)
            {
                var copiedThreadConfigData = new ThreadConfigDataPerStimulator(
                    kvp.Value.GlobalStimId,
                    DeepCopyLeads(kvp.Value.IndependentLeads), 
                    DeepCopyStimParamData(kvp.Value.StimParamData),
                    new SortedSet<string>(kvp.Value.ModulatableStimParams)
                );

                copy[kvp.Key] = copiedThreadConfigData;
            }

            return copy;
        }

        private static List<Lead> DeepCopyLeads(IEnumerable<Lead> originalLeads)
        {
            return originalLeads.Select(lead => new Lead(
                new SortedSet<int>(lead.ContactSet),
                new SortedSet<int>(lead.OutputSet),
                lead.CurrentDirection
            )).ToList();
        }
        
        private static Dictionary<string, Tuple<IDataLimits, object>> DeepCopyStimParamData(Dictionary<string, Tuple<IDataLimits, object>> original)
        {
            var copy = new Dictionary<string, Tuple<IDataLimits, object>>();

            foreach (var kvp in original)
            {
                var copiedTuple = new Tuple<IDataLimits, object>(
                    DeepCopyDataLimits(kvp.Value.Item1), 
                    kvp.Value.Item2
                );

                copy[kvp.Key] = copiedTuple;
            }

            return copy;
        }
        
        private static IDataLimits DeepCopyDataLimits(IDataLimits original)
        {
            switch (original)
            { 
                case ContinuousDataLimits continuousLimits:
                    return new ContinuousDataLimits(continuousLimits.MinBound, continuousLimits.MaxBound);
                case FixedOptionDataLimits<Type> fixedOptionLimits:
                    return new FixedOptionDataLimits<Type>(fixedOptionLimits.DataOptions);
                case FixedOptionDataLimits<int> fixedOptionLimitsInt:
                    return new FixedOptionDataLimits<int>(fixedOptionLimitsInt.DataOptions);
                case ContinuousIntDataLimits continuousIntLimits:
                    return new ContinuousIntDataLimits(continuousIntLimits.MinBound, continuousIntLimits.MaxBound);
                default:
                    throw new InvalidOperationException("Unsupported IDataLimits type.");
            }
        }
    }
}