using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Controls the visual display of one hotbar slot.
    /// Shows the linked inventory slot's icon, stack count, a keybind label,
    /// and a selection highlight when this slot is the active hotbar slot.
    /// Initialised by HotbarUI when the hotbar row is built.
    /// </summary>
    public class HotbarSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityBorderImage;
        [SerializeField] private Image _selectionHighlight;
        [SerializeField] private TextMeshProUGUI _stackCountText;
        [SerializeField] private TextMeshProUGUI _keybindLabel;

        // ── Runtime state ──────────────────────────────────────────────────

        private int _hotbarIndex = -1;
        private HotbarSystem _hotbarSystem;
        private InventoryConfigSO _config;

        // ── Constants ──────────────────────────────────────────────────────

        /// <summary>Stack count label is hidden when the stack is exactly 1.</summary>
        private const int HIDE_STACK_BELOW = 2;

        /// <summary>Keybind display offset — hotbar index 0 shows "1", index 4 shows "5".</summary>
        private const int KEYBIND_DISPLAY_OFFSET = 1;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void OnEnable()
        {
            InventoryEvents.OnHotbarLinkChanged += HandleHotbarLinkChanged;
            InventoryEvents.OnHotbarSlotSelected += HandleHotbarSlotSelected;
            InventoryEvents.OnInventoryRefreshed += HandleInventoryRefreshed;
        }

        private void OnDisable()
        {
            InventoryEvents.OnHotbarLinkChanged -= HandleHotbarLinkChanged;
            InventoryEvents.OnHotbarSlotSelected -= HandleHotbarSlotSelected;
            InventoryEvents.OnInventoryRefreshed -= HandleInventoryRefreshed;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Links this UI element to a specific hotbar slot index.
        /// Must be called by HotbarUI immediately after instantiation.
        /// </summary>
        /// <param name="hotbarIndex">The hotbar slot index this element represents (0-based).</param>
        /// <param name="hotbarSystem">Reference to the HotbarSystem for reading linked slot data.</param>
        /// <param name="config">Config asset used for rarity colors.</param>
        public void Initialise(int hotbarIndex, HotbarSystem hotbarSystem, InventoryConfigSO config)
        {
            _hotbarIndex = hotbarIndex;
            _hotbarSystem = hotbarSystem;
            _config = config;

            SetKeybindLabel();
            SetHighlight(false);
            Refresh();
        }

        /// <summary>The hotbar slot index this UI element is bound to.</summary>
        public int HotbarIndex => _hotbarIndex;

        // ── Refresh logic ──────────────────────────────────────────────────

        private void Refresh()
        {
            if (_hotbarSystem == null) return;

            InventorySlot slot = _hotbarSystem.GetLinkedSlot(_hotbarIndex);

            if (slot == null || slot.IsEmpty)
            {
                SetVisualEmpty();
                return;
            }

            SetVisualOccupied(slot.ItemInstance);
        }

        private void SetVisualOccupied(ItemInstance item)
        {
            // -- Icon --
            if (_iconImage != null)
            {
                _iconImage.enabled = item.Data.Icon != null;
                _iconImage.sprite = item.Data.Icon;
            }

            // -- Rarity border --
            if (_rarityBorderImage != null && _config != null)
            {
                _rarityBorderImage.enabled = true;
                _rarityBorderImage.color = _config.GetRarityColor(item.Data.Rarity);
            }

            // -- Stack count --
            if (_stackCountText != null)
            {
                bool showCount = item.StackCount >= HIDE_STACK_BELOW;
                _stackCountText.enabled = showCount;
                if (showCount)
                    _stackCountText.SetText("{0}", item.StackCount);
            }
        }

        private void SetVisualEmpty()
        {
            if (_iconImage != null)
            {
                _iconImage.enabled = false;
                _iconImage.sprite = null;
            }

            if (_rarityBorderImage != null)
                _rarityBorderImage.enabled = false;

            if (_stackCountText != null)
                _stackCountText.enabled = false;
        }

        private void SetKeybindLabel()
        {
            if (_keybindLabel == null) return;

            // Display as 1-based: slot 0 → "1", slot 4 → "5"
            _keybindLabel.SetText("{0}", _hotbarIndex + KEYBIND_DISPLAY_OFFSET);
        }

        private void SetHighlight(bool isActive)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.enabled = isActive;
        }

        // ── Event handlers ─────────────────────────────────────────────────

        private void HandleHotbarLinkChanged(int hotbarIndex, int inventorySlotIndex)
        {
            // Only react to changes that affect this specific hotbar slot.
            if (hotbarIndex != _hotbarIndex) return;
            Refresh();
        }

        private void HandleHotbarSlotSelected(int selectedHotbarIndex)
        {
            // Turn highlight on if this is the newly selected slot, off otherwise.
            SetHighlight(selectedHotbarIndex == _hotbarIndex);
        }

        private void HandleInventoryRefreshed()
        {
            Refresh();
        }

        // NEW — add this method, it did not exist before
        /// <summary>
        /// Selects this hotbar slot when the player left-clicks it directly.
        /// Right-click is reserved for context actions. Drag is unaffected —
        /// Unity separates click and drag events so there is no conflict.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (_hotbarSystem == null) return;

            _hotbarSystem.SelectHotbarSlot(_hotbarIndex);
        }
    }
}