/*
 * ---------------------------------------------------------------------------
 * Description: Manages input display UI elements by automatically updating input icons
 *              based on current input control type (Keyboard or Gamepad) and input events.
 *              Supports single inputs and directional inputs with color transitions.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.InputSystem;
using InputSystemExtension;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

using static UnityEngine.InputSystem.InputSystem;

[AddComponentMenu("UI/Input System Extension/Input Display Manager", 6)]
public class InputDisplayManager : MonoBehaviour
{
    #region === Enums ===

    /// <summary>
    /// Defines the type of input control being used.
    /// </summary>
    public enum ControlType
    {
        Keyboard,  // Represents keyboard-based input.
        Gamepad    // Represents gamepad-based input.
    }

    #endregion

    #region === Serializable Structs ===

    /// <summary>
    /// Struct that defines data for a single input viewer.
    /// Used to display a single input icon and handle its events.
    /// </summary>
    [Serializable]
    public struct InputViewerData
    {
        public string nameTag; // Identifier for this viewer entry.
        public bool enable; // Whether this input view is active.

        [Space(10)]
        [GetAction] public InputActionReference inputActionReference; // Reference to the Input Action asset.

        // Binding identifiers used to select the correct input icon based on control type.
        [BindingId(nameof(inputActionReference))] public string keyboardId;
        [BindingId(nameof(inputActionReference))] public string gamepadId;

        [Space(10)]
        public Image inputIcon; // UI Image used to display the icon.

        [NonSerialized] public OnInputSystemEventConfig<float> inputEvent; // Input event handler for float input types (e.g., button press).
    }

    /// <summary>
    /// Struct that defines data for multiple input directions (Up, Down, Left, Right).
    /// Used to handle directional inputs like D-Pad or Arrow Keys.
    /// </summary>
    [Serializable]
    public struct InputMultipleViewsData
    {
        public string nameTag; // Identifier for this directional viewer.
        public bool enable; // Whether this set is active.

        [Space(10)]
        [GetAction] public InputActionReference inputActionReference; // Reference to the Input Action asset.

        // Binding identifiers used to select the correct input icon based on control type.
        [BindingId(nameof(inputActionReference))] public string keyboardId;
        [BindingId(nameof(inputActionReference))] public string gamepadId;

        [Space(10)]
        public Image inputIconUp;     // Icon representing the "up" input.
        public Image inputIconDown;   // Icon representing the "down" input.
        public Image inputIconLeft;   // Icon representing the "left" input.
        public Image inputIconRight;  // Icon representing the "right" input.

        [NonSerialized] public OnInputSystemEventConfig<Vector2> inputEvent; // Input event handler for directional input.
    }

    #endregion

    #region === Inspector Fields ===

    [Header("Auto Detection Settings")]
    [SerializeField] private bool automatic = true; // If true, the control type (Keyboard or Gamepad) will be automatically detected at runtime.
    [SerializeField] private ControlType controlType = ControlType.Keyboard; // Default control type used when automatic detection is disabled.

    [Header("Input Visual Settings")]
    [SerializeField] private Color activatedColor = Color.white; // The color to apply when an input is active (e.g., pressed).
    [SerializeField] private Color disabledColor = Color.gray; // The color to apply when an input is inactive or released.
    [Space(10)]
    [SerializeField] private float transitionTime = 0.1f; // Duration of the color transition when an input changes state.
    [SerializeField] private float hideTransitionTime = 0.5f; // Duration of the fade-out effect when disabling a viewer.

    [Header("Input Viewer Data")]
    [SerializeField] private List<InputViewerData> inputViewerData = new(); // List of single input data (e.g., jump button).
    [SerializeField] private List<InputMultipleViewsData> inputMultipleViewsData = new(); // List of directional input data (e.g., movement arrows or joystick directions).

    #endregion

    #region === Private Fields ===

    // Cached reference to the extension data containing sprites and input mappings.
    // Used to retrieve appropriate icons based on control type and bindings.
    private InputSystemExtensionData extensionData;

    // Stores active color transition coroutines for each image.
    // This ensures that multiple transitions on the same image don't overlap.
    private readonly Dictionary<Image, Coroutine> colorTransitions = new();

    #endregion

    #region === Public Properties ===

    /// <summary>
    /// Enables or disables automatic control type detection.
    /// </summary>
    public bool SetAutomatic
    {
        get => automatic;
        set => automatic = value;
    }

    /// <summary>
    /// Sets or gets the current control type (Keyboard or Gamepad).
    /// When set manually, it also updates the input icons accordingly.
    /// </summary>
    public ControlType SetControlType
    {
        get => controlType;
        set
        {
            // Update control type and refresh icons only if the value changed.
            if (controlType != value)
            {
                controlType = value;
                UpdateIcons(); // Refresh icons to match the new control type.
            }
        }
    }

    /// <summary>
    /// Gets or sets the list of input viewer data (single input).
    /// This is mainly used for editor access or dynamic modification.
    /// </summary>
    public List<InputViewerData> InputViewerDataEditor
    {
        get => inputViewerData;
        set => inputViewerData = value;
    }

    /// <summary>
    /// Gets or sets the list of directional input data.
    /// Useful for editor editing or runtime configuration.
    /// </summary>
    public List<InputMultipleViewsData> InputMultipleViewsDataEditor
    {
        get => inputMultipleViewsData;
        set => inputMultipleViewsData = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary>
    /// Called before Start(). Validates the input data to ensure all fields are properly assigned.
    /// </summary>
    private void Awake()
    {
        ValidateData(); // Perform initial validation on serialized input lists.
    }

    /// <summary>
    /// Called at the start of the scene. Initializes all input event bindings.
    /// </summary>
    private void Start()
    {
        InitializeInputViewerData(); // Setup event handlers for single input icons.
        InitializeInputMultipleViewsData(); // Setup event handlers for directional inputs.
    }

    /// <summary>
    /// Called when the GameObject is enabled. Registers device change listeners and updates icons.
    /// </summary>
    private void OnEnable()
    {
        if (automatic) DetectControlType(); // Automatically determine the control type based on connected devices.

        onDeviceChange += OnDeviceChange; // Subscribe to device change events.

        UpdateIcons(); // Apply appropriate sprites to icons based on current control type.
    }

    /// <summary>
    /// Called when the GameObject is disabled. Unsubscribes from device change events.
    /// </summary>
    private void OnDisable()
    {
        onDeviceChange -= OnDeviceChange; // Stop listening for device changes.
    }

    /// <summary>
    /// Called when the GameObject is destroyed. Ensures that all resources and coroutines are properly cleaned.
    /// </summary>
    private void OnDestroy()
    {
        UnbindAllEvents(); // Remove all input event bindings.
        StopAllColorTransitions(); // Stop any ongoing color animations.
    }

    #endregion

    #region === Validation ===

    /// <summary>
    /// Validates all serialized input data to ensure required fields are assigned correctly.
    /// Logs errors in the console if any issues are found.
    /// </summary>
    private void ValidateData()
    {
        bool hasError = false;

        // Validate each InputViewerData entry.
        for (int i = 0; i < inputViewerData.Count; i++)
        {
            var data = inputViewerData[i];

            // Check if the name tag is assigned.
            if (string.IsNullOrWhiteSpace(data.nameTag))
            {
                Debug.LogError($"[InputDisplayManager] InputViewerData at index {i} is missing a nameTag.", this);
                hasError = true;
            }

            // Check if the InputActionReference is assigned.
            if (data.inputActionReference == null)
            {
                Debug.LogError($"[InputDisplayManager] InputViewerData '{data.nameTag}' is missing an InputActionReference.", this);
                hasError = true;
            }

            // Check if the inputIcon (UI image) is assigned.
            if (data.inputIcon == null)
            {
                Debug.LogError($"[InputDisplayManager] InputViewerData '{data.nameTag}' is missing an InputIcon Image reference.", this);
                hasError = true;
            }
        }

        // Validate each InputMultipleViewsData entry.
        for (int i = 0; i < inputMultipleViewsData.Count; i++)
        {
            var data = inputMultipleViewsData[i];

            // Check if the name tag is assigned.
            if (string.IsNullOrWhiteSpace(data.nameTag))
            {
                Debug.LogError($"[InputDisplayManager] InputMultipleViewsData at index {i} is missing a nameTag.", this);
                hasError = true;
            }

            // Check if the InputActionReference is assigned.
            if (data.inputActionReference == null)
            {
                Debug.LogError($"[InputDisplayManager] InputMultipleViewsData '{data.nameTag}' is missing an InputActionReference.", this);
                hasError = true;
            }

            // Check if all directional icons are assigned.
            if (data.inputIconUp == null || data.inputIconDown == null || data.inputIconLeft == null || data.inputIconRight == null)
            {
                Debug.LogError($"[InputDisplayManager] InputMultipleViewsData '{data.nameTag}' is missing one or more direction icons.", this);
                hasError = true;
            }
        }

        // If no errors were found, log a success message.
        if (!hasError)
        {
            Debug.Log("[InputDisplayManager] Initialization check passed.", this);
        }
    }

    #endregion

    #region === Initialization Helpers ===

    /// <summary>
    /// Initializes the input event bindings and icon states for single input viewers.
    /// Sets up event handlers to respond to input press and release.
    /// </summary>
    private void InitializeInputViewerData()
    {
        for (int i = 0; i < inputViewerData.Count; i++)
        {
            var data = inputViewerData[i];

            // Skip if the referenced InputAction is not assigned.
            if (data.inputActionReference.action == null) continue;

            data.inputEvent?.UnbindAll(); // Unbind any existing events to avoid duplicates.

            // Create a new event handler for the input action.
            // It only activates if the viewer is enabled.
            data.inputEvent = OnInputSystemEvent<float>.WithAction(data.inputActionReference.action, () => data.enable)
                // When the input is pressed, start the color transition to the activated color.
                .OnPressed(_ => StartColorTransition(data.inputIcon, activatedColor, transitionTime))
                // When the input is released, start the color transition to the disabled color.
                .OnReleased(() => StartColorTransition(data.inputIcon, disabledColor, transitionTime));

            // Set the initial icon color and active state based on enable flag.
            if (data.enable)
            {
                SetInitialColor(data.inputIcon);
            }
            else
            {
                SetTransparent(data.inputIcon);
            }

            inputViewerData[i] = data; // Store updated struct back to the list.
        }
    }

    /// <summary>
    /// Initializes the input event bindings and icon states for directional input viewers.
    /// Sets up handlers for pressed, held, and released states with proper direction icon activation.
    /// </summary>
    private void InitializeInputMultipleViewsData()
    {
        for (int i = 0; i < inputMultipleViewsData.Count; i++)
        {
            var data = inputMultipleViewsData[i];

            // Skip if the referenced InputAction is not assigned.
            if (data.inputActionReference.action == null) continue;

            data.inputEvent?.UnbindAll(); // Unbind existing events to prevent duplicates.

            // Create new event handler for Vector2 input (e.g., joystick or D-pad).
            // Activates only if the viewer is enabled.
            data.inputEvent = OnInputSystemEvent<Vector2>.WithAction(data.inputActionReference.action, () => data.enable)
                .OnPressed(_ =>
                {
                    // On press, if control type is Gamepad, highlight the "up" icon.
                    if (controlType == ControlType.Gamepad && data.inputIconUp != null)
                    {
                        StartColorTransition(data.inputIconUp, activatedColor, transitionTime);
                    }
                })
                .OnHold(direction =>
                {
                    // While holding, if control type is Keyboard, set directional icons active based on input vector.
                    if (controlType == ControlType.Keyboard)
                    {
                        SetDirectionActive(data.inputIconUp, direction.y > 0.5f);
                        SetDirectionActive(data.inputIconDown, direction.y < -0.5f);
                        SetDirectionActive(data.inputIconRight, direction.x > 0.5f);
                        SetDirectionActive(data.inputIconLeft, direction.x < -0.5f);
                    }
                })
                .OnReleased(() =>
                {
                    // On release, fade icons back to disabled color or deactivate accordingly.
                    if (data.inputIconUp != null) StartColorTransition(data.inputIconUp, disabledColor, transitionTime);

                    if (controlType == ControlType.Keyboard)
                    {
                        if (data.inputIconDown != null) StartColorTransition(data.inputIconDown, disabledColor, transitionTime);
                        if (data.inputIconRight != null) StartColorTransition(data.inputIconRight, disabledColor, transitionTime);
                        if (data.inputIconLeft != null) StartColorTransition(data.inputIconLeft, disabledColor, transitionTime);
                    }
                });

            // Set initial icon colors or hide icons based on enable flag.
            if (data.enable)
            {
                SetInitialColor(data.inputIconUp);
                SetInitialColor(data.inputIconDown);
                SetInitialColor(data.inputIconLeft);
                SetInitialColor(data.inputIconRight);
            }
            else
            {
                SetTransparent(data.inputIconUp);
                SetTransparent(data.inputIconDown);
                SetTransparent(data.inputIconLeft);
                SetTransparent(data.inputIconRight);
            }

            inputMultipleViewsData[i] = data; // Store updated struct back to the list.
        }
    }

    #endregion

    #region === Input Detection & Control Type ===

    /// <summary>
    /// Detects the current control type by checking if any gamepads are connected.
    /// Defaults to Gamepad if any are found; otherwise, Keyboard.
    /// </summary>
    private void DetectControlType() => controlType = Gamepad.all.Count > 0 ? ControlType.Gamepad : ControlType.Keyboard;

    /// <summary>
    /// Event handler called when an input device changes (connected, disconnected, etc.).
    /// If automatic detection is enabled, updates the control type accordingly.
    /// </summary>
    /// <param name="device">The input device that changed.</param>
    /// <param name="change">The type of change.</param>
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (!automatic) return;

        if (device is Gamepad)
        {
            // Determine new control type based on connected gamepads.
            ControlType newControlType = Gamepad.all.Count > 0 ? ControlType.Gamepad : ControlType.Keyboard;

            // Update control type and refresh icons if changed.
            if (newControlType != controlType)
            {
                controlType = newControlType;
                UpdateIcons();
            }
        }
    }

    #endregion

    #region === Icon Update ===

    /// <summary>
    /// Updates all input icons for both single and multiple views based on the current control type.
    /// Ensures the displayed sprites correspond to the correct bindings (keyboard or gamepad).
    /// </summary>
    public void UpdateIcons()
    {
        // Lazily initialize extension data if null.
        if (extensionData == null) extensionData = InputSystemExtensionHelper.GetInputSystemExtensionData();

        UpdateInputViewerIcons();       // Update icons for single input viewers.
        UpdateInputMultipleViewsIcons(); // Update icons for directional inputs.
    }

    /// <summary>
    /// Updates the sprites of single input icons based on the current control type and binding IDs.
    /// </summary>
    private void UpdateInputViewerIcons()
    {
        for (int i = 0; i < inputViewerData.Count; i++)
        {
            var data = inputViewerData[i];

            // Skip if action or icon is not assigned.
            if (data.inputActionReference.action == null || data.inputIcon == null) continue;

            // Select the correct binding ID depending on control type.
            string bindingId = controlType == ControlType.Keyboard ? data.keyboardId : data.gamepadId;

            // Retrieve the sprite corresponding to the binding.
            Sprite sprite = GetSpriteForBinding(data.inputActionReference.action, bindingId);
            if (sprite != null)
            {
                data.inputIcon.sprite = sprite;
            }

            inputViewerData[i] = data;
        }
    }

    /// <summary>
    /// Updates the directional input icons’ sprites and their visibility based on the current control type.
    /// Handles keyboard direction keys or gamepad buttons/sticks.
    /// </summary>
    private void UpdateInputMultipleViewsIcons()
    {
        for (int i = 0; i < inputMultipleViewsData.Count; i++)
        {
            var data = inputMultipleViewsData[i];
            if (data.inputActionReference.action == null) continue;

            // Select the correct binding ID depending on control type.
            string bindingId = controlType == ControlType.Keyboard ? data.keyboardId : data.gamepadId;
            if (string.IsNullOrEmpty(bindingId)) continue;

            var action = data.inputActionReference.action;

            // Find the binding index by matching the binding ID.
            var bindingIndex = action.bindings.ToList().FindIndex(b => b.id.ToString() == bindingId);
            if (bindingIndex < 0) continue;

            var binding = action.bindings[bindingIndex];
            string path = binding.effectivePath;

            if (controlType == ControlType.Keyboard)
            {
                // For keyboard, find composite bindings representing each direction.
                var directionBindings = action.bindings.Where(b => b.isPartOfComposite && b.action == action.name).ToList();

                foreach (var b in directionBindings)
                {
                    // Extract key name, removing the device prefix.
                    string key = b.effectivePath.Replace("<Keyboard>/", "").ToUpperInvariant();
                    Sprite dirSprite = null;

                    // Try to parse the key name to a KeyCode enum.
                    if (Enum.TryParse(key, out KeyCode keyCode))
                    {
                        // Lookup sprite from extension data matching the KeyCode.
                        dirSprite = extensionData.KeyCodes.Find(k => k.keyCode == keyCode).sprite;
                    }

                    // If not found by KeyCode, try fallback by name.
                    if (dirSprite == null && !string.IsNullOrEmpty(b.name))
                    {
                        var fallback = extensionData.KeyCodes.FirstOrDefault(k => k.keyCode.ToString().Equals(b.name, StringComparison.OrdinalIgnoreCase));
                        dirSprite = fallback.sprite;
                    }

                    if (dirSprite == null) dirSprite = extensionData.defaultSprite; // Use default sprite if none found.

                    // Assign the sprite to the corresponding directional icon.
                    switch (b.name.ToLowerInvariant())
                    {
                        case "up":
                            if (data.inputIconUp != null) data.inputIconUp.sprite = dirSprite;
                            break;
                        case "down":
                            if (data.inputIconDown != null) data.inputIconDown.sprite = dirSprite;
                            break;
                        case "left":
                            if (data.inputIconLeft != null) data.inputIconLeft.sprite = dirSprite;
                            break;
                        case "right":
                            if (data.inputIconRight != null) data.inputIconRight.sprite = dirSprite;
                            break;
                    }
                }

                SetIconsActive(data, up: true, down: true, left: true, right: true); // Set all directional icons active for keyboard.
            }
            else
            {
                string cleanPath = path.Split('/').Last(); // For gamepad, get clean path name (e.g., buttonSouth).

                // Detect if the current gamepad is a PS4 controller.
                bool isPS4 = false;
                if (Gamepad.current != null)
                {
                    string product = Gamepad.current.description.product?.ToLowerInvariant();
                    string manufacturer = Gamepad.current.description.manufacturer?.ToLowerInvariant();

                    isPS4 = product?.Contains("wireless controller") == true || manufacturer?.Contains("sony") == true;
                }

                // Retrieve the correct sprite from PS4 or Xbox mappings.
                Sprite sprite = isPS4 ? extensionData.ps4.GetSprite(cleanPath) : extensionData.xbox.GetSprite(cleanPath);
                if (sprite == null) sprite = extensionData.defaultSprite;

                // Assign sprite to the "up" directional icon.
                if (data.inputIconUp != null) data.inputIconUp.sprite = sprite;

                // Activate only the "up" icon for gamepad, hide others.
                SetIconsActive(data, up: true, down: false, left: false, right: false);
            }

            inputMultipleViewsData[i] = data;
        }
    }

    /// <summary>
    /// Retrieves the appropriate sprite for a given InputAction and binding ID.
    /// Selects the sprite based on current control type (keyboard or gamepad).
    /// </summary>
    /// <param name="action">The InputAction to query.</param>
    /// <param name="bindingId">The binding identifier string.</param>
    /// <returns>The sprite representing the input binding, or null if none found.</returns>
    private Sprite GetSpriteForBinding(InputAction action, string bindingId)
    {
        if (string.IsNullOrEmpty(bindingId)) return null;

        // Find the binding index by matching the binding ID.
        var bindingIndex = action.bindings.ToList().FindIndex(b => b.id.ToString() == bindingId);
        if (bindingIndex < 0) return null;

        var binding = action.bindings[bindingIndex];
        string path = binding.effectivePath;

        if (controlType == ControlType.Keyboard)
        {
            // If the binding is from keyboard, parse the key name.
            if (path.StartsWith("<Keyboard>/", StringComparison.OrdinalIgnoreCase))
            {
                string keyName = path.Replace("<Keyboard>/", "").ToLowerInvariant();

                // Capitalize first letter to match KeyCode enum naming.
                if (Enum.TryParse(typeof(KeyCode), UpperFirst(keyName), out var result))
                {
                    KeyCode key = (KeyCode)result;

                    // Return the matching sprite or default.
                    return extensionData.KeyCodes.Find(k => k.keyCode == key).sprite != null ? extensionData.KeyCodes.Find(k => k.keyCode == key).sprite : extensionData.defaultSprite;
                }
            }

            return extensionData.defaultSprite; // Fallback to default sprite if key parsing failed.
        }
        else
        {
            string cleanPath = path.Split('/').Last(); // For gamepad, get clean path name (e.g., buttonSouth).

            // Detect if the current gamepad is a PS4 controller.
            bool isPS4 = false;
            if (Gamepad.current != null)
            {
                string product = Gamepad.current.description.product?.ToLowerInvariant();
                string manufacturer = Gamepad.current.description.manufacturer?.ToLowerInvariant();

                isPS4 = product?.Contains("wireless controller") == true || manufacturer?.Contains("sony") == true;
            }

            // Return sprite from PS4 or Xbox mappings or default.
            return isPS4 ? extensionData.ps4.GetSprite(cleanPath) : extensionData.xbox.GetSprite(cleanPath);
        }
    }

    /// <summary>
    /// Converts the first character of the input string to uppercase.
    /// Used to format strings for enum parsing.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>String with first character uppercased.</returns>
    private static string UpperFirst(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        if (input.Length == 1) return input.ToUpperInvariant();
        return char.ToUpperInvariant(input[0]) + input[1..];
    }

    /// <summary>
    /// Activates or deactivates the directional icons based on the boolean flags.
    /// </summary>
    /// <param name="data">The InputMultipleViewsData containing the icons.</param>
    /// <param name="up">Whether to activate the up icon.</param>
    /// <param name="down">Whether to activate the down icon.</param>
    /// <param name="left">Whether to activate the left icon.</param>
    /// <param name="right">Whether to activate the right icon.</param>
    private void SetIconsActive(InputMultipleViewsData data, bool up, bool down, bool left, bool right)
    {
        if (data.inputIconUp != null) data.inputIconUp.gameObject.SetActive(up);
        if (data.inputIconDown != null) data.inputIconDown.gameObject.SetActive(down);
        if (data.inputIconLeft != null) data.inputIconLeft.gameObject.SetActive(left);
        if (data.inputIconRight != null) data.inputIconRight.gameObject.SetActive(right);
    }

    #endregion

    #region === View Control ===

    /// <summary>
    /// Enables or disables a specific input viewer identified by its nameTag.
    /// Updates the icon's visibility and color accordingly.
    /// </summary>
    /// <param name="nameTag">The unique identifier for the viewer to update.</param>
    /// <param name="enable">True to enable; false to disable.</param>
    public void EnableAndDisableViewer(string nameTag, bool enable)
    {
        // Search and update in single input viewer data.
        for (int i = 0; i < inputViewerData.Count; i++)
        {
            if (inputViewerData[i].nameTag == nameTag)
            {
                var data = inputViewerData[i];
                data.enable = enable;
                inputViewerData[i] = data;

                HandleIconEnableDisable(data.inputIcon, enable);

                return; // Exit once found and updated.
            }
        }

        // Search and update in multiple views input data.
        for (int i = 0; i < inputMultipleViewsData.Count; i++)
        {
            if (inputMultipleViewsData[i].nameTag == nameTag)
            {
                var data = inputMultipleViewsData[i];
                data.enable = enable;
                inputMultipleViewsData[i] = data;

                if (controlType == ControlType.Gamepad)
                {
                    // For gamepad, only show the "up" icon and hide others.
                    HandleIconEnableDisable(data.inputIconUp, enable);

                    if (data.inputIconDown != null) data.inputIconDown.gameObject.SetActive(false);
                    if (data.inputIconLeft != null) data.inputIconLeft.gameObject.SetActive(false);
                    if (data.inputIconRight != null) data.inputIconRight.gameObject.SetActive(false);
                }
                else
                {
                    // For keyboard, handle all directional icons.
                    var icons = new[] { data.inputIconUp, data.inputIconDown, data.inputIconLeft, data.inputIconRight };

                    foreach (var icon in icons)
                    {
                        HandleIconEnableDisable(icon, enable);
                    }
                }

                return; // Exit after updating.
            }
        }

        Debug.LogError($"Viewer with nameTag '{nameTag}' not found in either inputViewerData or inputMultipleViewsData.", this);
    }

    /// <summary>
    /// Helper method to enable or disable an icon's GameObject and handle its color transition.
    /// </summary>
    /// <param name="image">The UI Image component of the icon.</param>
    /// <param name="enable">True to enable; false to disable.</param>
    private void HandleIconEnableDisable(Image image, bool enable)
    {
        if (image == null) return;

        if (enable)
        {
            image.gameObject.SetActive(true);

            // Start transition to activated color.
            StartColorTransition(image, disabledColor, hideTransitionTime);
        }
        else
        {
            // Start fading out and deactivate after transition.
            StartCoroutine(FadeOutAndDeactivate(image, hideTransitionTime));
        }
    }

    /// <summary>
    /// Coroutine that fades out an icon's color to transparent over a duration and then deactivates it.
    /// </summary>
    /// <param name="image">The UI Image to fade out.</param>
    /// <param name="duration">The duration of the fade-out animation.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator FadeOutAndDeactivate(Image image, float duration)
    {
        if (image == null) yield break;

        Color startColor = image.color;
        Color targetColor = new(startColor.r, startColor.g, startColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }

        image.color = targetColor;
        image.gameObject.SetActive(false);
    }

    #endregion

    #region === Icon Color Helpers ===

    /// <summary>
    /// Starts a smooth color transition on the given Image to the target color over the specified duration.
    /// Cancels any existing transition on the same Image to avoid conflicts.
    /// </summary>
    /// <param name="image">The UI Image to animate.</param>
    /// <param name="targetColor">The target color to transition to.</param>
    /// <param name="duration">The duration of the color transition.</param>
    private void StartColorTransition(Image image, Color targetColor, float duration)
    {
        if (image == null) return;

        // If there's already a transition running for this image, stop it first.
        if (colorTransitions.ContainsKey(image))
        {
            StopCoroutine(colorTransitions[image]);
            colorTransitions.Remove(image);
        }

        // Start the color transition coroutine and track it.
        Coroutine coroutine = StartCoroutine(ColorTransitionCoroutine(image, targetColor, duration));
        colorTransitions[image] = coroutine;
    }

    /// <summary>
    /// Coroutine that gradually changes an Image's color from its current color to a target color over time.
    /// </summary>
    /// <param name="image">The UI Image to animate.</param>
    /// <param name="targetColor">The final color to reach.</param>
    /// <param name="duration">The duration over which the transition happens.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator ColorTransitionCoroutine(Image image, Color targetColor, float duration)
    {
        Color startColor = image.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }

        image.color = targetColor;

        // Remove the coroutine from the tracking dictionary once done.
        colorTransitions.Remove(image);
    }

    /// <summary>
    /// Sets the initial color and activates the GameObject of the given Image.
    /// Typically used to prepare the icon for visible state.
    /// </summary>
    /// <param name="image">The Image component to set.</param>
    private void SetInitialColor(Image image)
    {
        if (image == null) return;

        image.color = disabledColor;
        image.gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets the given Image transparent and deactivates its GameObject.
    /// Typically used to hide the icon without abrupt disappearance.
    /// </summary>
    /// <param name="image">The Image component to set transparent.</param>
    private void SetTransparent(Image image)
    {
        if (image == null) return;

        // Set the color to activatedColor but fully transparent.
        image.color = new Color(activatedColor.r, activatedColor.g, activatedColor.b, 0f);
        image.gameObject.SetActive(false);
    }

    /// <summary>
    /// Activates or deactivates a directional icon by starting a color transition.
    /// Uses activatedColor if active is true; otherwise disabledColor.
    /// </summary>
    /// <param name="image">The directional Image icon to update.</param>
    /// <param name="active">True to activate; false to deactivate.</param>
    private void SetDirectionActive(Image image, bool active)
    {
        if (image == null) return;

        if (active)
        {
            StartColorTransition(image, activatedColor, transitionTime);
        }
        else
        {
            StartColorTransition(image, disabledColor, transitionTime);
        }
    }

    /// <summary>
    /// Stops all ongoing color transition coroutines and clears the tracking dictionary.
    /// Used typically during cleanup to avoid dangling coroutines.
    /// </summary>
    private void StopAllColorTransitions()
    {
        foreach (var coroutine in colorTransitions.Values)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        colorTransitions.Clear();
    }

    #endregion

    #region === Unbind Events ===

    /// <summary>
    /// Unbinds all input events from both single input viewers and multiple views input data.
    /// Ensures that no event callbacks remain registered to prevent memory leaks or unintended behavior.
    /// </summary>
    private void UnbindAllEvents()
    {
        // Unbind events for single input viewer data.
        foreach (var data in inputViewerData)
        {
            data.inputEvent?.UnbindAll();
        }

        // Unbind events for multiple views input data.
        foreach (var data in inputMultipleViewsData)
        {
            data.inputEvent?.UnbindAll();
        }
    }

    #endregion
}