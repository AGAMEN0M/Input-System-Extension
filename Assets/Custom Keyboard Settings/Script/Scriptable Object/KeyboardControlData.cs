/*
 * ---------------------------------------------------------------------------
 * Description: Groups the InputData.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using System.Collections.Generic;
using UnityEngine;

/*[CreateAssetMenu(fileName = "KeyboardControlData", menuName = "ScriptableObjects/KeyboardControlData", order = 2)]*/
// Class to manage a list of InputData objects.
// This is a ScriptableObject that contains a list of InputData, used for keyboard control configurations.
public class KeyboardControlData : ScriptableObject
{
    public List<InputData> inputDataList; // List of InputData objects, each representing a keyboard input configuration.
}