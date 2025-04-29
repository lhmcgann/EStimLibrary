using EStimLibrary.Extensions.HardwareInterfaces;


namespace EStimLibrary.UnitTests.Extensions.HardwareInterfaces;


/// <summary>
/// A test class for the GelPad INeuralInterfaceHardware class.
/// </summary>
public class GelPadTests
{
    /// <summary>
    /// Make sure ID is -1 after constructor (ID is only set once added to a
    /// session). 
    /// Number of contacts for gel pad should be 1.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitWithCorrectParams()
    {
        // Arrange (Init new object)
        var gelPad = new GelPad();

        // Assert (Test if initialized params are correct)
        Assert.Equal(-1, gelPad.Id);
        Assert.Equal(1, gelPad.NumContacts);

    }
}
