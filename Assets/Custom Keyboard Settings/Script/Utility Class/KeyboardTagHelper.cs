/*
 * ---------------------------------------------------------------------------
 * Description: A utility class for managing and persisting keyboard control data 
 *              within a Unity project. It handles retrieval, updating, and saving 
 *              of InputData associated with specific keyboard tags using PlayerPrefs.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using System.Collections.Generic;
using UnityEngine;

namespace CustomKeyboard
{
    // Helper class for managing keyboard control data.
    public static class KeyboardTagHelper
    {
        // Retrieve the KeyboardControlData resource from the Resources folder.
        public static KeyboardControlData GetKeyboardControlData()
        {
            KeyboardControlData keyboardControlData = Resources.Load<KeyboardControlData>("Keyboard Control Data");

            if (keyboardControlData == null)
            {
                Debug.LogError("Failed to load KeyboardControlData from Resources. Ensure the resource exists and is named correctly.");
                return null;
            }

            return keyboardControlData;
        }

        // Retrieve the InputData associated with a specific keyboardTag.
        public static InputData GetInputFromTag(string tag)
        {
            KeyboardControlData keyboardControlData = GetKeyboardControlData();
            if (keyboardControlData == null)
            {
                Debug.LogError("KeyboardControlData is null.");
                return null;
            }

            // Search for InputData with the specified keyboardTag.
            foreach (var inputData in keyboardControlData.inputDataList)
            {
                if (inputData.keyboardTag == tag)
                {
                    return inputData; // Return the InputData that matches the tag.
                }
            }

            return null; // Return null if no matching InputData is found.
        }

        // Update the KeyCode for the specified InputData.
        public static void SetKey(InputData inputData, KeyCode newKeyCode)
        {
            if (inputData == null)
            {
                Debug.LogError("InputData is null.");
                return;
            }

            inputData.keyboard = newKeyCode;
        }

        // Update the KeyCode for InputData associated with the specified tag.
        public static void SetKeyFromTag(string tag, KeyCode newKeyCode)
        {
            InputData inputData = GetInputFromTag(tag);
            if (inputData == null)
            {
                Debug.LogError($"No InputData found with tag '{tag}'.");
                return;
            }

            inputData.keyboard = newKeyCode;
        }

        // Save the current keyboard control data to PlayerPrefs.
        public static void SaveKeyboardControlData()
        {
            KeyboardControlData keyboardControlData = GetKeyboardControlData();
            if (keyboardControlData == null)
            {
                Debug.LogError("KeyboardControlData is null.");
                return;
            }

            // Prepare a data model for saving.
            KeyboardControlDataSave data = new()
            {
                inputDataListSaves = new List<InputDataListSave>()
            };

            // Convert each InputData into InputDataListSave and add to the list.
            foreach (var inputData in keyboardControlData.inputDataList)
            {
                data.inputDataListSaves.Add(new InputDataListSave
                {
                    keyboardTag = inputData.keyboardTag,
                    keyboard = inputData.keyboard
                });
            }

            // Serialize the data to JSON and save it to PlayerPrefs.
            string jsonData = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("Keyboard Control Data", jsonData);
        }

        // Load keyboard control data from PlayerPrefs at runtime.
        [RuntimeInitializeOnLoadMethod]
        public static void LoadKeyboardControlData()
        {
            // Check if PlayerPrefs contains saved keyboard control data.
            if (PlayerPrefs.HasKey("Keyboard Control Data"))
            {
                // Retrieve and deserialize the JSON data from PlayerPrefs.
                string jsonData = PlayerPrefs.GetString("Keyboard Control Data");
                var data = JsonUtility.FromJson<KeyboardControlDataSave>(jsonData);

                // Obtain the KeyboardControlData instance.
                KeyboardControlData keyboardControlData = GetKeyboardControlData();
                if (keyboardControlData == null)
                {
                    Debug.LogError("Could not find KeyboardControlData with name 'Keyboard Control Data'");
                    return;
                }

                // Update the KeyboardControlData with the loaded data.
                foreach (var inputDataSave in data.inputDataListSaves)
                {
                    // Find the existing InputData by keyboardTag.
                    InputData existingInputData = GetInputFromTag(inputDataSave.keyboardTag);
                    if (existingInputData != null)
                    {
                        // Update the existing InputData with the saved KeyCode.
                        existingInputData.keyboard = inputDataSave.keyboard;
                    }
                    else
                    {
                        Debug.LogError($"InputData with tag '{inputDataSave.keyboardTag}' not found.");
                    }
                }
            }
        }
    }

    // Serializable class for storing saved keyboard control data.
    [System.Serializable]
    public class KeyboardControlDataSave
    {
        public List<InputDataListSave> inputDataListSaves; // List of saved InputData objects.
    }

    // Serializable class for storing individual InputData for saving.
    [System.Serializable]
    public class InputDataListSave
    {
        public string keyboardTag; // Tag associated with the keyboard input.
        public KeyCode keyboard; // KeyCode for the keyboard input.
    }
}