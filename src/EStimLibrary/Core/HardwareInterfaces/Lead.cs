using EStimLibrary.Core;


namespace EStimLibrary.Core.HardwareInterfaces;


/// <summary>
/// Create a new circuit between stimulator outputs and interface contacts.
/// To change a connection to any single output or contact connected to by
/// this lead, this lead must be destroyed and a new one created.
/// I.e., leads are immutable.
/// </summary>
/// <param name="ContactSet">The global IDs of the contacts connected to by
/// this lead.</param>
/// <param name="OutputSet">The global IDs of the outputs connected to by
/// this lead. The outputs must be on the same stimulator.
///             ^^^TODO: enforce that somewhere</param>
/// <param name="CurrentDirection">The default current direction for this
/// lead: source or sink.</param>
public record Lead(SortedSet<int> ContactSet, SortedSet<int> OutputSet,
    Constants.CurrentDirection CurrentDirection) :
    IIdentifiable
{
    // Manager-given ID of the lead, -1 if unset.
    public int Id => this._Id;      // IIdentifiable
    internal int _Id = -1;          // to be set by the manager.

    /// <summary>
    /// Get which outputs are connected to a given output or contact by this
    /// Lead.
    /// </summary>
    /// <param name="id">The global ID of the search output or contact.</param>
    /// <param name="searchIsAnOutput">True if the given search ID is of an
    /// output, False if the given search ID is of a contact.</param>
    /// <param name="connectedOutputs">An output parameter: the set of outputs
    /// connected to the searched output or contact. If an output was searched,
    /// the set will exclude that output. If the method returns False, this will
    /// just be the set of all outputs in this Lead.</param>
    /// <returns>True if the given search ID was found in this Lead and the
    /// returned output ID set is valid, False if not.</returns>
    public bool GetConnectedOutputs(int id, bool searchIsAnOutput,
        out SortedSet<int> connectedOutputs)
    {
        // Output the set of outputs even if the requested ID is invalid.
        connectedOutputs = new(this.OutputSet);

        var validId = false;
        // Search by output or contact ID, respectively.
        if (searchIsAnOutput)
        {
            // Exclude the search output ID from the returned set.
            validId = this.OutputSet.Contains(id);
            connectedOutputs.ExceptWith(new int[] { id });
        }
        else
        {
            validId = this.ContactSet.Contains(id);
        }

        // Return T/F if the search ID given was valid and thus if the output
        // connected output set is correct.
        return validId;
    }


    /// <summary>
    /// Get which contacts are connected to a given contact or output by this
    /// Lead.
    /// </summary>
    /// <param name="id">The global ID of the search contact or output.</param>
    /// <param name="searchIsAContact">True if the given search ID is of a
    /// contact, False if the given search ID is of an output.</param>
    /// <param name="connectedContacts">An output parameter: the set of
    /// contacts connected to the searched contact or output. If a contact was
    /// searched, the set will exclude that contact. If the method returns False,
    /// this will just be the set of all contacts in this Lead.</param>
    /// <returns>True if the given search ID was found in this Lead and the
    /// returned contact ID set is valid, False if not.</returns>
    public bool GetConnectedContacts(int id, bool searchIsAContact,
        out SortedSet<int> connectedContacts)
    {
        // Output the set of contacts even if the requested ID is invalid.
        connectedContacts = new(this.ContactSet);

        var validId = false;
        // Search by contact or output ID, respectively.
        if (searchIsAContact)
        {
            validId = this.ContactSet.Contains(id);
            // Exclude the search contact ID from the returned set.
            connectedContacts.ExceptWith(new int[] { id });
        }
        else
        {
            validId = this.OutputSet.Contains(id);
        }

        // Return T/F if the search ID given was valid and thus if the returned
        // connected contact set is correct.
        return validId;
    }

    /// <summary>
    /// Check if two Leads are fully independent, meaning they involve none of
    /// the same contacts or outputs.
    /// </summary>
    /// <param name="other">The other Lead.</param>
    /// <returns>T/F if independent.</returns>
    public bool IsFullyIndependent(Lead other)
    {
        var sameContacts = this.ContactSet.Intersect(other.ContactSet);
        var sameOutputs = this.OutputSet.Intersect(other.OutputSet);
        return (sameContacts.Count() == 0) && (sameOutputs.Count() == 0);
    }

    public static bool IndependentLeadsExist(IEnumerable<Lead> leadSet,
        bool checkCurrentDirection = false)
    {
        var leadList = leadSet.ToList();
        // Nested loops to compare all lead pairs.
        // TODO: better way to do this?
        for (int i = 0; i < leadList.Count; i++)
        {
            var currentLead = leadList[i];
            // Compare current lead to all following leads. Comparisons to leads
            // prior have already been done.
            for (int j = i + 1; j < leadList.Count; j++)
            {
                var otherLead = leadList[j];

                // Return true early if independent lead pair found and:
                // a) not looking for further specs to be met, or
                // b) further specs met: leads have opposite current direction
                if (currentLead.IsFullyIndependent(otherLead) &&
                    (!checkCurrentDirection ||
                    currentLead.CurrentDirection !=
                        otherLead.CurrentDirection))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // TODO: is partially independent relevant? Even possible? Bc all outputs
    // and contacts in a single lead are connected in the same circuit...

    // TODO: override any needed equality check methods so it checks by values;
    // or is this done inherently if a struct or other more value-based data
    // type?
}

