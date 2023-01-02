using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSystem : StaticInstance<DebugSystem>
{
    private bool _showConsole;
    private string _consoleInput;

    private InputSystem input;

    private void Start() {
        input = InputSystem.Instance;
    }

    private void Update() {
        if(input.Action(KeybindingAction.Debug)) { _showConsole = !_showConsole; }
    }
    
    private void OnGUI() {
        if(!_showConsole) { return; }

        float y = 0f;
        
        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        _consoleInput = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), _consoleInput);
    }
}