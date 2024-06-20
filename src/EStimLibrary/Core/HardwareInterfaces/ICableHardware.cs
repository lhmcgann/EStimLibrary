namespace EStimLibrary.Core.HardwareInterfaces;

// TODO: create an example derived cable HW type
// the cable type should specify/restrict the count and multiplicity of
// leads, as well as the count of how many leads of each specific
// multiplicity supported, but not necessarily assign them to outputs
// or contacts, that is done here.

public interface ICableHardware : ISelectable
{
    /// <summary>
    /// Check if a given requested Lead is valid within the restrictions of the
    /// specific cable hardware.
    /// </summary>
    /// <param name="lead">The Lead to validate.</param>
    /// <param name="markAsUsedIfValid">True to mark the Lead as used
    /// internally if it's valid and thus affect the next Lead validation,
    /// False if not.</param>
    /// <returns>True if valid, False if not.</returns>
    public bool IsValidLead(Lead lead, bool markAsUsedIfValid);
}

