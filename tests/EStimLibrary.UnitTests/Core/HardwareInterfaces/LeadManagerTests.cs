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
                1
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
    /// <summary>
    /// Get all Leads wired to a specific output.
    /// </summary>
    /// <param name="outputId">The global output ID to query.</param>
    /// <returns>A list of all Leads attached to the given output. An empty
    /// list if the queried output ID is invalid or un-wired.</returns>
    [Theory]
    [MemberData(nameof(GetLeadsWiredToOutputData))]
    public void GetLeadsWiredToOutput_ShouldReturnCorrectLeads_WhenOutputIsWired(LeadManager leadManager, int outputId, List<Lead> expectedLeads)
    {
        var results = leadManager.GetLeadsOfOutput(outputId);
        
        Assert.Equal(expectedLeads.Count, results.Count);
        foreach (var lead in expectedLeads)
        {
            Assert.Contains(lead, results);
        }
    }

    public static IEnumerable<object[]> GetLeadsWiredToOutputData()
    {
        var leadManager = new LeadManager();
        
        // Setting up the LeadManager with some leads
        var lead1 = new Lead(new SortedSet<int> { 1, 2, 3 }, new SortedSet<int> { 4 }, Constants.CurrentDirection.SINK);
        var lead2 = new Lead(new SortedSet<int> { 4, 5 }, new SortedSet<int> { 6 }, Constants.CurrentDirection.SINK);
        var lead3 = new Lead(new SortedSet<int> { 7, 8 }, new SortedSet<int> { 4 }, Constants.CurrentDirection.SINK);
        
        leadManager.TryAddLead(lead1, out _);
        leadManager.TryAddLead(lead2, out _);
        leadManager.TryAddLead(lead3, out _);

        return new List<object[]>
        {
            new object[]
            {
                leadManager,
                4,
                new List<Lead> {lead1, lead3}
            },
            new object[]
            {
                leadManager,
                6,
                new List<Lead> {lead2}
            },
            new object[]
            {
                leadManager,
                10, // Invalid output ID
                new List<Lead>() // Expecting an empty list
            },
            new object[]
            {
                leadManager,
                -1, // Invalid output ID
                new List<Lead>() // Expecting an empty list
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetLeadsWiredToOutputWithInvalidIdData))]
    public void GetLeadsWiredToOutput_ShouldReturnEmptyList_WhenOutputIsInvalid(LeadManager leadManager, int outputId)
    {
        var results = leadManager.GetLeadsOfOutput(outputId);
        
        Assert.Empty(results);
    }

    public static IEnumerable<object[]> GetLeadsWiredToOutputWithInvalidIdData()
    {
        var leadManager = new LeadManager();
        
        // Setting up with no leads
        return new List<object[]>
        {
            new object[] { leadManager, -1 }, // Negative ID
            new object[] { leadManager, 0 }, // Zero ID
            new object[] { leadManager, 100 } // High invalid ID
        };
    }

    /// <summary>
    /// Fully remove a lead from this manager, including all object and ID
    /// references. Lead lookup based on global ID.
    /// </summary>
    /// <param name="leadId">The global ID of the lead to remove.</param>
    /// <param name="removedLead">An output parameter: the Lead removed. Only
    /// contains valid data if the method returns true, otherwise may be null.
    /// </param>
    /// <returns>True if the queried lead ID was valid and the lead could be
    /// removed. False if not.</returns>
    [Theory]
    [MemberData(nameof(RemoveLeadData))]
    public void RemoveLead_ShouldReturnTrueAndRemoveLead_WhenLeadExists(LeadManager leadManager, int leadId, Lead expectedLead)
    {
        var result = leadManager.TryRemoveLead(leadId, out var removedLead);

        Assert.True(result);
        Assert.Equal(expectedLead, removedLead);
        var outputs = leadManager.GetLeadsOfOutput(expectedLead.OutputSet.First()); // Assuming at least one output
        Assert.DoesNotContain(expectedLead, outputs);
    }

    public static IEnumerable<object[]> RemoveLeadData()
    {
        var leadManager = new LeadManager();

        // Setting up the LeadManager with some leads
        var lead1 = new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK);
        var lead2 = new Lead(new SortedSet<int> { 4, 5 }, new SortedSet<int> { 6 }, Constants.CurrentDirection.SINK);
        leadManager.TryAddLead(lead1, out var id1);
        leadManager.TryAddLead(lead2, out var id2);

        return new List<object[]>
        {
            new object[]
            {
                leadManager,
                id1,
                lead1
            },
            new object[]
            {
                leadManager,
                id2,
                lead2
            }
        };
    }

    [Theory]
    [MemberData(nameof(RemoveLeadWithInvalidIdData))]
    public void RemoveLead_ShouldReturnFalse_WhenLeadDoesNotExist(LeadManager leadManager, int leadId)
    {
        var result = leadManager.TryRemoveLead(leadId, out var removedLead);

        Assert.False(result);
        Assert.Null(removedLead);
    }

    public static IEnumerable<object[]> RemoveLeadWithInvalidIdData()
    {
        var leadManager = new LeadManager();

        // Setting up with some leads
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK), out _);
        
        return new List<object[]>
        {
            new object[] { leadManager, -1 }, // Negative ID
            new object[] { leadManager, 1}, // Zero ID
            new object[] { leadManager, 100 } // High invalid ID
        };
    }
    
    /// <summary>
    /// Fully remove a lead from this manager, including all object and ID
    /// references. Lead lookup based on lead value.
    /// </summary>
    /// <param name="leadId">An output parameter: the global ID of the lead
    /// removed. Only contains valid data if a lead value match could be found,
    /// otherwise -1.</param>
    /// <param name="removedLead">An output parameter: the Lead removed. Only
    /// contains valid data if the method returns true, otherwise may be null.
    /// </param>
    /// <returns>True if the queried lead could be found and removed. False if
    /// not.</returns>
    [Theory]
    [MemberData(nameof(RemoveLeadByValueData))]
    public void RemoveLeadByValue_ShouldReturnTrueAndRemoveLead_WhenLeadExists(LeadManager leadManager, Lead leadToRemove, int expectedLeadId)
    {
        var result = leadManager.TryRemoveLead(leadToRemove, out var removedLeadId, out var removedLead);

        Assert.True(result);
        Assert.Equal(expectedLeadId, removedLeadId);
        Assert.Equal(removedLead, leadToRemove);

        // Verify the lead is not returned in any output
        var outputs = leadManager.GetLeadsOfOutput(leadToRemove.OutputSet.First()); // Assuming at least one output
        Assert.DoesNotContain(removedLead, outputs);
    }

    public static IEnumerable<object[]> RemoveLeadByValueData()
    {
        var leadManager = new LeadManager();

        // Setting up the LeadManager with some leads
        var lead1 = new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK);
        var lead2 = new Lead(new SortedSet<int> { 4, 5 }, new SortedSet<int> { 6 }, Constants.CurrentDirection.SINK);
        leadManager.TryAddLead(lead1, out var id1);
        leadManager.TryAddLead(lead2, out var id2);

        return new List<object[]>
        {
            new object[]
            {
                leadManager,
                lead1,
                id1
            },
            new object[]
            {
                leadManager,
                lead2,
                id2
            }
        };
    }

    [Theory]
    [MemberData(nameof(RemoveLeadByValueWithInvalidData))]
    public void RemoveLeadByValue_ShouldReturnFalse_WhenLeadDoesNotExist(LeadManager leadManager, Lead leadToRemove)
    {
        var result = leadManager.TryRemoveLead(leadToRemove, out var removedLeadId, out var removedLead);

        Assert.False(result);
        Assert.Equal(-1, removedLeadId);
        Assert.Null(removedLead);
    }

    public static IEnumerable<object[]> RemoveLeadByValueWithInvalidData()
    {
        var leadManager = new LeadManager();

        // Setting up with some leads
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK), out _);
        
        return new List<object[]>
        {
            new object[] { leadManager, new Lead(new SortedSet<int> { 4, 5 }, new SortedSet<int> { 6 }, Constants.CurrentDirection.SINK) }, // Non-existing lead
            new object[] { leadManager, new Lead(new SortedSet<int> { 10, 11 }, new SortedSet<int> { 12 }, Constants.CurrentDirection.SINK) } // Another non-existing lead
        };
    }


}