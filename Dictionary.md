# Dictionary of The Domain: A Software Library for Electrotactile Stimulation
Last Updated by Laura McGann on 12/17/2023.

## Hardware Terms
* **Stimulator**: The electrical stimulation waveform generator, including current or timers, voltage drivers, and switching matrices.
* **Output**: A physical connection point, i.e., pin, on the exterior of a physical stimulator.
* **Current Driver**: A physical component internal to stimulators that drives current and can act as a current source or sink.
* **Output Configuration**: The assigned mapping between a set of stimulator outputs and current drivers, namely determining if each output is connected to - and thus acts as - a current **source** or **sink**.
    * An output configuration must contain at least two outputs: one configured as a source, and one configured as a sink.
    * There may be more than one output configured as a source or a sink in the same configuration, depending on the capabilities of the stimulator.
    * Hardware limitation: `2 <= count(outputs assigned as sources) + count(outputs assigned as sinks) <= total number of current drivers in the stimulator`
* **Contact**: The smallest divisible physical electrically conductive piece contacting biological tissue. Most often, this will be a single metal piece or a gel pad or similar.
* **Contact Configuration**: The specific placements of each contact in a contect set on locations on a wearer's body or a body/limb model.
* **Neural Interface**: A wearable that holds contacts. Conceptually, this term can refer to a single physical neural interface unit (e.g., a single ring with 2 contacts) and/or the aggregate collection of physical neural interface units (e.g., 5 rings with 2 contacts each yields a "net" neural interface of 10 contacts). Programmatically in this library, a neural interface refers only to the former: a single physical neural interface unit, or a single indivisible piece of wearable, contact-containing hardware.
* **Lead**: A physical and electrical connection - as well as conceptual correlate - between outputs and contacts, potentially in a many-to-many relationship, but at least one-to-one. A lead is equivalent to 1 independent electrical system, and can be modeled as a complete bipartate graph (as in graph theory) where the two sets are the output set and the contact set.
* **Cable**: The physical collection of all leads. May be an explicit part-numbered cable, for example.
* **Hardware Configuration**: The entirety of a specific hardware setup, including selections of stimulators, neural interfaces, contact placement, cables, and leads.
* **Interface Configuration**: The mapping information yielded by the entirety of a specific hardware setup, namely the number of contacts, contact locations, and contact-output connections created by specific wiring of the cable/leads. _(TODO: we need a better name for this)_

In summary, **stimulator outputs** are physically and electrically connected to the **contacts** of a **neural interface** by the set of **leads** in a **cable**. The **output configuration**, or mapping of outputs to **current drivers** usually via a switching matrix, is set internal to the stimulator and may or may not be programmatically changeable, depending on the stimulator. Contacts are placed at certain locations on a real or modeled body when the neural interface is worn, and this placement set is the **contact configuration**.

## Stimulation Terms
* **Phase**: The smallest, independently executable time period of charge injection. Phases are bounded by two conditions:
    1. A phase must be of a single **polarity**, meaning its amplitude cannot cross 0. Such a desired cross must elicit a new phase.
    2. In other words, a phase consists of a single **output configuration**. Desired stimulation that requires a different output configuration, i.e., a different assignment of outputs as current sources and sinks, must elicit a new phase.
    * So, a phase is comprised of two elements:
        1. The stimulation parameter data to elicit the desired **phase shape**, such as **width** (us) or **amplitude** (mA). NOTE: The actual achievable phase shape is limited by the stimulator hardware itself, e.g., rounded rather than sharp corners, or a limited number of discrete amplitude steps depending on the desired parameters and the stimulator's timer and current driver resolutions.
        2. The output configuration through which the stimulation is executed. NOTE: The sum of amplitudes across the set of source outputs and the set of sink outputs must be equal to 0 at all times, becuase that's how electricity works.
* **Feature**: Parameterizable subcomponents of a phase that - taken together in immediate sequence - form the **phase shape**.
    * By definition of a phase, all features in the same phase must be of the same polarity as all other features in the phase.
* **Pulse** (aka **waveform**): A series of phases over time.
    * Functionally, it is the smallest stimulation artifact meant to excite an axonal population and elicit a percept.
    * Phases in a pulse are spaced by **inter-phase delays** (**IPDs**, us).
        * Hardware limitation: must be greater than or equal to the time it takes the stimulator to switch output configurations.
    * E.g., a cathodic-first, charge-balanced, biphasic square pulse would consist of two square phases separated by one IPD.
* **Stimulation Train** (aka **pulse train**, aka **train shape**): A series of pulses over time.
    * E.g., train **period** (classically pulse frequency, or PF), modulation of pulses via whatever user-implemented method (e.g., sinusoid, linear ramp, bursting, or other patterning), etc.

## Software Implementation and Other Conceptual Terms
* **Perceptual Event** (aka **Percept**, aka **Haptic Event**): The set of interaction information elicited by a specific physical or virtual interaction between the user's body and the physical (local or remote) or virtual environment.
    * The event occurs at a specific time, at a specific location, and over a specific area on the user's body.
    * The interaction information thus essentially includes 4 key pieces of information:
        1. Temporal information ("when"): start time and duration (when applicable, i.e., if not simply instantaneous).
        2. Spatial information ("where"): the interaction area - see **haptic area** below - over which the percept occurs.
        3. Haptic information ("what"): quality, intensity, etc.
* **Haptic Area**: An area on a real or modeled body on which haptic percepts can *occur* during live interaction.
    * NOTE: this is distinct from a **Perceptual Area**, the idea being that the system creates the best approximation of the desired haptic event using whatever means necessary within the limitations of the system and scientific knowledge.
* **Perceptual Area** (aka **reachable haptic area**): A distinct area on which haptic percepts can be *elicited* by stimulation.
    * A perceptual area is tied to one or more **contact pools** which can independently elicit percepts over the entire perceptual area.
    * *(Similar to the idea of max flow in graph theory.)*
* **Contact Pool**: A *functionally coherent* subset of contacts in the neural interface.
    * *Functionally coherent* meaning all contacts in the pool are needed to elicit percepts on the given percetual area the contact pool is tied to. Adding a contact to the pool would be redundant information, i.e., would not change the perceptual area covered, or would individually grow or limit the perceptual area and thus require creation of a new contact pool to represent the coverage of the new perceptual area.
    * There may be multiple completely independent contact pools that cover the same perceptual area.
    * *(Similar to the idea of min cut in graph theory: a min cut yields a certain max flow, and min cuts are not unique, meaning there are multiple min cuts that can yield the same max flow.)*
    * Programmatically, contact pools are replaced by Lead Pools in case leads wire to >1 contact and only 1 contact is selected by the user to be in the pool: the second, unmentioned contact must also be included in the pool because it is not electrically independent.
* **Location Mapping Data**: The experimental data, usually **location drawings** showing the perceptual areas elicited or haptic areas reachable by certain contact pools given the specific interface configuration. Specific locations, qualities, intesities, etc. of percepts depend on the stimulation parameters and output configurations used, but this data taken together over the tested parameters and configurations provides the possible or *reachable* haptic area for a given contact pool.
    * This information, given to the system during setup in addition to the interface configuration itself, determines the contact pool and perceptual area map/sets known to the system for a given runtime.
* **Stimulation Thread**: A concurrent process responsible for transducing a specific percept - namely quality at a certain haptic area with the other parameters like intensity being dynamically modulated - to the stimulation pulses needed to best approximate the percept.
    * I.e., **there is one thread per contact pool.**
    * A stimulation thread may create one or more stimulation trains to elicit more complex percepts. However, the pulses of these trains must be condensed in to a single, correctly ordered sequence of pulses before being sent to the stimulator hardware.
    * Ultimately, a stimulation thread is a single stream of information going to stimulator hardware to elicit a desired perceptual event.
* **Stimulation Thread Pool**: A set of *functionally coherent* stimulation threads.
    * *Functionally coherent* meaning all threads in the pool elicit percepts in the same reachable haptic area, i.e., all threads corresponding to the contact pools mapping to a single perceptual area.
* **Percept Transduction**: The decision process by which specific threads are selected and the output configrations and stimulation parameters setup on trains within each of them are deduced based on a given perceptual event (quality, intensity, etc. at a given location with a given area at a given time). There are three main steps to this process:
    1. Thread pool selection based on percept location: choose the pool mapping to the haptic area in which the event occurred.
    2. Thread / contact pool (remember, 1:1) selection based on the desired percept. For example, if two contact pools and thus two threads can elicit a percept at the same location, but contact pool A can only achieve certain qualities, while contact pool B can elicit other qualities, choose the pool that can elicit the desired quality.
    3. Train construction(s) within the thread, i.e., create however many threads with the pulses and phases (parameter information on specific output configurations) sequenced in whichever way needed to elicit the best approximation of the desired percept.
        * *(This is where the matrix computation most readily comes in.)*
    _(TODO: we also need a better name for Transduction or Transducer since that more readily means a specific electrical component. Transformer was an idea, but still not happy with it)_
