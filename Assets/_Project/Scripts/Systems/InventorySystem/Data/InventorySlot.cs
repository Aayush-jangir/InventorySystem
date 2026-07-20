namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Represents one cell in the inventory grid.
    /// A slot is either empty (ItemInstance is null) or occupied.
    /// All inventory operations go through these slots.
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        private ItemInstance _itemInstance;

        // ── Public properties ─────────────────────────────────────────────

        /// <summary>The item currently in this slot. Null if empty.</summary>
        public ItemInstance ItemInstance => _itemInstance;

        /// <summary>True if this slot contains no item.</summary>
        public bool IsEmpty => _itemInstance == null;

        /// <summary>True if this slot has an item whose stack is not yet full.</summary>
        public bool HasPartialStack =>
            !IsEmpty && !_itemInstance.IsStackFull;

        // ── Public methods ────────────────────────────────────────────────

        /// <summary>
        /// Places an ItemInstance into this slot.
        /// Overwrites whatever was here — call IsEmpty first if needed.
        /// </summary>
        public void SetItem(ItemInstance instance)
        {
            _itemInstance = instance;
        }

        /// <summary>
        /// Removes and returns the ItemInstance from this slot.
        /// Slot becomes empty after this call.
        /// </summary>
        public ItemInstance TakeItem()
        {
            ItemInstance taken = _itemInstance;
            _itemInstance = null;
            return taken;
        }

        /// <summary>Clears this slot without returning the item.</summary>
        public void Clear()
        {
            _itemInstance = null;
        }

        /// <summary>
        /// Returns true if the given ItemDataSO matches the item in this slot.
        /// Safe to call on an empty slot (returns false).
        /// </summary>
        public bool ContainsItemType(ItemDataSO data)
        {
            return !IsEmpty && _itemInstance.Data == data;
        }
    }
}