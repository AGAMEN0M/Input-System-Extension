/*
 * ---------------------------------------------------------------------------
 * Description: Debug script that logs both the legacy Input (KeyCode) system 
 *              and the new Input System paths side-by-side for any key pressed.
 *              It also verifies if each key/path exists in 
 *              InputSystemExtensionData.KeyCodes and displays its index and sprite status.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using InputSystemExtension;
using System.Linq;
using UnityEngine;
using System;

[AddComponentMenu("UI/Input System Extension/Debug/Input Debug Logger", 1)]
public class InputDebugLogger : MonoBehaviour
{
    #region === Manual Map ===

    /// <summary>
    /// Manual mapping for keys that do not have direct one-to-one name correspondence
    /// between the legacy Input (KeyCode) and the new Input System path.
    /// </summary>
    private static readonly Dictionary<KeyCode, string> ManualMap = new()
    {
        { KeyCode.LeftControl, "<Keyboard>/leftCtrl" },
        { KeyCode.RightControl, "<Keyboard>/rightCtrl" },
        { KeyCode.LeftShift, "<Keyboard>/leftShift" },
        { KeyCode.RightShift, "<Keyboard>/rightShift" },
        { KeyCode.LeftAlt, "<Keyboard>/leftAlt" },
        { KeyCode.RightAlt, "<Keyboard>/rightAlt" },
        { KeyCode.Return, "<Keyboard>/enter" },
        { KeyCode.KeypadEnter, "<Keyboard>/numpadenter" },

        { KeyCode.BackQuote, "<Keyboard>/backquote" },
        { KeyCode.Backslash, "<Keyboard>/backslash" },
        { KeyCode.Slash, "<Keyboard>/slash" },
        { KeyCode.Semicolon, "<Keyboard>/semicolon" },
        { KeyCode.Comma, "<Keyboard>/comma" },
        { KeyCode.Period, "<Keyboard>/period" },
        { KeyCode.Quote, "<Keyboard>/quote" },
        { KeyCode.Equals, "<Keyboard>/equals" },
        { KeyCode.Minus, "<Keyboard>/minus" },
        { KeyCode.Space, "<Keyboard>/space" },
        { KeyCode.Tab, "<Keyboard>/tab" },
        { KeyCode.Escape, "<Keyboard>/escape" },
        { KeyCode.Delete, "<Keyboard>/delete" },
        { KeyCode.Insert, "<Keyboard>/insert" },
        { KeyCode.Home, "<Keyboard>/home" },
        { KeyCode.End, "<Keyboard>/end" },
        { KeyCode.PageUp, "<Keyboard>/pageUp" },
        { KeyCode.PageDown, "<Keyboard>/pageDown" },
        { KeyCode.CapsLock, "<Keyboard>/capsLock" },
        { KeyCode.Numlock, "<Keyboard>/numLock" },

        // Extra mouse buttons (if you want to detect via legacy KeyCode).
        { KeyCode.Mouse0, "<Mouse>/leftButton" },
        { KeyCode.Mouse1, "<Mouse>/rightButton" },
        { KeyCode.Mouse2, "<Mouse>/middleButton" },
        { KeyCode.Mouse3, "<Mouse>/forwardButton" },
        { KeyCode.Mouse4, "<Mouse>/backButton" },
        { KeyCode.Mouse5, "<Mouse>/extraButton1" },
        { KeyCode.Mouse6, "<Mouse>/extraButton2" }
    };

    #endregion

    #region === Fields ===

    private InputSystemExtensionData data; // Reference to the InputSystemExtensionData object.

    #endregion

    #region === Unity Methods ===

    private void Start()
    {
        // Retrieve the global InputSystemExtensionData instance for KeyCode lookups.
        data = InputSystemExtensionHelper.GetInputSystemExtensionData();

        // If data is missing, warn the developer but allow script to continue running.
        if (data == null)
        {
            Debug.LogWarning("[InputDebugLogger] InputSystemExtensionData not found (data == null). Index checks will be skipped.", this);
        }
    }

    private void Update()
    {
        // Step 1: Collect new Input System pressed paths for this frame.
        var pressedNewPaths = CollectPressedNewInputPaths();

        // Step 2: Process legacy KeyCode presses and log corresponding new paths.
        ProcessLegacyInputKeys(pressedNewPaths);

        // Step 3: Log keys detected only by the new Input System.
        LogNewOnlyInputs(pressedNewPaths);
    }

    #endregion

    #region === Input Collection ===

    /// <summary>
    /// Collects all new Input System paths pressed this frame from keyboard, mouse, and gamepad.
    /// </summary>
    private static List<string> CollectPressedNewInputPaths()
    {
        var pressedNewPaths = new List<string>();

        // Collect keyboard inputs.
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            foreach (var k in keyboard.allKeys)
            {
                if (k != null && k.wasPressedThisFrame)
                    pressedNewPaths.Add(k.path); // Example: "<Keyboard>/a".
            }
        }

        // Collect mouse inputs.
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame) pressedNewPaths.Add("<Mouse>/leftButton");
            if (mouse.rightButton.wasPressedThisFrame) pressedNewPaths.Add("<Mouse>/rightButton");
            if (mouse.middleButton.wasPressedThisFrame) pressedNewPaths.Add("<Mouse>/middleButton");
            if (mouse.forwardButton != null && mouse.forwardButton.wasPressedThisFrame) pressedNewPaths.Add("<Mouse>/forwardButton");
            if (mouse.backButton != null && mouse.backButton.wasPressedThisFrame) pressedNewPaths.Add("<Mouse>/backButton");
        }

        // Collect gamepad inputs.
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame) pressedNewPaths.Add("<Gamepad>/buttonSouth");
            if (gamepad.buttonNorth.wasPressedThisFrame) pressedNewPaths.Add("<Gamepad>/buttonNorth");
            if (gamepad.buttonEast.wasPressedThisFrame) pressedNewPaths.Add("<Gamepad>/buttonEast");
            if (gamepad.buttonWest.wasPressedThisFrame) pressedNewPaths.Add("<Gamepad>/buttonWest");
            if (gamepad.dpad.up.wasPressedThisFrame) pressedNewPaths.Add("<Gamepad>/dpad/up");
            if (gamepad.dpad.down.wasPressedThisFrame) pressedNewPaths.Add("<Gamepad>/dpad/down");
            if (gamepad.dpad.left.wasPressedThisFrame) pressedNewPaths.Add("<Gamepad>/dpad/left");
            if (gamepad.dpad.right.wasPressedThisFrame) pressedNewPaths.Add("<Gamepad>/dpad/right");
        }

        return pressedNewPaths;
    }

    #endregion

    #region === Input Logging ===

    /// <summary>
    /// Processes legacy Input.GetKeyDown events, finds corresponding InputSystem paths,
    /// and logs detailed information about mapping, index, and sprite status.
    /// </summary>
    private void ProcessLegacyInputKeys(List<string> pressedNewPaths)
    {
        foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
        {
            try
            {
                if (!Input.GetKeyDown(kc))
                    continue;

                string newPath = ResolveNewPathForKey(kc, pressedNewPaths);

                // Prepare index and sprite lookup data.
                int indexByKeyCode = -1;
                int indexByName = -1;
                string foundName = null;
                bool hasSpriteByKeyCode = false;
                bool hasSpriteByName = false;

                if (data != null && data.KeyCodes != null)
                {
                    indexByKeyCode = data.KeyCodes.FindIndex(k => k.keyCode == kc);

                    if (!string.IsNullOrEmpty(newPath))
                        indexByName = data.KeyCodes.FindIndex(k => string.Equals(k.keyName, newPath, StringComparison.OrdinalIgnoreCase));

                    if (indexByKeyCode >= 0)
                    {
                        foundName = data.KeyCodes[indexByKeyCode].keyName;
                        hasSpriteByKeyCode = data.KeyCodes[indexByKeyCode].sprite != null;
                    }

                    if (indexByName >= 0)
                        hasSpriteByName = data.KeyCodes[indexByName].sprite != null;
                }

                // Compose log message.
                string msg = $"[Pressed] KeyCode: {kc} | New InputSystem: {newPath}";
                if (data == null)
                {
                    msg += " | KeyCodes: data==null (skipped)";
                }
                else
                {
                    msg += $" | idxByKeyCode: {indexByKeyCode}" +
                           $" | idxByName: {indexByName}" +
                           $" | keyNameAtIdxByKeyCode: {(foundName ?? "n/a")}" +
                           $" | spriteAtIdxByKeyCode: {(indexByKeyCode >= 0 ? (hasSpriteByKeyCode ? "yes" : "no") : "n/a")}" +
                           $" | spriteAtIdxByName: {(indexByName >= 0 ? (hasSpriteByName ? "yes" : "no") : "n/a")}";
                }

                Debug.Log(msg, this);
            }
            catch (Exception ex)
            {
                // Protects against rare KeyCode exceptions (e.g., invalid KeyCode states).
                Debug.LogWarning($"[InputDebugLogger] Exception checking KeyCode {kc}: {ex.Message}", this);
            }
        }
    }

    /// <summary>
    /// Logs keys detected only by the new Input System (not detected by legacy Input).
    /// </summary>
    private void LogNewOnlyInputs(IEnumerable<string> pressedNewPaths)
    {
        foreach (var p in pressedNewPaths.Distinct())
        {
            if (data != null && data.KeyCodes != null)
            {
                int idx = data.KeyCodes.FindIndex(k => string.Equals(k.keyName, p, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                {
                    Debug.Log($"[New Only] Path pressed: {p} | found in KeyCodes at index {idx} | has sprite: {(data.KeyCodes[idx].sprite != null ? "yes" : "no")}.", this);
                    continue;
                }
            }

            Debug.Log($"[New Only] Path pressed: {p} | not present in extensionData.KeyCodes.", this);
        }
    }

    #endregion

    #region === Path Resolution ===

    /// <summary>
    /// Resolves the best matching InputSystem path for a given KeyCode using:
    /// 1) manual mapping,
    /// 2) simple pattern rules (letters, numbers, keypad, function keys),
    /// 3) heuristic matching based on paths pressed this frame.
    /// </summary>
    private string ResolveNewPathForKey(KeyCode kc, List<string> pressedNewPaths)
    {
        // 1) Manual mapping.
        if (ManualMap.TryGetValue(kc, out var mapped))
        {
            if (mapped.StartsWith("<Keyboard>/", StringComparison.OrdinalIgnoreCase) && Keyboard.current != null)
            {
                string child = mapped["<Keyboard>/".Length..];
                var ctrl = Keyboard.current.TryGetChildControl<KeyControl>(child);
                if (ctrl != null) return mapped;
            }
            else return mapped;
        }

        // 2) Letter keys (A-Z).
        if (kc >= KeyCode.A && kc <= KeyCode.Z)
        {
            string letter = kc.ToString().ToLowerInvariant();
            return $"<Keyboard>/{letter}";
        }

        // 3) Top-row number keys (Alpha0 - Alpha9).
        if (kc >= KeyCode.Alpha0 && kc <= KeyCode.Alpha9)
        {
            int digit = (int)kc - (int)KeyCode.Alpha0;
            return $"<Keyboard>/{digit}";
        }

        // 4) Numpad keys (Keypad0 - Keypad9).
        if (kc >= KeyCode.Keypad0 && kc <= KeyCode.Keypad9)
        {
            int n = (int)kc - (int)KeyCode.Keypad0;
            return $"<Keyboard>/numpad{n}";
        }

        // 5) Function keys (F1 - F15).
        if (kc >= KeyCode.F1 && kc <= KeyCode.F15)
        {
            string f = kc.ToString().ToLowerInvariant();
            return $"<Keyboard>/{f}";
        }

        // 6) Heuristic: attempt to match pressed new-system paths this frame.
        string normK = NormalizeForMatch(kc.ToString());
        foreach (var p in pressedNewPaths)
        {
            if (string.IsNullOrEmpty(p)) continue;
            string candidate = p.ToLowerInvariant();
            if (candidate.StartsWith("<keyboard>/")) candidate = candidate["<keyboard>/".Length..];
            if (candidate.StartsWith("<mouse>/")) candidate = candidate["<mouse>/".Length..];
            if (candidate.StartsWith("<gamepad>/")) candidate = candidate["<gamepad>/".Length..];
            candidate = NormalizeForMatch(candidate);

            if (candidate.Contains(normK) || normK.Contains(candidate))
                return p;
        }

        // 7) Fallback when no match is found.
        return "Unknown mapping";
    }

    /// <summary>
    /// Normalizes a string for comparison by lowercasing and removing all non-alphanumeric characters.
    /// </summary>
    private static string NormalizeForMatch(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var chars = s.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c)).ToArray();
        return new string(chars);
    }

    #endregion
}