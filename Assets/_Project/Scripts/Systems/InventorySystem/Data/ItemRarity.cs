namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Defines the rarity tier of an item.
    /// Order matters — higher value = rarer.
    /// Used for border color, tooltip color, and sort logic.
    /// </summary>
    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }
}