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
    /// <param name="interfaceSpecificParams">Parameters specific to that interface</param>
    [Theory]
    [InlineData(typeof(ReusableIdPool), new object[]{0, 0, new int[0]})]
    [InlineData(typeof(int), new object[]{})]
    [InlineData(typeof(NeuralInterfaceHardware), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldThowError_WhenInvalidType(Type interfaceType, object[] interfaceSpecificParams) 
    {
        // Arrange
        var NIManager = new NeuralInterfaceManager();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out _));

    }

    /// <summary>
    /// Given a valid Type, the manager assigns the next available ID to the new neural interface
    /// </summary>
    /// <param name="interfaceType">The type of the hardware interface, for example: ContactGroup, GelPad</param>
    /// <param name="interfaceSpecificParams">Parameters specific to that interface</param>
    [Theory]
    [InlineData(typeof(ContactGroup), new object[]{1})]
    [InlineData(typeof(GelPad), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldInit_WhenValidType(Type interfaceType, object[] interfaceSpecificParams) 
    {
        // Arrange
        var NIManager = new NeuralInterfaceManager();

        // Act
        var result = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out int globalInterfaceId);
        
        // Assert
        Assert.NotNull(result);

        // That Id is stored in the out parameter
        Assert.True(NIManager.IsValidResourceId(globalInterfaceId));

        // That the neural interface object created can later be looked up via the GetNeuralInterface method
        Assert.True(NIManager.TryGetNeuralInterface(globalInterfaceId, out _));
    }

    /// <summary>
    /// The new neural interface object of the correct Type is created successfully with the given object[] parameters.
    /// Test with classes that takes additional parameters.
    /// </summary>
    /// <param name="interfaceType">The type of the hardware interface, for example: ContactGroup, GelPad</param>
    /// <param name="interfaceSpecificParams">Parameters specific to that interface</param>
    [Theory]
    [InlineData(typeof(ContactGroup), new object[]{1})]
    [InlineData(typeof(ContactGroup), new object[]{2})]
    [InlineData(typeof(ContactGroup), new object[]{3})]
    public void CreateAndRegisterNeuralInterface_ShouldInitNeuralInterfaceWithCorrectParams_WhenValidType_WithAdditionalParams(Type interfaceType, object[] interfaceSpecificParams) 
    {
        // Arrange
        var NIManager = new NeuralInterfaceManager();

        // Act
        var result = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out int globalInterfaceId);
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
    /// <param name="interfaceType">The type of the hardware interface, for example: ContactGroup, GelPad</param>
    /// <param name="interfaceSpecificParams">Parameters specific to that interface</param>
    [Theory]
    [InlineData(typeof(GelPad), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldInitWithCorrectParams_WhenValidType_NoAdditionalParams(Type interfaceType, object[] interfaceSpecificParams) 
    {
        // Arrange
        var NIManager = new NeuralInterfaceManager();

        // Act
        var result = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out int globalInterfaceId);
        NIManager.TryGetNeuralInterface(globalInterfaceId, out NeuralInterfaceHardware neuralInterface);

        // Assert
        // That the new neural interface object is successfully created
        Assert.NotNull(neuralInterface);

        // That the new neural interface object is of the correct Type
        Assert.Equal(interfaceType, neuralInterface.GetType());

        // That the new neural interface object is created with the given object[] parameters, in this case, none.
        if (interfaceType == typeof(GelPad) && neuralInterface is GelPad gelPad) {
            Assert.Equal(new GelPad().NumContacts, gelPad.NumContacts); 
        }
    }

    /// <summary>
    /// The manager assigns global contact IDs for the new neural interface. 
    /// Test that this ID is returned by the method CreateAndRegisterNeuralInterface.
    /// </summary>
    /// <param name="interfaceType">The type of the hardware interface, for example: ContactGroup, GelPad</param>
    /// <param name="interfaceSpecificParams">Parameters specific to that interface</param>
    [Theory]
    [InlineData(typeof(ContactGroup), new object[]{1})]
    [InlineData(typeof(GelPad), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldReturnAssignedGlobalContactIDs(Type interfaceType, object[] interfaceSpecificParams)
    {
        // Arrange
        var NIManager = new NeuralInterfaceManager();

        // Act
        var contactIds = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out _);

        // Assert
        Assert.NotEmpty(contactIds);
        Assert.All(contactIds, id => Assert.True(NIManager.IsValidContactId(id)));
    }

    /// <summary>
    /// The manager assigns global contact IDs for the new neural interface. 
    /// Test that this contact assignment upholds global contact ID uniqueness when the interface is the first one added.
    /// </summary>
    /// <param name="interfaceType">The type of the hardware interface, for example: ContactGroup, GelPad</param>
    /// <param name="interfaceSpecificParams">Parameters specific to that interface</param>
    [Theory]
    [InlineData(typeof(ContactGroup), new object[]{1})]
    [InlineData(typeof(GelPad), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldAssignUniqueGlobalContactIDs_WhenFirstInterface(Type interfaceType, object[] interfaceSpecificParams)
    {
        // Arrange
        var NIManager = new NeuralInterfaceManager();

        // Act
        var contactIds = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out _);

        // Assert
        Assert.Equal(contactIds, contactIds.Distinct());
    }

    /// <summary>
    /// The manager assigns global contact IDs for the new neural interface. 
    /// Test that this contact assignment upholds global contact ID uniqueness when the interface is not the first one added.
    /// </summary>
    /// <param name="interfaceType">The type of the hardware interface, for example: ContactGroup, GelPad</param>
    /// <param name="interfaceSpecificParams">Parameters specific to that interface</param>
    [Theory]
    [InlineData(typeof(ContactGroup), new object[]{1})]
    [InlineData(typeof(GelPad), new object[]{})]
    public void CreateAndRegisterNeuralInterface_ShouldAssignUniqueGlobalContactIDs_WhenNotFirstInterface(Type interfaceType, object[] interfaceSpecificParams)
    {
        // Arrange
        var NIManager = new NeuralInterfaceManager();

        // Act
        var contactIds1 = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out _);
        var contactIds2 = NIManager.CreateAndRegisterNeuralInterface(interfaceType, interfaceSpecificParams, out _);

        // Assert
        var allContactIds = contactIds1.Union(contactIds2).ToList();
        Assert.Equal(allContactIds.Count, allContactIds.Distinct().Count());
    }

    /// <summary>
    /// The manager assigns global contact IDs for the new neural interface. 
    /// Test that the correct number of contact IDs was assigned.
    /// </summary>
    /// <param name="numContacts">Number of contacts for the Neural Interface</param>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void CreateAndRegisterNeuralInterface_ShouldAssignCorrectNumberOfContactIDs(int numContacts)
    {
        // Arrange
        var NIManager = new NeuralInterfaceManager();
        Type interfaceType = typeof(ContactGroup);
        object[] parameters = new object[] { numContacts };

        // Act
        var contactIds = NIManager.CreateAndRegisterNeuralInterface(interfaceType, parameters, out _);

        // Assert
        Assert.Equal(numContacts, contactIds.Count);
    }

}