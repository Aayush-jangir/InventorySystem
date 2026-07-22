#pragma warning disable CS0414 // Field assigned but never used (intentional — used in UNITY_EDITOR block only)
using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Shows or hides the mobile on-screen controls canvas based on the
    /// current platform. Attach to the PFB_MobileControls prefab root.
    ///
    /// On Android  → controls are shown automatically.
    /// On PC/WebGL → controls are hidden automatically.
    /// In Editor   → controls are hidden unless Force Show In Editor is ticked.
    ///
    /// The Force Show In Editor checkbox lets you test the mobile layout
    /// without building to a device.
    /// </summary>
    public class MobileControlsUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Canvas GameObject that contains all on-screen controls " +
                 "(joystick, inventory button, pickup button).")]
        [SerializeField] private GameObject _mobileControlsCanvas;

        [Header("Settings")]
        [Tooltip("Tick this to force mobile controls visible in the Editor " +
                 "so you can test the layout without building to Android.")]
        [SerializeField] private bool _forceShowInEditor = false;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_CANVAS =
            "[MobileControlsUI] No Mobile Controls Canvas assigned.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Start()
        {
            if (_mobileControlsCanvas == null)
            {
                Debug.LogError(ERROR_NO_CANVAS);
                return;
            }

#if UNITY_EDITOR
            _mobileControlsCanvas.SetActive(_forceShowInEditor);
#else
            _mobileControlsCanvas.SetActive(Application.isMobilePlatform);
#endif
        }
    }
}