using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Enables drag operations initiated FROM a hotbar slot.
    /// Resolves the hotbar slot's actual inventory index via HotbarSystem
    /// and passes it to InventoryDragController — identical flow to
    /// InventorySlotDragHandler so all existing drop targets work unchanged.
    /// Attach to PFB_HotbarSlot. Initialise() called by HotbarUI.
    /// </summary>
    public class HotbarSlotDragHandler : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
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
        /// Provides the HotbarSystem reference needed to resolve the real
        /// inventory slot index on drag begin.
        /// Called by HotbarUI immediately after instantiation.
        /// </summary>
        public void Initialise(HotbarSystem hotbarSystem)
        {
            _hotbarSystem = hotbarSystem;
        }

        // ── Drag interface implementation ──────────────────────────────────

        /// <summary>
        /// Translates hotbar index → real inventory slot index, then tells
        /// InventoryDragController to begin the drag from that slot.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_hotbarSlotUI == null || _hotbarSystem == null) return;

            int inventoryIndex =
                _hotbarSystem.GetLinkedInventoryIndex(_hotbarSlotUI.HotbarIndex);

            if (inventoryIndex < 0) return;

            InventoryDragController.Instance?.BeginDrag(inventoryIndex, eventData.position);
        }

        /// <summary>Passes pointer position to the drag ghost every frame.</summary>
        public void OnDrag(PointerEventData eventData)
        {
            InventoryDragController.Instance?.UpdateDrag(eventData.position);
        }

        /// <summary>
        /// Cancels the drag if it was not resolved by a drop target.
        /// If the pointer landed on a valid IDropHandler, CancelDrag is a safe no-op.
        /// </summary>
        // NEW
        public void OnEndDrag(PointerEventData eventData)
        {
            if (InventoryDragController.Instance == null) return;

            if (InventoryDragController.Instance.IsDragging)
                InventoryDragController.Instance.DropToWorld();
            else
                InventoryDragController.Instance.CancelDrag();
        }
    }
}