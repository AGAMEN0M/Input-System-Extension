using System.Collections.Generic;
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

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    // Confirm and delete the selected InputData entry.
                    if (EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete this item?\nIt will not be possible to undo this operation.", "Delete", "Cancel"))
                    {
                        DeleteInputData(script, i);
                    }
                    continue;
                }
                GUI.backgroundColor = Color.white;
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

        // Mark the script as dirty if any changes were made.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
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
            string newFileName = $"{inputData.keyboardTag} (Input Data).asset";
            string newAssetPath = Path.Combine(folderPath, newFileName);

            int counter = 1;
            while (AssetDatabase.LoadAssetAtPath<InputData>(newAssetPath) != null && oldAssetPath != newAssetPath)
            {
                newAssetPath = Path.Combine(folderPath, $"{inputData.keyboardTag} (Input Data) ({counter}).asset");
                counter++;
            }

            AssetDatabase.MoveAsset(oldAssetPath, newAssetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Repaint();
    }
}