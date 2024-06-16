using System.ComponentModel;
using System.IO.Ports;
using System.Reflection;

using EStimLibrary.Core.Stimulation.Stimulators;


namespace EStimLibrary.Core;


// TODO: make utils functions independent of console app use case. Separate get
// and user select.
public static class Utils
{
    #region Math Helper Functions
    /// <summary>
    /// Check if a value adheres to an upper bound, allowing for an infinite
    /// bound, as well as an inclusive or exclusive bound.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="upperBound">The bound. May be the positive infinity value
    /// as defined in this library.</param>
    /// <param name="inclusive">An optional parameter (default true): whether
    /// or not to consider the given bound inclusive.</param>
    /// <returns>Whether or not the test value is within the bound. Always true
    /// if the bound is infinite.</returns>
    public static bool IsWithinUpperBound(double value, double upperBound,
        bool inclusive = true)
    {
        return upperBound == Constants.POS_INFINITY ||
            (inclusive) ? value <= upperBound : value < upperBound;
    }

    /// <summary>
    /// Check if a value adheres to a lower bound, allowing for an infinite
    /// bound, as well as an inclusive or exclusive bound.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="lowerBound">The bound. May be the negative infinity value
    /// as defined in this library.</param>
    /// <param name="inclusive">An optional parameter (default true): whether
    /// or not to consider the given bound inclusive.</param>
    /// <returns>Whether or not the test value is within the bound. Always true
    /// if the bound is infinite.</returns>
    public static bool IsWithinLowerBound(double value, double lowerBound,
        bool inclusive = true)
    {
        return lowerBound == Constants.NEG_INFINITY ||
            (inclusive) ? lowerBound <= value : lowerBound < value;
    }

    /// <summary>
    /// Scale a normalized value into a specified range.
    /// </summary>
    /// <param name="normValue">The value [0.0, 1.0] to scale.</param>
    /// <param name="min">The minimum value of the range to scale into
    /// (inclusive).</param>
    /// <param name="max">The maximum value of the range to scale into
    /// (inclusive).</param>
    /// <returns>The value scaled into the min-max range.</returns>
    public static double ScaleValue(double normValue, double min, double max)
    {
        var range = max - min;
        return normValue * range + min;
    }
    #endregion

    #region Reflection Functions
    /// <summary>
    /// Get all specific implemented or derived Types of T available in this
    /// AppDomain.
    /// </summary>
    /// <typeparam name="T">The type to search for. Must be a subclass of
    /// ISelectable.</typeparam>
    /// <returns>A dictionary of (string name, Type) pairs.</returns>
    public static Dictionary<string, Type> GetAvailableTypes<T>()
        where T : ISelectable
    {
        return Utils.GetAvailableTypes(typeof(T));
    }

    /// <summary>
    /// Get all specific implemented or derived Types of T available in this
    /// AppDomain.
    /// </summary>
    /// <param name="searchType">The type to search for. Must be a subclass of
    /// ISelectable, but this requirement is not enforced.</param>
    /// <returns>A dictionary of (string name, Type) pairs.</returns>
    public static Dictionary<string, Type> GetAvailableTypes(Type searchType)
    {
        // Get a list of all the derived classes (Types) of the given type T.
        List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(testType => !testType.IsInterface && !testType.IsAbstract &&
                Utils.IsAssignableFromType(searchType, testType))
            .ToList();
        // Return a dictionary of string names and types.
        // TODO: is this actually using the ISelectable Name property, or just
        // the inherent Name property (as returned by the nameof() operator)? If
        // the latter, do we even need the ISelectable interface?
        return types.ToDictionary(t => t.Name, t => t);
    }

    /// <summary>
    /// Just a temporary debug helper function so I dono't have to put all of
    /// these in Program.cs
    /// </summary>
    public static void TEST_PRINTS()
    {
        //// false
        //Console.WriteLine($"WSS.IsGeneric: {typeof(WSS).IsGenericType}");
        //// true
        //Console.WriteLine($"WSS.Base.IsGeneric: " +
        //    $"{typeof(WSS).BaseType.IsGenericType}");
        //// false
        //Console.WriteLine($"WSS.Base.IsGenTDef: " +
        //    $"{typeof(WSS).BaseType.IsGenericTypeDefinition}");
        //// true
        //Console.WriteLine($"WSS.Base.IsCstrGen: " +
        //    $"{typeof(WSS).BaseType.IsConstructedGenericType}");

        //// true
        //Console.WriteLine($"Stim<>.IsGeneric: " +
        //    $"{typeof(Stimulator<>).IsGenericType}");
        //// true
        //Console.WriteLine($"Stim<>.IsGenTDef: " +
        //    $"{typeof(Stimulator<>).IsGenericTypeDefinition}");
        //// false
        //Console.WriteLine($"Stim<>.IsCstrGen: " +
        //    $"{typeof(Stimulator<>).IsConstructedGenericType}");
        //// false
        //Console.WriteLine($"Stim<>.AssignableFrom(WSS): " +
        //    $"{typeof(Stimulator<>).IsAssignableFrom(typeof(WSS))}");
        //// true
        //Console.WriteLine($"Stim<>.AssignableFrom(WSS.Base.GenTDef): " +
        //    $"{typeof(Stimulator<>).IsAssignableFrom(
        //        typeof(WSS).BaseType.GetGenericTypeDefinition())}");

        //// true
        //Console.WriteLine($"Stim<B>.IsGeneric: " +
        //    $"{typeof(Stimulator<BaseStimParam>).IsGenericType}");
        //// false
        //Console.WriteLine($"Stim<B>.IsGenTDef: " +
        //    $"{typeof(Stimulator<BaseStimParam>).IsGenericTypeDefinition}");
        //// true
        //Console.WriteLine($"Stim<B>.IsCstrGen: " +
        //    $"{typeof(Stimulator<BaseStimParam>).IsConstructedGenericType}");
        //// true
        //Console.WriteLine($"Stim<B>.AssignableFrom(WSS): " +
        //    $"{typeof(Stimulator<BaseStimParam>).IsAssignableFrom(
        //        typeof(WSS))}");
        //// false
        //Console.WriteLine($"Stim<B>.AssignableFrom(WSS.Base.GenTDef): " +
        //    $"{typeof(Stimulator<BaseStimParam>).IsAssignableFrom(
        //        typeof(WSS).BaseType.GetGenericTypeDefinition())}");

        //Console.WriteLine("\n\n");

        //// true
        //Console.WriteLine($"Stim<>.IsAssignableFromType(WSS): " +
        //    $"{Utils.IsAssignableFromType(typeof(Stimulator<>), typeof(WSS))}");
        //// true
        //Console.WriteLine($"Stim<B>.IsAssignableFromType(WSS): " +
        //    $"{Utils.IsAssignableFromType(typeof(Stimulator<BaseStimParam>),
        //    typeof(WSS))}");
        //// 
        //Console.WriteLine($"Stim<Enum>.IsAssignableFromType(WSS): " +
        //    $"{Utils.IsAssignableFromType(typeof(Stimulator<Enum>),
        //    typeof(WSS))}");
    }

    public static bool IsAssignableFromType(Type baseTargetType,
        Type derivedTestType)
    {
        // If target type is simple and not a generic type, use existing check.
        if (!baseTargetType.IsGenericType)
        {
            return baseTargetType.IsAssignableFrom(derivedTestType);
        }

        // Otherwise, perform a more in-depth evaluation for the generic type.
        // 1) Check the derived type directly.
        if (derivedTestType.IsGenericType)
        {
            return Utils.IsGenericAssignableFrom(baseTargetType,
                derivedTestType);
        }
        // 2) Check if the derived type inherits from a base type that is
        // generic. BaseType property returns null if from Object or interface.
        var derivedBaseType = derivedTestType.BaseType;
        if (derivedBaseType is not null && derivedBaseType.IsGenericType)
        {
            return Utils.IsGenericAssignableFrom(baseTargetType,
                derivedBaseType);
        }
        // 3) Check if the derived type implements any interfaces that are
        // generic types.
        var interfaceTypes = derivedTestType.GetInterfaces();
        bool success = false;
        foreach (var iType in interfaceTypes)
        {
            if (iType.IsGenericType)
            {
                success |= Utils.IsGenericAssignableFrom(baseTargetType, iType);
            }
        }
        // Return if any acceptable interface found.
        return success;

        //if (baseTargetType.IsGenericTypeDefinition)
        //{
        //    Type[] derivedBaseTypeGenericArgs = derivedTestType.BaseType?.GetGenericArguments();
        //    if (derivedBaseTypeGenericArgs != null && derivedBaseTypeGenericArgs.Length > 0)
        //    {
        //        Type baseTypeDefinition = derivedTestType.BaseType.GetGenericTypeDefinition();
        //        return baseTypeDefinition == baseTargetType;
        //    }
        //}
    }

    /// <summary>
    /// Test if a generic base type can be assigned from a test generic type,
    /// i.e., is the test type derived from the base. Allows the test type to
    /// have generic parameters of derived types as well (e.g.,
    /// base:List<Rectangle> and test:List<Square> would pass).
    /// </summary>
    /// <param name="baseGenericTypeOrDefinition">The base generic type. Can be
    /// a constructed/closed generic type (e.g., List<int>) or an open generic
    /// (aka generic type definition, e.g., List<>).</param>
    /// <param name="testGenericType">The test generic type. Must be a
    /// constructed generic type.</param>
    /// <returns>True if the base type can be assigned from the test type.
    /// </returns>
    public static bool IsGenericAssignableFrom(
        Type baseGenericTypeOrDefinition, Type testGenericType)
    {
        // Only test if both params are generic types, and if the test type is
        // a closed/constructed generic type specifically.
        if (baseGenericTypeOrDefinition.IsGenericType &&
            testGenericType.IsGenericType &&
            testGenericType.IsConstructedGenericType)
        {
            // 1) Test open generic type (e.g., List<>) given as the base.
            if (baseGenericTypeOrDefinition.IsGenericTypeDefinition)
            {
                return baseGenericTypeOrDefinition.IsAssignableFrom(
                    testGenericType.GetGenericTypeDefinition());
            }
            // 2) Test closed generic type (e.g., List<int>) given as the base.
            if (baseGenericTypeOrDefinition.IsConstructedGenericType)
            {
                // a) Test if direct assignment works, succeeding if so.
                if (baseGenericTypeOrDefinition.IsAssignableFrom(
                    testGenericType))
                {
                    return true;
                }
                // b) If that fails, make sure the generic type definitions are
                // at least the same, failing if not.
                if (baseGenericTypeOrDefinition.GetGenericTypeDefinition()
                    != testGenericType.GetGenericTypeDefinition())
                {
                    return false;
                }
                // c) If that succeeds, then check if the generic param(s) are
                // just also derived. Type def success means same num params.
                Type[] baseParamTypes = baseGenericTypeOrDefinition
                    .GetGenericArguments();
                Type[] testParamTypes = testGenericType.GetGenericArguments();
                for (int i = 0; i < baseParamTypes.Length; i++)
                {
                    Type baseParamType = baseParamTypes[i];
                    Type testParamType = testParamTypes[i];
                    // Fail if a test param is not derived from the base param
                    // type.
                    if (!Utils.IsAssignableFromType(baseParamType, testParamType))
                    {
                        return false;
                    }
                }
                // Otherwise, succeed!
                return true;
            }
        }

        // Fail otherwise.
        return false;
    }

    /// <summary>
    /// Get all specific implemented or derived generic Types that use the given
    /// target Type(s) as generics parameter(s).
    /// </summary>
    /// <param name="genericType">The generic type to filter the search of
    /// implemented types by, i.e., the class parent type.</param>
    /// <param name="genericParamTypes">The generics parameter type(s). The
    /// search looks for implemented Types (of genericType) that use this/these
    /// Type(s) as generic paremeter(s).</param>
    /// <returns>A dictionary of (string name, Type) pairs.</returns>
    public static Dictionary<string, Type> GetAvailableGenericTypes(
        Type genericType, Type[] genericParamTypes)
    {
        var fullGenericType = genericType.MakeGenericType(genericParamTypes);

        // Find types that implement the generic interface with specified type
        var matchingTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => fullGenericType.IsAssignableFrom(x)
                        && !x.IsInterface && !x.IsAbstract);
        return matchingTypes.ToDictionary(t => t.Name, t => t);
    }

    public static bool AreTypeParametersCompatible(Type typeA, Type typeB)
    {
        // Check if typeA and typeB are generic types.
        if (typeA.IsGenericType && typeB.IsGenericType)
        {
            // Get the generic type definitions.
            var genericTypeA = typeA.GetGenericTypeDefinition();
            var genericTypeB = typeB.GetGenericTypeDefinition();

            // Check if the generic types are the same.
            if (genericTypeA == genericTypeB)
            {
                // Check if the type parameters are the same or derived.
                var typeParametersA = typeA.GetGenericArguments();
                var typeParametersB = typeB.GetGenericArguments();

                for (int i = 0; i < typeParametersA.Length; i++)
                {
                    if (!typeParametersA[i]
                        .IsAssignableFrom(typeParametersB[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        return false;
    }

    // TODO: can delete once the above is tested for Stimulator
    /// <summary>
    /// Get all specific Stimulator derived class Types available in this
    /// AppDomain.
    /// </summary>
    /// <returns>A dictionary of derived stimulator class (string name, Type)
    /// pairs.</returns>
    public static Dictionary<string, Type> GetAvailableStimulatorTypes()
    {
        // Get a list of all the derived classes (Types) of Stimulator.
        List<Type> stimTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(Stimulator).IsAssignableFrom(x) &&
                !x.IsInterface && !x.IsAbstract)
            .ToList();
        // Return a dictionary of string names of derived classes to the class
        // Type.
        return stimTypes
            .Select(t => (t.Name, t))
            .ToDictionary(tuple => tuple.Name, tuple => tuple.t);
    }

    /// <summary>
    /// Check if a certain type is a manufacturable product, i.e., it has an
    /// IFactory associated with it within this assembly.
    /// </summary>
    /// <param name="testProductType">The product type to search for.</param>
    /// <param name="availableFactoryTypes">An output parameter: the dictionary
    /// of stringName-Type pairs for each IFactory found to produce the given
    /// product type. Only valid data if the function returns true.</param>
    /// <returns>True if the given type can be produced by a factory, False if
    /// not.</returns>
    public static bool IsManufacturableProduct(Type testProductType,
        out Dictionary<string, Type> availableFactoryTypes)
    {
        // Try getting the factories of the given product type.
        availableFactoryTypes = GetAvailableGenericTypes(typeof(IFactory<>),
            new Type[] { testProductType });
        // Return T/F if any were found.
        return availableFactoryTypes is not null &&
            availableFactoryTypes.Count > 0;
    }

    // TODO: delete? currently not used
    public static bool UserInputDataIsValidType(string userInput,
        Type expectedType)
    {
        return TypeDescriptor.GetConverter(expectedType).IsValid(userInput);
    }

    public static bool TryConvertFromType(string userInput, Type targetType,
        out object convertedVal)
    {
        convertedVal = TypeDescriptor.GetConverter(targetType)
            .ConvertFrom(userInput);
        return convertedVal is not null;
    }

    public static Dictionary<string, object> RequestFactoryCreateParamValues(
        string helpMsg, Dictionary<string, IDataLimits> paramLims,
        Action<string> displayOutput, Func<string> readInput)
    {
        // Display the overall factory help message.
        displayOutput(helpMsg);

        // Get user-input values for each param needed by the factory to create
        // a product, displaying further per-param description and performing
        // live data validation.
        Dictionary<string, object> paramValues = new();
        foreach (var (name, limits) in paramLims)
        {
            // The prompt, error, and confirmation messages.
            string prompt = $"\nPlease enter a value parseable as type " +
                $"'{limits.ValidDataType}' for the parameter '{name}'. " +
                $"{limits.Description}: ";
            string errorMsg = $"\nInvalid input. Please try again.";
            string confMsg = $"\nYou have entered the value:";

            // Get and return the user input param value.
            var value = Utils.RequestUserInput(prompt, errorMsg, confMsg,
                displayOutput, readInput,
                (string userInput, out object parsedVal) =>
                    Utils.TryConvertFromType(userInput, limits.ValidDataType,
                        out parsedVal) &&
                    limits.IsValidDataValue(parsedVal));

            // Convert the user input to the correct type.
            var paramValue = Convert.ChangeType(value, limits.ValidDataType);
            // Add the value to the dictionary.
            paramValues.Add(name, paramValue);
        }

        // Return the values.
        return paramValues;
    }

    /// <summary>
    /// Perform a full user-input-based factory creation of a product. This only
    /// works if the object given as the factory is only a factory of one type
    /// of product.
    /// </summary>
    /// <param name="factory">The factory object to use. Must implement a
    /// singular IFactory<T> type.</param>
    /// <param name="product">The output product upon successful creation.
    /// </param>
    /// <param name="displayOutput">A function to convey information to the
    /// user.</param>
    /// <param name="readInput">A function to capture information from the
    /// user.</param>
    /// <returns></returns>
    public static bool TryFactoryCreate(dynamic factory, out dynamic product,
        Action<string> displayOutput, Func<string> readInput)
    {
        // Get the parameter values.
        var paramValues = Utils.RequestFactoryCreateParamValues(factory.HelpMsg,
            factory.ParamLimits, displayOutput, readInput);

        // Use the factory to create the product with the input values.
        // TODO: got an error saying invalid params given for the best
        // overloaded method found for this definition, but nothing incorrect...
        // Maybe issue w/ having an optional param?
        return factory.TryCreate(paramValues, out product);
    }

    //public static bool TryFactoryCreate(Type productType, out dynamic product,
    //    Func<string[], string> factorySelectFunction,
    //    Action<string> displayOutput, Func<string> readInput)
    ////// Param request order, Type specifications, validated values
    ////Func<List<string>, Dictionary<string, Type>,
    ////    (object[], Dictionary<string, object>)> productParamRequestFunction)
    //{
    //    // Try getting the factory(ies) for the given product type. Fail if none
    //    // found.
    //    if (!Utils.IsManufacturableProduct(productType,
    //        out Dictionary<string, Type> factoryTypes))
    //    {
    //        product = null;
    //        return false;
    //    }

    //    // Ask the user to pick the factory via the input I/O function.
    //    var factoryOptions = factoryTypes.Keys.ToArray();
    //    var chosenFactoryName = factorySelectFunction(factoryOptions);
    //    Type chosenFactoryType = factoryTypes[chosenFactoryName];

    //    // Create the factory.
    //    //var factory = Utils.CreateObjectOfType(factoryTypes[chosenFactoryName],
    //    //    new(), new());
    //    //var fullGenericType = genericType.MakeGenericType(genericParamTypes);
    //    dynamic factory = Activator.CreateInstance(chosenFactoryType);

    //    // (call RequestFactoryCreateParams or whatever else here)
    //}

    // Will return the details pertaining to the first constructor defined.
    public static (List<string>, Dictionary<string, Type>)
        GetConstructorParamInfo(Type desiredType)
    {
        // Get the first or default (empty) constructor of the given type.
        ConstructorInfo constructor =
            desiredType.GetConstructors().FirstOrDefault();

        // If no constructor found, error.
        if (constructor == null)
        {
            throw new ArgumentException($"No constructor found for type " +
                $"{desiredType}");
        }

        //// Else, scrape parameter name and type data.
        //List<string> orderedParamNames = constructor.GetParameters()
        //        .Select(param => param.Name).ToList();
        //Dictionary<string, Type> paramTypes = constructor.GetParameters()
        //    .ToDictionary(param => param.Name, param => param.ParameterType);

        //return (orderedParamNames, paramTypes);
        return GetMethodParamInfo(constructor);
    }

    public static (List<string>, Dictionary<string, Type>)
        GetMethodParamInfo(MethodBase methodInfo)
    {
        // Scrape parameter name and type data.
        List<string> orderedParamNames = methodInfo.GetParameters()
                .Select(param => param.Name).ToList();
        Dictionary<string, Type> paramTypes = methodInfo.GetParameters()
            .ToDictionary(param => param.Name, param => param.ParameterType);

        return (orderedParamNames, paramTypes);
    }

    public static object CreateObjectOfType(Type desiredType,
        List<string> parameterNames,
        Dictionary<string, object> parameterValues)
    {
        // Get the constructor with the specified parameter types
        ConstructorInfo constructor = desiredType.GetConstructor(
            parameterValues.Values.Select(value => value.GetType()).ToArray());

        // If no constructor found, error.
        if (constructor == null)
        {
            throw new ArgumentException($"No constructor found with the " +
                $"specified parameter types for type {desiredType}");
        }

        // Else, extract the parameter values in the correct order.
        object[] constructorArgs = parameterNames
            .Select(name => parameterValues[name]).ToArray();

        // Create and return an instance of the class with the param values.
        return constructor.Invoke(constructorArgs);
    }

    public static bool GetObjectProperty(object instance, Type objectType,
        string propertyName, out object propertyValue)
    {
        PropertyInfo countProperty = objectType.GetProperty(propertyName);
        if (countProperty != null)
        {
            propertyValue = countProperty.GetValue(instance);
            return true;
        }
        propertyValue = null;
        return false;
    }

    public static bool CallObjectMethod(object instance, Type objectType,
        string methodName, object[] methodParams, out object returnValue)
    {
        returnValue = null; // Start value. If not successful, indicates error.
        MethodInfo method = objectType.GetMethod(methodName);
        if (method != null)
        {
            // Call the method with the given params. Assuming all goes well.
            returnValue = method.Invoke(instance, methodParams);
        }
        return returnValue != null;
    }

    #endregion Reflections Functions


    public static string ReadJSON(string filePath)
    {
        // Read the entire contents of the JSON file into a string.
        return File.ReadAllText(filePath);  // May throw an IOException.
    }

    public static string EnumerableToString(IEnumerable<object> values)
    {
        return $"[{string.Join(',', values)}]";
    }

    public static bool TryParseEnumerableOfStrings(object value,
        out List<string> stringList)
    {
        // Try to extract the IEnumerable<string>, assuming object is that exact
        // type.
        if (value is IEnumerable<string> strings)
        {
            stringList = new(strings);
            return true;
        }
        else
        {
            stringList = new();
            return false;
        }
    }

    public static object ConstructWithUserInputParams(Type desiredType)
    {
        // Get the constructor info.
        (List<string> orderedParamNames, Dictionary<string, Type> paramTypes) =
            GetConstructorParamInfo(desiredType);
        // Get the user input param values.
        (var _, Dictionary<string, object> paramValuesByName) =
            RequestParameterValues(orderedParamNames, paramTypes);
        // Construct the object.
        return CreateObjectOfType(desiredType, orderedParamNames,
            paramValuesByName);
    }


    #region Helper I/O Functions
    public delegate bool ParseAndValidateFunc<T>(string userInput,
        out T parsedUserInput);
    // Note: all Funcs return something whereas all Actions return void
    public static T RequestUserInput<T>(string prompt, string errorMsg,
        string confMsg, Action<string> displayOutput, Func<string> readInput,
        //Func<string, bool> isValidInput, Func<string, T> parseInput)
        ParseAndValidateFunc<T> parseAndValidateInput)
    {
        // Output the initial prompt
        //Console.WriteLine(prompt);
        displayOutput(prompt);

        // Read for the user's input until a valid value is given.
        string userInputStr = readInput();//Console.ReadLine();
        T parsedUserInput;  // Declare variable so can be used after loop.
        while (!parseAndValidateInput(userInputStr, out parsedUserInput))
        {
            // Error message.
            //Console.WriteLine(errorMsg);
            displayOutput(errorMsg);
            // Rewrite the prompt.
            //Console.WriteLine(prompt);
            displayOutput(prompt);
            // Wait until new input given.
            userInputStr = readInput();//Console.ReadLine();
        }

        // Parse the user input.
        //T parsedUserInput = parseInput(userInputStr);

        // Output the confirmation message.
        //Console.WriteLine($"{confMsg} {parsedUserInput}");
        displayOutput($"{confMsg} {parsedUserInput}");

        // Return the parsed user input.
        return parsedUserInput;
    }

    public static string SelectFromList(string[] options)
    {
        // Build the prompt message of selections.
        string prompt = "Available Options:";
        for (int i = 0; i < options.Length; i++)
        {
            prompt += $"\n\t{i + 1}. {options[i]}";
        }
        prompt += "\nPlease enter the integer corresponding to the desired " +
            "option: ";

        // Error and confirmation messages.
        string errorMsg = "Invalid selection. Please try again.";
        string confMsg = "You have selected:";

        // Get the user input.
        int optSelect = RequestUserInput(prompt, errorMsg, confMsg,
            Console.WriteLine, Console.ReadLine,
            (string userInput, out int selectedIndex) => (
                int.TryParse(userInput, out selectedIndex) &&
                selectedIndex >= 1 &&
                selectedIndex <= options.Length));

        // Return actual string option.
        return options[optSelect - 1];
    }

    public static Type SelectType(Dictionary<string, Type> typeDict,
        out string typeName)
    {
        // Get the list of string names of all types.
        string[] typeStrs = typeDict.Keys.ToArray();
        // Get the user selection.
        typeName = SelectFromList(typeStrs);
        // Return the Type selection.
        return typeDict[typeName];
    }

    // TODO: delete once the above generalized function is tested
    public static Type SelectStimulatorType(
        Dictionary<string, Type> stimTypeDict,
        out string stimTypeName)
    {
        // Get the list of string names of all stim types.
        string[] stimTypeStrs = stimTypeDict.Keys.ToArray();
        // Get the user selection.
        stimTypeName = SelectFromList(stimTypeStrs);
        // Return the stim Type selection.
        return stimTypeDict[stimTypeName];
    }

    /// <summary>
    /// SelectPort() retrieves the list of available serial ports, asks the user to
    /// choose one, and returns the selected port name in a string.
    /// </summary>
    public static string SelectPort()
    {
        // Get the list of port name options.
        string[] portNames = SerialPort.GetPortNames();
        // Get the user selection.
        return SelectFromList(portNames);
    }

    public static int GetHardwareId()
    {
        // Get and return the user input hardware ID.
        return Utils.GetInt("hardware ID",
            " number associated with this stimulator");
    }

    public static int GetInt(string intName, string intNameExtension = "")
    {
        // The prompt, error, and confirmation messages.
        string prompt = $"Please enter the integer {intName}{intNameExtension}: ";
        string errorMsg = $"Invalid input: {intName} must be an integer. Please try again.";
        string confMsg = $"You have entered the {intName}:";

        // Get and return the user input hardware ID.
        return RequestUserInput(prompt, errorMsg, confMsg,
            Console.WriteLine, Console.ReadLine,
            (string userInput, out int hwID) =>
                int.TryParse(userInput, out hwID));
    }

    public static (object[], Dictionary<string, object>)
        RequestConstructorParameterValues(Type desiredType)
    {
        // Get the constructor information (first or default constructor).
        (List<string> orderedParamNames, Dictionary<string, Type> paramTypes) =
            GetConstructorParamInfo(desiredType);

        // return the ordered object[] and Dictionary<string, object>.
        return RequestParameterValues(orderedParamNames, paramTypes);
    }

    public static (object[], Dictionary<string, object>) RequestParameterValues(
        List<string> orderedParamNames, Dictionary<string, Type> paramTypes)
    {
        // Create a List to store the parameter values in order.
        List<object> parameterValues = new();
        // Create a Dictionary to lookup param values by param name.
        Dictionary<string, object> paramValuesByName = new();

        // Ask the user for input for each parameter.
        foreach (var paramName in orderedParamNames)
        {
            Type paramType = paramTypes[paramName];
            string prompt = $"Enter value for parameter {paramName}" +
                $"({paramType}): ";
            string errorMsg = $"Invalid input. Please try again.";
            string confMsg = "You have entered the value:";

            // Get an object variable that represents the correct type-converted
            // value given by the user.
            var value = RequestUserInput(prompt, errorMsg, confMsg,
                Console.WriteLine, Console.ReadLine,
                (string userInput, out object parsedVal) =>
                    TryConvertFromType(userInput, paramType, out parsedVal));

            // Convert the user input to the correct type.
            var paramValue = Convert.ChangeType(value, paramType);
            // Add the param value to output variables.
            parameterValues.Add(paramValue);
            paramValuesByName.Add(paramName, paramValue);
        }

        // Return an array of object parameter values.
        return (parameterValues.ToArray(), paramValuesByName);

        //// Create an instance of the class using the constructor and provided values
        //object myObject = constructor.Invoke(parameterValues);

        //// Now you can work with myObject
        //if (myObject is T myInstance)
        //{
        //    return myInstance;
        //}
        //return null;
    }

    #endregion
}


//public static T RequestUserInput<T>(string prompt, string errorMsg, string confMsg,
//        Func<string, T> tryParse, Func<T, bool> isValid)
//{
//    // Output the initial prompt
//    Console.WriteLine(prompt);

//    // Create a variable for parsed user input.
//    T parsedUserInput;

//    // Read for the user's input until a valid value is given.
//    string userInputStr = Console.ReadLine();
//    while (true)
//    {
//        // Try parsing input: string => T
//        try
//        {
//            parsedUserInput = tryParse(userInputStr);

//            // If parsing does not thrown an error but value not valid,
//            // throw an error to loop input value request.
//            if (!isValid(parsedUserInput))
//            {
//                throw new Exception();
//            }
//            // Else if valid, break upon this success.
//            break;
//        }
//        // Upon failure, try again.
//        catch
//        {
//            // Print error message and repeat request for input.
//            Console.WriteLine(errorMsg);        // Error message.
//            Console.WriteLine(prompt);          // Rewrite the prompt.
//            userInputStr = Console.ReadLine();  // Wait until new input given.
//        }
//    }

//    // Output the confirmation message.
//    Console.WriteLine($"{confMsg} {parsedUserInput}");

//    // Return the parsed user input.
//    return parsedUserInput;
//}
