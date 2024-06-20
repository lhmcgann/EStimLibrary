using EStimLibrary.Core.HardwareInterfaces;


namespace EStimLibrary.Core;


public class NeuralInterfaceManager : ResourceManager<NeuralInterfaceHardware>
{
    // Some key properties are inherited from the parent class ResourceManager.
    // The NI objects themselves are stored in Resources.
    // The ID pool is IdPool.
    // If a managed NI resource exists, its ID should be marked as Used.
    // Free IDs can exist in the pool, e.g., if a NI was added then removed, but
    // there must be no managed resource in Resources under that ID.

    // Private data structures specific to interface management.
    protected ReusableIdPool _ContactIdPool;
    /// <summary>
    /// The global contact IDs that currently exist in this session.
    /// </summary>
    // NOTE: They are only those marked as "used" by the NI manager since some
    // IDs may have been removed, leaving non-consecutive IDs in the pool
    // overall, so only the "used" IDs are live.
    public SortedSet<int> ContactIds => this._ContactIdPool.UsedIds;
    /// <summary>
    /// Map of global contact ID to the global ID of the neural interface it
    /// belongs to.
    /// </summary>
    protected Dictionary<int, int> _ContactInterfaceIdMap { get; set; }

    public int NumTotalInterfaces => this.NumTotalResources;
    // NOTE: The "total" *live* contact IDs are only those marked as "used" by
    // the NI manager. This is to account for NI (and subsequent contact ID)
    // removal which would leave NI and contact IDs in existence in the pool,
    // just not presently used.
    public int NumTotalContacts => this.ContactIds.Count;

    public NeuralInterfaceManager() : base()
    {
        // Init empty dictionaries, resource maps, etc.
        this._ContactIdPool = new ReusableIdPool();
        this._ContactInterfaceIdMap = new();
    }

    public SortedSet<int> CreateAndRegisterNeuralInterface(Type interfaceType,
        object[] interfaceSpecificParams, out int globalInterfaceId)
    {
        // 0) Check interfaceType is valid derived class, throw exception if not.
        if (!(typeof(NeuralInterfaceHardware).IsAssignableFrom(interfaceType)
             && !interfaceType.IsInterface && !interfaceType.IsAbstract))
        {
            throw new ArgumentException($"The provided type {interfaceType} " +
                $"is not derived from BaseClass " +
                $"{typeof(NeuralInterfaceHardware)}.");
        }

        // 1) Try to generate this new interface's new managed ID.
        if (!this.TryGetNextAvailableId(out globalInterfaceId))
        {
            return new();   // TODO: refactor to return bool and put this in out param?
        }

        // 2) Create the new interface of the given type.
        // 2a) Create the interface (dynamic runtime type determination)
        dynamic newInterface = Activator.CreateInstance(interfaceType,
            interfaceSpecificParams);
        // 2b) Set the global ID of the interface.
        newInterface.Id = globalInterfaceId;

        // 3) Add this interface to the dictionary with its new given ID.
        this.TryAddResource(globalInterfaceId, newInterface);  // Save object.

        // 4) Generate the new contact IDs and add them to internal structs.
        var newContactIds = this._UseContacts(newInterface.NumContacts);

        // 5) Add the global contact IDs to the contact-NI ID map.
        foreach (var globalContactId in newContactIds)
        {
            this._ContactInterfaceIdMap.Add(globalContactId, globalInterfaceId);
        }

        // Return the contact IDs.
        return newContactIds;
    }

    /// <summary>
    /// Use the given number of contacts to the global pool, claiming existing
    /// IDs or adding new ones as needed.
    /// </summary>
    /// <param name="numContacts">The number of contacts to use.</param>
    /// <returns>The set of contact IDs used.</returns>
    protected SortedSet<int> _UseContacts(int numContacts)
    {
        // Init a contact ID set to fill in and then return.
        SortedSet<int> contactIds = new();

        // Generate n new contact IDs.
        for (int i = 0; i < numContacts; i++)
        {
            // Generate a new contact ID.
            int contactId = this._GetNextAvailableContactId();
            // Mark the new ID as "used" so another GetNext call doesn't get the
            // same ID.
            this._ContactIdPool.UseId(contactId);
            // Add the new contact ID to the returned set.
            contactIds.Add(contactId);
        }
        // Return the new set of contact IDs.
        return contactIds;
    }

    /// <summary>
    /// Get the next free global contact ID, increasing the pool ID count if
    /// none are immediately free.
    /// </summary>
    /// <returns>The next free global contact ID.</returns>
    protected int _GetNextAvailableContactId()
    {
        // Get the next available ID. If none and if still haven't hit hard max
        // limit on resource count, increase the ID pool max and try again, else
        // return failure.
        int globalContactId;
        while (!this._ContactIdPool.TryGetNextFreeId(out globalContactId))
        {
            // Else increment the number of IDs in the pool and try again.
            this._ContactIdPool.IncrementNumIds(1);
        }
        return globalContactId;
    }

    /// <summary>
    /// Check if a global contact ID is valid among currently registered neural
    /// interfaces.
    /// </summary>
    /// <param name="contactId">The global int contact ID.</param>
    /// <returns>True or false.</returns>
    public bool IsValidContactId(int contactId)
    {
        return this.ContactIds.Contains(contactId);
    }

    public bool TryGetNeuralInterface(int globalInterfaceId,
        out NeuralInterfaceHardware neuralInterface)
    {
        return this.TryGetResource(globalInterfaceId, out neuralInterface);
    }

    public Dictionary<int, SortedSet<int>> ContactsPerInterface()
    {
        Dictionary<int, SortedSet<int>> res = new();
        foreach (var niId in this.IdPool.Ids)
        {
            // Get all contact IDs that map to the current NI ID.
            SortedSet<int> contacts = new(this._ContactInterfaceIdMap
                .Where(kvp => kvp.Value == niId)    // filter by NI ID
                .Select(kvp => kvp.Key));           // save only contact IDs
            // Add to the return dict.
            res.Add(niId, contacts);
        }
        return res;
    }
}

