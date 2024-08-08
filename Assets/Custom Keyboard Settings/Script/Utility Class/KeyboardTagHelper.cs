using System.Collections.Generic;
using UnityEngine;

// Helper class for working with keyboard control data.
public static class KeyboardTagHelper
{
    // Retrieve the InputData associated with a specific tag.
    public static InputData GetInputFromTag(KeyboardControlData keyboardControlData, string tag)
    {
        // Iterate through the inputDataList in the provided KeyboardControlData.
        foreach (var inputData in keyboardControlData.inputDataList)
        {
            // Check if the keyboardTag matches the specified tag.
            if (inputData.keyboardTag == tag)
            {
                return inputData; // Return the InputData associated with the matching tag.
            }
        }

        return null; // If no match is found, return null.
    }

    // Set the KeyCode for the given InputData.
    public static void SetKey(InputData inputData, KeyCode newKeyCode)
    {
        inputData.keyboard = newKeyCode;
    }

    // Save the keyboard control data to PlayerPrefs.
    public static void SaveKeyboardControlData(KeyboardControlData keyboardControlData)
    {
        // Create a new data model for saving.
        KeyboardControlDataSave data = new()
        {
            inputDataListSaves = new List<InputDataListSave>()
        };

        // Convert each InputData to InputDataListSave and add to the list.
        foreach (var inputData in keyboardControlData.inputDataList)
        {
            data.inputDataListSaves.Add(new InputDataListSave
            {
                keyboardTag = inputData.keyboardTag,
                keyboard = inputData.keyboard
            });
        }

        // Convert the data to JSON and save it to PlayerPrefs.
        string jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("Keyboard Control Data", jsonData);
    }

    // Load the keyboard control data from PlayerPrefs.
    [RuntimeInitializeOnLoadMethod]
    public static void LoadKeyboardControlData()
    {
        // Check if PlayerPrefs has saved keyboard control data.
        if (PlayerPrefs.HasKey("Keyboard Control Data"))
        {
            // Load the saved JSON data from PlayerPrefs.
            string jsonData = PlayerPrefs.GetString("Keyboard Control Data");
            var data = JsonUtility.FromJson<KeyboardControlDataSave>(jsonData);

            // Find or create a KeyboardControlData instance.
            KeyboardControlData keyboardControlData = Resources.Load<KeyboardControlData>("Keyboard Control Data");
            if (keyboardControlData == null)
            {
                Debug.LogError("Could not find keyboardControlData with name 'Keyboard Control Data'");
                return;
            }

            // Update keyboardControlData with the loaded data.
            foreach (var inputDataSave in data.inputDataListSaves)
            {
                // Find the existing InputData with the matching keyboardTag.
                InputData existingInputData = GetInputFromTag(keyboardControlData, inputDataSave.keyboardTag);
                if (existingInputData != null)
                {
                    // Update the existing InputData with the new key code.
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

[System.Serializable]
public class KeyboardControlDataSave
{
    public List<InputDataListSave> inputDataListSaves; // List to store saved InputData objects.
}

[System.Serializable]
public class InputDataListSave
{
    public string keyboardTag; // Tag associated with the keyboard input.
    public KeyCode keyboard; // KeyCode associated with the keyboard input.
}