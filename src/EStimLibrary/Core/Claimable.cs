namespace EStimLibrary.Core;

// TODO: change name to ClaimableResource? or IClaimable or IClaimableResource
// even though it's an abstract class not an interface?
public abstract class Claimable
{
    // TODO: implement locking, mutex, or something that can claim exclusive use
    // of this pool until choosing to release it

    public Claimable() { }

    public void Claim()
    {
        throw new NotImplementedException();
    }

    public bool IsClaimed()
    {
        throw new NotImplementedException();
    }

    public void Release()
    {
        throw new NotImplementedException();
    }
}

