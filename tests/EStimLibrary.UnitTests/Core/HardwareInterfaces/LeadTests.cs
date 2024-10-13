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
            SortedSet<int> outputSet, SortedSet<int> expectedOutputs, bool expectedResult)
        {
            // Arrange
            var lead = new Lead(new SortedSet<int> { 1, 2 }, outputSet, Constants.CurrentDirection.SOURCE);

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
                new object[] { 3, true, new SortedSet<int> { 3, 4, 5 }, new SortedSet<int> { 4, 5 }, true },
                new object[] { 6, true, new SortedSet<int> { 3, 4, 5 }, new SortedSet<int> { 3, 4, 5 }, false },
                new object[] { 1, false, new SortedSet<int> { 3, 4, 5 }, new SortedSet<int> { 3, 4, 5 }, false }
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
