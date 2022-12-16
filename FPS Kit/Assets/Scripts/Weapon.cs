using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "FPS Kit/Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Weapon Movement Sway")]
    public float mSwayAmountX = 100f;
    public float mSwayAmountY = 100f;
    public float mSwayAmountZ = 100f;
    public float mSwaySmoothing = 100f;
    public float mSwayResetSmoothing = 1f;
    public bool mSwayZAxis = true;
    public bool mSwayXInverted = false;
    public bool mSwayYInverted = false;
    public bool mSwayZInverted = false;
    public float mSwayClampX;
    public float mSwayClampY;
    public float mSwayClampZ;

    [Header("Weapon Breathing Sway")]
    public float bSwayAmountA = 1f;
    public float bSwayAmountB = 1f;
    public float bSwayScale = 600f;
    public float bSwayLerpSpeed = 14f;
}
