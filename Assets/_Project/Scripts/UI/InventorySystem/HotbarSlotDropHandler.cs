using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Accepts inventory item drops onto a hotbar slot.
    /// When an inventory slot is dropped here, re-links this hotbar slot
    /// to point at the dragged inventory slot index.
    /// Attach to PFB_HotbarSlot alongside HotbarSlotUI.
    /// Initialise() must be called by HotbarUI after instantiation.
    /// </summary>
    public class HotbarSlotDropHandler : MonoBehaviour, IDropHandler
    {
        private HotbarSlotUI _hotbarSlotUI;
        private HotbarSystem _hotbarSystem;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            TryGetComponent(out _hotbarSlotUI);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Provides the HotbarSystem reference needed to re-link slots on drop.
        /// Called by HotbarUI immediately after instantiation.
        /// </summary>
        public void Initialise(HotbarSystem hotbarSystem)
        {
            _hotbarSystem = hotbarSystem;
        }

        // ── Drop interface implementation ──────────────────────────────────

        /// <summary>
        /// Called by Unity when a drag operation is released over this hotbar slot.
        /// Tells InventoryDragController to re-link this hotbar slot to the
        /// origin inventory slot index of the current drag.
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            if (_hotbarSlotUI == null || _hotbarSystem == null) return;

            InventoryDragController.Instance?.EndDragOnHotbarSlot(
                _hotbarSlotUI.HotbarIndex,
                _hotbarSystem);
        }
    }
}