using System.Collections.Generic;
using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Object pool for WorldItem instances.
    /// Listens for OnItemDroppedToWorld and spawns a pooled WorldItem at the drop position.
    /// Never uses Instantiate/Destroy at runtime — pool is pre-warmed on Awake.
    /// Attach to the same GameObject as InventorySystem and HotbarSystem.
    /// </summary>
    public class ItemDropper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InventoryConfigSO _config;

        [Header("Pool Settings")]
        [SerializeField] private WorldItem _worldItemPrefab;
        [SerializeField][Min(4)] private int _poolSize = 16;

        [Header("Drop Settings")]
        [SerializeField] private float _dropScatterRadius = 0.5f;

        private readonly List<WorldItem> _pool = new List<WorldItem>();
        private Transform _poolParent;

        // ── Constants ─────────────────────────────────────────────────────

        private const string ERROR_NO_PREFAB = "[ItemDropper] No WorldItem prefab assigned.";
        private const string POOL_PARENT_NAME = "[WorldItem Pool]";

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (_worldItemPrefab == null)
            {
                Debug.LogError(ERROR_NO_PREFAB);
                return;
            }

            CreatePoolParent();
            PrewarmPool();
        }

        private void OnEnable()
        {
            InventoryEvents.OnItemDroppedToWorld += HandleItemDropped;
        }

        private void OnDisable()
        {
            InventoryEvents.OnItemDroppedToWorld -= HandleItemDropped;
        }

        // ── Pool Setup ────────────────────────────────────────────────────

        private void CreatePoolParent()
        {
            GameObject parent = new GameObject(POOL_PARENT_NAME);
            parent.transform.SetParent(transform);
            _poolParent = parent.transform;
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                WorldItem instance = Instantiate(_worldItemPrefab, _poolParent);
                instance.gameObject.SetActive(false);
                _pool.Add(instance);
            }
        }

        // ── Pool API ──────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves an inactive WorldItem from the pool.
        /// If all pool slots are in use, the pool grows by one (logged as a warning).
        /// </summary>
        private WorldItem GetFromPool()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].gameObject.activeSelf)
                    return _pool[i];
            }

            // Pool exhausted — grow by one and log so the developer can
            // increase _poolSize in the Inspector if this happens often.
            Debug.LogWarning("[ItemDropper] Pool exhausted — growing by 1. " +
                             "Consider increasing Pool Size in the Inspector.");

            WorldItem newInstance = Instantiate(_worldItemPrefab, _poolParent);
            newInstance.gameObject.SetActive(false);
            _pool.Add(newInstance);
            return newInstance;
        }

        /// <summary>
        /// Spawns a WorldItem from the pool at the given world position.
        /// Applies a small random scatter so stacked drops do not overlap.
        /// </summary>
        public void SpawnWorldItem(ItemInstance itemInstance, Vector3 position)
        {
            if (itemInstance == null || itemInstance.Data == null) return;

            WorldItem worldItem = GetFromPool();

            Vector2 scatter = Random.insideUnitCircle * _dropScatterRadius;
            Vector3 spawnPos = position + new Vector3(scatter.x, scatter.y, 0f);

            worldItem.transform.position = spawnPos;

            float cooldown = 0f;
            if (_config != null)
                cooldown = _config.PickupCooldown;

            worldItem.gameObject.SetActive(true);
            worldItem.Initialise(itemInstance.Data, itemInstance.StackCount, cooldown);
        }

        // ── Event Handlers ────────────────────────────────────────────────

        private void HandleItemDropped(ItemInstance itemInstance, Vector3 position)
        {
            SpawnWorldItem(itemInstance, position);
        }

        // ── Debug helpers (Editor only) ───────────────────────────────────

        [ContextMenu("Debug — Print Pool Status")]
        private void DebugPrintPoolStatus()
        {
            int active = 0;
            int inactive = 0;

            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i].gameObject.activeSelf) active++;
                else inactive++;
            }

            Debug.Log($"[ItemDropper] Pool size: {_pool.Count} " +
                      $"| Active: {active} | Available: {inactive}");
        }
    }
}