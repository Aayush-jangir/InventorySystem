using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Controls the visual display of one inventory slot.
    /// Shows the item icon, rarity border color, and stack count label.
    /// Initialised by InventoryUI when the grid is built.
    /// Subscribes to InventoryEvents and redraws itself only when its own slot changes.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityBorderImage;
        [SerializeField] private TextMeshProUGUI _stackCountText;

        // ── Runtime state ──────────────────────────────────────────────────

        private int _slotIndex = -1;
        private InventorySystem _inventorySystem;
        private InventoryConfigSO _config;

        // ── Constants ──────────────────────────────────────────────────────

        /// <summary>
        /// Stack count is hidden when there is only 1 item — avoids visual clutter.
        /// </summary>
        private const int HIDE_STACK_BELOW = 2;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void OnEnable()
        {
            InventoryEvents.OnSlotChanged += HandleSlotChanged;
            InventoryEvents.OnInventoryRefreshed += HandleInventoryRefreshed;

            // Refresh immediately when the panel becomes active.
            // This catches any slot changes that fired while the panel was hidden
            // (e.g. DemoItemSpawner adding items before the player opens inventory).
            if (_slotIndex >= 0)
                Refresh();
        }
        private void OnDisable()
        {
            InventoryEvents.OnSlotChanged -= HandleSlotChanged;
            InventoryEvents.OnInventoryRefreshed -= HandleInventoryRefreshed;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Links this UI element to a specific inventory slot.
        /// Must be called by InventoryUI immediately after instantiation.
        /// </summary>
        /// <param name="slotIndex">The inventory slot index this element represents.</param>
        /// <param name="inventorySystem">Reference to the runtime inventory data.</param>
        /// <param name="config">Config asset used to look up rarity colors.</param>
        public void Initialise(int slotIndex, InventorySystem inventorySystem, InventoryConfigSO config)
        {
            _slotIndex = slotIndex;
            _inventorySystem = inventorySystem;
            _config = config;

            Refresh();
        }

        /// <summary>The inventory slot index this UI element is bound to.</summary>
        public int SlotIndex => _slotIndex;

        // ── Refresh logic ──────────────────────────────────────────────────

        /// <summary>
        /// Reads the current slot data and updates all child visuals.
        /// Called on initialisation, on slot change events, and on full inventory refresh.
        /// </summary>
        private void Refresh()
        {
            if (_inventorySystem == null) return;

            InventorySlot slot = _inventorySystem.GetSlot(_slotIndex);

            if (slot == null || slot.IsEmpty)
            {
                SetVisualEmpty();
                return;
            }

            SetVisualOccupied(slot.ItemInstance);
        }

        // NEW
        private void SetVisualOccupied(ItemInstance item)
        {
            Color rarityColor = _config != null
                ? _config.GetRarityColor(item.Data.Rarity)
                : Color.white;

            // -- Background tinted by rarity so slot is always visually occupied --
            // NEW — lerps 30% toward rarity color, keeps original alpha, always visible
            if (_backgroundImage != null)
            {
                Color baseColor = new Color(0.118f, 0.118f, 0.118f, 0.824f);
                _backgroundImage.color = Color.Lerp(baseColor, rarityColor, 0.30f);
            }

            // -- Icon --
            // NEW — tint icon image by rarity when no real icon is assigned
            if (_iconImage != null)
            {
                if (item.Data.Icon != null)
                {
                    _iconImage.enabled = true;
                    _iconImage.sprite = item.Data.Icon;
                    _iconImage.color = Color.white;
                }
                else
                {
                    // No icon assigned — show a rarity-colored square using the default white sprite
                    _iconImage.enabled = true;
                    _iconImage.sprite = null;
                    _iconImage.color = rarityColor;
                }
            }

            // -- Rarity border --
            if (_rarityBorderImage != null && _config != null)
            {
                _rarityBorderImage.enabled = true;
                _rarityBorderImage.color = rarityColor;
            }

            // -- Stack count label --
            if (_stackCountText != null)
            {
                bool showCount = item.StackCount >= HIDE_STACK_BELOW;
                _stackCountText.enabled = showCount;

                if (showCount)
                    _stackCountText.SetText("{0}", item.StackCount);
            }
        }

        // NEW
        private void SetVisualEmpty()
        {
            // Reset background to default dark color when slot is empty
            if (_backgroundImage != null)
                _backgroundImage.color = new Color(0.118f, 0.118f, 0.118f, 0.824f);

            // NEW
            if (_iconImage != null)
            {
                _iconImage.enabled = false;
                _iconImage.sprite = null;
                _iconImage.color = Color.white;
            }

            if (_rarityBorderImage != null)
                _rarityBorderImage.enabled = false;

            if (_stackCountText != null)
                _stackCountText.enabled = false;
        }

        // ── Event handlers ─────────────────────────────────────────────────

        private void HandleSlotChanged(int changedIndex)
        {
            // Only react to changes that affect this specific slot.
            // All other InventorySlotUI instances receive this event too
            // but will exit here — no wasted work.
            if (changedIndex != _slotIndex) return;
            Refresh();
        }

        private void HandleInventoryRefreshed()
        {
            // Full refresh — redraw unconditionally.
            // Fired after bulk operations like load-from-save.
            Refresh();
        }
    }
}