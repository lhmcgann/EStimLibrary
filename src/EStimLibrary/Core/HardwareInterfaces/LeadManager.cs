namespace EStimLibrary.Core.HardwareInterfaces;


/// <summary>
/// LeadManager represents a collection of physical electrical contacts that are
/// wired to specific stimulator outputs via leads.
/// </summary>
public class LeadManager : ResourceManager<Lead>
{
    // Some key properties are inherited from the parent class ResourceManager.
    // The Lead objects themselves are stored in Resources.
    // The ID pool is IdPool.
    // If a managed Lead resource exists, its ID should be marked as Used.
    // Free IDs can exist in the pool, e.g., if a Lead was added then removed, 
    // but there must be no managed resource in Resources under that ID.

    /// <summary>
    /// The global contact IDs that are currently wired to one or more leads.
    /// Only valid IDs are added.
    /// </summary>
    internal SortedSet<int> _WiredContacts { get; private set; }
    /// <summary>
    /// The global output IDs that are currently wired to one or more leads.
    /// Only valid IDs are added.
    /// </summary>
    internal SortedSet<int> _WiredOutputs { get; private set; }

    protected Dictionary<int, SortedSet<int>> _ContactLeadIdMap;
    protected Dictionary<int, SortedSet<int>> _OutputLeadIdMap;

    //TODO? public Dictionary<int, int> ContactOutputMap { get; protected set; }

    public LeadManager()
    {
        // Initialize wired ID sets.
        this._WiredContacts = new();
        this._WiredOutputs = new();

        // Initialize empty dictionaries and a zero total count of contacts.
        this._ContactLeadIdMap = new();
        this._OutputLeadIdMap = new();

        //this.ContactOutputMap = new();
    }

    /// <summary>
    /// Check if a contact has been wired (i.e., attached to 1+ leads).
    /// </summary>
    /// <param name="contactId">The global int ID of the contact in question.
    /// </param>
    /// <returns>True if the contact ID is valid and the contact is wired.
    /// False if invalid or not wired.</returns>
    public bool IsWiredContact(int contactId)
    {
        // Inherent ID validation bc only valid IDs will be added to
        // WiredContacts.
        return this._WiredContacts.Contains(contactId);
    }

    /// <summary>
    /// Check if an output has been wired (i.e., attached to 1+ leads).
    /// </summary>
    /// <param name="outputId">The global int ID of the output in question.
    /// </param>
    /// <returns>True if the output ID is valid and the contact is wired.
    /// False if invalid or not wired.</returns>
    public bool IsWiredOutput(int outputId)
    {
        return this._WiredOutputs.Contains(outputId);
    }

    /// <summary>
    /// Try adding a new lead to this interface config. Upon success, all of
    /// the lead's contacts and outputs will be marked as 'wired' if they are
    /// not already.
    /// </summary>
    /// <param name="lead">The lead to add. Assumes all contact and output IDs
    /// in the lead have already been validated.</param>
    /// <param name="leadId">An output parameter: the global int ID given to
    /// the new lead upon successful addition. -1 if fails.</param>
    /// <returns>True if the lead could be added, False if not.</returns>
    public bool TryAddLead(Lead lead, out int leadId)
    {
        // Get the next available lead ID and try adding the lead to the
        // resource pool.
        if (!this.TryGetNextAvailableId(out leadId) ||
            !this.TryAddResource(leadId, lead))
        {
            // Return early if failed.
            return false;
        }

        // Actually set the lead's ID.
        lead._Id = leadId;

        // Update extra internal structs. For all contacts and outputs:
        // a) Mark as wired.
        this._WiredContacts.UnionWith(lead.ContactSet);
        this._WiredOutputs.UnionWith(lead.OutputSet);
        // b) Add the lead ID to their list of leads.
        // TODO: why need to make a copy to avoid collection mod? no mod tho...
        //  AND only an issue during debug
        foreach (var contactId in lead.ContactSet.ToList())
        {
            // Add a new lead set for this contact if it doesn't have one.
            if (!this._ContactLeadIdMap.TryGetValue(contactId,
                out var contactRelatedLeadIds))
            {
                contactRelatedLeadIds = new();
                this._ContactLeadIdMap.Add(contactId, contactRelatedLeadIds);
            }
            // Add new lead ID to the contact's set.
            contactRelatedLeadIds.Add(leadId);
        }
        // TODO: why need to make a copy to avoid collection mod? no mod tho...
        //  AND only an issue during debug
        foreach (var outputId in lead.OutputSet.ToList())
        {
            // Add a new lead set for this output if it doesn't have one.
            if (!this._OutputLeadIdMap.TryGetValue(outputId,
                out var outputRelatedLeadIds))
            {
                outputRelatedLeadIds = new();
                this._OutputLeadIdMap.Add(outputId, outputRelatedLeadIds);
            }
            // Add new lead ID to the output's set.
            outputRelatedLeadIds.Add(leadId);
        }
        // Return success.
        return true;
    }

    /// <summary>
    /// Get all Leads wired to a specific contact.
    /// </summary>
    /// <param name="contactId">The global contact ID to query.</param>
    /// <returns>A list of all Leads attached to the given contact. An empty
    /// list if the queried contact ID is invalid or un-wired.</returns>
    public List<Lead> GetLeadsOfContact(int contactId)
    {
        List<Lead> leads = new();
        if (this._ContactLeadIdMap.TryGetValue(contactId, out var leadIds))
        {
            foreach (var leadId in leadIds)
            {
                // Using [] get syntax because assuming lead IDs stored
                // internally are valid.
                leads.Add(this.Resources[leadId]);
            }
        }
        return leads;
    }
    /// <summary>
    /// Get all Leads wired to a specific output.
    /// </summary>
    /// <param name="outputId">The global output ID to query.</param>
    /// <returns>A list of all Leads attached to the given output. An empty
    /// list if the queried output ID is invalid or un-wired.</returns>
    public List<Lead> GetLeadsOfOutput(int outputId)
    {
        List<Lead> leads = new();
        if (this._OutputLeadIdMap.TryGetValue(outputId, out var leadIds))
        {
            foreach (var leadId in leadIds)
            {
                // Using [] get syntax because assuming lead IDs stored
                // internally are valid.
                leads.Add(this.Resources[leadId]);
            }
        }
        return leads;
    }

    /// <summary>
    /// Fully remove a lead from this manager, including all object and ID
    /// references. Lead lookup based on global ID.
    /// </summary>
    /// <param name="leadId">The global ID of the lead to remove.</param>
    /// <param name="removedLead">An output parameter: the Lead removed. Only
    /// contains valid data if the method returns true, otherwise may be null.
    /// </param>
    /// <returns>True if the queried lead ID was valid and the lead could be
    /// removed. False if not.</returns>
    public bool TryRemoveLead(int leadId, out Lead removedLead)
    {
        // Note: this method is basically just a wrapper for the inherited
        // ResourceManager<Lead>.TryRemoveResource method. This method also
        // handles removal of all lead references, i.e., the contact and
        // output lead sets 'wired' status.

        // Try removing lead from resource pool and marking ID as free.
        if (!this.TryRemoveResource(leadId, out removedLead))
        {
            // Return early upon failure (e.g., invalid resource ID).
            return false;
        }

        // Update extra internal structs: for all contacts and outputs of the
        // removed lead, remove the lead ID from their lead sets, and if
        // removing the lead leaves the contact or output unwired, mark as such.
        foreach (var contactId in removedLead.ContactSet)
        {
            // Remove the lead ID from the contact's lead set.
            var leadSet = this._ContactLeadIdMap[contactId];
            leadSet.Remove(leadId);
            // Remove the contact from the "wired" set if no leads left on it.
            if (leadSet.Count == 0)
            {
                this._WiredContacts.Remove(contactId);
            }
        }
        foreach (var outputId in removedLead.OutputSet)
        {
            // Remove the lead ID from the output's lead set.
            var leadSet = this._OutputLeadIdMap[outputId];
            leadSet.Remove(leadId);
            // Remove the output from the "wired" set if no leads left on it.
            if (leadSet.Count == 0)
            {
                this._WiredOutputs.Remove(outputId);
            }
        }

        // Return success.
        return true;
    }

    /// <summary>
    /// Fully remove a lead from this manager, including all object and ID
    /// references. Lead lookup based on lead value.
    /// </summary>
    /// <param name="leadId">An output parameter: the global ID of the lead
    /// removed. Only contains valid data if a lead value match could be found,
    /// otherwise -1.</param>
    /// <param name="removedLead">An output parameter: the Lead removed. Only
    /// contains valid data if the method returns true, otherwise may be null.
    /// </param>
    /// <returns>True if the queried lead could be found and removed. False if
    /// not.</returns>
    public bool TryRemoveLead(Lead lead, out int leadId, out Lead removedLead)
    {
        // Try to get the ID of the Lead (Leads are records: Equals by value).
        // TODO: actually test if this int? and FirstOrDefault strategy works.
        // Was having some troubles with it in another class where HasValue
        // returned true but only because the default value (stored upon lookup
        // failure) was technically a valid value by that check --> TEST TEST!!!
        int? id = this.Resources.FirstOrDefault(kv => kv.Value.Equals(lead)).Key;
        if (id.HasValue)
        {
            // Store the ID of the lead to be removed.
            leadId = (int)id;

            // Full lead removal, including references per contact and output.
            this.TryRemoveLead(leadId, out removedLead);

            // Return success.
            return true;
        }
        // Return failure.
        leadId = -1;
        removedLead = null;
        return false;
    }


    // TODO: figure out how to read and write to files
    // TODO: make these just to/from string or specifically CSV string methods
    // so I/O calls can be handled by another class
    public static void SaveMapToCSV(string outfile)
    {
    }

    public static LeadManager LoadMapFromCSV(string infile)
    {
        throw new NotImplementedException();
    }
}

