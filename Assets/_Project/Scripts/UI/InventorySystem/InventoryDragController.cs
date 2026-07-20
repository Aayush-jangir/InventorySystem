using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Singleton controller for inventory drag and drop operations.
    /// Owns all drag state and resolves what happens when a slot is dropped.
    /// Other scripts call BeginDrag, UpdateDrag, and EndDrag — this class
    /// decides the outcome and calls the appropriate InventorySystem methods.
    /// </summary>
    public class InventoryDragController : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────

        /// <summary>Global access point for the drag controller.</summary>
        public static InventoryDragController Instance { get; private set; }

        // ── Inspector references ───────────────────────────────────────────

        [Header("References")]
        [SerializeField] private InventorySystem _inventorySystem;
        [SerializeField] private InventoryConfigSO _config;
        [SerializeField] private DragGhostUI _dragGhost;

        // ── Drag state ─────────────────────────────────────────────────────

        private bool _isDragging;
        private int _originSlotIndex = -1;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_INVENTORY = "[InventoryDragController] No InventorySystem assigned.";
        private const string ERROR_NO_CONFIG = "[InventoryDragController] No InventoryConfigSO assigned.";
        private const string ERROR_NO_GHOST = "[InventoryDragController] No DragGhostUI assigned.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[InventoryDragController] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_inventorySystem == null) Debug.LogError(ERROR_NO_INVENTORY);
            if (_config == null) Debug.LogError(ERROR_NO_CONFIG);
            if (_dragGhost == null) Debug.LogError(ERROR_NO_GHOST);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Begins a drag from the given inventory slot index.
        /// Activates the drag ghost with the slot's item icon.
        /// Does nothing if the slot is empty or a drag is already in progress.
        /// </summary>
        public void BeginDrag(int slotIndex, Vector2 screenPosition)
        {
            if (_isDragging) return;
            if (_inventorySystem == null) return;

            InventorySlot slot = _inventorySystem.GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty) return;

            _isDragging = true;
            _originSlotIndex = slotIndex;

            _dragGhost.Show(slot.ItemInstance, _config, screenPosition);
        }

        /// <summary>
        /// Updates the ghost position to follow the pointer.
        /// Called every frame during a drag by InventorySlotDragHandler.
        /// </summary>
        public void UpdateDrag(Vector2 screenPosition)
        {
            if (!_isDragging) return;
            _dragGhost.UpdatePosition(screenPosition);
        }

        /// <summary>
        /// Ends the drag and attempts to drop onto the target inventory slot.
        /// If target slot index is -1 or the drop is invalid, drag is cancelled.
        /// </summary>
        public void EndDragOnInventorySlot(int targetSlotIndex)
        {
            if (!_isDragging) return;

            if (targetSlotIndex >= 0 && targetSlotIndex != _originSlotIndex)
                _inventorySystem.SwapSlots(_originSlotIndex, targetSlotIndex);

            CancelDrag();
        }

        /// <summary>
        /// Ends the drag and re-links the target hotbar slot to the origin inventory slot.
        /// </summary>
        // NEW — drag to hotbar slot now SWAPS items instead of re-linking
        public void EndDragOnHotbarSlot(int hotbarSlotIndex, HotbarSystem hotbarSystem)
        {
            if (!_isDragging) return;

            if (hotbarSlotIndex >= 0 && hotbarSystem != null)
            {
                int targetSlotIndex = hotbarSystem.GetLinkedInventoryIndex(hotbarSlotIndex);

                if (targetSlotIndex >= 0 && targetSlotIndex != _originSlotIndex)
                    _inventorySystem.SwapSlots(_originSlotIndex, targetSlotIndex);
            }

            CancelDrag();
        }

        // NEW — add DropToWorld above CancelDrag
        /// <summary>
        /// Called when the drag ends over empty space with no valid UI drop target.
        /// Drops the entire stack from the origin slot into the world at the player's
        /// position. ItemDropper listens to OnItemDroppedToWorld and spawns the WorldItem.
        /// </summary>
        public void DropToWorld()
        {
            if (!_isDragging) return;

            _inventorySystem.DropSlot(_originSlotIndex);
            CancelDrag();
        }

        public void CancelDrag()
        {
            if (!_isDragging) return;

            _isDragging = false;
            _originSlotIndex = -1;

            _dragGhost.Hide();
        }

        /// <summary>True while a drag operation is in progress.</summary>
        public bool IsDragging => _isDragging;

        /// <summary>The inventory slot index the current drag originated from.</summary>
        public int OriginSlotIndex => _originSlotIndex;
    }
}