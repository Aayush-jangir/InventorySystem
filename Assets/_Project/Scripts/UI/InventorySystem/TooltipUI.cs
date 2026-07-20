using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Singleton tooltip panel that displays item details on slot hover.
    /// Positions itself near the pointer and clamps to screen bounds.
    /// Hidden by default — shown and hidden via Show() and Hide().
    /// Must be placed as a late child of InventoryCanvas (before DragGhost).
    /// </summary>
    public class TooltipUI : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────

        /// <summary>Global access point for the tooltip.</summary>
        public static TooltipUI Instance { get; private set; }

        // ── Inspector references ───────────────────────────────────────────

        [Header("Panel")]
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _rarityTypeText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("Offset")]
        [SerializeField] private Vector2 _pointerOffset = new Vector2(12f, -12f);

        // ── Private state ──────────────────────────────────────────────────

        private RectTransform _canvasRect;
        private Canvas _rootCanvas;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_PANEL = "[TooltipUI] No Panel RectTransform assigned.";
        private const string ERROR_NO_CANVAS = "[TooltipUI] No CanvasGroup assigned.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _rootCanvas = GetComponentInParent<Canvas>();
            if (_rootCanvas != null)
                _canvasRect = _rootCanvas.transform as RectTransform;

            if (_panelRect == null) Debug.LogError(ERROR_NO_PANEL);
            if (_canvasGroup == null) Debug.LogError(ERROR_NO_CANVAS);

            Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnEnable()
        {
            InventoryEvents.OnInventoryClosed += Hide;
        }

        private void OnDisable()
        {
            InventoryEvents.OnInventoryClosed -= Hide;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Populates and shows the tooltip for the given item at the given screen position.
        /// Does nothing if a drag is currently in progress.
        /// </summary>
        public void Show(ItemInstance item, InventoryConfigSO config, Vector2 screenPosition)
        {
            // Never show a tooltip while dragging — it would obscure the ghost
            if (InventoryDragController.Instance != null &&
                InventoryDragController.Instance.IsDragging) return;

            if (item == null) return;

            PopulateContent(item, config);
            SetVisible(true);
            PositionNearPointer(screenPosition);
        }

        /// <summary>
        /// Hides the tooltip immediately.
        /// </summary>
        public void Hide()
        {
            SetVisible(false);
        }

        /// <summary>
        /// Updates the tooltip position to follow the pointer.
        /// Called by InventorySlotTooltipHandler on pointer move.
        /// </summary>
        public void UpdatePosition(Vector2 screenPosition)
        {
            if (_canvasGroup == null || _canvasGroup.alpha < 1f) return;
            PositionNearPointer(screenPosition);
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void PopulateContent(ItemInstance item, InventoryConfigSO config)
        {
            // Item name — colored by rarity
            if (_itemNameText != null)
            {
                _itemNameText.SetText(item.Data.ItemName);

                if (config != null)
                    _itemNameText.color = config.GetRarityColor(item.Data.Rarity);
            }

            // Rarity + type combined: e.g. "Rare  •  Weapon"
            if (_rarityTypeText != null)
            {
                _rarityTypeText.SetText(
                    $"{item.Data.Rarity}  \u2022  {item.Data.ItemType}");
            }

            // Description
            if (_descriptionText != null)
            {
                bool hasDescription = !string.IsNullOrEmpty(item.Data.Description);
                _descriptionText.gameObject.SetActive(hasDescription);

                if (hasDescription)
                    _descriptionText.SetText(item.Data.Description);
            }
        }

        private void SetVisible(bool visible)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = false; // never block slot events
            _canvasGroup.interactable = false;
        }

        private void PositionNearPointer(Vector2 screenPosition)
        {
            if (_panelRect == null || _canvasRect == null) return;

            Camera cam = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _rootCanvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect,
                screenPosition,
                cam,
                out Vector2 localPoint);

            // Apply offset so tooltip does not sit directly under the cursor
            localPoint += _pointerOffset;

            // Clamp so the tooltip stays within the canvas bounds
            Vector2 canvasSize = _canvasRect.rect.size;
            Vector2 tooltipSize = _panelRect.rect.size;
            Vector2 halfCanvas = canvasSize * 0.5f;

            localPoint.x = Mathf.Clamp(localPoint.x,
                -halfCanvas.x,
                 halfCanvas.x - tooltipSize.x);

            localPoint.y = Mathf.Clamp(localPoint.y,
                -halfCanvas.y + tooltipSize.y,
                 halfCanvas.y);

            _panelRect.localPosition = localPoint;
        }
    }
}