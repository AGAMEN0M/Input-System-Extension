using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace InputSystemExtension
{
    /// <summary>
    /// Utility class to create and instantiate predefined UI prefab objects related to Input System Extension.
    /// Adds entries to the GameObject menu for quick creation.
    /// </summary>
    public static class InputSystemExtensionPrefabCreator
    {
        #region === Utilities ===

        /// <summary>
        /// Creates a Canvas and EventSystem in the scene if none exist.
        /// </summary>
        /// <returns>The created Canvas instance.</returns>
        private static Canvas CreateUICanvas()
        {
            // Create the Canvas GameObject.
            GameObject canvasGO = new("Canvas");

            // Add required UI components.
            var canvas = canvasGO.AddComponent<Canvas>();
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Set canvas to render on screen overlay.
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.gameObject.layer = LayerMask.NameToLayer("UI");
            canvas.sortingOrder = 0;
            canvas.targetDisplay = 0;

            // Create the EventSystem GameObject.
            GameObject eventSystemGO = new("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Register the creation with Undo system.
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
            Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");

            return canvas;
        }

        /// <summary>
        /// Instantiates a prefab by name and parents it to the selected GameObject or the canvas if UI.
        /// </summary>
        /// <param name="fileName">Name of the prefab (without extension).</param>
        /// <param name="selectedGameObject">Currently selected GameObject in hierarchy.</param>
        /// <param name="isUI">If true, will ensure the object is parented under a UI canvas.</param>
        private static void CreateAndConfigurePrefab(string fileName, GameObject selectedGameObject, bool isUI = false)
        {
            // Try to find or create a Canvas if this is a UI prefab.
#pragma warning disable
#pragma warning disable UNT0007
            var canvas = isUI ? UnityEngine.Object.FindAnyObjectByType<Canvas>() ?? CreateUICanvas() : null;
#pragma warning restore UNT0007
#pragma warning restore

            // Find the prefab asset in the project.
            var prefab = FindPrefabByName(fileName);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found: {fileName}.prefab. Ensure it exists in the project.");
                return;
            }

            // Determine parent transform for the new prefab instance.
            var parent = selectedGameObject != null ? selectedGameObject.transform : (isUI ? canvas.transform : null);

            // Instantiate the prefab.
            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;

            // Final setup for renaming and unpacking.
            FinalizePrefabSetup(fileName, instance);
        }

        /// <summary>
        /// Searches the project for a prefab by name.
        /// </summary>
        /// <param name="prefabName">Name of the prefab (without extension).</param>
        /// <returns>The prefab GameObject asset, or null if not found.</returns>
        public static GameObject FindPrefabByName(string prefabName)
        {
            // Find all prefab assets matching the name.
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetFileNameWithoutExtension(path).Equals(prefabName, StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }

            Debug.LogError($"Prefab with the name '{prefabName}' not found.");
            return null;
        }

        /// <summary>
        /// Unpacks the prefab instance, registers Undo, and enables renaming.
        /// </summary>
        /// <param name="fileName">Name of the prefab being instantiated.</param>
        /// <param name="newGameObject">The new instance created from the prefab.</param>
        private static void FinalizePrefabSetup(string fileName, GameObject newGameObject)
        {
            if (newGameObject == null) return;

            // Register creation with Undo system.
            Undo.RegisterCreatedObjectUndo(newGameObject, $"Create {fileName}");

            // Unpack prefab so it's fully editable in scene.
            PrefabUtility.UnpackPrefabInstance(newGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // Select the new object.
            Selection.activeGameObject = newGameObject;

            // Trigger renaming via F2 key on the selected object.
            EditorApplication.delayCall += () =>
            {
                if (Selection.activeGameObject == newGameObject)
                {
                    EditorWindow.focusedWindow.SendEvent(new()
                    {
                        keyCode = KeyCode.F2,
                        type = EventType.KeyDown
                    });
                }
            };
        }

        #endregion

        #region === Menu Items ===

        /// <summary>
        /// Adds a menu option to instantiate the "Rebind Control Manager (Legacy)" UI prefab.
        /// </summary>
        [MenuItem("GameObject/UI/Input System Extension/Rebind Control Manager (Legacy)", false, 1)]
        public static void CreateRebindControlManagerPrefab()
        {
            CreateAndConfigurePrefab("Rebind Control Manager (Legacy)", Selection.activeGameObject, true);
        }

        /// <summary>
        /// Adds a menu option to instantiate the "Rebind Control Manager (TMP)" UI prefab.
        /// </summary>
        [MenuItem("GameObject/UI/Input System Extension/Rebind Control Manager (TMP)", false, 2)]
        public static void CreateRebindControlManagerTMPPrefab()
        {
            CreateAndConfigurePrefab("Rebind Control Manager (TMP)", Selection.activeGameObject, true);
        }

        /// <summary>
        /// Adds a menu option to instantiate the "Button (Reset All) [Legacy]" UI prefab.
        /// </summary>
        [MenuItem("GameObject/UI/Input System Extension/Button (Reset All) [Legacy]", false, 3)]
        public static void CreateButtonResetAllPrefab()
        {
            CreateAndConfigurePrefab("Button (Reset All) [Legacy]", Selection.activeGameObject, true);
        }

        /// <summary>
        /// Adds a menu option to instantiate the "Button (Reset All) [TMP]" UI prefab.
        /// </summary>
        [MenuItem("GameObject/UI/Input System Extension/Button (Reset All) [TMP]", false, 4)]
        public static void CreateButtonResetAllTMPPrefab()
        {
            CreateAndConfigurePrefab("Button (Reset All) [TMP]", Selection.activeGameObject, true);
        }

        #endregion
    }
}