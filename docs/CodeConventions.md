# Coding Conventions
This document details some coding conventions - e.g., naming, style - to adhere to within this codebase to maintain cohesion and readability.

These guidelines are based on the following sources:
* [C# Corner - C# Naming Conventions](https://www.c-sharpcorner.com/UploadFile/8a67c0/C-Sharp-coding-standards-and-naming-conventions/)
* [Microsoft - Common C# Code Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
* [C# Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
* [C# Coding Standards and Naming Conventions](https://github.com/ktaranov/naming-convention/blob/master/C%23%20Coding%20Standards%20and%20Naming%20Conventions.md)

Disclaimer: The codebase does not currently fully adhere to these standards, but it does for the most part and the goal is to get it to adhere as completely as possible.


## Outline
1. [File Scope](#file-scope)
2. [Naming Conventions](#naming-conventions)
3. [Content Conventions](#content-conventions)
4. [Style Conventions](#style-conventions)


## File Scope
* Only include one class (or one primary class/construct with a few closely related but minor supporting classes/constructs) per file
* Name the file the same as the primary class it contains
* Use the file-based namespace convention (assumed since .NET 6), i.e., the namespace of a file directly matches the directory structure it lives in
* Similarly, use only one namespace per file to again make use of file-scoped namespacing
  * E.g.,
    ```
    namespace MyLibrary.MainTech;
    
    public class MyClass
    {
    ...
    }
    ```
    NOT
    ```
    namespace MyLibrary.MainTech
    {
      public class MyClass
      {
      ...
      }
    }
    ```

## Naming Conventions
### General
* Names should be concise but descriptive, longer if necessary to convey meaning
* Use alphanumeric characters and underscores only
* Use noun phrases to name structures (e.g., classes, interfaces)
* Acronyms/abbreviations
  * Do not use except for commonly accepted acronyms or abbreviations, such as ID, Xml, Msg, Min, Max, Num
  * If three or more letters, capitalize only the first letter (PascalCase), exceptions being for official names, such as HFI
* Use just an underscore '_' to name [discard variables](https://www.c-sharpcorner.com/blogs/c-sharp-hidden-gems-sharp1-discards-variable#:~:text=From%20version%207.0%2C%20C%23%20introduced,not%20creating%20a%20variable%20explicitly.)

### Specific
* Use PascalCase as a default. The following elements abide by this rule with the following clarifications/add-ons:
  * Namespaces: no underscores; use periods to separate components (e.g., HFI.Stimulators as the root namespace; *Note: this is not currently the case, but can be changed*)
  * Assemblies: name the same as the namespace if an assembly only has one namespace or has an entire self-contained root namespace
  * Classes, Structs, and Records: no underscores, leading "C"s or other class-denoting prefixes/suffixes; a leading capital "I" must be followed by a lowercase letter, else it looks like an interface
  * Interfaces: follow class naming conventions but start with a capital letter "I"
  * Abstract Classes: follow class naming conventions but end with the suffix "Base"
  * Delegates: follow class naming conventions; end the name with the word "Delegate"
  * Exceptions: follow class naming conventions; end the name with the word "Exception"
  * Enums: follow class naming conventions; do NOT end the name with the word "Enum"; the name should be singular not plural unless it represents a bit field (aka flag enum)
  * Properties: follow class naming conventions; do not use Get and Set in property names
  * Methods: follow class naming conventions; use verbs to convey function
* Event-related components:
  * Name with verbs since Events are actions
  * Use past or present tense as appropriate
  * Append the "EventHandler" suffix to the name of an event handler
  * Use two parameters named `sender` and `e` in an event handler
  * Name event argument classes with the "EventArgs" suffix
* For private, protected, or protected internal fields (not properties), use camelCase and prepend an underscore '_'
  * If the field is additionally static or thread static, use the prefix `s_` or `t_`, respectively 
* For private, protected, or protected internal methods, still use PascalCase but prepend an underscore '_'
* Use camelCase for local variables and constructor/method parameters
* Use ALL_CAPS and underscores for constants
   


## Content Conventions 
* Always explicitly declare visibility (e.g., `public`, `private`, `protected`, `protected internal`, etc)
* Remove use of magic numbers whenever possible in favor of a variable (e.g., constant) with a descriptive name
  * Exclusions are almost exclusively limited to integer values `-1`, `0`, `1`, and `2`, used for decrementing/incrementing, positivity checks (i.e., > 0), duplication, integer true/false
    notation, or nullity checks (although explicit `true`/`false` `bool` values or keyword `null` should be used instead when possible)
* Use language keywords for types instead of runtime types when possible (e.g., `string` not `System.String`, `int` not `System.Int32`, `object` not `System.Object`)
* Use `&&` and `||` comparison operators instead of `&` and `|` operators to avoid runtime errors (e.g., an `&&` will evaluate clauses in order and terminate early
 if a clause evaluates to false, potentially avoiding divide-by-0 or null-pointer errors)
* Use concise forms of object/field instantiation, i.e., `var` and `new()` instead of explicit type declaration, when the type can be inferred from the expression/context
* Consider using the [`out` parameter keyword](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/out-parameter-modifier) as a way to initialize variables directly from a function 'return' value
* Put all `using` statements (directives) before the `namespace` declaration
* For clarity/readability, use `this.` notation when calling a class's own variables and functions even though removing it would not change functionality
* Use [documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments) for each class
  and method (including tests), including at minimum the `<summary>` tag and - when relevant - the `<param name=''>`, `<returns>`, and `<exception>` tags



## Style Conventions
* Use [Allman style](https://en.wikipedia.org/wiki/Indentation_style#Allman_style) indentation: open and closing brace its own new line.
  Braces line up with the current indentation level
* **Use 4 spaces instead of tabs (You can set this as a setting in your IDE. See how to do in VS
 [here](https://learn.microsoft.com/en-us/visualstudio/ide/reference/options-text-editor-csharp-formatting?view=vs-2022))**
  * Set your tab stop to be 4 spaces
* Write one statement/declaration per line (e.g., NOT `var x = 5; var y = 7;`)
* Separate methods within the same container (e.g., class) by 1 empty line
* Separate separate code elements in a file (e.g., `using` statement block from `namespace` from the class(es)) by 2 empty lines
* Lines should be no longer than 80 characters (this is not too strict, but try to stay close to 80 max)
  * If continuation lines aren't indented automatically, indent them one tab stop (four spaces)
  * Split lines at a logical point, e.g., after a comma, after an operator, before the next dot-notation item
  * Example:
  ```
  public SortedSet<string> FixedStimParams =>
      new(this._StimParamsAvailable.Except(this.ModulatableStimParams));
  ```
* Use `//` comments for short, single-line comments, usually placed above the line(s) of code it refers to
  * If placing comments in-line, include at least one tab of whitespace between the end of the code line and the beginning of the comment, and align in-line
   comments if on multiple subsequent lines
  * Avoid use of `/* */` comments
  * Include a space after the `//` and before the comment text
  * Begin the comment with a capital letter and end with a period
* Use parentheses to make clauses in an expression apparent, e.g., `if ((x > min) && (x < max))`
  * Put a space between the expression and the preceding `if`, `while`, or similar keyword
* Use spaces between elements for readability, e.g.:
  * `x > min` NOT `x>min`
  * `myArray[i + 1]` NOT `myArray[i+1]` (unless removing those spaces makes `myArray[i+1]` appear as a more cohesive and distinct element among a much larger
    expression, e.g., `(myArray[i+1] + 2) > threshold`)
  * `param1, param2, ..., paramN` NOT `param1,param2,...,paramN`

