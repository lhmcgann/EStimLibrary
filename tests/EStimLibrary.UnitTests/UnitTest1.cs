using Xunit.Abstractions;


namespace EStimLibrary.UnitTests;


public class UnitTest1
{
    private readonly ITestOutputHelper _output;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        this._output = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        this._output.WriteLine("This should print.");
        Assert.True(true);
    }
}