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
        // No assertions are needed since we are only ensuring the object initializes correctly
    }
    
    /// <summary>
    /// Test the behavior of the TryAddLead method when adding a lead to an empty LeadManager.
    /// Verifies that the method returns the correct ID and updates the wiring state of the contacts and outputs.
    /// <param name="leadManager">The LeadManager instance to test the TryAddLead method on.</param>
    /// <param name="lead">The Lead object to add to the LeadManager.</param>
    /// <param name="expectedId">The expected ID that should be assigned to the lead after it is added.</param>
    /// </summary>
    [Theory]
    [MemberData(nameof(TryAddLeadEmptyData))]
    public void TryAddLead_ShouldReturnCorrectIdWhenLeadMangerIsEmpty(LeadManager leadManager, Lead lead,
        int expectedId)
    {

        var results = leadManager.TryAddLead(lead, out var id);
        // Assert that the operation is successful and returns the expected ID
        Assert.True(results);
        Assert.Equal(expectedId, id);
        // Assert that all contacts in the lead are correctly wired in the lead manager
        foreach (var contact in lead.ContactSet)
        {
            Assert.True(leadManager.IsWiredContact(contact));
        }
        // Assert that all outputs in the lead are correctly wired in the lead manager
        foreach (var output in lead.OutputSet)
        {
            Assert.True(leadManager.IsWiredOutput(output));
        }
    }

    /// <summary>
    /// Test parameter data for TryAddLead, following the form:
    ///    leadManager (Instance to be used),
    ///    lead (to be added),
    ///    expectedId
    /// Assuming all global IDs and leads are valid in the session.
    /// </summary>
    public static IEnumerable<object[]> TryAddLeadEmptyData()
    {
        // Starting with an empty LeadManager
        var leadManager = new LeadManager();
        var lead = new Lead(new SortedSet<int> { 2, 3, 4, 8 },
            new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK);
        
        return new List<object[]>
        {
            // Testing adding a lead when the manager is empty
            new object[]
            {
                leadManager,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK),
                0
            },
            // Testing  adding a second lead when the manager now has one thing in it
            new object[]
            {
                leadManager,
                lead,
                1
            },
            // Testing adding the same lead again, we expect to get the same id
            new object[]
            {
                leadManager,
                lead,
                1
            }
        };
    }
    
    /// <summary>
    /// Tests the TryAddLead method when some IDs in the LeadManager are freed up.
    /// Verifies that the correct ID is returned when adding a lead and ensures
    /// that the lead is wired correctly.
    /// </summary>
    /// <param name="leadManager">The LeadManager instance to test the TryAddLead method on.</param>
    /// <param name="lead">The Lead object to add to the LeadManager.</param>
    /// <param name="expectedId">The expected ID to be returned when the lead is added to the manager.</param>
    [Theory]
    [MemberData(nameof(TryAddLeadFirstIdMissingData))]
    public void TryAddLead_ShouldReturnCorrectId_WhenSomeIdFreesUp(LeadManager leadManager, Lead lead, int expectedId)
    {
        // Add some initial leads to the manager
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 1, 2, 3 },
            new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK), out _);
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 3, 4, 5 },
            new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SINK), out _);
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 97 },
            new SortedSet<int> { 0, 43, 76 }, Constants.CurrentDirection.SINK), out _);
        
        // Remove a lead to free up an ID
        leadManager.TryRemoveLead(expectedId, out _);

        var results = leadManager.TryAddLead(lead, out var id);
        
        // Assert that the operation is successful and the correct ID is returned
        Assert.True(results);
        Assert.Equal(expectedId, id);
        
        // Ensure the lead is wired correctly
        foreach (var contact in lead.ContactSet)
        {
            Assert.True(leadManager.IsWiredContact(contact));
        }

        foreach (var output in lead.OutputSet)
        {
            Assert.True(leadManager.IsWiredOutput(output));
        }

    }

    /// <summary>
    /// Data for testing TryAddLead when some lead IDs are freed up in the form
    ///    leadManager (Instance to be used),
    ///    lead (to be added),
    ///    expectedId
    ///    Assuming all global IDs and leads are valid in the session.
    /// </summary>
    public static IEnumerable<object[]> TryAddLeadFirstIdMissingData()
    {
        var leadManager = new LeadManager();

        return new List<object[]>
        {
            new object[]
            {
                // Testing when the first lead in the manager is freed up
                leadManager,
                new Lead(new SortedSet<int> { 2, 3, 4 },
                    new SortedSet<int> { 3, 2 }, Constants.CurrentDirection.SINK),
                0
            },
            new object[]
            {
                // Testing when a lead in the middle of the manager is freed up
                leadManager,
                new Lead(new SortedSet<int> { 3, 4, 5 },
                    new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SINK),
                1
            },
            new object[]
            {
                // Testing when the last lead in the manager is freed up
                leadManager,
                new Lead(new SortedSet<int> { 97 },
                    new SortedSet<int> { 0, 43, 76 }, Constants.CurrentDirection.SINK),
                2
            },
        };
    }
    /// <summary>
    /// Method to check that GetLeadsOfOutput returns the corrected list of leads
    /// </summary>
    /// <param name="leadManager">The lead manager instance to perform the tests on</param>
    /// <param name="outputId">The Id of the output we want to search</param>
    /// <param name="expectedLeads">A list of the leads we expect to be returned
    ///  from the search </param>
    [Theory]
    [MemberData(nameof(GetLeadsWiredToOutputData))]
    public void GetLeadsWiredToOutput_ShouldReturnCorrectLeads_WhenOutputIsWired(LeadManager leadManager, int outputId, List<Lead> expectedLeads)
    {
        var results = leadManager.GetLeadsOfOutput(outputId);
        
        // Check that we got the right about of leads
        Assert.Equal(expectedLeads.Count, results.Count);
        // Check to make sure our results are exactly the same as the expected results
        foreach (var lead in expectedLeads)
        {
            Assert.Contains(lead, results);
        }
    }

    /// <summary>
    /// Data for testing GetLeadsOfOutput
    ///    leadManager (Instance to be used),
    ///    outputId (The Id of the output to be searched for)
    ///    expectedLeads (The list of leads we expect to be returned)
    ///    Assuming all global IDs and leads are valid in the session.
    /// </summary>
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
            // Testing the case that many leads are returned
            new object[]
            {
                leadManager,
                4,
                new List<Lead> {lead1, lead3}
            },
            // Testing the case that one lead is returned
            new object[]
            {
                leadManager,
                6,
                new List<Lead> {lead2}
            },
            // Testing with an output ID that is not in any of the leads
            new object[]
            {
                leadManager,
                10, 
                new List<Lead>() 
            },
            // Testing with an invalid ID
            new object[]
            {
                leadManager,
                -1, 
                new List<Lead>()
            }
        };
    }

    /// <summary>
    /// A method to test TryRemoveLead when the input lead exists.
    /// </summary>
    /// <param name="leadManager">The lead manager instance to perform the tests on.</param>
    /// <param name="leadId"> The Id of the lead to be removed.</param>
    /// <param name="expectedLead">The lead we expected to be removed and returned.</param>
    [Theory]
    [MemberData(nameof(RemoveLeadData))]
    public void RemoveLead_ShouldReturnTrueAndRemoveLead_WhenLeadExists(LeadManager leadManager, int leadId, Lead expectedLead)
    {
        var result = leadManager.TryRemoveLead(leadId, out var removedLead);
        
        // We expect that the method will always succeed 
        Assert.True(result);
        // We check removed lead vs the expected lead
        Assert.Equal(expectedLead, removedLead);
        var outputs = leadManager.GetLeadsOfOutput(expectedLead.OutputSet.First()); // Assuming at least one output
        // Make sure the outputs don't contain the lead we removed
        Assert.DoesNotContain(expectedLead, outputs);
    }

    /// <summary>
    /// Data for testing RemoveLead when the input data is valid
    ///    leadManager (Instance to be used),
    ///    leadId (The Id of the lead to be removed),
    ///    expectedLead (The lead we expect to be removed)
    ///    Assuming all global IDs and leads are valid in the session.
    /// </summary>
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
            // Test to remove the first lead from the manager
            new object[]
            {
                leadManager,
                id1,
                lead1
            },
            // Test to remove the last lead from the manager
            new object[]
            {
                leadManager,
                id2,
                lead2
            }
        };
    }

    /// <summary>
    /// Method to check that GetLeadsOfOutput returns the corrected list of leads when the
    /// data is invalid
    /// </summary>
    /// <param name="leadManager">The lead manager instance to perform the tests on</param>
    /// <param name="leadId">The Id of the output we want to remove. (Should be invalid)</param>
    [Theory]
    [MemberData(nameof(RemoveLeadWithInvalidIdData))]
    public void RemoveLead_ShouldReturnFalse_WhenLeadDoesNotExist(LeadManager leadManager, int leadId)
    {
        var result = leadManager.TryRemoveLead(leadId, out var removedLead);
        
        // The input should be invalid so we expect the method to fail
        Assert.False(result);
        // We expect no lead to be removed
        Assert.Null(removedLead);
    }

    /// <summary>
    /// Data for testing RemoveLead when the input data is invalid
    ///    leadManager (Instance to be used),
    ///    leadId (The Id of the lead to be removed, will be invalid),
    /// </summary>
    public static IEnumerable<object[]> RemoveLeadWithInvalidIdData()
    {
        var leadManager = new LeadManager();

        // Setting up with some leads
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK), out _);
        
        return new List<object[]>
        {
            new object[] { leadManager, -1 }, // Negative ID
            new object[] { leadManager, 1},
            new object[] { leadManager, 100 } // High invalid ID
        };
    }
    
    /// <summary>
    /// Testing removing a lead by the value not Id, when the lead to remove is
    /// valid. 
    /// </summary>
    /// <param name="leadManager">The instance of lead manager the tests will be
    /// performed on.</param>
    /// <param name="leadToRemove"> The lead that will be removed from the lead
    /// manager </param>
    /// <param name="expectedLeadId"> The Id of the lead that is expected to be removed
    /// </param>
    [Theory]
    [MemberData(nameof(RemoveLeadByValueData))]
    public void RemoveLeadByValue_ShouldReturnTrueAndRemoveLead_WhenLeadExists(LeadManager leadManager, 
        Lead leadToRemove, int expectedLeadId)
    {
        var result = leadManager.TryRemoveLead(leadToRemove, out var removedLeadId, out var removedLead);

        Assert.True(result);
        Assert.Equal(expectedLeadId, removedLeadId);
        Assert.Equal(removedLead, leadToRemove);

        // Verify the lead is not returned in any output
        var outputs = leadManager.GetLeadsOfOutput(leadToRemove.OutputSet.First());
        Assert.DoesNotContain(removedLead, outputs);
    }

    /// <summary>
    /// Data for testing RemoveLead by value when the input data is valid
    ///    leadManager (Instance to be used),
    ///    leadToRemove
    ///    expectedLeadId (The Id of the lead we expect to be returned)
    /// </summary>
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
            // Removing the first lead by value
            new object[]
            {
                leadManager,
                lead1,
                id1
            },
            // Removing the last lead by value
            new object[]
            {
                leadManager,
                lead2,
                id2
            }
        };
    }

    /// <summary>
    /// Method to check that RemoveLeadByValue returns false when the inputs are invalid
    /// </summary>
    /// <param name="leadManager">The lead manager instance to perform the tests on</param>
    /// <param name="leadToRemove">The lead that we will attempt to remove (Should be invalid)</param>
    [Theory]
    [MemberData(nameof(RemoveLeadByValueWithInvalidData))]
    public void RemoveLeadByValue_ShouldReturnFalse_WhenLeadDoesNotExist(LeadManager leadManager, Lead leadToRemove)
    {
        var result = leadManager.TryRemoveLead(leadToRemove, out var removedLeadId, out var removedLead);

        Assert.False(result);
        Assert.Equal(-1, removedLeadId);
        Assert.Null(removedLead);
    }

    /// <summary>
    /// Data for testing RemoveLead by value when the input data is invalid
    ///    leadManager (Instance to be used),
    ///    leadToRemove (The lead to be removed, will be invalid),
    /// </summary>
    public static IEnumerable<object[]> RemoveLeadByValueWithInvalidData()
    {
        var leadManager = new LeadManager();

        // Setting up with some leads
        leadManager.TryAddLead(new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK), out _);
        
        return new List<object[]>
        {
            new object[] { leadManager, new Lead(new SortedSet<int> { 4, 5 },
                new SortedSet<int> { 6 }, Constants.CurrentDirection.SINK) }, // Non-existing lead
            new object[] { leadManager, new Lead(new SortedSet<int> { 10, 11 },
                new SortedSet<int> { 12 }, Constants.CurrentDirection.SINK) } // Another non-existing lead
        };
    }


}