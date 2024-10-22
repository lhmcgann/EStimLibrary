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
    /// 
    /// </summary>
    [Theory]
    [MemberData(nameof(TryAddLeadEmptyData))]
    public void TryAddLead_ShouldReturnCorrectIdWhenLeadMangerIsEmpty(LeadManager leadManager, Lead lead,
        int expectedId)
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

    public static IEnumerable<object[]> TryAddLeadEmptyData()
    {
        // Starting with an empty LeadManager
        var leadManager = new LeadManager();

        return new List<object[]>
        {
            new object[]
            {
                leadManager,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK),
                0
            },
            new object[]
            {
                leadManager,
                new Lead(new SortedSet<int> { 2, 3, 4, 8 },
                    new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK),
                1
            },
            new object[]
            {
                leadManager,
                new Lead(new SortedSet<int> { 2, 3, 4, 8 },
                    new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK),
                2
            }
        };
    }

    [Theory]
    [MemberData(nameof(TryAddLeadFirstIdMissingData))]
    public void TryAddLead_Returns_Correct_Id_When_Some_Id_Frees_Up(LeadManager leadManager, Lead lead, int expectedId)
    {
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 1, 2, 3 },
            new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK), out _);
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 3, 4, 5 },
            new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SINK), out _);
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 97 },
            new SortedSet<int> { 0, 43, 76 }, Constants.CurrentDirection.SINK), out _);
        leadManager.TryRemoveLead(expectedId, out _);

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

    public static IEnumerable<object[]> TryAddLeadFirstIdMissingData()
    {
        var leadManager = new LeadManager();

        return new List<object[]>
        {
            new object[]
            {
                leadManager,
                new Lead(new SortedSet<int> { 2, 3, 4 },
                    new SortedSet<int> { 3, 2 }, Constants.CurrentDirection.SINK),
                0
            },
            new object[]
            {
                leadManager,
                new Lead(new SortedSet<int> { 3, 4, 5 },
                    new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SINK),
                1
            },
            new object[]
            {
                leadManager,
                new Lead(new SortedSet<int> { 97 },
                    new SortedSet<int> { 0, 43, 76 }, Constants.CurrentDirection.SINK),
                2
            },
        };
    }

}