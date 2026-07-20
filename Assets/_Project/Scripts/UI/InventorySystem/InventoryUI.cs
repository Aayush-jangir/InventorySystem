using UnityEngine;
using UnityEngine.UI;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Builds the inventory slot grid at runtime and controls panel visibility.
    /// Reads row/column counts from InventoryConfigSO and instantiates
    /// one InventorySlotUI prefab per slot into a GridLayoutGroup container.
    /// Responds to InventoryEvents.OnInventoryOpened / OnInventoryClosed
    /// to show and hide the panel — no direct calls from other systems needed.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InventoryConfigSO _config;
        [SerializeField] private InventorySystem _inventorySystem;

        [Header("Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Grid")]
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private InventorySlotUI _slotPrefab;

        // ── Runtime state ──────────────────────────────────────────────────

        private InventorySlotUI[] _slotUIs;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_CONFIG = "[InventoryUI] No InventoryConfigSO assigned.";
        private const string ERROR_NO_INVENTORY = "[InventoryUI] No InventorySystem assigned.";
        private const string ERROR_NO_PANEL = "[InventoryUI] No Panel Root assigned.";
        private const string ERROR_NO_CONTAINER = "[InventoryUI] No Grid Container assigned.";
        private const string ERROR_NO_PREFAB = "[InventoryUI] No Slot Prefab assigned.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (!ValidateReferences()) return;

            BuildGrid();

            // Panel starts hidden — player opens it with I key
            SetPanelVisible(false);
        }

        private void OnEnable()
        {
            InventoryEvents.OnInventoryOpened += HandleInventoryOpened;
            InventoryEvents.OnInventoryClosed += HandleInventoryClosed;
        }

        private void OnDisable()
        {
            InventoryEvents.OnInventoryOpened -= HandleInventoryOpened;
            InventoryEvents.OnInventoryClosed -= HandleInventoryClosed;
        }

        // ── Grid construction ──────────────────────────────────────────────

        private void BuildGrid()
        {
            // NEW — grid only builds the grid portion, not hotbar slots
            int gridSlots = _config.GridSlotCount;
            _slotUIs = new InventorySlotUI[gridSlots];

            for (int i = 0; i < gridSlots; i++)
            {
                InventorySlotUI slotUI = Instantiate(_slotPrefab, _gridContainer);
                slotUI.gameObject.name = $"Slot_{i:D2}";
                slotUI.Initialise(i, _inventorySystem, _config);

                // NEW — wire the tooltip handler with system references
                if (slotUI.TryGetComponent(out InventorySlotTooltipHandler tooltipHandler))
                    tooltipHandler.Initialise(_inventorySystem, _config);

                // Wire the context handler (right-click → split stack popup)
                if (slotUI.TryGetComponent(out InventorySlotContextHandler contextHandler))
                    contextHandler.Initialise(_inventorySystem);

                _slotUIs[i] = slotUI;
            }
        }

        // ── Panel visibility ───────────────────────────────────────────────

        /// <summary>
        /// Shows or hides the inventory panel root.
        /// </summary>
        private void SetPanelVisible(bool visible)
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(visible);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the InventorySlotUI at the given slot index.
        /// Returns null if the index is out of range or the grid has not been built.
        /// Used by drag-and-drop logic in Phase 5.
        /// </summary>
        public InventorySlotUI GetSlotUI(int slotIndex)
        {
            if (_slotUIs == null || slotIndex < 0 || slotIndex >= _slotUIs.Length)
                return null;

            return _slotUIs[slotIndex];
        }

        /// <summary>Total number of slot UI elements in the grid.</summary>
        public int SlotUICount => _slotUIs != null ? _slotUIs.Length : 0;

        // ── Event handlers ─────────────────────────────────────────────────

        private void HandleInventoryOpened() => SetPanelVisible(true);
        private void HandleInventoryClosed() => SetPanelVisible(false);

        // ── Validation ─────────────────────────────────────────────────────

        private bool ValidateReferences()
        {
            bool valid = true;

            if (_config == null)
            {
                Debug.LogError(ERROR_NO_CONFIG);
                valid = false;
            }

            if (_inventorySystem == null)
            {
                Debug.LogError(ERROR_NO_INVENTORY);
                valid = false;
            }

            if (_panelRoot == null)
            {
                Debug.LogError(ERROR_NO_PANEL);
                valid = false;
            }

            if (_gridContainer == null)
            {
                Debug.LogError(ERROR_NO_CONTAINER);
                valid = false;
            }

            if (_slotPrefab == null)
            {
                Debug.LogError(ERROR_NO_PREFAB);
                valid = false;
            }

            return valid;
        }
    }
}