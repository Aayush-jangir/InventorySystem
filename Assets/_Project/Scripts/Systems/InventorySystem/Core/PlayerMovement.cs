using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Simple top-down player movement for the inventory demo scene.
    /// DEMO ONLY — not part of the reusable inventory system.
    /// Uses the new Input System for zero-allocation keyboard reading.
    /// Requires a Rigidbody2D on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 4f;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_RIGIDBODY =
            "[PlayerMovement] Rigidbody2D not found on Player.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (!TryGetComponent(out _rigidbody))
                Debug.LogError(ERROR_NO_RIGIDBODY);
        }

        private void Update()
        {
            // Direct keyboard polling — no allocations, no events needed
            // for a simple demo movement script
            float x = 0f;
            float y = 0f;

            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;

            _moveInput = new Vector2(x, y).normalized;
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null) return;
            _rigidbody.linearVelocity = _moveInput * _moveSpeed;
        }
    }
}