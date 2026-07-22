using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Represents a physical item sitting in the game world, waiting to be picked up.
    /// Detects player proximity via a trigger collider.
    /// When the player interacts, fires InventoryEvents.WorldItemPickedUp.
    /// Managed by ItemDropper — never destroy this directly, call Deactivate() instead.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class WorldItem : MonoBehaviour
    {
        [Header("Pickup Settings")]
        [SerializeField] private float _pickupRadius = 1.2f;
        [SerializeField] private InventoryConfigSO _config;

        private bool _canBePickedUp = true;
        private Coroutine _cooldownCoroutine;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _floatRoot;

        // ── Runtime data ──────────────────────────────────────────────────

        private ItemDataSO _itemData;
        private int _stackCount;
        private CircleCollider2D _collider;
        private bool _playerInRange;

        /// <summary>
        /// True while the player is inside the pickup trigger radius.
        /// Used by UI to show/hide the interaction prompt.
        /// </summary>
        public bool IsPlayerInRange => _playerInRange;

        // ── Static interactable registry ──────────────────────────────────

        /// <summary>
        /// The WorldItem the player is currently standing next to.
        /// InventoryInputReader reads this and calls Pickup() when E is pressed.
        /// Only one WorldItem can be the current interactable at a time.
        /// </summary>
        public static WorldItem CurrentInteractable { get; private set; }

        // ── Public read-only data ─────────────────────────────────────────

        /// <summary>The item blueprint this world object represents.</summary>
        public ItemDataSO ItemData => _itemData;

        /// <summary>How many of this item will be added to the inventory on pickup.</summary>
        public int StackCount => _stackCount;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (_spriteRenderer == null)
                TryGetComponent(out _spriteRenderer);

            TryGetComponent(out _collider);

            if (_collider != null)
            {
                _collider.isTrigger = true;
                _collider.radius = _pickupRadius;
            }
        }

        private void OnDisable()
        {
            // If this item was the current interactable when deactivated,
            // clear the static reference so nothing tries to pick it up.
            if (CurrentInteractable == this)
                CurrentInteractable = null;

            _playerInRange = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = true;
            CurrentInteractable = this;

            // Auto-pickup mode — pick up immediately if cooldown has expired
            if (_config != null && _config.AutoPickup && _canBePickedUp)
                Pickup();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = false;

            if (CurrentInteractable == this)
                CurrentInteractable = null;
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Initialises this WorldItem with item data, stack count, and optional pickup cooldown.
        /// Called by ItemDropper after retrieving from pool.
        /// </summary>
        public void Initialise(ItemDataSO data, int stackCount = 1, float cooldown = 0f)
        {
            _itemData = data;
            _stackCount = Mathf.Max(1, stackCount);

            // NEW
            if (_spriteRenderer != null && data != null)
            {
                // Use assigned icon if available, otherwise keep the default sprite
                if (data.Icon != null)
                    _spriteRenderer.sprite = data.Icon;

                // Always tint by rarity color so world items are visually distinct
                _spriteRenderer.color = _config != null
                    ? _config.GetRarityColor(data.Rarity)
                    : Color.white;
            }

            // Start cooldown if one was requested
            if (cooldown > 0f)
            {
                _canBePickedUp = false;

                if (_cooldownCoroutine != null)
                    StopCoroutine(_cooldownCoroutine);

                _cooldownCoroutine = StartCoroutine(PickupCooldownRoutine(cooldown));
            }
            else
            {
                _canBePickedUp = true;
            }
        }

        /// <summary>
        /// Called by InventoryInputReader when the player presses Interact (E / tap).
        /// Fires the pickup event and deactivates this object back into the pool.
        /// </summary>
        public void Pickup()
        {
            if (_itemData == null) return;

            InventoryEvents.WorldItemPickedUp(this);
            Deactivate();
        }

        /// <summary>
        /// Returns this WorldItem to the pool by deactivating it.
        /// Always use this instead of Destroy.
        /// </summary>
        public void Deactivate()
        {
            if (_cooldownCoroutine != null)
            {
                StopCoroutine(_cooldownCoroutine);
                _cooldownCoroutine = null;
            }

            _canBePickedUp = true;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Waits for the cooldown duration then allows this item to be picked up.
        /// Prevents immediate re-pickup after dropping.
        /// </summary>
        private System.Collections.IEnumerator PickupCooldownRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            _canBePickedUp = true;

            // If the player is already standing on the item when cooldown expires
            // and auto-pickup is on, pick it up immediately.
            if (_config != null && _config.AutoPickup && _playerInRange)
                Pickup();
        }
    }
}