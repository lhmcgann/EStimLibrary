using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Extensions.SpatialModel.StringHierarchy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy;


/// <summary>
/// Unit tests for the StringHierarchyBodyModelBuilder class.
/// </summary>
public class StringHierarchyBodyModelBuilderTests
{
    private readonly ITestOutputHelper _output;

    protected const string TEST_FILEPATH =
        "./../../../Extensions/SpatialModel/StringHierarchy" +
        "/StringHierarchyBodyModelBuilderTestFiles";

    /// <summary>
    /// Test class constructor creates an output helper so can write console
    /// </summary>
    /// <param name="testOutputHelper">Test console.</param>
    // output.
    public StringHierarchyBodyModelBuilderTests(
        ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    /// <summary>
    /// Test GetTemplateJSONString static method.
    /// </summary>
    [Fact]
    public void GetTemplateJSONString_ShouldReturnParseableString()
    {
        var templateJSON =
            StringHierarchyBodyModelBuilder.GetTemplateJSONString();

        JObject obj = JObject.Parse(templateJSON);

        Assert.NotNull(obj);
    }


    #region _CheckJSONPropertyType Tests

    /// <summary>
    /// Test _CheckJSONPropertyType with differing types.
    /// </summary>
    [Fact]
    public void CheckJSONPropertyType_ShouldThrowArgumentException()
    {
        Type type = typeof(StringHierarchyBodyModelBuilder);

        JProperty prop = new JProperty("property", 8);
        JTokenType tok = JTokenType.Boolean;

        Assert.Throws<ArgumentException>(() =>
        {
            try
            {
                type.InvokeMember("_CheckJSONPropertyType",
                    System.Reflection.BindingFlags.InvokeMethod |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static, null, null,
                    new object[2] { prop, tok });
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        });
    }

    /// <summary>
    /// Test _CheckJSONPropertyType with matching types. Should complete 
    /// without throwing an exception, returning nothing.
    /// </summary>
    [Fact]
    public void CheckJSONPropertyType_ShouldReturnNothing()
    {
        Type type = typeof(StringHierarchyBodyModelBuilder);

        JProperty prop = new JProperty("property", 8);
        JTokenType tok = JTokenType.Integer;

        Assert.Null(type.InvokeMember("_CheckJSONPropertyType",
            System.Reflection.BindingFlags.InvokeMethod |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Static, null, null,
            new object[2] { prop, tok }));
    }

    #endregion _CheckJSONPropertyType Tests



    #region Constructor Tests

    /// <summary>
    /// Test constructor with invalid filepath.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrowIOException()
    {
        Assert.ThrowsAny<IOException>(() =>
            new StringHierarchyBodyModelBuilder("invalid-filapath"));
    }

    /// <summary>
    /// Test constructor with valid filepath but invalid JSON specification.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrowJsonReaderException()
    {
        Assert.Throws<JsonReaderException>(() =>
            new StringHierarchyBodyModelBuilder(TEST_FILEPATH +
                "/Constructor_ShouldThrowJsonReaderException.json"));
    }

    /// <summary>
    /// Test constructor with invalid properties.
    /// </summary>
    [Theory]
    // Test invalid number of properties.
    [InlineData(TEST_FILEPATH +
        "/Constructor_ShouldThrowArgumentExceptionNumProps.json")]
    // Test missing required modifier array as first property.
    [InlineData(TEST_FILEPATH +
        "/Constructor_ShouldThrowArgumentExceptionNoReqs.json")]
    // Test required modifier property as not an array.
    [InlineData(TEST_FILEPATH +
        "/Constructor_ShouldThrowArgumentExceptionReqsNotArray.json")]
    public void Constructor_ShouldThrowArgumentException(string filepath)
    {
        Assert.Throws<ArgumentException>(() =>
            new StringHierarchyBodyModelBuilder(filepath));
    }
    
    /// <summary>
    /// Test constructor with normal inputs.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInit()
    {
        Type type = typeof(StringHierarchyBodyModelBuilder);

        StringHierarchyBodyModelBuilder builder =
            new StringHierarchyBodyModelBuilder(TEST_FILEPATH +
                "/Constructor_ShouldInit.json");

        Assert.Equal("StringHierarchyModelBuilder", builder.Name);

        // Set up expected _availableBaseRegions values
        StringHierarchyRegion root = new StringHierarchyRegion("root", null);

        StringHierarchyRegion region1 = new StringHierarchyRegion(
            "basebodyregion1", root, options: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "y", new HashSet<string> { "superior", "inferior" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            });

        StringHierarchyRegion existing;

        region1.AddSubregion(new StringHierarchyRegion("subregion1", region1,
            parentOptions: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "y", new HashSet<string> {
                    "proximal", "intermediate", "distal" } },
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            }), out existing);

        region1.AddSubregion(new StringHierarchyRegion("subregionn", region1,
            parentOptions: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "y", new HashSet<string> { "rostral", "middle", "caudal" } },
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            }), out existing);

        Dictionary<string, StringHierarchyRegion> baseRegions =
            new Dictionary<string, StringHierarchyRegion> { };

        foreach (string name in region1.OptionedRegionNames)
        {
            baseRegions.Add(name, region1);
        }

        foreach (KeyValuePair<string,StringHierarchyRegion> subregionPair
            in region1.Subregions)
        {
            foreach (string option in region1.Options)
            {
                baseRegions.Add(option + " " + subregionPair.Key,
                    subregionPair.Value);
            }
        }

        foreach (KeyValuePair<string, StringHierarchyRegion> entry
            in (Dictionary<string, StringHierarchyRegion>)
            type.InvokeMember("_availableBaseRegions",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.GetProperty,
            null, builder, null))
        {
            Assert.Equal(baseRegions[entry.Key].ToString(),
                entry.Value.ToString());
        }

        Assert.Equivalent(baseRegions.Keys, builder.AvailableModelNames);
    }

    #endregion Constructor Tests



    #region _ParseJSONBodyRegion Tests

    /// <summary>
    /// Test _ParseJSONBodyRegion (via constructor) with invalid regionJson
    /// body or missing modifiers.
    /// </summary>
    [Theory]
    // Test un-parseable regionJson body
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldThrowArgumentException.json")]
    //Test missing required mods
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldThrowArgumentExceptionReqMods.json")]
    public void ParseJSONBodyRegion_ShouldThrowArgumentException(
        string filepath)
    {
        Assert.Throws<ArgumentException>(() =>
            new StringHierarchyBodyModelBuilder(filepath));
    }

    /// <summary>
    /// Test _ParseJSONBodyRegion (via constructor) with valid JSON input which
    /// provokes a warning.
    /// </summary>
    [Theory]
    // Test invalid property
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldInitAndWarnInvalidProp.json")]
    // Test invalid modifier
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldInitAndWarnInvalidMod.json")]
    // Test duplicate subregions
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldInitAndWarnDuplicateSubregions.json")]
    public void Constructor_ShouldInitAndWarn(string filepath)
    {
        Type type = typeof(StringHierarchyBodyModelBuilder);

        StringHierarchyBodyModelBuilder builder =
            new StringHierarchyBodyModelBuilder(filepath);

        // Set up expected _availableBaseRegions values
        StringHierarchyRegion root = new StringHierarchyRegion("root", null);

        StringHierarchyRegion region1 = new StringHierarchyRegion(
            "basebodyregion1", root, options: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "y", new HashSet<string> { "superior", "inferior" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            });

        StringHierarchyRegion existing;

        region1.AddSubregion(new StringHierarchyRegion("subregion1", region1,
            parentOptions: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "y", new HashSet<string> {
                    "proximal", "intermediate", "distal" } },
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            }), out existing);

        region1.AddSubregion(new StringHierarchyRegion("subregionn", region1,
            parentOptions: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "y", new HashSet<string> { "superior", "inferior" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            }), out existing);

        Dictionary<string, StringHierarchyRegion> baseRegions =
            new Dictionary<string, StringHierarchyRegion> { };

        foreach (string name in region1.OptionedRegionNames)
        {
            baseRegions.Add(name, region1);
        }

        foreach (KeyValuePair<string, StringHierarchyRegion> subregionPair
            in region1.Subregions)
        {
            foreach (string option in region1.Options)
            {
                baseRegions.Add(option + " " + subregionPair.Key,
                    subregionPair.Value);
            }
        }

        foreach (KeyValuePair<string, StringHierarchyRegion> entry
            in (Dictionary<string, StringHierarchyRegion>)
            type.InvokeMember("_availableBaseRegions",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.GetProperty,
            null, builder, null))
        {
            Assert.Equal(baseRegions[entry.Key].ToString(),
                entry.Value.ToString());
        }

        Assert.Equivalent(baseRegions.Keys, builder.AvailableModelNames);
    }

    /// <summary>
    /// Test _ParseJSONBodyRegion (via constructor) with no options.
    /// </summary>
    [Fact]
    public void ParseJSONBodyRegion_ShouldInitNoOptions()
    {
        Type type = typeof(StringHierarchyBodyModelBuilder);

        StringHierarchyBodyModelBuilder builder =
            new StringHierarchyBodyModelBuilder(TEST_FILEPATH +
                "/ParseJSONBodyRegion_ShouldInitNoOptions.json");

        // Set up expected _availableBaseRegions values
        StringHierarchyRegion root = new StringHierarchyRegion("root", null);

        StringHierarchyRegion region1 = new StringHierarchyRegion(
            "basebodyregion1", root,
            modifiers: new Dictionary<string, HashSet<string>> {
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "y", new HashSet<string> { "superior", "inferior" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            });

        StringHierarchyRegion existing;

        region1.AddSubregion(new StringHierarchyRegion("subregion1", region1,
            modifiers: new Dictionary<string, HashSet<string>> {
                { "y", new HashSet<string> {
                    "proximal", "intermediate", "distal" } },
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            }), out existing);

        region1.AddSubregion(new StringHierarchyRegion("subregionn", region1,
            modifiers: new Dictionary<string, HashSet<string>> {
                { "y", new HashSet<string> { "rostral", "middle", "caudal" } },
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            }), out existing);

        Dictionary<string, StringHierarchyRegion> baseRegions =
            new Dictionary<string, StringHierarchyRegion> { };

        foreach (string name in region1.OptionedRegionNames)
        {
            baseRegions.Add(name, region1);
        }

        foreach (KeyValuePair<string, StringHierarchyRegion> subregionPair
            in region1.Subregions)
        {
            baseRegions.Add(subregionPair.Key, subregionPair.Value);
        }

        foreach (KeyValuePair<string, StringHierarchyRegion> entry
            in (Dictionary<string, StringHierarchyRegion>)
            type.InvokeMember("_availableBaseRegions",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.GetProperty,
            null, builder, null))
        {
            Assert.Equal(baseRegions[entry.Key].ToString(),
                entry.Value.ToString());
        }

        Assert.Equivalent(baseRegions.Keys, builder.AvailableModelNames);
    }

    /// <summary>
    /// Test _ParseJSONBodyRegion (via constructor) with both propagated and
    /// locally-defined options.
    /// </summary>
    [Fact]
    public void ParseJSONBodyRegion_ShouldInitNestedOptions()
    {
        Type type = typeof(StringHierarchyBodyModelBuilder);

        StringHierarchyBodyModelBuilder builder =
            new StringHierarchyBodyModelBuilder(TEST_FILEPATH +
                "/ParseJSONBodyRegion_ShouldInitNestedOptions.json");

        // Set up expected _availableBaseRegions values
        StringHierarchyRegion root = new StringHierarchyRegion("root", null);

        StringHierarchyRegion region1 = new StringHierarchyRegion(
            "basebodyregion1", root, options: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "y", new HashSet<string> { "superior", "inferior" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            });

        StringHierarchyRegion existing;

        region1.AddSubregion(new StringHierarchyRegion("subregion1", region1,
            options: new HashSet<string> {
                "suboptiona1", "suboptionan" },
            parentOptions: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "y", new HashSet<string> {
                    "proximal", "intermediate", "distal" } },
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            }), out existing);

        region1.AddSubregion(new StringHierarchyRegion("subregionn", region1,
            parentOptions: new HashSet<string> {
                "independentoptiona1", "independentoptionan" },
            modifiers: new Dictionary<string, HashSet<string>> {
                { "y", new HashSet<string> { "rostral", "middle", "caudal" } },
                { "x", new HashSet<string> {
                    "medial", "central", "lateral" } },
                { "z", new HashSet<string> { "anterior", "posterior" } }
            }), out existing);

        Dictionary<string, StringHierarchyRegion> baseRegions =
            new Dictionary<string, StringHierarchyRegion> { };

        foreach (string name in region1.OptionedRegionNames)
        {
            baseRegions.Add(name, region1);
        }

        foreach (KeyValuePair<string, StringHierarchyRegion> subregionPair
            in region1.Subregions)
        {
            foreach (string option in region1.Options)
            {
                if (subregionPair.Value.HasOptions)
                {
                    foreach (string name
                        in subregionPair.Value.OptionedRegionNames)
                    {
                        baseRegions.Add(option + " " + name,
                            subregionPair.Value);
                    }
                }
                else
                {
                    baseRegions.Add(option + " " + subregionPair.Key,
                    subregionPair.Value);
                }
            }
        }

        foreach (KeyValuePair<string, StringHierarchyRegion> entry
            in (Dictionary<string, StringHierarchyRegion>)
            type.InvokeMember("_availableBaseRegions",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.GetProperty,
            null, builder, null))
        {
            Assert.Equal(baseRegions[entry.Key].ToString(),
                entry.Value.ToString());
        }

        Assert.Equivalent(baseRegions.Keys, builder.AvailableModelNames);
    }

    #endregion _ParseJSONBodyRegion Tests



    #region TryCreate Tests

    /// <summary>
    /// Test TryCreate with specified region spec not in available base regions
    /// </summary>
    [Theory]
    // Test invalid region spec
    [InlineData("option1 option2 basebodyregion1")]
    // Test region spec not in available base regions
    [InlineData("option1 basebodyregion1")]
    public void TryCreate_ShouldReturnFalse(string regionspec)
    {
        StringHierarchyBodyModelBuilder builder =
            new StringHierarchyBodyModelBuilder(TEST_FILEPATH +
                "/Constructor_ShouldInit.json");

        IBodyModel model;

        Assert.False(builder.TryCreate(regionspec, out model));
    }

    /// <summary>
    /// Test TryCreate with specified region spec in available base regions
    /// </summary>
    [Fact]
    public void TryCreate_ShouldCreateAndReturnTrue()
    {
        StringHierarchyBodyModelBuilder builder =
            new StringHierarchyBodyModelBuilder(TEST_FILEPATH +
                "/Constructor_ShouldInit.json");

        IBodyModel model;

        Assert.True(builder.TryCreate("independentoptiona1 basebodyregion1",
            out model));

        Assert.NotNull(model);
    }

    #endregion TryCreate Tests
    
}