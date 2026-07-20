using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Master registry of every ItemDataSO asset in the project.
    /// Assign all item assets here once — other systems look items up
    /// by ID through this database instead of using Resources.Load().
    /// Create via: Right-click → Create → PlayMatrix → Inventory System → Item Database
    /// </summary>
    [CreateAssetMenu(
        fileName = "ItemDatabase_Default",
        menuName = "PlayMatrix/Inventory System/Item Database")]
    public class ItemDatabaseSO : ScriptableObject
    {
        [SerializeField] private ItemDataSO[] _items = new ItemDataSO[0];

        /// <summary>Read-only access to the full item list.</summary>
        public ItemDataSO[] Items => _items;

        /// <summary>
        /// Finds and returns an ItemDataSO whose name matches the given item name.
        /// Returns null if no match is found.
        /// Uses a plain loop — no LINQ, no allocations, safe for mobile.
        /// </summary>
        public ItemDataSO GetItemByName(string itemName)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null && _items[i].ItemName == itemName)
                    return _items[i];
            }

            Debug.LogWarning($"[ItemDatabaseSO] Item not found: '{itemName}'");
            return null;
        }

        /// <summary>
        /// Finds and returns an ItemDataSO by its Unity asset name (the .asset filename).
        /// Useful for save/load — store the asset name, look it up on load.
        /// Returns null if no match is found.
        /// </summary>
        public ItemDataSO GetItemByAssetName(string assetName)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null && _items[i].name == assetName)
                    return _items[i];
            }

            Debug.LogWarning($"[ItemDatabaseSO] Asset not found: '{assetName}'");
            return null;
        }
    }
}