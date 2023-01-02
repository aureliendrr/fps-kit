using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    [Header("View")]
    public float mouseX;
    public float mouseY;

    [Header("Movements")]
    public int horizontalAxis;
    public int verticalAxis;
    public bool crouchInput;
    public bool proneInput;
    public bool jumpInput;
    public bool sprintInput;
    public bool slideInput;

    [Header("Weapons")]
    public bool aimInput;

    private InputSystem input;
    private PlayerMovements playerMovements;
    
    private void Start(){
        input = InputSystem.Instance;
        playerMovements = GetComponent<PlayerMovements>();
    }

    private void Update()
    {
        HandleMovementsInputs();
        HandleViewInputs();
        HandleWeaponsInputs();
    }

    private void HandleMovementsInputs()
    {
        horizontalAxis = input.Action(KeybindingAction.Right) ? 1 : (input.Action(KeybindingAction.Left) ? -1 : 0);
        verticalAxis = input.Action(KeybindingAction.Forward) ? 1 : (input.Action(KeybindingAction.Backward) ? -1 : 0);
        crouchInput = input.Action(KeybindingAction.Crouch);
        proneInput = input.Action(KeybindingAction.Prone);
        jumpInput = input.Action(KeybindingAction.Jump);
        sprintInput = input.Action(KeybindingAction.Sprint);
        slideInput = input.Action(KeybindingAction.Slide);
    }

    private void HandleViewInputs()
    {
        mouseX = input.AxisRaw("Mouse X");
        mouseY = input.AxisRaw("Mouse Y");
    }

    private void HandleWeaponsInputs()
    {
        aimInput = input.Action(KeybindingAction.Aim);
    }
}
