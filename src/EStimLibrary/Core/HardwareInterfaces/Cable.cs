//namespace EStimLibrary.Core.HardwareInterfaces;

//public class Cable
//{
//    // The leads each individual stimulator output and interface contact is
//    // connected to by this cable. Key by global output and contact IDs,
//    // respectively.
//    protected Dictionary<int, Lead> _OutputToLeadMap;
//    protected Dictionary<int, Lead> _ContactToLeadMap;

//    public Cable()
//    {
//        this._OutputToLeadMap = new();
//        this._ContactToLeadMap = new();
//    }

//    /// <summary>
//    /// Factory method for creating Cables from specific real cable hardware.
//    /// </summary>
//    /// <typeparam name="T">The specific type of ICableHardware to create.
//    /// </typeparam>
//    /// <param name="leadSet">The set of Leads to try and add through this
//    /// Cable.</param>
//    /// <param name="cable">An output parameter: the new Cable created with the
//    /// given Leads according to the restrictions of the specific ICableHardware
//    /// type.</param>
//    /// <returns>True if the Cable was created sucessfully and the object in
//    /// output parameter cable is valid, False if not.</returns>
//    /// <exception cref="NotImplementedException"></exception>
//    public static bool CreateCable<T>(List<Lead> leadSet,
//        out Cable cable) where T : ICableHardware
//    {
//        // Create the new Cable to return, valid if all leads successfully
//        // added.
//        cable = new();

//        // Create an instance of the specific ICableHardware type to validate
//        // requested leads.
//        ICableHardware cableHardware =
//            (ICableHardware)Activator.CreateInstance(typeof(T));

//        // Check to make sure each requested lead is valid for the specific
//        // cable hardware type.
//        bool validLeadSet = true;
//        foreach (var lead in leadSet)
//        {
//            validLeadSet &= cableHardware.IsValidLead(lead, true);
//        }

//        // If all leads valid, add them to the new Cable.
//        if (validLeadSet)
//        {
//            // TODO: catch thrown exceptions from AddLead errors
//            foreach (var lead in leadSet)
//            {
//                cable.AddLead(lead);
//            }
//        }

//        return validLeadSet;
//    }

//    /// TODO: different factory cable create function that is smarter? i.e.,
//    /// just takes in lists of output and contact IDs in the order they should
//    /// be connected, then uses the details within the specific ICableHardware
//    /// type to create the Leads accordingly.
//    public static bool CreateCable<T>(List<int> outputSet, List<int> contactSet,
//        out Cable cable) where T : ICableHardware
//    {
//        throw new NotImplementedException();
//    }

//    /// <summary>
//    /// Create a new circuit between stimulator outputs and interface contacts.
//    /// All outputs and contacts specified must NOT already be connected to by
//    /// the a different lead in this cable.
//    /// To change a connection to any single output or contact connected to by
//    /// this lead, this lead must be removed and a new one created because Leads
//    /// are immutable.
//    /// </summary>
//    /// <param name="outputSet">The global IDs of the outputs connected to by
//    /// this lead.</param>
//    /// <param name="contactSet">The global IDs of the contacts connected to by
//    /// this lead.</param>
//    /// <exception cref="NotImplementedException"></exception>
//    public void AddLead(IEnumerable<int> outputSet, IEnumerable<int> contactSet)
//    {
//        // Error if any outputs or contacts are already wired.
//        if (outputSet.Intersect(this._OutputToLeadMap.Keys.ToList()).Count() != 0 ||
//            contactSet.Intersect(this._ContactToLeadMap.Keys.ToList()).Count() != 0)
//        {
//            throw new NotImplementedException();    // TODO
//        }

//        // Create a Lead based on the output and contact sets given.
//        var lead = new Lead(new(outputSet), new(contactSet));

//        // Store each output and contact as being wired by the new lead.
//        // TODO
//    }

//    /// <summary>
//    /// Add the given Lead to this Cable. All outputs and contacts specified
//    /// must NOT already be connected to by the a different lead in this cable.
//    /// To change a connection to any single output or contact connected to by
//    /// this lead, this lead must be removed and a new one created because Leads
//    /// are immutable.
//    /// </summary>
//    /// <param name="lead">The Lead to try and add.</param>
//    /// <exception cref="NotImplementedException"></exception>
//    public void AddLead(Lead lead)
//    {
//        // Error if any outputs or contacts are already wired.
//        if (lead.OutputSet.Intersect(this._OutputToLeadMap.Keys.ToList()).Count() != 0 ||
//            lead.ContactSet.Intersect(this._ContactToLeadMap.Keys.ToList()).Count() != 0)
//        {
//            throw new NotImplementedException();    // TODO
//        }

//        // Add each output to internal structures.
//        // TODO: depending on if decide Lead is class, struct, whatever, make a
//        // copy of the lead and add that rather than adding the parameter lead.
//        foreach (var outputId in lead.OutputSet)
//        {
//            this._OutputToLeadMap.Add(outputId, lead);
//        }

//        // Add each contact to internal structures.
//        // TODO: depending on if decide Lead is class, struct, whatever, make a
//        // copy of the lead and add that rather than adding the parameter lead.
//        foreach (var contactId in lead.ContactSet)
//        {
//            this._ContactToLeadMap.Add(contactId, lead);
//        }
//    }

//    /// <summary>
//    /// Get the lead with which the given output is wired.
//    /// </summary>
//    /// <param name="outputId">The global output ID of the output in question.
//    /// </param>
//    /// <param name="lead">An output parameter. The desired Lead, if the output
//    /// ID is found in this Cable.</param>
//    /// <returns>True if the output ID is found in this cable and the Lead is
//    /// successfully retrieved, False if not.</returns>
//    public bool GetLeadOfOutput(int outputId, out Lead lead)
//    {
//        return this._OutputToLeadMap.TryGetValue(outputId, out lead);
//    }

//    /// <summary>
//    /// Get the lead with which the given contact is wired.
//    /// </summary>
//    /// <param name="contactId">The global contact ID of the contact in question.
//    /// </param>
//    /// <param name="lead">An output parameter. The desired Lead, if the contact
//    /// ID is found in this Cable.</param>
//    /// <returns>True if the contact ID is found in this cable and the Lead is
//    /// successfully retrieved, False if not.</returns>
//    public bool GetLeadOfContact(int contactId, out Lead lead)
//    {
//        return this._ContactToLeadMap.TryGetValue(contactId, out lead);
//    }

//    /// <summary>
//    /// Remove the Lead with which the given output or contact is wired.
//    /// </summary>
//    /// <param name="id">The global ID of the output or contact to search by.
//    /// </param>
//    /// <param name="searchIsAnOutput">True if the given search ID is of an
//    /// output, False if the given search ID is of a contact.</param>
//    /// <param name="lead">An output parameter: the desired Lead that was
//    /// removed from this Cable. Only valid if the output ID is found in this
//    /// Cable.</param>
//    /// <returns>True if the output or contact ID is found in this Cable and the
//    /// Lead is successfully removed, False if not.</returns>
//    public bool RemoveLead(int id, bool searchIsAnOutput, out Lead lead)
//    {
//        var validId = false;
//        // Search by output or contact ID, respectively.
//        if (searchIsAnOutput)
//        {
//            validId = this._OutputToLeadMap.TryGetValue(id, out lead);
//        }
//        else
//        {
//            validId = this._ContactToLeadMap.TryGetValue(id, out lead);
//        }

//        // If the ID lookup was successful, get the other outputs and contacts
//        // in the lead to remove.
//        var success = false;
//        if (validId)
//        {
//            lead.GetConnectedOutputs(id, searchIsAnOutput, out var connectedOutputs);
//            lead.GetConnectedContacts(id, !searchIsAnOutput, out var connectedContacts);

//            // Remove each output from internal structures.
//            foreach (var outputId in connectedOutputs)
//            {
//                this._OutputToLeadMap.Remove(outputId);
//            }

//            // Remove each contact from internal structures.
//            foreach (var contactId in connectedContacts)
//            {
//                this._ContactToLeadMap.Remove(contactId);
//            }

//            // Determine if removal was successful, i.e., connected outputs
//            // and contacts removed.
//            success = (connectedOutputs.Intersect(this._OutputToLeadMap.Keys.ToList()).Count() == 0 &&
//                connectedContacts.Intersect(this._ContactToLeadMap.Keys.ToList()).Count() == 0);
//        }

//        // Return if the search ID was valid and all associated IDs were
//        // removed.
//        return validId && success;
//    }
//}

