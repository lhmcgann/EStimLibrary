using EStimLibrary.Core;
using EStimLibrary.Core.HardwareInterfaces;

namespace EStimLibrary.UnitTests.Core.HardwareInterfaces;

public class LeadManagerTests
{
    /// <summary>
    /// Test the empty constructor. There is nothing to test since all fields that get
    /// initialized are protected or internal with no public get methods so they cannot be checked
    /// </summary>
    [Fact]
    public void EmptyConstuctor_ShouldInitEmpty()
    {
        var leadManager = new LeadManager();
    }

    /// <summary>
    /// Test the parameterized constructor with different data values.
    /// </summary>
    [Theory]
    [MemberData(nameof(TryAddLeadData))]
    public void TryAddLead_ShouldReturnCorrectID(LeadManager leadManager, Lead lead, int expectedId)
    {
        var results = leadManager.TryAddLead(lead, out var id);
        Assert.True(results);
        Assert.Equal(expectedId, id);
        foreach (var contact in lead.ContactSet)
        {
            Assert.True(leadManager.IsWiredContact(contact));
        }

        foreach (var output in lead.OutputSet)
        {
            Assert.True(leadManager.IsWiredOutput(output));
        }
    }

    public static IEnumerable<object[]> TryAddLeadData()
    {
        // Starting with an empty LeadManager
        var leadManager1 = new LeadManager();
        
        // Starting with populated LeadManager with a missing id 0
        var leadManager2 = new LeadManager();
        leadManager2.TryAddLead(new Lead(new SortedSet<int> { 1, 2, 3 },
            new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK), out _);
        leadManager2.TryAddLead(new Lead(new SortedSet<int> { 3, 4, 5 },
            new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SINK), out _);
        leadManager2.TryAddLead(new Lead(new SortedSet<int> { 97 },
            new SortedSet<int> { 0, 43, 76 }, Constants.CurrentDirection.SINK), out _);
        leadManager2.TryRemoveLead(0, out _);
        
        // Starting with a populated LeadManager with a missing id 1
        var leadManager3 = new LeadManager();
        leadManager3.TryAddLead(new Lead(new SortedSet<int> { 1, 2, 3 },
            new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK), out _);
        leadManager3.TryAddLead(new Lead(new SortedSet<int> { 3, 4, 5 },
            new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SINK), out _);
        leadManager3.TryAddLead(new Lead(new SortedSet<int> { 97 },
            new SortedSet<int> { 0, 43, 76 }, Constants.CurrentDirection.SINK), out _);
        leadManager3.TryRemoveLead(1, out _); 
        
        // Starting with a populated LeadManager with a missing id 2
        var leadManager4 = new LeadManager();
        leadManager4.TryAddLead(new Lead(new SortedSet<int> { 1, 2, 3 },
            new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK), out _);
        leadManager4.TryAddLead(new Lead(new SortedSet<int> { 3, 4, 5 },
            new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SINK), out _);
        leadManager4.TryAddLead(new Lead(new SortedSet<int> { 97 },
            new SortedSet<int> { 0, 43, 76 }, Constants.CurrentDirection.SINK), out _);
        leadManager4.TryRemoveLead(2, out _);
        return new List<object[]>
        {
            new object[]
            {
                leadManager1,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK),
                0
            },
            new object[]
            {
                leadManager1,
                new Lead(new SortedSet<int> { 2, 3, 4, 8 },
                    new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK),
                1
            },
            new object[]
            {
            leadManager1,
            new Lead(new SortedSet<int> { 2, 3, 4, 8 },
                new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK),
                2
            },
            new object[]
            {
                leadManager2,
                new Lead(new SortedSet<int> {2, 3, 4},
                    new SortedSet<int> {3, 2}, Constants.CurrentDirection.SINK),
                0
            },
            new object[]
            {
                leadManager3,
                new Lead(new SortedSet<int> {2, 3, 4},
                    new SortedSet<int> {3, 2}, Constants.CurrentDirection.SINK),
                1
            },
            new object[]
            {
                leadManager4,
                new Lead(new SortedSet<int> {2, 3, 4},
                    new SortedSet<int> {3, 2}, Constants.CurrentDirection.SINK),
                2
            }
        };
    }
    
}