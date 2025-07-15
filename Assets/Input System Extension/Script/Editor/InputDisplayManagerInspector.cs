/*
 * ---------------------------------------------------------------------------
 * Description: Custom inspector for InputDisplayManager to add an automatic
 *              presets creation button for input icon images.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.UI;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for the InputDisplayManager component.
/// Adds a button to auto-generate input icon presets based on assigned InputActionReferences.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(InputDisplayManager))]
public class InputDisplayManagerInspector : Editor
{
    /// <summary>
    /// Overrides the default inspector GUI to add a custom button and handle automatic preset creation.
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Updates serialized fields before drawing UI.
        serializedObject.Update();

        // Button to trigger automatic creation of input icon presets.
        if (GUILayout.Button("Create Automatic Presets", GUILayout.Height(30)))
        {
            foreach (var targetObject in targets)
            {
                var script = (InputDisplayManager)targetObject;

                // Registers undo for the main component before modification.
                Undo.RecordObject(script, "Create Automatic Presets");

                // Performs the automatic generation of icons based on the action references.
                CreateAutomaticPresets(script);

                // Marks the object as dirty to ensure changes are saved.
                EditorUtility.SetDirty(script);
            }
        }

        EditorGUILayout.Space(10);

        // Draws the rest of the inspector normally.
        DrawDefaultInspector();

        // Applies serialized field changes.
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Automatically creates or updates UI icon images based on the input action references.
    /// It ensures each action has a corresponding UI Image with consistent naming and hierarchy.
    /// </summary>
    private static void CreateAutomaticPresets(InputDisplayManager script)
    {
        var parentTransform = script.transform;

        // Process all single input viewers (non-directional).
        for (int i = 0; i < script.InputViewerDataEditor.Count; i++)
        {
            var data = script.InputViewerDataEditor[i];

            // Skip invalid entries with no action reference.
            if (data.inputActionReference == null || data.inputActionReference.action == null) continue;

            // Set nameTag to match the InputAction name.
            data.nameTag = data.inputActionReference.action.name;

            Image imageComponent;
            string currentName = $"Image ({data.nameTag})";

            if (data.inputIcon != null)
            {
                // If the Image already exists, rename it and register undo.
                Undo.RecordObject(data.inputIcon.gameObject, "Rename Image");
                data.inputIcon.gameObject.name = currentName;
                imageComponent = data.inputIcon;
            }
            else
            {
                // Try to find an existing image with that name in the hierarchy.
                var existingImageTransform = parentTransform.Find(currentName);
                if (existingImageTransform == null)
                {
                    // Create new Image if not found.
                    GameObject imageGO = new(currentName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    Undo.RegisterCreatedObjectUndo(imageGO, "Create Image GameObject");
                    imageGO.transform.SetParent(parentTransform, false);

                    var rt = imageGO.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(100, 100); // Set default size.

                    imageComponent = imageGO.GetComponent<Image>();
                }
                else
                {
                    // Reuse existing image and ensure it has an Image component.
                    imageComponent = existingImageTransform.GetComponent<Image>();
                    if (imageComponent == null)
                    {
                        Undo.RecordObject(existingImageTransform.gameObject, "Add Image Component");
                        imageComponent = existingImageTransform.gameObject.AddComponent<Image>();
                    }

                    Undo.RecordObject(existingImageTransform, "Rename Image");
                    existingImageTransform.name = currentName;
                }
            }

            // Assign the image back to the data.
            data.inputIcon = imageComponent;
            script.InputViewerDataEditor[i] = data;
        }

        // Process all directional input viewers (e.g., D-pad).
        for (int i = 0; i < script.InputMultipleViewsDataEditor.Count; i++)
        {
            var data = script.InputMultipleViewsDataEditor[i];

            // Skip invalid entries with no action reference.
            if (data.inputActionReference == null || data.inputActionReference.action == null) continue;

            // Set nameTag based on InputAction name.
            data.nameTag = data.inputActionReference.action.name;

            // Create or rename each directional image (Up, Down, Left, Right).
            data.inputIconUp = CreateOrRenameDirectionalImage(parentTransform, data.nameTag + "_Up", data.inputIconUp);
            data.inputIconDown = CreateOrRenameDirectionalImage(parentTransform, data.nameTag + "_Down", data.inputIconDown);
            data.inputIconLeft = CreateOrRenameDirectionalImage(parentTransform, data.nameTag + "_Left", data.inputIconLeft);
            data.inputIconRight = CreateOrRenameDirectionalImage(parentTransform, data.nameTag + "_Right", data.inputIconRight);

            // Save modified struct back.
            script.InputMultipleViewsDataEditor[i] = data;
        }
    }

    /// <summary>
    /// Creates or renames a directional input icon image under the parent transform.
    /// Handles undo operations and ensures the image is properly configured.
    /// </summary>
    /// <param name="parent">The parent transform under which the image is created or found.</param>
    /// <param name="expectedName">The desired name for the image GameObject.</param>
    /// <param name="currentImage">The existing image reference (can be null).</param>
    /// <returns>The Image component created or found.</returns>
    private static Image CreateOrRenameDirectionalImage(Transform parent, string expectedName, Image currentImage)
    {
        Image imageComp;
        string currentName = $"Image ({expectedName})";

        if (currentImage != null)
        {
            // If already assigned, rename and return.
            Undo.RecordObject(currentImage.gameObject, "Rename Directional Image");
            currentImage.gameObject.name = currentName;
            imageComp = currentImage;
        }
        else
        {
            // Try to find an existing transform with that name.
            var existingTransform = parent.Find(currentName);
            if (existingTransform != null)
            {
                imageComp = existingTransform.GetComponent<Image>();
                if (imageComp == null)
                {
                    Undo.RecordObject(existingTransform.gameObject, "Add Image Component");
                    imageComp = existingTransform.gameObject.AddComponent<Image>();
                }
            }
            else
            {
                // Create new GameObject if nothing exists.
                GameObject go = new(currentName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                Undo.RegisterCreatedObjectUndo(go, "Create Directional Image GameObject");
                go.transform.SetParent(parent, false);

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 100); // Default image size.

                imageComp = go.GetComponent<Image>();
            }
        }

        return imageComp;
    }
}