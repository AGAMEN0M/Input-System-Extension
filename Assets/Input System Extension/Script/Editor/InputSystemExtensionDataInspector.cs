/*
 * ---------------------------------------------------------------------------
 * Description: Custom inspector for InputSystemExtensionData that enables 
 *              bulk icon generation for keyboard and gamepad bindings. 
 *              Supports multi-object editing via Unity Editor.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using static InputSystemExtension.InputSystemExtensionData;

namespace InputSystemExtension
{
    /// <summary>
    /// Custom inspector for InputSystemExtensionData with support for multi-object editing.
    /// Adds a button to auto-generate default icon mappings for selected ScriptableObjects.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(InputSystemExtensionData))]
    public class InputSystemExtensionDataInspector : Editor
    {
        /// <summary>
        /// Draws the custom inspector UI, including the "Get Default Icons" button.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Updates the serialized data.

            // Button that applies the mapping to all selected objects.
            if (GUILayout.Button("Get Default Icons", GUILayout.Height(30)))
            {
                foreach (var targetObject in targets)
                {
                    var script = (InputSystemExtensionData)targetObject;
                    GenerateKeyCodeList(script);
                    EditorUtility.SetDirty(script); // Mark as modified to save.
                }
            }

            EditorGUILayout.Space(10);

            DrawDefaultInspector(); // Draws the default inspector for public fields.

            serializedObject.ApplyModifiedProperties(); // Applies any serialized modifications.
        }

        /// <summary>
        /// Populates the InputSystemExtensionData with default sprites for Xbox and PS4 gamepads,
        /// as well as basic keyboard key sprite mappings.
        /// </summary>
        /// <param name="script">The target ScriptableObject to populate.</param>
        private static void GenerateKeyCodeList(InputSystemExtensionData script)
        {
            script.KeyCodes.Clear(); // Clears existing keyboard mappings.
            var sprite = script.defaultSprite; // Uses the default keyboard sprite as a fallback.

            // Initializes Xbox gamepad icons using predefined sprite names.
            script.xbox = new GamepadIcons
            {
                buttonSouth = GetSprite("XboxOne_A", sprite),
                buttonNorth = GetSprite("XboxOne_Y", sprite),
                buttonEast = GetSprite("XboxOne_B", sprite),
                buttonWest = GetSprite("XboxOne_X", sprite),
                startButton = GetSprite("XboxOne_Menu", sprite),
                selectButton = GetSprite("XboxOne_Windows", sprite),
                leftTrigger = GetSprite("XboxOne_LT", sprite),
                rightTrigger = GetSprite("XboxOne_RT", sprite),
                leftShoulder = GetSprite("XboxOne_LB", sprite),
                rightShoulder = GetSprite("XboxOne_RB", sprite),
                dpad = GetSprite("XboxOne_Dpad", sprite),
                dpadUp = GetSprite("XboxOne_Dpad_Up", sprite),
                dpadDown = GetSprite("XboxOne_Dpad_Down", sprite),
                dpadLeft = GetSprite("XboxOne_Dpad_Left", sprite),
                dpadRight = GetSprite("XboxOne_Dpad_Right", sprite),
                leftStick = GetSprite("XboxOne_Left_Stick", sprite),
                rightStick = GetSprite("XboxOne_Right_Stick", sprite),
                leftStickPress = GetSprite("XboxOne_Left_Stick", sprite),
                rightStickPress = GetSprite("XboxOne_Right_Stick", sprite),
            };

            // Initializes PS4 gamepad icons using predefined sprite names.
            script.ps4 = new GamepadIcons
            {
                buttonSouth = GetSprite("PS4_Cross", sprite),
                buttonNorth = GetSprite("PS4_Triangle", sprite),
                buttonEast = GetSprite("PS4_Circle", sprite),
                buttonWest = GetSprite("PS4_Square", sprite),
                startButton = GetSprite("PS4_Options", sprite),
                selectButton = GetSprite("PS4_Share", sprite),
                leftTrigger = GetSprite("PS4_L2", sprite),
                rightTrigger = GetSprite("PS4_R2", sprite),
                leftShoulder = GetSprite("PS4_L1", sprite),
                rightShoulder = GetSprite("PS4_R1", sprite),
                dpad = GetSprite("PS4_Dpad", sprite),
                dpadUp = GetSprite("PS4_Dpad_Up", sprite),
                dpadDown = GetSprite("PS4_Dpad_Down", sprite),
                dpadLeft = GetSprite("PS4_Dpad_Left", sprite),
                dpadRight = GetSprite("PS4_Dpad_Right", sprite),
                leftStick = GetSprite("PS4_Left_Stick", sprite),
                rightStick = GetSprite("PS4_Right_Stick", sprite),
                leftStickPress = GetSprite("PS4_Left_Stick", sprite),
                rightStickPress = GetSprite("PS4_Right_Stick", sprite),
            };

            // Creates a predefined list of keyboard key to sprite mappings.
            var keyCodeList = new List<InputSpriteList> {
                new() { keyCode = KeyCode.None, sprite = GetSprite("None", sprite) },
                new() { keyCode = KeyCode.Backspace, sprite = GetSprite("Backspace", sprite) },
                new() { keyCode = KeyCode.Delete, sprite = GetSprite("Delete", sprite) },
                new() { keyCode = KeyCode.Tab, sprite = GetSprite("Tab", sprite) },
                new() { keyCode = KeyCode.Clear, sprite = GetSprite("Clear", sprite) },
                new() { keyCode = KeyCode.Return, sprite = GetSprite("Return", sprite) },
                new() { keyCode = KeyCode.Pause, sprite = GetSprite("Pause", sprite) },
                new() { keyCode = KeyCode.Escape, sprite = GetSprite("Escape", sprite) },
                new() { keyCode = KeyCode.Space, sprite = GetSprite("Space", sprite) },
                new() { keyCode = KeyCode.Keypad0, sprite = GetSprite("Keypad0", sprite) },
                new() { keyCode = KeyCode.Keypad1, sprite = GetSprite("Keypad1", sprite) },
                new() { keyCode = KeyCode.Keypad2, sprite = GetSprite("Keypad2", sprite) },
                new() { keyCode = KeyCode.Keypad3, sprite = GetSprite("Keypad3", sprite) },
                new() { keyCode = KeyCode.Keypad4, sprite = GetSprite("Keypad4", sprite) },
                new() { keyCode = KeyCode.Keypad5, sprite = GetSprite("Keypad5", sprite) },
                new() { keyCode = KeyCode.Keypad6, sprite = GetSprite("Keypad6", sprite) },
                new() { keyCode = KeyCode.Keypad7, sprite = GetSprite("Keypad7", sprite) },
                new() { keyCode = KeyCode.Keypad8, sprite = GetSprite("Keypad8", sprite) },
                new() { keyCode = KeyCode.Keypad9, sprite = GetSprite("Keypad9", sprite) },
                new() { keyCode = KeyCode.KeypadPeriod, sprite = GetSprite("Keypad Period", sprite) },
                new() { keyCode = KeyCode.KeypadDivide, sprite = GetSprite("Keypad Divide", sprite) },
                new() { keyCode = KeyCode.KeypadMultiply, sprite = GetSprite("Keypad Multiply", sprite) },
                new() { keyCode = KeyCode.KeypadMinus, sprite = GetSprite("Keypad Minus", sprite) },
                new() { keyCode = KeyCode.KeypadPlus, sprite = GetSprite("Keypad Plus", sprite) },
                new() { keyCode = KeyCode.KeypadEnter, sprite = GetSprite("Keypad Enter", sprite) },
                new() { keyCode = KeyCode.KeypadEquals, sprite = GetSprite("Keypad Equals", sprite) },
                new() { keyCode = KeyCode.UpArrow, sprite = GetSprite("Up Arrow", sprite) },
                new() { keyCode = KeyCode.DownArrow, sprite = GetSprite("Down Arrow", sprite) },
                new() { keyCode = KeyCode.RightArrow, sprite = GetSprite("Right Arrow", sprite) },
                new() { keyCode = KeyCode.LeftArrow, sprite = GetSprite("Left Arrow", sprite) },
                new() { keyCode = KeyCode.Insert, sprite = GetSprite("Insert", sprite) },
                new() { keyCode = KeyCode.Home, sprite = GetSprite("Home", sprite) },
                new() { keyCode = KeyCode.End, sprite = GetSprite("End", sprite) },
                new() { keyCode = KeyCode.PageUp, sprite = GetSprite("Page Up", sprite) },
                new() { keyCode = KeyCode.PageDown, sprite = GetSprite("Page Down", sprite) },
                new() { keyCode = KeyCode.F1, sprite = GetSprite("F1", sprite) },
                new() { keyCode = KeyCode.F2, sprite = GetSprite("F2", sprite) },
                new() { keyCode = KeyCode.F3, sprite = GetSprite("F3", sprite) },
                new() { keyCode = KeyCode.F4, sprite = GetSprite("F4", sprite) },
                new() { keyCode = KeyCode.F5, sprite = GetSprite("F5", sprite) },
                new() { keyCode = KeyCode.F6, sprite = GetSprite("F6", sprite) },
                new() { keyCode = KeyCode.F7, sprite = GetSprite("F7", sprite) },
                new() { keyCode = KeyCode.F8, sprite = GetSprite("F8", sprite) },
                new() { keyCode = KeyCode.F9, sprite = GetSprite("F9", sprite) },
                new() { keyCode = KeyCode.F10, sprite = GetSprite("F10", sprite) },
                new() { keyCode = KeyCode.F11, sprite = GetSprite("F11", sprite) },
                new() { keyCode = KeyCode.F12, sprite = GetSprite("F12", sprite) },
                new() { keyCode = KeyCode.F13, sprite = GetSprite("F13", sprite) },
                new() { keyCode = KeyCode.F14, sprite = GetSprite("F14", sprite) },
                new() { keyCode = KeyCode.F15, sprite = GetSprite("F15", sprite) },
                new() { keyCode = KeyCode.Alpha0, sprite = GetSprite("Alpha0", sprite) },
                new() { keyCode = KeyCode.Alpha1, sprite = GetSprite("Alpha1", sprite) },
                new() { keyCode = KeyCode.Alpha2, sprite = GetSprite("Alpha2", sprite) },
                new() { keyCode = KeyCode.Alpha3, sprite = GetSprite("Alpha3", sprite) },
                new() { keyCode = KeyCode.Alpha4, sprite = GetSprite("Alpha4", sprite) },
                new() { keyCode = KeyCode.Alpha5, sprite = GetSprite("Alpha5", sprite) },
                new() { keyCode = KeyCode.Alpha6, sprite = GetSprite("Alpha6", sprite) },
                new() { keyCode = KeyCode.Alpha7, sprite = GetSprite("Alpha7", sprite) },
                new() { keyCode = KeyCode.Alpha8, sprite = GetSprite("Alpha8", sprite) },
                new() { keyCode = KeyCode.Alpha9, sprite = GetSprite("Alpha9", sprite) },
                new() { keyCode = KeyCode.Exclaim, sprite = GetSprite("Exclaim", sprite) },
                new() { keyCode = KeyCode.DoubleQuote, sprite = GetSprite("Double Quote", sprite) },
                new() { keyCode = KeyCode.Hash, sprite = GetSprite("Hash", sprite) },
                new() { keyCode = KeyCode.Dollar, sprite = GetSprite("Dollar", sprite) },
                new() { keyCode = KeyCode.Percent, sprite = GetSprite("Percent", sprite) },
                new() { keyCode = KeyCode.Ampersand, sprite = GetSprite("Ampersand", sprite) },
                new() { keyCode = KeyCode.Quote, sprite = GetSprite("Quote", sprite) },
                new() { keyCode = KeyCode.LeftParen, sprite = GetSprite("Left Paren", sprite) },
                new() { keyCode = KeyCode.RightParen, sprite = GetSprite("Right Paren", sprite) },
                new() { keyCode = KeyCode.Asterisk, sprite = GetSprite("Asterisk", sprite) },
                new() { keyCode = KeyCode.Plus, sprite = GetSprite("Plus", sprite) },
                new() { keyCode = KeyCode.Comma, sprite = GetSprite("Comma", sprite) },
                new() { keyCode = KeyCode.Minus, sprite = GetSprite("Minus", sprite) },
                new() { keyCode = KeyCode.Period, sprite = GetSprite("Period", sprite) },
                new() { keyCode = KeyCode.Slash, sprite = GetSprite("Slash", sprite) },
                new() { keyCode = KeyCode.Colon, sprite = GetSprite("Colon", sprite) },
                new() { keyCode = KeyCode.Semicolon, sprite = GetSprite("Semicolon", sprite) },
                new() { keyCode = KeyCode.Less, sprite = GetSprite("Less", sprite) },
                new() { keyCode = KeyCode.Equals, sprite = GetSprite("Equals", sprite) },
                new() { keyCode = KeyCode.Greater, sprite = GetSprite("Greater", sprite) },
                new() { keyCode = KeyCode.Question, sprite = GetSprite("Question", sprite) },
                new() { keyCode = KeyCode.At, sprite = GetSprite("At", sprite) },
                new() { keyCode = KeyCode.LeftBracket, sprite = GetSprite("Left Bracket", sprite) },
                new() { keyCode = KeyCode.Backslash, sprite = GetSprite("Backslash", sprite) },
                new() { keyCode = KeyCode.RightBracket, sprite = GetSprite("Right Bracket", sprite) },
                new() { keyCode = KeyCode.Caret, sprite = GetSprite("Caret", sprite) },
                new() { keyCode = KeyCode.Underscore, sprite = GetSprite("Underscore", sprite) },
                new() { keyCode = KeyCode.BackQuote, sprite = GetSprite("Back Quote", sprite) },
                new() { keyCode = KeyCode.A, sprite = GetSprite("A", sprite) },
                new() { keyCode = KeyCode.B, sprite = GetSprite("B", sprite) },
                new() { keyCode = KeyCode.C, sprite = GetSprite("C", sprite) },
                new() { keyCode = KeyCode.D, sprite = GetSprite("D", sprite) },
                new() { keyCode = KeyCode.E, sprite = GetSprite("E", sprite) },
                new() { keyCode = KeyCode.F, sprite = GetSprite("F", sprite) },
                new() { keyCode = KeyCode.G, sprite = GetSprite("G", sprite) },
                new() { keyCode = KeyCode.H, sprite = GetSprite("H", sprite) },
                new() { keyCode = KeyCode.I, sprite = GetSprite("I", sprite) },
                new() { keyCode = KeyCode.J, sprite = GetSprite("J", sprite) },
                new() { keyCode = KeyCode.K, sprite = GetSprite("K", sprite) },
                new() { keyCode = KeyCode.L, sprite = GetSprite("L", sprite) },
                new() { keyCode = KeyCode.M, sprite = GetSprite("M", sprite) },
                new() { keyCode = KeyCode.N, sprite = GetSprite("N", sprite) },
                new() { keyCode = KeyCode.O, sprite = GetSprite("O", sprite) },
                new() { keyCode = KeyCode.P, sprite = GetSprite("P", sprite) },
                new() { keyCode = KeyCode.Q, sprite = GetSprite("Q", sprite) },
                new() { keyCode = KeyCode.R, sprite = GetSprite("R", sprite) },
                new() { keyCode = KeyCode.S, sprite = GetSprite("S", sprite) },
                new() { keyCode = KeyCode.T, sprite = GetSprite("T", sprite) },
                new() { keyCode = KeyCode.U, sprite = GetSprite("U", sprite) },
                new() { keyCode = KeyCode.V, sprite = GetSprite("V", sprite) },
                new() { keyCode = KeyCode.W, sprite = GetSprite("W", sprite) },
                new() { keyCode = KeyCode.X, sprite = GetSprite("X", sprite) },
                new() { keyCode = KeyCode.Y, sprite = GetSprite("Y", sprite) },
                new() { keyCode = KeyCode.Z, sprite = GetSprite("Z", sprite) },
                new() { keyCode = KeyCode.LeftCurlyBracket, sprite = GetSprite("Left Curly Bracket", sprite) },
                new() { keyCode = KeyCode.Pipe, sprite = GetSprite("Pipe", sprite) },
                new() { keyCode = KeyCode.RightCurlyBracket, sprite = GetSprite("Right Curly Bracket", sprite) },
                new() { keyCode = KeyCode.Tilde, sprite = GetSprite("Tilde", sprite) },
                new() { keyCode = KeyCode.Numlock, sprite = GetSprite("Numlock", sprite) },
                new() { keyCode = KeyCode.CapsLock, sprite = GetSprite("Caps Lock", sprite) },
                new() { keyCode = KeyCode.ScrollLock, sprite = GetSprite("Scroll Lock", sprite) },
                new() { keyCode = KeyCode.RightShift, sprite = GetSprite("Right Shift", sprite) },
                new() { keyCode = KeyCode.LeftShift, sprite = GetSprite("Left Shift", sprite) },
                new() { keyCode = KeyCode.RightControl, sprite = GetSprite("Right Control", sprite) },
                new() { keyCode = KeyCode.LeftControl, sprite = GetSprite("Left Control", sprite) },
                new() { keyCode = KeyCode.RightAlt, sprite = GetSprite("Right Alt", sprite) },
                new() { keyCode = KeyCode.LeftAlt, sprite = GetSprite("Left Alt", sprite) },
                new() { keyCode = KeyCode.LeftMeta, sprite = GetSprite("Left Meta", sprite) },
                new() { keyCode = KeyCode.LeftCommand, sprite = GetSprite("Left Command", sprite) },
                new() { keyCode = KeyCode.LeftApple, sprite = GetSprite("Left Apple", sprite) },
                new() { keyCode = KeyCode.LeftWindows, sprite = GetSprite("Left Windows", sprite) },
                new() { keyCode = KeyCode.RightMeta, sprite = GetSprite("Right Meta", sprite) },
                new() { keyCode = KeyCode.RightCommand, sprite = GetSprite("Right Command", sprite) },
                new() { keyCode = KeyCode.RightApple, sprite = GetSprite("Right Apple", sprite) },
                new() { keyCode = KeyCode.RightWindows, sprite = GetSprite("Right Windows", sprite) },
                new() { keyCode = KeyCode.AltGr, sprite = GetSprite("Alt Gr", sprite) },
                new() { keyCode = KeyCode.Help, sprite = GetSprite("Help", sprite) },
                new() { keyCode = KeyCode.Print, sprite = GetSprite("Print", sprite) },
                new() { keyCode = KeyCode.SysReq, sprite = GetSprite("Sys Req", sprite) },
                new() { keyCode = KeyCode.Break, sprite = GetSprite("Break", sprite) },
                new() { keyCode = KeyCode.Menu, sprite = GetSprite("Menu", sprite) },
                new() { keyCode = KeyCode.WheelUp, sprite = GetSprite("Wheel Up", sprite) },
                new() { keyCode = KeyCode.WheelDown, sprite = GetSprite("Wheel Down", sprite) },
                new() { keyCode = KeyCode.Mouse0, sprite = GetSprite("Mouse0", sprite) },
                new() { keyCode = KeyCode.Mouse1, sprite = GetSprite("Mouse1", sprite) },
                new() { keyCode = KeyCode.Mouse2, sprite = GetSprite("Mouse2", sprite) },
                new() { keyCode = KeyCode.Mouse3, sprite = GetSprite("Mouse3", sprite) },
                new() { keyCode = KeyCode.Mouse4, sprite = GetSprite("Mouse4", sprite) },
                new() { keyCode = KeyCode.Mouse5, sprite = GetSprite("Mouse5", sprite) },
                new() { keyCode = KeyCode.Mouse6, sprite = GetSprite("Mouse6", sprite) },
                new() { keyCode = KeyCode.JoystickButton0, sprite = GetSprite("Joystick Button 0", sprite) },
                new() { keyCode = KeyCode.JoystickButton1, sprite = GetSprite("Joystick Button 1", sprite) },
                new() { keyCode = KeyCode.JoystickButton2, sprite = GetSprite("Joystick Button 2", sprite) },
                new() { keyCode = KeyCode.JoystickButton3, sprite = GetSprite("Joystick Button 3", sprite) },
                new() { keyCode = KeyCode.JoystickButton4, sprite = GetSprite("Joystick Button 4", sprite) },
                new() { keyCode = KeyCode.JoystickButton5, sprite = GetSprite("Joystick Button 5", sprite) },
                new() { keyCode = KeyCode.JoystickButton6, sprite = GetSprite("Joystick Button 6", sprite) },
                new() { keyCode = KeyCode.JoystickButton7, sprite = GetSprite("Joystick Button 7", sprite) },
                new() { keyCode = KeyCode.JoystickButton8, sprite = GetSprite("Joystick Button 8", sprite) },
                new() { keyCode = KeyCode.JoystickButton9, sprite = GetSprite("Joystick Button 9", sprite) },
                new() { keyCode = KeyCode.JoystickButton10, sprite = GetSprite("Joystick Button 10", sprite) },
                new() { keyCode = KeyCode.JoystickButton11, sprite = GetSprite("Joystick Button 11", sprite) },
                new() { keyCode = KeyCode.JoystickButton12, sprite = GetSprite("Joystick Button 12", sprite) },
                new() { keyCode = KeyCode.JoystickButton13, sprite = GetSprite("Joystick Button 13", sprite) },
                new() { keyCode = KeyCode.JoystickButton14, sprite = GetSprite("Joystick Button 14", sprite) },
                new() { keyCode = KeyCode.JoystickButton15, sprite = GetSprite("Joystick Button 15", sprite) },
                new() { keyCode = KeyCode.JoystickButton16, sprite = GetSprite("Joystick Button 16", sprite) },
                new() { keyCode = KeyCode.JoystickButton17, sprite = GetSprite("Joystick Button 17", sprite) },
                new() { keyCode = KeyCode.JoystickButton18, sprite = GetSprite("Joystick Button 18", sprite) },
                new() { keyCode = KeyCode.JoystickButton19, sprite = GetSprite("Joystick Button 19", sprite) },
                new() { keyCode = KeyCode.Joystick1Button0, sprite = GetSprite("Joystick 1 Button 0", sprite) },
                new() { keyCode = KeyCode.Joystick1Button1, sprite = GetSprite("Joystick 1 Button 1", sprite) },
                new() { keyCode = KeyCode.Joystick1Button2, sprite = GetSprite("Joystick 1 Button 2", sprite) },
                new() { keyCode = KeyCode.Joystick1Button3, sprite = GetSprite("Joystick 1 Button 3", sprite) },
                new() { keyCode = KeyCode.Joystick1Button4, sprite = GetSprite("Joystick 1 Button 4", sprite) },
                new() { keyCode = KeyCode.Joystick1Button5, sprite = GetSprite("Joystick 1 Button 5", sprite) },
                new() { keyCode = KeyCode.Joystick1Button6, sprite = GetSprite("Joystick 1 Button 6", sprite) },
                new() { keyCode = KeyCode.Joystick1Button7, sprite = GetSprite("Joystick 1 Button 7", sprite) },
                new() { keyCode = KeyCode.Joystick1Button8, sprite = GetSprite("Joystick 1 Button 8", sprite) },
                new() { keyCode = KeyCode.Joystick1Button9, sprite = GetSprite("Joystick 1 Button 9", sprite) },
                new() { keyCode = KeyCode.Joystick1Button10, sprite = GetSprite("Joystick 1 Button 10", sprite) },
                new() { keyCode = KeyCode.Joystick1Button11, sprite = GetSprite("Joystick 1 Button 11", sprite) },
                new() { keyCode = KeyCode.Joystick1Button12, sprite = GetSprite("Joystick 1 Button 12", sprite) },
                new() { keyCode = KeyCode.Joystick1Button13, sprite = GetSprite("Joystick 1 Button 13", sprite) },
                new() { keyCode = KeyCode.Joystick1Button14, sprite = GetSprite("Joystick 1 Button 14", sprite) },
                new() { keyCode = KeyCode.Joystick1Button15, sprite = GetSprite("Joystick 1 Button 15", sprite) },
                new() { keyCode = KeyCode.Joystick1Button16, sprite = GetSprite("Joystick 1 Button 16", sprite) },
                new() { keyCode = KeyCode.Joystick1Button17, sprite = GetSprite("Joystick 1 Button 17", sprite) },
                new() { keyCode = KeyCode.Joystick1Button18, sprite = GetSprite("Joystick 1 Button 18", sprite) },
                new() { keyCode = KeyCode.Joystick1Button19, sprite = GetSprite("Joystick 1 Button 19", sprite) },
                new() { keyCode = KeyCode.Joystick2Button0, sprite = GetSprite("Joystick 2 Button 0", sprite) },
                new() { keyCode = KeyCode.Joystick2Button1, sprite = GetSprite("Joystick 2 Button 1", sprite) },
                new() { keyCode = KeyCode.Joystick2Button2, sprite = GetSprite("Joystick 2 Button 2", sprite) },
                new() { keyCode = KeyCode.Joystick2Button3, sprite = GetSprite("Joystick 2 Button 3", sprite) },
                new() { keyCode = KeyCode.Joystick2Button4, sprite = GetSprite("Joystick 2 Button 4", sprite) },
                new() { keyCode = KeyCode.Joystick2Button5, sprite = GetSprite("Joystick 2 Button 5", sprite) },
                new() { keyCode = KeyCode.Joystick2Button6, sprite = GetSprite("Joystick 2 Button 6", sprite) },
                new() { keyCode = KeyCode.Joystick2Button7, sprite = GetSprite("Joystick 2 Button 7", sprite) },
                new() { keyCode = KeyCode.Joystick2Button8, sprite = GetSprite("Joystick 2 Button 8", sprite) },
                new() { keyCode = KeyCode.Joystick2Button9, sprite = GetSprite("Joystick 2 Button 9", sprite) },
                new() { keyCode = KeyCode.Joystick2Button10, sprite = GetSprite("Joystick 2 Button 10", sprite) },
                new() { keyCode = KeyCode.Joystick2Button11, sprite = GetSprite("Joystick 2 Button 11", sprite) },
                new() { keyCode = KeyCode.Joystick2Button12, sprite = GetSprite("Joystick 2 Button 12", sprite) },
                new() { keyCode = KeyCode.Joystick2Button13, sprite = GetSprite("Joystick 2 Button 13", sprite) },
                new() { keyCode = KeyCode.Joystick2Button14, sprite = GetSprite("Joystick 2 Button 14", sprite) },
                new() { keyCode = KeyCode.Joystick2Button15, sprite = GetSprite("Joystick 2 Button 15", sprite) },
                new() { keyCode = KeyCode.Joystick2Button16, sprite = GetSprite("Joystick 2 Button 16", sprite) },
                new() { keyCode = KeyCode.Joystick2Button17, sprite = GetSprite("Joystick 2 Button 17", sprite) },
                new() { keyCode = KeyCode.Joystick2Button18, sprite = GetSprite("Joystick 2 Button 18", sprite) },
                new() { keyCode = KeyCode.Joystick2Button19, sprite = GetSprite("Joystick 2 Button 19", sprite) },
                new() { keyCode = KeyCode.Joystick3Button0, sprite = GetSprite("Joystick 3 Button 0", sprite) },
                new() { keyCode = KeyCode.Joystick3Button1, sprite = GetSprite("Joystick 3 Button 1", sprite) },
                new() { keyCode = KeyCode.Joystick3Button2, sprite = GetSprite("Joystick 3 Button 2", sprite) },
                new() { keyCode = KeyCode.Joystick3Button3, sprite = GetSprite("Joystick 3 Button 3", sprite) },
                new() { keyCode = KeyCode.Joystick3Button4, sprite = GetSprite("Joystick 3 Button 4", sprite) },
                new() { keyCode = KeyCode.Joystick3Button5, sprite = GetSprite("Joystick 3 Button 5", sprite) },
                new() { keyCode = KeyCode.Joystick3Button6, sprite = GetSprite("Joystick 3 Button 6", sprite) },
                new() { keyCode = KeyCode.Joystick3Button7, sprite = GetSprite("Joystick 3 Button 7", sprite) },
                new() { keyCode = KeyCode.Joystick3Button8, sprite = GetSprite("Joystick 3 Button 8", sprite) },
                new() { keyCode = KeyCode.Joystick3Button9, sprite = GetSprite("Joystick 3 Button 9", sprite) },
                new() { keyCode = KeyCode.Joystick3Button10, sprite = GetSprite("Joystick 3 Button 10", sprite) },
                new() { keyCode = KeyCode.Joystick3Button11, sprite = GetSprite("Joystick 3 Button 11", sprite) },
                new() { keyCode = KeyCode.Joystick3Button12, sprite = GetSprite("Joystick 3 Button 12", sprite) },
                new() { keyCode = KeyCode.Joystick3Button13, sprite = GetSprite("Joystick 3 Button 13", sprite) },
                new() { keyCode = KeyCode.Joystick3Button14, sprite = GetSprite("Joystick 3 Button 14", sprite) },
                new() { keyCode = KeyCode.Joystick3Button15, sprite = GetSprite("Joystick 3 Button 15", sprite) },
                new() { keyCode = KeyCode.Joystick3Button16, sprite = GetSprite("Joystick 3 Button 16", sprite) },
                new() { keyCode = KeyCode.Joystick3Button17, sprite = GetSprite("Joystick 3 Button 17", sprite) },
                new() { keyCode = KeyCode.Joystick3Button18, sprite = GetSprite("Joystick 3 Button 18", sprite) },
                new() { keyCode = KeyCode.Joystick3Button19, sprite = GetSprite("Joystick 3 Button 19", sprite) },
                new() { keyCode = KeyCode.Joystick4Button0, sprite = GetSprite("Joystick 4 Button 0", sprite) },
                new() { keyCode = KeyCode.Joystick4Button1, sprite = GetSprite("Joystick 4 Button 1", sprite) },
                new() { keyCode = KeyCode.Joystick4Button2, sprite = GetSprite("Joystick 4 Button 2", sprite) },
                new() { keyCode = KeyCode.Joystick4Button3, sprite = GetSprite("Joystick 4 Button 3", sprite) },
                new() { keyCode = KeyCode.Joystick4Button4, sprite = GetSprite("Joystick 4 Button 4", sprite) },
                new() { keyCode = KeyCode.Joystick4Button5, sprite = GetSprite("Joystick 4 Button 5", sprite) },
                new() { keyCode = KeyCode.Joystick4Button6, sprite = GetSprite("Joystick 4 Button 6", sprite) },
                new() { keyCode = KeyCode.Joystick4Button7, sprite = GetSprite("Joystick 4 Button 7", sprite) },
                new() { keyCode = KeyCode.Joystick4Button8, sprite = GetSprite("Joystick 4 Button 8", sprite) },
                new() { keyCode = KeyCode.Joystick4Button9, sprite = GetSprite("Joystick 4 Button 9", sprite) },
                new() { keyCode = KeyCode.Joystick4Button10, sprite = GetSprite("Joystick 4 Button 10", sprite) },
                new() { keyCode = KeyCode.Joystick4Button11, sprite = GetSprite("Joystick 4 Button 11", sprite) },
                new() { keyCode = KeyCode.Joystick4Button12, sprite = GetSprite("Joystick 4 Button 12", sprite) },
                new() { keyCode = KeyCode.Joystick4Button13, sprite = GetSprite("Joystick 4 Button 13", sprite) },
                new() { keyCode = KeyCode.Joystick4Button14, sprite = GetSprite("Joystick 4 Button 14", sprite) },
                new() { keyCode = KeyCode.Joystick4Button15, sprite = GetSprite("Joystick 4 Button 15", sprite) },
                new() { keyCode = KeyCode.Joystick4Button16, sprite = GetSprite("Joystick 4 Button 16", sprite) },
                new() { keyCode = KeyCode.Joystick4Button17, sprite = GetSprite("Joystick 4 Button 17", sprite) },
                new() { keyCode = KeyCode.Joystick4Button18, sprite = GetSprite("Joystick 4 Button 18", sprite) },
                new() { keyCode = KeyCode.Joystick4Button19, sprite = GetSprite("Joystick 4 Button 19", sprite) },
                new() { keyCode = KeyCode.Joystick5Button0, sprite = GetSprite("Joystick 5 Button 0", sprite) },
                new() { keyCode = KeyCode.Joystick5Button1, sprite = GetSprite("Joystick 5 Button 1", sprite) },
                new() { keyCode = KeyCode.Joystick5Button2, sprite = GetSprite("Joystick 5 Button 2", sprite) },
                new() { keyCode = KeyCode.Joystick5Button3, sprite = GetSprite("Joystick 5 Button 3", sprite) },
                new() { keyCode = KeyCode.Joystick5Button4, sprite = GetSprite("Joystick 5 Button 4", sprite) },
                new() { keyCode = KeyCode.Joystick5Button5, sprite = GetSprite("Joystick 5 Button 5", sprite) },
                new() { keyCode = KeyCode.Joystick5Button6, sprite = GetSprite("Joystick 5 Button 6", sprite) },
                new() { keyCode = KeyCode.Joystick5Button7, sprite = GetSprite("Joystick 5 Button 7", sprite) },
                new() { keyCode = KeyCode.Joystick5Button8, sprite = GetSprite("Joystick 5 Button 8", sprite) },
                new() { keyCode = KeyCode.Joystick5Button9, sprite = GetSprite("Joystick 5 Button 9", sprite) },
                new() { keyCode = KeyCode.Joystick5Button10, sprite = GetSprite("Joystick 5 Button 10", sprite) },
                new() { keyCode = KeyCode.Joystick5Button11, sprite = GetSprite("Joystick 5 Button 11", sprite) },
                new() { keyCode = KeyCode.Joystick5Button12, sprite = GetSprite("Joystick 5 Button 12", sprite) },
                new() { keyCode = KeyCode.Joystick5Button13, sprite = GetSprite("Joystick 5 Button 13", sprite) },
                new() { keyCode = KeyCode.Joystick5Button14, sprite = GetSprite("Joystick 5 Button 14", sprite) },
                new() { keyCode = KeyCode.Joystick5Button15, sprite = GetSprite("Joystick 5 Button 15", sprite) },
                new() { keyCode = KeyCode.Joystick5Button16, sprite = GetSprite("Joystick 5 Button 16", sprite) },
                new() { keyCode = KeyCode.Joystick5Button17, sprite = GetSprite("Joystick 5 Button 17", sprite) },
                new() { keyCode = KeyCode.Joystick5Button18, sprite = GetSprite("Joystick 5 Button 18", sprite) },
                new() { keyCode = KeyCode.Joystick5Button19, sprite = GetSprite("Joystick 5 Button 19", sprite) },
                new() { keyCode = KeyCode.Joystick6Button0, sprite = GetSprite("Joystick 6 Button 0", sprite) },
                new() { keyCode = KeyCode.Joystick6Button1, sprite = GetSprite("Joystick 6 Button 1", sprite) },
                new() { keyCode = KeyCode.Joystick6Button2, sprite = GetSprite("Joystick 6 Button 2", sprite) },
                new() { keyCode = KeyCode.Joystick6Button3, sprite = GetSprite("Joystick 6 Button 3", sprite) },
                new() { keyCode = KeyCode.Joystick6Button4, sprite = GetSprite("Joystick 6 Button 4", sprite) },
                new() { keyCode = KeyCode.Joystick6Button5, sprite = GetSprite("Joystick 6 Button 5", sprite) },
                new() { keyCode = KeyCode.Joystick6Button6, sprite = GetSprite("Joystick 6 Button 6", sprite) },
                new() { keyCode = KeyCode.Joystick6Button7, sprite = GetSprite("Joystick 6 Button 7", sprite) },
                new() { keyCode = KeyCode.Joystick6Button8, sprite = GetSprite("Joystick 6 Button 8", sprite) },
                new() { keyCode = KeyCode.Joystick6Button9, sprite = GetSprite("Joystick 6 Button 9", sprite) },
                new() { keyCode = KeyCode.Joystick6Button10, sprite = GetSprite("Joystick 6 Button 10", sprite) },
                new() { keyCode = KeyCode.Joystick6Button11, sprite = GetSprite("Joystick 6 Button 11", sprite) },
                new() { keyCode = KeyCode.Joystick6Button12, sprite = GetSprite("Joystick 6 Button 12", sprite) },
                new() { keyCode = KeyCode.Joystick6Button13, sprite = GetSprite("Joystick 6 Button 13", sprite) },
                new() { keyCode = KeyCode.Joystick6Button14, sprite = GetSprite("Joystick 6 Button 14", sprite) },
                new() { keyCode = KeyCode.Joystick6Button15, sprite = GetSprite("Joystick 6 Button 15", sprite) },
                new() { keyCode = KeyCode.Joystick6Button16, sprite = GetSprite("Joystick 6 Button 16", sprite) },
                new() { keyCode = KeyCode.Joystick6Button17, sprite = GetSprite("Joystick 6 Button 17", sprite) },
                new() { keyCode = KeyCode.Joystick6Button18, sprite = GetSprite("Joystick 6 Button 18", sprite) },
                new() { keyCode = KeyCode.Joystick6Button19, sprite = GetSprite("Joystick 6 Button 19", sprite) },
                new() { keyCode = KeyCode.Joystick7Button0, sprite = GetSprite("Joystick 7 Button 0", sprite) },
                new() { keyCode = KeyCode.Joystick7Button1, sprite = GetSprite("Joystick 7 Button 1", sprite) },
                new() { keyCode = KeyCode.Joystick7Button2, sprite = GetSprite("Joystick 7 Button 2", sprite) },
                new() { keyCode = KeyCode.Joystick7Button3, sprite = GetSprite("Joystick 7 Button 3", sprite) },
                new() { keyCode = KeyCode.Joystick7Button4, sprite = GetSprite("Joystick 7 Button 4", sprite) },
                new() { keyCode = KeyCode.Joystick7Button5, sprite = GetSprite("Joystick 7 Button 5", sprite) },
                new() { keyCode = KeyCode.Joystick7Button6, sprite = GetSprite("Joystick 7 Button 6", sprite) },
                new() { keyCode = KeyCode.Joystick7Button7, sprite = GetSprite("Joystick 7 Button 7", sprite) },
                new() { keyCode = KeyCode.Joystick7Button8, sprite = GetSprite("Joystick 7 Button 8", sprite) },
                new() { keyCode = KeyCode.Joystick7Button9, sprite = GetSprite("Joystick 7 Button 9", sprite) },
                new() { keyCode = KeyCode.Joystick7Button10, sprite = GetSprite("Joystick 7 Button 10", sprite) },
                new() { keyCode = KeyCode.Joystick7Button11, sprite = GetSprite("Joystick 7 Button 11", sprite) },
                new() { keyCode = KeyCode.Joystick7Button12, sprite = GetSprite("Joystick 7 Button 12", sprite) },
                new() { keyCode = KeyCode.Joystick7Button13, sprite = GetSprite("Joystick 7 Button 13", sprite) },
                new() { keyCode = KeyCode.Joystick7Button14, sprite = GetSprite("Joystick 7 Button 14", sprite) },
                new() { keyCode = KeyCode.Joystick7Button15, sprite = GetSprite("Joystick 7 Button 15", sprite) },
                new() { keyCode = KeyCode.Joystick7Button16, sprite = GetSprite("Joystick 7 Button 16", sprite) },
                new() { keyCode = KeyCode.Joystick7Button17, sprite = GetSprite("Joystick 7 Button 17", sprite) },
                new() { keyCode = KeyCode.Joystick7Button18, sprite = GetSprite("Joystick 7 Button 18", sprite) },
                new() { keyCode = KeyCode.Joystick7Button19, sprite = GetSprite("Joystick 7 Button 19", sprite) },
                new() { keyCode = KeyCode.Joystick8Button0, sprite = GetSprite("Joystick 8 Button 0", sprite) },
                new() { keyCode = KeyCode.Joystick8Button1, sprite = GetSprite("Joystick 8 Button 1", sprite) },
                new() { keyCode = KeyCode.Joystick8Button2, sprite = GetSprite("Joystick 8 Button 2", sprite) },
                new() { keyCode = KeyCode.Joystick8Button3, sprite = GetSprite("Joystick 8 Button 3", sprite) },
                new() { keyCode = KeyCode.Joystick8Button4, sprite = GetSprite("Joystick 8 Button 4", sprite) },
                new() { keyCode = KeyCode.Joystick8Button5, sprite = GetSprite("Joystick 8 Button 5", sprite) },
                new() { keyCode = KeyCode.Joystick8Button6, sprite = GetSprite("Joystick 8 Button 6", sprite) },
                new() { keyCode = KeyCode.Joystick8Button7, sprite = GetSprite("Joystick 8 Button 7", sprite) },
                new() { keyCode = KeyCode.Joystick8Button8, sprite = GetSprite("Joystick 8 Button 8", sprite) },
                new() { keyCode = KeyCode.Joystick8Button9, sprite = GetSprite("Joystick 8 Button 9", sprite) },
                new() { keyCode = KeyCode.Joystick8Button10, sprite = GetSprite("Joystick 8 Button 10", sprite) },
                new() { keyCode = KeyCode.Joystick8Button11, sprite = GetSprite("Joystick 8 Button 11", sprite) },
                new() { keyCode = KeyCode.Joystick8Button12, sprite = GetSprite("Joystick 8 Button 12", sprite) },
                new() { keyCode = KeyCode.Joystick8Button13, sprite = GetSprite("Joystick 8 Button 13", sprite) },
                new() { keyCode = KeyCode.Joystick8Button14, sprite = GetSprite("Joystick 8 Button 14", sprite) },
                new() { keyCode = KeyCode.Joystick8Button15, sprite = GetSprite("Joystick 8 Button 15", sprite) },
                new() { keyCode = KeyCode.Joystick8Button16, sprite = GetSprite("Joystick 8 Button 16", sprite) },
                new() { keyCode = KeyCode.Joystick8Button17, sprite = GetSprite("Joystick 8 Button 17", sprite) },
                new() { keyCode = KeyCode.Joystick8Button18, sprite = GetSprite("Joystick 8 Button 18", sprite) },
                new() { keyCode = KeyCode.Joystick8Button19, sprite = GetSprite("Joystick 8 Button 19", sprite) }
            };

            script.KeyCodes.AddRange(keyCodeList); // Adds the generated keyboard key mappings to the ScriptableObject.
        }

        /// <summary>
        /// Attempts to find and return a Sprite asset by name.
        /// If the named sprite is not found, returns the provided default sprite.
        /// </summary>
        /// <param name="spriteName">Name of the sprite to search for.</param>
        /// <param name="defaultSprite">Fallback sprite to return if not found.</param>
        /// <returns>The matching sprite if found, or the default sprite otherwise.</returns>
        private static Sprite GetSprite(string spriteName, Sprite defaultSprite)
        {
            var guids = AssetDatabase.FindAssets("t:Sprite"); // Find all assets of type Sprite in the project.

            foreach (var guid in guids)
            {
                // Convert GUID to the asset path.
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // Attempt to get the TextureImporter for the asset.
                var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                // Proceed only if it's a Sprite type asset.
                if (textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite)
                {
                    // Load all sprite sub-assets from the file.
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToArray();

                    foreach (var sprite in sprites)
                    {
                        // Check for matching sprite name.
                        if (sprite.name == spriteName)
                        {
                            Debug.Log($"Sprite Found [{sprite.name}]");
                            return sprite;
                        }
                    }
                }
            }

            return defaultSprite; // If no matching sprite is found, return the fallback.
        }
    }
}