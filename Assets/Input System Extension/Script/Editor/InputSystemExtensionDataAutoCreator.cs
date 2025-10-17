/*
 * ---------------------------------------------------------------------------
 * Description: Automatically creates the InputSystemExtensionData asset at 
 *              editor startup or via menu, ensuring required configuration 
 *              for input system extension is always available.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEditor;
using UnityEngine;
using System.IO;

namespace InputSystemExtension
{
    #region === Asset Creation Menu ===

    /// <summary>
    /// Responsible for creating the InputSystemExtensionData asset in the Resources folder via the Unity Editor menu.
    /// </summary>
    public static class InputSystemExtensionDataAutoCreator
    {
        /// <summary>
        /// Creates the InputSystemExtensionData ScriptableObject at Assets/Resources if it does not already exist.
        /// Accessible via the Unity Editor menu.
        /// </summary>
        [MenuItem("Assets/Create/Input System Extension/Input System Extension Data")]
        public static void CreateCustomObjectData()
        {
            string path = "Assets/Resources";
            string assetPath = $"{path}/Input System Extension Data.asset";

            // Ensure the Resources folder exists.
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder("Assets", "Resources");

            // If the asset already exists, ask the user if they want to overwrite it.
            if (AssetDatabase.LoadAssetAtPath<InputSystemExtensionData>(assetPath) != null)
            {
                if (!EditorUtility.DisplayDialog("Replace File", "There is already an 'Input System Extension Data' asset. Do you want to replace it?", "Yes", "No"))
                {
                    return;
                }
            }

            // Create and save the new ScriptableObject asset.
            var asset = ScriptableObject.CreateInstance<InputSystemExtensionData>();
            AssetDatabase.CreateAsset(asset, assetPath);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Focus the Project window and select the new asset.
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }

    #endregion

    #region === Automatic Asset Creation on Editor Startup ===

    /// <summary>
    /// Ensures the InputSystemExtensionData asset is created automatically on Unity Editor startup, if missing.
    /// </summary>
    [InitializeOnLoad]
    public static class InputSystemExtensionDataStartup
    {
        static InputSystemExtensionDataStartup()
        {
            EditorApplication.delayCall += () =>
            {
                string assetPath = "Assets/Resources/Input System Extension Data.asset";

                if (!File.Exists(assetPath))
                {
                    InputSystemExtensionDataAutoCreator.CreateCustomObjectData();
                }
            };
        }
    }

    #endregion
}