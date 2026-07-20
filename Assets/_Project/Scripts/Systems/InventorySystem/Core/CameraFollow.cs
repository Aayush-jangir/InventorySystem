using UnityEngine;

namespace PlayMatrix.InventorySystem
{
    /// <summary>
    /// Smoothly follows a target Transform in top-down 2D.
    /// DEMO ONLY — not part of the reusable inventory system.
    /// Attach to Main Camera.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 8f;

        private const string ERROR_NO_TARGET =
            "[CameraFollow] No follow target assigned.";

        private void Awake()
        {
            if (_target == null)
                Debug.LogError(ERROR_NO_TARGET);
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = new Vector3(
                _target.position.x,
                _target.position.y,
                transform.position.z);

            transform.position = Vector3.Lerp(
                transform.position,
                desired,
                _smoothSpeed * Time.deltaTime);
        }
    }
}