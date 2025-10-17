/*
 * ---------------------------------------------------------------------------
 * Description: Simple test script demonstrating input handling for jump and movement using 
 *              the new Input System and applying forces/velocities to a Rigidbody.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.InputSystem;
using InputSystemExtension;
using UnityEngine;

[AddComponentMenu("UI/Input System Extension/Debug/Test Script", 2)]
public class TestScript : MonoBehaviour
{
    #region === Fields ===

    [Header("Input Settings")]
    [SerializeField, GetAction, Tooltip("Input action reference for jumping (float expected).")]
    private InputActionReference actionJump;

    [SerializeField, GetAction, Tooltip("Input action reference for movement (Vector2 expected).")]
    private InputActionReference actionMove;

    [Header("Target Settings")]
    [SerializeField, Tooltip("Rigidbody to apply the movement and jump forces.")]
    private Rigidbody targetRigidbody;

    [SerializeField, Tooltip("Movement speed multiplier.")]
    private float speed = 5f;

    [Header("Display Manager")]
    [SerializeField, Tooltip("Reference to the Input Display Manager used to show or hide input UI.")]
    private InputDisplayManager displayManager;

    [Space(5)]
    [SerializeField, Tooltip("Tag name used to identify input display elements.")]
    private string nameTag = "Interact";

    [SerializeField, Tooltip("Determines whether the display should be enabled or disabled.")]
    private bool enable = true;

    private OnInputSystemEventConfig<float> jumpInputEvent;  // Input event configuration for jump action.
    private OnInputSystemEventConfig<Vector2> moveInputEvent; // Input event configuration for move action.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the input action reference used for jumping.
    /// </summary>
    public InputActionReference ActionJump
    {
        get => actionJump;
        set => actionJump = value;
    }

    /// <summary>
    /// Gets or sets the input action reference used for movement.
    /// </summary>
    public InputActionReference ActionMove
    {
        get => actionMove;
        set => actionMove = value;
    }

    /// <summary>
    /// Gets or sets the target Rigidbody that receives movement and jump forces.
    /// </summary>
    public Rigidbody TargetRigidbody
    {
        get => targetRigidbody;
        set => targetRigidbody = value;
    }

    /// <summary>
    /// Gets or sets the movement speed multiplier.
    /// </summary>
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    /// <summary>
    /// Gets or sets the Input Display Manager reference.
    /// </summary>
    public InputDisplayManager DisplayManager
    {
        get => displayManager;
        set => displayManager = value;
    }

    /// <summary>
    /// Gets or sets the name tag associated with the display manager viewer.
    /// </summary>
    public string NameTag
    {
        get => nameTag;
        set => nameTag = value;
    }

    /// <summary>
    /// Gets or sets whether the input viewer display should be enabled.
    /// </summary>
    public bool Enable
    {
        get => enable;
        set => enable = value;
    }

    #endregion

    #region === Unity Methods ===

    private void Start()
    {
        // Configure jump input event.
        // When the jump action is pressed, reset vertical velocity and apply an upward impulse.
        jumpInputEvent = OnInputSystemEvent<float>.WithAction(actionJump, this)
            .OnPressed(_ =>
            {
                // Reset Y-axis velocity to ensure consistent jumping height.
                targetRigidbody.linearVelocity = new Vector3(targetRigidbody.linearVelocity.x, 0f, targetRigidbody.linearVelocity.z);

                // Apply a vertical impulse to simulate a jump.
                targetRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            });

        // Configure move input event.
        // While movement input is held, update horizontal velocity according to input direction and speed.
        moveInputEvent = OnInputSystemEvent<Vector2>.WithAction(actionMove, this)
            .OnHold(value =>
            {
                // Calculate directional movement based on input and speed.
                Vector3 direction = new(value.x * speed, targetRigidbody.linearVelocity.y, value.y * speed);

                // Apply horizontal movement while preserving vertical velocity.
                targetRigidbody.linearVelocity = direction;
            })
            .OnReleased(() =>
            {
                // Stop horizontal movement while preserving vertical velocity.
                targetRigidbody.linearVelocity = new Vector3(0f, targetRigidbody.linearVelocity.y, 0f);
            });
    }

    private void OnDestroy()
    {
        // Unsubscribe and dispose events to prevent memory leaks.
        jumpInputEvent?.Dispose();
        moveInputEvent?.Dispose();
    }

    #endregion

    #region === Public Methods ===

    /// <summary>
    /// Enables or disables the input viewer UI associated with the current name tag.
    /// </summary>
    [ContextMenu("Enable And Disable Viewer")]
    public void EnableAndDisableViewer()
    {
        // Toggle the visibility of the input display viewer.
        displayManager.EnableAndDisableViewer(nameTag, enable);
    }

    #endregion
}