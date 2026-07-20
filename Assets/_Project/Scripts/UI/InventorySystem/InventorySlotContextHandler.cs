using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Detects right-click on an inventory slot and opens the split stack popup.
    /// Attach to PFB_InventorySlot alongside InventorySlotUI and InventorySlotDragHandler.
    /// Initialise() must be called by InventoryUI after grid instantiation,
    /// exactly as InventorySlotTooltipHandler is initialised.
    /// </summary>
    public class InventorySlotContextHandler : MonoBehaviour, IPointerClickHandler
    {
        // ── Runtime references ─────────────────────────────────────────────

        private InventorySlotUI _slotUI;
        private InventorySystem _inventorySystem;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            // Find the sibling InventorySlotUI on the same GameObject —
            // same pattern used by InventorySlotDragHandler and InventorySlotTooltipHandler.
            TryGetComponent(out _slotUI);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Provides the InventorySystem reference needed to read slot data on right-click.
        /// Called by InventoryUI immediately after grid instantiation.
        /// </summary>
        public void Initialise(InventorySystem inventorySystem)
        {
            _inventorySystem = inventorySystem;
        }

        // ── Pointer interface implementation ───────────────────────────────

        /// <summary>
        /// Called by Unity UI when the pointer is pressed and released on this slot.
        /// Only acts on right-click. Left-click is handled by InventorySlotDragHandler.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // Only respond to right-click
            if (eventData.button != PointerEventData.InputButton.Right) return;

            if (_slotUI == null || _inventorySystem == null) return;

            // Do not open popup while a drag is in progress
            if (InventoryDragController.Instance != null &&
                InventoryDragController.Instance.IsDragging) return;

            InventorySlot slot = _inventorySystem.GetSlot(_slotUI.SlotIndex);

            // Only open the popup if this slot has a stackable item with more than 1
            if (slot == null || slot.IsEmpty) return;
            if (slot.ItemInstance.StackCount <= 1) return;

            // Hide the tooltip — context menu and tooltip are mutually exclusive
            TooltipUI.Instance?.Hide();

            // Open the split stack popup for this slot
            SplitStackPopupUI.Instance?.Show(_slotUI.SlotIndex, slot.ItemInstance);
        }
    }
}