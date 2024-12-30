/*
 * ---------------------------------------------------------------------------
 * Description: Custom editor for the KeyboardControlData ScriptableObject in Unity.
 *              Provides an intuitive user interface to manage and edit InputData entries, 
 *              including features for key detection, deletion, saving, and asset creation.
 *              Enables the generation of keySprites for specific key codes and customization 
 *              of keySprite lists. The editor also facilitates the saving and renaming of InputData 
 *              assets based on their tags.
 * Author: Lucas Gomes Cecchini.
 * Pseudonym: AGAMENOM.
 * ---------------------------------------------------------------------------
*/

using CustomKeyboard;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

// Custom editor for KeyboardControlData ScriptableObject.
// Provides a user interface to manage InputData entries within the Unity Editor.
[CustomEditor(typeof(KeyboardControlData))]
public class KeyboardControlDataInspector : Editor
{
    private bool isDetectingKey = false; // Indicates if a key detection is in progress.
    private int detectingIndex = -1;     // Index of the InputData being edited for key detection.

    public override void OnInspectorGUI()
    {
        KeyboardControlData script = (KeyboardControlData)target;
        serializedObject.Update();

        script.inputDataList ??= new List<InputData>(); // Ensure the input data list is initialized.

        // Display the script name, but make it read-only.
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(script), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        // Display header for the Keyboard Control Data section.
        EditorGUILayout.LabelField("Keyboard Control Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Input Data List", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        // Display a message if the input data list is empty.
        if (script.inputDataList == null || script.inputDataList.Count == 0)
        {
            EditorGUILayout.LabelField("There are no items in the list", EditorStyles.boldLabel);
        }
        else
        {
            // Iterate through the input data list and display each entry.
            for (int i = 0; i < script.inputDataList.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                // Display and edit the keyboard tag, with a delete button.
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tag:", GUILayout.Width(30));
                script.inputDataList[i].keyboardTag = EditorGUILayout.TextField(script.inputDataList[i].keyboardTag);

                EditorGUILayout.Space(10);

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    // Confirm and delete the selected InputData entry.
                    if (EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete this item?\nIt will not be possible to undo this operation.", "Delete", "Cancel"))
                    {
                        DeleteInputData(script, i);
                        break;
                    }
                    continue;
                }
                GUI.backgroundColor = originalColor;
                EditorGUILayout.EndHorizontal();

                // Display and edit the key code, with a detect button.
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Key:", GUILayout.Width(30));
                if (isDetectingKey && detectingIndex == i)
                {
                    EditorGUILayout.LabelField("Press any key...", GUILayout.Width(100));
                }
                else
                {
                    script.inputDataList[i].keyboard = (KeyCode)EditorGUILayout.EnumPopup(script.inputDataList[i].keyboard);
                }

                EditorGUILayout.Space(10);

                if (GUILayout.Button("Detect", GUILayout.Width(60)))
                {
                    // Start key detection for the selected InputData entry.
                    isDetectingKey = true;
                    detectingIndex = i;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal(); // Buttons for saving all InputData entries and creating a new InputData entry.

        EditorGUI.BeginDisabledGroup(script.inputDataList == null || script.inputDataList.Count == 0);
        if (GUILayout.Button("Save All", GUILayout.Width(120)))
        {
            SaveAllInputData(script);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Create Input Data", GUILayout.Width(120)))
        {
            CreateInputData(script);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Buttons for Generate keyCodesSprites List and Default Sprite.
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate 'key Codes Sprites' List", GUILayout.Width(210)))
        {
            GenerateKeyCodeList(script);
        }

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Default Sprite:", GUILayout.Width(90));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultSprite"), GUIContent.none, GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("keyCodesSprites"));

        // Mark the script as dirty if any changes were made.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
        }

        // Handle key detection if active.
        if (isDetectingKey)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                script.inputDataList[detectingIndex].keyboard = e.keyCode;
                isDetectingKey = false;
                detectingIndex = -1;
                Repaint();
                GUIUtility.ExitGUI();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Delete the InputData asset at the specified index.
    private void DeleteInputData(KeyboardControlData script, int index)
    {
        if (index < 0 || index >= script.inputDataList.Count)
        {
            return;
        }

        InputData inputData = script.inputDataList[index];
        if (inputData != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(inputData);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        script.inputDataList.RemoveAt(index);
        Repaint();
    }

    // Create a new InputData asset and add it to the list.
    private void CreateInputData(KeyboardControlData script)
    {
        string assetPath = AssetDatabase.GetAssetPath(script);
        string directory = Path.GetDirectoryName(assetPath);
        string folderPath = Path.Combine(directory, script.name);

        // Ensure the folder for storing InputData assets exists.
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(directory, script.name);
        }

        InputData newData = CreateInstance<InputData>();
        newData.keyboardTag = "NewTag";
        newData.keyboard = KeyCode.None;

        // Generate a unique file path for the new InputData asset.
        string assetFilePath = Path.Combine(folderPath, $"{newData.keyboardTag} (InputData).asset");
        int counter = 1;
        while (AssetDatabase.LoadAssetAtPath<InputData>(assetFilePath) != null)
        {
            assetFilePath = Path.Combine(folderPath, $"{newData.keyboardTag} ({counter}) (InputData).asset");
            counter++;
        }

        AssetDatabase.CreateAsset(newData, assetFilePath);

        script.inputDataList.Add(newData);
        AssetDatabase.SaveAssets();
        Repaint();
    }

    // Save all InputData assets and rename them based on their tag.
    private void SaveAllInputData(KeyboardControlData script)
    {
        string assetPath = AssetDatabase.GetAssetPath(script);
        string directory = Path.GetDirectoryName(assetPath);
        string folderPath = Path.Combine(directory, script.name);

        // Dictionary to count occurrences of each keyboardTag.
        Dictionary<string, int> tagCounts = new();

        // First step: count how many times each keyboardTag appears.
        foreach (InputData inputData in script.inputDataList)
        {
            if (inputData == null)
            {
                continue;
            }

            if (tagCounts.ContainsKey(inputData.keyboardTag))
            {
                tagCounts[inputData.keyboardTag]++;
            }
            else
            {
                tagCounts[inputData.keyboardTag] = 1;
            }
        }

        // Step Two: Rename the files as needed.
        Dictionary<string, int> currentTagCounter = new();

        foreach (InputData inputData in script.inputDataList)
        {
            if (inputData == null)
            {
                continue;
            }

            EditorUtility.SetDirty(inputData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string oldAssetPath = AssetDatabase.GetAssetPath(inputData);
            string baseFileName = $"{inputData.keyboardTag} (Input Data)";
            string newFileName;

            // If the tag is duplicated, add a counter to the file name.
            if (tagCounts[inputData.keyboardTag] > 1)
            {
                if (!currentTagCounter.ContainsKey(inputData.keyboardTag))
                {
                    currentTagCounter[inputData.keyboardTag] = 1;
                }
                else
                {
                    currentTagCounter[inputData.keyboardTag]++;
                }
                newFileName = $"{baseFileName} ({currentTagCounter[inputData.keyboardTag]}).asset";
            }
            else
            {
                newFileName = $"{baseFileName}.asset";
            }

            string newAssetPath = Path.Combine(folderPath, newFileName);

            // Move the asset only if the new path is different from the old one.
            if (oldAssetPath != newAssetPath)
            {
                AssetDatabase.MoveAsset(oldAssetPath, newAssetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Repaint();
    }

    // Method to populate the keyCodesSprites list with InputSpriteList entries for various KeyCode values.
    public void GenerateKeyCodeList(KeyboardControlData script)
    {
        script.keyCodesSprites.Clear(); // Clears any existing entries in the keyCodesSprites list.

        // Defines a list of InputSpriteList objects, each representing a KeyCode and an associated sprite.
        // Uses the GetSprite method to retrieve sprites based on the spriteName or defaultSprite if not found.
        var keyCodeList = new List<InputSpriteList>
        {
            new() { keyCode = KeyCode.None, sprite = GetSprite("None", script.defaultSprite) },
            new() { keyCode = KeyCode.Backspace, sprite = GetSprite("Backspace", script.defaultSprite) },
            new() { keyCode = KeyCode.Delete, sprite = GetSprite("Delete", script.defaultSprite) },
            new() { keyCode = KeyCode.Tab, sprite = GetSprite("Tab", script.defaultSprite) },
            new() { keyCode = KeyCode.Clear, sprite = GetSprite("Clear", script.defaultSprite) },
            new() { keyCode = KeyCode.Return, sprite = GetSprite("Return", script.defaultSprite) },
            new() { keyCode = KeyCode.Pause, sprite = GetSprite("Pause", script.defaultSprite) },
            new() { keyCode = KeyCode.Escape, sprite = GetSprite("Escape", script.defaultSprite) },
            new() { keyCode = KeyCode.Space, sprite = GetSprite("Space", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad0, sprite = GetSprite("Keypad0", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad1, sprite = GetSprite("Keypad1", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad2, sprite = GetSprite("Keypad2", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad3, sprite = GetSprite("Keypad3", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad4, sprite = GetSprite("Keypad4", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad5, sprite = GetSprite("Keypad5", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad6, sprite = GetSprite("Keypad6", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad7, sprite = GetSprite("Keypad7", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad8, sprite = GetSprite("Keypad8", script.defaultSprite) },
            new() { keyCode = KeyCode.Keypad9, sprite = GetSprite("Keypad9", script.defaultSprite) },
            new() { keyCode = KeyCode.KeypadPeriod, sprite = GetSprite("Keypad Period", script.defaultSprite) },
            new() { keyCode = KeyCode.KeypadDivide, sprite = GetSprite("Keypad Divide", script.defaultSprite) },
            new() { keyCode = KeyCode.KeypadMultiply, sprite = GetSprite("Keypad Multiply", script.defaultSprite) },
            new() { keyCode = KeyCode.KeypadMinus, sprite = GetSprite("Keypad Minus", script.defaultSprite) },
            new() { keyCode = KeyCode.KeypadPlus, sprite = GetSprite("Keypad Plus", script.defaultSprite) },
            new() { keyCode = KeyCode.KeypadEnter, sprite = GetSprite("Keypad Enter", script.defaultSprite) },
            new() { keyCode = KeyCode.KeypadEquals, sprite = GetSprite("Keypad Equals", script.defaultSprite) },
            new() { keyCode = KeyCode.UpArrow, sprite = GetSprite("Up Arrow", script.defaultSprite) },
            new() { keyCode = KeyCode.DownArrow, sprite = GetSprite("Down Arrow", script.defaultSprite) },
            new() { keyCode = KeyCode.RightArrow, sprite = GetSprite("Right Arrow", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftArrow, sprite = GetSprite("Left Arrow", script.defaultSprite) },
            new() { keyCode = KeyCode.Insert, sprite = GetSprite("Insert", script.defaultSprite) },
            new() { keyCode = KeyCode.Home, sprite = GetSprite("Home", script.defaultSprite) },
            new() { keyCode = KeyCode.End, sprite = GetSprite("End", script.defaultSprite) },
            new() { keyCode = KeyCode.PageUp, sprite = GetSprite("Page Up", script.defaultSprite) },
            new() { keyCode = KeyCode.PageDown, sprite = GetSprite("Page Down", script.defaultSprite) },
            new() { keyCode = KeyCode.F1, sprite = GetSprite("F1", script.defaultSprite) },
            new() { keyCode = KeyCode.F2, sprite = GetSprite("F2", script.defaultSprite) },
            new() { keyCode = KeyCode.F3, sprite = GetSprite("F3", script.defaultSprite) },
            new() { keyCode = KeyCode.F4, sprite = GetSprite("F4", script.defaultSprite) },
            new() { keyCode = KeyCode.F5, sprite = GetSprite("F5", script.defaultSprite) },
            new() { keyCode = KeyCode.F6, sprite = GetSprite("F6", script.defaultSprite) },
            new() { keyCode = KeyCode.F7, sprite = GetSprite("F7", script.defaultSprite) },
            new() { keyCode = KeyCode.F8, sprite = GetSprite("F8", script.defaultSprite) },
            new() { keyCode = KeyCode.F9, sprite = GetSprite("F9", script.defaultSprite) },
            new() { keyCode = KeyCode.F10, sprite = GetSprite("F10", script.defaultSprite) },
            new() { keyCode = KeyCode.F11, sprite = GetSprite("F11", script.defaultSprite) },
            new() { keyCode = KeyCode.F12, sprite = GetSprite("F12", script.defaultSprite) },
            new() { keyCode = KeyCode.F13, sprite = GetSprite("F13", script.defaultSprite) },
            new() { keyCode = KeyCode.F14, sprite = GetSprite("F14", script.defaultSprite) },
            new() { keyCode = KeyCode.F15, sprite = GetSprite("F15", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha0, sprite = GetSprite("Alpha0", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha1, sprite = GetSprite("Alpha1", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha2, sprite = GetSprite("Alpha2", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha3, sprite = GetSprite("Alpha3", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha4, sprite = GetSprite("Alpha4", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha5, sprite = GetSprite("Alpha5", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha6, sprite = GetSprite("Alpha6", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha7, sprite = GetSprite("Alpha7", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha8, sprite = GetSprite("Alpha8", script.defaultSprite) },
            new() { keyCode = KeyCode.Alpha9, sprite = GetSprite("Alpha9", script.defaultSprite) },
            new() { keyCode = KeyCode.Exclaim, sprite = GetSprite("Exclaim", script.defaultSprite) },
            new() { keyCode = KeyCode.DoubleQuote, sprite = GetSprite("Double Quote", script.defaultSprite) },
            new() { keyCode = KeyCode.Hash, sprite = GetSprite("Hash", script.defaultSprite) },
            new() { keyCode = KeyCode.Dollar, sprite = GetSprite("Dollar", script.defaultSprite) },
            new() { keyCode = KeyCode.Percent, sprite = GetSprite("Percent", script.defaultSprite) },
            new() { keyCode = KeyCode.Ampersand, sprite = GetSprite("Ampersand", script.defaultSprite) },
            new() { keyCode = KeyCode.Quote, sprite = GetSprite("Quote", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftParen, sprite = GetSprite("Left Paren", script.defaultSprite) },
            new() { keyCode = KeyCode.RightParen, sprite = GetSprite("Right Paren", script.defaultSprite) },
            new() { keyCode = KeyCode.Asterisk, sprite = GetSprite("Asterisk", script.defaultSprite) },
            new() { keyCode = KeyCode.Plus, sprite = GetSprite("Plus", script.defaultSprite) },
            new() { keyCode = KeyCode.Comma, sprite = GetSprite("Comma", script.defaultSprite) },
            new() { keyCode = KeyCode.Minus, sprite = GetSprite("Minus", script.defaultSprite) },
            new() { keyCode = KeyCode.Period, sprite = GetSprite("Period", script.defaultSprite) },
            new() { keyCode = KeyCode.Slash, sprite = GetSprite("Slash", script.defaultSprite) },
            new() { keyCode = KeyCode.Colon, sprite = GetSprite("Colon", script.defaultSprite) },
            new() { keyCode = KeyCode.Semicolon, sprite = GetSprite("Semicolon", script.defaultSprite) },
            new() { keyCode = KeyCode.Less, sprite = GetSprite("Less", script.defaultSprite) },
            new() { keyCode = KeyCode.Equals, sprite = GetSprite("Equals", script.defaultSprite) },
            new() { keyCode = KeyCode.Greater, sprite = GetSprite("Greater", script.defaultSprite) },
            new() { keyCode = KeyCode.Question, sprite = GetSprite("Question", script.defaultSprite) },
            new() { keyCode = KeyCode.At, sprite = GetSprite("At", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftBracket, sprite = GetSprite("Left Bracket", script.defaultSprite) },
            new() { keyCode = KeyCode.Backslash, sprite = GetSprite("Backslash", script.defaultSprite) },
            new() { keyCode = KeyCode.RightBracket, sprite = GetSprite("Right Bracket", script.defaultSprite) },
            new() { keyCode = KeyCode.Caret, sprite = GetSprite("Caret", script.defaultSprite) },
            new() { keyCode = KeyCode.Underscore, sprite = GetSprite("Underscore", script.defaultSprite) },
            new() { keyCode = KeyCode.BackQuote, sprite = GetSprite("Back Quote", script.defaultSprite) },
            new() { keyCode = KeyCode.A, sprite = GetSprite("A", script.defaultSprite) },
            new() { keyCode = KeyCode.B, sprite = GetSprite("B", script.defaultSprite) },
            new() { keyCode = KeyCode.C, sprite = GetSprite("C", script.defaultSprite) },
            new() { keyCode = KeyCode.D, sprite = GetSprite("D", script.defaultSprite) },
            new() { keyCode = KeyCode.E, sprite = GetSprite("E", script.defaultSprite) },
            new() { keyCode = KeyCode.F, sprite = GetSprite("F", script.defaultSprite) },
            new() { keyCode = KeyCode.G, sprite = GetSprite("G", script.defaultSprite) },
            new() { keyCode = KeyCode.H, sprite = GetSprite("H", script.defaultSprite) },
            new() { keyCode = KeyCode.I, sprite = GetSprite("I", script.defaultSprite) },
            new() { keyCode = KeyCode.J, sprite = GetSprite("J", script.defaultSprite) },
            new() { keyCode = KeyCode.K, sprite = GetSprite("K", script.defaultSprite) },
            new() { keyCode = KeyCode.L, sprite = GetSprite("L", script.defaultSprite) },
            new() { keyCode = KeyCode.M, sprite = GetSprite("M", script.defaultSprite) },
            new() { keyCode = KeyCode.N, sprite = GetSprite("N", script.defaultSprite) },
            new() { keyCode = KeyCode.O, sprite = GetSprite("O", script.defaultSprite) },
            new() { keyCode = KeyCode.P, sprite = GetSprite("P", script.defaultSprite) },
            new() { keyCode = KeyCode.Q, sprite = GetSprite("Q", script.defaultSprite) },
            new() { keyCode = KeyCode.R, sprite = GetSprite("R", script.defaultSprite) },
            new() { keyCode = KeyCode.S, sprite = GetSprite("S", script.defaultSprite) },
            new() { keyCode = KeyCode.T, sprite = GetSprite("T", script.defaultSprite) },
            new() { keyCode = KeyCode.U, sprite = GetSprite("U", script.defaultSprite) },
            new() { keyCode = KeyCode.V, sprite = GetSprite("V", script.defaultSprite) },
            new() { keyCode = KeyCode.W, sprite = GetSprite("W", script.defaultSprite) },
            new() { keyCode = KeyCode.X, sprite = GetSprite("X", script.defaultSprite) },
            new() { keyCode = KeyCode.Y, sprite = GetSprite("Y", script.defaultSprite) },
            new() { keyCode = KeyCode.Z, sprite = GetSprite("Z", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftCurlyBracket, sprite = GetSprite("Left Curly Bracket", script.defaultSprite) },
            new() { keyCode = KeyCode.Pipe, sprite = GetSprite("Pipe", script.defaultSprite) },
            new() { keyCode = KeyCode.RightCurlyBracket, sprite = GetSprite("Right Curly Bracket", script.defaultSprite) },
            new() { keyCode = KeyCode.Tilde, sprite = GetSprite("Tilde", script.defaultSprite) },
            new() { keyCode = KeyCode.Numlock, sprite = GetSprite("Numlock", script.defaultSprite) },
            new() { keyCode = KeyCode.CapsLock, sprite = GetSprite("Caps Lock", script.defaultSprite) },
            new() { keyCode = KeyCode.ScrollLock, sprite = GetSprite("Scroll Lock", script.defaultSprite) },
            new() { keyCode = KeyCode.RightShift, sprite = GetSprite("Right Shift", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftShift, sprite = GetSprite("Left Shift", script.defaultSprite) },
            new() { keyCode = KeyCode.RightControl, sprite = GetSprite("Right Control", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftControl, sprite = GetSprite("Left Control", script.defaultSprite) },
            new() { keyCode = KeyCode.RightAlt, sprite = GetSprite("Right Alt", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftAlt, sprite = GetSprite("Left Alt", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftMeta, sprite = GetSprite("Left Meta", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftCommand, sprite = GetSprite("Left Command", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftApple, sprite = GetSprite("Left Apple", script.defaultSprite) },
            new() { keyCode = KeyCode.LeftWindows, sprite = GetSprite("Left Windows", script.defaultSprite) },
            new() { keyCode = KeyCode.RightMeta, sprite = GetSprite("Right Meta", script.defaultSprite) },
            new() { keyCode = KeyCode.RightCommand, sprite = GetSprite("Right Command", script.defaultSprite) },
            new() { keyCode = KeyCode.RightApple, sprite = GetSprite("Right Apple", script.defaultSprite) },
            new() { keyCode = KeyCode.RightWindows, sprite = GetSprite("Right Windows", script.defaultSprite) },
            new() { keyCode = KeyCode.AltGr, sprite = GetSprite("Alt Gr", script.defaultSprite) },
            new() { keyCode = KeyCode.Help, sprite = GetSprite("Help", script.defaultSprite) },
            new() { keyCode = KeyCode.Print, sprite = GetSprite("Print", script.defaultSprite) },
            new() { keyCode = KeyCode.SysReq, sprite = GetSprite("Sys Req", script.defaultSprite) },
            new() { keyCode = KeyCode.Break, sprite = GetSprite("Break", script.defaultSprite) },
            new() { keyCode = KeyCode.Menu, sprite = GetSprite("Menu", script.defaultSprite) },
            new() { keyCode = KeyCode.WheelUp, sprite = GetSprite("Wheel Up", script.defaultSprite) },
            new() { keyCode = KeyCode.WheelDown, sprite = GetSprite("Wheel Down", script.defaultSprite) },
            new() { keyCode = KeyCode.Mouse0, sprite = GetSprite("Mouse0", script.defaultSprite) },
            new() { keyCode = KeyCode.Mouse1, sprite = GetSprite("Mouse1", script.defaultSprite) },
            new() { keyCode = KeyCode.Mouse2, sprite = GetSprite("Mouse2", script.defaultSprite) },
            new() { keyCode = KeyCode.Mouse3, sprite = GetSprite("Mouse3", script.defaultSprite) },
            new() { keyCode = KeyCode.Mouse4, sprite = GetSprite("Mouse4", script.defaultSprite) },
            new() { keyCode = KeyCode.Mouse5, sprite = GetSprite("Mouse5", script.defaultSprite) },
            new() { keyCode = KeyCode.Mouse6, sprite = GetSprite("Mouse6", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton0, sprite = GetSprite("Joystick Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton1, sprite = GetSprite("Joystick Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton2, sprite = GetSprite("Joystick Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton3, sprite = GetSprite("Joystick Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton4, sprite = GetSprite("Joystick Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton5, sprite = GetSprite("Joystick Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton6, sprite = GetSprite("Joystick Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton7, sprite = GetSprite("Joystick Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton8, sprite = GetSprite("Joystick Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton9, sprite = GetSprite("Joystick Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton10, sprite = GetSprite("Joystick Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton11, sprite = GetSprite("Joystick Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton12, sprite = GetSprite("Joystick Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton13, sprite = GetSprite("Joystick Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton14, sprite = GetSprite("Joystick Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton15, sprite = GetSprite("Joystick Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton16, sprite = GetSprite("Joystick Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton17, sprite = GetSprite("Joystick Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton18, sprite = GetSprite("Joystick Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.JoystickButton19, sprite = GetSprite("Joystick Button 19", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button0, sprite = GetSprite("Joystick 1 Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button1, sprite = GetSprite("Joystick 1 Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button2, sprite = GetSprite("Joystick 1 Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button3, sprite = GetSprite("Joystick 1 Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button4, sprite = GetSprite("Joystick 1 Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button5, sprite = GetSprite("Joystick 1 Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button6, sprite = GetSprite("Joystick 1 Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button7, sprite = GetSprite("Joystick 1 Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button8, sprite = GetSprite("Joystick 1 Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button9, sprite = GetSprite("Joystick 1 Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button10, sprite = GetSprite("Joystick 1 Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button11, sprite = GetSprite("Joystick 1 Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button12, sprite = GetSprite("Joystick 1 Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button13, sprite = GetSprite("Joystick 1 Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button14, sprite = GetSprite("Joystick 1 Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button15, sprite = GetSprite("Joystick 1 Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button16, sprite = GetSprite("Joystick 1 Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button17, sprite = GetSprite("Joystick 1 Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button18, sprite = GetSprite("Joystick 1 Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick1Button19, sprite = GetSprite("Joystick 1 Button 19", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button0, sprite = GetSprite("Joystick 2 Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button1, sprite = GetSprite("Joystick 2 Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button2, sprite = GetSprite("Joystick 2 Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button3, sprite = GetSprite("Joystick 2 Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button4, sprite = GetSprite("Joystick 2 Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button5, sprite = GetSprite("Joystick 2 Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button6, sprite = GetSprite("Joystick 2 Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button7, sprite = GetSprite("Joystick 2 Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button8, sprite = GetSprite("Joystick 2 Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button9, sprite = GetSprite("Joystick 2 Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button10, sprite = GetSprite("Joystick 2 Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button11, sprite = GetSprite("Joystick 2 Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button12, sprite = GetSprite("Joystick 2 Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button13, sprite = GetSprite("Joystick 2 Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button14, sprite = GetSprite("Joystick 2 Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button15, sprite = GetSprite("Joystick 2 Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button16, sprite = GetSprite("Joystick 2 Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button17, sprite = GetSprite("Joystick 2 Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button18, sprite = GetSprite("Joystick 2 Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick2Button19, sprite = GetSprite("Joystick 2 Button 19", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button0, sprite = GetSprite("Joystick 3 Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button1, sprite = GetSprite("Joystick 3 Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button2, sprite = GetSprite("Joystick 3 Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button3, sprite = GetSprite("Joystick 3 Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button4, sprite = GetSprite("Joystick 3 Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button5, sprite = GetSprite("Joystick 3 Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button6, sprite = GetSprite("Joystick 3 Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button7, sprite = GetSprite("Joystick 3 Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button8, sprite = GetSprite("Joystick 3 Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button9, sprite = GetSprite("Joystick 3 Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button10, sprite = GetSprite("Joystick 3 Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button11, sprite = GetSprite("Joystick 3 Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button12, sprite = GetSprite("Joystick 3 Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button13, sprite = GetSprite("Joystick 3 Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button14, sprite = GetSprite("Joystick 3 Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button15, sprite = GetSprite("Joystick 3 Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button16, sprite = GetSprite("Joystick 3 Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button17, sprite = GetSprite("Joystick 3 Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button18, sprite = GetSprite("Joystick 3 Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick3Button19, sprite = GetSprite("Joystick 3 Button 19", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button0, sprite = GetSprite("Joystick 4 Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button1, sprite = GetSprite("Joystick 4 Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button2, sprite = GetSprite("Joystick 4 Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button3, sprite = GetSprite("Joystick 4 Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button4, sprite = GetSprite("Joystick 4 Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button5, sprite = GetSprite("Joystick 4 Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button6, sprite = GetSprite("Joystick 4 Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button7, sprite = GetSprite("Joystick 4 Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button8, sprite = GetSprite("Joystick 4 Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button9, sprite = GetSprite("Joystick 4 Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button10, sprite = GetSprite("Joystick 4 Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button11, sprite = GetSprite("Joystick 4 Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button12, sprite = GetSprite("Joystick 4 Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button13, sprite = GetSprite("Joystick 4 Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button14, sprite = GetSprite("Joystick 4 Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button15, sprite = GetSprite("Joystick 4 Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button16, sprite = GetSprite("Joystick 4 Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button17, sprite = GetSprite("Joystick 4 Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button18, sprite = GetSprite("Joystick 4 Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick4Button19, sprite = GetSprite("Joystick 4 Button 19", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button0, sprite = GetSprite("Joystick 5 Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button1, sprite = GetSprite("Joystick 5 Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button2, sprite = GetSprite("Joystick 5 Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button3, sprite = GetSprite("Joystick 5 Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button4, sprite = GetSprite("Joystick 5 Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button5, sprite = GetSprite("Joystick 5 Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button6, sprite = GetSprite("Joystick 5 Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button7, sprite = GetSprite("Joystick 5 Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button8, sprite = GetSprite("Joystick 5 Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button9, sprite = GetSprite("Joystick 5 Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button10, sprite = GetSprite("Joystick 5 Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button11, sprite = GetSprite("Joystick 5 Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button12, sprite = GetSprite("Joystick 5 Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button13, sprite = GetSprite("Joystick 5 Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button14, sprite = GetSprite("Joystick 5 Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button15, sprite = GetSprite("Joystick 5 Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button16, sprite = GetSprite("Joystick 5 Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button17, sprite = GetSprite("Joystick 5 Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button18, sprite = GetSprite("Joystick 5 Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick5Button19, sprite = GetSprite("Joystick 5 Button 19", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button0, sprite = GetSprite("Joystick 6 Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button1, sprite = GetSprite("Joystick 6 Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button2, sprite = GetSprite("Joystick 6 Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button3, sprite = GetSprite("Joystick 6 Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button4, sprite = GetSprite("Joystick 6 Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button5, sprite = GetSprite("Joystick 6 Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button6, sprite = GetSprite("Joystick 6 Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button7, sprite = GetSprite("Joystick 6 Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button8, sprite = GetSprite("Joystick 6 Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button9, sprite = GetSprite("Joystick 6 Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button10, sprite = GetSprite("Joystick 6 Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button11, sprite = GetSprite("Joystick 6 Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button12, sprite = GetSprite("Joystick 6 Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button13, sprite = GetSprite("Joystick 6 Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button14, sprite = GetSprite("Joystick 6 Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button15, sprite = GetSprite("Joystick 6 Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button16, sprite = GetSprite("Joystick 6 Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button17, sprite = GetSprite("Joystick 6 Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button18, sprite = GetSprite("Joystick 6 Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick6Button19, sprite = GetSprite("Joystick 6 Button 19", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button0, sprite = GetSprite("Joystick 7 Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button1, sprite = GetSprite("Joystick 7 Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button2, sprite = GetSprite("Joystick 7 Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button3, sprite = GetSprite("Joystick 7 Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button4, sprite = GetSprite("Joystick 7 Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button5, sprite = GetSprite("Joystick 7 Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button6, sprite = GetSprite("Joystick 7 Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button7, sprite = GetSprite("Joystick 7 Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button8, sprite = GetSprite("Joystick 7 Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button9, sprite = GetSprite("Joystick 7 Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button10, sprite = GetSprite("Joystick 7 Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button11, sprite = GetSprite("Joystick 7 Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button12, sprite = GetSprite("Joystick 7 Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button13, sprite = GetSprite("Joystick 7 Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button14, sprite = GetSprite("Joystick 7 Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button15, sprite = GetSprite("Joystick 7 Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button16, sprite = GetSprite("Joystick 7 Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button17, sprite = GetSprite("Joystick 7 Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button18, sprite = GetSprite("Joystick 7 Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick7Button19, sprite = GetSprite("Joystick 7 Button 19", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button0, sprite = GetSprite("Joystick 8 Button 0", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button1, sprite = GetSprite("Joystick 8 Button 1", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button2, sprite = GetSprite("Joystick 8 Button 2", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button3, sprite = GetSprite("Joystick 8 Button 3", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button4, sprite = GetSprite("Joystick 8 Button 4", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button5, sprite = GetSprite("Joystick 8 Button 5", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button6, sprite = GetSprite("Joystick 8 Button 6", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button7, sprite = GetSprite("Joystick 8 Button 7", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button8, sprite = GetSprite("Joystick 8 Button 8", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button9, sprite = GetSprite("Joystick 8 Button 9", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button10, sprite = GetSprite("Joystick 8 Button 10", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button11, sprite = GetSprite("Joystick 8 Button 11", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button12, sprite = GetSprite("Joystick 8 Button 12", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button13, sprite = GetSprite("Joystick 8 Button 13", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button14, sprite = GetSprite("Joystick 8 Button 14", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button15, sprite = GetSprite("Joystick 8 Button 15", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button16, sprite = GetSprite("Joystick 8 Button 16", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button17, sprite = GetSprite("Joystick 8 Button 17", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button18, sprite = GetSprite("Joystick 8 Button 18", script.defaultSprite) },
            new() { keyCode = KeyCode.Joystick8Button19, sprite = GetSprite("Joystick 8 Button 19", script.defaultSprite) }
        };

        script.keyCodesSprites.AddRange(keyCodeList); // Adiciona a lista preenchida à propriedade keyCodesSprites do script.
    }

    // Method to retrieve a sprite asset by name or return the default sprite if not found.
    private Sprite GetSprite(string spriteName, Sprite defaultSprite)
    {
        var guids = AssetDatabase.FindAssets($"t:Sprite"); // Searches all sprite assets in the project using their GUIDs.

        // Iterates through each GUID to find sprites matching the specified spriteName.
        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            // Checks if the asset is a sprite and loads all sprites at the asset path.
            if (textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite)
            {
                var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToArray();

                // Searches for a sprite matching the provided spriteName.
                foreach (var sprite in sprites)
                {
                    if (sprite.name == spriteName)
                    {
                        Debug.Log($"Sprite Found [{sprite.name}]");
                        return sprite; // Returns the found sprite.
                    }
                }
            }
        }

        return defaultSprite; // Returns the default sprite if no matching sprite is found.
    }
}