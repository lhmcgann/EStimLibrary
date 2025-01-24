using EStimLibrary.Core;

namespace EStimLibrary.UnitTests.Core;

/// <summary>
/// Unit tests for the utility methods in the Utils class.
/// </summary>
public class UtilsTests
{
    /// <summary>
    /// Tests that GetAvailableTypes returns types assignable from the specified base type.
    /// </summary>
    [Fact]
    public void GetAvailableTypes_ShouldReturnTypes_WhenTypeExists()
    {
        // Arrange & Act: Get available types assignable to ISelectable
        var result = Utils.GetAvailableTypes(typeof(ISelectable));

        // Assert: Result is not null and all values are assignable from ISelectable
        Assert.NotNull(result);
        Assert.True(result.Values.All(
            t => typeof(ISelectable).IsAssignableFrom(t)));
    }

    /// <summary>
    /// Tests the generic overload of GetAvailableTypes returns types assignable from the specified base type.
    /// </summary>
    [Fact]
    public void GetAvailableTypes_Generic_ShouldReturnTypes_WhenTypeExists()
    {
        // Arrange & Act: Get available generic types assignable to ISelectable
        var result = Utils.GetAvailableTypes<ISelectable>();

        // Assert: Result is not null and all values are assignable from ISelectable
        Assert.NotNull(result);
        Assert.True(result.Values.All(
            t => typeof(ISelectable).IsAssignableFrom(t)));
    }

    /// <summary>
    /// Tests that IsAssignableFromType returns the expected result for given base and derived types.
    /// </summary>
    [Theory]
    [InlineData(typeof(BaseClass), typeof(DerivedClass), true)]
    [InlineData(typeof(BaseClass), typeof(UnrelatedClass), false)]
    public void IsAssignableFromType_ShouldReturnExpectedResult(Type baseType,
        Type derivedType, bool expected)
    {
        // Act: Check if the derivedType is assignable to baseType
        var result = Utils.IsAssignableFromType(baseType, derivedType);

        // Assert: Result matches the expected value
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that IsGenericAssignableFrom returns true for compatible generic types.
    /// </summary>
    [Fact]
    public void IsGenericAssignableFrom_ShouldReturnTrue_ForCompatibleGenericTypes()
    {
        // Act: Check compatibility between List<> and List<int>
        var result = Utils.IsGenericAssignableFrom(typeof(List<>),
            typeof(List<int>));

        // Assert: Result is true for compatible types
        Assert.True(result);
    }

    /// <summary>
    /// Tests that IsGenericAssignableFrom returns false for incompatible generic types.
    /// </summary>
    [Fact]
    public void IsGenericAssignableFrom_ShouldReturnFalse_ForIncompatibleGenericTypes()
    {
        // Act: Check compatibility between List<> and Dictionary<int, int>
        var result = Utils.IsGenericAssignableFrom(typeof(List<>),
            typeof(Dictionary<int, int>));

        // Assert: Result is false for incompatible types
        Assert.False(result);
    }

    /// <summary>
    /// Tests that GetAvailableGenericTypes returns types with specified generic parameters.
    /// </summary>
    [Fact]
    public void GetAvailableGenericTypes_ShouldReturnTypes_WithSpecifiedGenericParameters()
    {
        // Act: Get available generic types with specific parameters
        var result = Utils.GetAvailableGenericTypes(typeof(IFactory<>),
            new Type[] { typeof(SampleProduct) });

        // Assert: Result is not null and all types are assignable from IFactory<SampleProduct>
        Assert.NotNull(result);
        Assert.All(result.Values, type => Assert.True(
            typeof(IFactory<SampleProduct>).IsAssignableFrom(type)));
    }

    /// <summary>
    /// Tests that AreTypeParametersCompatible returns true for matching type parameters.
    /// </summary>
    [Fact]
    public void AreTypeParametersCompatible_ShouldReturnTrue_ForMatchingTypeParameters()
    {
        // Act: Check compatibility between List<int> and List<int>
        var result = Utils.AreTypeParametersCompatible(typeof(List<int>),
            typeof(List<int>));

        // Assert: Result is true for matching parameters
        Assert.True(result);
    }

    /// <summary>
    /// Tests that AreTypeParametersCompatible returns false for non-matching type parameters.
    /// </summary>
    [Fact]
    public void AreTypeParametersCompatible_ShouldReturnFalse_ForNonMatchingTypeParameters()
    {
        // Act: Check compatibility between List<int> and List<string>
        var result = Utils.AreTypeParametersCompatible(typeof(List<int>),
            typeof(List<string>));

        // Assert: Result is false for non-matching parameters
        Assert.False(result);
    }

    /// <summary>
    /// Tests that GetConstructorParamInfo returns the parameter information for a valid type.
    /// </summary>
    [Fact]
    public void GetConstructorParamInfo_ShouldReturnParameterInfo_ForValidType()
    {
        // Act: Retrieve constructor parameter information for SampleClass
        var (paramNames, paramTypes) = Utils.GetConstructorParamInfo(typeof(SampleClass));

        // Assert: Parameter names and types match expected values
        Assert.Contains("param1", paramNames);
        Assert.Equal(typeof(int), paramTypes["param1"]);
    }

    /// <summary>
    /// Tests that CreateObjectOfType creates an object with valid parameters.
    /// </summary>
    [Fact]
    public void CreateObjectOfType_ShouldCreateObject_ForValidParameters()
    {
        // Arrange: Set up parameters for object creation
        var parameters = new Dictionary<string, object> { { "param1", 5 } };

        // Act: Create an object of SampleClass with parameters
        var result = Utils.CreateObjectOfType(typeof(SampleClass), new List<string> { "param1" }, parameters);

        // Assert: Result is of type SampleClass
        Assert.IsType<SampleClass>(result);
    }

    /// <summary>
    /// Tests that GetObjectProperty returns the value of a valid property.
    /// </summary>
    [Fact]
    public void GetObjectProperty_ShouldReturnPropertyValue_ForValidProperty()
    {
        // Arrange: Create an instance of SampleClass
        var instance = new SampleClass(5);

        // Act: Retrieve the value of the property Param1
        Utils.GetObjectProperty(instance, typeof(SampleClass), "Param1", out var propertyValue);

        // Assert: Property value matches expected
        Assert.Equal(5, propertyValue);
    }

    /// <summary>
    /// Tests that CallObjectMethod invokes a method and returns the correct result.
    /// </summary>
    [Fact]
    public void CallObjectMethod_ShouldInvokeMethodAndReturnResult()
    {
        // Arrange: Create an instance of SampleClass
        var instance = new SampleClass();

        // Act: Invoke SampleMethod with arguments and retrieve result
        Utils.CallObjectMethod(instance, typeof(SampleClass), "SampleMethod", new object[] { 3, 2 }, out var result);

        // Assert: Result matches expected value
        Assert.Equal(5, result);
    }

    // Nested mock classes for testing purposes

    /// <summary>
    /// Base class used for type compatibility tests.
    /// </summary>
    public class BaseClass { }

    /// <summary>
    /// Derived class used for type compatibility tests.
    /// </summary>
    public class DerivedClass : BaseClass { }

    /// <summary>
    /// Unrelated class used for type compatibility tests.
    /// </summary>
    public class UnrelatedClass { }

    /// <summary>
    /// Sample class with a property and method for testing purposes.
    /// </summary>
    public class SampleClass
    {
        public int Param1 { get; private set; }

        public SampleClass(int param1 = 0)
        {
            Param1 = param1;
        }

        public int SampleMethod(int a, int b) => a + b;
    }

    /// <summary>
    /// Factory interface for generic type testing.
    /// </summary>
    public interface IFactory<T> { }

    /// <summary>
    /// Sample product class used for generic type testing.
    /// </summary>
    public class SampleProduct { }
}
