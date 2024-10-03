// See https://aka.ms/new-console-template for more information
using EStimLibrary.Core;
using EStimLibrary.Core.Stimulation.Stimulators;
using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Core.SpatialModel;
using EStimLibrary.Core.Haptics;

using EStimLibrary.Extensions.SpatialModel.StringHierarchy;
using EStimLibrary.Extensions.HardwareInterfaces;
using EStimLibrary.Extensions.Stimulation.Stimulators;
using EStimLibrary.Extensions.Haptics;

using MathNet.Numerics.LinearAlgebra;

// TODO: is there any programmatic way to enforce the order of config? to error
// if trying to do something the session doesn't have enough information for
// yet? e.g., place contacts without specifying a spatial reference frame or
// adding all interfaces? is there a way to error and/or indicate what prereqs
// must be done (if not actively request them be done)? something other than
// documentation and trust?

#region TESTING
//Utils.TEST_PRINTS();
#endregion


#region Session Config

// 0) Create a HapticSession.
HapticSession session = new();

// For print-outs for readability
int stepNumber = 0;

// This program is a simple console app demonstrating the HapticSession config
// process and then a few example events. The config can be done using a
// hard-coded example or by back-and-forth I/O, making use of the reflective
// Utils API of this library.
Console.WriteLine("Welcome to the console app demonstrating the " +
    "EStimLibrary.\n");

// Ask if the user would like to use the hard-coded example or user I/O.
Console.WriteLine($"\nWould you like to configure this HapticSession using " +
    $"the hard-coded example or user input?");
// Get the user selection
string[] configModes = new string[] { "Hard-coded example", "User input" };
string configSelection = Utils.SelectFromList(configModes);
bool HARD_CODE_CONFIG = configSelection.Equals(configModes[0]);
Console.WriteLine($"You have selected the '{configSelection}' configuration.\n");

// TODO: since removed everything from HapticSession constructor, how to enforce
// - if at all - restrictions on the spatial types of events (e.g., if
// Transducers have certain event types)? or just say we're not doing any of
// that ever - bc not rn? basically getting at an old comment question:
// does transducer need to know spatial data type(s)? e.g., incoming events can
// only be of that type? or only needs to work in the generic type(s)?

if (HARD_CODE_CONFIG)
{
    #region Hard-Coded Config
    // 1) Build a body model builder, string hierarchy type
    var modelPath = @"../../../../../models/EStimLibrary.SpatialModels/StringHierarchyModels/Surface_FullBody.json";
    var bodyModelName = "left hand";

    Console.WriteLine($"\nSteps {++stepNumber},{++stepNumber}: Trying to " +
        $"build StringHierarchy body model '{bodyModelName}' from model " +
        $"file '{modelPath}'...");

    var bodyModelBuilder = new StringHierarchyBodyModelBuilder(modelPath);
    // Create the IBodyModel. Assume only 1 model in the session: the left hand.
    bool success = bodyModelBuilder.TryCreate(bodyModelName, out IBodyModel bodyModel);
    // Add the body model to the session.
    success = session.TryAddBodyModel(bodyModel, out string bodyModelKey);

    Console.WriteLine($"\n... {(success ? "success" : "failure")}");


    // 2) Add the neural interfaces to the session.
    Console.WriteLine($"\n\nStep {++stepNumber}: Trying to add neural interfaces:" +
        $"\n\t* a ContactGroup with 2 contacts\n\t* a GelPad");
    // First add an multi-contact ring with 2 contacts (ids 0 and 1).
    SortedSet<int> ringContactIds = session.AddInterface(typeof(ContactGroup),
        new object[] { 2 }, out int ringId);
    // Then add a gel pad, inherently with 1 contact (id 2).
    SortedSet<int> gelPadContactIds = session.AddInterface(typeof(GelPad),
        new object[] { }, out int gelPadId);

    // Print contacts per Neural Interface for transparency/readability.
    var contactsPerNI = session.ContactsPerInterface;
    string header = "Contacts per Neural Interface (by global IDs):\n";
    Console.WriteLine($"... {(success ? "success" : "failure")}" +
        $"\n{Utils.DictOfSetsToString(contactsPerNI, header)}");


    // 3) Place the contacts on the body model.
    Console.WriteLine($"\nStep {++stepNumber}: Trying to place contacts on " +
        $"the body model ...");
    // First (id: 0) ring contact on ulnar side.
    var loc0 = new StringHierarchyLocation(
        "left hand, index finger, proximal phalanx | palmar, ulnar");
    success = session.TryPlaceOrMoveContact(0, bodyModelKey, loc0);
    Console.WriteLine($"\t...{(success ? "success" : "failure")}: " +
        $"Placed contact 0 at {loc0}");
    // Second (id: 1) ring contact on radial side.
    var loc1 = new StringHierarchyLocation(
        "left hand, index finger, proximal phalanx | palmar, radial");
    success = session.TryPlaceOrMoveContact(1, bodyModelKey, loc1);
    Console.WriteLine($"\t...{(success ? "success" : "failure")}: " +
        $"Placed contact 1 at {loc1}");
    // Gel pad on back of hand.
    var loc2 = new StringHierarchyLocation("left hand, handbody | dorsal");
    success = session.TryPlaceOrMoveContact(2, bodyModelKey, loc2);
    Console.WriteLine($"\t...{(success ? "success" : "failure")}: " +
        $"Placed contact 2 at {loc2}");


    // 4) Create a dummy "echo" stimulator with 4 outputs (ids 0-3).
    var port = "COM7";
    var nOutputs = 4;
    Console.WriteLine($"\n\nStep {++stepNumber}: Trying to add an " +
        $"EchoStimulator on port {port} with {nOutputs} outputs ...");
    success = session.TryAddStimulator(typeof(EchoStimulator),
        new object[] { port, nOutputs }, out int globalStimId,
        out SortedSet<int> globalOutputIds);

    // Print outputs per Stimulator for transparency/readability.
    var outputsPerStim = session.OutputsPerStimulator;
    header = "Outputs per Stimulator (by global IDs):\n";
    Console.WriteLine($"... {(success ? "success" : "failure")}\n" +
        $"{Utils.DictOfSetsToString(outputsPerStim, header)}");


    // 5) Create 3 leads.
    Console.WriteLine($"\n\nStep {++stepNumber}: Trying to add 3 1:1 leads ...");
    // Lead 0: C0-O0
    success = session.TryAddLead(new Lead(new(new[] { 0 }), new(new[] { 0 }), (Constants.CurrentDirection)Constants.OutputAssignment.CATHODE),
        out int lead0Id);
    // Lead 1: C1-O1
    success = session.TryAddLead(new Lead(new(new[] { 1 }), new(new[] { 1 }), (Constants.CurrentDirection)Constants.OutputAssignment.CATHODE),
        out int lead1Id);
    // Lead 2: C2-O2. THIS is the ground lead.
    success = session.TryAddLead(new Lead(new(new[] { 2 }), new(new[] { 2 }), (Constants.CurrentDirection)Constants.OutputAssignment.ANODE),
        out int lead2Id);

    string leadStr = $"... {(success ? "success" : "failure")}\nLeads added:";
    foreach (var (leadId, lead) in session.Leads)
    {
        // L#: C[c0, ..., cN] - O[o0, ..., oM]
        leadStr += $"\n\t{leadId}: C{Utils.EnumerableToString(
            lead.ContactSet.Select(c => (object)c))} - O{Utils.EnumerableToString(
                lead.OutputSet.Select(o => (object)o))}";
    }
    Console.WriteLine(leadStr);


    // 6) Ask if the user would like to map a lead pool per contact or all in
    // one area?
    Console.WriteLine($"\n\nStep {++stepNumber}: Percept Mapping");
    Console.WriteLine($"Would you like to map both cathodic contacts to a " +
        $"single reachable area, or map each cathode to its own area?");
    // Get the user selection
    string[] areaSplits = new string[] { "Single area", "Split areas" };
    string areaSplit = Utils.SelectFromList(areaSplits);
    bool SINGLE_REACHABLE_AREA = areaSplit.Equals(areaSplits[0]);
    Console.WriteLine($"You have selected the '{areaSplit}' mapping.\nTrying" +
        $" to map leads ...");

    if (SINGLE_REACHABLE_AREA)
    {
        // Percept mapping: single reachable haptic areas.
        // Lead group 0 has leads 0, 1 (ring contacts) and 2 (gel pad ground).
        var lp0 = new SortedSet<int>(new[] { 0, 1, 2 });
        var a0 = new StringHierarchyArea(
            "left hand, index finger | palmar");
        success = session.TryMapLeadPool(lp0, bodyModelKey, a0,
            out int firstInvalidLeadId);
        Console.WriteLine($"... {(success ? "success" : "failure")}: lead " +
            $"pool 0 {Utils.EnumerableToString(lp0)} to {a0}");
    }
    else
    {
        // Percept mapping: Group leads to reachable haptic areas.
        // Lead group 0 has leads 0 (ulnar ring contact) and 2 (gel pad ground).
        var lp0 = new SortedSet<int>(new[] { 0, 2 });
        var a0 = new StringHierarchyArea(
            "left hand, index finger | palmar, ulnar");
        success = session.TryMapLeadPool(lp0, bodyModelKey, a0,
            out int firstInvalidLeadId);
        Console.WriteLine($"\t... {(success ? "success" : "failure")}: lead " +
            $"pool 0 {Utils.EnumerableToString(lp0)} to {a0}");
        // Lead group 1 has leads 1 (radial ring contact) and 2 (gel pad ground).
        var lp1 = new SortedSet<int>(new[] { 1, 2 });
        var a1 = new StringHierarchyArea(
            "left hand, index finger | palmar, radial");
        success = session.TryMapLeadPool(lp1, bodyModelKey, a1,
            out int nextInvalidLeadId);
        Console.WriteLine($"\t... {(success ? "success" : "failure")}: lead " +
            $"pool 1 {Utils.EnumerableToString(lp1)} to {a1}");
    }

    // 7) Add the transducer to the session. PW mod.
    Console.WriteLine($"\nStep {++stepNumber}: Trying to add a PW classic " +
        $"transducer ...");
    var transducer = new ClassicDirectTransducer("PW");
    session.SetTransducer(transducer);
    Console.WriteLine("... success");
    #endregion
}
else
{
    Console.WriteLine("Please follow the steps below to configure this " +
        "HapticSession.\n");

    #region Spatial Types and Body Model Selection and Setup
    // 1) Get the spatial reference frame within which this session will run.
    Console.WriteLine($"\nStep {++stepNumber}: Please select the spatial data " +
        $"types you will use in this session. You will select a Location, Area, " +
        $"and BodyModelBuilder types in sequence. There are no automatic " +
        $"validation checks, so \n***please be careful to select compatible " +
        $"types.***\n");

    // Start with selecting a location type.
    Console.WriteLine("Please select the *Location* data type.\n");
    Dictionary<string, Type> locationTypes = Utils.GetAvailableTypes<ILocation>();
    Type locationType = Utils.SelectType(locationTypes,
        out string locationTypeName);
    Console.WriteLine($"You have selected the location type: " +
        $"'{locationTypeName}'\n");
    // Then select an area type from a list of those of the correct type.
    Console.WriteLine("Please select the *Area* data type.\n");
    Dictionary<string, Type> areaTypes = Utils.GetAvailableTypes<IArea>();
    Type areaType = Utils.SelectType(areaTypes, out string areaTypeName);
    Console.WriteLine($"You have selected the area type: '{areaTypeName}'\n");

    Type[] spatialTypes = { locationType, areaType };

    // Then select a body model builder from a list of those of the correct type.
    Console.WriteLine("Please select the *BodyModelBuilder* data type.\n");
    Dictionary<string, Type> bodyModelBuilderTypes = Utils
        .GetAvailableTypes<BodyModelBuilderBase>();
    Type bodyModelBuilderType = Utils.SelectType(bodyModelBuilderTypes,
        out string bodyModelBuilderTypeName);
    Console.WriteLine($"You have selected the body model builder type: " +
        $"'{bodyModelBuilderTypeName}'. Please input required construction " +
        $"parameters.\n");
    // Get the body model builder constructor param values.
    (object[] bodyModelBuilderParams, var _) = Utils
            .RequestConstructorParameterValues(bodyModelBuilderType);
    // Create the body model builder.
    dynamic bodyModelBuilder = Activator.CreateInstance(bodyModelBuilderType,
        bodyModelBuilderParams);

    // 2) Body Models
    Console.WriteLine($"\n\n\nStep {++stepNumber}: Please select the body " +
        $"model(s) you will use in this session.\n");
    // Get the names of the body models available.
    List<string> availableModelNames = bodyModelBuilder.AvailableModelNames;

    // Let the user select body models within the reference frame type.
    var numBodyModels = Utils.GetInt("number of body models",
        " you are using");
    for (int i = 0; i < numBodyModels; i++)
    {
        // Some extra print-outs for readability.
        Console.WriteLine($"\nYou have {numBodyModels - i} body models left to " +
            $"select and configure.");
        // Get the body model name.
        string modelName = Utils.SelectFromList(availableModelNames.ToArray());
        // Create the IBodyModel.
        bool success = bodyModelBuilder.TryCreate(modelName,
            out IBodyModel bodyModel);

        // Try to add the selected body model to the session.
        success = session.TryAddBodyModel(bodyModel, out string bodyModelKey);
        if (success)
        {
            Console.WriteLine($"Body Model of type '{modelName}' was added with " +
                $"key '{bodyModelKey}'.");
        }
        else
        {
            Console.WriteLine($"Body Model of type '{modelName}' Could not be " +
                $"added. Please try again.");
            // Decrement i so the loop makes up for (retries) the failed iteration.
            i--;
        }
    }
    #endregion


    #region Neural Interface Selection
    Console.WriteLine($"\nStep {++stepNumber}: Please select the neural " +
        $"interface(s) you will use in this session.\n");
    // 1) Neural Interface selection, config, and other setup
    var numInterfaces = Utils.GetInt("number of neural interfaces",
        " you are using");
    // Get all implemented neural interface types found.
    Dictionary<string, Type> interfaceTypeDict = Utils
        .GetAvailableTypes<NeuralInterfaceHardware>();
    // Let user select which NI type is used for each NI.
    for (int i = 0; i < numInterfaces; i++)
    {
        // Some extra print-outs for readability.
        Console.WriteLine($"\nYou have {numInterfaces - i} neural interfaces " +
            $"left to select and configure.");
        // Get the NI type.
        Type neuralInterfaceType = Utils.SelectType(interfaceTypeDict,
            out string neuralInterfaceTypeName);
        // Get the constructor params needed for the selected NI type.
        (object[] neuralInterfaceParams, var _) = Utils
            .RequestConstructorParameterValues(neuralInterfaceType);
        // Add the NI to the session.
        SortedSet<int> globalContactIds = session.AddInterface(
            neuralInterfaceType, neuralInterfaceParams,
            out int globalNeuralInterfaceId);
        // Print success.
        Console.WriteLine($"Neural interface {globalNeuralInterfaceId} " +
            $"(type: {neuralInterfaceTypeName}) was added with contacts: " +
            $"[{string.Join(',', globalContactIds)}]");
    }

    // Print contacts per Neural Interface for transparency/readability.
    var contactsPerNI = session.ContactsPerInterface;
    string header = "Contacts per Neural Interface (by global IDs):\n";
    var cpnStr = $"\n\nFor Reference: " +
        $"{Utils.DictOfSetsToString(contactsPerNI, header)}";
    Console.WriteLine(cpnStr);
    #endregion


    #region Contact Placement
    Console.WriteLine($"\n\n\nStep {++stepNumber}: Please select where you have " +
        $"placed the neural interface contacts for this session.\n");
    // 1) Select contact placement locations
    List<int> contactIds = new(session.ContactIds);
    // Get the user-input location for each contact.
    for (int i = 0; i < contactIds.Count; i++)
    {
        int globalContactId = contactIds[i];

        // Get the string key for the body model.
        Console.WriteLine($"Please select the high-level body model in which " +
            $"contact {globalContactId} is placed.");
        string bodyModelKey = Utils.SelectFromList(session.BodyModelKeys.ToArray());

        // Get the IBodyModel associated. Method should succeed since key from
        // internal list.
        session.TryGetBodyModel(bodyModelKey, out var bodyModel);
        var paramValues = Utils.RequestFactoryCreateParamValues(
            bodyModel.LocationFactory.HelpMsg,
            bodyModel.LocationFactory.ParamLimits, Console.WriteLine,
            Console.ReadLine);
        bool success = bodyModel.LocationFactory.TryCreate(paramValues,
            out ILocation contactPosition);

        // Try placing the contact
        Console.WriteLine($"\nTrying to place contact {i} in body model " +
            $"'{bodyModelKey}' at position {contactPosition} ...");

        if (session.TryPlaceOrMoveContact(globalContactId, bodyModelKey,
            contactPosition))
        {
            Console.WriteLine($"\nContact {globalContactId} placed " +
                $"successfully.\n");
        }
        else
        {
            Console.WriteLine($"\nContact {globalContactId} could not be " +
                $"placed. Please try again.\n");
            // Decrement i so the loop makes up for (retries) the failed iteration.
            i--;
        }

    }
    #endregion


    #region Stimulator Selection
    // TODO: Consider an interface for SerialStimulator and BluetoothStimulator so
    //  could then type check the stimType selected and proceed with the following
    //  functions conditionally (at least the port select)
    Console.WriteLine($"\n\n\nStep {++stepNumber}: Please select the " +
        $"stimulator(s) you will use in this session.\n");
    // 5) Stimulator selection, config, and other setup
    var numStimulators = Utils.GetInt("number of stimulators", " you are using");
    // Get all implemented Stimulator types found.
    Dictionary<string, Type> stimTypeDict =
        Utils.GetAvailableTypes<Stimulator>();
    // Let user select which Stimulator type is used for each stimulator.
    for (int i = 0; i < numStimulators; i++)
    {
        // Some extra print-outs for readability.
        Console.WriteLine($"\nYou have {numStimulators - i} stimulators left to " +
            $"select and configure.");
        // Get the stim type.
        Type stimType = Utils.SelectType(stimTypeDict, out string stimTypeName);
        // Get the constructor params needed for the selected stim type.
        (object[] stimParams, var _) =
            Utils.RequestConstructorParameterValues(stimType);
        // Add the stimulator to the session.
        if (session.TryAddStimulator(stimType, stimParams, out int globalStimId,
            out SortedSet<int> globalOutputIds))
        {
            Console.WriteLine($"Stimulator {globalStimId} (type: {stimTypeName}) " +
                $"was added with outputs: [{string.Join(',', globalOutputIds)}]");
        }
        else
        {
            Console.WriteLine($"Could not add Stimulator of type {stimTypeName} " +
                $"with parameter values: [{string.Join(",", stimParams)}]. " +
                $"Please try again.");
            i--;    // Decrement to redo the entry for this ith stimulator.
        }
    }

    // TODO: OutputsPerStimulator is not working! --> check if this was fixed, I think yes
    // Print outputs per Stimulator for transparency/readability.
    var outputsPerStim = session.OutputsPerStimulator;
    header = "Outputs per Stimulator (by global IDs):\n";
    var opsStr = $"\n\nFor Reference: " +
        $"{Utils.DictOfSetsToString(outputsPerStim, header)}";
    Console.WriteLine(opsStr);
    #endregion


    #region Wiring
    Console.WriteLine($"\n\n\nStep {++stepNumber}: Please select the leads you " +
        $"have wired in this session.\n");
    // 1) Select wiring/leads
    List<Lead> leadSet = new();
    int numLeads = Utils.GetInt("number of leads", " you are using");

    // Let the user specify the contact and output ID sets for each Lead.
    for (int i = 0; i < numLeads; i++)
    {
        // Some extra print-outs for readability.
        Console.WriteLine($"\nYou have {numLeads - i} leads left to specify.\n");

        // Get the number of contacts in this Lead.
        int numContacts = Utils.GetInt("number of neural interface contacts",
            $" attached to lead {i}");

        // Some extra help print-outs.
        Console.WriteLine(cpnStr);

        // Get n contact IDs.
        SortedSet<int> leadContactIds = new();
        for (int c = 0; c < numContacts; c++)
        {
            // Some extra print-outs for readability.
            Console.WriteLine($"\nYou have {numContacts - c} contacts left to " +
                $"specify for lead {i}.");
            // Get the contact ID.
            int contactId = Utils.GetInt("contact ID");
            // Check if it's valid.
            if (session.IsValidContactId(contactId))
            {
                leadContactIds.Add(contactId);
            }
            else
            {
                Console.WriteLine($"Invalid contact ID. Please enter an int in " +
                    $"the range [{session.ContactIds.Min}, " +
                    $"{session.ContactIds.Max}]");
                c--;    // Decrement to redo the ID entry for this c-th contact.
            }
        }

        // Get the number of outputs in this Lead.
        int numOutputs = Utils.GetInt("number of stimulator outputs",
            $" attached to lead {i}");

        // Some extra help print-outs.
        Console.WriteLine($"\nFor Reference: {opsStr}");

        // Get n output IDs.
        SortedSet<int> leadOutputIds = new();
        for (int o = 0; o < numOutputs; o++)
        {
            // Some extra print-outs for readability.
            Console.WriteLine($"\nYou have {numOutputs - o} outputs left to " +
                $"specify for lead {i}.");
            // Get output ID.
            int outputId = Utils.GetInt("output ID");
            // Check if it's valid.
            if (session.IsValidOutputId(outputId))
            {
                leadOutputIds.Add(outputId);
            }
            else
            {
                Console.WriteLine($"Invalid output ID. Please enter an int in " +
                    $"the range [{session.OutputIds.Min}, " +
                    $"{session.OutputIds.Max}]");
                o--;    // Decrement to redo the ID entry for this o-th output.
            }
        }

        // Get the default polarity / current direction of this lead.
        Console.WriteLine("\nPlease specify the default polarity, or current " +
            $"direction, of lead {i}.");
        string currentDirName = Utils.SelectFromList(
            Enum.GetNames(typeof(Constants.CurrentDirection)));
        var currentDir = (Constants.CurrentDirection)Enum.Parse(
            typeof(Constants.CurrentDirection), currentDirName);

        // Create the Lead and try adding it to the session.
        if (session.TryAddLead(
            new Lead(leadContactIds, leadOutputIds, currentDir),
            out int leadId))
        {
            Console.WriteLine($"Added lead {leadId} with contacts " +
                $"[{string.Join(",", leadContactIds)}], outputs " +
                $"[{string.Join(",", leadOutputIds)}], and current direction" +
                $" {currentDirName}.");
        }
        else
        {
            Console.WriteLine($"Failed to add lead with contacts " +
                $"[{string.Join(",", leadContactIds)}] and outputs " +
                $"[{string.Join(",", leadOutputIds)}]. Please try again.");
            i--;    // Decrement to redo the ID entry for this ith lead.
        }
    }

    // TODO: how to factor in specific cables???
    //// Get all implemented ICableHardware types found.
    //Dictionary<string, Type> cableTypeDict = Utils.GetAvailableTypes<ICableHardware>();
    //// Let user select which Cable is used.
    //Type cableType = Utils.SelectType(cableTypeDict, out string cableTypeName);
    //// Create the Cable using reflection to handle the generic function CreateCable
    //// with the desired cableType in a variable value rather than passed as the generic type.
    //Cable cable;
    //var createCableMethod = typeof(Utils).GetMethod(nameof(Cable.CreateCable)).MakeGenericMethod(cableType);
    //object[] parameters = { leadSet, null }; // Create array to hold parameters.
    //var success = (bool)createCableMethod.Invoke(null, parameters);
    //cable = (Cable)parameters[1]; // Retrieve the modified cable value.
    #endregion


    #region Percept Mapping
    Console.WriteLine($"\n\n\nStep {++stepNumber}: Please enter your percept " +
        $"mapping data (lead pools to reachable haptic areas) for this session.\n");
    // Add lead pools that cover a certain reachable haptic area on the
    // previously selected body model(s). This area-pool pair will be added to the
    // session successfully if: 1) the area is valid within the body model, 2) the
    // lead pool contains at least 2 independent leads per stimulator involved.
    // If valid, the session will internally add a StimThread for the new lead
    // pool and map it to the given area.
    // TODO: somehow enforce that cannot do percept mapping until all leads have
    // been created? or just leave to documentation and smart user use?
    var numLeadPools = Utils.GetInt("number of lead pools",
        " you are defining");
    for (int p = 0; p < numLeadPools; p++)
    {
        // Get the contacts in this pool.
        var numLeadsInPool = Utils.GetInt("number of leads", $" in pool {p}");
        SortedSet<int> requestedLeadSet = new();
        for (int l = 1; l <= numLeadsInPool; l++)
        {
            string suffix = (l == 0 || l > 2) ? "th" : ((l == 1) ? "st" : "nd");
            var leadId = Utils.GetInt("lead ID", $" of the {l}{suffix} " +
                $"lead in pool {p}. Choose a lead not already in this " +
                $"pool, from the global set {session.LeadIds.Min}-" +
                $"{session.LeadIds.Max} (inclusive)");
            if (requestedLeadSet.Contains(leadId))
            {
                Console.WriteLine($"Pool {p} already contains lead " +
                    $"{leadId}. Please enter a different ID.");
                l--;    // Decrement to redo the ID entry for this lth lead.
            }
            else
            {
                requestedLeadSet.Add(leadId);
                Console.WriteLine($"Lead {leadId} was added to pool {p}.");
            }
        }

        Console.WriteLine($"\nPlease specify the reachable haptic area for lead " +
            $"pool {p}: [{string.Join(',', requestedLeadSet)}].");

        // Get the string key for the body model.
        string bodyModelKey = Utils.SelectFromList(session.BodyModelKeys.ToArray());
        // Get the BodyModel itself.
        session.TryGetBodyModel(bodyModelKey, out var bodyModel);

        // Get the reachable haptic area for this pool.
        //Type areaType = typeof(IArea).MakeGenericType(locationType);
        //dynamic reachableHapticArea = Utils.ConstructWithUserInputParams(areaType);
        var paramValues = Utils.RequestFactoryCreateParamValues(
            bodyModel.AreaFactory.HelpMsg,
            bodyModel.AreaFactory.ParamLimits, Console.WriteLine,
            Console.ReadLine);
        bool success = bodyModel.AreaFactory.TryCreate(paramValues,
            out IArea reachableHapticArea);

        // Try adding the area-mapped lead pool.
        Console.WriteLine($"Trying to add lead pool {p} " +
            $"[{string.Join(",", requestedLeadSet)}] to cover area " +
            $"{reachableHapticArea} in body model '{bodyModelKey}'.");

        success = session.TryMapLeadPool(requestedLeadSet, bodyModelKey,
            reachableHapticArea, out int firstInvalidLeadId);

        if (success)
        {
            Console.WriteLine($"Successfully added lead pool {p}: " +
                $"[{string.Join(",", requestedLeadSet)}]");
        }
        else
        {
            Console.WriteLine($"Invalid lead set or area. First lead that " +
                $"was invalid, if any: {firstInvalidLeadId}. Please try " +
                $"again.");
            p--;    // Decrement to redo the ID entry for this pth pool.
        }
    }
    #endregion


    #region Transducer Selection
    // 1) Select and create transducer.
    Console.WriteLine($"\n\n\nStep {++stepNumber}: Please select the haptic " +
        $"transducer you will use in this session.\n");
    // Get all available transducer types.
    Dictionary<string, Type> transducerDataTypes =
        Utils.GetAvailableTypes<HapticTransducer>();
    // Get the user's selection of desired transducer type.
    Type transducerType = Utils.SelectType(transducerDataTypes,
        out string transducerTypeName);
    Console.WriteLine($"You have selected the transducer '{transducerTypeName}'\n");
    // Create the transducer given user input parameter values.
    // TODO: is there a more information-forward way to request the modulated
    // param? Rn just have to know what the options are
    dynamic transducer = Utils.ConstructWithUserInputParams(transducerType);
    // Add the transducer to the session.
    session.SetTransducer(transducer);
    #endregion
}

#endregion Session Config


Console.WriteLine("\n\n\nHapticSession config complete! We are ready to " +
    "begin the session.\n\n\n");


#region Live Session
if (!session.Start())
{
    Console.WriteLine("The session has not been configured properly! Failed " +
        "to start.");
}
// TODO: Implement a user-input "run-time" too because unless the user input
// config sets up the expected reachable areas, events won't do anything.
// Start by printing the config info again (for reference). Then loop until
// specific keyboard input is given (e.g., 'q' or 'quit'), and within that loop,
// ask for the user to create event-by-event. Can use factory/utils create
// functions
else
{
    // Do stuff! run the system, hit 'play', start stim, make events happen,
    // you decide!
    List<double> pressureValues = new() { 0.0, 0.2, 0.4, 0.7, 1.0 };
    var eventArea1 = new StringHierarchyArea(
                        "left hand, index finger, distal phalanx | palmar");
    foreach (var pVal in pressureValues)
    {
        var pNormVec = Vector<double>.Build.Dense(new double[] { pVal });
        var event1 = new HapticEvent(DateTime.Now, null,
            new() {
                { "left hand", new List<IArea>() { eventArea1 } }//, eventArea2 } }
            },
            pNormVec);
        session.AddEvent(event1);
    }
    session.Stop();
}
#endregion Live Session