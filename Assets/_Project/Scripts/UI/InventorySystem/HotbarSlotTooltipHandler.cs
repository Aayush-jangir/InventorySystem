using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Shows and hides the item tooltip when the pointer enters and exits
    /// a hotbar slot. Reads slot data through HotbarSystem — identical behaviour
    /// to InventorySlotTooltipHandler but for hotbar slots.
    /// Attach to PFB_HotbarSlot. Initialise() called by HotbarUI.
    /// </summary>
    public class HotbarSlotTooltipHandler : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerMoveHandler
    {
        private HotbarSlotUI _hotbarSlotUI;
        private HotbarSystem _hotbarSystem;
        private InventoryConfigSO _config;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            TryGetComponent(out _hotbarSlotUI);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Provides references needed to read slot data and look up rarity colors.
        /// Called by HotbarUI immediately after instantiation.
        /// </summary>
        public void Initialise(HotbarSystem hotbarSystem, InventoryConfigSO config)
        {
            _hotbarSystem = hotbarSystem;
            _config = config;
        }

        // ── Pointer interface implementation ───────────────────────────────

        /// <summary>Shows the tooltip if this hotbar slot contains an item.</summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_hotbarSlotUI == null || _hotbarSystem == null) return;

            InventorySlot slot =
                _hotbarSystem.GetLinkedSlot(_hotbarSlotUI.HotbarIndex);

            if (slot == null || slot.IsEmpty) return;

            TooltipUI.Instance?.Show(slot.ItemInstance, _config, eventData.position);
        }

        /// <summary>Hides the tooltip when the pointer leaves this slot.</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipUI.Instance?.Hide();
        }

        /// <summary>Keeps the tooltip positioned near the cursor while hovering.</summary>
        public void OnPointerMove(PointerEventData eventData)
        {
            TooltipUI.Instance?.UpdatePosition(eventData.position);
        }
    }
}