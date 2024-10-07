# Unified Modeling Language (UML)
UML is a graphical way to define and describe object-oriented software systems. More specifically, the [Object Management Group (OMG)](https://www.omg.org/about/index.htm) specification states:

*"The Unified Modeling Language (UML) is a graphical language for visualizing, specifying, constructing, and documenting the artifacts of a software-intensive system. The UML offers a standard way to write a system's blueprints, including conceptual things such as business processes and system functions as well as concrete things such as programming language statements, database schemas, and reusable software components."*

We are using a small subset of UML model and diagram types to convey the structure and function of this stimulator library. Briefly outlined below are descriptions of the most relevant types of models and diagrams, along with the key relationships and notations to understand.



## Table of Contents
* [Object-Oriented Background](#object-oriented-background)
* [Common UML Models](#common-uml-models)
  * [The Logical or Class Model](#the-logical-or-class-model)
    * [Class Diagram](#class-diagram) (**main diagram, TL;DR focus here**)
    * [Object Diagram](#object-diagram)
  * [The Use Case Model and Diagram](#the-use-case-model)
  * [The State or Dynamic Model](#the-state-or-dynamic-model)
    * [Scenario or Sequence Diagram](#scenario-or-sequence-diagram)
    * [Activity Diagram](#activity-diagram)
  * [The Component Model](#the-component-model)
    * [Package Diagram](#package-diagram)
    * [Component Diagram](#component-diagram)
  * [The Physical Deployment Model](#the-physical-deployment-model)
* [Our UML Diagrams](#our-uml-diagrams)
* [Sources and Resources](#sources-and-resources)


## Object-Oriented Background
Before diving too deep into UML, a solid understanding of core object-oriented programming (OOP) concepts is crucial. Below are some key terms and concepts.
* An **object** is an **instance** of a **class**. This is critical to understand. A class is the static template or pattern from which objects are constructed, or instantiated, at run-time. You may also hear an object simply referred to as an **instance**.
* **[Inheritance](https://stackify.com/oop-concept-inheritance/)**: The principle that classes can be arranged in a hierarchy, much like a "family tree" where **derived, child, or subclasses** *inherit* some attributes and capabilities from their **parent or superclass**. Child classes can then *extend* their inherited abilities to do more than the superclass.
* Other words you should be familiar with: **attributes (state), methods (behavior), constructor (instantiation, initialization), abstraction, inheritance, encapsulation, polymorphism, overloading, overriding**
  * [Helpful terminology link 1](https://homepages.uc.edu/~thomam/Misc/OO_Terminology.html)
  * [Helpful terminology link 2](https://www.indeed.com/career-advice/career-development/oop-terminology)
  * [Helpful terminology link 3](https://www.d.umn.edu/~gshute/softeng/object-oriented.html)
  * [Helpful terminology link 4](https://www.eeng.dcu.ie/~ee553/ee402notes/html/ch01s04.html)
* For more on OOP _design_ principles, see the [SOLID principles](https://www.freecodecamp.org/news/solid-principles-explained-in-plain-english/)



## Common UML Models

### [The Logical or Class Model](https://sparxsystems.com/resources/tutorials/uml/logical-model.html)
The logical, or class, model is a static model of the system (different from the keyword `static` that's in some languages), mainly conveying the internal structural elements and their relation to each other. This model is at the core of object-oriented systems as it is fundamentally a class specification, detailing "the pattern from which objects will be produced at run-time."
These models and diagrams focus on detailing class attributes, methods (behaviors), inheritance, and other relationships.
#### [Class Diagram](https://sparxsystems.com/resources/tutorials/uml2/class-diagram.html)
**This is the main diagram to understand, as it is a predominant starting place - if not ending place - for smaller team UML use.**
* Class Notation: classes are notated by a box with 3 compartments: a name compartment (top), an attributes compartment (middle), and a method compartment (bottom).
  * Attributes are specified as such:
    * _\<accessibility> \<name> : \<type>_
    * e.g., `+ firstName : string`
  * Methods are listed only as their signatures:
    * _\<accessibility> \<name>(\<param1Type>, ...) : \<returnType>_
    * e.g., `+ SetName(string) : void`
  * The accessibility (i.e., public, private, etc.) of each attribute and method is indicated using the following graphical notations:
    * public: `+`
    * private: `-`
    * protected: `#`
    * internal: `~`
    * protected internal: `#~`
  * Some additional annotations clarify some characteristics of attributes and methods:
    * `<<create>>` denotes a constructor
    * static elements are underlined
    * specific to C#, properties are denoted with the `<<property>>` tag before the property name
    * readonly fields are followed by a `(readOnly)` annotation. Note: Properties marked with `(readOnly)` do not actually have the `readonly` keyword (they cannot), but they are effectively `readonly` because only the `get` method is allowed/implemented.
      * e.g., `public int MyID { get; }`
      * Properties that do have a `set` method but a different access modifier than the `get` method are noted with the two corresponding access modifier symbols (separated by a comma ',') preceding the `<<property>>` tag
        * e.g., `public int MyID {get; protected set; }` --> `+,# <<property>> MyID : int`
        * Note: this specific notation element/adaptation was devised for this project and is not known to be standard anywhere
    * Inner classes can be denoted in one of two ways:
      * Brief version: listed just like any other attribute in the middle compartment of its outer class box, but with a `<<class>>` tag before the inner class name
      * Detailed version: as its own class box with a special association line going to its outer class (see the bottom of the Class Diagram tutorial page linked above)

<div align="center" >
 <img alt="UML Class Notation" src="./docs/images/class_notation-tutorialspoint.jpeg" width="500" />
 <p><i>UML class notation (Image Source: TutorialsPoint)</i></p>
</div>

* Abstract Classes and Inheritance/Generalizations
  * Anything abstract, whether it be the name of a class, attribute, or method, is indicated in _italics_.
  * Abstract classes are composed of the same 3-compartment boxes as normal classes.
  * **Inheritance**, also called **generalization**, is denoted by a solid-lined, white-arrowhead arrow going from subclass to superclass.
  * Subclasses do not need to re-specify inherited attributes or methods, only additional or previously abstract, now-implemented methods and properties.
* Interfaces and Implementation/Realization
  * Interfaces are denoted similarly to classes but with only a 2-compartment box - since they cannot have their own attributes - containing the interface name and method signatures.
  * The tag `<<interface>>` is included above the interface name to denote it is an interface.
  * The **implementation** (also known as **realization**) relationship is denoted very similarly to inheritance - white-arrowhead arrow going from implementing to implemented class - but with a dashed rather than solid line.
* [Association, Aggregation, and Composition](https://www.visual-paradigm.com/guide/uml-unified-modeling-language/uml-aggregation-vs-composition/)
  * **[Association](https://techvidvan.com/tutorials/java-association/)**: a relationship between two separate classes formed if the two classes need to communicate with each other at run-time.
    * General association, rather than more specific kinds discussed below, are usually bidirectional.
    * Association is notated by a solid line connecting the two classes involved.
    * Annotations at either end of the solid line indicate the **multiplicity** of the association. The annotations can be read in either direction (left-to-right usually annotated on top of the line, right-to-left below) to describe the relationship from either class's perspective.
      * E.g., "One student can learn from many instructors."
      * E.g., "One instructor can teach many students."
    * There are 4 kinds of multiplicity relationships, annotated as such:
      * **one-to-one**:    `1________1`
      * **one-to-many**:   `1________1..*`
      * **many-to-one**:   `1..*________1`
      * **many-to-many**:  `1..*________1..*`
    * Additional text descriptors can be annotated on the association line to provide detail about the relationship (e.g., "learns from" or "teaches").

<div align="center" >
 <img alt="Association Multiplicities" src="./docs/images/association_multiplicity_concept-techvidvan.png" width="500" />
 <p><i>Concept of association multiplicity relationships (Image Source: TechVidvan)</i></p>
</div>

  * **Aggregation**: represents a unidirectional, "has-a", _weak_ association relationship where both entities (classes) involved can exist independently of each other (e.g., an Institute has Students)
  * **Composition**: represents a "part-of" or "made-of", _strong_ association relationship where there is a close dependency and the composed class cannot exist without its composing parts (e.g., a Vehicle is made of an Engine and Tires)

<table align="center">
 <tr>
  <td>
   <div align="center" >
    <img alt="UML Relationship Notations" src="./docs/images/uml_arrows-stack_overflow.png" height="300" />
    <p><i>UML relationship notations (Image Source: Stack Overflow)</i></p>
   </div>
  </td>
  <td>
   <div align="center" >
    <img alt="Association Multiplicity Annotations" src="./docs/images/association_multiplicity_annotations-vertabelo.png" height="300" />
    <p><i>Association multiplicity annotations (Image Source: Vertabelo)</i></p>
   </div>
  </td>
 </tr>
</table>



#### [Object Diagram](https://sparxsystems.com/resources/tutorials/uml2/object-diagram.html)
Object diagrams are a special case of class diagrams that explicitly show run-time class instances, or objects, to illustrate an example run-time state. For more details, see the linked page.



### [The Use Case Model](https://sparxsystems.com/resources/tutorials/uml/use-case-model.html)
The use case model describes the proposed functionality of the system, so without conveying much about the internal structure or function of the system, these models/diagrams show the system as a user would interact with it. Use case models should capture the requirements and constraints of the system, example scenarios of its use, and any actors involved. Any given system needs multiple use case diagrams to illustrate each different exemplary use scenario.
Some terms:
* **Actor**: any human or machine entity external to the system that interacts with the system in some way to perform some work to achieve a goal.
  * The set of use cases an actor is involved in defines their role in relation to the system.
  * An actor is notated by a stick figure.
  * Actors can be of general or more specific types and follow inheritance specialization schemes, much like classes would (e.g., Customer vs VipCustomer). Diagrammatically, this "inheritance" relationship is denoted using the same directional arrow as class inheritance is denoted, just between actors (stick figures) rather than classes (boxes).
* **Requirements**: formal functional specifications the system must provide to the end user(s) (actors involved) in a given use case
* **Constraints**: formal rules and limitations the use case operates under, including **pre-conditions**, **post-conditions**, and **invariants** (for more details, see the Use Case Model link above)
* **Scenarios**: formal, sequential descriptions of steps taken or flow of events to carry out the use case
* **Includes Relationship**: a use case can _include_ the functionality of another part of the system as part of its normal processing. This is a way to factor out common behavior and not have to duplicate low-level system functionality in higher-level use cases (both in diagrams and in actual implementation!)
* **Extends Relationship**: a use case can extend the behavior of another, e.g., for edge case scenarios

#### [Use Case Diagram](https://sparxsystems.com/resources/tutorials/uml2/use-case-diagram.html)
The use case diagram is essentially what has been described above: a diagram of system _function_ in a _specifically defined use case scenario_, including actors, key functional steps of the system, and the associations between actor(s) and functional step(s).



### [The State or Dynamic Model](https://sparxsystems.com/resources/tutorials/uml/dynamic-model.html)
Dynamic models express system state and behavior over time and are somewhat of an extension or elaboration of use case diagrams. There are 3 main kinds: sequence, activity, and state diagrams.

#### [Scenario or Sequence Diagram](https://sparxsystems.com/resources/tutorials/uml2/sequence-diagram.html)
Sequence diagrams, or scenario diagrams (as in use case scenario), depict interaction and workflow (e.g., of a use case), graphically portraying the _sequence_ of functional steps at run-time, often showing actor or **object lifelines** (activity throughout their lifespans) running vertically down the page. These diagrams are particularly valuable for communicating:
* temporal characteristics of the system during real, run-time functioning
  * execution occurrences
  * synchronous vs asynchronous processes
  * activations, focus, holds, or transitions of control
  * duration and time constraints
  * loops
* roles and responsibilities of different objects
* information passed between objects
  * message content
  * sender and receiver roles
  * can also illustrate recursion or communication via methods in the same class/object
* functional dependencies
* object life start and end

#### [Activity Diagram](https://sparxsystems.com/resources/tutorials/uml2/activity-diagram.html)
Activity diagrams provide a more internal view of system workflow, including their activation, logical conditions or checkpoints along the way, different decision paths, and their ultimate termination points. Some terms:
* **Activity**: the specification of a parameterized sequence of behavior, notated via a round-cornered rectangle enclosing all actions, control flows, and other composing elements
* **Action**: a single step within an activity, also denoted by round-cornered rectangles
  * Actions can have constraints (e.g., pre- or post-conditions) attached to the action box
* **Control flow** is the flow from one action to the next, denoted by an arrow
* Important logic steps are also depicted, including:
  * **Decisions**: multi-input, condition-based outcomes
  * **Merges**: points at which previously branched logical paths return to the same single path forward, regardless of previous decision(s)
  * **Forks** and **joins**: the start and end of concurrent actions, _different_ from decisions and merges which are the start and end of either/or actions
* Activity diagrams can also convey interrupting logic and actions

#### [State [Machine] Diagrams](https://sparxsystems.com/resources/tutorials/uml2/state-diagram.html)
State diagrams illustrate the state and behavior of a single object over time, including start and end points, transitions, actions available in each state, and logical dependencies in state progression.



### [The Component Model](https://sparxsystems.com/resources/tutorials/uml/component-model.html)
Component models return to portraying the structural composition of software systems. **Components** are high-level aggregations of smaller software pieces. They are meant to encapsulate lower-level class or other diagrams, providing more of a "black box" construction of the system and showing only the high-level interface that will be exposed.
Components are drawn as rectangles with their name and two smaller rectangular tabs jutting out from the top left corner (to distinguish from other rectangular diagram representations). Key elements of the exposed interface (e.g., high-level methods) are noted with left-jutting pin-tabs and the name of the method.

#### [Package Diagram](https://sparxsystems.com/resources/tutorials/uml2/package-diagram.html)
Package diagrams show the organization of software packages and their elements. They can represent namespaces instead if chosen to represent class rather than package elements. Package diagrams most commonly organize class diagrams or use cases. They can also communicate package imports and other connections.

#### [Component Diagram](https://sparxsystems.com/resources/tutorials/uml2/component-diagram.html)
Component diagrams illustrate components, the largest building blocks of the system (again, at a higher level of abstraction than class diagrams) and the assembly connectors that "link" them through provided interfaces. Similar to package diagrams, they define boundaries and are used to group elements into logical structures, the difference being component diagrams are based more on semantic rather than actual implementation organization. Component diagrams depict only private elements while package diagrams depict only public elements.



### [The Physical Deployment Model](https://sparxsystems.com/resources/tutorials/uml/physical-models.html)
The deployment model depicts how the real, physical system will be deployed across the system infrastructure. This includes the distinction of different servers, web-based platforms, local computers, other hardware components, etc. to show how this system will manifest and be deployed in real life.



## Sources and Resources
The information in this document is additionally based on the following resources. They also contain further information not repeated here if you want to learn more.
* [Sparx Systems - Tutorials](https://sparxsystems.com/resources/tutorials/uml-tutorials.html) (particularly [Part 1](https://sparxsystems.com/resources/tutorials/uml/part1.html))
* [TutorialsPoint - UML Quick Start Guide](https://www.tutorialspoint.com/uml/uml_quick_guide.htm)
* [Visual Paradigm - UML Tutorial](https://www.visual-paradigm.com/guide/uml-unified-modeling-language/uml-class-diagram-tutorial/)
* [Lucidchart - Class Diagram Tutorial](https://www.lucidchart.com/pages/uml-class-diagram)
* [Additional Association Clarifications](https://stackoverflow.com/questions/1874049/explanation-of-the-uml-arrows)
