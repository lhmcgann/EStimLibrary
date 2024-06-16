namespace EStimLibrary.Core.SpatialModel;


//public interface ILocationBase : ISelectable
//{ }

public interface ILocation : ISelectable
{
    //bool Equals(LocationType other);
    bool IsLocationCompatible(ILocation location);
}


// Thanks to the help of ChatGPT:
// In C#, you can use covariance and contravariance with generics to achieve a
// more precise type match between the interface methods and the implementing
// class. (see my ChatGPT chat of google for what this is and how to do it).
// My understanding:
// If using generics without variance modifiers (in or out), the interface or
// class or whatever is defined as invariant in T, meaning you always have to
// supply specifically T, not a DerivedT when BaseT is expected, nor BaseT when
// DerivedT is expected.
// Using the 'out' modifier, e.g., ILocation<out T>, makes ILocation covariant
// in T, meaning whenever T is returned you can return a more DerivedT. I.e.,
// even though only BaseT is expected, you can return a more specific DerivedT
// object, BUT only in the context of returns.
// Using the 'in' modifier, e.g., ILocation<in T>, makes ILocation contravariant
// in T, meaning whenever T is accepted you can accept a BaseT. I.e., even
// though DerivedT is expected, you can accept a less specific BaseT object, BUT
// only in the context of function parameters.
// TL;DR:
// 'in T'  --> accept BaseT when expecting DerivedT
// 'out T' --> return DerivedT when expecting BaseT

// However, in your specific case, because you're trying to ensure that
// the generic type parameter for IArea<T> and ILocation<T> is exactly the
// derived type, it is not possible to enforce this constraint directly through
// generics.
// This design uses self-referencing generics to enforce a more precise type
// relationship between the interface methods and the implementing class. It
// provides a way to express that IArea and ILocation are parametrized by their
// own derived types.

//public interface ILocation<T> where T : ISpatialDataType
//{
//}

//public interface ILocation<TLocation>
//    where TLocation : ILocation<TLocation>
//{
//}

//// An interface just to alias the full generic name.
//public interface ILocation : ISpatialReferenceFrame<ISpatialDataType>.
//    ILocation<ISpatialDataType>
//{

//}

//public interface ILocation<in T> where T : ISpatialDataType
//{
//}