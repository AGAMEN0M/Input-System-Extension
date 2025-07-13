using InputSystemExtension;
using UnityEngine;

using static InputSystemExtension.InputSystemExtensionHelper;
using static UnityEngine.InputSystem.InputSystem;

/// <summary>
/// Handles replacement of text-based input binding display with corresponding gamepad icons.
/// Listens to RebindControlManager updates and swaps icon or text depending on the control bound.
/// </summary>
[AddComponentMenu("UI/Input System Extension/Gamepad Icon Rebind Handler (Legacy)", 3)]
public class GamepadIconRebindHandler : MonoBehaviour
{
    [Header("Rebind Control Group Reference")]

    [Tooltip("Parent GameObject containing one or more RebindControlManager components.")]
    [SerializeField] private GameObject rebindControlGroup;

    /// <summary>
    /// Reference to the InputSystemExtensionData ScriptableObject containing icon mappings.
    /// </summary>
    private InputSystemExtensionData extensionData;

    /// <summary>
    /// Initializes the handler and hooks up listeners for all RebindControlManager components found in the specified control group.
    /// </summary>
    private void Start()
    {
        // Load the ScriptableObject with icon data.
        if (extensionData == null) extensionData = GetInputSystemExtensionData();

        // Find all managers under the specified group.
        var managers = rebindControlGroup.GetComponentsInChildren<RebindControlManager>();

        foreach (var manager in managers)
        {
            // Subscribe to UI update events and refresh display once at start.
            manager.onUpdateBindingUI.AddListener(OnBindingUIUpdate);
            manager.RefreshBindingDisplay();
        }
    }

    /// <summary>
    /// Called by RebindControlManager to update the visual display of a binding.
    /// Replaces the text with a sprite icon if one is mapped for the control path.
    /// </summary>
    /// <param name="manager">The RebindControlManager invoking the update.</param>
    /// <param name="bindingDisplay">The text display name for the binding.</param>
    /// <param name="deviceLayoutName">The layout name of the bound input device.</param>
    /// <param name="controlPath">The control path string from the Input System binding.</param>
    public void OnBindingUIUpdate(RebindControlManager manager, string bindingDisplay, string deviceLayoutName, string controlPath)
    {
        // Skip update if required data is missing or extension asset is not available.
        if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath) || extensionData == null) return;

        // Try to get the corresponding icon for the control path.
        var icon = default(Sprite);

        if (IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
        {
            // Use PS4 icon mapping.
            icon = extensionData.ps4.GetSprite(controlPath);
        }
        else if (IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
        {
            // Use Xbox icon mapping (fallback for other generic gamepads).
            icon = extensionData.xbox.GetSprite(controlPath);
        }

        // Get references to UI elements.
        var textComponent = manager.BindingDisplayText;
        if (textComponent == null)
        {
            Debug.LogError("BindingDisplayText reference is missing.", this);
            return;
        }

        var iconImage = manager.ActionBindingIcon;
        if (iconImage == null)
        {
            Debug.LogError("ActionBindingIcon reference is missing.", this);
            return;
        }

        var textRebindComponent = manager.RebindPromptText;
        if (textRebindComponent == null)
        {
            Debug.LogError("RebindPromptText reference is missing.", this);
            return;
        }

        var iconImageRebind = manager.ActionRebindIcon;
        if (iconImageRebind == null)
        {
            Debug.LogError("ActionRebindIcon reference is missing.", this);
            return;
        }

        // If we have an icon, show it and hide text.
        if (icon != null)
        {
            iconImage.sprite = icon;
            iconImageRebind.sprite = icon;
            iconImage.gameObject.SetActive(true);
            textComponent.gameObject.SetActive(false);
            iconImageRebind.gameObject.SetActive(true);
            textRebindComponent.gameObject.SetActive(false);
        }
        else
        {
            // No icon found: fall back to text display.
            iconImage.gameObject.SetActive(false);
            textComponent.gameObject.SetActive(true);
            iconImageRebind.gameObject.SetActive(false);
            textRebindComponent.gameObject.SetActive(true);
        }
    }
}