/*
 * ---------------------------------------------------------------------------
 * Description: Handles replacement of text-based input binding display with corresponding gamepad icons.
 *              Listens to RebindControlManagerTMP updates and swaps icon or text depending on the control bound.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using InputSystemExtension;
using UnityEngine;

using static InputSystemExtension.InputSystemExtensionHelper;
using static UnityEngine.InputSystem.InputSystem;

/// <summary>
/// Handles replacement of text-based input binding display with corresponding gamepad icons.
/// Listens to RebindControlManagerTMP updates and swaps icon or text depending on the control bound.
/// </summary>
[AddComponentMenu("UI/Input System Extension/Rebind/Gamepad Icon Rebind Handler (TMP)", 4)]
public class GamepadIconRebindHandlerTMP : MonoBehaviour
{
    #region === Inspector Fields ===

    [Header("Rebind Control Group Reference")]
    [SerializeField, Tooltip("Parent GameObject containing one or more RebindControlManager components.")]
    private GameObject rebindControlGroup;

    #endregion

    #region === Private Fields ===

    /// <summary>
    /// Reference to the InputSystemExtensionData ScriptableObject containing icon mappings.
    /// </summary>
    private InputSystemExtensionData extensionData;

    #endregion

    #region === Unity Methods ===

    /// <summary>
    /// Initializes the handler and hooks up listeners for all RebindControlManagerTMP components found in the specified control group.
    /// </summary>
    private void Start()
    {
        // Load the ScriptableObject that contains the mappings for input icons.
        if (extensionData == null) extensionData = GetInputSystemExtensionData();

        // Find all RebindControlManagerTMP components under the specified parent object.
        var managers = rebindControlGroup.GetComponentsInChildren<RebindControlManagerTMP>();

        // Subscribe to each manager's update event and refresh their display on start.
        foreach (var manager in managers)
        {
            manager.onUpdateBindingUI.AddListener(OnBindingUIUpdate);
            manager.RefreshBindingDisplay();
        }
    }

    #endregion

    #region === Event Handlers ===

    /// <summary>
    /// Called by RebindControlManagerTMP to update the visual display of a binding.
    /// Replaces the text with a sprite icon if one is mapped for the control path.
    /// </summary>
    /// <param name="manager">The RebindControlManagerTMP invoking the update.</param>
    /// <param name="bindingDisplay">The text display name for the binding.</param>
    /// <param name="deviceLayoutName">The layout name of the bound input device.</param>
    /// <param name="controlPath">The control path string from the Input System binding.</param>
    public void OnBindingUIUpdate(RebindControlManagerTMP manager, string bindingDisplay, string deviceLayoutName, string controlPath)
    {
        // Validate input and ensure extension data is loaded.
        if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath) || extensionData == null) return;

        // Initialize the icon variable.
        var icon = default(Sprite);

        // Check the type of controller and use the corresponding icon mapping.
        if (IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
        {
            // Use PS4 icon mapping.
            icon = extensionData.ps4.GetSprite(controlPath);
        }
        else if (IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
        {
            // Use Xbox icon mapping (default for most generic controllers).
            icon = extensionData.xbox.GetSprite(controlPath);
        }

        // Retrieve all UI references from the manager.
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

        // Determine whether to show icons or text.
        if (icon != null)
        {
            // Icon found: show icons and hide text.
            iconImage.sprite = icon;
            iconImageRebind.sprite = icon;

            iconImage.gameObject.SetActive(true);
            iconImageRebind.gameObject.SetActive(true);

            textComponent.gameObject.SetActive(false);
            textRebindComponent.gameObject.SetActive(false);
        }
        else
        {
            // No icon found: show text instead.
            iconImage.gameObject.SetActive(false);
            iconImageRebind.gameObject.SetActive(false);

            textComponent.gameObject.SetActive(true);
            textRebindComponent.gameObject.SetActive(true);
        }
    }

    #endregion
}