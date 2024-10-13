using EStimLibrary.Core;
using EStimLibrary.Core.HardwareInterfaces;
using Xunit;
using System.Collections.Generic;

namespace EStimLibrary.UnitTests.Core.HardwareInterfaces
{
    public class LeadTests
    {
        /// <summary>
        /// Test the parameterized constructor with different data values.
        /// </summary>
        [Theory]
        [MemberData(nameof(ConstructorTestData))]
        public void ConstructorShouldInitSortedSetsAndDirection(SortedSet<int> contactSet, 
            SortedSet<int> outputSet, Constants.CurrentDirection currentDirection) 
        {
            // Act
            var lead = new Lead(contactSet, outputSet, currentDirection);
            
            // Assert
            Assert.Equal(contactSet, lead.ContactSet);
            Assert.Equal(outputSet, lead.OutputSet);
            Assert.Equal(currentDirection, lead.CurrentDirection);
        }

        public static IEnumerable<object[]> ConstructorTestData() 
        {
            return new List<object[]> 
            {
                new object[] 
                {
                    new SortedSet<int> { 1, 2, 3 }, 
                    new SortedSet<int> { 0, 2, 3 },
                    Constants.CurrentDirection.SOURCE 
                },
                new object[] 
                { 
                    new SortedSet<int> { 4, 5, 6 }, 
                    new SortedSet<int> { 1, 2 }, 
                    Constants.CurrentDirection.SOURCE 
                },
                new object[] 
                {
                    new SortedSet<int> { 7, 8, 9 },
                    new SortedSet<int> { 3, 5, 7 },
                    Constants.CurrentDirection.SINK
                }
            };
        }

        // Test for GetConnectedOutputs
        [Theory]
        [MemberData(nameof(GetConnectedOutputsTestData))]
        public void GetConnectedOutputs_ShouldReturnExpectedResults(int id, bool searchIsAnOutput, 
            Lead lead, SortedSet<int> expectedOutputs, bool expectedResult)
        {
            // Arrange
            // Act
            var result = lead.GetConnectedOutputs(id, searchIsAnOutput, out var connectedOutputs);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedOutputs, connectedOutputs);
        }

        public static IEnumerable<object[]> GetConnectedOutputsTestData()
        {
            return new List<object[]>
            {
                // Testing with an output as input when that output is connected to the lead
                // We expect true and a modified output set
                new object[] {3, true, new Lead(new SortedSet<int> {1, 2, 3}, 
                    new SortedSet<int> {3, 4, 5}, Constants.CurrentDirection.SINK),
                    new SortedSet<int> {4, 5}, true},
                new object[] {8, true, new Lead(new SortedSet<int> {1}, 
                        new SortedSet<int> {3, 4, 5, 6, 8}, Constants.CurrentDirection.SINK),
                    new SortedSet<int> {3, 4, 5, 6}, true},
                new object[] {3, true, new Lead(new SortedSet<int> {2, 3, 4, 8}, 
                        new SortedSet<int> {3}, Constants.CurrentDirection.SINK),
                    new SortedSet<int> {}, true},
                // Testing with an output as the input where it is connected to the lead
                // We expect to get false and an unmodified output set
                new object[] {6, true, new Lead(new SortedSet<int> {1, 2, 6}, 
                    new SortedSet<int> {3, 4, 5},Constants.CurrentDirection.SOURCE),
                    new SortedSet<int> {3, 4, 5}, false},
                new object[] {6, true, new Lead(new SortedSet<int> {1, 2}, 
                        new SortedSet<int> {},Constants.CurrentDirection.SOURCE),
                    new SortedSet<int> {}, false},
                new object[] {1, true, new Lead(new SortedSet<int> {1, 2}, 
                        new SortedSet<int> {2, 3, 4, 5, 6, 7, 8},Constants.CurrentDirection.SOURCE),
                    new SortedSet<int> {2, 3, 4, 5, 6, 7, 8}, false},
                // Testing with a contact as the input which is connected to the lead
                // We expect to get true and an unmodified output set
                new object[] {1, false, new Lead(new SortedSet<int> {1, 2}, 
                        new SortedSet<int> {3, 4, 5},Constants.CurrentDirection.SINK),
                    new SortedSet<int> {3, 4, 5}, true},
                new object[] {2, false, new Lead(new SortedSet<int> {1, 2}, 
                    new SortedSet<int> {3, 4, 5, 6, 7},Constants.CurrentDirection.SINK),
                new SortedSet<int> {3, 4, 5, 6, 7}, true},
                new object[] {2, false, new Lead(new SortedSet<int> {2}, 
                        new SortedSet<int> {},Constants.CurrentDirection.SINK),
                    new SortedSet<int> {}, true},
                new object[] {2, false, new Lead(new SortedSet<int> {1, 2}, 
                        new SortedSet<int> {1},Constants.CurrentDirection.SINK),
                    new SortedSet<int> {1}, true},
                // Testing with a contact as the input which is not connected to the lead
                // We expect to get false and an unmodified output set
                new object[] {1, false, new Lead(new SortedSet<int> {2}, 
                        new SortedSet<int> {3, 4, 5},Constants.CurrentDirection.SINK),
                    new SortedSet<int> {3, 4, 5}, false},
                new object[] {2, false, new Lead(new SortedSet<int> {}, 
                        new SortedSet<int> {3, 4, 5, 6, 7},Constants.CurrentDirection.SINK),
                    new SortedSet<int> {3, 4, 5, 6, 7}, false},
                new object[] {2, false, new Lead(new SortedSet<int> {9}, 
                        new SortedSet<int> {},Constants.CurrentDirection.SINK),
                    new SortedSet<int> {}, false},
                new object[] {2, false, new Lead(new SortedSet<int> {5, 6, 7, 8}, 
                        new SortedSet<int> {1},Constants.CurrentDirection.SINK),
                    new SortedSet<int> {1}, false}
            };
        }

        // Test for GetConnectedContacts
        [Theory]
        [MemberData(nameof(GetConnectedContactsTestData))]
        public void GetConnectedContacts_ShouldReturnExpectedResults(int id, bool searchIsAContact, 
            SortedSet<int> contactSet, SortedSet<int> expectedContacts, bool expectedResult)
        {
            // Arrange
            var lead = new Lead(contactSet, new SortedSet<int> { 3, 4, 5 }, Constants.CurrentDirection.SINK);

            // Act
            var result = lead.GetConnectedContacts(id, searchIsAContact, out var connectedContacts);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedContacts, connectedContacts);
        }

        public static IEnumerable<object[]> GetConnectedContactsTestData()
        {
            return new List<object[]>
            {
                new object[] { 1, true, new SortedSet<int> { 1, 2, 3 }, new SortedSet<int> { 2, 3 }, true },
                new object[] { 4, false, new SortedSet<int> { 1, 2 }, new SortedSet<int> { 1, 2 }, false },
                new object[] { 2, false, new SortedSet<int> { 1, 2, 3 }, new SortedSet<int> { 1, 3 }, true }
            };
        }

        // Test for IsFullyIndependent
        [Theory]
        [MemberData(nameof(IsFullyIndependentTestData))]
        public void IsFullyIndependent_ShouldReturnExpectedResults(SortedSet<int> contacts1, 
            SortedSet<int> outputs1, SortedSet<int> contacts2, SortedSet<int> outputs2, bool expectedResult)
        {
            // Arrange
            var lead1 = new Lead(contacts1, outputs1, Constants.CurrentDirection.SOURCE);
            var lead2 = new Lead(contacts2, outputs2, Constants.CurrentDirection.SINK);

            // Act
            var result = lead1.IsFullyIndependent(lead2);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> IsFullyIndependentTestData()
        {
            return new List<object[]>
            {
                new object[] 
                {
                    new SortedSet<int> { 1, 2 }, 
                    new SortedSet<int> { 3 }, 
                    new SortedSet<int> { 4, 5 }, 
                    new SortedSet<int> { 6 }, 
                    true 
                },
                new object[] 
                {
                    new SortedSet<int> { 1, 2 }, 
                    new SortedSet<int> { 3 }, 
                    new SortedSet<int> { 2, 4 }, 
                    new SortedSet<int> { 5 }, 
                    false 
                }
            };
        }

        // Test for IndependentLeadsExist
        [Theory]
        [MemberData(nameof(IndependentLeadsExistTestData))]
        public void IndependentLeadsExist_ShouldReturnExpectedResults(IEnumerable<Lead> leads, bool expectedResult)
        {
            // Act
            var result = Lead.IndependentLeadsExist(leads);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> IndependentLeadsExistTestData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    new List<Lead>
                    {
                        new Lead(new SortedSet<int> { 1 }, new SortedSet<int> { 2 }, Constants.CurrentDirection.SOURCE),
                        new Lead(new SortedSet<int> { 3 }, new SortedSet<int> { 4 }, Constants.CurrentDirection.SINK)
                    },
                    true
                },
                new object[]
                {
                    new List<Lead>
                    {
                        new Lead(new SortedSet<int> { 1 }, new SortedSet<int> { 2 }, Constants.CurrentDirection.SOURCE),
                        new Lead(new SortedSet<int> { 1 }, new SortedSet<int> { 4 }, Constants.CurrentDirection.SINK)
                    },
                    false
                }
            };
        }
    }
}
