using EStimLibrary.Core;
using EStimLibrary.Core.Data;
using EStimLibrary.Core.Haptics;
using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Core.Stimulation.Stimulators;
using EStimLibrary.Extensions.Data;
using EStimLibrary.Extensions.Haptics;
using EStimLibrary.Extensions.HardwareInterfaces;
using EStimLibrary.Extensions.SpatialModel.StringHierarchy;
using EStimLibrary.Extensions.Stimulation.Stimulators;


namespace EStimLibrary.UnitTests.Core;


/// <summary>
/// Comprehensive unit tests for the <see cref="Utils"/> class.
/// These tests cover every method inside Utils.cs,
/// including math helpers, reflection‐based functions,
/// file I/O, and interactive input functions.
/// </summary>
public class UtilsTests
{
    private readonly ITestOutputHelper _output;

    // Test class constructor creates an output helper so can write console
    // output.
    public UtilsTests(ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    #region Math Helper Functions Tests

    /// <summary>
    /// Tests that <see cref="Utils.IsWithinUpperBound(double, double, bool)"/>
    /// returns true when the upper bound is infinite.
    /// </summary>
    [Fact]
    public void IsWithinUpperBound_WhenUpperBoundIsInfinite_ShouldReturnTrue()
    {
        // Assuming Constants.POS_INFINITY is defined in the library.
        bool result = Utils.IsWithinUpperBound(100, Constants.POS_INFINITY);
        Assert.True(result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsWithinUpperBound(double, double, bool)"/> 
    /// handles inclusive and exclusive comparisons correctly.
    /// </summary>
    [Fact]
    public void IsWithinUpperBound_WhenCalledWithInclusiveAndExclusiveComparisons_ShouldReturnExpectedResults()
    {
        Assert.True(Utils.IsWithinUpperBound(10, 10, inclusive: true));
        Assert.False(Utils.IsWithinUpperBound(10, 10, inclusive: false));
        Assert.True(Utils.IsWithinUpperBound(9, 10, inclusive: false));
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsWithinLowerBound(double, double, bool)"/>
    /// returns true when the lower bound is infinite.
    /// </summary>
    [Fact]
    public void IsWithinLowerBound_WhenLowerBoundIsInfinite_ShouldReturnTrue()
    {
        bool result = Utils.IsWithinLowerBound(-100, Constants.NEG_INFINITY);
        Assert.True(result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsWithinLowerBound(double, double, bool)"/>
    /// handles inclusive and exclusive comparisons correctly.
    /// </summary>
    [Fact]
    public void IsWithinLowerBound_WhenCalledWithInclusiveAndExclusiveComparisons_ShouldReturnExpectedResults()
    {
        Assert.True(Utils.IsWithinLowerBound(10, 10, inclusive: true));
        Assert.False(Utils.IsWithinLowerBound(10, 10, inclusive: false));
        Assert.True(Utils.IsWithinLowerBound(11, 10, inclusive: false));
    }

    /// <summary>
    /// Tests that <see cref="Utils.ScaleValue(double, double, double)"/> 
    /// scales a normalized value correctly into the target range.
    /// </summary>
    [Fact]
    public void ScaleValue_WhenScalingNormalizedValue_ShouldReturnValueInTargetRange()
    {
        double result = Utils.ScaleValue(0.5, 0, 100);
        Assert.Equal(50, result, precision: 5);
    }

    #endregion

    #region Reflection Functions Tests

    /// <summary>
    /// Tests that <see cref="Utils.GetAvailableTypes(Type)"/>
    /// returns types that implement <see cref="ISelectable"/>.
    /// </summary>
    [Fact]
    public void GetAvailableTypes_ISelectable_ShouldFindLibraryTypes()
    {
        var result = Utils.GetAvailableTypes(typeof(ISelectable));
        // Some result found
        Assert.NotNull(result);
        // Can convert all values to ISelectable
        Assert.True(result.Values.All(t => typeof(ISelectable).IsAssignableFrom(t)));

        Dictionary<string, Type> someKnownConcreteDerivedTypes = new()
        {
            { "ContactGroup", typeof(ContactGroup) },
            { "ClassicDirectTransducer", typeof(ClassicDirectTransducer) },
            { "FixedOptionDataLimits`1", typeof(FixedOptionDataLimits<>) },
            { "StringHierarchyArea", typeof(StringHierarchyArea) }
        };

        // DEBUG
        // foreach (var kvp in result)
        // {
        //     this._output.WriteLine($"{kvp.Key}: {kvp.Value}");
        // }

        // Assert each key-value pair in expected exists in actual
        Assert.All(someKnownConcreteDerivedTypes, kvp =>
        {
            Assert.True(result.TryGetValue(kvp.Key, out var actualValue), 
                        $"Key '{kvp.Key}' not found in actual dictionary.");
            Assert.Equal(kvp.Value, actualValue);
        });
    }

    /// <summary>
    /// Tests that <see cref="Utils.GetAvailableTypes(Type)"/>
    /// returns types that implement <see cref="Stimulator"/>.
    /// </summary>
    [Fact]
    public void GetAvailableTypes_Stimulator_ShouldFindTwo()
    {
        var result = Utils.GetAvailableTypes(typeof(Stimulator));
        // Some result found
        Assert.NotNull(result);
        // Can convert all values to ISelectable
        Assert.True(result.Values.All(t => typeof(ISelectable).IsAssignableFrom(t)));

        Dictionary<string, Type> knownTypes = new()
        {
            { "EchoStimulator", typeof(EchoStimulator) }
        };

        // Assert each key-value pair in expected exists in actual
        Assert.All(knownTypes, kvp =>
        {
            Assert.True(result.TryGetValue(kvp.Key, out var actualValue), 
                        $"Key '{kvp.Key}' not found in actual dictionary.");
            Assert.Equal(kvp.Value, actualValue);
        });

        // Shouldn't find any more types
        Assert.Equal(knownTypes.Count, result.Count);
    }

    /// <summary>
    /// Tests that the generic overload <see cref="Utils.GetAvailableTypes{T}()"/>
    /// returns types that implement <see cref="ISelectable"/>.
    /// </summary>
    [Fact]
    public void GetAvailableTypes_Generic_WhenTypeExists_ShouldReturnTypes()
    {
        var result = Utils.GetAvailableTypes<ISelectable>();
        Assert.NotNull(result);
        Assert.True(result.Values.All(t => typeof(ISelectable).IsAssignableFrom(t)));
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsAssignableFromType(Type, Type)"/> 
    /// correctly determines assignability between non-generic base and derived
    /// types.
    /// </summary>
    [Theory]
    [InlineData(typeof(BaseClass), typeof(DerivedClass), true)]
    [InlineData(typeof(BaseClass), typeof(UnrelatedClass), false)]
    public void IsAssignableFromType_NonGenericSingleLevel_ShouldReturnExpectedResult(
        Type baseType, Type derivedType, bool expected)
    {
        var result = Utils.IsAssignableFromType(baseType, derivedType);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsAssignableFromType(Type, Type)"/> 
    /// correctly determines assignability between non-generic base and derived
    /// types.
    /// </summary>
    [Theory]
    // Success: generic inherited by generic
    [InlineData(typeof(BaseGenericClass<>), typeof(DerivedGenericClass<>), true)]
    // Success: generic inherited by non-generic (param filled)
    [InlineData(typeof(BaseGenericClass<>), typeof(DerivedStringParamClass), true)]
    // Success: generic interface inherited by generic interface
    [InlineData(typeof(IBaseGeneric<>), typeof(IDerivedGeneric<>), true)]
    // Success: generic interface secondarily inherited by generic
    [InlineData(typeof(IBaseGeneric<>), typeof(InterfaceDerivedGenericClass<>), true)]
    // Success: generic interface secondarily inherited by non-generic (param filled)
    [InlineData(typeof(IBaseGeneric<>), typeof(InterfaceDerivedStringParamClass), true)]
    // Fail: normal inherited by generic
    [InlineData(typeof(BaseClass), typeof(DerivedGenericClass<>), false)]
    // Fail: generic inherited by normal
    [InlineData(typeof(BaseGenericClass<>), typeof(DerivedClass), true)]
    // Fail: generic inherited by unrelated generic
    [InlineData(typeof(BaseGenericClass<>), typeof(UnrelatedGenericClass<>), false)]
    public void IsAssignableFromType_GenericMultilevel_ShouldReturnExpectedResult(
        Type baseType, Type derivedType, bool expected)
    {
        var result = Utils.IsAssignableFromType(baseType, derivedType);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsGenericAssignableFrom(Type, Type)"/> returns true when types are compatible.
    /// </summary>
    [Fact]
    public void IsGenericAssignableFrom_WhenTypesAreCompatible_ShouldReturnTrue()
    {
        var result = Utils.IsGenericAssignableFrom(typeof(List<>), typeof(List<int>));
        Assert.True(result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsGenericAssignableFrom(Type, Type)"/> returns false when types are incompatible.
    /// </summary>
    [Fact]
    public void IsGenericAssignableFrom_WhenTypesAreIncompatible_ShouldReturnFalse()
    {
        var result = Utils.IsGenericAssignableFrom(typeof(List<>), typeof(Dictionary<int, int>));
        Assert.False(result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.GetAvailableGenericTypes(Type, Type[])"/> returns generic types that use the specified parameters.
    /// </summary>
    [Fact]
    public void GetAvailableGenericTypes_WhenGivenGenericParameters_ShouldReturnTypes()
    {
        // Use the production IFactory<> interface and our test product.
        var result = Utils.GetAvailableGenericTypes(typeof(IFactory<>), new Type[] { typeof(TestProduct) });
        Assert.NotNull(result);
        Assert.All(result.Values, type =>
            Assert.True(typeof(IFactory<TestProduct>).IsAssignableFrom(type)));
    }

    /// <summary>
    /// Tests that <see cref="Utils.AreTypeParametersCompatible(Type, Type)"/> returns true when type parameters match.
    /// </summary>
    [Fact]
    public void AreTypeParametersCompatible_WhenTypeParametersMatch_ShouldReturnTrue()
    {
        var result = Utils.AreTypeParametersCompatible(typeof(List<int>), typeof(List<int>));
        Assert.True(result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.AreTypeParametersCompatible(Type, Type)"/> returns false when type parameters do not match.
    /// </summary>
    [Fact]
    public void AreTypeParametersCompatible_WhenTypeParametersDoNotMatch_ShouldReturnFalse()
    {
        var result = Utils.AreTypeParametersCompatible(typeof(List<int>), typeof(List<string>));
        Assert.False(result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.GetConstructorParamInfo(Type)"/> returns the correct parameter info when the type is valid.
    /// </summary>
    [Fact]
    public void GetConstructorParamInfo_WhenTypeIsValid_ShouldReturnParameterInfo()
    {
        var (paramNames, paramTypes) = Utils.GetConstructorParamInfo(typeof(SampleClass));
        Assert.Contains("param1", paramNames);
        Assert.Equal(typeof(int), paramTypes["param1"]);
    }

    /// <summary>
    /// Tests that <see cref="Utils.CreateObjectOfType(Type, List{string}, Dictionary{string, object})"/> creates an object when valid parameters are provided.
    /// </summary>
    [Fact]
    public void CreateObjectOfType_WhenGivenValidParameters_ShouldCreateObject()
    {
        var parameters = new Dictionary<string, object> { { "param1", 5 } };
        var result = Utils.CreateObjectOfType(typeof(SampleClass), new List<string> { "param1" }, parameters);
        Assert.IsType<SampleClass>(result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.GetObjectProperty(object, Type, string, out object)"/> returns the property value when the property exists.
    /// </summary>
    [Fact]
    public void GetObjectProperty_WhenPropertyExists_ShouldReturnValue()
    {
        var instance = new SampleClass(5);
        Utils.GetObjectProperty(instance, typeof(SampleClass), "Param1", out var propertyValue);
        Assert.Equal(5, propertyValue);
    }

    /// <summary>
    /// Tests that <see cref="Utils.CallObjectMethod(object, Type, string, object[], out object)"/> returns the correct result when a method is invoked.
    /// </summary>
    [Fact]
    public void CallObjectMethod_WhenMethodIsInvoked_ShouldReturnCorrectResult()
    {
        var instance = new SampleClass();
        Utils.CallObjectMethod(instance, typeof(SampleClass), "SampleMethod", new object[] { 3, 2 }, out var result);
        Assert.Equal(5, result);
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsGenericAssignableFrom(Type, Type)"/> returns false when the test generic type is open.
    /// </summary>
    [Fact]
    public void IsGenericAssignableFrom_WhenTestGenericTypeIsOpen_ShouldReturnFalse()
    {
        var result = Utils.IsGenericAssignableFrom(typeof(List<>), typeof(List<>));
        Assert.False(result);
    }

    #endregion

    #region Additional Reflection and Object Creation Coverage Tests

    /// <summary>
    /// Helper method that calls CreateObjectOfType with a type that has no matching constructor.
    /// </summary>
    private void CallCreateObjectOfType_NoMatchingConstructor()
    {
        Utils.CreateObjectOfType(
            typeof(NoMatchingConstructorClass),
            new List<string> { "param1" },
            new Dictionary<string, object> { { "param1", 5 } });
    }

    /// <summary>
    /// Helper method that calls GetConstructorParamInfo with a type that has no public constructor.
    /// </summary>
    private void CallGetConstructorParamInfo_NoPublicConstructor()
    {
        Utils.GetConstructorParamInfo(typeof(NoPublicConstructor));
    }

    /// <summary>
    /// Tests that CreateObjectOfType throws an ArgumentException when no matching constructor exists.
    /// </summary>
    [Fact]
    public void CreateObjectOfType_WhenNoMatchingConstructorExists_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(CallCreateObjectOfType_NoMatchingConstructor);
    }

    /// <summary>
    /// Tests that GetConstructorParamInfo throws an ArgumentException when no public constructor exists.
    /// </summary>
    [Fact]
    public void GetConstructorParamInfo_WhenNoPublicConstructorExists_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(CallGetConstructorParamInfo_NoPublicConstructor);
    }

    /// <summary>
    /// Tests that GetObjectProperty returns false and null when the property does not exist.
    /// </summary>
    [Fact]
    public void GetObjectProperty_WhenPropertyDoesNotExist_ShouldReturnFalseAndNull()
    {
        var instance = new SampleClass(5);
        bool found = Utils.GetObjectProperty(instance, typeof(SampleClass), "NonExistentProperty", out var value);
        Assert.False(found);
        Assert.Null(value);
    }

    /// <summary>
    /// Tests that CallObjectMethod returns false and null when the method is not found.
    /// </summary>
    [Fact]
    public void CallObjectMethod_WhenMethodDoesNotExist_ShouldReturnFalseAndNull()
    {
        var instance = new SampleClass();
        bool invoked = Utils.CallObjectMethod(instance, typeof(SampleClass), "NonExistentMethod", new object[] { 1, 2 }, out var result);
        Assert.False(invoked);
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that TryFactoryCreate returns false when the factory fails to create a product.
    /// </summary>
    [Fact]
    public void TryFactoryCreate_WhenInvalidParametersProvided_ShouldReturnFalse()
    {
        // Use a factory that always fails.
        //var factory = new FailingDummyFactory();
        IFactory<TestProduct> factory = new FailingDummyFactory();
        Func<string> readInput = () => "5"; // valid input according to the limits
        var outputs = new List<string>();
        Action<string> displayOutput = s => outputs.Add(s);

        bool success = Utils.TryFactoryCreate(factory, out TestProduct product, displayOutput, readInput);
        Assert.False(success);
        Assert.Null(product);
    }

    /// <summary>
    /// Tests that RequestUserInput loops on invalid input before returning a valid parsed value.
    /// </summary>
    [Fact]
    public void RequestUserInput_WhenFirstInputInvalidThenValid_ShouldReturnParsedValue()
    {
        string prompt = "Enter int:";
        string errorMsg = "Invalid input";
        string confMsg = "You entered:";
        var outputs = new List<string>();
        Action<string> displayOutput = s => outputs.Add(s);

        int callCount = 0;
        Func<string> readInput = () =>
        {
            callCount++;
            return callCount == 1 ? "abc" : "42";
        };

        int result = Utils.RequestUserInput<int>(prompt, errorMsg, confMsg, displayOutput, readInput,
            (string input, out int parsed) => int.TryParse(input, out parsed));
        Assert.Equal(42, result);
    }

    #endregion

    #region Dummy Types for Testing

    /// <summary>
    /// Dummy base class for testing assignability.
    /// </summary>
    public class BaseClass { }

    /// <summary>
    /// Dummy derived class for testing assignability.
    /// </summary>
    public class DerivedClass : BaseGenericClass<object> { }

    /// <summary>
    /// Dummy unrelated class for testing assignability.
    /// </summary>
    public class UnrelatedClass { }

    /// <summary>
    /// Dummy base generic class for testing assignability.
    /// </summary>
    public class BaseGenericClass<T> : BaseClass { }

    /// <summary>
    /// Dummy derived class for testing assignability.
    /// Generic derived type from generic base type.
    /// </summary>
    public class DerivedGenericClass<T> : BaseGenericClass<T> { }

    /// <summary>
    /// Dummy derived class for testing assignability.
    /// Non-generic derived type from generic base type.
    /// </summary>
    public class DerivedStringParamClass : BaseGenericClass<string> { }

    /// <summary>
    /// Dummy unrelated class for testing assignability.
    /// </summary>
    public class UnrelatedGenericClass<T> { }

    /// <summary>
    /// Dummy base generic interface for testing assignability.
    /// </summary>
    public interface IBaseGeneric<T> { }

    /// <summary>
    /// Dummy derived generic interface for testing assignability.
    /// </summary>
    public interface IDerivedGeneric<T> : IBaseGeneric<T> { }

    /// <summary>
    /// Dummy derived class of generic interface for testing assignability.
    /// Generic derived type from generic base interface type.
    /// </summary>
    public class InterfaceDerivedGenericClass<T> : IDerivedGeneric<T> { }

    /// <summary>
    /// Dummy derived class of generic interface for testing assignability.
    /// Non-generic derived type from generic base type.
    /// </summary>
    public class InterfaceDerivedStringParamClass : IDerivedGeneric<string> { }

    /// <summary>
    /// A sample class used for testing object construction and method invocation.
    /// </summary>
    public class SampleClass
    {
        /// <summary>
        /// Gets the parameter value used to construct the object.
        /// </summary>
        public int Param1 { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleClass"/> class.
        /// </summary>
        /// <param name="param1">An integer parameter.</param>
        public SampleClass(int param1 = 0)
        {
            Param1 = param1;
        }

        /// <summary>
        /// A sample method that adds two integers.
        /// </summary>
        /// <param name="a">The first integer.</param>
        /// <param name="b">The second integer.</param>
        /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public int SampleMethod(int a, int b) => a + b;
    }

    #region Additional Utility and Interactive Functions Tests

    /// <summary>
    /// Tests that TryParseEnumerableOfStrings returns true and the correct list when given an IEnumerable<string>.
    /// </summary>
    [Fact]
    public void TryParseEnumerableOfStrings_WithEnumerable_ShouldReturnTrue()
    {
        IEnumerable<string> input = new List<string> { "a", "b", "c" };
        bool result = Utils.TryParseEnumerableOfStrings(input, out List<string> output);
        Assert.True(result);
        Assert.Equal(new List<string> { "a", "b", "c" }, output);
    }

    /// <summary>
    /// Tests that TryParseEnumerableOfStrings returns false and an empty list when given a non-enumerable object.
    /// </summary>
    [Fact]
    public void TryParseEnumerableOfStrings_WithNonEnumerable_ShouldReturnFalse()
    {
        object input = 123;
        bool result = Utils.TryParseEnumerableOfStrings(input, out List<string> output);
        Assert.False(result);
        Assert.Empty(output);
    }

    /// <summary>
    /// Tests that DictOfSetsToString returns a correctly formatted string containing the header and dictionary entries.
    /// </summary>
    [Fact]
    public void DictOfSetsToString_ShouldReturnFormattedString()
    {
        var dict = new Dictionary<string, SortedSet<int>>
    {
        { "key1", new SortedSet<int> { 1, 2 } },
        { "key2", new SortedSet<int> { 3, 4 } }
    };
        string header = "Header:";
        string result = Utils.DictOfSetsToString(dict, header);
        Assert.Contains("Header:", result);
        Assert.Contains("key1", result);
        Assert.Contains("1", result);
        Assert.Contains("2", result);
        Assert.Contains("key2", result);
        Assert.Contains("3", result);
        Assert.Contains("4", result);
    }

    /// <summary>
    /// Tests that SelectFromList returns the correct option when simulated input is provided.
    /// </summary>
    [Fact]
    public void SelectFromList_ShouldReturnCorrectOption()
    {
        var options = new[] { "Option1", "Option2", "Option3" };
        using (var sr = new StringReader("2\n"))
        {
            // Redirect Console input.
            Console.SetIn(sr);
            string selected = Utils.SelectFromList(options);
            Assert.Equal("Option2", selected);
        }
    }

    /// <summary>
    /// Tests that SelectType returns the correct type and sets the out parameter based on simulated input.
    /// </summary>
    [Fact]
    public void SelectType_ShouldReturnCorrectTypeAndSetOutParameter()
    {
        var dict = new Dictionary<string, Type>
    {
         { "TypeA", typeof(int) },
         { "TypeB", typeof(string) }
    };
        using (var sr = new StringReader("1\n"))
        {
            Console.SetIn(sr);
            string outName;
            var type = Utils.SelectType(dict, out outName);
            Assert.Equal(typeof(int), type);
            Assert.Equal("TypeA", outName);
        }
    }

    /// <summary>
    /// Tests that GetHardwareId returns the correct integer value when simulated input is provided.
    /// </summary>
    [Fact]
    public void GetHardwareId_ShouldReturnParsedInteger()
    {
        // Save original Console input.
        TextReader originalIn = Console.In;

        try
        {
            // Test with first simulated input "42"
            using (var sr = new StringReader("42\n"))
            {
                Console.SetIn(sr);
                int id = Utils.GetHardwareId();
                Assert.Equal(42, id);
            }

            // Test with second simulated input "123"
            using (var sr = new StringReader("123\n"))
            {
                Console.SetIn(sr);
                int id = Utils.GetHardwareId();
                Assert.Equal(123, id);
            }
        }
        finally
        {
            // Restore the original Console input.
            Console.SetIn(originalIn);
        }
    }


    /// <summary>
    /// Dummy class with a constructor taking a single integer, used to test ConstructWithUserInputParams.
    /// </summary>
    public class DummyConstructor
    {
        public int X { get; }
        public DummyConstructor(int x) { X = x; }
    }

    /// <summary>
    /// Tests that ConstructWithUserInputParams creates an instance of the type with the correct parameter value
    /// when simulated input is provided.
    /// </summary>
    [Fact]
    public void ConstructWithUserInputParams_ShouldReturnInstanceWithCorrectValue()
    {
        using (var sr = new StringReader("100\n"))
        {
            Console.SetIn(sr);
            object instance = Utils.ConstructWithUserInputParams(typeof(DummyConstructor));
            Assert.IsType<DummyConstructor>(instance);
            var dummy = (DummyConstructor)instance;
            Assert.Equal(100, dummy.X);
        }
    }

    /// <summary>
    /// Tests that EnumerableToString returns a correctly formatted comma‐separated string.
    /// </summary>
    [Fact]
    public void EnumerableToString_ShouldReturnCommaSeparatedString()
    {
        var list = new List<int> { 1, 2, 3 };
        string result = Utils.EnumerableToString(list);
        Assert.Equal("[1,2,3]", result);
    }

    /// <summary>
    /// Tests that ReadJSON returns the correct file content by creating a temporary file.
    /// </summary>
    [Fact]
    public void ReadJSON_ShouldReturnFileContents()
    {
        // First test with expected content "TestContent"
        string tempFile1 = Path.GetTempFileName();
        try
        {
            string expected1 = "TestContent";
            File.WriteAllText(tempFile1, expected1);
            string content1 = Utils.ReadJSON(tempFile1);
            Assert.Equal(expected1, content1);
        }
        finally
        {
            File.Delete(tempFile1);
        }

        // Second test with expected content "Test JSON Content"
        string tempFile2 = Path.GetTempFileName();
        try
        {
            string expected2 = "Test JSON Content";
            File.WriteAllText(tempFile2, expected2);
            string content2 = Utils.ReadJSON(tempFile2);
            Assert.Equal(expected2, content2);
        }
        finally
        {
            File.Delete(tempFile2);
        }
    }


    /// <summary>
    /// Tests that UserInputDataIsValidType validates input correctly using the type converter.
    /// </summary>
    [Fact]
    public void UserInputDataIsValidType_ShouldValidateCorrectly()
    {
        bool result = Utils.UserInputDataIsValidType("123", typeof(int));
        Assert.True(result);
    }

    #endregion

    #region Interactive Helper Functions Tests

    [Fact]
    public void RequestUserInput_WhenInvalidThenValidInput_ShouldReturnParsedValue()
    {
        // Arrange
        string prompt = "Enter int:";
        string errorMsg = "Invalid input";
        string confMsg = "You entered:";
        int callCount = 0;
        // Simulate first invalid ("abc") then valid ("99") input.
        Func<string> readInput = () =>
        {
            callCount++;
            return callCount == 1 ? "abc" : "99";
        };
        var outputs = new List<string>();
        Action<string> displayOutput = s => outputs.Add(s);

        // Act
        int result = Utils.RequestUserInput<int>(prompt, errorMsg, confMsg, displayOutput, readInput,
            (string input, out int parsed) => int.TryParse(input, out parsed));

        // Assert
        Assert.Equal(99, result);
    }

    [Fact]
    public void ConstructWithUserInputParams_ShouldCreateObjectFromConsoleInput()
    {
        // Arrange: Create a dummy class with a constructor that accepts an int.
        // We'll use an inline dummy type for this test.
        Type typeToConstruct = typeof(DummyForInput);
        // Simulate user entering "77"
        var input = new StringReader("77\n");
        var originalIn = Console.In;
        Console.SetIn(input);

        try
        {
            // Act
            var obj = Utils.ConstructWithUserInputParams(typeToConstruct) as DummyForInput;

            // Assert
            Assert.NotNull(obj);
            Assert.Equal(77, obj.Value);
        }
        finally
        {
            Console.SetIn(originalIn);
        }
    }

    [Fact]
    public void EnumerableToString_ShouldReturnCommaSeparatedValues()
    {
        // Arrange
        var items = new List<string> { "A", "B", "C" };

        // Act
        string result = Utils.EnumerableToString(items);

        // Assert (format: [A,B,C])
        Assert.Equal("[A,B,C]", result);
    }

    [Fact]
    public void TryParseEnumerableOfStrings_WithValidIEnumerable_ShouldReturnTrueAndList()
    {
        // Arrange
        object input = new List<string> { "X", "Y", "Z" };

        // Act
        bool success = Utils.TryParseEnumerableOfStrings(input, out List<string> output);

        // Assert
        Assert.True(success);
        Assert.Equal(new List<string> { "X", "Y", "Z" }, output);
    }

    #endregion

    // Dummy type for ConstructWithUserInputParams test.
    public class DummyForInput
    {
        public int Value { get; }
        public DummyForInput(int value)
        {
            Value = value;
        }
    }


    #region Test Product

    /// <summary>
    /// A simple test product type used in factory tests.
    /// </summary>
    public class TestProduct { }

    #endregion

    #region Additional Dummy Types for Error Testing

    /// <summary>
    /// A dummy class with a constructor that takes a string.
    /// </summary>
    public class NoMatchingConstructorClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoMatchingConstructorClass"/> class.
        /// </summary>
        /// <param name="text">A string parameter.</param>
        public NoMatchingConstructorClass(string text) { }
    }

    /// <summary>
    /// A dummy class with only a private constructor.
    /// </summary>
    public class NoPublicConstructor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoPublicConstructor"/> class.
        /// </summary>
        private NoPublicConstructor() { }
    }

    #endregion

    #region Factory Creation Dummy Types

    /// <summary>
    /// Dummy factory implementation of <see cref="IFactory{TestProduct}"/> for testing factory creation.
    /// </summary>
    public class DummyFactory : IFactory<TestProduct>
    {
        /// <summary>
        /// Gets a help message for the factory.
        /// </summary>
        public string HelpMsg => "Dummy factory help message";

        /// <summary>
        /// Gets the parameter limits for factory creation.
        /// </summary>
        public Dictionary<string, IDataLimits> ParamLimits { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyFactory"/> class.
        /// </summary>
        public DummyFactory()
        {
            // Using production data limits: ContinuousIntDataLimits requires a value between 1 and 100.
            ParamLimits = new Dictionary<string, IDataLimits>
            {
                { "value", new ContinuousIntDataLimits(1, 100) }
            };
        }

        /// <summary>
        /// Attempts to create a product using the provided parameter values.
        /// </summary>
        /// <param name="paramValues">The parameter values for product creation.</param>
        /// <param name="product">When this method returns, contains the created product if successful; otherwise, null.</param>
        /// <param name="skipValueValidation">Optional parameter to skip deeper validation.</param>
        /// <returns>True if the product was created successfully; otherwise, false.</returns>
        public bool TryCreate(Dictionary<string, object> paramValues, out TestProduct product, bool skipValueValidation = false)
        {
            if (paramValues.ContainsKey("value") &&
                paramValues["value"] is int val && val >= 1 && val <= 100)
            {
                product = new TestProduct();
                return true;
            }
            product = null;
            return false;
        }
    }

    /// <summary>
    /// Dummy factory implementation of <see cref="IFactory{TestProduct}"/> that always fails to create a product.
    /// Used to simulate a failure in factory creation.
    /// </summary>
    public class FailingDummyFactory : IFactory<TestProduct>
    {
        /// <summary>
        /// Gets a help message for the factory.
        /// </summary>
        public string HelpMsg => "Failing dummy factory help message";

        /// <summary>
        /// Gets the parameter limits for factory creation.
        /// </summary>
        public Dictionary<string, IDataLimits> ParamLimits { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailingDummyFactory"/> class.
        /// </summary>
        public FailingDummyFactory()
        {
            ParamLimits = new Dictionary<string, IDataLimits>
            {
                { "value", new ContinuousIntDataLimits(1, 100) }
            };
        }

        /// <summary>
        /// Always fails to create a product.
        /// </summary>
        /// <param name="paramValues">The parameter values for product creation.</param>
        /// <param name="product">Always null.</param>
        /// <param name="skipValueValidation">Optional parameter to skip deeper validation.</param>
        /// <returns>False.</returns>
        public bool TryCreate(Dictionary<string, object> paramValues, out TestProduct product, bool skipValueValidation = false)
        {
            product = null;
            return false;
        }
    }

    #endregion

    #region Manufacturability Tests

    /// <summary>
    /// Dummy product type that is manufacturable (i.e. a factory exists for it).
    /// </summary>
    public class ManufacturableProduct { }

    /// <summary>
    /// Dummy product type that is non-manufacturable (i.e. no factory exists for it).
    /// </summary>
    public class NonManufacturableProduct { }

    /// <summary>
    /// A dummy factory for <see cref="ManufacturableProduct"/> that implements the production interface IFactory&lt;ManufacturableProduct&gt;.
    /// </summary>
    public class ManufacturableProductFactory : IFactory<ManufacturableProduct>
    {
        /// <inheritdoc />
        public string HelpMsg => "Test factory for ManufacturableProduct";

        /// <inheritdoc />
        public Dictionary<string, IDataLimits> ParamLimits { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManufacturableProductFactory"/> class.
        /// </summary>
        public ManufacturableProductFactory()
        {
            // For testing, we use ContinuousIntDataLimits to require an integer between 1 and 10.
            ParamLimits = new Dictionary<string, IDataLimits>
    {
        { "dummy", new ContinuousIntDataLimits(1, 10) }
    };
        }

        /// <inheritdoc />
        public bool TryCreate(Dictionary<string, object> paramValues, out ManufacturableProduct product, bool skipValueValidation = false)
        {
            if (paramValues.ContainsKey("dummy") &&
                paramValues["dummy"] is int value &&
                value >= 1 && value <= 10)
            {
                product = new ManufacturableProduct();
                return true;
            }
            product = null;
            return false;
        }
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsManufacturableProduct(Type, out Dictionary{string, Type})"/> returns true
    /// when a factory exists for the product type.
    /// </summary>
    [Fact]
    public void IsManufacturableProduct_WhenFactoryExists_ShouldReturnTrue()
    {
        // We know that ManufacturableProduct is produced by ManufacturableProductFactory.
        // The production IFactory<> implementations are scanned via GetAvailableGenericTypes.
        bool manufacturable = Utils.IsManufacturableProduct(typeof(ManufacturableProduct), out Dictionary<string, Type> factoryTypes);
        Assert.True(manufacturable);
        Assert.NotNull(factoryTypes);
        Assert.True(factoryTypes.Count > 0);
    }

    /// <summary>
    /// Tests that <see cref="Utils.IsManufacturableProduct(Type, out Dictionary{string, Type})"/> returns false
    /// when no factory exists for the product type.
    /// </summary>
    [Fact]
    public void IsManufacturableProduct_WhenNoFactoryExists_ShouldReturnFalse()
    {
        // NonManufacturableProduct is not produced by any factory in this assembly.
        bool manufacturable = Utils.IsManufacturableProduct(typeof(NonManufacturableProduct), out Dictionary<string, Type> factoryTypes);
        Assert.False(manufacturable);
        Assert.NotNull(factoryTypes);
        Assert.Empty(factoryTypes);
    }

    #endregion

    #endregion
}
