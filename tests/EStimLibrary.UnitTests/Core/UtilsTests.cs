using EStimLibrary.Core;


namespace EStimLibrary.UnitTests.Core;


public class UtilsTests
{
    [Fact]
    public void GetAvailableTypes_ShouldReturnTypes_WhenTypeExists()
    {
        var result = Utils.GetAvailableTypes(typeof(ISelectable));
        Assert.NotNull(result);
        Assert.True(result.Values.All(
            t => typeof(ISelectable).IsAssignableFrom(t)));
    }

    [Fact]
    public void GetAvailableTypes_Generic_ShouldReturnTypes_WhenTypeExists()
    {
        var result = Utils.GetAvailableTypes<ISelectable>();
        Assert.NotNull(result);
        Assert.True(result.Values.All(
            t => typeof(ISelectable).IsAssignableFrom(t)));
    }

    [Theory]
    [InlineData(typeof(BaseClass), typeof(DerivedClass), true)]
    [InlineData(typeof(BaseClass), typeof(UnrelatedClass), false)]
    public void IsAssignableFromType_ShouldReturnExpectedResult(Type baseType,
        Type derivedType, bool expected)
    {
        var result = Utils.IsAssignableFromType(baseType, derivedType);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void
        IsGenericAssignableFrom_ShouldReturnTrue_ForCompatibleGenericTypes()
    {
        var result = Utils.IsGenericAssignableFrom(typeof(List<>),
            typeof(List<int>));
        Assert.True(result);
    }

    [Fact]
    public void
        IsGenericAssignableFrom_ShouldReturnFalse_ForIncompatibleGenericTypes()
    {
        var result = Utils.IsGenericAssignableFrom(typeof(List<>),
            typeof(Dictionary<int, int>));
        Assert.False(result);
    }

    [Fact]
    public void
        GetAvailableGenericTypes_ShouldReturnTypes_WithSpecifiedGenericParameters()
    {
        var result = Utils.GetAvailableGenericTypes(typeof(IFactory<>),
            new Type[] { typeof(SampleProduct) });
        Assert.NotNull(result);
        Assert.All(result.Values, type => Assert.True(
            typeof(IFactory<SampleProduct>).IsAssignableFrom(type)));
    }

    [Fact]
    public void
        AreTypeParametersCompatible_ShouldReturnTrue_ForMatchingTypeParameters()
    {
        var result = Utils.AreTypeParametersCompatible(typeof(List<int>),
            typeof(List<int>));
        Assert.True(result);
    }

    [Fact]
    public void
        AreTypeParametersCompatible_ShouldReturnFalse_ForNonMatchingTypeParameters()
    {
        var result = Utils.AreTypeParametersCompatible(typeof(List<int>),
            typeof(List<string>));
        Assert.False(result);
    }

    [Fact]
    public void GetConstructorParamInfo_ShouldReturnParameterInfo_ForValidType()
    {
        var (paramNames, paramTypes) = Utils.GetConstructorParamInfo(typeof(SampleClass));
        Assert.Contains("param1", paramNames);
        Assert.Equal(typeof(int), paramTypes["param1"]);
    }

    [Fact]
    public void CreateObjectOfType_ShouldCreateObject_ForValidParameters()
    {
        var parameters = new Dictionary<string, object> { { "param1", 5 } };
        var result = Utils.CreateObjectOfType(typeof(SampleClass), new List<string> { "param1" }, parameters);
        Assert.IsType<SampleClass>(result);
    }

    [Fact]
    public void GetObjectProperty_ShouldReturnPropertyValue_ForValidProperty()
    {
        var instance = new SampleClass(5);
        Utils.GetObjectProperty(instance, typeof(SampleClass), "Param1", out var propertyValue);
        Assert.Equal(5, propertyValue);
    }

    [Fact]
    public void CallObjectMethod_ShouldInvokeMethodAndReturnResult()
    {
        var instance = new SampleClass();
        Utils.CallObjectMethod(instance, typeof(SampleClass), "SampleMethod", new object[] { 3, 2 }, out var result);
        Assert.Equal(5, result);
    }

    // Nested mock classes for testing purposes
    public class BaseClass { }
    public class DerivedClass : BaseClass { }
    public class UnrelatedClass { }

    public class SampleClass
    {
        public int Param1 { get; private set; }

        public SampleClass(int param1 = 0)
        {
            Param1 = param1;
        }

        public int SampleMethod(int a, int b) => a + b;
    }

    public interface IFactory<T> { }
    public class SampleProduct { }
}
