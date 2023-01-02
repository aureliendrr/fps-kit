using UnityEngine;

[CreateAssetMenu(fileName = "Player Settings", menuName = "FPS Kit/Player Settings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Abilities")]
    public bool canCrouch;
    public bool canProne;
    public bool canJump;
    public bool canSlide;

    [Header("Custom")]
    public int fov = 65;
    public float sensitivityX = 4f;
    public float sensitivityY = 4f;
}