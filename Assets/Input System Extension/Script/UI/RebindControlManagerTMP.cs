/*
 * ---------------------------------------------------------------------------
 * Description: Component that manages interactive input rebinding using 
 *              TextMeshPro UI elements. Supports real-time updates, composite 
 *              bindings, icon representation, duplicate prevention, and 
 *              cancel input handling.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using InputSystemExtension;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

[AddComponentMenu("UI/Input System Extension/Rebind/Rebind Control Manager (TMP)", 2)]
public class RebindControlManagerTMP : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("Input Settings")]
    [SerializeField, GetAction, Tooltip("Reference to an Input Action that cancels the current rebind process when performed.")]
    private InputActionReference cancelRebindActionReference; // Action used to cancel rebinding.

    [SerializeField, GetAction, Tooltip("Reference to the Input Action whose binding will be modified by this manager.")]
    private InputActionReference inputActionReference; // Action being rebinding target.

    [SerializeField, BindingId(nameof(inputActionReference)), Tooltip("Unique identifier (GUID) of the specific binding within the action that will be rebinding.")]
    private string targetBindingId; // Binding ID to rebind.

    [SerializeField, Tooltip("Options that control how the binding display string is generated (e.g., showing device layout, path, or name).")]
    private InputBinding.DisplayStringOptions displayOptions; // Display options for binding text.

    [Header("UI Elements")]
    [SerializeField, Tooltip("Text UI element that shows the name of the action associated with this binding.")]
    private TMP_Text actionLabel; // UI label displaying action name.

    [SerializeField, Tooltip("If true, automatically updates the action label based on the assigned action's name.")]
    private bool autoLabelFromAction = true; // Auto-update label from action name.

    [Space(10)]

    [SerializeField, Tooltip("Button that, when clicked, starts the interactive rebind operation.")]
    private Button startRebindButton; // Trigger button for rebinding.

    [SerializeField, Tooltip("Text UI element showing the current binding's display string.")]
    private TMP_Text bindingDisplayText; // UI element showing binding string.

    [SerializeField, Tooltip("Image UI element used to represent the current binding visually with an icon.")]
    private Image actionBindingIcon; // Icon representing the binding's control.

    [Space(10)]

    [SerializeField, Tooltip("Overlay GameObject displayed during the rebinding process to block input and show prompt.")]
    private GameObject rebindOverlayUI; // Overlay for rebind process.

    [SerializeField, Tooltip("Text UI element shown during rebinding to guide the user (e.g., '<Press a key>').")]
    private TMP_Text rebindPromptText; // Prompt text during rebinding.

    [SerializeField, Tooltip("Image UI element representing the control being pressed during the rebind process.")]
    private Image actionRebindIcon; // Icon for the rebind input.

    [Space(10)]

    [SerializeField, Tooltip("Button that resets the current binding to its default value when clicked.")]
    private Button resetButton; // Reset binding to default.

    [Header("Events")]
    [Tooltip("Event triggered when a rebind operation starts, providing access to the manager and rebinding operation.")]
    public InteractiveRebindEventTMP onRebindStart; // Event fired when rebind starts.

    [Space(5)]

    [Tooltip("Event triggered when a rebind operation ends, either completed or canceled, providing the manager and operation.")]
    public InteractiveRebindEventTMP onRebindStop; // Event fired when rebind stops.

    [Space(5)]

    [Tooltip("Event triggered whenever the binding UI should refresh (e.g., after rebinding or resetting).")]
    public UpdateBindingUIEventTMP onUpdateBindingUI; // Event for UI refresh.

    #endregion

    #region === Private Fields ===

    private InputActionRebindingExtensions.RebindingOperation currentRebind; // Reference to the ongoing rebind operation.
    private static List<RebindControlManagerTMP> allRebindManagers; // Static list of all active rebind managers for centralized updates.
    private EventSystem currentEventSystem; // Cached reference to the active EventSystem to avoid null when EventSystem.current is unavailable.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the input action reference.
    /// Updates the label and binding display when changed.
    /// </summary>
    public InputActionReference ActionReference
    {
        get => inputActionReference;
        set
        {
            inputActionReference = value;
            UpdateActionLabel();
            RefreshBindingDisplay();
        }
    }

    /// <summary>
    /// Gets or sets the display string options for the binding.
    /// Refreshes the UI display when changed.
    /// </summary>
    public InputBinding.DisplayStringOptions DisplayOptions
    {
        get => displayOptions;
        set
        {
            displayOptions = value;
            RefreshBindingDisplay();
        }
    }

    /// <summary>
    /// Gets or sets the binding display text component.
    /// Triggers a UI refresh when changed.
    /// </summary>
    public TMP_Text BindingDisplayText
    {
        get => bindingDisplayText;
        set
        {
            bindingDisplayText = value;
            RefreshBindingDisplay();
        }
    }

    /// <summary>
    /// Gets the icon image component used to display the binding's action icon.
    /// </summary>
    public Image ActionBindingIcon => actionBindingIcon;

    /// <summary>
    /// Gets or sets the rebind prompt text component.
    /// Triggers UI refresh or update when set.
    /// </summary>
    public TMP_Text RebindPromptText
    {
        get => rebindPromptText;
        set => rebindPromptText = value;
    }

    /// <summary>
    /// Gets the icon image component used to display the rebind's action icon.
    /// </summary>
    public Image ActionRebindIcon => actionRebindIcon;

    #endregion

    #region === Unity Callbacks ===

    private void Awake()
    {
        if (!cancelRebindActionReference) Debug.LogWarning("Missing reference: Cancel Rebind Action Reference is not assigned.", this);
        if (!inputActionReference) Debug.LogWarning("Missing reference: Input Action Reference is not assigned.", this);
        if (string.IsNullOrEmpty(targetBindingId)) Debug.LogWarning("Missing binding ID: Target Binding ID is not set.", this);
        if (!actionLabel && autoLabelFromAction) Debug.LogWarning("Missing UI element: Action Label is not assigned.", this);
        if (!startRebindButton) Debug.LogWarning("Missing UI element: Start Rebind Button is not assigned.", this);
        if (!bindingDisplayText) Debug.LogWarning("Missing UI element: Binding Display Text is not assigned.", this);
        if (!actionBindingIcon) Debug.LogWarning("Missing UI element: Binding Display Image Icon is not assigned.", this);
        if (!rebindOverlayUI) Debug.LogWarning("Missing UI element: Rebind Overlay UI GameObject is not assigned.", this);
        if (!rebindPromptText) Debug.LogWarning("Missing UI element: Rebind Prompt Text is not assigned.", this);
        if (!actionRebindIcon) Debug.LogWarning("Missing UI element: Rebind Display Image Icon is not assigned.", this);
        if (!resetButton) Debug.LogWarning("Missing UI element: Reset Button is not assigned.", this);
    }

    private void Start()
    {
        // Hide the rebind overlay UI when the game starts.
        if (rebindOverlayUI != null) rebindOverlayUI.SetActive(false);

        // Register the rebind and reset button click events.
        startRebindButton.onClick.AddListener(StartInteractiveRebind);
        resetButton.onClick.AddListener(ResetToDefault);

        // Cache the current EventSystem in a variable.
        currentEventSystem = EventSystem.current;
    }

    private void OnEnable()
    {
        // Ensure the static list exists and add this instance to it.
        allRebindManagers ??= new List<RebindControlManagerTMP>();
        allRebindManagers.Add(this);

        // If this is the first manager, subscribe to global action change events.
        if (allRebindManagers.Count == 1)
        {
            UnityEngine.InputSystem.InputSystem.onActionChange += HandleActionChange;
        }
    }

    private void OnDisable()
    {
        // Dispose any ongoing rebind operation.
        currentRebind?.Dispose();
        currentRebind = null;

        // Remove this instance from the shared list.
        if (allRebindManagers != null)
        {
            allRebindManagers.Remove(this);

            // If this was the last manager, unsubscribe from input system changes.
            if (allRebindManagers.Count == 0)
            {
                allRebindManagers = null;
                UnityEngine.InputSystem.InputSystem.onActionChange -= HandleActionChange;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Return early if any required reference or ID is missing.
        if (cancelRebindActionReference == null || inputActionReference == null || string.IsNullOrEmpty(targetBindingId)) return;

        // Ensures UI is always in sync in the Editor.
        UpdateActionLabel();
        RefreshBindingDisplay();
    }
#endif

    #endregion

    #region === Core Methods ===

    /// <summary>
    /// Resolves the action and binding index from the binding ID.
    /// </summary>
    private bool TryResolveBinding(out InputAction resolvedAction, out int bindingIndex)
    {
        bindingIndex = -1;

        // Get the action reference.
        resolvedAction = inputActionReference != null ? inputActionReference.action : null;

        // Fail if action or binding ID is invalid.
        if (resolvedAction == null || string.IsNullOrEmpty(targetBindingId)) return false;

        // Try to find the index of the binding using the targetBindingId.
        var guid = new Guid(targetBindingId);
        bindingIndex = resolvedAction.bindings.IndexOf(x => x.id == guid);

        // Log an error if not found.
        if (bindingIndex == -1)
        {
            Debug.LogError($"Cannot find binding with ID '{guid}' on '{resolvedAction}'", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Updates the UI text and reset button status based on the current binding state.
    /// </summary>
    public void RefreshBindingDisplay()
    {
        string bindingDisplay = string.Empty;
        string layoutName = null;
        string controlPath = null;

        var action = inputActionReference != null ? inputActionReference.action : null;
        bool isModified = false;

        if (action != null)
        {
            // Find the root binding index using the binding ID.
            var rootIndex = action.bindings.IndexOf(x => x.id.ToString() == targetBindingId);
            if (rootIndex != -1)
            {
                // Get the display string for the binding.
                bindingDisplay = action.GetBindingDisplayString(rootIndex, out layoutName, out controlPath, displayOptions);
                var rootBinding = action.bindings[rootIndex];

                // Check if the binding or its composite parts are modified.
                if (rootBinding.isComposite)
                {
                    if (!string.IsNullOrEmpty(rootBinding.overridePath) && rootBinding.overridePath != rootBinding.path)
                    {
                        isModified = true;
                    }

                    for (int i = rootIndex + 1; i < action.bindings.Count; i++)
                    {
                        var part = action.bindings[i];
                        if (!part.isPartOfComposite) break;

                        if (!string.IsNullOrEmpty(part.overridePath) && part.overridePath != part.path)
                        {
                            isModified = true;
                            break;
                        }
                    }
                }
                else
                {
                    isModified = !string.IsNullOrEmpty(rootBinding.overridePath) && rootBinding.overridePath != rootBinding.path;
                }
            }
        }

        // Update the binding display text UI.
        if (bindingDisplayText != null)
        {
            bindingDisplayText.text = bindingDisplay;
        }

        // Enable or disable the reset button based on modification state.
        if (resetButton != null)
        {
            resetButton.interactable = isModified;
        }

        // Notify external listeners of UI update.
        onUpdateBindingUI?.Invoke(this, bindingDisplay, layoutName, controlPath);
    }

    /// <summary>
    /// Resets the current binding (and its composite parts) to the default.
    /// </summary>
    public void ResetToDefault()
    {
        // Try to resolve the target binding.
        if (!TryResolveBinding(out var action, out var bindingIndex)) return;

        // Remove the override from the root binding.
        action.RemoveBindingOverride(bindingIndex);

        // Also reset composite parts if the binding is a composite.
        if (action.bindings[bindingIndex].isComposite)
        {
            for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
            {
                action.RemoveBindingOverride(i);
            }
        }

        // Refresh UI and persist changes.
        RefreshBindingDisplay();
        InputBindingSaver.SaveDefaultBindings();
    }

    /// <summary>
    /// Initiates a rebind process for the selected binding.
    /// Starts from the first composite part if applicable.
    /// </summary>
    private void StartInteractiveRebind()
    {
        // Try to find the binding to rebind.
        if (!TryResolveBinding(out var action, out var bindingIndex)) return;

        // If it's a composite, start from the first part.
        if (action.bindings[bindingIndex].isComposite)
        {
            var nextIndex = bindingIndex + 1;
            if (nextIndex < action.bindings.Count && action.bindings[nextIndex].isPartOfComposite)
            {
                ExecuteInteractiveRebind(action, nextIndex, true);
            }
        }
        else
        {
            ExecuteInteractiveRebind(action, bindingIndex); // Otherwise, rebind the single control.
        }
    }

    /// <summary>
    /// Executes the interactive rebind operation.
    /// Handles rebind lifecycle, UI, duplicate input validation, and chaining for composites.
    /// </summary>
    private void ExecuteInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
    {
        currentRebind?.Cancel(); // Cancel any active rebind operation before starting a new one.

        // Local cleanup method to reset state after the operation ends.
        void CleanUp()
        {
            // Dispose and nullify the rebind operation.
            currentRebind?.Dispose();
            currentRebind = null;

            action.actionMap.Enable(); // Re-enable the action map to resume normal gameplay input.

            // Re-enable the EventSystem after rebind ends or is canceled.
            if (currentEventSystem != null)
            {
                currentEventSystem.gameObject.SetActive(true);
            }

            // Unsubscribe from the cancel action and disable it.
            if (cancelRebindActionReference != null && cancelRebindActionReference.action != null)
            {
                cancelRebindActionReference.action.performed -= OnCancelInputPerformed;
                cancelRebindActionReference.action.Disable();
            }
        }

        action.actionMap.Disable(); // Disable the action map to prevent other inputs during rebinding.

        // Disable the EventSystem to block UI interaction during rebind.
        if (currentEventSystem != null)
        {
            currentEventSystem.gameObject.SetActive(false);
        }

        // Begin the rebind operation for the specified binding index.
        currentRebind = action.PerformInteractiveRebinding(bindingIndex)
            .OnCancel(operation =>
            {
                // Trigger event and hide overlay on cancel.
                onRebindStop?.Invoke(this, operation);
                if (rebindOverlayUI != null) rebindOverlayUI.SetActive(false);
                RefreshBindingDisplay();
                CleanUp();
            })
            .OnComplete(operation =>
            {
                // Hide overlay and notify listeners when rebind completes.
                if (rebindOverlayUI != null) rebindOverlayUI.SetActive(false);
                onRebindStop?.Invoke(this, operation);

                // If the binding is a duplicate, remove it and retry.
                if (CheckDuplicateBindings(action, bindingIndex))
                {
                    action.RemoveBindingOverride(bindingIndex);
                    CleanUp();
                    ExecuteInteractiveRebind(action, bindingIndex, allCompositeParts);
                    return;
                }

                // Refresh UI and persist binding changes.
                RefreshBindingDisplay();
                CleanUp();
                InputBindingSaver.SaveDefaultBindings();

                // Return focus to the rebind button for controller navigation.
                if (startRebindButton != null && currentEventSystem != null)
                {
                    currentEventSystem.gameObject.SetActive(true);
                    currentEventSystem.SetSelectedGameObject(startRebindButton.gameObject);
                }

                // If this is part of a composite, continue rebinding next part.
                if (allCompositeParts)
                {
                    var nextIndex = bindingIndex + 1;
                    if (nextIndex < action.bindings.Count && action.bindings[nextIndex].isPartOfComposite)
                    {
                        ExecuteInteractiveRebind(action, nextIndex, true);
                    }
                }
            })
            .OnPotentialMatch(operation =>
            {
                // Validate the selected control before completing.
                var candidateControl = operation.selectedControl;
                if (candidateControl == null) return;

                string inputPath = candidateControl.path;
                var currentBindingId = action.bindings[bindingIndex].id;

                // Try to locate the asset that contains this binding.
                InputActionAsset asset = null;
                if (inputActionReference != null && inputActionReference.action != null && inputActionReference.action.actionMap != null)
                {
                    asset = inputActionReference.action.actionMap.asset;
                }

                if (asset == null) return;

                // Check for duplicate usage of this input path in the asset.
                foreach (var map in asset.actionMaps)
                {
                    foreach (var act in map.actions)
                    {
                        foreach (var binding in act.bindings)
                        {
                            if (binding.id == currentBindingId) continue;

                            var effectivePath = string.IsNullOrEmpty(binding.overridePath) ? binding.effectivePath : binding.overridePath;
                            if (effectivePath == inputPath)
                            {
                                Debug.LogWarning($"Input '{candidateControl.displayName}' is already used by '{act.name}' in '{map.name}'.");
                                operation.Cancel(); // Abort the rebind if duplicate is found.
                                return;
                            }
                        }
                    }
                }
            });

        // Show the overlay UI for rebinding.
        if (rebindOverlayUI != null)
        {
            rebindOverlayUI.SetActive(true);
        }

        // Set the prompt text to guide the user.
        if (rebindPromptText != null)
        {
            rebindPromptText.text = GenerateCustomPrompt(action, bindingIndex, allCompositeParts);
        }

        // Notify that the rebind has started and begin monitoring cancel input.
        onRebindStart?.Invoke(this, currentRebind);
        currentRebind.Start();
        MonitorCancelInput();
    }

    /// <summary>
    /// Checks if the new input path is already assigned to a different binding.
    /// Prevents duplicate control bindings in the input system.
    /// </summary>
    private bool CheckDuplicateBindings(InputAction action, int bindingIndex)
    {
        // Get the binding being checked.
        var newBinding = action.bindings[bindingIndex];

        // Retrieve the action asset.
        var asset = action.actionMap?.asset;
        if (asset == null) return false;

        // Determine the effective input path of the new binding.
        string newPath = string.IsNullOrEmpty(newBinding.overridePath) ? newBinding.effectivePath : newBinding.overridePath;

        // Search through all bindings in the asset.
        foreach (var map in asset.actionMaps)
        {
            foreach (var act in map.actions)
            {
                foreach (var binding in act.bindings)
                {
                    // Skip the current binding.
                    if (map == action.actionMap && act == action && binding.id == newBinding.id) continue;

                    string path = string.IsNullOrEmpty(binding.overridePath) ? binding.effectivePath : binding.overridePath;

                    // Compare paths to detect duplicates.
                    if (path == newPath)
                    {
                        Debug.LogWarning($"Duplicate binding detected: '{newPath}' is already used by action '{act.name}' in map '{map.name}'.");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Generates a visual prompt for the binding, including formatting for composite parts.
    /// </summary>
    private string GenerateCustomPrompt(InputAction action, int bindingIndex, bool allCompositeParts)
    {
        if (!allCompositeParts)
        {
            // Single binding prompt.
            string display = action.GetBindingDisplayString(bindingIndex);
            if (string.IsNullOrEmpty(display)) display = "...";

            return $"<{display}>";
        }

        List<string> compositeParts = new(); // Composite binding: construct a prompt string showing all parts.

        // Find the root of the composite.
        int rootIndex = bindingIndex;
        while (rootIndex > 0 && action.bindings[rootIndex].isPartOfComposite)
        {
            rootIndex--;
        }

        // Collect each composite part name.
        for (int i = rootIndex + 1; i < action.bindings.Count; i++)
        {
            var part = action.bindings[i];
            if (!part.isPartOfComposite) break;

            string display = action.GetBindingDisplayString(i);
            if (string.IsNullOrEmpty(display)) display = "...";

            // Highlight the current part being rebound.
            if (i == bindingIndex)
            {
                compositeParts.Add($"<{display}>");
            }
            else
            {
                compositeParts.Add(display);
            }
        }

        return string.Join("/", compositeParts); // Combine the parts with slashes.
    }

    /// <summary>
    /// Enables listening to a cancel input action to stop the rebind.
    /// </summary>
    private void MonitorCancelInput()
    {
        // Skip if no cancel input action is defined.
        if (cancelRebindActionReference == null || cancelRebindActionReference.action == null) return;

        // Enable and subscribe to the cancel input action.
        cancelRebindActionReference.action.Enable();
        cancelRebindActionReference.action.performed += OnCancelInputPerformed;
    }

    /// <summary>
    /// Called when the cancel input action is performed during rebind.
    /// </summary>
    private void OnCancelInputPerformed(InputAction.CallbackContext context)
    {
        currentRebind?.Cancel(); // Cancel the current rebind operation if active.
    }

    /// <summary>
    /// Updates all managers if input system actions are changed at runtime.
    /// </summary>
    private static void HandleActionChange(object context, InputActionChange change)
    {
        // Only respond to binding changes.
        if (change != InputActionChange.BoundControlsChanged) return;

        // Determine which action, map, or asset changed.
        var changedAction = context as InputAction;
        var changedMap = changedAction?.actionMap ?? context as InputActionMap;

        InputActionAsset changedAsset = null;
        if (changedMap != null && changedMap.asset != null)
        {
            changedAsset = changedMap.asset;
        }
        else if (context is InputActionAsset asset)
        {
            changedAsset = asset;
        }

        // Notify managers whose references are affected by the change.
        foreach (var manager in allRebindManagers)
        {
            var referencedAction = manager.ActionReference != null ? manager.ActionReference.action : null;
            if (referencedAction == null) continue;

            if (referencedAction == changedAction || referencedAction.actionMap == changedMap || referencedAction.actionMap?.asset == changedAsset)
            {
                manager.RefreshBindingDisplay();
            }
        }
    }

    /// <summary>
    /// Automatically updates the label with the action name if enabled.
    /// </summary>
    private void UpdateActionLabel()
    {
        // Update the action label UI with the action name, if enabled.
        if (autoLabelFromAction && actionLabel != null)
        {
            var action = inputActionReference != null ? inputActionReference.action : null;
            actionLabel.text = action != null ? action.name : string.Empty;
        }
    }

    #endregion

    #region === Nested Classes / Events ===

    /// <summary>
    /// Event called when the binding UI should update.
    /// </summary>
    [Serializable]
    public class UpdateBindingUIEventTMP : UnityEvent<RebindControlManagerTMP, string, string, string> { }

    /// <summary>
    /// Event triggered when a rebind operation starts or ends.
    /// </summary>
    [Serializable]
    public class InteractiveRebindEventTMP : UnityEvent<RebindControlManagerTMP, InputActionRebindingExtensions.RebindingOperation> { }

    #endregion
}