using EStimLibrary.Extensions.SpatialModel.StringHierarchy;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace EStimLibrary.UnitTests.Extensions.SpatialModel.StringHierarchy;


// Test class naming convention: LibClassTests
public class StringHierarchyBodyModelBuilderTests
{
    private readonly ITestOutputHelper _output;

    protected const string TEST_FILEPATH =
        "./../../../Extensions/SpatialModel/StringHierarchy" +
        "/StringHierarchyBodyModelBuilderTestFiles";

    // Test class constructor creates an output helper so can write console
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
    /// Test _CheckJSONPropertyType with matching types.
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
                "/Constructor_ShouldThrowJsonReaderException.txt"));
    }

    /// <summary>
    /// Test constructor with invalid properties.
    /// </summary>
    [Theory]
    // Test invalid number of properties
    [InlineData(TEST_FILEPATH +
        "/Constructor_ShouldThrowArgumentExceptionNumProps.txt")]
    // Test no required modifier array as first property
    [InlineData(TEST_FILEPATH +
        "/Constructor_ShouldThrowArgumentExceptionNoReqs.txt")]
    // Test required modifier property not an array
    [InlineData(TEST_FILEPATH +
        "/Constructor_ShouldThrowArgumentExceptionReqsNotArray.txt")]
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
                "/Constructor_ShouldInit.txt");

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

    /// <summary>
    /// Test _ParseJSONBodyRegion (via constructor) with invalid regionJson
    /// body or missing modifiers.
    /// </summary>
    [Theory]
    // Test un-parseable regionJson body
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldThrowArgumentException.txt")]
    //Test missing required mods
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldThrowArgumentExceptionReqMods.txt")]
    public void ParseJSONBodyRegion_ShouldThrowArgumentException(
        string filepath)
    {
        Assert.Throws<ArgumentException>(() =>
            new StringHierarchyBodyModelBuilder(TEST_FILEPATH +
                "/ParseJSONBodyRegion_ShouldThrowArgumentException.txt"));
    }

    /// <summary>
    /// Test _ParseJSONBodyRegion (via constructor) with valid JSON input which
    /// provokes a warning.
    /// </summary>
    [Theory]
    // Test invalid property
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldInitAndWarnInvalidProp.txt")]
    // Test invalid modifier
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldInitAndWarnInvalidMod.txt")]
    // Test duplicate subregions
    [InlineData(TEST_FILEPATH +
        "/ParseJSONBodyRegion_ShouldInitAndWarnDuplicateSubregions.txt")]
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
                "/ParseJSONBodyRegion_ShouldInitNoOptions.txt");

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
                "/ParseJSONBodyRegion_ShouldInitNestedOptions.txt");

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

    /// <summary>
    /// Test TryCreate...
    /// </summary>
}