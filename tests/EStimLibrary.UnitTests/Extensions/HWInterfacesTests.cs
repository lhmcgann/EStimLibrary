using System.ComponentModel.DataAnnotations;
using EStimLibrary.Extensions.HardwareInterfaces;

namespace EStimLibrary.UnitTests.Extensions.HardwareInterfaces 
{
    public class GelPadTests
    {
        /// <summary>
        ///  Make sure ID is -1 after constructor (ID is only set once added to a session). 
        ///  Number of contact for gel pad should be 1.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitWithCorrectParams() 
        {
            //Arrange (Init new object)
            var gelPad = new GelPad();

            //Assert (Test if initialized params are correct)
            Assert.Equal(-1, gelPad.Id);
            Assert.Equal(1, gelPad.NumContacts);

        }
    }

    public class ContactGroupTests
    {
        /// <summary>
        /// Test the parameterized constructor with different data values.
        /// </summary>
        [Theory]
        // Test valid (positive) values for num contacts.
        [InlineData(1)]
        [InlineData(10)]
        public void Constructor_ShouldInit(int numContacts) {
            var contactGroup = new ContactGroup(numContacts);

            Assert.Equal(numContacts, contactGroup.NumContacts);
        }
        
        [Theory]
        // Test invalid parameter values, expect object to fail to be created 
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]

        public void Constructor_ShouldNotInit(int numContacts) {
            // Init a new object
            var contactGroup = new ContactGroup(numContacts);

            // contactGroup should fail to be created
            Assert.Null(contactGroup);
        }
    }
}
