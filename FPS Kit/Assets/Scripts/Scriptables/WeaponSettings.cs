using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Settings", menuName = "FPS Kit/Weapon Settings")]
public class WeaponSettings : ScriptableObject
{
    [Header("General")]
    public string weaponName;
    public float rpm;
    public bool canSingle = true;
    public bool canFullauto = true;
    public bool canBurst;
    public int bulletsPerMag;
    public int maxBullets;
    public WeaponRecoil recoil;

    [Header("Animations")]
    public WeaponAnimation walkAnimation;
    public WeaponAnimation runAnimation;

    [Header("Aiming")]
    public Vector3 weaponPivotPos;
    public Vector3 weaponNormalPos;
    public Vector3 weaponAimPos;
    public float weaponNormalToAimSpeed;
    public float weaponAimToNormalSpeed;

    [Header("Sway")]
    public bool weaponSway;
    public float swayAmountX = 100f;
    public float swayAmountY = 100f;
    public float swayAmountZ = 100f;
    public float swaySmoothing = 100f;
    public float swayResetSmoothing = 1f;
    public bool swayZAxis = true;
    public bool swayXInverted = false;
    public bool swayYInverted = false;
    public bool swayZInverted = false;
    public float swayClampX;
    public float swayClampY;
    public float swayClampZ;
    public float weaponSwayAimingReducer;

    [Header("Movement Sway")]
    public bool weaponMovementSway;
    public float mSwayAmount;
    public bool mSwayInverted;
    public float mSwayClamp;
    public float mSwaySmoothing = 100f;
    public float mSwayResetSmoothing = 1f;
    public float weaponMovementSwayAimingReducer;

    [Header("Breathing Sway")]
    public bool weaponBreathingSway;
    public float bSwayAmountA = 1f;
    public float bSwayAmountB = 1f;
    public float bSwayScale = 600f;
    public float bSwayLerpSpeed = 14f;
    public float weaponBreathingSwayAimingReducer;
}

[System.Serializable]
public class WeaponAnimation
{
    [Header("Timing")]
    public float speed;

    [Header("Position")]
    public AnimationCurve posX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve posY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve posZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    
    [Header("Rotation")]
    public AnimationCurve rotX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve rotY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve rotZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
}

[System.Serializable]
public class WeaponRecoil
{
    [Header("Aiming")]
    public float aimMultiplier;

    [Header("Rotations")]
    public Vector2 pitch;
    public Vector2 yaw;
    public Vector2 roll;
    public float rotationAccel;
    public float rotationDamp;

    [Header("Kick back")]
    public Vector2 kickBackAmount;
    public float kickBackAccel;
    public float kickBackDamp;
    
    /*
    [Header("Noise")]
    public Vector2 noiseX;
    public Vector2 noiseY;
    public float noiseAccel;
    public float noiseDamp;
    public float noiseScale;
    */


    [Header("Look Settings")]
    public AnimationCurve recoilCurveX;
    public AnimationCurve recoilCurveY;
}

public enum WeaponMode
{
    Single,
    FullAuto,
    Burst
}