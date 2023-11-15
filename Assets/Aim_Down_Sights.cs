using UnityEngine;

public class Aim_Down_Sights : MonoBehaviour
{
    [SerializeField] Transform activeWeapon;

    [SerializeField] Transform defaultPosition;
    [SerializeField] Transform adsPosition;
    [SerializeField] Vector3 weaponPosition; // set to 0 0 0 in inspector

    [SerializeField] float aimSpeed = 0.25f;  // time to enter ADS
    [SerializeField] float defaultFOV = 80f; // FOV in degrees
    [SerializeField] float zoomRatio = 0.5f;  // 1/zoom times

    [SerializeField] CameraController fpsCam; // player camera

    // Cached References
    Animator gunAnimator;
    TimeManager timeManager;
    PlayerMovement player;

    // Cached Hashes
    readonly static int IsADS = Animator.StringToHash("isADS");

    void Start()
    {
        gunAnimator = FindObjectOfType<Gun>().GetComponentInChildren<Animator>();
        timeManager = FindObjectOfType<TimeManager>();
        player      = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        // ADS camera and gun movement
        if(Input.GetButton("Fire2"))
        {
            weaponPosition = Vector3.Lerp(weaponPosition, adsPosition.localPosition, aimSpeed * Time.unscaledDeltaTime);
            activeWeapon.localPosition = weaponPosition;
            SetFieldOfView(Mathf.Lerp(fpsCam.FOV, zoomRatio * defaultFOV, aimSpeed * Time.unscaledDeltaTime));

            // slow down idle animation
            gunAnimator.SetBool(IsADS, true);

            // Allow witch time while in the air and aiming.
            if (!player.IsGrounded) timeManager.DoSlowMotion();
        }
        else
        {
            weaponPosition = Vector3.Lerp(weaponPosition, defaultPosition.localPosition, aimSpeed * Time.unscaledDeltaTime);
            activeWeapon.localPosition = weaponPosition;
            SetFieldOfView(Mathf.Lerp(fpsCam.FOV, defaultFOV, aimSpeed * Time.unscaledDeltaTime));
            gunAnimator.SetBool(IsADS, false);
            TimeManager.ResetTimeScale();
        }
    }

    void SetFieldOfView(float fov)
    {
        fpsCam.FOV = fov;
    }
}