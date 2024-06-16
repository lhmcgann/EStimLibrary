namespace EStimLibrary.Core;

/// <summary>
/// A type to represent a library error. Can be implemented to create different
/// error types - with according string error messages - of this library.
/// </summary>
public interface Error
{
    public string GetErrorMsg();
}

// TODO: how to have a common prefix for "HFI Stimulator" lib Errors?
//public static string ErrorPrefix = "HFISE";

//public enum ResourceErrorCode
//{

//}