using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Singleton popup that lets the player choose how many items to
    /// split from a stack. Opened by InventorySlotContextHandler on
    /// right-click. Calls InventorySystem.SplitStack on confirm.
    ///
    /// Scene setup: attach to SplitStackPopup panel inside InventoryCanvas.
    /// Wire all references via the Inspector — no Find() calls used.
    /// </summary>
    public class SplitStackPopupUI : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────

        /// <summary>Global access point for the split stack popup.</summary>
        public static SplitStackPopupUI Instance { get; private set; }

        // ── Inspector references ───────────────────────────────────────────

        [Header("System Reference")]
        [SerializeField] private InventorySystem _inventorySystem;

        [Header("Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Slider _slider;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        // ── Runtime state ──────────────────────────────────────────────────

        private int _targetSlotIndex = -1;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_INVENTORY =
            "[SplitStackPopupUI] No InventorySystem assigned.";
        private const string ERROR_NO_PANEL =
            "[SplitStackPopupUI] No Panel Root assigned.";
        private const string ERROR_NO_SLIDER =
            "[SplitStackPopupUI] No Slider assigned.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[SplitStackPopupUI] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            ValidateReferences();
            WireButtons();

            // Popup is hidden until a right-click opens it
            Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            // Always remove listeners when destroyed to avoid memory leaks
            _confirmButton?.onClick.RemoveListener(OnConfirmClicked);
            _cancelButton?.onClick.RemoveListener(Hide);
            _slider?.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnEnable()
        {
            // Auto-hide if the player closes the inventory while popup is open
            InventoryEvents.OnInventoryClosed += Hide;
        }

        private void OnDisable()
        {
            InventoryEvents.OnInventoryClosed -= Hide;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Opens the popup for the given inventory slot.
        /// Configures the slider range based on the item's current stack count.
        /// Does nothing if the stack has only 1 item (nothing to split).
        /// </summary>
        /// <param name="slotIndex">Inventory slot index the player right-clicked.</param>
        /// <param name="item">The item instance in that slot.</param>
        public void Show(int slotIndex, ItemInstance item)
        {
            // Nothing to split if stack is 1 or fewer
            if (item == null || item.StackCount <= 1) return;

            // Do not open popup while a drag is in progress
            if (InventoryDragController.Instance != null &&
                InventoryDragController.Instance.IsDragging) return;

            _targetSlotIndex = slotIndex;

            // -- Item name --
            if (_itemNameText != null)
                _itemNameText.SetText(item.Data.ItemName);

            // -- Slider range --
            // Min = 1 (must split at least 1 away)
            // Max = stackCount - 1 (must leave at least 1 in the source slot)
            // Default = half the stack (integer floor)
            if (_slider != null)
            {
                _slider.wholeNumbers = true;
                _slider.minValue = 1f;
                _slider.maxValue = item.StackCount - 1f;

                int defaultSplit = Mathf.Clamp(item.StackCount / 2, 1, item.StackCount - 1);
                _slider.value = defaultSplit;
                // onValueChanged fires here, which updates _amountText via OnSliderValueChanged
            }

            _panelRoot?.SetActive(true);
        }

        /// <summary>
        /// Closes the popup without making any changes to the inventory.
        /// Also called automatically when the inventory panel is closed.
        /// </summary>
        public void Hide()
        {
            _targetSlotIndex = -1;
            _panelRoot?.SetActive(false);
        }

        /// <summary>Whether the popup panel is currently visible.</summary>
        public bool IsOpen => _panelRoot != null && _panelRoot.activeSelf;

        // ── Private helpers ────────────────────────────────────────────────

        private void WireButtons()
        {
            _confirmButton?.onClick.AddListener(OnConfirmClicked);
            _cancelButton?.onClick.AddListener(Hide);
            _slider?.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            // Cast to int — slider has wholeNumbers = true so value is always integral
            if (_amountText != null)
                _amountText.SetText("{0}", (int)value);
        }

        private void OnConfirmClicked()
        {
            if (_targetSlotIndex < 0 || _inventorySystem == null) return;

            int splitAmount = (int)_slider.value;
            _inventorySystem.SplitStack(_targetSlotIndex, splitAmount);

            // InventorySystem.SplitStack fires OnSlotChanged internally —
            // InventorySlotUI redraws automatically. No manual refresh needed.
            Hide();
        }

        // ── Validation ─────────────────────────────────────────────────────

        private void ValidateReferences()
        {
            if (_inventorySystem == null) Debug.LogError(ERROR_NO_INVENTORY);
            if (_panelRoot == null) Debug.LogError(ERROR_NO_PANEL);
            if (_slider == null) Debug.LogError(ERROR_NO_SLIDER);
        }
    }
}