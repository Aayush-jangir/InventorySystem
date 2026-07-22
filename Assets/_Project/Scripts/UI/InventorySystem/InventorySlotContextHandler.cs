using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Detects right-click (PC) or long press (mobile) on an inventory slot
    /// and opens the split stack popup.
    /// 
    /// PC:     Right-click any stackable slot → split popup opens.
    /// Mobile: Hold finger on any stackable slot for LongPressDuration
    ///         seconds → split popup opens.
    /// 
    /// Attach to PFB_InventorySlot alongside InventorySlotUI.
    /// Initialise() must be called by InventoryUI after grid instantiation.
    /// </summary>
    public class InventorySlotContextHandler : MonoBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        // ── Runtime references ─────────────────────────────────────────────

        private InventorySlotUI _slotUI;
        private InventorySystem _inventorySystem;

        // ── Long press state ───────────────────────────────────────────────

        private bool _isPointerDown;
        private float _pointerDownTime;

        // ── Constants ──────────────────────────────────────────────────────

        /// <summary>
        /// Seconds the player must hold before the split popup opens on mobile.
        /// 0.5 s feels responsive without triggering on accidental touches.
        /// </summary>
        private const float LONG_PRESS_DURATION = 0.5f;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            TryGetComponent(out _slotUI);
        }

        private void Update()
        {
            if (!_isPointerDown) return;

            if (Time.time - _pointerDownTime >= LONG_PRESS_DURATION)
            {
                _isPointerDown = false;
                TryOpenSplitPopup();
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Provides the InventorySystem reference needed to read slot data.
        /// Called by InventoryUI immediately after grid instantiation.
        /// </summary>
        public void Initialise(InventorySystem inventorySystem)
        {
            _inventorySystem = inventorySystem;
        }

        // ── Pointer interface implementation ───────────────────────────────

        /// <summary>
        /// PC only — right-click opens the split popup immediately.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            TryOpenSplitPopup();
        }

        /// <summary>
        /// Starts the long press timer when the pointer is pressed down.
        /// Works for both mouse and touch.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            // Only track left button / primary touch — right-click
            // is handled by OnPointerClick above
            if (eventData.button != PointerEventData.InputButton.Left) return;

            _isPointerDown = true;
            _pointerDownTime = Time.time;
        }

        /// <summary>
        /// Cancels the long press timer if the finger/mouse is released early.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            _isPointerDown = false;
        }

        // ── Private helpers ────────────────────────────────────────────────

        /// <summary>
        /// Validates the slot and opens the split popup if conditions are met.
        /// Shared by both right-click and long press paths.
        /// </summary>
        private void TryOpenSplitPopup()
        {
            if (_slotUI == null || _inventorySystem == null) return;

            if (InventoryDragController.Instance != null &&
                InventoryDragController.Instance.IsDragging) return;

            InventorySlot slot = _inventorySystem.GetSlot(_slotUI.SlotIndex);

            if (slot == null || slot.IsEmpty) return;
            if (slot.ItemInstance.StackCount <= 1) return;

            TooltipUI.Instance?.Hide();
            SplitStackPopupUI.Instance?.Show(_slotUI.SlotIndex, slot.ItemInstance);
        }
    }
}