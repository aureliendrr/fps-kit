using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private InputManager inputManager;

    [Header("Current Weapon")]
    public Weapon currentWeapon;

    [Header("Weapon Movement Sway")]
    [SerializeField] private Transform weaponMovementSwayObj;
    private Vector3 weaponMovementSwayRot;
    private Vector3 weaponMovementSwayRotVel;
    private Vector3 targetWeaponMovementSwayRot;
    private Vector3 targetWeaponMovementSwayRotVel;

    [Header("Weapon Breathing Sway")]
    [SerializeField] private Transform weaponBreathingSwayObj;
    private float bSwayTime;

    private void Start()
    {
        if (currentWeapon == null)
        {
            Debug.LogError("The current weapon is null");
        }

        if (weaponMovementSwayObj == null)
        {
            Debug.LogError("Need transform for weapon movement sway");
        }

        if (weaponBreathingSwayObj == null)
        {
            Debug.LogError("Need transform for weapon breathing sway");
        }

        SetDefaultValues();
    }

    private void SetDefaultValues()
    {
        inputManager = InputManager.instance;
        weaponMovementSwayRot = weaponMovementSwayObj.localRotation.eulerAngles;
    }

    private void Update()
    {
        HandleWeaponBreathingSway();
        HandleWeaponMovementSway();
    }

    private void HandleWeaponBreathingSway()
    {
        Vector3 targetPos = LissajousCurve(bSwayTime, currentWeapon.bSwayAmountA, currentWeapon.bSwayAmountB) / currentWeapon.bSwayScale;
        bSwayTime += Time.deltaTime;
        if(bSwayTime > 6.3f) { bSwayTime = 0f; }
        weaponBreathingSwayObj.localPosition = Vector3.Lerp(weaponBreathingSwayObj.localPosition, targetPos, currentWeapon.bSwayLerpSpeed * Time.smoothDeltaTime);
    }

    private void HandleWeaponMovementSway()
    {
        targetWeaponMovementSwayRot.y += currentWeapon.mSwayAmountY * (currentWeapon.mSwayXInverted ? -inputManager.AxisRaw("Mouse X") : inputManager.AxisRaw("Mouse X")) * Time.deltaTime;
        targetWeaponMovementSwayRot.x += currentWeapon.mSwayAmountX * (currentWeapon.mSwayYInverted ? inputManager.AxisRaw("Mouse Y") : -inputManager.AxisRaw("Mouse Y")) * Time.deltaTime;
        if (currentWeapon.mSwayZAxis)
            targetWeaponMovementSwayRot.z += currentWeapon.mSwayAmountZ * (currentWeapon.mSwayZInverted ? inputManager.AxisRaw("Mouse X") : -inputManager.AxisRaw("Mouse X")) * Time.deltaTime;

        targetWeaponMovementSwayRot.x = Mathf.Clamp(targetWeaponMovementSwayRot.x, -currentWeapon.mSwayClampX, currentWeapon.mSwayClampX);
        targetWeaponMovementSwayRot.y = Mathf.Clamp(targetWeaponMovementSwayRot.y, -currentWeapon.mSwayClampY, currentWeapon.mSwayClampY);
        targetWeaponMovementSwayRot.z = Mathf.Clamp(targetWeaponMovementSwayRot.z, -currentWeapon.mSwayClampZ, currentWeapon.mSwayClampZ);

        targetWeaponMovementSwayRot = Vector3.SmoothDamp(targetWeaponMovementSwayRot, Vector3.zero, ref targetWeaponMovementSwayRotVel, currentWeapon.mSwayResetSmoothing);
        weaponMovementSwayRot = Vector3.SmoothDamp(weaponMovementSwayRot, targetWeaponMovementSwayRot, ref weaponMovementSwayRotVel, currentWeapon.mSwaySmoothing);

        weaponMovementSwayObj.localRotation = Quaternion.Euler(weaponMovementSwayRot);
    }

    private Vector3 LissajousCurve(float time, float a, float b)
    {
        return new Vector3(Mathf.Sin(time), a * Mathf.Sin(b * time * Mathf.PI));
    }
}
