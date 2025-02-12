# EStimLibrary
A C# library to facilitate electrical stimulation research and development.

## Table of Contents
* [Dependencies](#dependencies)
    * [Extensions for VSCode Users](#extensions-for-vscode-users)
* [Repo Contents](#repo-contents)
    * [Top-Level Structures](#top-level-structure)
    * [Projects and Solutions](#projects-and-solutions)
* [Library Structure](#library-structure)
* [UML Diagrams](#uml-diagrams)
* [Usage](#usage)
    * [VisualStudio](#visualstudio)
    * [VSCode or your other preferred IDE](#vscode-or-your-other-preferred-ide)

## Dependencies
* .NET 9.0
    * 10-minute [Tim Corey video](https://www.youtube.com/watch?v=sXEsvqCCTTc): how to upgrade or install fresh
    * Windows: Use VisualStudio
    * MacOS, Linux: download and install manually to access `dotnet` command-line interface (CLI)
* `nuget` packages:
    * For base library:
        * System.IO.Ports
        * MathNet.Numerics
        * Newtonsoft.Json
    * For xUnit testing project:
        * Microsoft.NET.Test.Sdk
        * xunit
        * xunit.runner.visualstudio
        * xunit.runner.console
        * coverlet.collector
    * Versions are listed in the `.csproj` files and should be pulled automatically when building projects and solutions in this repo.

### Extensions for VSCode Users
Optional but highly recommended:
* C# Dev Kit: provides much of the same C# support and within-solution navigation features as VisualStudio

## Repo Contents
### Top-Level Structure
* `src/` - the library source code project
* `tests/` - the test project(s)
* `models/` - examples of spatial model definition files
    * contains [`EStimLibrary.SpatialModels`](https://github.com/lhmcgann/EStimLibrary.SpatialModels/) as a submodule
* `examples/` - example applications (projects) using the library's high-level API
* `docs/` - additional documentation files, including a dictionary and coding conventions

### Projects and Solutions
There are currently 3 projects in this repo:
1. `src/EStimLibrary/EStimLibrary.csproj`: the base library project itself
    * Builds into a `.dll`
2. `tests/EStimLibrary.UnitTests/EStimLibrary.UnitTests.csproj`: the xUnit test project
    * The unit tests for the library
3. `examples/EStimLibrary.ConsoleAppDemo/EStimLibrary.ConsoleAppDemo.csproj`: an example console app project that either runs through a hard-coded session config and execution, or walks through the config step-by-step, asking for user input at each step
    * Highlights the `Utils` reflection capabilities,
    * and exemplifies the major `HapticSession` config components.

There are 3 corresponding solutions in this repo:
1. `EStimLibrary.sln`: builds only the base library project
2. `tests/EStimLibrary.UnitTests/EStimLibrary.UnitTests.sln`: builds the base library and the xUnit test projects
3. `examples/EStimLibrary.ConsoleAppDemo/EStimLibrary.ConsoleAppDemo.sln`: builds the base library and the example console app projects

## Library Structure
The main library, in `src/EStimLibrary/`, is broken down into two top-level folders:
* `Core/`: the base declarations and implementatsions of interfaces, abstract classes, and core classes
* `Extensions/`: provided implementations of some core interfaces and abstract classes that are commonly used

The `Core/` folder contains the following structure:
* `Data/`: primarily `IDataLimits`
* `Haptics/`: classes on the haptic event side of the pipeline, e.g., `HapticSession`, `HapticEvent`, and `HapticTransducer`
* `HardwareInterfaces/`: leads, cables, neural interfaces
* `SpatialModel/`: location, area, body model, and related content
* `Stimulation/`: stimulators and stimulation representations
* Miscellaneous infrastructure interfaces and classes are in the top level of the `Core/` folder, e.g., `Utils`, `ResourceManager`, `ReusableIdPool`

The `Extensions/` folder mimics this structure to the extent that example implementations are provided.

The `EStimLibrary.UnitTests` project also mimics the `Core/`, `Extensions/`, and sub-directory structures.

## UML Diagrams
Last updated: February 2025
* [Class Diagram - High-Level](https://lucid.app/lucidchart/1366a885-086c-45d0-8763-63448fe11a86/edit?viewport_loc=-3539%2C-4330%2C16143%2C7727%2C0_0&invitationId=inv_10ca78f7-8fcc-4d34-8466-aa8da86d8716)
* [`StringHierarchy` Class Diagram](https://lucid.app/lucidchart/8f0c6f70-c343-444e-9e6f-5c51557384ec/edit?viewport_loc=-2888%2C-1102%2C7382%2C3534%2C0_0&invitationId=inv_7e104f4d-b695-4ff7-84b2-87b3bff9a9fb)
* [Sequence Diagrams](https://lucid.app/lucidchart/312bfb4e-807b-4f90-9e56-7cdc64a0172d/edit?viewport_loc=-858%2C313%2C2104%2C917%2C0_0&invitationId=inv_94e9dfe8-1656-45c9-abc7-d433c77d7a95)
* [Use Case Diagrams](https://lucid.app/lucidchart/85097f76-39d7-49a4-b1a6-5fa749e0ad9f/edit?viewport_loc=-3588%2C-303%2C10396%2C4532%2C0_0&invitationId=inv_c1be5bbe-3b3e-4b08-b021-a66debdfbed8)

Beneficial in the future:
* package diagram showing the interface exposed to the public
* use case, sequence, and/or activity diagram(s) depicting how this codebase interacts with a calling UI, the different objects instantiated at run-time, the function calls and messages passed between, the synchronous vs asynchronous functioning, etc
* class diagram of the robot and UI code (in their respective repos if not included here)
* deployment model of the whole networked system

See [UML.md](./docs/UML.md) for a UML intro.


## Usage
To use the library, clone the repo and checkout the `release-v2.0` branch for the most recent development:
```
# To clone submodules as well: call git clone with the --recurse-submodules option if you have Git 2.13 or later, else --recursive
git clone git@github.com:lhmcgann/EStimLibrary.git
cd EStimLibrary
git checkout release-v2.0
```

### VisualStudio
1. Open the `.sln` file corresponding to the projects you want to open. 
2. Press the "play" button in the top left to build (and run, if applicable) the loaded solution.
3. To run the test project, click the green "play" button in Test Explorer.

### VSCode or your other preferred IDE
1. Open the `EStimLibrary/` folder in your workspace.
2. In Terminal, navigate to the folder with the desired project or solution.
3. Use `dotnet` CLI commands. See a cheatsheet [here](https://www.lostindetails.com/articles/dotnet-cheatsheet).
    1. `dotnet build` to build any project
    2. `dotnet run` to run the example console app
    3. `dotnet test` to run the xUnit test suite


