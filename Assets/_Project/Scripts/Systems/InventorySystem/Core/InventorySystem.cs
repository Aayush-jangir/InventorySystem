using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Core inventory manager. Owns the slot array and handles all
    /// inventory operations: Add, Remove, Swap, Split, and Drop.
    /// Communicates results through InventoryEvents — no direct UI references.
    /// Attach to a persistent GameObject in the scene (e.g. GameManager).
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InventoryConfigSO _config;

        [Header("Drop Settings")]
        [SerializeField] private Transform _dropSpawnPoint;

        private InventorySlot[] _slots;
        private bool _isOpen;

        // ── Constants ─────────────────────────────────────────────────────

        private const string ERROR_NO_CONFIG = "[InventorySystem] No InventoryConfigSO assigned.";

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError(ERROR_NO_CONFIG);
                return;
            }

            InitialiseSlots();
        }

        private void OnEnable()
        {
            InventoryEvents.OnWorldItemPickedUp += HandleWorldItemPickedUp;
        }

        private void OnDisable()
        {
            InventoryEvents.OnWorldItemPickedUp -= HandleWorldItemPickedUp;
        }

        // ── Initialisation ────────────────────────────────────────────────

        private void InitialiseSlots()
        {
            _slots = new InventorySlot[_config.TotalSlots];
            for (int i = 0; i < _slots.Length; i++)
                _slots[i] = new InventorySlot();
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the slot at the given index. Null if index is out of range.
        /// </summary>
        public InventorySlot GetSlot(int index)
        {
            if (!IsValidIndex(index)) return null;
            return _slots[index];
        }

        // Add HotbarStartIndex property directly below SlotCount
        /// <summary>Returns the total number of slots in this inventory.</summary>
        public int SlotCount => _slots != null ? _slots.Length : 0;

        /// <summary>
        /// The slot index at which hotbar slots begin.
        /// Slots 0 to HotbarStartIndex-1 are grid slots.
        /// Slots HotbarStartIndex to SlotCount-1 are hotbar slots.
        /// </summary>
        public int HotbarStartIndex => _config != null ? _config.HotbarStartIndex : 0;

        /// <summary>
        /// Attempts to add an item to the inventory.
        /// First tries to stack onto existing partial stacks, then fills empty slots.
        /// Returns true if all items were added, false if the inventory was full.
        /// </summary>
        // NEW
        public bool AddItem(ItemDataSO data, int amount = 1)
        {
            if (data == null || amount <= 0) return false;

            int remaining = amount;
            int hotbarStart = _config.HotbarStartIndex;

            // Pass 1 — stack onto existing partial stacks, hotbar priority first
            if (data.MaxStackSize > 1)
            {
                remaining = FillPartialStacks(data, remaining, hotbarStart, _slots.Length);
                remaining = FillPartialStacks(data, remaining, 0, hotbarStart);
            }

            // Pass 2 — fill empty slots, hotbar priority first, then grid
            remaining = FillEmptySlots(data, remaining, hotbarStart, _slots.Length);
            remaining = FillEmptySlots(data, remaining, 0, hotbarStart);

            return remaining == 0;
        }

        /// <summary>
        /// Removes the given amount from the slot at slotIndex.
        /// Clears the slot if the stack reaches zero.
        /// Returns true if the removal was successful.
        /// </summary>
        public bool RemoveFromSlot(int slotIndex, int amount = 1)
        {
            if (!IsValidIndex(slotIndex)) return false;

            InventorySlot slot = _slots[slotIndex];
            if (slot.IsEmpty || amount <= 0) return false;

            slot.ItemInstance.RemoveFromStack(amount);

            if (slot.ItemInstance.StackCount <= 0)
                slot.Clear();

            InventoryEvents.SlotChanged(slotIndex);
            return true;
        }

        /// <summary>
        /// Swaps the contents of two slots.
        /// If both slots contain the same item type and the target has space,
        /// items are stacked instead of swapped.
        /// </summary>
        public void SwapSlots(int fromIndex, int toIndex)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex)) return;
            if (fromIndex == toIndex) return;

            InventorySlot from = _slots[fromIndex];
            InventorySlot to = _slots[toIndex];

            // If both have the same item type — try to stack
            if (!from.IsEmpty && !to.IsEmpty &&
                from.ItemInstance.Data == to.ItemInstance.Data)
            {
                int overflow = to.ItemInstance.AddToStack(from.ItemInstance.StackCount);

                if (overflow == 0)
                {
                    from.Clear();
                }
                else
                {
                    from.ItemInstance.RemoveFromStack(
                        from.ItemInstance.StackCount - overflow);
                }

                InventoryEvents.SlotChanged(fromIndex);
                InventoryEvents.SlotChanged(toIndex);
                return;
            }

            // Otherwise — plain swap
            ItemInstance temp = from.TakeItem();
            from.SetItem(to.TakeItem());
            to.SetItem(temp);

            InventoryEvents.SlotChanged(fromIndex);
            InventoryEvents.SlotChanged(toIndex);
        }

        /// <summary>
        /// Splits the stack at slotIndex by splitAmount.
        /// The original slot keeps the remainder.
        /// The split portion is placed in the first available empty slot.
        /// Returns true if the split succeeded.
        /// </summary>
        public bool SplitStack(int slotIndex, int splitAmount)
        {
            if (!IsValidIndex(slotIndex)) return false;

            InventorySlot source = _slots[slotIndex];
            if (source.IsEmpty || source.ItemInstance.StackCount <= 1) return false;

            int emptyIndex = FindFirstEmptySlot();
            if (emptyIndex < 0)
            {
                Debug.LogWarning("[InventorySystem] Cannot split — no empty slot available.");
                return false;
            }

            ItemInstance splitPortion = source.ItemInstance.Split(splitAmount);
            _slots[emptyIndex].SetItem(splitPortion);

            InventoryEvents.SlotChanged(slotIndex);
            InventoryEvents.SlotChanged(emptyIndex);
            return true;
        }

        /// <summary>
        /// Drops the entire contents of a slot into the world.
        /// Fires OnItemDroppedToWorld for ItemDropper to handle spawning.
        /// Clears the slot after dropping.
        /// </summary>
        public void DropSlot(int slotIndex)
        {
            if (!IsValidIndex(slotIndex)) return;

            InventorySlot slot = _slots[slotIndex];
            if (slot.IsEmpty) return;

            Vector3 spawnPosition = _dropSpawnPoint != null
                ? _dropSpawnPoint.position
                : Vector3.zero;

            ItemInstance dropped = slot.TakeItem();
            InventoryEvents.ItemDroppedToWorld(dropped, spawnPosition);
            InventoryEvents.SlotChanged(slotIndex);
        }

        /// <summary>
        /// Opens the inventory panel and fires OnInventoryOpened.
        /// Does nothing if already open.
        /// </summary>
        public void OpenInventory()
        {
            if (_isOpen) return;
            _isOpen = true;
            InventoryEvents.InventoryOpened();
        }

        /// <summary>
        /// Closes the inventory panel and fires OnInventoryClosed.
        /// Does nothing if already closed.
        /// </summary>
        public void CloseInventory()
        {
            if (!_isOpen) return;
            _isOpen = false;
            InventoryEvents.InventoryClosed();
        }

        /// <summary>Toggles the inventory panel open or closed.</summary>
        public void ToggleInventory()
        {
            if (_isOpen) CloseInventory();
            else OpenInventory();
        }

        /// <summary>Whether the inventory panel is currently open.</summary>
        public bool IsOpen => _isOpen;

        // ── Private helpers ───────────────────────────────────────────────

        private bool IsValidIndex(int index)
        {
            return _slots != null && index >= 0 && index < _slots.Length;
        }

        // NEW — hotbar checked first so split stack land in hotbar when possible
        private int FindFirstEmptySlot()
        {
            int hotbarStart = _config.HotbarStartIndex;

            for (int i = hotbarStart; i < _slots.Length; i++)
                if (_slots[i].IsEmpty) return i;

            for (int i = 0; i < hotbarStart; i++)
                if (_slots[i].IsEmpty) return i;

            return -1;
        }

        // ── Event handlers ────────────────────────────────────────────────

        private void HandleWorldItemPickedUp(WorldItem worldItem)
        {
            bool added = AddItem(worldItem.ItemData, worldItem.StackCount);

            if (!added)
                Debug.LogWarning("[InventorySystem] Inventory full — could not pick up item.");
        }

        // NEW — add both methods, they did not exist before

        /// <summary>
        /// Scans slots from startIndex to endIndex and tops up any partial stacks
        /// that match the given item. Returns remaining amount that did not fit.
        /// </summary>
        private int FillPartialStacks(ItemDataSO data, int remaining, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex && remaining > 0; i++)
            {
                if (_slots[i].ContainsItemType(data) && _slots[i].HasPartialStack)
                {
                    remaining = _slots[i].ItemInstance.AddToStack(remaining);
                    InventoryEvents.SlotChanged(i);
                }
            }
            return remaining;
        }

        /// <summary>
        /// Scans slots from startIndex to endIndex and fills empty slots with the
        /// given item. Returns remaining amount that did not fit.
        /// </summary>
        private int FillEmptySlots(ItemDataSO data, int remaining, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex && remaining > 0; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    int amountForSlot = Mathf.Min(remaining, data.MaxStackSize);
                    _slots[i].SetItem(new ItemInstance(data, amountForSlot));
                    remaining -= amountForSlot;
                    InventoryEvents.SlotChanged(i);
                }
            }
            return remaining;
        }

        // ── Debug helpers (Editor only) ───────────────────────────────────

        [ContextMenu("Debug — Print All Slots")]
        private void DebugPrintSlots()
        {
            if (_slots == null)
            {
                Debug.Log("[InventorySystem] Slots not initialised.");
                return;
            }

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsEmpty)
                    Debug.Log($"Slot {i}: EMPTY");
                else
                    Debug.Log($"Slot {i}: {_slots[i].ItemInstance.Data.ItemName} " +
                              $"x{_slots[i].ItemInstance.StackCount}");
            }
        }
    }
}