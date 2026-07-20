using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Master configuration asset for the Inventory System.
    /// Controls grid dimensions, hotbar size, and rarity border colors.
    /// Create via: Right-click → Create → PlayMatrix → Inventory System → Inventory Config
    /// Assign one instance to InventorySystem in the scene.
    /// </summary>
    [CreateAssetMenu(
        fileName = "InventoryConfig_Default",
        menuName = "PlayMatrix/Inventory System/Inventory Config")]
    public class InventoryConfigSO : ScriptableObject
    {
        [Header("Grid Settings")]
        [SerializeField][Min(1)] private int _rows = 4;
        [SerializeField][Min(1)] private int _columns = 6;

        [Header("Hotbar Settings")]
        [SerializeField][Min(1)] private int _hotbarSlotCount = 5;

        [Header("Pickup Settings")]
        [SerializeField] private bool _autoPickup = false;
        [SerializeField][Min(0f)] private float _pickupCooldown = 0.8f;

        [Header("Rarity Colors")]
        [SerializeField] private Color _commonColor = new Color(0.78f, 0.78f, 0.78f); // silver-grey
        [SerializeField] private Color _uncommonColor = new Color(0.12f, 0.75f, 0.12f); // green
        [SerializeField] private Color _rareColor = new Color(0.00f, 0.44f, 0.87f); // blue
        [SerializeField] private Color _epicColor = new Color(0.64f, 0.21f, 0.93f); // purple
        [SerializeField] private Color _legendaryColor = new Color(1.00f, 0.50f, 0.00f); // orange-gold


        // ── Grid properties ───────────────────────────────────────────────

        /// <summary>Number of rows in the inventory grid.</summary>
        public int Rows => _rows;

        /// <summary>Number of columns in the inventory grid.</summary>
        public int Columns => _columns;

        /// <summary>
        /// Number of grid-only slots (rows × columns).
        /// Used by InventoryUI to build the visible grid — does NOT include hotbar slots.
        /// </summary>
        public int GridSlotCount => _rows * _columns;

        /// <summary>
        /// The slot index inside InventorySystem's slot array where hotbar slots begin.
        /// Hotbar slot 0 → array index HotbarStartIndex + 0, and so on.
        /// </summary>
        public int HotbarStartIndex => _rows * _columns;

        /// <summary>
        /// Total slots owned by InventorySystem — grid slots plus dedicated hotbar slots.
        /// Changing rows, columns, or hotbarSlotCount here automatically adjusts everything.
        /// </summary>
        public int TotalSlots => _rows * _columns + _hotbarSlotCount;

        // ── Hotbar properties ─────────────────────────────────────────────

        /// <summary>Number of slots in the hotbar row.</summary>
        public int HotbarSlotCount => _hotbarSlotCount;

        // ── Pickup properties ─────────────────────────────────────────────

        /// <summary>
        /// If true, items are picked up automatically on contact (Minecraft style).
        /// If false, the player must press Interact (E / tap) to pick up.
        /// </summary>
        public bool AutoPickup => _autoPickup;

        /// <summary>
        /// Seconds after a WorldItem spawns before it can be picked up.
        /// Prevents immediately re-picking up a just-dropped item.
        /// Only relevant when AutoPickup is true.
        /// </summary>
        public float PickupCooldown => _pickupCooldown;

        // ── Rarity color API ──────────────────────────────────────────────

        /// <summary>
        /// Returns the border color that corresponds to the given rarity tier.
        /// Used by InventorySlotUI and HotbarSlotUI to tint the rarity border image.
        /// </summary>
        public Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return _commonColor;
                case ItemRarity.Uncommon: return _uncommonColor;
                case ItemRarity.Rare: return _rareColor;
                case ItemRarity.Epic: return _epicColor;
                case ItemRarity.Legendary: return _legendaryColor;
                default: return _commonColor;
            }
        }
    }
}