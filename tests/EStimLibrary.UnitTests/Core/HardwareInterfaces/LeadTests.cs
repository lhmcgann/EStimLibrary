using EStimLibrary.Core;
using EStimLibrary.Core.HardwareInterfaces;

namespace EStimLibrary.UnitTests.Core.HardwareInterfaces;

/// <summary>
/// A class for unit testing the lead class
/// </summary>
public class LeadTests
{
    /// <summary>
    /// Test the parameterized constructor with different data values.
    /// </summary>
    /// <param name="contactSet">The set of contact IDs.</param>
    /// <param name="outputSet">The set of output IDs.</param>
    /// <param name="currentDirection">The current direction of the lead.</param>
    [Theory]
    [MemberData(nameof(Constructor_Test_Data))]
    public void Constructor_Should_Init_SortedSets_And_Direction(SortedSet<int> contactSet,
        SortedSet<int> outputSet, Constants.CurrentDirection currentDirection)
    {
        // Act
        var lead = new Lead(contactSet, outputSet, currentDirection);

        // Assert
        Assert.Equal(contactSet, lead.ContactSet);
        Assert.Equal(outputSet, lead.OutputSet);
        Assert.Equal(currentDirection, lead.CurrentDirection);
    }

    /// <summary>
    /// Test parameter data for the Lead constructor, following the form:
    ///    globalContactIdSet,
    ///    globalOutputIdSet,
    ///    defaultCurrentDirection
    /// Assuming all global IDs are valid (existing non-negative integers) in the session.
    /// </summary>
    public static IEnumerable<object[]> Constructor_Test_Data()
    {
        return new List<object[]>
        {
            new object[]
            {
                new SortedSet<int> { 1, 2, 3 },
                new SortedSet<int> { 0, 2, 3 },
                Constants.CurrentDirection.SOURCE
            },
            // Different output and contact set sizes
            new object[]
            {
                new SortedSet<int> { 4, 5, 6 },
                new SortedSet<int> { 1, 2 },
                Constants.CurrentDirection.SOURCE
            },
            // Different current direction
            new object[]
            {
                new SortedSet<int> { 7, 8, 9 },
                new SortedSet<int> { 3, 5, 7 },
                Constants.CurrentDirection.SINK
            }
        };
    }

    /// <summary>
    /// Test for GetConnectedOutputs.
    /// </summary>
    /// <param name="id">The ID to check for connections.</param>
    /// <param name="searchIsAnOutput">Indicates if the search is for an output.</param>
    /// <param name="lead">The lead instance to test.</param>
    /// <param name="expectedOutputs">The expected outputs.</param>
    /// <param name="expectedResult">The expected result of the search.</param>
    [Theory]
    [MemberData(nameof(GetConnectedOutputs_Test_Data))]
    public void GetConnectedOutputs_Should_Return_Expected_Results(int id, bool searchIsAnOutput,
        Lead lead, SortedSet<int> expectedOutputs, bool expectedResult)
    {
        var originalOutputs = lead.OutputSet; 
        var originalContacts = lead.ContactSet; 

        var expectedOriginalOutputs = new SortedSet<int>(originalOutputs);
        var expectedOriginalContacts = new SortedSet<int>(originalContacts);

        var result = lead.GetConnectedOutputs(id, searchIsAnOutput, out var connectedOutputs);
    
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedOutputs, connectedOutputs);

        Assert.Equal(expectedOriginalOutputs, lead.OutputSet);
        Assert.Equal(expectedOriginalContacts, lead.ContactSet);
        Assert.Same(originalOutputs, lead.OutputSet); 
        Assert.Same(originalContacts, lead.ContactSet); 
    }

    
    /// <summary>
    /// Test parameter data for GetConnectedOutputs, following the form:
    ///    id,                        
    ///    boolean (searchIsAContact),           
    ///    leadInstance,               
    ///    globalOutputIdSet,         
    ///    boolean (expectedResult)
    /// Assuming all global IDs and leads are valid in the session.
    /// </summary>
    public static IEnumerable<object[]> GetConnectedOutputs_Test_Data()
    {
        return new List<object[]>
        {
            // Testing with an output as input when that output is connected to the lead
            // We expect true and an output set without the given output
            new object[]
            {
                3, true, new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 4, 5 }, true
            },
            // Test when contactSet is much smaller than outputSet
            new object[]
            {
                8, true, new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 3, 4, 5, 6, 8 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5, 6 }, true
            },
            // Test when outputSet is much smaller than contactSet
            new object[]
            {
                3, true, new Lead(new SortedSet<int> { 2, 3, 4, 8 },
                    new SortedSet<int> { 3 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { }, true
            },
            // Testing with an output as the input where it is connected to the lead
            // We expect to get false and an unmodified output set
            new object[]
            {
                6, true, new Lead(new SortedSet<int> { 1, 2, 6 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 3, 4, 5 }, false
            },
            // Testing with an empty outputSet
            new object[]
            {
                6, true, new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { }, false
            },
            // Testing with a very large outputSet
            new object[]
            {
                1, true, new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 2, 3, 4, 5, 6, 7, 8 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 2, 3, 4, 5, 6, 7, 8 }, false
            },
            // Testing with a contact as the input which is connected to the lead
            // We expect to get true and an unmodified output set
            new object[]
            {
                1, false, new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5 }, true
            },
            // Testing with many things in the outputSet
            new object[]
            {
                2, false, new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 3, 4, 5, 6, 7 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5, 6, 7 }, true
            },
            // Testing with an empty outputSet
            new object[]
            {
                2, false, new Lead(new SortedSet<int> { 2 },
                    new SortedSet<int> { }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { }, true
            },
            // Testing with only one object in the outputSet
            new object[]
            {
                2, false, new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1 }, true
            },
            // Testing with a contact as the input which is not connected to the lead
            // We expect to get false and an unmodified output set
            new object[]
            {
                1, false, new Lead(new SortedSet<int> { 2 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5 }, false
            },
            // Testing with an empty contact set
            new object[]
            {
                2, false, new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 3, 4, 5, 6, 7 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5, 6, 7 }, false
            },
            // Testing with an empty outputSet
            new object[]
            {
                2, false, new Lead(new SortedSet<int> { 9 },
                    new SortedSet<int> { }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { }, false
            },
            // Testing with a large contactSet and an outputSet with only one element in it
            new object[]
            {
                2, false, new Lead(new SortedSet<int> { 5, 6, 7, 8 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1 }, false
            }
        };
    }

    /// <summary>
    /// Test for GetConnectedContacts.
    /// </summary>
    /// <param name="id">The ID to check for connections.</param>
    /// <param name="searchIsAContact">Indicates if the search is for a contact.</param>
    /// <param name="lead">The lead instance to test.</param>
    /// <param name="expectedContacts">The expected contacts.</param>
    /// <param name="expectedResult">The expected result of the search.</param>
    [Theory]
    [MemberData(nameof(GetConnectedContacts_Test_Data))]
    public void GetConnectedContacts_Should_Return_Expected_Results(int id, bool searchIsAContact,
        Lead lead, SortedSet<int> expectedContacts, bool expectedResult)
    {
        var originalOutputs = lead.OutputSet; 
        var originalContacts = lead.ContactSet;

        var expectedOriginalOutputs = new SortedSet<int>(originalOutputs);
        var expectedOriginalContacts = new SortedSet<int>(originalContacts);
        
        var result = lead.GetConnectedContacts(id, searchIsAContact, out var connectedContacts);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedContacts, connectedContacts);

        Assert.Equal(expectedOriginalOutputs, lead.OutputSet);
        Assert.Equal(expectedOriginalContacts, lead.ContactSet);
        Assert.Same(originalOutputs, lead.OutputSet); 
        Assert.Same(originalContacts, lead.ContactSet); 
    }
    
    /// <summary>
    /// Test parameter data for GetConnectedContacts, following the form:
    ///    id,                        
    ///    boolean (searchIsAContact),           
    ///    leadInstance,               
    ///    globalContactIdSet,         
    ///    boolean (expectedResult)
    /// Assuming all global IDs and leads are valid in the session.
    /// </summary>
    public static IEnumerable<object[]> GetConnectedContacts_Test_Data()
    {
        return new List<object[]>
        {
            // Testing when a contact is given and that contact is connected to the lead
            // We expect true and a modified set of contacts
            new object[]
            {
                3, true, new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 2 }, true
            },
            // Testing with only one object in the contactSet
            new object[]
            {
                2, true, new Lead(new SortedSet<int> { 2 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { }, true
            },
            // Testing with an empty outputSet
            new object[]
            {
                3, true, new Lead(new SortedSet<int> { 2, 3 },
                    new SortedSet<int> { }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 2 }, true
            },
            // Testing with only one contact in the contactSet
            new object[]
            {
                3, true, new Lead(new SortedSet<int> { 3 },
                    new SortedSet<int> { 2, 3, 4 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { }, true
            },
            // Testing when a contact is given and that contact is not connected to the lead
            // We expect false and an unmodified set of contacts
            new object[]
            {
                4, true, new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 2, 3 }, false
            },
            // Testing with a different current direction
            new object[]
            {
                5, true, new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1, 2, 3 }, false
            },
            // Testing with an empty contactSet
            new object[]
            {
                4, true, new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 3, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { }, false
            },
            // Testing with a contact set of size 1
            new object[]
            {
                2, true, new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1 }, false
            },
            // Testing when an output is given and that output is connected to the lead
            // We expect true and an unmodified set of contacts
            new object[]
            {
                4, false, new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 2, 3 }, true
            },
            // Testing with an outputSet of size 1
            new object[]
            {
                3, false, new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1, 2, 3 }, true
            },
            // Testing with a contactSet of size 1
            new object[]
            {
                3, false, new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 3 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1 }, true
            },
            // Testing with an empty contact set
            new object[]
            {
                4, false, new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 3, 4 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { }, true
            },
            // Testing when and output is given and that output is not connected to the lead
            // We expect false and an unmodified set of contacts
            new object[]
            {
                4, false, new Lead(new SortedSet<int> { 1, 2, 3, 4 },
                    new SortedSet<int> { 3, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 2, 3, 4 }, false
            },
            // Testing with an empty outputSet
            new object[]
            {
                5, false, new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1 }, false
            },
            // Testing with an empty contactSet
            new object[]
            {
                4, false, new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 3, 5 }, Constants.CurrentDirection.SINK),
                new SortedSet<int> { }, false
            },
            // Testing with no overlap between the output and contact set ids.
            new object[]
            {
                2, false, new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 3 }, Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1, 2 }, false
            },
        };
    }


    /// <summary>
    /// Test for IsFullyIndependent.
    /// </summary>
    /// <param name="lead1">The first instance to check for independence.</param>
    /// <param name="lead2">The second instance to compare against.</param>
    /// <param name="expectedResult">The expected result indicating whether the two leads are fully independent.</param>
    // 
    [Theory]
    [MemberData(nameof(IsFullyIndependent_Test_Data))]
    public void IsFullyIndependent_Should_Return_Expected_Results(Lead lead1, Lead lead2, bool expectedResult)
    {
        // Act
        var result = lead1.IsFullyIndependent(lead2);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    /// <summary>
    /// Test parameter data for isFullyIndependent, following the form:
    ///    leadInstance,                      
    ///    leadInstance,                     
    ///    boolean (expectedResult)              
    /// Assuming all leads are valid in the session.
    /// </summary>
    public static IEnumerable<object[]> IsFullyIndependent_Test_Data()
    {
        return new List<object[]>
        {
            // Test when the leads are fully independent 
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 2 },
                    new SortedSet<int> { 2 }, Constants.CurrentDirection.SOURCE),
                true
            },
            // Testing on two empty leads
            new object[]
            {
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { }, Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { }, Constants.CurrentDirection.SINK),
                true
            },
            // Testing on leads with many contacts and outputs
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 1, 4, 5 }, Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 4, 5, 6 },
                    new SortedSet<int> { 2, 3, 6 }, Constants.CurrentDirection.SOURCE),
                true
            },
            // Testing where contact and output sets are the same ids for both leads
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 4, 5 },
                    new SortedSet<int> { 1, 4, 5 }, Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 2, 3, 6 },
                    new SortedSet<int> { 2, 3, 6 }, Constants.CurrentDirection.SOURCE),
                true
            },
            // Testing where one lead has many contacts and outputs and the other has few
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 4 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 2, 3, 6 },
                    new SortedSet<int> { 2, 3, 6 }, Constants.CurrentDirection.SINK),
                true
            },
            // Testing where one lead has empty sets
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 4 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { }, Constants.CurrentDirection.SINK),
                true
            },
            // Test for when the contact set of one lead is many but the other is one
            // And the opposite for the output sets
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 2, 4 },
                    new SortedSet<int> { 4 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 3 },
                    new SortedSet<int> { 1, 2, 3, 5, 6, 7, 8 }, Constants.CurrentDirection.SINK),
                true
            },
            // Tests for when the leads are not fully independent
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when the leads are the exact same but with larger contact sets
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when the leads are the exact same but with larger output sets
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1, 2, 3 }, Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when the leads are the exact same but with larger contact sets
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 4, 5, 6 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when one lead has only one output and the other has many
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1, 2, 4 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 3 }, Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when both leads have very different sized output and contact set sizes
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1, 2, 3, 4 }, Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 2, 3, 4, 5 },
                    new SortedSet<int> { 4 }, Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when both leads have the same current direction
            new object[]
            {
                new Lead(new SortedSet<int> { 8, 9, 10 },
                    new SortedSet<int> { 3, 4 }, Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 1, 2, 3, 9 },
                    new SortedSet<int> { 1 }, Constants.CurrentDirection.SOURCE),
                false
            },
        };
    }

    /// <summary>
    /// Test for IndependentLeadsExist.
    /// </summary>
    /// <param name="leads">A collection of Lead instances to check for independence.</param>
    /// <param name="expectedResult">The expected result indicating whether independent leads exist.</param>
    // 
    [Theory]
    [MemberData(nameof(IndependentLeadsExist_Test_Data))]
    public void IndependentLeadsExist_Should_Return_Expected_Results(IEnumerable<Lead> leads, bool expectedResult)
    {
        // Act
        var result = Lead.IndependentLeadsExist(leads);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    /// <summary>
    /// Test parameter data for IndependentLeadsExist, following the form:
    ///    IEnumerableLeadSet,                     
    ///    boolean (expectedResult)              
    /// Assuming all leads are valid in the session.
    /// </summary>
    public static IEnumerable<object[]> IndependentLeadsExist_Test_Data()
    {
        return new List<object[]>
        {
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 5, 6 }, new SortedSet<int> { 7, 8 },
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            // Testing where the outputSets have only one object and leads are dependent 
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 1, 3 }, new SortedSet<int> { 4 },
                        Constants.CurrentDirection.SINK)
                },
                false
            },
            // Testing where currentDirection is opposite 
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 5 },
                        Constants.CurrentDirection.SINK)
                },
                false
            },
            // Testing where the outputSets have only one object and leads are independent 
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4, 5 }, new SortedSet<int> { 6 },
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            // Testing where sets are the same size and dependent 
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 2, 3 }, new SortedSet<int> { 4, 5 },
                        Constants.CurrentDirection.SOURCE)
                },
                false
            },
            // Testing with more than 2 leads in list
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4, 5 }, new SortedSet<int> { 6, 7 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 8, 9 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE)
                },
                true
            },
            // Testing with more than 2 leads where all currents are the same 
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 3 }, new SortedSet<int> { 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4 }, new SortedSet<int> { 5 },
                        Constants.CurrentDirection.SOURCE)
                },
                true
            },
            // Testing with leads with same outputs but different contacts
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4 }, new SortedSet<int> { 3, 5 },
                        Constants.CurrentDirection.SINK)
                },
                false
            },
            // Testing with an empty lead set
            new object[]
            {
                new List<Lead>(),
                false
            },
            // Testing with a Single lead
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1 }, new SortedSet<int> { 2 },
                        Constants.CurrentDirection.SOURCE)
                },
                false
            },
            // Testing with large independent lead set
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 5, 6 }, new SortedSet<int> { 7, 8 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 9, 10 }, new SortedSet<int> { 11 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> {5, 12, 13 }, new SortedSet<int> { 11 },
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            // Testing with large dependent lead set
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 2, 3 }, new SortedSet<int> { 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4, 5 }, new SortedSet<int> { 5 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 5 }, new SortedSet<int> { 6 }, Constants.CurrentDirection.SOURCE)
                },
                true
            },
            // Testing with large lead set with mixed independence
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 3, 4 }, new SortedSet<int> { 5 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 6, 7 }, new SortedSet<int> { 8 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 9, 10 }, new SortedSet<int> { 11 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 2, 9 }, new SortedSet<int> { 10 },
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 }, new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 3, 4 }, new SortedSet<int> { 5, 3},
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 3, 7 }, new SortedSet<int> { 8, 3 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> {1, 3, 9, 10 }, new SortedSet<int> { 11 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> {2, 3, 9 }, new SortedSet<int> { 10 },
                        Constants.CurrentDirection.SINK)
                },
                false
            }
        };
    }
}
