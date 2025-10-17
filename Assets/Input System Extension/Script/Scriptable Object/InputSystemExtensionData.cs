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
    #region === Input System Extension Data ===

    /// <summary>
    /// Stores configuration data for the input system, including custom binding persistence
    /// and gamepad icon mappings for visual representation in the UI.
    /// </summary>
    public class InputSystemExtensionData : ScriptableObject
    {
        #region === Input Settings ===

        [Header("Input Settings")]
        [Tooltip("Reference to the main InputActionAsset that defines the player input bindings.")]
        public InputActionAsset defaultInputAction;

        [Tooltip("Unique PlayerPrefs key used to store serialized binding data as JSON.")]
        public string playerPrefsKey = "BindingsData";

        #endregion

        #region === Icon Settings ===

        [Header("Icons Settings")]
        [Tooltip("Default sprite to use for keyboard inputs when no specific key icon is found.")]
        public Sprite defaultSprite;

        [Space(10)]

        [Tooltip("List of keyboard key-to-sprite mappings for UI display.")]
        public List<InputSpriteList> KeyCodes;

        [Tooltip("Set of sprites for Xbox-style gamepads (A, B, X, Y, triggers, etc.).")]
        public GamepadIcons xbox;

        [Tooltip("Set of sprites for PS4 (DualShock)-style gamepads (Cross, Circle, Square, Triangle, etc.).")]
        public GamepadIcons ps4;

        #endregion

        #region === Struct: InputSpriteList ===

        /// <summary>
        /// Represents a sprite mapping for a specific keyboard key.
        /// </summary>
        [Serializable]
        public struct InputSpriteList
        {
            [Tooltip("KeyCode associated with this sprite.")]
            public KeyCode keyCode; // The KeyCode associated with this sprite.

            [Tooltip("Name of the key used by the Input Action")]
            public string keyName; // Key name.

            [Tooltip("Sprite representing this specific key visually.")]
            public Sprite sprite; // The Sprite that represents the key visually.
        }

        #endregion

        #region === Struct: GamepadIcons ===

        /// <summary>
        /// Struct representing a collection of sprites that map to common gamepad control paths.
        /// Used for replacing text labels in UI with corresponding visual icons.
        /// </summary>
        [Serializable]
        public struct GamepadIcons
        {
            // === Face Buttons ===
            [Tooltip("Sprite for the South button (A / Cross).")]
            public Sprite buttonSouth;

            [Tooltip("Sprite for the North button (Y / Triangle).")]
            public Sprite buttonNorth;

            [Tooltip("Sprite for the East button (B / Circle).")]
            public Sprite buttonEast;

            [Tooltip("Sprite for the West button (X / Square).")]
            public Sprite buttonWest;

            // === Menu Buttons ===
            [Tooltip("Sprite for the Start or Options button.")]
            public Sprite startButton;

            [Tooltip("Sprite for the Select or Share button.")]
            public Sprite selectButton;

            // === Triggers and Bumpers ===
            [Tooltip("Sprite for the Left Trigger (LT / L2).")]
            public Sprite leftTrigger;

            [Tooltip("Sprite for the Right Trigger (RT / R2).")]
            public Sprite rightTrigger;

            [Tooltip("Sprite for the Left Shoulder (LB / L1).")]
            public Sprite leftShoulder;

            [Tooltip("Sprite for the Right Shoulder (RB / R1).")]
            public Sprite rightShoulder;

            // === D-Pad ===
            [Tooltip("Sprite representing the full D-Pad.")]
            public Sprite dpad;

            [Tooltip("Sprite representing the D-Pad Up direction.")]
            public Sprite dpadUp;

            [Tooltip("Sprite representing the D-Pad Down direction.")]
            public Sprite dpadDown;

            [Tooltip("Sprite representing the D-Pad Left direction.")]
            public Sprite dpadLeft;

            [Tooltip("Sprite representing the D-Pad Right direction.")]
            public Sprite dpadRight;

            // === Analog Sticks ===
            [Tooltip("Sprite representing the Left Stick.")]
            public Sprite leftStick;

            [Tooltip("Sprite representing the Right Stick.")]
            public Sprite rightStick;

            [Tooltip("Sprite representing a Left Stick press action.")]
            public Sprite leftStickPress;

            [Tooltip("Sprite representing a Right Stick press action.")]
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
        #endregion
    }

    #endregion

#if UNITY_EDITOR

    #region === Editor Integration ===

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

    #endregion

#endif
}