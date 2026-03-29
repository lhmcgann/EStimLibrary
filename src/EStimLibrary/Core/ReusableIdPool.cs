using System.ComponentModel.DataAnnotations;

namespace EStimLibrary.Core;


/// <summary>
/// A consecutive block of integer IDs. The block begins at the base ID passed
/// in through the constructor and cannot be changed after that. The block
/// continues up through a certain number of IDs, also passed in at the time of
/// construction but can be changed at any time using class methods. The pool of
/// IDs will always contain the IDs [baseId, baseId+numIds).
/// All IDs start as free but can be marked as used or again freed (unused) in
/// any order at any time.
/// </summary>
public class ReusableIdPool
{
    private static int MIN_BASE_ID = 0;
    private static int MIN_NUM_IDS = 0;

    // Private fields should never be accessed or manipulated directly. Use
    // corresponding properties.

    /// <summary>
    /// The base ID value for this pool. All IDs in the pool will be in a
    /// consecutive block including and after this value, up to NumIds IDs.
    /// Guaranteed to be >= 0 at all times after construction.
    /// </summary>
    private int _baseId;
    /// <summary>
    /// The number of IDs in this pool. Can be changed via class methods.
    /// Guaranteed to be >= 0 at all times after construction.
    /// </summary>
    private int _numIds;

    // Protective BaseId and NumIds properties for the private fields.
    // Ensures limits when setting the values.
    /// <summary>
    /// The lowest ID in this pool. Cannot be below 0.
    /// </summary>
    public int BaseId
    {
        get => this._baseId;
        protected set => this._baseId = Math.Max(MIN_BASE_ID, value);
    }
    /// <summary>
    /// The number of IDs in this pool. Cannot be below 0.
    /// </summary>
    public int NumIds
    {
        get => this._numIds;
        //protected set => this._numIds = Math.Max(MIN_NUM_IDS, value);
        protected set
        {
            long maxAllowableNumIds = (long)Constants.POS_INFINITY - (long)this.BaseId + 1;
            int minAllowableNumIds = MIN_NUM_IDS;

            if (this.UsedIds != null)
            {
                if (this.UsedIds.Count > 0)
                {
                    minAllowableNumIds = this.UsedIds.Max - this.BaseId + 1;
                }
            }

            if (value < minAllowableNumIds) { this._numIds = minAllowableNumIds; }
            else if (value > maxAllowableNumIds) { this._numIds = (int)maxAllowableNumIds; }
            else this._numIds = value;
        }
    }

    /// <summary>
    /// Set of IDs in this pool, generated based on BaseId and NumIds:
    /// [BaseId, BaseId+NumIds)
    /// Empty if no IDs currently in this pool.
    /// </summary>
    public SortedSet<int> Ids
    {
        get
        {
            // Return an empty set if no IDs in this pool.
            if (this.NumIds == 0)
            {
                return new SortedSet<int>();
            }

            // Ensure that baseId + numIds - 1 does not exceed Constants.POS_INFINITY
            if ((long)this.BaseId + (long)this.NumIds - 1 > Constants.POS_INFINITY)
            {
                this.NumIds = Math.Min(NumIds - BaseId, NumIds);
            }
            // Return all IDs in the [BaseId, BaseId+NumIds) range.
            return new(Enumerable.Range(this.BaseId, this.NumIds));
        }
    }

    /// <summary>
    /// The number of IDs in this pool that are currently marked as used.
    /// </summary>
    public int NumUsedIds { get => this.UsedIds.Count; }
    /// <summary>
    /// Set of used IDs. UsedIds is the only set manipulated.
    /// </summary>
    public SortedSet<int> UsedIds { get; protected set; }

    /// <summary>
    /// The number of IDs in this pool that are currently marked as free, or
    /// unsused.
    /// </summary>
    public int NumFreeIds { get => this.FreeIds.Count; }
    /// <summary>
    /// Set of free, or unused, IDs.
    /// </summary>
    public SortedSet<int> FreeIds
    {
        get
        {
            // Return all IDs except the used IDs.
            return new(this.Ids.Except(this.UsedIds));
        }
    }

    /// <summary>
    /// Create a pool of resusable IDs from [baseId, baseId+numIds).
    /// </summary>
    /// <param name="baseId">The base ID at which this pool of IDs will start.
    /// Defaults to 0 if a negative number is given. This value cannot be
    /// changed after construction.</param>
    /// <param name="numIds">The number of IDs in this pool. Defaults to 0 if a
    /// negative number is given. Can be changed after construction. Will be
    /// bounded by ((max possible integer value) - baseId + 1).</param>
    public ReusableIdPool(int baseId, int numIds)
    {
        // Ensure BaseId is not negative
        this.BaseId = baseId;

        this.NumIds = numIds;

        // Initialize UsedIds
        this.UsedIds = new SortedSet<int>();
    }

    /// <summary>
    /// Create an empty pool of reusable IDs. Base ID defaults to 0.
    /// </summary>
    public ReusableIdPool()
    {
        // Base ID and Num IDs = 0.
        this.BaseId = MIN_BASE_ID;
        this.NumIds = MIN_NUM_IDS;

        // Initialize set of used IDs to empty. This is the only set manually
        // that is method-editable not auto-generated.
        this.UsedIds = new();
    }

    /// <summary>
    /// Change the number of IDs in this pool by a specified amount.
    /// </summary>
    /// <param name="increment">The positive or negavtive integer number of IDs
    /// to add or remove from the current pool. Cannot icnrement to beyond the
    /// delta between BaseId and the max possible integer value. Cannot remove
    /// IDs beyond the highest currently used ID.</param>
    /// <returns>The resulting NumIds value.</returns>
    public int IncrementNumIds(int increment)
    {
        this.NumIds += increment;
        return this.NumIds;
    }

    /// <summary>
    /// Overwrite the number of IDs in this pool to a new specified amount.
    /// </summary>
    /// <param name="newMax">The new number of IDs to set in this pool. Will be
    /// bounded by the delta between BaseId and the max possible integer value,
    /// and the highest current used ID.</param>
    /// <returns>The resulting NumIds value.</returns>
    public int ResetNumIds(int newMax)
    {
        this.NumIds = newMax;
        return this.NumIds;
    }

    public bool IsValidId(int id)
    {
        return (this.BaseId <= id) && (id < this.BaseId + this.NumIds);
    }

    public bool TryGetLocalId(int globalId, out int localId)
    {
        // Store local ID no matter what.
        localId = globalId - this.BaseId;
        // Return T/F if the output local ID value is valid or not.
        return this.IsValidId(globalId);
    }

    public bool TryGetGlobalId(int localId, out int globalId)
    {
        // Store global ID no matter what.
        globalId = localId + this.BaseId;
        // Return T/F if the output (computed) global ID value is valid or not.
        return this.IsValidId(globalId);
    }

    public Dictionary<int, int> GetGlobalLocalMap()
    {
        return this.Ids.ToDictionary(globalId => globalId,
            globalId => globalId - this.BaseId);
    }

    /// <summary>
    /// Mark the given ID as used.
    /// </summary>
    /// <param name="globalId">The ID to mark as used.</param>
    /// <returns>True if successful, False if invalid ID.</returns>
    public bool UseId(int globalId)
    {
        if (this.IsValidId(globalId))
        {
            this.UsedIds.Add(globalId);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Mark the given ID as unused, or free.
    /// </summary>
    /// <param name="globalId">The ID to mark as free.</param>
    /// <returns>True if successful or if already free, False if invalid ID.
    /// </returns>
    public bool FreeId(int globalId)
    {
        // Checks if the globalId is in range of the pool IDs
        if (!this.IsValidId(globalId))
        {
            return false;
        }
        // If ID currently used, return bool success of the removal operation.
        if (this.IsUsed(globalId))
        {
            return this.UsedIds.Remove(globalId);
        }
        // If ID not used in the first place, return true as well.
        return true;
    }

    /// <summary>
    /// Check if a given ID is marked as used.
    /// </summary>
    /// <param name="globalId">The ID to check.</param>
    /// <returns>True if the ID is used, False if the ID is invalid or unused.
    /// </returns>
    public bool IsUsed(int globalId)
    {
        return this.UsedIds.TryGetValue(globalId, out _);
    }

    public bool IsFree(int globalId)
    {
        // GENERAL TODO: when/where should ID validation be done? split IsValid
        // and IsFree (or whatever other check) into separate methods? E.g.,
        // IsValid is public and gets used in the public IsFree check, but that
        // check also uses the internal _IsFree check which skips validation so
        // you don't have to call validation repeatedly on the same value...

        return this.IsValidId(globalId) && !this.IsUsed(globalId);
    }

    public bool TryGetNextFreeId(out int id)
    {
        try
        {
            id = this.FreeIds.ToArray()[0];
            return true;
        }
        // In case there are no free IDs.
        catch
        {
            id = -1;
            return false;
        }
    }

    public SortedSet<int> GetSubset(int startId, int numIds)
    {
        if (numIds <= 0)
        {
            return new SortedSet<int>();
        }
        if (this.IsValidId(startId))
        {
            // Find the *inclusive* end ID bound.
            int endId = Math.Min(startId + numIds,
                                 this.BaseId + this.NumIds) - 1;
            return this.Ids.GetViewBetween(startId, endId);
        }
        // Return empty set if invalid ID request.
        return new();
    }
}

