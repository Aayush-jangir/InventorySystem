using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Shows and hides the item tooltip when the pointer enters and exits
    /// an inventory slot. Attach to PFB_InventorySlot alongside InventorySlotUI
    /// and InventorySlotDragHandler.
    /// Initialise() must be called by InventoryUI after grid instantiation.
    /// </summary>
    public class InventorySlotTooltipHandler : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerMoveHandler
    {
        // ── Runtime references ─────────────────────────────────────────────

        private InventorySlotUI _slotUI;
        private InventorySystem _inventorySystem;
        private InventoryConfigSO _config;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            TryGetComponent(out _slotUI);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Provides InventorySystem and config references needed to read slot data.
        /// Called by InventoryUI immediately after grid instantiation.
        /// </summary>
        public void Initialise(InventorySystem inventorySystem, InventoryConfigSO config)
        {
            _inventorySystem = inventorySystem;
            _config = config;
        }

        // ── Pointer interface implementation ───────────────────────────────

        /// <summary>
        /// Called when the pointer enters this slot.
        /// Reads the current item and shows the tooltip if the slot is occupied.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_slotUI == null || _inventorySystem == null) return;

            InventorySlot slot = _inventorySystem.GetSlot(_slotUI.SlotIndex);
            if (slot == null || slot.IsEmpty) return;

            TooltipUI.Instance?.Show(slot.ItemInstance, _config, eventData.position);
        }

        /// <summary>
        /// Called when the pointer exits this slot.
        /// Hides the tooltip unconditionally.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipUI.Instance?.Hide();
        }

        /// <summary>
        /// Called every frame the pointer moves within this slot.
        /// Keeps the tooltip positioned near the cursor.
        /// </summary>
        public void OnPointerMove(PointerEventData eventData)
        {
            TooltipUI.Instance?.UpdatePosition(eventData.position);
        }
    }
}