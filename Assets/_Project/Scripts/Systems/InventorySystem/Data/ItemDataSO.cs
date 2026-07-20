using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Defines the static blueprint for a single item type.
    /// Create instances via: Right-click → Create → PlayMatrix → Inventory System → Item Data
    /// One asset per unique item (e.g. Iron Sword, Health Potion, Steel Helmet).
    /// </summary>
    [CreateAssetMenu(
        fileName = "ItemData_New",
        menuName = "PlayMatrix/Inventory System/Item Data")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _itemName = "New Item";
        [SerializeField][TextArea(2, 4)] private string _description = "";
        [SerializeField] private Sprite _icon = null;

        [Header("Classification")]
        [SerializeField] private ItemType _itemType = ItemType.Misc;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;

        [Header("Stacking")]
        [SerializeField][Min(1)] private int _maxStackSize = 1;

        [Header("World Drop")]
        [SerializeField] private GameObject _worldPrefab = null;

        // ── Public read-only properties ──────────────────────────────────

        /// <summary>Display name shown in the tooltip and hotbar.</summary>
        public string ItemName => _itemName;

        /// <summary>Flavour text shown in the tooltip.</summary>
        public string Description => _description;

        /// <summary>Icon displayed in inventory slots and the hotbar.</summary>
        public Sprite Icon => _icon;

        /// <summary>Category of this item (Weapon, Armor, Consumable, etc.).</summary>
        public ItemType ItemType => _itemType;

        /// <summary>Rarity tier — drives border color and tooltip color.</summary>
        public ItemRarity Rarity => _rarity;

        /// <summary>
        /// Maximum number of this item that can share one inventory slot.
        /// 1 = non-stackable (e.g. weapons). Higher values = stackable (e.g. potions).
        /// </summary>
        public int MaxStackSize => _maxStackSize;

        /// <summary>
        /// The prefab spawned in the world when this item is dropped.
        /// Assigned at authoring time — no runtime loading.
        /// </summary>
        public GameObject WorldPrefab => _worldPrefab;
    }
}