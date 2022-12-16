using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    //-----PRIVATE
    private InputManager inputManager;

    private LayerMask layerMask;
    private Collider[] obstructions = new Collider[8];

    private Vector3 moveDirection;
    private float walkSpeed;
    private float runSpeed;

    private Vector3 slopeMoveDirection;
    private RaycastHit slopeHit;

    private float currentHeight;

    [Header("Settings")]
    [Header("Speed (Normal, Running)")]
    [SerializeField] private Vector2 standingSpeed;
    [SerializeField] private Vector2 crouchSpeed;
    [SerializeField] private Vector2 proningSpeed;

    [Header("Speed Multiplier")]
    [SerializeField] private float groundSpeedMultiplier = 1f;
    [SerializeField] private float airSpeedMultiplier = .1f;

    [Header("Collider (Radius, Height, YOffset)")]
    [SerializeField] private Vector3 standingCollider;
    [SerializeField] private Vector3 crouchingCollider;
    [SerializeField] private Vector3 proningCollider;
    [SerializeField] private float colliderThreshold;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpStamina = 35f;
    
    [Header("Ridigbody Drag")]
    [SerializeField] private float rbGroundDrag = 6f;
    [SerializeField] private float rbAirDrag = .1f;
    
    [Header("Slope")]
    [SerializeField] private float maxSlopeAngle = 40f;
    [SerializeField] private float maxSlopeDistance = 0.05f;
    
    [Header("Grounded")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = 0.4f;

    [Header("Stats")]
    public CharacterStance stance;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float stamina;
    [SerializeField] private bool sliding = true;

    [Header("Player Controls")]
    [SerializeField] private int horizontalAxis;
    [SerializeField] private int verticalAxis;
    [SerializeField] private bool crouchInput;
    [SerializeField] private bool proneInput;
    [SerializeField] private bool jumpInput;
    [SerializeField] private bool sprintInput;
    [SerializeField] private bool slideInput;

    [Header("References")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;

    public enum CharacterStance
    {
        Standing,
        Crouching,
        Proning
    }

    private void Start()
    {
        SetDefaultValues();
        ComponentsCheck();
    }

    private void SetDefaultValues()
    {
        inputManager = InputManager.instance;

        walkSpeed = standingSpeed.x;
        runSpeed = standingSpeed.y;
        stance = CharacterStance.Standing;
        SetColliderDimensions(standingCollider);

        int mask = 0;
        for (int i = 0; i < 32; i++)
            if(!Physics.GetIgnoreLayerCollision(gameObject.layer, i))
                mask |= 1 << i;

        layerMask = mask;
    }

    private void ComponentsCheck()
    {
        if(inputManager == null)
        {
            Debug.LogError("You need to have an InputManager instance ine the scene !");
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            Debug.LogWarning("You need to reference the rigidbody to " + this.name);
        }
        rb.freezeRotation = true;

        if (capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            Debug.LogWarning("You need to reference the capsule collider to " + this.name);
        }

        if (orientation == null)
        {
            Debug.LogError("You need to reference the orientation transform to " + this.name);
        }
    }

    public bool Grounded()
    {
        return Physics.CheckSphere(transform.position, groundDistance, groundMask);
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, currentHeight * 0.5f + maxSlopeDistance))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private void Update()
    {
        HandlePlayerMovementsInput();
        HandleCharacterSpeed();
        HandleRigidbody();
        HandleSlope();
        HandleJump();
        /*
HandleStamina();
HandleSlide();
*/
    }

    private void HandlePlayerMovementsInput()
    {
        //Get inputs
        horizontalAxis = inputManager.Action(KeybindingAction.Right) ? 1 : (inputManager.Action(KeybindingAction.Left) ? -1 : 0);
        verticalAxis = inputManager.Action(KeybindingAction.Forward) ? 1 : (inputManager.Action(KeybindingAction.Backward) ? -1 : 0);
        crouchInput = inputManager.Action(KeybindingAction.Crouch);
        proneInput = inputManager.Action(KeybindingAction.Prone);
        jumpInput = inputManager.Action(KeybindingAction.Jump);
        sprintInput = inputManager.Action(KeybindingAction.Sprint);
        slideInput = inputManager.Action(KeybindingAction.Slide);

        //Apply to move direction
        moveDirection = orientation.forward * verticalAxis + orientation.right * horizontalAxis;
    }

    private void HandleCharacterSpeed()
    {
        currentSpeed = sprintInput ? runSpeed : walkSpeed;
    }

    private void HandleSlope()
    {
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void HandleRigidbody()
    {
        rb.drag = Grounded() ? rbGroundDrag : rbAirDrag;
        //rb.useGravity = !OnSlope();
    }

    private void HandleJump()
    {
        if (jumpInput && Grounded())
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumpInput = false;
        }
    }

    private void LateUpdate()
    {
        HandleCharacterStance();
    }

    private void HandleCharacterStance()
    {
        switch (stance)
        {
            case CharacterStance.Standing:
                if (crouchInput) { RequestCharacterStanceChange(CharacterStance.Crouching); }
                else if (proneInput) { RequestCharacterStanceChange(CharacterStance.Proning); }
                break;
            case CharacterStance.Crouching:
                if (crouchInput) { RequestCharacterStanceChange(CharacterStance.Standing); }
                else if (proneInput) { RequestCharacterStanceChange(CharacterStance.Proning); }
                break;
            case CharacterStance.Proning:
                if (crouchInput) { RequestCharacterStanceChange(CharacterStance.Crouching); }
                else if (proneInput) { RequestCharacterStanceChange(CharacterStance.Standing); }
                break;
        }
    }

    public bool RequestCharacterStanceChange(CharacterStance newStance)
    {
        if (stance == newStance)
            return true;

        switch (stance)
        {
            case CharacterStance.Standing:
                if(newStance == CharacterStance.Crouching)
                {
                    if (!CharacterOverlap(crouchingCollider))
                    {
                        walkSpeed = crouchSpeed.x;
                        runSpeed = crouchSpeed.y;
                        stance = newStance;
                        SetColliderDimensions(crouchingCollider);
                        return true;
                    }
                }
                else if (newStance == CharacterStance.Proning)
                {
                    if (!CharacterOverlap(proningCollider))
                    {
                        walkSpeed = proningSpeed.x;
                        runSpeed = proningSpeed.y;
                        stance = newStance;
                        SetColliderDimensions(proningCollider);
                        return true;
                    }
                }
                break;
            case CharacterStance.Crouching:
                if (newStance == CharacterStance.Standing)
                {
                    if (!CharacterOverlap(standingCollider))
                    {
                        walkSpeed = standingSpeed.x;
                        runSpeed = standingSpeed.y;
                        stance = newStance;
                        SetColliderDimensions(standingCollider);
                        return true;
                    }
                }
                else if (newStance == CharacterStance.Proning)
                {
                    if (!CharacterOverlap(proningCollider))
                    {
                        walkSpeed = proningSpeed.x;
                        runSpeed = proningSpeed.y;
                        stance = newStance;
                        SetColliderDimensions(proningCollider);
                        return true;
                    }
                }
                break;
            case CharacterStance.Proning:
                if (newStance == CharacterStance.Standing)
                {
                    if (!CharacterOverlap(standingCollider))
                    {
                        walkSpeed = standingSpeed.x;
                        runSpeed = standingSpeed.y;
                        stance = newStance;
                        SetColliderDimensions(standingCollider);
                        return true;
                    }
                }
                else if (newStance == CharacterStance.Crouching)
                {
                    if (!CharacterOverlap(crouchingCollider))
                    {
                        walkSpeed = crouchSpeed.x;
                        runSpeed = crouchSpeed.y;
                        stance = newStance;
                        SetColliderDimensions(crouchingCollider);
                        return true;
                    }
                }
                break;
        }

        return false;
    }

    private bool CharacterOverlap(Vector3 collider)
    {
        float radius = collider.x - colliderThreshold;
        float height = collider.y;
        Vector3 center = new Vector3(capsuleCollider.center.x, collider.z, capsuleCollider.center.z);

        Vector3 point0;
        Vector3 point1;
        if(height < radius * 2)
        {
            point0 = transform.position + center;
            point1 = transform.position + center;
        }
        else
        {
            point0 = transform.position + center + (transform.up * (height * 0.5f - radius));
            point1 = transform.position + center - (transform.up * (height * 0.5f - radius));
        }

        Debug.Log(point0 + ", " + point1 + ", " + radius);
        int numOverlaps = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, obstructions, layerMask);

        for (int i = 0; i < numOverlaps; i++){
            Debug.Log(obstructions[i].name);
            if (obstructions[i] == capsuleCollider)
                numOverlaps--;
        }

        return numOverlaps > 0;
    }

    private void SetColliderDimensions(Vector3 collider)
    {
        capsuleCollider.center = new Vector3(capsuleCollider.center.x, collider.z, capsuleCollider.center.z);
        capsuleCollider.radius = collider.x;
        capsuleCollider.height = collider.y;
        currentHeight = collider.y;
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        rb.AddForce((Grounded() ? groundSpeedMultiplier : airSpeedMultiplier) * currentSpeed * (OnSlope() ? slopeMoveDirection.normalized : moveDirection.normalized), ForceMode.Acceleration);
    }
}
