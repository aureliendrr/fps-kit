using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponSettings settings;

    [Header("References")]
    public Transform leftHandIKTarget;
    public Transform rightHandIKTarget;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Transform ejectionPoint;

    [Header("Use")]
    public WeaponMode weaponMode;

    [Header("Ammo")]
    [SerializeField] private int bullets;
    [SerializeField] private int bulletsLeft;

    [Header("Shoot")]
    [SerializeField] private float fireRate;
    private float fireTimer;

    //References
    private PlayerWeapon playerWeapon;
    
    public void InitializeWeapon(PlayerWeapon playerWeapon)
    {
        this.playerWeapon = playerWeapon;

        bullets = settings.bulletsPerMag;
        bulletsLeft = settings.maxBullets;

        fireRate = 1 / (settings.rpm / 60);
        fireTimer = 0.0f;
    }

    private void Update()
    {
        HandleShoot();
    }

    private void HandleShoot()
    {
        if(fireTimer <= fireRate)
        {
            fireTimer += Time.deltaTime;
        }
    }

    public void Shoot()
    {
        if(fireTimer < fireRate || bullets <= 0)
            return;

        Debug.Log("Shoot");
        bullets--;

        playerWeapon.RequestRecoil();

        Rigidbody sheelRb = Instantiate(settings.shellPrefab, ejectionPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
        sheelRb.AddForce(ejectionPoint.forward * settings.shellEjectionForce, ForceMode.Impulse);
        sheelRb.AddTorque(Vector3.up * settings.shellEjectionTorque, ForceMode.VelocityChange);
        Destroy(sheelRb.gameObject, settings.destroyShellAfter);

        fireTimer = 0.0f;
    }
}
