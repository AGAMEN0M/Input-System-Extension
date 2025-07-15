/*
 * ---------------------------------------------------------------------------
 * Description: Simple test script demonstrating input handling for jump and movement using the new Input System
 *              and applying forces/velocities to a Rigidbody.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.InputSystem;
using InputSystemExtension;
using UnityEngine;

[AddComponentMenu("UI/Input System Extension/Test Script", 4)]
public class TestScript : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField, GetAction] private InputActionReference actionJump; // Reference to the input action for jumping (float expected).
    [SerializeField, GetAction] private InputActionReference actionMove; // Reference to the input action for moving (Vector2 expected).
    [Space(5)]
    [SerializeField] private Rigidbody targetRigidbody; // Rigidbody to apply the movement and jump force.
    [SerializeField] private float speed = 5; // Movement speed multiplier.

    [Header("Display Manager")]
    [SerializeField] private InputDisplayManager displayManager; // Reference to the input display manager used to show/hide input UI.
    [Space(5)]
    [SerializeField] private string nameTag = "Interact"; // Tag name to identify input display elements.
    [SerializeField] private bool enable = true; // Toggle flag to determine whether the display should be enabled or disabled.

    private OnInputSystemEventConfig<float> jumpInputEvent; // Input event configuration for the jump action.
    private OnInputSystemEventConfig<Vector2> moveInputEvent; // Input event configuration for the move action.

    private void Start()
    {
        // Configure the jump input event.
        // When the action is pressed, reset the velocity and apply an upward force to simulate jumping.
        jumpInputEvent = OnInputSystemEvent<float>.WithAction(actionJump)
            .OnPressed(_ =>
            {
                // Reset current velocity to avoid unintended movement influence.
                targetRigidbody.linearVelocity = new Vector3(targetRigidbody.linearVelocity.x, 0, targetRigidbody.linearVelocity.y);

                // Apply a vertical impulse force for jumping.
                targetRigidbody.AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
            });

        // Configure the move input event.
        // While the move action is held, apply horizontal movement based on input direction and speed.
        moveInputEvent = OnInputSystemEvent<Vector2>.WithAction(actionMove)
            .OnHold(value =>
            {
                // Calculate the movement direction scaled by the speed.
                var direction = value * speed;

                // Set the horizontal velocity while preserving the vertical component.
                targetRigidbody.linearVelocity = new Vector3(direction.x, targetRigidbody.linearVelocity.y, direction.y);
            })
            .OnReleased(() =>
            {
                // Stop horizontal movement while keeping vertical movement unaffected.
                targetRigidbody.linearVelocity = new Vector3(0, targetRigidbody.linearVelocity.y, 0);
            });
    }

    private void OnDestroy()
    {
        // Unbind all events when the script is destroyed to avoid memory leaks or unexpected behavior.
        jumpInputEvent?.UnbindAll();
        moveInputEvent?.UnbindAll();
    }

    /// <summary>
    /// Enables or disables the input viewer UI associated with the given name tag.
    /// </summary>
    [ContextMenu("Enable And Disable Viewer")]
    public void EnableAndDisableViewer()
    {
        // Toggle the display manager viewer visibility using the provided tag and flag.
        displayManager.EnableAndDisableViewer(nameTag, enable);
    }
}