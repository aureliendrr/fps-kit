using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField] private Keybindings keybindings;

    private void Awake()
    {
        if(instance != null) {
            Destroy(this);
        }
        else {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public KeyCode GetKeyForAction(KeybindingAction action)
    {
        foreach(Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks)
        {
            if(keybindingCheck.action == action)
            {
                return keybindingCheck.code;
            }
        }

        return KeyCode.None;
    }

    public bool Action(KeybindingAction action)
    {
        foreach (Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks)
        {
            if (keybindingCheck.action == action)
            {
                switch (keybindingCheck.type)
                {
                    case GetKeyType.GetKey:
                        return Input.GetKey(keybindingCheck.code);
                    case GetKeyType.GetKeyDown:
                        return Input.GetKeyDown(keybindingCheck.code);
                    case GetKeyType.GetKeyUp:
                        return Input.GetKeyUp(keybindingCheck.code);
                    default:
                        return false;
                }
            }
        }

        return false;
    }

    public float Axis(string axisName)
    {
        return Input.GetAxis(axisName);
    }

    public float AxisRaw(string axisName)
    {
        return Input.GetAxisRaw(axisName);
    }
}