/*
 * ---------------------------------------------------------------------------
 * Description: Provides static utility methods for detecting input activity
 *              across multiple device types using Unity's new Input System.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;

namespace InputSystemExtension
{
    /// <summary>
    /// Provides static helper methods for detecting input activity
    /// from multiple input devices (keyboard, mouse, gamepad, and touch)
    /// using Unity's new Input System.
    /// </summary>
    public static class InputSystemUtility
    {
        #region === Public Static Methods ===

        /// <summary>
        /// Checks if any input (keyboard, mouse, gamepad, or touch) was pressed this frame.
        /// </summary>
        /// <returns>True if any input was detected this frame; otherwise, false.</returns>
        public static bool IsAnyInputPressedThisFrame()
        {
            // Check each input category individually.
            return IsKeyboardPressed() || IsMousePressed() || IsGamepadPressed() || IsTouchPressed();
        }

        /// <summary>
        /// Checks if any keyboard key was pressed this frame.
        /// </summary>
        /// <returns>True if any keyboard key was pressed; otherwise, false.</returns>
        public static bool IsKeyboardPressed() => Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        /// <summary>
        /// Checks if any mouse button was pressed this frame.
        /// </summary>
        /// <returns>True if any mouse button was pressed; otherwise, false.</returns>
        public static bool IsMousePressed()
        {
            if (Mouse.current == null) return false;

            // Check common mouse buttons (left, right, middle).
            return Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame;
        }

        /// <summary>
        /// Checks if any button on any connected gamepad was pressed this frame.
        /// </summary>
        /// <returns>True if any gamepad button was pressed; otherwise, false.</returns>
        public static bool IsGamepadPressed()
        {
            // Iterate through all connected gamepads.
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad == null) continue;

                // Loop through all control elements (buttons, triggers, etc.)
                foreach (var control in gamepad.allControls)
                {
                    // Check if the control is a button and was pressed this frame.
                    if (control is ButtonControl button && button.wasPressedThisFrame) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if any touch was detected this frame (useful for mobile devices).
        /// </summary>
        /// <returns>True if a touch was pressed this frame; otherwise, false.</returns>
        public static bool IsTouchPressed() => Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        #endregion
    }
}