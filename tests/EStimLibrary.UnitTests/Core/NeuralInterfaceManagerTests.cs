using EStimLibrary.Core;
using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Extensions.HardwareInterfaces;


namespace EStimLibrary.UnitTests.Core;


public class NeuralInterfaceManagerTests
{
    /// <summary>
    /// An error is thrown when the Type is invalid, i.e. types are not concrete derived class of NeuralInterfaceHardware.
    /// </summary>
    /// <param name="interfaceType">The type of the hardware interface, for example: ContactGroup, GelPad</param>
    /// <param name="interfaceSpecificParams">Parameters for the specific type of interface</param>
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
    /// <param name="interfaceType">The type of the hardware interface, for example: ContactGroup, GelPad</param>
    /// <param name="interfaceSpecificParams">Parameters for the specific type of interface</param>
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
    /// <summary>
    /// The new neural interface object of the correct Type is created successfully with the given object[] parameters.
    /// Test with classes that takes additional parameters.
    /// </summary>
    /// <param name="interfaceType"></param>
    /// <param name="interfaceSpecificParams"></param>
    [Theory]
    [InlineData(typeof(ContactGroup), new object[]{1})]
    [InlineData(typeof(ContactGroup), new object[]{2})]
    [InlineData(typeof(ContactGroup), new object[]{3})]
    public void CreateAndRegisterNeuralInterface_ShouldInitNeuralInterfaceWithCorrectParams_WhenValidType_WithAdditionalParams(Type interfaceType, object[] interfaceSpecificParams) {
        // Arrange
        var NIManager = new NeuralInterfaceManager();
        int globalInterfaceId;

        // Act
        var result = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out globalInterfaceId);
        NIManager.TryGetNeuralInterface(globalInterfaceId, out NeuralInterfaceHardware neuralInterface);

        // Assert
        // That the new neural interface object is successfully created
        Assert.NotNull(neuralInterface);

        // That the new neural interface object is of the correct Type
        Assert.Equal(interfaceType, neuralInterface.GetType());

        // That the new neural interface object is created with the given object[] parameters.
        if (interfaceType == typeof(ContactGroup) && neuralInterface is ContactGroup contactGroup) {
            Assert.Equal(interfaceSpecificParams[0], contactGroup.NumContacts); 
        }
    }

    /// <summary>
    /// The new neural interface object of the correct Type is created successfully with the given object[] parameters.
    /// Test with classes that requires no additional parameters.
    /// </summary>
    /// <param name="interfaceType"></param>
    /// <param name="interfaceSpecificParams"></param>
    [Theory]
    [InlineData(typeof(GelPad), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldInitWithCorrectParams_WhenValidType_NoAdditionalParams(Type interfaceType, object[] interfaceSpecificParams) {
        // Arrange
        var NIManager = new NeuralInterfaceManager();
        int globalInterfaceId;

        // Act
        var result = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out globalInterfaceId);
        NIManager.TryGetNeuralInterface(globalInterfaceId, out NeuralInterfaceHardware neuralInterface);

        // Assert
        // That the new neural interface object is successfully created
        Assert.NotNull(neuralInterface);

        // That the new neural interface object is of the correct Type
        Assert.Equal(interfaceType, neuralInterface.GetType());

        // That the new neural interface object is created with the given object[] parameters, in this case, none.
        if (interfaceType == typeof(GelPad) && neuralInterface is GelPad gelPad) {
            Assert.Equal(new GelPad(), gelPad); 
        }
    }
}