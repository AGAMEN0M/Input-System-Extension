/*
 * ---------------------------------------------------------------------------
 * Description: ScriptableObject configuration for Input System Extension. 
 *              Stores binding settings, gamepad icons, and provides editor 
 *              access via menu for centralized input management.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace InputSystemExtension
{
    /// <summary>
    /// Stores configuration data for the input system, including custom binding persistence
    /// and gamepad icon mappings for visual representation in the UI.
    /// </summary>
    public class InputSystemExtensionData : ScriptableObject
    {
        [Header("Input Settings")]

        /// <summary>
        /// Reference to the primary InputActionAsset whose bindings will be saved and loaded.
        /// </summary>
        public InputActionAsset defaultInputAction;

        /// <summary>
        /// The PlayerPrefs key used to store binding override data in JSON format.
        /// </summary>
        public string playerPrefsKey = "BindingsData";

        [Header("Icons Settings")]

        /// <summary>
        /// Default sprite used when displaying keyboard input visuals.
        /// </summary>
        public Sprite defaultSprite;

        [Space(10)]

        /// <summary>
        /// List of sprites mapped to specific keyboard keys for visual display.
        /// </summary>
        public List<InputSpriteList> KeyCodes;

        /// <summary>
        /// Sprite set for Xbox-style gamepads, used to visually represent controller buttons.
        /// </summary>
        public GamepadIcons xbox;

        /// <summary>
        /// Sprite set for PS4 (DualShock)-style gamepads, used to visually represent controller buttons.
        /// </summary>
        public GamepadIcons ps4;

        /// <summary>
        /// Represents a sprite mapping for a specific keyboard key.
        /// </summary>
        [Serializable]
        public struct InputSpriteList
        {
            public KeyCode keyCode; // The KeyCode associated with this sprite. 
            public Sprite sprite; // The Sprite that represents the key visually.
        }

        /// <summary>
        /// Struct representing a collection of sprites that map to common gamepad control paths.
        /// Used for replacing text labels in UI with corresponding visual icons.
        /// </summary>
        [Serializable]
        public struct GamepadIcons
        {
            // Face buttons
            public Sprite buttonSouth;   // A / Cross
            public Sprite buttonNorth;   // Y / Triangle
            public Sprite buttonEast;    // B / Circle
            public Sprite buttonWest;    // X / Square

            // Menu buttons
            public Sprite startButton;   // Start / Options
            public Sprite selectButton;  // Select / Share

            // Triggers and bumpers
            public Sprite leftTrigger;   // LT / L2
            public Sprite rightTrigger;  // RT / R2
            public Sprite leftShoulder;  // LB / L1
            public Sprite rightShoulder; // RB / R1

            // D-Pad
            public Sprite dpad;
            public Sprite dpadUp;
            public Sprite dpadDown;
            public Sprite dpadLeft;
            public Sprite dpadRight;

            // Analog sticks
            public Sprite leftStick;
            public Sprite rightStick;
            public Sprite leftStickPress;
            public Sprite rightStickPress;

            /// <summary>
            /// Returns the appropriate sprite for a given control path string.
            /// </summary>
            /// <param name="controlPath">The control path (e.g., "buttonSouth", "dpad/left").</param>
            /// <returns>The matching sprite, or null if no match is found.</returns>
            public readonly Sprite GetSprite(string controlPath)
            {
                return controlPath switch
                {
                    "buttonSouth" => buttonSouth,
                    "buttonNorth" => buttonNorth,
                    "buttonEast" => buttonEast,
                    "buttonWest" => buttonWest,
                    "start" => startButton,
                    "select" => selectButton,
                    "leftTrigger" => leftTrigger,
                    "rightTrigger" => rightTrigger,
                    "leftShoulder" => leftShoulder,
                    "rightShoulder" => rightShoulder,
                    "dpad" => dpad,
                    "dpad/up" => dpadUp,
                    "dpad/down" => dpadDown,
                    "dpad/left" => dpadLeft,
                    "dpad/right" => dpadRight,
                    "leftStick" => leftStick,
                    "rightStick" => rightStick,
                    "leftStickPress" => leftStickPress,
                    "rightStickPress" => rightStickPress,
                    _ => HandleUnknown(controlPath),
                };
            }

            /// <summary>
            /// Handles unknown control paths by logging a warning and returning null.
            /// </summary>
            /// <param name="path">The unknown control path.</param>
            /// <returns>Always returns null.</returns>
            private static Sprite HandleUnknown(string path)
            {
                Debug.LogWarning($"Unknown control path: {path}");
                return null;
            }
        }
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