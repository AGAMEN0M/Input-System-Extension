using UnityEngine.InputSystem;
using UnityEngine;

namespace InputSystemExtension
{
    /// <summary>
    /// Handles saving and loading input binding overrides using Unity's PlayerPrefs system.
    /// </summary>
    public static class InputBindingSaver
    {
        /// <summary>
        /// Saves the current binding overrides from the InputSystemExtensionData to PlayerPrefs.
        /// </summary>
        public static void SaveDefaultBindings()
        {
            var extensionData = InputSystemExtensionHelper.GetInputSystemExtensionData();
            if (extensionData == null)
            {
                Debug.LogError("InputSystemExtensionData is null. Make sure it exists in Resources.");
                return;
            }

            SaveBindings(extensionData.defaultInputAction, extensionData.playerPrefsKey);
        }

        /// <summary>
        /// Loads binding overrides from PlayerPrefs into the InputSystemExtensionData's InputActionAsset on startup.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void LoadDefaultBindings()
        {
            var extensionData = InputSystemExtensionHelper.GetInputSystemExtensionData();
            if (extensionData == null)
            {
                Debug.LogError("InputSystemExtensionData is null. Make sure it exists in Resources.");
                return;
            }

            LoadBindings(extensionData.defaultInputAction, extensionData.playerPrefsKey);
        }

        /// <summary>
        /// Saves the binding overrides of the given InputActionAsset into PlayerPrefs.
        /// </summary>
        /// <param name="inputActionAsset">The asset containing bindings to save.</param>
        /// <param name="playerPrefsKey">The key used to store the data in PlayerPrefs.</param>
        public static void SaveBindings(InputActionAsset inputActionAsset, string playerPrefsKey)
        {
            if (inputActionAsset == null)
            {
                Debug.LogWarning("Cannot save bindings. InputActionAsset is null.");
                return;
            }

            string json = inputActionAsset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(playerPrefsKey, json);
            PlayerPrefs.Save();

            Debug.Log($"Input bindings saved to PlayerPrefs under key '{playerPrefsKey}'.");
        }

        /// <summary>
        /// Loads the binding overrides from PlayerPrefs and applies them to the given InputActionAsset.
        /// </summary>
        /// <param name="inputActionAsset">The asset to apply overrides to.</param>
        /// <param name="playerPrefsKey">The key used to retrieve the data from PlayerPrefs.</param>
        public static void LoadBindings(InputActionAsset inputActionAsset, string playerPrefsKey)
        {
            if (inputActionAsset == null)
            {
                Debug.LogWarning("Cannot load bindings. InputActionAsset is null.");
                return;
            }

            if (!PlayerPrefs.HasKey(playerPrefsKey)) return;

            string json = PlayerPrefs.GetString(playerPrefsKey);
            inputActionAsset.LoadBindingOverridesFromJson(json);

            Debug.Log($"Input bindings loaded from PlayerPrefs using key '{playerPrefsKey}'.");
        }
    }
}