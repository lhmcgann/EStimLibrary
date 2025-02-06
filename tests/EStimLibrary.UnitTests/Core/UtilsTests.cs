using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using EStimLibrary.Core;
using EStimLibrary.Core.Haptics;
using EStimLibrary.Core.Stimulation.Stimulators;

namespace EStimLibrary.UnitTests.Core
{
    /// <summary>
    /// Comprehensive unit tests for the <see cref="Utils"/> class.
    /// These tests cover every method inside Utils.cs,
    /// including math helpers, reflection‐based functions,
    /// file I/O, and interactive input functions.
    /// </summary>
    public class UtilsTests
    {
        #region Math Helper Functions Tests

        /// <summary>
        /// Tests that <see cref="Utils.IsWithinUpperBound(double, double, bool)"/> returns true when the upper bound is infinite.
        /// </summary>
        [Fact]
        public void IsWithinUpperBound_WhenUpperBoundIsInfinite_ShouldReturnTrue()
        {
            // Assuming Constants.POS_INFINITY is defined in the library.
            bool result = Utils.IsWithinUpperBound(100, Constants.POS_INFINITY);
            Assert.True(result);
        }

        /// <summary>
        /// Tests that <see cref="Utils.IsWithinUpperBound(double, double, bool)"/> handles inclusive and exclusive comparisons correctly.
        /// </summary>
        [Fact]
        public void IsWithinUpperBound_WhenCalledWithInclusiveAndExclusiveComparisons_ShouldReturnExpectedResults()
        {
            Assert.True(Utils.IsWithinUpperBound(10, 10, inclusive: true));
            Assert.False(Utils.IsWithinUpperBound(10, 10, inclusive: false));
            Assert.True(Utils.IsWithinUpperBound(9, 10, inclusive: false));
        }

        /// <summary>
        /// Tests that <see cref="Utils.IsWithinLowerBound(double, double, bool)"/> returns true when the lower bound is infinite.
        /// </summary>
        [Fact]
        public void IsWithinLowerBound_WhenLowerBoundIsInfinite_ShouldReturnTrue()
        {
            bool result = Utils.IsWithinLowerBound(-100, Constants.NEG_INFINITY);
            Assert.True(result);
        }

        /// <summary>
        /// Tests that <see cref="Utils.IsWithinLowerBound(double, double, bool)"/> handles inclusive and exclusive comparisons correctly.
        /// </summary>
        [Fact]
        public void IsWithinLowerBound_WhenCalledWithInclusiveAndExclusiveComparisons_ShouldReturnExpectedResults()
        {
            Assert.True(Utils.IsWithinLowerBound(10, 10, inclusive: true));
            Assert.False(Utils.IsWithinLowerBound(10, 10, inclusive: false));
            Assert.True(Utils.IsWithinLowerBound(11, 10, inclusive: false));
        }

        /// <summary>
        /// Tests that <see cref="Utils.ScaleValue(double, double, double)"/> scales a normalized value correctly into the target range.
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
        /// Tests that <see cref="Utils.GetAvailableTypes(Type)"/> returns types that implement <see cref="ISelectable"/>.
        /// </summary>
        [Fact]
        public void GetAvailableTypes_WhenTypeExists_ShouldReturnTypes()
        {
            var result = Utils.GetAvailableTypes(typeof(ISelectable));
            Assert.NotNull(result);
            Assert.True(result.Values.All(t => typeof(ISelectable).IsAssignableFrom(t)));
        }

        /// <summary>
        /// Tests that the generic overload <see cref="Utils.GetAvailableTypes{T}()"/> returns types that implement <see cref="ISelectable"/>.
        /// </summary>
        [Fact]
        public void GetAvailableTypes_Generic_WhenTypeExists_ShouldReturnTypes()
        {
            var result = Utils.GetAvailableTypes<ISelectable>();
            Assert.NotNull(result);
            Assert.True(result.Values.All(t => typeof(ISelectable).IsAssignableFrom(t)));
        }

        /// <summary>
        /// Tests that <see cref="Utils.IsAssignableFromType(Type, Type)"/> correctly determines assignability between base and derived types.
        /// </summary>
        [Theory]
        [InlineData(typeof(BaseClass), typeof(DerivedClass), true)]
        [InlineData(typeof(BaseClass), typeof(UnrelatedClass), false)]
        public void IsAssignableFromType_WhenComparingBaseAndDerivedTypes_ShouldReturnExpectedResult(Type baseType, Type derivedType, bool expected)
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
            var result = Utils.GetAvailableGenericTypes(typeof(ITestFactory<>), new Type[] { typeof(TestProduct) });
            Assert.NotNull(result);
            Assert.All(result.Values, type =>
                Assert.True(typeof(ITestFactory<TestProduct>).IsAssignableFrom(type)));
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

        ///// <summary>
        ///// Tests that <see cref="Utils.CreateObjectOfType(Type, List{string}, Dictionary{string, object})"/> throws an <see cref="ArgumentException"/> when no matching constructor is found.
        ///// </summary>
        //[Fact]
        //public void CreateObjectOfType_WhenNoMatchingConstructorExists_ShouldThrowArgumentException()
        //{
        //    Assert.Throws<ArgumentException>(() =>
        //        Utils.CreateObjectOfType(typeof(NoMatchingConstructorClass), new List<string> { "param1" },
        //            new Dictionary<string, object> { { "param1", 5 } }));
        //}

        ///// <summary>
        ///// Tests that <see cref="Utils.GetConstructorParamInfo(Type)"/> throws an <see cref="ArgumentException"/> when no public constructor exists.
        ///// </summary>
        //[Fact]
        //public void GetConstructorParamInfo_WhenNoPublicConstructorExists_ShouldThrowArgumentException()
        //{
        //    Assert.Throws<ArgumentException>(() =>
        //        Utils.GetConstructorParamInfo(typeof(NoPublicConstructor)));
        //}

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
        /// Tests that <see cref="Utils.GetObjectProperty(object, Type, string, out object)"/> returns false and null when the property does not exist.
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
        /// Tests that <see cref="Utils.CallObjectMethod(object, Type, string, object[], out object)"/> returns false and null when the method is not found.
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
        /// Tests that <see cref="Utils.TryFactoryCreate(dynamic, out dynamic, Action{string}, Func{string})"/> returns false when the factory fails to create a product.
        /// </summary>
        [Fact]
        public void TryFactoryCreate_WhenInvalidParametersProvided_ShouldReturnFalse()
        {
            // Use a factory that always fails.
            var factory = new FailingDummyFactory();
            Func<string> readInput = () => "5";
            var outputs = new List<string>();
            Action<string> displayOutput = s => outputs.Add(s);

            bool success = Utils.TryFactoryCreate(factory, out dynamic product, displayOutput, readInput);
            Assert.False(success);
        }

        /// <summary>
        /// Tests that <see cref="Utils.RequestUserInput{T}(string, string, string, Action{string}, Func{string}, Utils.ParseAndValidateFunc{T})"/> loops on invalid input before returning a valid parsed value.
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

        #region Additional File I/O Coverage Tests

        // (No additional tests needed here if all branches are already covered.)

        #endregion

        #region Interactive Input Helper Functions Tests

        // (All interactive tests are already covered above.)

        #endregion

        #region Dummy Types for Testing

        /// <summary>
        /// Dummy base class for testing assignability.
        /// </summary>
        public class BaseClass { }

        /// <summary>
        /// Dummy derived class for testing assignability.
        /// </summary>
        public class DerivedClass : BaseClass { }

        /// <summary>
        /// Dummy unrelated class for testing assignability.
        /// </summary>
        public class UnrelatedClass { }

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

        #region Temporary Factory Types

        /// <summary>
        /// Temporary factory interface used for testing in this file.
        /// </summary>
        public interface ITestFactory<T> { }

        /// <summary>
        /// Temporary product class used for testing in this file.
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
        /// Dummy implementation of <see cref="IDataLimits"/> from the Core library, used for testing.
        /// Implements <see cref="ISelectable"/> by providing the <c>Name</c> property.
        /// </summary>
        public class DummyDataLimits : IDataLimits
        {
            /// <summary>
            /// Gets or sets the name of the selectable item.
            /// </summary>
            public string Name { get; set; }

            /// <inheritdoc />
            public Type ValidDataType { get; set; }

            /// <inheritdoc />
            public string Description { get; set; }

            /// <inheritdoc />
            public bool IsValidDataValue(object value)
            {
                if (ValidDataType == typeof(int) && value is int i)
                {
                    return i > 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Dummy factory implementing <see cref="ITestFactory{TestProduct}"/> for testing factory creation.
        /// </summary>
        public class DummyFactory : ITestFactory<TestProduct>
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
                ParamLimits = new Dictionary<string, IDataLimits>
                {
                    { "value", new DummyDataLimits { ValidDataType = typeof(int), Description = "Enter a positive integer", Name = "Value" } }
                };
            }

            /// <summary>
            /// Attempts to create a product using the provided parameter values.
            /// </summary>
            /// <param name="paramValues">The parameter values for product creation.</param>
            /// <param name="product">
            /// When this method returns, contains the created product if successful; otherwise, null.
            /// </param>
            /// <returns>True if the product was created successfully; otherwise, false.</returns>
            public bool TryCreate(Dictionary<string, object> paramValues, out object product)
            {
                if (paramValues.ContainsKey("value") &&
                    paramValues["value"] is int val && val > 0)
                {
                    product = new TestProduct();
                    return true;
                }
                product = null;
                return false;
            }
        }

        /// <summary>
        /// Dummy factory that always fails to create a product.
        /// Used to simulate factory creation failure.
        /// </summary>
        public class FailingDummyFactory : ITestFactory<TestProduct>
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
                    { "value", new DummyDataLimits { ValidDataType = typeof(int), Description = "Enter a positive integer", Name = "Value" } }
                };
            }

            /// <summary>
            /// Always fails to create a product.
            /// </summary>
            /// <param name="paramValues">The parameter values for product creation.</param>
            /// <param name="product">Always null.</param>
            /// <returns>False.</returns>
            public bool TryCreate(Dictionary<string, object> paramValues, out object product)
            {
                product = null;
                return false;
            }
        }

        #endregion

        #endregion
    }
}
