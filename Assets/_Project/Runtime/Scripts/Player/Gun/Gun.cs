#region
using System;
using System.Collections;
using Lumina.Essentials;
using Lumina.Essentials.Sequencer;
using UnityEngine;
using static Interfaces;
using Random = UnityEngine.Random;
#endregion

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] float range = 100f;
    [SerializeField] float shootDelay;
    [SerializeField] int damage = 35;

    [Header("Bullet")]
    [SerializeField] float bulletForce = 15f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform barrelExit;

    [Header("Read-Only Fields")]
    [SerializeField] bool canFire;
    [SerializeField] AudioSource gunSFX;

    // Cached References.
    Magazine magazine;
    Camera playerCam;
    Aim_Down_Sights aimDownSights;
    GunParticleManager gunParticleManager;
    GameObject bulletsFired;
    TimeManager timeManager;

    float shootDelayBeforeTimescale;

    // onshoot event
    public delegate void OnShoot();
    public event OnShoot onShoot;

    // Cached Hashes
    readonly static int DoShoot = Animator.StringToHash("doShoot");

    public Animator GunAnim { get; private set; }

    public float ShootDelay
    {
        get => shootDelay;
        set => shootDelay  = value;
    }

    public bool CanFire
    { 
        get => canFire; 
        set => canFire = value; 
    }

    void Start()
    {
        // subscribe to the onShoot event
        onShoot += Shoot;

        magazine            = GetComponent<Magazine>();
        GunAnim             = FindObjectOfType<GunAnimationEvents>().GetComponent<Animator>();
        playerCam           = FindObjectOfType<Camera>();
        aimDownSights       = FindObjectOfType<Aim_Down_Sights>();
        gunParticleManager  = FindObjectOfType<GunParticleManager>();
        timeManager         = FindObjectOfType<TimeManager>();

        // Create a header for the bullets fired.
        bulletsFired = new GameObject("Bullets Fired");

        // Set the gun to be able to fire.
        CanFire = true;

        // Set the current magazine to the maximum size.
        magazine.CurrentMagCount = magazine.MaxMagazineSize;

        // Set the shoot delay before timescale.
        shootDelayBeforeTimescale = shootDelay;
    }

    void Update()
    {
        // "Fire1" == Left mouse button.
        if (Input.GetButtonDown("Fire1") && magazine.CurrentMagCount > 0 && CanFire && !magazine.Reloading())
        {
            onShoot?.Invoke();
        }
    }

    void Shoot()
    {
        var shootSequence = new Sequence(this);
        shootSequence.Execute(Fire).ContinueWith(() =>
        {
            ShootDelayUnscaled();
            CanFire = true;
        });

        return;
        void Fire()
        {
            CanFire = false;
            gunSFX.Play();

            StartCoroutine(DeductTimeScalePerShot());

            // Start the shoot animation.
            GunAnim.SetTrigger(DoShoot);
            gunParticleManager.GunParticles();

            // Reduce ammo and update the text displayed on the gun.
            magazine.CurrentMagCount--;
            magazine.UpdateAmmoText();

            // instantiate a bullet and add force towards the hit object
            GameObject bullet = Instantiate(bulletPrefab, barrelExit.position, barrelExit.rotation);
            bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bulletForce, ForceMode.Impulse);

            bullet.transform.parent = bulletsFired.transform;
            Destroy(bullet, 3.5f);

            // Raycast to a distance of 100 units and debug the name of the object hit.
            if (!Physics.Raycast
                (playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit, range)) return;

            Debug.Log($"Hit: {hit.transform.name}");

            switch (hit.transform.tag)
            {
                case "Enemy":
                    // Get the health component from the object hit.
                    if (hit.transform.TryGetComponent(out IDamageable component))
                        // Do on-hit logic, such as damaging the enemy etc.
                        component.TakeDamage(damage);
                    break;

                case "DroppedGun":
                    // Add force to the dropped gun in the direction of the raycast
                    Rigidbody droppedGunRB = hit.transform.GetComponent<Rigidbody>();
                    droppedGunRB.AddForce(playerCam.transform.forward * 6, ForceMode.Impulse);
                    droppedGunRB.AddForce(transform.up                * 8, ForceMode.Impulse);
                    droppedGunRB.AddTorque
                    (new (Random.Range(-20, 20), Random.Range(-20, 20), Random.Range(-20, 20)),
                     ForceMode.Impulse);
                    break;
            }
        }
    }

    public float ShootDelayUnscaled()
    {
        shootDelayBeforeTimescale = shootDelay;

        // Adjust the shoot delay based on the time scale
        shootDelay = Time.timeScale < 1 ? shootDelay * shootDelayBeforeTimescale : shootDelayBeforeTimescale;

        return shootDelay;
    }

    IEnumerator DeductTimeScalePerShot()
    {
        while (Math.Abs(Time.timeScale - 1) > 0.001)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, 0.25f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        timeManager.SlowdownFactor = 0.05f;
    }
}