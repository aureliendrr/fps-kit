using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Keybindings", menuName = "FPS Kit/Keybindings")]
public class Keybindings : ScriptableObject
{
    [System.Serializable]
    public class KeybindingCheck
    {
        public KeybindingAction action;
        public GetKeyType type;
        public KeyCode code;
    }

    public KeybindingCheck[] keybindingChecks;
}
