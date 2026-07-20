using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Manages the hotbar — a row of dedicated inventory slots that are always
    /// visible on screen. Hotbar slot i maps permanently to inventory slot
    /// (HotbarStartIndex + i). No linking or reassignment — the mapping is fixed.
    /// Attach to the same GameObject as InventorySystem.
    /// </summary>
    public class HotbarSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InventoryConfigSO _config;
        [SerializeField] private InventorySystem _inventorySystem;

        private int _activeHotbarIndex;

        // ── Constants ─────────────────────────────────────────────────────

        private const string ERROR_NO_CONFIG = "[HotbarSystem] No InventoryConfigSO assigned.";
        private const string ERROR_NO_INVENTORY = "[HotbarSystem] No InventorySystem assigned.";

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError(ERROR_NO_CONFIG);
                return;
            }

            if (_inventorySystem == null)
            {
                Debug.LogError(ERROR_NO_INVENTORY);
                return;
            }

            _activeHotbarIndex = 0;
        }

        private void OnEnable()
        {
            InventoryEvents.OnSlotChanged += HandleSlotChanged;
        }

        private void OnDisable()
        {
            InventoryEvents.OnSlotChanged -= HandleSlotChanged;
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the inventory slot index that the given hotbar slot maps to.
        /// Hotbar slot 0 always maps to HotbarStartIndex + 0, and so on.
        /// Returns -1 if hotbarIndex is out of range.
        /// </summary>
        public int GetLinkedInventoryIndex(int hotbarIndex)
        {
            if (!IsValidHotbarIndex(hotbarIndex)) return -1;
            return _inventorySystem.HotbarStartIndex + hotbarIndex;
        }

        /// <summary>
        /// Returns the InventorySlot for the given hotbar slot index.
        /// Returns null if the index is invalid.
        /// </summary>
        public InventorySlot GetLinkedSlot(int hotbarIndex)
        {
            int inventoryIndex = GetLinkedInventoryIndex(hotbarIndex);
            if (inventoryIndex < 0) return null;
            return _inventorySystem.GetSlot(inventoryIndex);
        }

        /// <summary>
        /// Sets the active hotbar slot (e.g. player pressed key 1–5).
        /// Fires OnHotbarSlotSelected so HotbarUI can update the highlight.
        /// </summary>
        public void SelectHotbarSlot(int hotbarIndex)
        {
            if (!IsValidHotbarIndex(hotbarIndex)) return;
            if (_activeHotbarIndex == hotbarIndex) return;

            _activeHotbarIndex = hotbarIndex;
            InventoryEvents.HotbarSlotSelected(_activeHotbarIndex);
        }

        /// <summary>The currently selected hotbar slot index (0-based).</summary>
        public int ActiveHotbarIndex => _activeHotbarIndex;

        /// <summary>Total number of hotbar slots, driven by InventoryConfigSO.</summary>
        public int HotbarSlotCount => _config != null ? _config.HotbarSlotCount : 0;

        // ── Private helpers ───────────────────────────────────────────────

        private bool IsValidHotbarIndex(int index)
        {
            return _config != null && index >= 0 && index < _config.HotbarSlotCount;
        }

        // ── Event handlers ────────────────────────────────────────────────

        private void HandleSlotChanged(int inventorySlotIndex)
        {
            // Only react if the changed slot is inside the hotbar range
            int hotbarStart = _inventorySystem.HotbarStartIndex;
            int hotbarEnd = hotbarStart + _config.HotbarSlotCount;

            if (inventorySlotIndex < hotbarStart || inventorySlotIndex >= hotbarEnd) return;

            int hotbarIndex = inventorySlotIndex - hotbarStart;
            InventoryEvents.HotbarLinkChanged(hotbarIndex, inventorySlotIndex);
        }

        // ── Debug helpers (Editor only) ───────────────────────────────────

        [ContextMenu("Debug — Print Hotbar Slots")]
        private void DebugPrintSlots()
        {
            if (_inventorySystem == null || _config == null)
            {
                Debug.Log("[HotbarSystem] Not ready.");
                return;
            }

            for (int i = 0; i < _config.HotbarSlotCount; i++)
            {
                InventorySlot slot = GetLinkedSlot(i);
                string active = (i == _activeHotbarIndex) ? " ◄ ACTIVE" : "";
                string contents = slot == null || slot.IsEmpty
                    ? "EMPTY"
                    : $"{slot.ItemInstance.Data.ItemName} x{slot.ItemInstance.StackCount}";

                Debug.Log($"Hotbar[{i}] → Inventory[{GetLinkedInventoryIndex(i)}]: {contents}{active}");
            }
        }
    }
}