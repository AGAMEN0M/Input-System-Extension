/*
 * ---------------------------------------------------------------------------
 * Description: UI utility component that resets all RebindControlManager and 
 *              RebindControlManagerTMP components under a target group to 
 *              their default input bindings when a button is clicked.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// This component allows resetting all rebindable controls (RebindControlManager and 
/// RebindControlManagerTMP) within a specified group to their default bindings when a 
/// designated button is clicked.
/// </summary>
[AddComponentMenu("UI/Input System Extension/Rebind/Reset All Rebind Control", 5)]
public class ResetAllRebindControl : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("UI Button")]
    [SerializeField, Tooltip("Button that triggers the reset operation for all rebind controls when clicked.")]
    private Button resetAllButton; // Button used to trigger the reset of all rebind controls.

    [Space(10)]

    [SerializeField, Tooltip("Parent GameObject that contains all RebindControlManager and RebindControlManagerTMP components to be reset.")]
    private GameObject rebindControlGroup; // GameObject that holds all rebind control components.

    #endregion

    #region === Private Fields ===

    private List<RebindControlManager> rebindControls; // List of RebindControlManager components found within the group.
    private List<RebindControlManagerTMP> rebindControlsTMP; // List of RebindControlManagerTMP components found within the group.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the button responsible for triggering the reset action.
    /// </summary>
    public Button ResetAllButton
    {
        get => resetAllButton;
        set => resetAllButton = value;
    }

    /// <summary>
    /// Gets or sets the GameObject that holds all rebind control components.
    /// </summary>
    public GameObject RebindControlGroup
    {
        get => rebindControlGroup;
        set => rebindControlGroup = value;
    }

    #endregion

    #region === Unity Events ===

    private void Start()
    {
        // Check if the reset button is assigned.
        if (resetAllButton != null)
        {
            // Add listener to call the ResetAll method when the button is clicked.
            resetAllButton.onClick.AddListener(ResetAll);
        }
        else
        {
            // Log a warning if the button was not assigned.
            Debug.LogWarning("ResetAllButton is not assigned.", this);
        }

        // Retrieve all RebindControlManager components within the assigned group.
        rebindControls = new List<RebindControlManager>(
            rebindControlGroup.GetComponentsInChildren<RebindControlManager>());

        // Retrieve all RebindControlManagerTMP components within the assigned group.
        rebindControlsTMP = new List<RebindControlManagerTMP>(
            rebindControlGroup.GetComponentsInChildren<RebindControlManagerTMP>());
    }

    #endregion

    #region === Reset Logic ===

    /// <summary>
    /// Resets all detected rebind control components (both standard and TMP-based) to their default input bindings.
    /// </summary>
    private void ResetAll()
    {
        // Iterate through each standard RebindControlManager and reset to default bindings.
        foreach (var manager in rebindControls)
        {
            if (manager != null)
            {
                manager.ResetToDefault(); // Reset this manager to its default binding.
            }
        }

        // Iterate through each RebindControlManagerTMP and reset to default bindings.
        foreach (var managerTMP in rebindControlsTMP)
        {
            if (managerTMP != null)
            {
                managerTMP.ResetToDefault(); // Reset this TMP-based manager to its default binding.
            }
        }
    }

    #endregion
}