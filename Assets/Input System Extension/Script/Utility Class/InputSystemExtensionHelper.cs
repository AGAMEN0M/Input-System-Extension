/*
 * ---------------------------------------------------------------------------
 * Description: Provides access to the InputSystemExtensionData ScriptableObject loaded from the Resources folder.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;

namespace InputSystemExtension
{
    /// <summary>
    /// Provides access to the InputSystemExtensionData ScriptableObject loaded from the Resources folder.
    /// </summary>
    public static class InputSystemExtensionHelper
    {
        /// <summary>
        /// Cached reference to the loaded InputSystemExtensionData asset.
        /// </summary>
        private static InputSystemExtensionData extensionData;

        /// <summary>
        /// Loads the InputSystemExtensionData asset from the Resources folder if not already cached.
        /// </summary>
        /// <returns>The loaded InputSystemExtensionData instance, or null if not found.</returns>
        public static InputSystemExtensionData GetInputSystemExtensionData()
        {
            if (extensionData == null)
            {
                extensionData = Resources.Load<InputSystemExtensionData>("Input System Extension Data");
            }

            if (extensionData == null)
            {
                Debug.LogError("Failed to load InputSystemExtensionData from Resources. Ensure the asset exists and is named 'Input System Extension Data'.");
                return null;
            }

            return extensionData;
        }
    }
}