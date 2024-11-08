using EStimLibrary.Extensions.HardwareInterfaces;

namespace EStimLibrary.UnitTests.Extensions.HardwareInterfaces; 

public class ContactGroupTests
{
    /// <summary>
    /// Test the parameterized constructor with different data values. Including:
    /// Valid (positive) values for number of contacts.
    /// Invalid values, expect object to fail to be created and error thrown.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Constructor_ShouldInit(int numContacts) {
        var contactGroup = new ContactGroup(numContacts);

        Assert.Equal(numContacts, contactGroup.NumContacts);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_ShouldThrowException(int numContacts) {
        Assert.Throws<ArgumentException>(() => new ContactGroup(numContacts));
    }
}
