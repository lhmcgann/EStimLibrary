namespace EStimLibrary.Core;


public class ResourceManager<ResourceType>
{
    public ReusableIdPool IdPool { get; protected set; }
    public Dictionary<int, ResourceType> Resources { get; protected set; }

    public readonly int MaxNumResources;

    public int NumTotalResources => this.IdPool.NumUsedIds;

    public ResourceManager(int baseId = 0, int initialNumResourceIds = 0,
        int maxNumResources = Constants.POS_INFINITY)
    {
        this.IdPool = new(baseId, initialNumResourceIds);
        this.Resources = new();
        this.MaxNumResources = maxNumResources;
    }

    /// <summary>
    /// Check if a manager resource currently exists with the given ID.
    /// </summary>
    /// <param name="globalId">The global ID to query.</param>
    /// <returns>True if the queried ID is a) valid within the manager's range
    /// at all, and b) there is currently a resource associated with the ID.
    /// </returns>
    public bool IsValidResourceId(int globalId)
    {
        // TODO: does Used check validity again?
        return this.IdPool.IsValidId(globalId) &&
            this.IdPool.IsUsed(globalId);
    }

    public bool TryGetNextAvailableId(out int globalId)
    {
        // Get the next available ID. If none and if still haven't hit hard max
        // limit on resource count, increase the ID pool max and try again, else
        // return failure.
        while (!this.IdPool.TryGetNextFreeId(out globalId))
        {
            // If max capacity already used, return failure.
            if (!Utils.IsWithinUpperBound(this.IdPool.NumUsedIds,
                this.MaxNumResources))
            {
                return false;
            }
            // Else increment the number of IDs in the pool and try again.
            this.IdPool.IncrementNumIds(1);
        }
        return true;
    }

    public bool TryAddResource(int globalId, ResourceType resource)
    {
        // Fail to add resource if invalid ID or already a resource with that
        // ID.
        if (!this.IdPool.IsValidId(globalId) || !this.IdPool.IsFree(globalId))
        {
            return false;
        }
        // Else, add the resource, mark ID as used, and return success.
        this.Resources.Add(globalId, resource);
        // small TODO: the Use method calls IsValidId again --> any way to
        // refactor to make more efficient?
        this.IdPool.UseId(globalId);
        return true;
    }

    public bool TryRemoveResource(int globalId,
        out ResourceType removedResource)
    {
        // Fail to remove resource if invalid ID or there's not a resource with
        // that ID.
        if (!this.IsValidResourceId(globalId))
        {
            removedResource = default;
            return false;
        }
        // Else, try removing the resource.
        bool removed = this.Resources.Remove(globalId, out removedResource);
        // Mark as free only if successfully removed.
        if (removed)
        {
            // small TODO: the Free method calls IsValidId again --> any way to
            // refactor to make more efficient?
            this.IdPool.FreeId(globalId);
        }
        // Return the success of removal.
        return removed;
    }

    public bool TryGetResource(int globalId, out ResourceType resource)
    {
        return this.Resources.TryGetValue(globalId, out resource);
    }
}

