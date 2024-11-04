using System.Runtime.InteropServices;
using EStimLibrary.Core;
using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Extensions.HardwareInterfaces;

namespace EStimLibrary.UnitTests.Core;


public class NeuralInterfaceManagerTests
{
    /// <summary>
    /// An error is thrown when the Type is invalid, i.e. types are not concrete derived class of NeuralInterfaceHardware.
    /// </summary>
    /// <param name="interfaceType"></param>
    /// <param name="interfaceSpecificParams"></param>
    [Theory]
    [InlineData(typeof(ReusableIdPool), new object[]{0, 0, new int[0]})]
    [InlineData(typeof(int), new object[]{})]
    [InlineData(typeof(NeuralInterfaceHardware), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldThowError_WhenInvalidType(Type interfaceType, object[] interfaceSpecificParams) {
        // Arrange
        var NIManager = new NeuralInterfaceManager();
        int globalInterfaceId;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out globalInterfaceId));

    }

    /// <summary>
    /// Given a valid Type, the manager assigns the next available ID to the new neural interface
    /// </summary>
    /// <param name="interfaceType"></param>
    /// <param name="interfaceSpecificParams"></param>
    [Theory]
    [InlineData(typeof(ContactGroup), new object[]{1})]
    [InlineData(typeof(GelPad), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldInit_WhenValidType(Type interfaceType, object[] interfaceSpecificParams) {
        // Arrange
        var NIManager = new NeuralInterfaceManager();
        int globalInterfaceId;

        // Act
        var result = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out globalInterfaceId);
        
        // Assert
        Assert.NotNull(result);

        // Id is stored in the out parameter
        Assert.True(NIManager.IsValidResourceId(globalInterfaceId));

        // The neural interface object created can later be looked up via the GetNeuralInterface method (ID given must be valid, i.e., previously output by the manager)
        Assert.True(NIManager.TryGetNeuralInterface(globalInterfaceId, out NeuralInterfaceHardware neuralInterface));
    }
}