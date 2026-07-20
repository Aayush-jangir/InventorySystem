// NEW
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Floating slot clone that follows the pointer during a drag operation.
    /// Mirrors the dragged item's rarity color on the background and shows
    /// the item icon if one is assigned — no extra sprite assets required.
    /// Must be the last child of InventoryCanvas so it renders on top of everything.
    /// Controlled exclusively by InventoryDragController.
    /// </summary>
    public class DragGhostUI : MonoBehaviour
    {
        // NEW
        [Header("References")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _rarityBorderImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _stackCountText;

        private RectTransform _rectTransform;
        private Canvas _rootCanvas;

        // ── Constants ──────────────────────────────────────────────────────

        private const float GHOST_ALPHA = 0.85f;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            TryGetComponent(out _rectTransform);
            _rootCanvas = GetComponentInParent<Canvas>();
            gameObject.SetActive(false);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Activates the ghost and populates it to look like the dragged slot.
        /// Background tinted by rarity, icon shown if available.
        /// No external sprite assets required — reads directly from ItemInstance.
        /// </summary>
        public void Show(ItemInstance item, InventoryConfigSO config, Vector2 screenPosition)
        {
            if (item == null) return;

            Color rarityColor = config != null
                ? config.GetRarityColor(item.Data.Rarity)
                : Color.white;

            // -- Background: dark slot color at reduced alpha for ghost feel --
            if (_backgroundImage != null)
            {
                Color bg = new Color(0.118f, 0.118f, 0.118f, GHOST_ALPHA);
                _backgroundImage.color = bg;
            }

            // -- Rarity border: full rarity color --
            if (_rarityBorderImage != null)
            {
                _rarityBorderImage.enabled = true;
                _rarityBorderImage.color = rarityColor;
            }

            // NEW
            // -- Icon: show if assigned, hide if null --
            // NEW
            if (_iconImage != null)
            {
                if (item.Data.Icon != null)
                {
                    _iconImage.enabled = true;
                    _iconImage.sprite = item.Data.Icon;
                    _iconImage.color = Color.white;
                }
                else
                {
                    _iconImage.enabled = true;
                    _iconImage.sprite = null;
                    _iconImage.color = rarityColor;
                }
            }

            // -- Stack count: only show when more than 1 --
            if (_stackCountText != null)
            {
                bool showCount = item.StackCount >= 2;
                _stackCountText.enabled = showCount;
                if (showCount)
                    _stackCountText.SetText("{0}", item.StackCount);
            }

            gameObject.SetActive(true);
            UpdatePosition(screenPosition);

            gameObject.SetActive(true);
            UpdatePosition(screenPosition);
        }

        /// <summary>
        /// Hides the ghost and resets all visuals ready for the next drag.
        /// </summary>
        public void Hide()
        {
            // NEW
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.color = Color.white;
                _iconImage.enabled = false;
            }

            // NEW
            if (_rarityBorderImage != null)
                _rarityBorderImage.enabled = false;

            if (_stackCountText != null)
                _stackCountText.enabled = false;

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Moves the ghost to match the current pointer position each frame.
        /// </summary>
        public void UpdatePosition(Vector2 screenPosition)
        {
            if (_rectTransform == null || _rootCanvas == null) return;

            Camera cam = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _rootCanvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                screenPosition,
                cam,
                out Vector2 localPoint);

            _rectTransform.localPosition = localPoint;
        }
    }
}