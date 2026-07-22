using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Simple top-down player movement for the inventory demo scene.
    /// DEMO ONLY — not part of the reusable inventory system.
    /// Reads movement from InventoryInputActions Player/Move so that
    /// OnScreenStick on mobile feeds in automatically — no extra wiring needed.
    /// Requires a Rigidbody2D on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 4f;

        private Rigidbody2D _rigidbody;
        private InventoryInputActions _inputActions;
        private Vector2 _moveInput;

        // ── Constants ──────────────────────────────────────────────────────

        private const string ERROR_NO_RIGIDBODY =
            "[PlayerMovement] Rigidbody2D not found on Player.";

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (!TryGetComponent(out _rigidbody))
                Debug.LogError(ERROR_NO_RIGIDBODY);

            _inputActions = new InventoryInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void Update()
        {
            _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null) return;
            _rigidbody.linearVelocity = _moveInput * _moveSpeed;
        }
    }
}