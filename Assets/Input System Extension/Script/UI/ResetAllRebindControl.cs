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
/// Resets all rebindable controls (RebindControlManager and RebindControlManagerTMP)
/// to their default bindings when the assigned button is clicked.
/// </summary>
[AddComponentMenu("UI/Input System Extension/Reset All Rebind Control", 5)]
public class ResetAllRebindControl : MonoBehaviour
{
    [Header("UI Button")]
    [SerializeField] private Button resetAllButton; // Button that triggers the reset of all rebind controls when clicked.

    [Space(10)]
    [SerializeField] private GameObject rebindControlGroup; // Parent GameObject containing all rebind control components.

    private List<RebindControlManager> rebindControls; // List of RebindControlManager components found in the specified group.
    private List<RebindControlManagerTMP> rebindControlsTMP; // List of RebindControlManagerTMP components found in the specified group.

    private void Start()
    {
        // Assign the ResetAll method to the button's onClick event, if the button is assigned.
        if (resetAllButton != null)
        {
            resetAllButton.onClick.AddListener(ResetAll);
        }
        else
        {
            Debug.LogWarning("ResetAllButton is not assigned.", this);
        }

        // Retrieve all RebindControlManager and RebindControlManagerTMP components within the specified group.
        rebindControls = new List<RebindControlManager>(rebindControlGroup.GetComponentsInChildren<RebindControlManager>());
        rebindControlsTMP = new List<RebindControlManagerTMP>(rebindControlGroup.GetComponentsInChildren<RebindControlManagerTMP>());
    }

    /// <summary>
    /// Resets all detected rebind controls (standard and TMP-based) to their default input bindings.
    /// </summary>
    private void ResetAll()
    {
        // Iterate through each standard rebind control and reset it to default bindings.
        foreach (var manager in rebindControls)
        {
            if (manager != null)
            {
                manager.ResetToDefault();
            }
        }

        // Iterate through each TMP-based rebind control and reset it to default bindings.
        foreach (var managerTMP in rebindControlsTMP)
        {
            if (managerTMP != null)
            {
                managerTMP.ResetToDefault();
            }
        }
    }
}