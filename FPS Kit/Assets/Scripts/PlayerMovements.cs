using System;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    //-----PRIVATE
    private PlayerInputs inputs;

    private Vector3 moveDirection;
    private float walkSpeed;
    private float runSpeed;

    private Vector3 slopeMoveDirection;
    private RaycastHit slopeHit;

    private float currentHeight;

    public enum CharacterStance
    {
        Standing,
        Crouching,
        Proning
    }

    [SerializeField] private bool debug;

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
    [SerializeField] private LayerMask obstructionMask;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;

    [Header("Slide")]
    [SerializeField] private float slideForce = 10f;
    
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

    [Header("References")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;

    //Actions

    #region Awake

    private void Awake()
    {
        SetDefaultValues();
        ComponentsCheck();
    }

    private void SetDefaultValues()
    {
        inputs = GetComponent<PlayerInputs>();

        walkSpeed = standingSpeed.x;
        runSpeed = standingSpeed.y;
        stance = CharacterStance.Standing;
        SetColliderDimensions(standingCollider);

        int mask = 0;
        for (int i = 0; i < 32; i++)
            if(!Physics.GetIgnoreLayerCollision(gameObject.layer, i))
                mask |= 1 << i;

        obstructionMask = mask;
    }

    private void ComponentsCheck()
    {
        if(inputs == null)
        {
            Debug.LogError("You need to have the PlayerInput attached to the player gameobject !");
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

    #endregion

    #region Actions

    public bool RequestCrouch()
    {
        if(stance == CharacterStance.Standing || stance == CharacterStance.Proning)
        {
            if (!CharacterOverlap(crouchingCollider))
            {
                walkSpeed = crouchSpeed.x;
                runSpeed = crouchSpeed.y;
                stance = CharacterStance.Crouching;
                SetColliderDimensions(crouchingCollider);
                return true;
            }
        }
        else
        {
            if (!CharacterOverlap(standingCollider))
            {
                walkSpeed = standingSpeed.x;
                runSpeed = standingSpeed.y;
                stance = CharacterStance.Standing;
                SetColliderDimensions(standingCollider);
                return true;
            }
        }

        return false;
    }

    public bool RequestProning()
    {
        if(stance == CharacterStance.Standing || stance == CharacterStance.Crouching)
        {
            if (!CharacterOverlap(proningCollider))
            {
                walkSpeed = proningSpeed.x;
                runSpeed = proningSpeed.y;
                stance = CharacterStance.Proning;
                SetColliderDimensions(proningCollider);
                return true;
            }
        }
        else
        {
            if (!CharacterOverlap(standingCollider))
            {
                walkSpeed = standingSpeed.x;
                runSpeed = standingSpeed.y;
                stance = CharacterStance.Standing;
                SetColliderDimensions(standingCollider);
                return true;
            }
        }
        return false;
    }

    public void RequestJump()
    {
        if (Grounded())
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void RequestSlide()
    {
        if(inputs.sprintInput && RequestCrouch()){
            rb.AddForce(orientation.forward * slideForce, ForceMode.Impulse);
        }
    }

    #endregion

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
        HandlePlayerMovements();
        HandleCharacterSpeed();
        HandleRigidbody();
        HandleSlope();
    }

    private void HandlePlayerMovements()
    {
        //Apply to move direction
        moveDirection = orientation.forward * inputs.verticalAxis + orientation.right * inputs.horizontalAxis;
    }

    private void HandleCharacterSpeed()
    {
        currentSpeed = inputs.sprintInput ? runSpeed : walkSpeed;
    }

    private void HandleSlope()
    {
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void HandleRigidbody()
    {
        rb.drag = Grounded() ? rbGroundDrag : rbAirDrag;
    }
    
    private bool CharacterOverlap(Vector3 collider)
    {
        if(debug)
        {
            Debug.DrawLine(transform.position, transform.position + (Vector3.up * collider.y), Color.white, 5f);
        }

        return Physics.Raycast(transform.position, transform.up, collider.y + 0.01f, obstructionMask);
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
