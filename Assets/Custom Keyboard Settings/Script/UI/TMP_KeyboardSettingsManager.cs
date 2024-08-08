using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

[AddComponentMenu("UI/Custom Keyboard Settings/Keyboard Settings Manager (TMP)")]
public class TMP_KeyboardSettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button selectButton; // Button to select a new KeyCode.
    [SerializeField] private TMP_Text selectedButtonText; // Text showing the selected KeyCode.
    [SerializeField] private Button resetButton; // Button to reset to the default KeyCode.
    [Space(5)]
    [Header("Default Settings")]
    [SerializeField] private KeyCode defaultKeyCode = KeyCode.E; // Default KeyCode.
    [Space(5)]
    [Header("Save Settings")]
    [SerializeField] private InputData inputData; // InputData associated with the current settings.
    [Space(5)]
    [SerializeField] private string keyboardTag = "DefaultTag"; // Tag to associate with the KeyCode in KeyboardControlData.
    [SerializeField] private KeyboardControlData keyboardControlData; // Data for saving and loading settings.

    [HideInInspector] public KeyCode currentKeyCode = KeyCode.None; // Current selected KeyCode.

    private KeyCode previousKeyCode = KeyCode.None; // Previously selected KeyCode.
    private bool isListening = false; // Indicates if the script is waiting for a new keyboard input.
    private List<TMP_KeyboardSettingsManager> otherManagers; // List of other KeyboardSettingsManager in the scene to avoid KeyCode conflicts.

    private float delayTimer = 0f; // Timer to control the delay before starting to listen for new keyboard inputs.
    private bool isDelaying = false; // Indicates if the system is in a delay state, waiting to start listening for new keyboard inputs.

    private void OnSelectButtonClick()
    {
        // Start listening for new input with a delay if not already delaying or listening.
        if (!isDelaying && !isListening)
        {
            delayTimer = Time.realtimeSinceStartup + 0.5f;
            isDelaying = true;
        }

        isListening = true;
        selectedButtonText.text = $"> {previousKeyCode} <";
    }

    private void OnResetButtonClick()
    {
        // Set to default settings and save.
        SetDefaultSettings();
        SaveSettings();
    }

    private void Start()
    {
        // Initialize the list of other KeyboardSettingsManager instances in the scene.
        otherManagers = new List<TMP_KeyboardSettingsManager>(FindObjectsOfType<TMP_KeyboardSettingsManager>());

        // Add listeners for button clicks.
        selectButton.onClick.RemoveListener(OnSelectButtonClick);
        resetButton.onClick.RemoveListener(OnResetButtonClick);

        selectButton.onClick.AddListener(OnSelectButtonClick);
        resetButton.onClick.AddListener(OnResetButtonClick);

        // Load saved settings or set default settings.
        if (inputData != null)
        {
            SetSettings();
        }
        else if (keyboardControlData != null)
        {
            inputData = KeyboardTagHelper.GetInputFromTag(keyboardControlData, keyboardTag);
            if (inputData != null)
            {
                SetSettings();
            }
            else
            {
                SetDefaultSettings();
            }
        }
        else
        {
            SetDefaultSettings();
        }
    }

    private void SetSettings()
    {
        // Set the current KeyCode from the provided InputData and update UI.
        currentKeyCode = inputData.keyboard;
        previousKeyCode = currentKeyCode;
        selectedButtonText.text = previousKeyCode.ToString();
    }

    private void SetDefaultSettings()
    {
        // Set to default KeyCode and update UI.
        currentKeyCode = defaultKeyCode;
        previousKeyCode = defaultKeyCode;
        selectedButtonText.text = previousKeyCode.ToString();
    }

    private void Update()
    {
        // Listen for new keyboard input if currently listening.
        if (isListening)
        {
            ListenForNewInput();
        }

        // Update the interactability of the reset button based on the current KeyCode.
        resetButton.interactable = currentKeyCode != defaultKeyCode;

        // Update delay timer if delaying.
        if (isDelaying)
        {
            // Allow new input detection if the delay timer has elapsed.
            float currentTime = Time.realtimeSinceStartup;
            if (currentTime >= delayTimer)
            {
                isDelaying = false;
            }
        }
    }

    private void ListenForNewInput()
    {
        // Iterate through all KeyCodes to find a new keyboard input.
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode) && !isDelaying)
            {
                // Check if the KeyCode is not used by other managers.
                if (!IsKeyCodeUsedByOtherManagers(keyCode))
                {
                    // Update KeyCode, UI, and save settings.
                    currentKeyCode = keyCode;
                    previousKeyCode = keyCode;
                    selectedButtonText.text = keyCode.ToString();
                    isListening = false;
                    SaveSettings();
                }
                else
                {
                    Debug.LogWarning("KeyCode is already in use by another KeyboardSettingsManager.");
                }
            }
        }
    }

    private bool IsKeyCodeUsedByOtherManagers(KeyCode keyCode)
    {
        // Check if the KeyCode is used by other managers in the scene.
        foreach (var manager in otherManagers)
        {
            if (manager != this && manager.currentKeyCode == keyCode)
            {
                return true;
            }
        }
        return false;
    }

    private void SaveSettings()
    {
        // Save settings to KeyboardControlData.
        if (keyboardControlData != null)
        {
            if (inputData != null)
            {
                KeyboardTagHelper.SetKey(inputData, currentKeyCode);
                KeyboardTagHelper.SaveKeyboardControlData(keyboardControlData);
            }
            else
            {
                Debug.LogError($"No InputData found with tag '{keyboardTag}'.");
            }
        }
        else
        {
            Debug.LogError("A KeyboardControlData is required.");
        }
    }
}