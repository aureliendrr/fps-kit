using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private PlayerInputs inputs;
    private Rigidbody rb;

    [Header("Weapon")]
    public Weapon currentWeapon;
    private WeaponSettings currentWeaponSettings;

    [Header("Animation")]
    [SerializeField] private WeaponAnimation currentAnimation;

    [Header("Aiming")]
    [SerializeField] private bool debugAim;
    [SerializeField] private bool aim;
    [SerializeField] private Transform weaponAimObj;

    [Header("Recoil")]
    [SerializeField] private Transform weaponRecoilObj;
    
    private Vector3 weaponTargetRotationRecoil;
    private Vector3 weaponCurrentRotationRecoil;

    private Vector2 weaponCurrentNoise;
    private float weaponTargetNoiseRecoil;
    private float weaponCurrentNoiseRecoil;

    private float weaponTargetKickback;
    private float weaponCurrentKickback;

    #region SwaySettings

    [Header("Sway")]
    [SerializeField] private Transform weaponSwayObj;
    private Vector3 weaponSwayRot;
    private Vector3 weaponSwayRotVel;
    private Vector3 targetWeaponSwayRot;
    private Vector3 targetWeaponSwayRotVel;
    private float weaponSwayAimSmoothing;

    private Vector3 weaponMovementSwayRot;
    private Vector3 weaponMovementSwayRotVel;
    private Vector3 targetWeaponMovementSwayRot;
    private Vector3 targetWeaponMovementSwayRotVel;
    private float weaponMovementSwayAimSmoothing;

    private float bSwayTime;

    #endregion

    private void Start()
    {
        if (weaponSwayObj == null)
        {
            Debug.LogError("Need transform for weapon movement sway");
        }

        if (weaponRecoilObj == null)
        {
            Debug.LogError("Need transform for weapon recoil");
        }

        SetDefaultValues();
    }

    private void SetDefaultValues()
    {
        inputs = GetComponent<PlayerInputs>();
        rb = GetComponent<Rigidbody>();
        weaponSwayRot = weaponSwayObj.localRotation.eulerAngles;
        currentWeaponSettings = currentWeapon.settings;
        currentWeapon.InitializeWeapon(this);
    }

    public void UpdateWeapon(Weapon newWeapon)
    {
        currentWeapon = newWeapon;
        currentWeaponSettings = currentWeapon.settings;
    }
 
    private void Update()
    {
        if(currentWeapon == null)
            return;

        HandleAiming();
        HandleWeaponBreathingSway();
        HandleWeaponSway();
        HandleAnimation();
        HandleRecoil();
    }  

    private void HandleWeaponBreathingSway()
    {
        if(!currentWeaponSettings.weaponBreathingSway)
            return;

        Vector3 targetPos = (LissajousCurve(bSwayTime, currentWeaponSettings.bSwayAmountA, currentWeaponSettings.bSwayAmountB) / currentWeaponSettings.bSwayScale) * (aim ? currentWeaponSettings.weaponSwayAimingReducer : 1);
        bSwayTime += Time.deltaTime;
        weaponSwayObj.localPosition = Vector3.Lerp(weaponSwayObj.localPosition, targetPos, currentWeaponSettings.bSwayLerpSpeed * Time.smoothDeltaTime);
    }

    private void HandleWeaponSway()
    {
        //Weapon Sway
        float mouseX = inputs.mouseX;
        float mouseY = inputs.mouseY;

        if(currentWeaponSettings.weaponSway){
            targetWeaponSwayRot.y += currentWeaponSettings.swayAmountY * (currentWeaponSettings.swayXInverted ? -mouseX : mouseX) * Time.deltaTime;
            targetWeaponSwayRot.x += currentWeaponSettings.swayAmountX * (currentWeaponSettings.swayYInverted ? mouseY : -mouseY) * Time.deltaTime;
            if (currentWeaponSettings.swayZAxis)
            targetWeaponSwayRot.z += currentWeaponSettings.swayAmountZ * (currentWeaponSettings.swayZInverted ? mouseX : -mouseX) * Time.deltaTime;

            targetWeaponSwayRot.x = Mathf.Clamp(targetWeaponSwayRot.x, -currentWeaponSettings.swayClampX, currentWeaponSettings.swayClampX);
            targetWeaponSwayRot.y = Mathf.Clamp(targetWeaponSwayRot.y, -currentWeaponSettings.swayClampY, currentWeaponSettings.swayClampY);
            targetWeaponSwayRot.z = Mathf.Clamp(targetWeaponSwayRot.z, -currentWeaponSettings.swayClampZ, currentWeaponSettings.swayClampZ);

            targetWeaponSwayRot = Vector3.SmoothDamp(targetWeaponSwayRot, Vector3.zero, ref targetWeaponSwayRotVel, currentWeaponSettings.swayResetSmoothing);
            weaponSwayRot = Vector3.SmoothDamp(weaponSwayRot, targetWeaponSwayRot, ref weaponSwayRotVel, currentWeaponSettings.swaySmoothing);
        
            weaponSwayAimSmoothing = Mathf.Lerp(weaponSwayAimSmoothing, (aim ? currentWeaponSettings.weaponSwayAimingReducer : 1),
                (aim ? currentWeaponSettings.weaponNormalToAimSpeed : currentWeaponSettings.weaponAimToNormalSpeed) * Time.deltaTime);
        
        }else{
            weaponSwayRot = Vector3.zero;
            weaponSwayAimSmoothing = 1;
        }

        //Weapon Movement Sway

        if(currentWeaponSettings.weaponMovementSway)
        {
            targetWeaponMovementSwayRot.x = rb.velocity.y * (currentWeaponSettings.mSwayInverted ? -currentWeaponSettings.mSwayAmount : currentWeaponSettings.mSwayAmount);
            targetWeaponMovementSwayRot.x = Mathf.Clamp(targetWeaponMovementSwayRot.x, -currentWeaponSettings.mSwayClamp, currentWeaponSettings.mSwayClamp);
            
            targetWeaponMovementSwayRot = Vector3.SmoothDamp(targetWeaponMovementSwayRot, Vector3.zero, ref weaponMovementSwayRotVel, currentWeaponSettings.mSwayResetSmoothing);
            weaponMovementSwayRot = Vector3.SmoothDamp(weaponMovementSwayRot, targetWeaponMovementSwayRot, ref weaponMovementSwayRotVel, currentWeaponSettings.mSwaySmoothing);

             weaponMovementSwayAimSmoothing = Mathf.Lerp(weaponMovementSwayAimSmoothing, (aim ? currentWeaponSettings.weaponMovementSwayAimingReducer : 1),
                (aim ? currentWeaponSettings.weaponNormalToAimSpeed : currentWeaponSettings.weaponAimToNormalSpeed) * Time.deltaTime);
        }else{
            weaponMovementSwayRot = Vector3.zero;
            weaponMovementSwayAimSmoothing = 1;
        }

        weaponSwayAimSmoothing = Mathf.Lerp(weaponSwayAimSmoothing, (aim ? currentWeaponSettings.weaponSwayAimingReducer : 1),
            (aim ? currentWeaponSettings.weaponNormalToAimSpeed : currentWeaponSettings.weaponAimToNormalSpeed) * Time.deltaTime);
        
        //Apply both
        weaponSwayObj.localRotation = Quaternion.Euler((weaponSwayRot * weaponSwayAimSmoothing) + (weaponMovementSwayRot * weaponMovementSwayAimSmoothing));
    }

    private Vector3 LissajousCurve(float time, float a, float b)
    {
        return new Vector3(Mathf.Sin(time), a * Mathf.Sin(b * time * Mathf.PI));
    }

    private void HandleAiming()
    {
        aim = inputs.aimInput || debugAim;

        if(aim){
            weaponAimObj.localPosition = Vector3.Lerp(weaponAimObj.localPosition, currentWeaponSettings.weaponAimPos, currentWeaponSettings.weaponNormalToAimSpeed * Time.deltaTime);
        }else{
            weaponAimObj.localPosition = Vector3.Lerp(weaponAimObj.localPosition, currentWeaponSettings.weaponNormalPos, currentWeaponSettings.weaponAimToNormalSpeed * Time.deltaTime);
        }
    }

    private void HandleAnimation()
    {
        
    }

    private void ApplyAnimation()
    {

    }

    public void RequestShoot()
    {
        currentWeapon?.Shoot();
    }

    private void FixedUpdate()
    {
        //HandleRecoil();
    }

    public void RequestRecoil()
    {
        WeaponRecoil recoil = currentWeaponSettings.recoil;

        float rotX = Random.Range(recoil.pitch.x, recoil.pitch.y);
        float rotY = Random.Range(recoil.yaw.x, recoil.yaw.y);
        float rotZ = Random.Range(recoil.roll.x, recoil.roll.y);
        weaponTargetRotationRecoil += new Vector3(rotX, rotY, rotZ);

        float kickBack = Random.Range(recoil.kickBackAmount.x, recoil.kickBackAmount.y);
        weaponTargetKickback = kickBack;
    }

    private void HandleRecoil()
    {
        WeaponRecoil recoil = currentWeaponSettings.recoil;

        weaponTargetRotationRecoil = Vector3.Lerp(weaponTargetRotationRecoil, Vector3.zero, recoil.rotationDamp * Time.deltaTime);
        weaponCurrentRotationRecoil = Vector3.Lerp(weaponCurrentRotationRecoil, weaponTargetRotationRecoil, recoil.rotationAccel * Time.deltaTime);
        weaponRecoilObj.localRotation = Quaternion.Euler(weaponCurrentRotationRecoil);

        weaponTargetKickback = Mathf.Lerp(weaponTargetKickback, 0f, recoil.kickBackDamp * Time.fixedDeltaTime);
        weaponCurrentKickback = Mathf.Lerp(weaponCurrentKickback, weaponTargetKickback, recoil.kickBackAccel * Time.fixedDeltaTime);

        Vector3 newPos = weaponRecoilObj.localPosition;
        newPos.z = weaponCurrentKickback;
        weaponRecoilObj.localPosition = newPos;
    }
}