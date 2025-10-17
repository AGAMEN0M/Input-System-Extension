/*
 * ---------------------------------------------------------------------------
 * Description: Handles saving and loading input binding overrides using Unity's PlayerPrefs system.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.InputSystem;
using UnityEngine;

namespace InputSystemExtension
{
    /// <summary>
    /// Handles saving and loading input binding overrides using Unity's PlayerPrefs system.
    /// </summary>
    public static class InputBindingSaver
    {
        #region === Default Save and Load Methods ===

        /// <summary>
        /// Saves the current binding overrides from the InputSystemExtensionData to PlayerPrefs.
        /// </summary>
        public static void SaveDefaultBindings()
        {
            // Retrieve the InputSystemExtensionData instance from the helper.
            var extensionData = InputSystemExtensionHelper.GetInputSystemExtensionData();

            // Ensure the extension data exists before proceeding.
            if (extensionData == null)
            {
                Debug.LogError("InputSystemExtensionData is null. Make sure it exists in Resources.");
                return;
            }

            // Save bindings from the default input action using the provided PlayerPrefs key.
            SaveBindings(extensionData.defaultInputAction, extensionData.playerPrefsKey);
        }

        /// <summary>
        /// Loads binding overrides from PlayerPrefs into the InputSystemExtensionData's InputActionAsset on startup.
        /// This method automatically executes when the game starts.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void LoadDefaultBindings()
        {
            // Retrieve the InputSystemExtensionData instance from the helper.
            var extensionData = InputSystemExtensionHelper.GetInputSystemExtensionData();

            // Ensure the extension data exists before attempting to load bindings.
            if (extensionData == null)
            {
                Debug.LogError("InputSystemExtensionData is null. Make sure it exists in Resources.");
                return;
            }

            // Load binding overrides from PlayerPrefs and apply them to the default input action.
            LoadBindings(extensionData.defaultInputAction, extensionData.playerPrefsKey);
        }

        #endregion

        #region === Save and Load Overrides ===

        /// <summary>
        /// Saves the binding overrides of the given InputActionAsset into PlayerPrefs.
        /// </summary>
        /// <param name="inputActionAsset">The asset containing bindings to save.</param>
        /// <param name="playerPrefsKey">The key used to store the data in PlayerPrefs.</param>
        public static void SaveBindings(InputActionAsset inputActionAsset, string playerPrefsKey)
        {
            // Prevent saving if the provided asset is null.
            if (inputActionAsset == null)
            {
                Debug.LogWarning("Cannot save bindings. InputActionAsset is null.");
                return;
            }

            // Convert the current binding overrides into a JSON string representation.
            string json = inputActionAsset.SaveBindingOverridesAsJson();

            // Store the JSON string in PlayerPrefs using the provided key.
            PlayerPrefs.SetString(playerPrefsKey, json);

            // Commit changes to disk.
            PlayerPrefs.Save();

            // Log a confirmation message to the console.
            Debug.Log($"Input bindings saved to PlayerPrefs under key '{playerPrefsKey}'.");
        }

        /// <summary>
        /// Loads the binding overrides from PlayerPrefs and applies them to the given InputActionAsset.
        /// </summary>
        /// <param name="inputActionAsset">The asset to apply overrides to.</param>
        /// <param name="playerPrefsKey">The key used to retrieve the data from PlayerPrefs.</param>
        public static void LoadBindings(InputActionAsset inputActionAsset, string playerPrefsKey)
        {
            // Prevent loading if the provided asset is null.
            if (inputActionAsset == null)
            {
                Debug.LogWarning("Cannot load bindings. InputActionAsset is null.");
                return;
            }

            // Skip loading if there is no saved data for the specified key.
            if (!PlayerPrefs.HasKey(playerPrefsKey)) return;

            // Retrieve the stored JSON string containing binding overrides.
            string json = PlayerPrefs.GetString(playerPrefsKey);

            // Apply the loaded overrides to the given InputActionAsset.
            inputActionAsset.LoadBindingOverridesFromJson(json);

            // Log a confirmation message to the console.
            Debug.Log($"Input bindings loaded from PlayerPrefs using key '{playerPrefsKey}'.");
        }

        #endregion
    }
}