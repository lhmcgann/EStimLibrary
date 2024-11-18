using EStimLibrary.Extensions.HardwareInterfaces;


namespace EStimLibrary.UnitTests.Extensions.HardwareInterfaces;


/// <summary>
/// A test class for the ContactGroup INeuralInterfaceHardware class.
/// </summary>
public class ContactGroupTests
{
    /// <summary>
    /// Test the parameterized constructor with different valid parameters,
    /// i.e., positive integer values for number of contacts. The neural
    /// interface object should initialize with correct NumContact and Id
    /// values.
    /// </summary>
    [Theory]
    [InlineData(1)]     // Valid single contact.
    [InlineData(10)]    // Valid multiple contacts.
    public void Constructor_ShouldInit(int numContacts)
    {
        var contactGroup = new ContactGroup(numContacts);

        Assert.Equal(-1, contactGroup.Id);
        Assert.Equal(numContacts, contactGroup.NumContacts);
    }

    /// <summary>
    /// Test the parameterized constructor with different invalid parameters,
    /// i.e., 0 or negative integer values for number of contacts.
    /// Constructioin should fail and throw an argument exception.
    /// </summary>
    [Theory]
    [InlineData(0)]     // Invalid 0 contacts.
    [InlineData(-1)]    // Invalid single negative contact number.
    [InlineData(-10)]   // Invalid multiple negative contact number.
    public void Constructor_ShouldThrowException(int numContacts)
    {
        Assert.Throws<ArgumentException>(() => new ContactGroup(numContacts));
    }
}
