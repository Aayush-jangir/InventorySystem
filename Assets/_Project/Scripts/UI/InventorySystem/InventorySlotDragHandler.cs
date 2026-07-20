using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Handles drag and drop input for a single inventory slot.
    /// Attach to PFB_InventorySlot — it finds its InventorySlotUI sibling automatically.
    /// Reports all drag events to InventoryDragController.
    /// 
    /// Unity UI drag event order when dropping on a valid target:
    ///   1. OnDrop fires on the target slot   → swap is resolved
    ///   2. OnEndDrag fires on the source slot → CancelDrag is a safe no-op
    /// 
    /// When dropped on empty space:
    ///   OnEndDrag fires on the source → CancelDrag hides the ghost
    /// </summary>
    public class InventorySlotDragHandler : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        // Cached reference — read in Awake, slot index read lazily at drag time
        // so it is always valid (Initialise on InventorySlotUI is called after Awake)
        private InventorySlotUI _slotUI;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            TryGetComponent(out _slotUI);
        }

        // ── Drag interface implementation ──────────────────────────────────

        /// <summary>
        /// Called once when the pointer begins dragging this slot.
        /// Tells InventoryDragController to start a drag from this slot's index.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_slotUI == null) return;

            InventoryDragController.Instance?.BeginDrag(
                _slotUI.SlotIndex,
                eventData.position);
        }

        /// <summary>
        /// Called every frame while the pointer is held and moving.
        /// Passes the current pointer position to the ghost via DragController.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            InventoryDragController.Instance?.UpdateDrag(eventData.position);
        }

        /// <summary>
        /// Called when the pointer is released — on the SOURCE slot.
        /// If the drop landed on a valid target, IsDragging is already false
        /// and CancelDrag returns immediately. If dropped on empty space,
        /// this call hides the ghost and clears drag state.
        /// </summary>
        // NEW
        public void OnEndDrag(PointerEventData eventData)
        {
            // If IsDragging is still true here, no IDropHandler was hit —
            // the player released over empty world space. Drop item to world.
            if (InventoryDragController.Instance == null) return;

            if (InventoryDragController.Instance.IsDragging)
                InventoryDragController.Instance.DropToWorld();
            else
                InventoryDragController.Instance.CancelDrag();
        }

        /// <summary>
        /// Called when a drag is released over THIS slot — making this the TARGET.
        /// Tells DragController to resolve the swap between origin and this slot.
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            if (_slotUI == null) return;

            InventoryDragController.Instance?.EndDragOnInventorySlot(
                _slotUI.SlotIndex);
        }
    }
}