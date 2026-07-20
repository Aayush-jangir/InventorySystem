using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// DEMO ONLY — Populates the inventory with one of each sample item on Start.
    /// This script exists solely to make the demo scene testable out of the box.
    /// Remove this component before shipping the system to a client or production build.
    /// Attach to the same GameObject as InventorySystem.
    /// </summary>
    public class DemoItemSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem _inventorySystem;
        [SerializeField] private ItemDatabaseSO _itemDatabase;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_INVENTORY =
            "[DemoItemSpawner] No InventorySystem assigned.";
        private const string ERROR_NO_DATABASE =
            "[DemoItemSpawner] No ItemDatabaseSO assigned.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Start()
        {
            if (_inventorySystem == null)
            {
                Debug.LogError(ERROR_NO_INVENTORY);
                return;
            }

            if (_itemDatabase == null)
            {
                Debug.LogError(ERROR_NO_DATABASE);
                return;
            }

            PopulateInventory();
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void PopulateInventory()
        {
            // Non-stackable — tests single-item slots, drag and drop, tooltip
            AddItem("Iron Sword", 1);
            AddItem("Steel Shield", 1);

            // Stackable consumables — tests stack display, split stack popup
            AddItem("Health Potion", 8);
            AddItem("Elixir of Power", 5);

            // Quest item — tests Epic rarity border color and tooltip
            AddItem("Ancient Relic", 1);

            // Legendary stackable — tests Legendary rarity color and split
            AddItem("Dragon Scale", 3);
        }

        /// <summary>
        /// Looks up an item by name in the database and adds the given amount
        /// to the inventory. Logs a warning if the inventory is full.
        /// </summary>
        private void AddItem(string itemName, int amount)
        {
            ItemDataSO data = _itemDatabase.GetItemByName(itemName);
            if (data == null) return;   // GetItemByName already logs a warning

            bool success = _inventorySystem.AddItem(data, amount);

            if (!success)
            {
                Debug.LogWarning(
                    $"[DemoItemSpawner] Inventory full — could not add '{itemName}'.");
            }
        }
    }
}