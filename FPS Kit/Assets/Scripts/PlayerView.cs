using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    //-----PRIVATE
    private PlayerManager manager;
    private PlayerInputs inputs;
    private PlayerMovements movements;
    

    private Vector3 viewTarget;
    private Vector2 clamp;

    [Header("Settings")]
    [Header("View Position (Standing, Crouching, Proning)")]
    [SerializeField] private Vector3 viewPos;
    [SerializeField] private float viewTransitionTime = 10f;
    [Header("Clamp Rotation (Standing, Crouching, Proning)")]
    [SerializeField] private Vector3 clampMinX;
    [SerializeField] private Vector3 clampMaxX;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform viewCamera;
    [SerializeField] private Camera playerCam;

    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Start()
    {
        manager = PlayerManager.instance;
        inputs = GetComponent<PlayerInputs>();
        movements = GetComponent<PlayerMovements>();

        if (viewCamera == null)
        {
            Debug.LogError("You need to reference the camera Gamobject of the player to " + this.name);
        }

        if (playerCam == null)
        {
            Debug.LogError("You need to reference the camera of the player to " + this.name);
        }

        playerCam.fieldOfView = manager.settings.fov;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandlePlayerStance();
        HandlePlayerCameraInput();
        HandleViewPosition();
        ApplyView();
    }

    private void HandlePlayerStance()
    {
        switch (movements.stance)
        {
            case PlayerMovements.CharacterStance.Standing:
                viewTarget = new Vector3(0, viewPos.x, 0);
                clamp = new Vector2(clampMinX.x, clampMaxX.x);
                break;
            case PlayerMovements.CharacterStance.Crouching:
                viewTarget = new Vector3(0, viewPos.y, 0);
                clamp = new Vector2(clampMinX.y, clampMaxX.y);
                break;
            case PlayerMovements.CharacterStance.Proning:
                viewTarget = new Vector3(0, viewPos.z, 0);
                clamp = new Vector2(clampMinX.z, clampMaxX.z);
                break;
            default:
                viewTarget = Vector3.zero;
                clamp = Vector2.zero;
                break;
        }
    }


    private void HandlePlayerCameraInput()
    {
        //Apply to the rotations
        rotationY += inputs.mouseX * manager.settings.sensitivityX;
        rotationX -= inputs.mouseY * manager.settings.sensitivityY;

        //Clamp
        rotationX = Mathf.Clamp(rotationX, clamp.x, clamp.y); 
        if(rotationY >= 360 || rotationY <= -360)
        {
            rotationY = 0;
        }
    }

    private void HandleViewPosition()
    {
        //apply pos to target with damping
        viewCamera.localPosition = Vector3.Lerp(viewCamera.localPosition, viewTarget, viewTransitionTime * Time.deltaTime);
    }

    private void ApplyView()
    {
        viewCamera.rotation = Quaternion.Euler(rotationX, rotationY, 0); //Looking up and down (only the camera)
        orientation.transform.rotation = Quaternion.Euler(0, rotationY, 0); //Looking right and left (by the player rotation)
    }
}
