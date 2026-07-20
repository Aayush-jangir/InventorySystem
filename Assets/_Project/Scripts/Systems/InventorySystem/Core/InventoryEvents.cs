using System;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Central event hub for the Inventory System.
    /// All systems communicate through these static C# events —
    /// no direct references between systems needed.
    /// 
    /// Usage:
    ///   Subscribe:   InventoryEvents.OnSlotChanged += MyHandler;
    ///   Unsubscribe: InventoryEvents.OnSlotChanged -= MyHandler;
    ///   Fire:        InventoryEvents.SlotChanged(slotIndex);
    /// </summary>
    public static class InventoryEvents
    {
        // ── Inventory Slot Events ─────────────────────────────────────────

        /// <summary>
        /// Fired when the contents of a specific inventory slot change.
        /// Parameter: the index of the slot that changed.
        /// Listeners: InventoryUI, HotbarUI, SaveSystem (future).
        /// </summary>
        public static event Action<int> OnSlotChanged;

        /// <summary>
        /// Fired when all inventory slots should be refreshed at once.
        /// Used after bulk operations like loading a save or clearing the inventory.
        /// </summary>
        public static event Action OnInventoryRefreshed;

        // ── Panel Events ──────────────────────────────────────────────────

        /// <summary>
        /// Fired when the inventory panel is opened.
        /// Listeners: InventoryUI (to show itself), AudioManager (future).
        /// </summary>
        public static event Action OnInventoryOpened;

        /// <summary>
        /// Fired when the inventory panel is closed.
        /// Listeners: InventoryUI (to hide itself), tooltip (to hide itself).
        /// </summary>
        public static event Action OnInventoryClosed;

        // ── Item World Events ─────────────────────────────────────────────

        /// <summary>
        /// Fired when an item is dropped from the inventory into the world.
        /// Parameter 1: the ItemInstance being dropped.
        /// Parameter 2: the world-space position to spawn it at.
        /// Listener: ItemDropper (to spawn the WorldItem prefab).
        /// </summary>
        public static event Action<ItemInstance, UnityEngine.Vector3> OnItemDroppedToWorld;

        /// <summary>
        /// Fired when a WorldItem in the scene is picked up by the player.
        /// Parameter: the WorldItem that was collected.
        /// Listener: InventorySystem (to add the item to a slot).
        /// </summary>
        public static event Action<WorldItem> OnWorldItemPickedUp;

        // ── Hotbar Events ─────────────────────────────────────────────────

        /// <summary>
        /// Fired when the active hotbar slot index changes (keyboard 1–5 or tap).
        /// Parameter: the new active hotbar slot index (0-based).
        /// Listener: HotbarUI (to update the selection highlight).
        /// </summary>
        public static event Action<int> OnHotbarSlotSelected;

        /// <summary>
        /// Fired when a hotbar slot's linked inventory slot index changes.
        /// Parameter 1: hotbar slot index.
        /// Parameter 2: inventory slot index it now points to.
        /// Listener: HotbarUI.
        /// </summary>
        public static event Action<int, int> OnHotbarLinkChanged;

        // ── Static fire methods ───────────────────────────────────────────
        // These are called by InventorySystem and HotbarSystem.
        // Using methods instead of direct event invocation keeps
        // null-check logic in one place.

        /// <summary>Notifies all listeners that the given slot index has changed.</summary>
        public static void SlotChanged(int slotIndex)
            => OnSlotChanged?.Invoke(slotIndex);

        /// <summary>Notifies all listeners to do a full inventory refresh.</summary>
        public static void InventoryRefreshed()
            => OnInventoryRefreshed?.Invoke();

        /// <summary>Notifies all listeners that the inventory panel was opened.</summary>
        public static void InventoryOpened()
            => OnInventoryOpened?.Invoke();

        /// <summary>Notifies all listeners that the inventory panel was closed.</summary>
        public static void InventoryClosed()
            => OnInventoryClosed?.Invoke();

        /// <summary>Notifies ItemDropper to spawn a WorldItem at the given position.</summary>
        public static void ItemDroppedToWorld(ItemInstance item, UnityEngine.Vector3 position)
            => OnItemDroppedToWorld?.Invoke(item, position);

        /// <summary>Notifies InventorySystem that a WorldItem was picked up.</summary>
        public static void WorldItemPickedUp(WorldItem worldItem)
            => OnWorldItemPickedUp?.Invoke(worldItem);

        /// <summary>Notifies HotbarUI that the selected slot index changed.</summary>
        public static void HotbarSlotSelected(int hotbarIndex)
            => OnHotbarSlotSelected?.Invoke(hotbarIndex);

        /// <summary>Notifies HotbarUI that a hotbar link changed.</summary>
        public static void HotbarLinkChanged(int hotbarIndex, int inventorySlotIndex)
            => OnHotbarLinkChanged?.Invoke(hotbarIndex, inventorySlotIndex);
    }
}