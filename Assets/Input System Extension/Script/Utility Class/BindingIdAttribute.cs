using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEngine.InputSystem;
using UnityEditor;
using System.Linq;
#endif

/// <summary>
/// Attribute used to link a binding ID field with an InputActionReference field in the same class.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class BindingIdAttribute : PropertyAttribute
{
    /// <summary>
    /// Name of the InputActionReference field this binding ID is associated with.
    /// </summary>
    public string actionFieldName;

    /// <summary>
    /// Creates a new instance of the attribute, referencing the given action field name.
    /// </summary>
    /// <param name="actionFieldName">The name of the InputActionReference field to get bindings from.</param>
    public BindingIdAttribute(string actionFieldName)
    {
        this.actionFieldName = actionFieldName;
    }
}

#if UNITY_EDITOR
/// <summary>
/// Custom property drawer that renders a popup of binding options from a referenced InputAction.
/// </summary>
[CustomPropertyDrawer(typeof(BindingIdAttribute))]
public class BindingIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Check if the attribute is valid.
        if (attribute is not BindingIdAttribute bindingIdAttr)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // Find the InputActionReference field using the name specified in the attribute.
        var actionProperty = property.serializedObject.FindProperty(bindingIdAttr.actionFieldName);
        if (actionProperty == null)
        {
            EditorGUI.HelpBox(position, $"Action field '{bindingIdAttr.actionFieldName}' not found.", MessageType.Error);
            return;
        }

        // Get the actual InputAction from the reference.
        var actionRef = actionProperty.objectReferenceValue as InputActionReference;
        var action = actionRef != null ? actionRef.action : null;
        if (action == null)
        {
            EditorGUI.HelpBox(position, "No InputAction selected.", MessageType.Warning);
            return;
        }

        var bindings = action.bindings;
        var bindingCount = bindings.Count;

        // Arrays to hold the popup options and corresponding binding IDs.
        var options = new GUIContent[bindingCount];
        var optionValues = new string[bindingCount];
        var currentBindingId = property.stringValue;
        int selectedIndex = -1;

        // Build display strings for each binding.
        for (int i = 0; i < bindingCount; i++)
        {
            var binding = bindings[i];
            var bindingId = binding.id.ToString();
            var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

            // Use display options that show long names and ignore overrides.
            var displayOptions = InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;

            // If no control schemes are defined, explicitly show device info.
            if (!haveBindingGroups) displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

            // Get the formatted display string for the binding.
            var displayString = action.GetBindingDisplayString(i, displayOptions);

            // If it's part of a composite binding, prefix it with the part name.
            if (binding.isPartOfComposite) displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";
            
            displayString = displayString.Replace('/', '\\'); // Prevent '/' from creating submenus in the popup.

            // If the binding is part of control schemes, add the scheme names.
            if (haveBindingGroups)
            {
                var asset = action.actionMap?.asset;
                if (asset != null)
                {
                    var controlSchemes = string.Join(", ", binding.groups.Split(InputBinding.Separator)
                        .Select(x =>
                        {
                            var scheme = asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x);
                            return scheme.name;
                        })
                        .Where(x => !string.IsNullOrEmpty(x)));

                    if (!string.IsNullOrEmpty(controlSchemes)) displayString = $"{displayString} ({controlSchemes})";
                }
            }

            options[i] = new GUIContent(displayString);
            optionValues[i] = bindingId;

            // Track currently selected binding.
            if (bindingId == currentBindingId) selectedIndex = i;
        }

        // Draw the popup field with binding options.
        EditorGUI.BeginProperty(position, label, property);

        int newIndex = EditorGUI.Popup(position, label, selectedIndex, options);
        if (newIndex != selectedIndex && newIndex >= 0)
        {
            property.stringValue = optionValues[newIndex];
        }

        EditorGUI.EndProperty();
    }
}
#endif