using EStimLibrary.Core;
using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.UnitTests.Core.HardwareInterfaces;


/// <summary>
/// A class for unit testing the Lead class
/// </summary>
public class LeadTests
{
    /// <summary>
    /// Test the parameterized Lead constructor with different data values.
    /// </summary>
    /// <param name="contactSet">The set of global contact IDs the lead is
    /// connected to. Assumed to be valid.</param>
    /// <param name="outputSet">The set of global output IDs the lead is
    /// connected to. Assumed to be valid.</param>
    /// <param name="currentDirection">The default direction of current on the
    /// lead.</param>
    [Theory]
    [MemberData(nameof(Constructor_TestData))]
    public void Constructor_ShouldInitSortedSetsAndDirection(
        SortedSet<int> contactSet, SortedSet<int> outputSet,
        Constants.CurrentDirection currentDirection)
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
    /// Assuming all global IDs are valid (existing non-negative integers) in
    /// the session.
    /// </summary>
    public static IEnumerable<object[]> Constructor_TestData()
    {
        return new List<object[]>
        {
            // 1:1 mapping
            new object[]
            {
                new SortedSet<int> { 1 },
                new SortedSet<int> { 0 },
                Constants.CurrentDirection.SOURCE
            },
            // 1:many mapping; non-consecutive
            new object[]
            {
                new SortedSet<int> { 1 },
                new SortedSet<int> { 0, 2, 4 },
                Constants.CurrentDirection.SOURCE
            },
            // many:1 mapping; non-consecutive; double-digit IDs
            new object[]
            {
                new SortedSet<int> { 1, 3, 6, 10, 21 },
                new SortedSet<int> { 0 },
                Constants.CurrentDirection.SOURCE
            },
            // many:many mapping, same number
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
    /// <param name="id">The output or contact ID to check the connections of.
    /// </param>
    /// <param name="searchIsAnOutput">Indicates if the search is for an output
    /// (true) or a contact (false).</param>
    /// <param name="lead">The lead instance to test.</param>
    /// <param name="expectedOutputs">The expected output IDs found.</param>
    /// <param name="expectedResult">The expected boolean result of the search.
    /// </param>
    [Theory]
    [MemberData(nameof(GetConnectedOutputs_TestData))]
    public void GetConnectedOutputs_ShouldReturnExpectedResults(int id,
        bool searchIsAnOutput, Lead lead, SortedSet<int> expectedOutputs,
        bool expectedResult)
    {
        var originalOutputs = lead.OutputSet;
        var originalContacts = lead.ContactSet;

        var expectedOriginalOutputs = new SortedSet<int>(originalOutputs);
        var expectedOriginalContacts = new SortedSet<int>(originalContacts);

        var result = lead.GetConnectedOutputs(id, searchIsAnOutput,
            out var connectedOutputs);

        // Make sure method outputs are correct.
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedOutputs, connectedOutputs);

        // Make sure the values of the lead's output and contact sets have not
        // changed.
        Assert.Equal(expectedOriginalOutputs, lead.OutputSet);
        Assert.Equal(expectedOriginalContacts, lead.ContactSet);
        // Make sure the objects (instances) of the lead's output and contact
        // sets have not been replaced.
        Assert.Same(originalOutputs, lead.OutputSet);
        Assert.Same(originalContacts, lead.ContactSet);
    }


    /// <summary>
    /// Test parameter data for GetConnectedOutputs, following the form:
    ///    id (to search for),
    ///    boolean (searchIsAContact),
    ///    leadInstance,
    ///    globalOutputIdSet,
    ///    boolean (expectedResult)
    /// Assuming all global IDs and leads are valid in the session.
    /// </summary>
    public static IEnumerable<object[]> GetConnectedOutputs_TestData()
    {
        return new List<object[]>
        {
            #region Search by Output ID
            // Testing with an output as input when that output is connected to
            // the lead. We expect true and an output set without the given
            // output.
            new object[]
            {
                3,
                true,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 4, 5 },
                true
            },
            // Test when contactSet is much smaller than outputSet
            new object[]
            {
                8,
                true,
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 3, 4, 5, 6, 8 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5, 6 },
                true
            },
            // Test when outputSet is much smaller than contactSet
            new object[]
            {
                3,
                true,
                new Lead(new SortedSet<int> { 2, 3, 4, 8 },
                    new SortedSet<int> { 3 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { },
                true
            },
            // Testing with an output ID that is not connected to the lead
            // (although there is a connected contact of that ID). We expect to
            // get false and an unmodified output set.
            new object[]
            {
                6,
                true,
                new Lead(new SortedSet<int> { 1, 2, 6 },
                    new SortedSet<int> { 3, 4, 5 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 3, 4, 5 },
                false
            },
            // Testing with an empty outputSet, so output ID DNE.
            new object[]
            {
                6,
                true,
                new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { },
                false
            },
            // Testing with a very large outputSet. Output ID DNE.
            new object[]
            {
                1,
                true,
                new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 2, 3, 4, 5, 6, 7, 8 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 2, 3, 4, 5, 6, 7, 8 },
                false
            },
            #endregion Search by Output ID
            #region Search by Contact ID
            // Testing with a connected contact ID. ID also exists as a
            // connected output ID. We expect to get true but an unmodified
            // output set
            new object[]
            {
                1,
                false,
                new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 1, 3, 4, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 3, 4, 5 },
                true
            },
            // Testing with many things in the outputSet
            new object[]
            {
                2,
                false,
                new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 3, 4, 5, 6, 7 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5, 6, 7 },
                true
            },
            // Testing with an empty outputSet but contact ID found.
            new object[]
            {
                2,
                false,
                new Lead(new SortedSet<int> { 2 },
                    new SortedSet<int> { },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { },
                true
            },
            // Testing with only one ID in the outputSet
            new object[]
            {
                2,
                false,
                new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1 },
                true
            },
            // Testing with a contact as the input which is not connected to
            // the lead. We expect to get false and an unmodified output set
            new object[]
            {
                1,
                false,
                new Lead(new SortedSet<int> { 2 },
                    new SortedSet<int> { 3, 4, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5 },
                false
            },
            // Testing with an empty contact set
            new object[]
            {
                2,
                false,
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 3, 4, 5, 6, 7 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 3, 4, 5, 6, 7 },
                false
            },
            // Testing with an empty outputSet and contact ID not found
            new object[]
            {
                2,
                false,
                new Lead(new SortedSet<int> { 9 },
                    new SortedSet<int> { },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { },
                false
            },
            // Testing with a large contactSet and an outputSet with only one
            // element in it
            new object[]
            {
                2,
                false,
                new Lead(new SortedSet<int> { 5, 6, 7, 8 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1 },
                false
            }
            #endregion Search by Contact ID
        };
    }

    /// <summary>
    /// Test for GetConnectedContacts.
    /// </summary>
    /// <param name="id">The contact or output ID to check the connections of.
    /// </param>
    /// <param name="searchIsAContact">Indicates if the search is for a contact
    /// (true) or an output (false).</param>
    /// <param name="lead">The lead instance to test.</param>
    /// <param name="expectedContacts">The expected contact IDs found.</param>
    /// <param name="expectedResult">The expected boolean result of the search.
    /// </param>
    [Theory]
    [MemberData(nameof(GetConnectedContacts_TestData))]
    public void GetConnectedContacts_ShouldReturnExpectedResults(int id,
        bool searchIsAContact, Lead lead, SortedSet<int> expectedContacts,
        bool expectedResult)
    {
        var originalOutputs = lead.OutputSet;
        var originalContacts = lead.ContactSet;

        var expectedOriginalOutputs = new SortedSet<int>(originalOutputs);
        var expectedOriginalContacts = new SortedSet<int>(originalContacts);

        var result = lead.GetConnectedContacts(id, searchIsAContact,
            out var connectedContacts);

        // Make sure method outputs are correct.
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedContacts, connectedContacts);

        // Make sure the values of the lead's output and contact sets have not
        // changed.
        Assert.Equal(expectedOriginalOutputs, lead.OutputSet);
        Assert.Equal(expectedOriginalContacts, lead.ContactSet);
        // Make sure the objects (instances) of the lead's output and contact
        // sets have not been replaced.
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
    public static IEnumerable<object[]> GetConnectedContacts_TestData()
    {
        return new List<object[]>
        {
            #region Search by Contact ID
            // Testing when a connected contact ID is given. We expect true and
            // a modified set of contacts.
            new object[]
            {
                3,
                true,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 2 },
                true
            },
            // Testing with only one ID in the contactSet
            new object[]
            {
                2,
                true,
                new Lead(new SortedSet<int> { 2 },
                    new SortedSet<int> { 3, 4, 5 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { },
                true
            },
            // Testing with an empty outputSet
            new object[]
            {
                3,
                true,
                new Lead(new SortedSet<int> { 2, 3 },
                    new SortedSet<int> { },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 2 },
                true
            },
            // Testing with only one contact in the contactSet
            new object[]
            {
                3,
                true,
                new Lead(new SortedSet<int> { 3 },
                    new SortedSet<int> { 2, 3, 4 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { },
                true
            },
            // Testing when an unconnected contact ID is given. We expect false
            // and an unmodified set of contacts.
            new object[]
            {
                4,
                true,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 2, 3 },
                false
            },
            // Testing with a different current direction
            new object[]
            {
                5,
                true,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1, 2, 3 },
                false
            },
            // Testing with an empty contactSet
            new object[]
            {
                4,
                true,
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 3, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { },
                false
            },
            // Testing with an empty contactSet but ID matches an output.
            // Searching by contact, so should still return false
            new object[]
            {
                4,
                true,
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 4, 3, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { },
                false
            },
            // Testing with a contact set of size 1
            new object[]
            {
                2,
                true,
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 3, 4, 5 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1 },
                false
            },
            #endregion Search by Contact ID
            #region Search by Output ID
            // Testing when a connected output is given. We expect true and an
            // unmodified set of contacts.
            new object[]
            {
                4,
                false,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3, 4, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 2, 3 },
                true
            },
            // Testing with an outputSet of size 1
            new object[]
            {
                3,
                false,
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 3 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1, 2, 3 },
                true
            },
            // Testing with a contactSet of size 1
            new object[]
            {
                3,
                false,
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 3 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1 },
                true
            },
            // Testing with an empty contact set
            new object[]
            {
                4,
                false,
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 3, 4 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { },
                true
            },
            // Testing when an unconnected output is given. We expect false and
            // an unmodified set of contacts.
            new object[]
            {
                4,
                false,
                new Lead(new SortedSet<int> { 1, 2, 3, 4 },
                    new SortedSet<int> { 3, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { 1, 2, 3, 4 },
                false
            },
            // Testing with an empty outputSet
            new object[]
            {
                5,
                false,
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1 },
                false
            },
            // Testing with an empty contactSet
            new object[]
            {
                4,
                false,
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { 3, 5 },
                    Constants.CurrentDirection.SINK),
                new SortedSet<int> { },
                false
            },
            // Testing with no overlap between the output and contact set ids.
            new object[]
            {
                2,
                false,
                new Lead(new SortedSet<int> { 1, 2 },
                    new SortedSet<int> { 3 },
                    Constants.CurrentDirection.SOURCE),
                new SortedSet<int> { 1, 2 },
                false
            }
            #endregion Search by Output ID
        };
    }


    /// <summary>
    /// Test for IsFullyIndependent.
    /// </summary>
    /// <param name="lead1">The first Lead instance to check for independence.
    /// </param>
    /// <param name="lead2">The second Lead instance to compare against.
    /// </param>
    /// <param name="expectedResult">The expected result indicating whether or
    /// or not the two Leads are fully independent.</param>
    [Theory]
    [MemberData(nameof(IsFullyIndependent_TestData))]
    public void IsFullyIndependent_ShouldReturnExpectedResults(Lead lead1,
        Lead lead2, bool expectedResult)
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
    public static IEnumerable<object[]> IsFullyIndependent_TestData()
    {
        return new List<object[]>
        {
            #region Test Independent Leads
            // Test when the leads are fully independent 
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 2 },
                    new SortedSet<int> { 2 },
                    Constants.CurrentDirection.SOURCE),
                true
            },
            // Testing on two empty leads
            new object[]
            {
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { },
                    Constants.CurrentDirection.SINK),
                true
            },
            // Testing on leads with many contacts and outputs. Some contact 
            // IDs match the other Lead's output IDs and vice versa, but still
            // independent.
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 1, 4, 5 },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 4, 5, 6 },
                    new SortedSet<int> { 2, 3, 6 },
                    Constants.CurrentDirection.SOURCE),
                true
            },
            // Testing where contact and output sets are the same IDs within
            // each Lead.
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 4, 5 },
                    new SortedSet<int> { 1, 4, 5 },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 2, 3, 6 },
                    new SortedSet<int> { 2, 3, 6 },
                    Constants.CurrentDirection.SOURCE),
                true
            },
            // Testing where one lead has many contacts and outputs and the
            // other has few.
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 4 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 2, 3, 6 },
                    new SortedSet<int> { 2, 3, 6 },
                    Constants.CurrentDirection.SINK),
                true
            },
            // Testing where one lead has empty sets
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 4 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { },
                    new SortedSet<int> { },
                    Constants.CurrentDirection.SINK),
                true
            },
            // Test for when the contact set of one lead is many but the other
            // is one, and the opposite for the output sets.
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 2, 4 },
                    new SortedSet<int> { 4 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 3 },
                    new SortedSet<int> { 1, 2, 3, 5, 6, 7, 8 },
                    Constants.CurrentDirection.SINK),
                true
            },
            #endregion Test Independent Leads
            #region Test Non-Independent Leads
            // Tests for when the leads are not fully independent
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when the leads are the exact same but with larger
            // contact sets than output sets.
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when the leads are the exact same but with larger
            // output sets than contact sets
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1, 2, 3 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1, 2, 3 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when the outputs are the exact same but different and
            // larger contact sets than output sets.
            new object[]
            {
                new Lead(new SortedSet<int> { 1, 2, 3 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 4, 5, 6 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when one Lead has only one output and the other has
            // many. Contact IDs conflict.
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1, 2, 4 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 3 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when both Leads have very different sized output and
            // contact sets, but one output conflicts.
            new object[]
            {
                new Lead(new SortedSet<int> { 1 },
                    new SortedSet<int> { 1, 2, 3, 4 },
                    Constants.CurrentDirection.SINK),
                new Lead(new SortedSet<int> { 2, 3, 4, 5 },
                    new SortedSet<int> { 4 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when both leads have the same current direction.
            // One output conflicts.
            new object[]
            {
                new Lead(new SortedSet<int> { 8, 9, 10 },
                    new SortedSet<int> { 3, 4 },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 1, 2, 3, 9 },
                    new SortedSet<int> { 1 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when one contact and one output conflict.
            // Same current direction.
            new object[]
            {
                new Lead(new SortedSet<int> { 8, 9, 10 },
                    new SortedSet<int> { 1, 3, 4 },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 1, 2, 3, 9 },
                    new SortedSet<int> { 1, 5 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when multiple contacts and one output conflict.
            // Same current direction.
            new object[]
            {
                new Lead(new SortedSet<int> { 7, 9, 10 },
                    new SortedSet<int> { 1, 3, 4 },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 1, 2, 3, 7, 9, 12 },
                    new SortedSet<int> { 1, 5 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when one contact and multiple outputs conflict.
            // Same current direction.
            new object[]
            {
                new Lead(new SortedSet<int> { 8, 9, 10 },
                    new SortedSet<int> { 1, 3, 4 },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 1, 2, 3, 9 },
                    new SortedSet<int> { 1, 4, 5, 6 },
                    Constants.CurrentDirection.SOURCE),
                false
            },
            // Test for when multiple contacts and multiple outputs conflict.
            // Same current direction.
            new object[]
            {
                new Lead(new SortedSet<int> { 7, 8, 9, 10 },
                    new SortedSet<int> { 1, 3, 4 },
                    Constants.CurrentDirection.SOURCE),
                new Lead(new SortedSet<int> { 1, 2, 3, 7, 9, 12 },
                    new SortedSet<int> { 1, 4, 5, 6 },
                    Constants.CurrentDirection.SOURCE),
                false
            }
            #endregion Test Non-Independent Leads
        };
    }

    /// <summary>
    /// Test for IndependentLeadsExist.
    /// </summary>
    /// <param name="leads">A collection of Lead instances to check for an
    /// independent pair.</param>
    /// <param name="expectedResult">The expected result indicating whether or
    /// not at least one pair of independent Leads exist.</param>
    [Theory]
    [MemberData(nameof(IndependentLeadsExist_TestData))]
    public void IndependentLeadsExist_ShouldReturnExpectedResults(
        IEnumerable<Lead> leads, bool expectedResult)
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
    public static IEnumerable<object[]> IndependentLeadsExist_TestData()
    {
        return new List<object[]>
        {
            // Testing where 2 leads are fully independent
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 5, 6 },
                        new SortedSet<int> { 7, 8 },
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            // Testing where the outputSets have only one ID and leads are
            // dependent (single conflicting contact)
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 1, 3 },
                        new SortedSet<int> { 4 },
                        Constants.CurrentDirection.SINK)
                },
                false
            },
            // Testing where currentDirection is opposite, but all conflicting
            // contacts
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 5 },
                        Constants.CurrentDirection.SINK)
                },
                false
            },
            // Testing where the outputSets have only one ID and leads are
            // independent 
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4, 5 },
                        new SortedSet<int> { 6 },
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            // Testing where sets are the same size and dependent: conflicting
            // single contact and output
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 2, 3 },
                        new SortedSet<int> { 4, 5 },
                        Constants.CurrentDirection.SOURCE)
                },
                false
            },
            // Testing with more than 2 leads in list. One conflicting pair by
            // single output ID, but independent pairs exist
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4, 5 },
                        new SortedSet<int> { 6, 7 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 8, 9 },
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE)
                },
                true
            },
            // Testing with more than 2 leads where all current directions are
            // the same 
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 3 },
                        new SortedSet<int> { 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4 },
                        new SortedSet<int> { 5 },
                        Constants.CurrentDirection.SOURCE)
                },
                true
            },
            // Testing with leads with conflicting outputs but different
            // contacts
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4 },
                        new SortedSet<int> { 3, 5 },
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
            // Testing with a single lead
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1 },
                        new SortedSet<int> { 2 },
                        Constants.CurrentDirection.SOURCE)
                },
                false
            },
            // Testing with large fully independent lead set
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 5, 6 },
                        new SortedSet<int> { 7, 8 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 9, 10 },
                        new SortedSet<int> { 11 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> {11, 12, 13 },
                        new SortedSet<int> { 12 },
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            // Testing with large mostly independent lead set
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3, 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 5, 6 },       // contact 5
                        new SortedSet<int> { 7, 8 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 9, 10 },
                        new SortedSet<int> { 11 },              // output 11
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> {5, 12, 13 },   // contact 5
                        new SortedSet<int> { 11 },              // output 11
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            // Testing with large lead set where some conflicting but a few
            // independent pairs exist.
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },   // indp A B
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 2, 3 },   // indp     C
                        new SortedSet<int> { 4 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 4, 5 },   // indp   B
                        new SortedSet<int> { 5 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 5 },      // indp A   C
                        new SortedSet<int> { 6 },
                        Constants.CurrentDirection.SOURCE)
                },
                true
            },
            // Testing with large lead set with mixed independence
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },   // contact 2
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 3, 4 },
                        new SortedSet<int> { 5 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 6, 7 },
                        new SortedSet<int> { 8 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> { 9, 10 },  // contact 9
                        new SortedSet<int> { 11 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 2, 9 },   // contact 2, 9
                        new SortedSet<int> { 10 },
                        Constants.CurrentDirection.SINK)
                },
                true
            },
            // Testing with longer list, and all conflict with each other in
            // some way.
            new object[]
            {
                new List<Lead>
                {
                    new Lead(new SortedSet<int> { 1, 2 },
                        new SortedSet<int> { 3 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 3, 4 },
                        new SortedSet<int> { 5, 3},
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> { 3, 7 },
                        new SortedSet<int> { 8, 3 },
                        Constants.CurrentDirection.SINK),
                    new Lead(new SortedSet<int> {1, 3, 9, 10 },
                        new SortedSet<int> { 11 },
                        Constants.CurrentDirection.SOURCE),
                    new Lead(new SortedSet<int> {2, 3, 9 },
                        new SortedSet<int> { 10 },
                        Constants.CurrentDirection.SINK)
                },
                false
            }
        };
    }
}
