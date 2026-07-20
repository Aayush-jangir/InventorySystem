using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Builds the hotbar slot row at runtime and manages the selection highlight.
    /// Reads the slot count from InventoryConfigSO and instantiates one
    /// HotbarSlotUI prefab per hotbar slot into a horizontal layout container.
    /// The hotbar is always visible — it does not respond to open/close events.
    /// </summary>
    public class HotbarUI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InventoryConfigSO _config;
        [SerializeField] private HotbarSystem _hotbarSystem;

        [Header("Layout")]
        [SerializeField] private Transform _hotbarContainer;
        [SerializeField] private HotbarSlotUI _hotbarSlotPrefab;

        // ── Runtime state ──────────────────────────────────────────────────

        private HotbarSlotUI[] _slotUIs;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_CONFIG = "[HotbarUI] No InventoryConfigSO assigned.";
        private const string ERROR_NO_HOTBAR = "[HotbarUI] No HotbarSystem assigned.";
        private const string ERROR_NO_CONTAINER = "[HotbarUI] No Hotbar Container assigned.";
        private const string ERROR_NO_PREFAB = "[HotbarUI] No Hotbar Slot Prefab assigned.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (!ValidateReferences()) return;

            BuildHotbar();
        }

        private void Start()
        {
            // Fire the initial selection so slot 0 starts highlighted.
            // Done in Start so HotbarSystem.Awake has already run
            // and _linkedSlotIndices is initialised before we query it.
            if (_hotbarSystem != null)
                InventoryEvents.HotbarSlotSelected(_hotbarSystem.ActiveHotbarIndex);
        }

        // ── Hotbar construction ────────────────────────────────────────────

        private void BuildHotbar()
        {
            int slotCount = _config.HotbarSlotCount;
            _slotUIs = new HotbarSlotUI[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                HotbarSlotUI slotUI = Instantiate(_hotbarSlotPrefab, _hotbarContainer);
                slotUI.gameObject.name = $"HotbarSlot_{i:D2}";
                slotUI.Initialise(i, _hotbarSystem, _config);

                if (slotUI.TryGetComponent(out HotbarSlotDropHandler dropHandler))
                    dropHandler.Initialise(_hotbarSystem);

                if (slotUI.TryGetComponent(out HotbarSlotDragHandler dragHandler))
                    dragHandler.Initialise(_hotbarSystem);

                if (slotUI.TryGetComponent(out HotbarSlotTooltipHandler tooltipHandler))
                    tooltipHandler.Initialise(_hotbarSystem, _config);

                _slotUIs[i] = slotUI;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the HotbarSlotUI at the given hotbar index.
        /// Returns null if index is out of range or the hotbar has not been built.
        /// </summary>
        public HotbarSlotUI GetSlotUI(int hotbarIndex)
        {
            if (_slotUIs == null || hotbarIndex < 0 || hotbarIndex >= _slotUIs.Length)
                return null;

            return _slotUIs[hotbarIndex];
        }

        /// <summary>Total number of hotbar slot UI elements.</summary>
        public int SlotUICount => _slotUIs != null ? _slotUIs.Length : 0;

        // ── Validation ─────────────────────────────────────────────────────

        private bool ValidateReferences()
        {
            bool valid = true;

            if (_config == null)
            {
                Debug.LogError(ERROR_NO_CONFIG);
                valid = false;
            }

            if (_hotbarSystem == null)
            {
                Debug.LogError(ERROR_NO_HOTBAR);
                valid = false;
            }

            if (_hotbarContainer == null)
            {
                Debug.LogError(ERROR_NO_CONTAINER);
                valid = false;
            }

            if (_hotbarSlotPrefab == null)
            {
                Debug.LogError(ERROR_NO_PREFAB);
                valid = false;
            }

            return valid;
        }
    }
}