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
        #region === Fields ===

        /// <summary>
        /// Cached reference to the loaded InputSystemExtensionData asset.
        /// This allows avoiding repeated loading operations from the Resources folder.
        /// </summary>
        private static InputSystemExtensionData extensionData;

        #endregion

        #region === Public Methods ===

        /// <summary>
        /// Loads the InputSystemExtensionData asset from the Resources folder if not already cached.
        /// </summary>
        /// <returns>
        /// The loaded InputSystemExtensionData instance, or null if not found.
        /// </returns>
        public static InputSystemExtensionData GetInputSystemExtensionData()
        {
            // Check if the asset has already been loaded and cached.
            if (extensionData == null)
            {
                // Attempt to load the ScriptableObject from the "Resources" folder.
                // The file name must exactly match "Input System Extension Data" (without extension).
                extensionData = Resources.Load<InputSystemExtensionData>("Input System Extension Data");
            }

            // If the asset still could not be loaded, log an error message for debugging purposes.
            if (extensionData == null)
            {
                Debug.LogError("Failed to load InputSystemExtensionData from Resources. Ensure the asset exists and is named 'Input System Extension Data'.");
                return null;
            }

            // Return the cached reference for reuse.
            return extensionData;
        }

        #endregion
    }
}