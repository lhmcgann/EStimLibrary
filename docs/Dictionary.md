# Dictionary of The Domain: A Software Library for Sensory Electrical Nerve Stimulation
Last Updated on 09/24/2024.

## Anatomical Terms
* **Afferent**: conducted or conducting inward or towards something, e.g., sensory information from sensory organs toward central processing
* **Efferent**: conducted or conducting outward or away from something, e.g., motor information from the central nervous system to muscles
* **Proximal**: directional descriptor referring to nearer to the center of the body or point of attachment
* **Distal**: directional descriptor opposite of proximal; refers to farther from the center of the body or point of attachment
* **Dorsal**: directional descriptor referring to the upper or back side of the body, e.g., back of the hand or top of the foot
* **Palmar/Volar/Ventral**: directional descriptor opposite of dorsal; refers to the lower or front side of the body, e.g., one's belly (ventral), the palms of the hands (palmar/volar), or the soles of the feet
* **Medial**: directional descriptor referring to closer to the midline of the body
    * when describing directionality on the hands, should be applied assuming a palm-ventral posture; i.e., pinky finger is more medial than the thumb
* **Lateral**: directional opposite of medial, referring to farther from the body midline or closer to the body outline, assuming a palms-ventral posture
* **Ulnar**: a peripheral nerve in the human upper extremities; when used directionally, refers to closer to the pinky fingers
* **Radial**: a peripheral nerve in human upper extremities; when used directionally, refers to closer to the thumbs (opposite of ulnar)
* **Median**: a peripheral nerve in human upper extremities

## Sensation Terms
* **Tactile Sensation**: referring to our sense of touch, including vibration, pressure, skin stretch, slip, texture and edge/shape detection; usually elicited by the consequent afferent neural activity of deforming various mechanoreceptors in our skin, but can also be caused by electrically activating the associated neural tissue directly
* **Proprioceptive Sensation**: referring to our sense of proprioception, including muscle length and tension, joint angle, limb position, and movement; usually elicited by the consequent afferent neural activity of deforming various mechanoreceptors in our tendons and muscle spindles, but can also be caused by electrically activating the associated neural tissue directly
* **Haptic Feedback** or **Haptics**: a broad term used to describe physical feedback related to touch, usually grouped into "tactile" and "kinesthetic"
* **Electrical Nerve Stimulation**: electrical stimulation (current- or voltage-driven waveforms) attempting to elicit sensory or motor responses by activating associated nerves
    * Has been used to target peripheral, central, and autonomic nervous systems, although within the context of sensorimotor HITL systems, the focus is on peripheral and/or central, depending on the target user population
    * Can be delivered via different interfaces, such as implanted (intramuscular, extraneural, interfascicular, intrafascicular, regenerative) or surface/wearable neural interfaces
    * NOTE: this software library is currently designed for sensory not motor stimulation, although it may be extended in the future
* **Local Sensation**: tactile sensations perceived to be occurring at the site of electrical stimulation
* **Referred Sensation**: tactile sensations perceived to be occurring NOT directly at the site of electrical stimulation; usually distal
* Related terms you may see in literature:
    * **transcutaneous electrical nerve stimulation (TENS)**: "through the skin", or surface, electrical stimulation targeting underlying nerves to elicit motor or sensory repsonses
    * **electrocutaneous stimulation**: surface electrical stimulation that elicits local skin activation as a form of feedback itself, often used for sensory substitution
    * **electrotactile stimulation**: commonly used interchangeably with electrocutaneous stimulation but can refer to any electrical stimulation eliciting tactile sensations
    * **vibrotactile feedback**: tactile sensations, namely vibration, elicited by vibrating actuators in contact with the skin, often used for sensory substitution
    * **force feedback**: a form of haptic feedback that provides information about the forces at play in a physical interaction; not exclusively but often delivered mechanically via an external apparatus

## Hardware Terms
* **Stimulator**: The electrical stimulation waveform generator, including current or voltage drivers, timers, and switching matrices.
* **Output**: A physical connection point, i.e., pin, on the exterior of a physical stimulator.
* **Current Driver**: A physical and electrical component internal to stimulators that drives current and can act as a current source or sink.
* **Output Configuration**: The assigned mapping between a set of stimulator outputs and current drivers, namely determining if each output is connected to - and thus acts as - a current **source** or **sink**.
    * An output configuration must contain at least two outputs: one configured as a source, and one configured as a sink.
    * There may be more than one output configured as a source or a sink in the same configuration, depending on the capabilities of the stimulator.
    * Hardware limitation: `2 <= count(outputs assigned as sources) + count(outputs assigned as sinks) <= total number of current drivers in the stimulator`
* **Contact**: The smallest divisible physical electrically conductive piece contacting biological tissue. Most often, this will be a single metal piece or a gel pad or similar.
* **Contact Configuration**: The specific positions of each contact in a contact set on a wearer's body or a body/limb model.
* **Neural Interface**: A wearable that holds contacts. Conceptually, this term can refer to a single physical neural interface unit (e.g., a single ring with 2 contacts) and/or the aggregate collection of physical neural interface units (e.g., 5 rings with 2 contacts each yields a "net" neural interface of 10 contacts). Programmatically in this library, a neural interface refers only to the former: a single physical neural interface unit, or a single indivisible piece of wearable, contact-containing hardware.
* **Lead**: A physical and electrical connection - as well as conceptual correlate - between outputs and contacts, potentially in a many-to-many relationship, but at least one-to-one. A lead is equivalent to 1 independent electrical system and can be modeled as a complete bipartite graph (as in graph theory) where the two sets are the output set and the contact set.
* **Cable**: The physical collection of all leads. May be an explicit part-numbered cable, for example.
* **Hardware Interface Configuration**: The entirety of a specific hardware setup, including selections of stimulators, neural interfaces, contact placement, cables, and leads. The key information provided from these configuration selections to the software library are number of outputs, associated stimulation parameter limitations on those outputs based on which stimulators they're on, number of contacts, contact positions, and contact-output connections created by specific wiring of the cable/leads.

In summary, **outputs** of a **stimulator** are physically and electrically connected to the **contacts** of a **neural interface** by the set of **leads** in a **cable**. The **output configuration**, or mapping of outputs to **current drivers** usually via a switching matrix, is set internal to the stimulator and may or may not be programmatically changeable, depending on the stimulator. Contacts are placed at certain positions on a real or modeled body when the neural interface is worn, and this placed set is the **contact configuration**.

## Stimulation Terms
* **Phase**: The smallest, independently executable time period of charge injection. Phases are bounded by two conditions:
    1. A phase must be of a single **polarity**, meaning its amplitude cannot cross 0. Such a desired cross must elicit a new phase.
        * A phase can be constructed as any deviation and subsequent return to a baseline amplitude value which may be non-zero, e.g., if a DC offset is desired. However, the definitional *limit* of a phase is crossing the 0 threshold.
    3. In other words, a phase consists of a single **output configuration**. Desired stimulation that requires a different output configuration, i.e., a different assignment of outputs as current sources and sinks, must elicit a new phase.
    * So, a phase is comprised of two elements:
        1. The stimulation parameter data to elicit the desired **phase shape**, such as **width** (us) or **amplitude** (mA). NOTE: The actual achievable phase shape is limited by the stimulator hardware itself, e.g., rounded rather than square corners, or a limited number of discrete amplitude steps depending on the desired parameters and the stimulator's timer and current driver resolutions.
        2. The output configuration through which the stimulation is executed. NOTE: The sum of amplitudes across the set of source outputs and the set of sink outputs must be equal to 0 at all times because that's how electricity works.
* **Feature**: Parameterizable subcomponents of a phase that - taken together in immediate sequence - form the **phase shape**.
    * By definition of a phase, all features in the same phase must be of the same polarity as all other features in the phase.
* **Pulse**: A series of phases over time.
    * Functionally, it is the smallest stimulation artifact meant to excite an axonal population and elicit a percept.
    * Phases in a pulse are spaced by **inter-phase delays** (**IPDs**, units: us).
        * Hardware limitation: IPDs must be greater than or equal to the time it takes the stimulator to switch output configurations which may be of opposite polarities.
    * E.g., a cathodic-first, charge-balanced, biphasic square pulse would consist of two equal-area square phases separated by one IPD where the first phase is a negative polarity (e.g., stimulation phase), the second positive (e.g., recharge phase).
* **Pattern**: A single non-repeating series of pulses over time, used as the base sequence of a stimulation train.
    * Specified by a sequence of pulses and each pulse's delay (us) from the pattern's start, or by a sequence of pulses and the **inter-pulse intervals** (**IPIs**, units: us) between them.
    * E.g., a single pulse for simple stimulation trains
    * E.g., a linear ramp, or a series of pulses with ascending/descending amplitude
    * E.g., a sequence of independent/unrelated pulses
* **Stimulation Train** (aka **pulse train**, aka **train shape**, aka **waveform**): A pattern repeated over time at a certain frequency.
    * The pattern may be modulated over each repeat
    * The train **period** (ms; T = 1/F) must be no shorter than the duration of the train's base pattern.
    * E.g., train **period** (classically pulse frequency, or PF = 1/PT), modulation of pulses via whatever user-implemented method (e.g., sinusoid, linear ramp, bursting, or other patterning), etc.
* **Stimulation Parameters**: the full set of stimulation information needed to define the stimulation waveform.
    * If an output configuration is the "where", stimulation parameters are the "what" and "when". Think of it as data (traffic) flowing through a tunnel. The output configuration is the tunnel, and the stimulation parameters (data) describe the traffic.

## Software Implementation and Other Conceptual Terms
* **Haptic Session**: The main entry point into this library's API, representing a single instance or "session" created at each runtime by applications wishing to use this library.
    * Stores hardware interface configuration selections and controls start/stop of stimulation.
        * Configurations do NOT persist over instances of sessions.
    * Once a session is configured and stimulation is started, incoming haptic events will be transduced into stimulation trains and sent to the appropriate stimulator hardware to evoke percepts.
* **Haptic Event**: The set of interaction information elicited by a specific physical or virtual interaction between the user's body and the physical (local or remote) or virtual environment.
    * The event occurs at a specific time, at a specific location, and over a specific area on the user's body.
    * The interaction information thus essentially includes 4 key pieces of information:
        1. Temporal information ("when"): start time and duration (when applicable, i.e., if not simply instantaneous).
        2. Spatial information ("where"): the interaction area - see **haptic area** below - over which the event occurs.
        3. Haptic information ("what"): quality, intensity, etc.
* **Haptic Area**: An area on a real or modeled body on which a haptic event occurs during runtime interaction.
    * NOTE: this is distinct from a **Percept\[ual\] Area**, the idea being that the system creates the best approximation of the desired haptic event using whatever means necessary within the limitations of the system and scientific knowledge.
* **Perceptual Event** (aka **Percept**, aka **Haptic Percept**): What a user actually perceives after receiving electrotactile stimulation
* **Percept Area** (aka **Perceptual Area**): An area on the user's real body at which they perceive a perceptual event.
    * In prior research, often referred to as **Percept *Location***
* **Percept Mapping Data**: The experimental data, usually **location drawings**, describing perceptual areas elicited by various stimuli, limited to sensory electrical nerve stimulation for the scope of this software library. Each percept is a result of a specific hardware interface configuration and set of stimulation parameters. Different stimulation parameters on the same hardware configuration may elicit different percepts (i.e., with varying intensities, qualities, etc.) at different areas/locations. However, even though the percept area/location may change based on stimulation parameters, the total potential area/location in which that percept may occur is bound by the hardware configuration, namely which contacts are involved. Thus, Location Mapping Data taken in aggregate provide a set of **Reachable/Potential Percept Areas** paired with specific **Contact Pools**.
    * This mapping information must be given to the haptic session during setup (in addition to the hardware interface configuration itself) so at runtime the session can automatically localize haptic events (which occur at specific areas) to relevant contact pools.
* **Reachable/Potential Percept\[ual\]/Haptic Area**: A distinct area on a real/modeled body on which percepts can be elicited by stimulation delivered by a specific group of contacts.
    * A potential perceptual area is by definition tied to one or more **contact pools** which can independently elicit percepts over the entire area.
    * *(Similar to the idea of max flow in graph theory.)*
* **Contact Pool**: A *functionally coherent* subset of contacts in the neural interface.
    * *Functionally coherent* meaning all contacts in the pool are needed to elicit percepts on the given reachable percept area the contact pool is tied to. Adding a contact to the pool would be redundant information, i.e., would not change the potential percept area covered, or would individually grow or limit the potential percept area and thus require the creation of a new contact pool to represent the coverage of the new potential percept area.
    * There may be multiple completely independent contact pools that cover the same potential percept area.
    * *(Similar to the idea of min cut in graph theory: a min cut yields a certain max flow, and min cuts are not unique, meaning there are multiple min cuts that can yield the same max flow.)*
    * Programmatically, contact pools are replaced by **Lead Pools** in case leads wire to >1 contact and only 1 contact is selected by the user to be in the pool; the second, unmentioned contact must also be included in the pool because it is not electrically independent.
* **Event Localization**: The determination of contact pool(s) (and thus stimulation thread(s)) relevant to eliciting the associated percept of an incoming haptic event.
    * This step is done automatically by the haptic session based on percept mapping data and the hardware configuration set up.
* **Stimulation Thread**: Conceptually, an independent timeline - or asynchronous process - per reachable haptic area (and thus per contact pool) that enables haptic and subsequent perceptual events to occur concurrently (within the bounds of what the host computer can perform). Practically, a stimulation thread is an object created per contact pool containing the actual stimulation data (trains) for that pool and that is manipulated on its own programmatic thread.
    * A stimulation thread may contain one or more stimulation trains to elicit more complex percepts. However, the pulses of these trains must be condensed into a single, correctly ordered sequence of pulses before being sent to the stimulator hardware.
    * Ultimately, a stimulation thread is a single stream of information going to stimulator hardware to elicit a desired perceptual event.
* **Event Transduction**: The conversion of haptic event data into the stimulation parameters needed to elicit a percept that best approximates the haptic event.
* **Haptic Transducer**: The object responsible for transducing events in the user's desired way.
    * The transducer is given all stimulation threads relevant to the haptic event, as determined by the automatic localization step.
    * The transducer must then generate the desired stimulation train(s) on the desired stimulation thread(s). There are two main steps to this process:
        1.  Thread/contact pool (remember, 1:1) selection based on the desired percept. For example, if two contact pools and thus two threads are provided (i.e., can elicit a percept at the same location), but contact pool A can only achieve certain qualities, while contact pool B can elicit other qualities, choose the pool that can elicit the desired quality.
        2.  Train construction(s) within the stimulation thread, i.e., create however many trains with the patterns, pulses, and phases (parameter information on specific output configurations) modulated in whichever way is needed to elicit the best approximation of the desired percept.
    * Users implement their specific transducer class to fulfill these responsibilities according to the desired transduction mechanism (e.g., simple stimulation parameter modulation, or something more complicated). The desired specific transducer class implementation must then be given to the haptic session before starting stimulation, and that implementation is what will be used during that session.