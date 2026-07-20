using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Represents one runtime stack of an item inside the inventory.
    /// Combines a reference to the item's static data (ItemDataSO)
    /// with the current stack count for this particular instance.
    /// </summary>
    [System.Serializable]
    public class ItemInstance
    {
        [SerializeField] private ItemDataSO _data;
        [SerializeField] private int _stackCount;

        // ── Constructor ───────────────────────────────────────────────────

        /// <summary>
        /// Creates a new ItemInstance with the given data and stack count.
        /// Stack count is clamped between 1 and the item's max stack size.
        /// </summary>
        public ItemInstance(ItemDataSO data, int stackCount = 1)
        {
            _data = data;
            _stackCount = Mathf.Clamp(stackCount, 1, data.MaxStackSize);
        }

        // ── Public properties ─────────────────────────────────────────────

        /// <summary>The static blueprint this instance references.</summary>
        public ItemDataSO Data => _data;

        /// <summary>How many of this item are in this stack.</summary>
        public int StackCount => _stackCount;

        /// <summary>True if this stack can accept more items of the same type.</summary>
        public bool IsStackFull => _stackCount >= _data.MaxStackSize;

        /// <summary>How many more items this stack can accept before it is full.</summary>
        public int RemainingSpace => _data.MaxStackSize - _stackCount;

        // ── Public methods ────────────────────────────────────────────────

        /// <summary>
        /// Adds the given amount to this stack.
        /// Returns how many items could NOT be added (overflow).
        /// </summary>
        public int AddToStack(int amount)
        {
            int spaceAvailable = RemainingSpace;
            int amountToAdd = Mathf.Min(amount, spaceAvailable);
            _stackCount += amountToAdd;
            return amount - amountToAdd;   // leftover that did not fit
        }

        /// <summary>
        /// Removes the given amount from this stack.
        /// Stack count will never go below zero.
        /// </summary>
        public void RemoveFromStack(int amount)
        {
            _stackCount = Mathf.Max(0, _stackCount - amount);
        }

        /// <summary>
        /// Creates a new ItemInstance split from this one.
        /// Reduces this stack by splitAmount and returns a new instance
        /// with that amount. splitAmount is clamped so at least 1 remains here.
        /// </summary>
        public ItemInstance Split(int splitAmount)
        {
            int maxSplit = _stackCount - 1;            // must leave at least 1 here
            int actual = Mathf.Clamp(splitAmount, 1, maxSplit);
            _stackCount -= actual;
            return new ItemInstance(_data, actual);
        }
    }
}