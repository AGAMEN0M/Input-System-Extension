using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.InputSystem;
using InputSystemExtension;
using System.Linq;
using UnityEditor;
#endif

/// <summary>
/// Attribute to show a dropdown in the inspector listing InputActionReference
/// options from the default InputActionAsset configured in InputSystemExtensionData.
/// </summary>
public class GetActionAttribute : PropertyAttribute { }

#if UNITY_EDITOR
/// <summary>
/// Custom PropertyDrawer for the [GetAction] attribute.
/// Allows selecting an InputActionReference from actions in the configured InputActionAsset.
/// </summary>
[CustomPropertyDrawer(typeof(GetActionAttribute))]
public class GetActionDrawer : PropertyDrawer
{
    private InputSystemExtensionData extensionData; // Cached reference to the ScriptableObject that holds the default InputActionAsset.

    /// <summary>
    /// Main method that draws the custom field in the inspector.
    /// </summary>
    /// <param name="position">Rect area where the field will be drawn.</param>
    /// <param name="property">The serialized property being drawn.</param>
    /// <param name="label">Label for the field in the inspector.</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Validate that the property is an ObjectReference (required for InputActionReference).
        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            EditorGUI.HelpBox(position, "Use [GetAction] with InputActionReference.", MessageType.Error);
            return;
        }

        // Load the ScriptableObject if not yet loaded.
        if (extensionData == null)
        {
            extensionData = InputSystemExtensionHelper.GetInputSystemExtensionData();
        }

        // Check if the ScriptableObject or its default InputActionAsset is missing.
        if (extensionData == null || extensionData.defaultInputAction == null)
        {
            EditorGUI.HelpBox(position, "Missing InputSystemExtensionData or defaultInputAction.", MessageType.Error);
            return;
        }

        // Gather all valid actions from all action maps in the default InputActionAsset.
        var actions = extensionData.defaultInputAction.actionMaps
            .Where(map => map != null)
            .SelectMany(map => map.actions)
            .Where(action => action != null)
            .ToList();

        // Display an error if no actions are found.
        if (actions.Count == 0)
        {
            EditorGUI.HelpBox(position, "No actions found in asset.", MessageType.Warning);
            return;
        }

        // Get the currently assigned InputActionReference from the property.
        var currentRef = property.objectReferenceValue as InputActionReference;

        // Find the index of the currently assigned action in the actions list.
        int currentIndex = (currentRef != null && currentRef.action != null)
            ? actions.FindIndex(a => a == currentRef.action)
            : -1;

        // Prepare the display strings for the popup: "ActionMapName/ActionName".
        string[] displayNames = actions.Select(a => $"{a.actionMap.name}/{a.name}").ToArray();

        // Draw the popup dropdown with current selection.
        int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, displayNames);

        // If the selected action has changed and the selection is valid, update the property.
        if (selectedIndex != currentIndex && selectedIndex >= 0)
        {
            var selectedAction = actions[selectedIndex];
            property.objectReferenceValue = InputActionReference.Create(selectedAction);
        }
    }
}
#endif