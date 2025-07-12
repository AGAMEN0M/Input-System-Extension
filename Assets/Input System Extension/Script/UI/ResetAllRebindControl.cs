using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Resets all rebindable controls (RebindControlManager and RebindControlManagerTMP) to their default bindings when triggered.
/// </summary>
[AddComponentMenu("UI/Input System Extension/Reset All Rebind Control", 3)]
public class ResetAllRebindControl : MonoBehaviour
{
    [Header("UI Button")]
    [SerializeField] private Button resetAllButton; // Button that triggers the reset for all bindings.

    private List<RebindControlManager> rebindControls; // List of all RebindControlManager instances in the scene.
    private List<RebindControlManagerTMP> rebindControlsTMP; // List of all RebindControlManagerTMP instances in the scene.

    private void Start()
    {
        // Register the button click event to trigger the reset method.
        if (resetAllButton != null)
        {
            resetAllButton.onClick.AddListener(ResetAll);
        }
        else
        {
            Debug.LogWarning("ResetAllButton is not assigned.", this);
        }

        // Find all instances of both manager types in the scene and store them.
        rebindControls = new List<RebindControlManager>(FindObjectsByType<RebindControlManager>(FindObjectsSortMode.None));
        rebindControlsTMP = new List<RebindControlManagerTMP>(FindObjectsByType<RebindControlManagerTMP>(FindObjectsSortMode.None));
    }

    /// <summary>
    /// Resets all registered bindings (regular and TMP-based) to their default configuration.
    /// </summary>
    private void ResetAll()
    {
        // Reset each non-TMP rebind control manager.
        foreach (var manager in rebindControls)
        {
            if (manager != null)
            {
                manager.ResetToDefault();
            }
        }

        // Reset each TMP-based rebind control manager.
        foreach (var managerTMP in rebindControlsTMP)
        {
            if (managerTMP != null)
            {
                managerTMP.ResetToDefault();
            }
        }
    }
}