using UnityEngine.InputSystem;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace InputSystemExtension
{
    /// <summary>
    /// Stores input system configuration data used for saving and loading custom bindings.
    /// </summary>
    public class InputSystemExtensionData : ScriptableObject
    {
        [Header("Input Settings")]

        /// <summary>
        /// Reference to the main InputActionAsset that will have its bindings saved/loaded.
        /// </summary>
        public InputActionAsset defaultInputAction;

        /// <summary>
        /// PlayerPrefs key used to store the binding overrides as JSON.
        /// </summary>
        public string playerPrefsKey = "BindingsData";
    }

#if UNITY_EDITOR
    /// <summary>
    /// Adds a menu option in the Unity Editor to open the InputSystemExtensionData asset via property inspector.
    /// </summary>
    public static class InputSystemExtensionDataWindow
    {
        [MenuItem("Window/Input System Extension/Input System Extension Data")]
        public static void OpenInputSystemExtensionData()
        {
            // Attempt to load the asset using the helper.
            var settingsData = InputSystemExtensionHelper.GetInputSystemExtensionData();

            if (settingsData != null)
            {
                // Check if an inspector window is already open for the asset.
                var existingWindow = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(window => window.titleContent.text == settingsData.name);

                if (existingWindow != null)
                {
                    existingWindow.Focus();
                }
                else
                {
                    EditorUtility.OpenPropertyEditor(settingsData); // Open the asset in the default Unity inspector.
                }
            }
            else
            {
                Debug.LogError("Failed to find or load InputSystemExtensionData. Ensure that the ScriptableObject exists and is properly referenced.");
            }
        }
    }
#endif
}