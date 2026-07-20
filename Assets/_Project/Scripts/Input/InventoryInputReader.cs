using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Reads input from InventoryInputActions and translates it into
    /// inventory system calls. Attach to the same GameObject as
    /// InventorySystem and HotbarSystem.
    /// 
    /// Handles:
    ///   - Toggle inventory panel (I key)
    ///   - Interact / pickup (E key — manual pickup mode only)
    ///   - Hotbar selection (1–5 keys)
    /// </summary>
    public class InventoryInputReader : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem _inventorySystem;
        [SerializeField] private HotbarSystem _hotbarSystem;
        [SerializeField] private InventoryConfigSO _config;

        private InventoryInputActions _inputActions;

        // ── Constants ─────────────────────────────────────────────────────

        private const string ERROR_NO_INVENTORY = "[InventoryInputReader] No InventorySystem assigned.";
        private const string ERROR_NO_HOTBAR = "[InventoryInputReader] No HotbarSystem assigned.";
        private const string ERROR_NO_CONFIG = "[InventoryInputReader] No InventoryConfigSO assigned.";

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (_inventorySystem == null) Debug.LogError(ERROR_NO_INVENTORY);
            if (_hotbarSystem == null) Debug.LogError(ERROR_NO_HOTBAR);
            if (_config == null) Debug.LogError(ERROR_NO_CONFIG);

            _inputActions = new InventoryInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Inventory.Enable();

            _inputActions.Inventory.ToggleInventory.performed += OnToggleInventory;
            _inputActions.Inventory.Interact.performed += OnInteract;
            _inputActions.Inventory.Hotkey1.performed += OnHotkey1;
            _inputActions.Inventory.Hotkey2.performed += OnHotkey2;
            _inputActions.Inventory.Hotkey3.performed += OnHotkey3;
            _inputActions.Inventory.Hotkey4.performed += OnHotkey4;
            _inputActions.Inventory.Hotkey5.performed += OnHotkey5;
        }

        private void OnDisable()
        {
            _inputActions.Inventory.ToggleInventory.performed -= OnToggleInventory;
            _inputActions.Inventory.Interact.performed -= OnInteract;
            _inputActions.Inventory.Hotkey1.performed -= OnHotkey1;
            _inputActions.Inventory.Hotkey2.performed -= OnHotkey2;
            _inputActions.Inventory.Hotkey3.performed -= OnHotkey3;
            _inputActions.Inventory.Hotkey4.performed -= OnHotkey4;
            _inputActions.Inventory.Hotkey5.performed -= OnHotkey5;

            _inputActions.Inventory.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        // ── Input Callbacks ───────────────────────────────────────────────

        private void OnToggleInventory(InputAction.CallbackContext context)
        {
            if (_inventorySystem == null) return;
            _inventorySystem.ToggleInventory();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            // In auto-pickup mode the trigger handles pickup — ignore E key
            if (_config != null && _config.AutoPickup) return;

            // Manual pickup — check if there is a WorldItem in range
            if (WorldItem.CurrentInteractable != null)
                WorldItem.CurrentInteractable.Pickup();
        }

        private void OnHotkey1(InputAction.CallbackContext context) => SelectHotbar(0);
        private void OnHotkey2(InputAction.CallbackContext context) => SelectHotbar(1);
        private void OnHotkey3(InputAction.CallbackContext context) => SelectHotbar(2);
        private void OnHotkey4(InputAction.CallbackContext context) => SelectHotbar(3);
        private void OnHotkey5(InputAction.CallbackContext context) => SelectHotbar(4);

        private void SelectHotbar(int index)
        {
            if (_hotbarSystem == null) return;
            _hotbarSystem.SelectHotbarSlot(index);
        }
    }
}