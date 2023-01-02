using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
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

    private Vector3 aimOrigiPos;
    private float normalToAimTime;
    private float aimToNormalTime;

    [Header("Recoil")]
    [SerializeField] private Transform weaponRecoilObj;
    [SerializeField] private Transform cameraRecoilObj;
    
    private Vector3 weaponTargetRotationRecoil;
    private Vector3 weaponCurrentRotationRecoil;
    private float weaponTargetKickback;
    private float weaponCurrentKickback;

    private Vector3 cameraTargetRotationRecoil;
    private Vector3 cameraCurrentRotationRecoil;

    #region SwaySettings

    [Header("Sway")]
    [SerializeField] private Transform weaponSwayObj;
    private Vector3 weaponSwayRot;
    private Vector3 weaponSwayRotVel;
    private Vector3 targetWeaponSwayRot;
    private Vector3 targetWeaponSwayRotVel;

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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        //Change with the futur Input Handler
        if(Input.GetKey(KeyCode.Mouse1) || debugAim) { RequestStartAiming(); }
        else { RequestStopAiming(); }

        HandleWeaponBreathingSway();
        HandleWeaponSway();
        HandleRecoil();
        HandleShoot();
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
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

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
        
        }else{
            weaponSwayRot = Vector3.zero;
        }

        //Weapon Movement Sway

        if(currentWeaponSettings.weaponMovementSway)
        {
            targetWeaponMovementSwayRot.x = rb.velocity.y * (currentWeaponSettings.mSwayInverted ? -currentWeaponSettings.mSwayAmount : currentWeaponSettings.mSwayAmount);
            targetWeaponMovementSwayRot.x = Mathf.Clamp(targetWeaponMovementSwayRot.x, -currentWeaponSettings.mSwayClamp, currentWeaponSettings.mSwayClamp);
            
            targetWeaponMovementSwayRot = Vector3.SmoothDamp(targetWeaponMovementSwayRot, Vector3.zero, ref weaponMovementSwayRotVel, currentWeaponSettings.mSwayResetSmoothing);
            weaponMovementSwayRot = Vector3.SmoothDamp(weaponMovementSwayRot, targetWeaponMovementSwayRot, ref weaponMovementSwayRotVel, currentWeaponSettings.mSwaySmoothing);

        }else{
            weaponMovementSwayRot = Vector3.zero;
        }
        
        //Apply both
        weaponSwayObj.localRotation = Quaternion.Euler((weaponSwayRot) + (weaponMovementSwayRot));
    }

    private Vector3 LissajousCurve(float time, float a, float b)
    {
        return new Vector3(Mathf.Sin(time), a * Mathf.Sin(b * time * Mathf.PI));
    }

    public void RequestStartAiming()
    {
        aim = true;
        aimOrigiPos = weaponAimObj.localPosition;
    }

    public void RequestStopAiming()
    {
        aim = false;
        aimOrigiPos = weaponAimObj.localPosition;
    }

    private void HandleShoot()
    {
        if(Input.GetKey(KeyCode.Mouse0))
        {
            RequestShoot();
        }
    }

    public void RequestShoot()
    {
        currentWeapon?.Shoot();
    }

    public void RequestRecoil()
    {
        WeaponRecoil recoil = currentWeaponSettings.recoil;

        float weaponTargetRotX = Random.Range(recoil.pitch.x, recoil.pitch.y);
        float weaponTargetRotY = Random.Range(recoil.yaw.x, recoil.yaw.y);
        float weaponTargetRotZ = Random.Range(recoil.roll.x, recoil.roll.y);
        weaponTargetRotationRecoil += new Vector3(weaponTargetRotX, weaponTargetRotY, weaponTargetRotZ);

        float kickBack = Random.Range(recoil.kickBackAmount.x, recoil.kickBackAmount.y);
        weaponTargetKickback = kickBack;

        float cameraTargetRotX = Random.Range(recoil.lookPitch.x, recoil.lookPitch.y);
        float cameraTargetRotY = Random.Range(recoil.lookYaw.x, recoil.lookYaw.y);
        float cameraTargetRotZ = Random.Range(recoil.lookRoll.x, recoil.lookRoll.y);
        cameraTargetRotationRecoil += new Vector3(cameraTargetRotX, cameraTargetRotY, cameraTargetRotZ);
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

        cameraTargetRotationRecoil = Vector3.Lerp(cameraTargetRotationRecoil, Vector3.zero, recoil.lookRotationDamp * Time.deltaTime);
        cameraCurrentRotationRecoil = Vector3.Lerp(cameraCurrentRotationRecoil, cameraTargetRotationRecoil, recoil.lookRotationAccel * Time.deltaTime);
        cameraRecoilObj.localEulerAngles = cameraCurrentRotationRecoil;
    }

    private void FixedUpdate()
    {
        HandleAiming();
    }

    private void HandleAiming()
    {
        if(aim)
        {
            aimToNormalTime = 0.01f;
            if(normalToAimTime <= currentWeaponSettings.weaponNormalToAimTime)
            {
                float percent = Mathf.Clamp01(normalToAimTime / currentWeaponSettings.weaponNormalToAimTime);
                float percentCurve = currentWeaponSettings.weaponNormalToAimCurve.Evaluate(percent);
                weaponAimObj.localPosition = Vector3.Lerp(aimOrigiPos, currentWeaponSettings.weaponAimPos, percentCurve);
                normalToAimTime += Time.deltaTime;
            }
        }
        else
        {
            normalToAimTime = 0.01f;
            if(aimToNormalTime <= currentWeaponSettings.weaponAimToNormalTime)
            {
                float percent = Mathf.Clamp01(aimToNormalTime / currentWeaponSettings.weaponAimToNormalTime);
                float percentCurve = currentWeaponSettings.weaponAimToNormalCurve.Evaluate(percent);
                weaponAimObj.localPosition = Vector3.Lerp(aimOrigiPos, currentWeaponSettings.weaponNormalPos, percent);
                aimToNormalTime += Time.deltaTime;
            }
        }
    }
}