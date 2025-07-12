using UnityEngine.InputSystem;
using InputSystemExtension;
using UnityEngine;

[AddComponentMenu("UI/Input System Extension/Test Script", 4)]
public class TestScript : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField, GetAction] private InputActionReference action; // Reference to an InputAction (selected via custom GetAction attribute)
    [Space(5)]
    [SerializeField] private Rigidbody targetRigidbody; // Rigidbody component to apply physics forces to

    // Holds the input event configuration for the specified InputAction (float type expected)
    private OnInputSystemEventConfig<float> testInputEvent;

    private void Start()
    {
        // Use the actual InputAction from the InputActionReference for the new WithAction method
        testInputEvent = OnInputSystemEvent<float>.WithAction(action.action)
            .OnPressed(_ =>
            {
                // Reset current velocity
                targetRigidbody.linearVelocity = Vector3.zero;

                // Apply an impulse force upwards
                targetRigidbody.AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
            });
    }

    private void OnDestroy()
    {
        // Unbind all callbacks to prevent memory leaks or invalid callbacks
        testInputEvent?.UnbindAll();
    }
}