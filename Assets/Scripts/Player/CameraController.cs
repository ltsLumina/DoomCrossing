#region
using UnityEngine;
#endregion

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] Vector2 sensitivity = new (1f, 1f);
    [SerializeField] float baseFov = 90f;
    [SerializeField] float maxFov = 140f;
    [SerializeField] float currentFOV;
    [SerializeField] float wallRunTilt = 15f;

    [Header("Cached References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera weaponCamera;

    float wishTilt;
    float curTilt;
    Vector2 currentLook;
    Vector2 sway = Vector2.zero;

    public float FOV
    {
        get => currentFOV;
        set => currentFOV = value;
    }

    Rigidbody rb;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        curTilt = transform.localEulerAngles.z;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() { RotateMainCamera(); }

    void FixedUpdate()
    {
        float addedFov = rb.velocity.magnitude - 3.44f;
        //FOV = Mathf.Lerp(FOV, baseFov + addedFov, 0.5f);
        FOV = Mathf.Clamp(FOV, baseFov, maxFov);

        mainCamera.fieldOfView = FOV;
        weaponCamera.fieldOfView = FOV;

        currentLook = Vector2.Lerp(currentLook, currentLook + sway, 0.8f);
        curTilt = Mathf.LerpAngle(curTilt, wishTilt * wallRunTilt, 0.05f);

        sway = Vector2.Lerp(sway, Vector2.zero, 0.2f);
    }

    void RotateMainCamera()
    {
        var mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        mouseInput.x *= sensitivity.x;
        mouseInput.y *= sensitivity.y;

        currentLook.x += mouseInput.x;
        currentLook.y = Mathf.Clamp(currentLook.y += mouseInput.y, -90, 90);

        transform.localRotation                = Quaternion.AngleAxis(-currentLook.y, Vector3.right);
        transform.localEulerAngles             = new (transform.localEulerAngles.x, transform.localEulerAngles.y, curTilt);
        transform.root.transform.localRotation = Quaternion.Euler(0, currentLook.x, 0);
    }

    public void Punch(Vector2 dir) { sway += dir; }

    #region Setters
    public void SetTilt(float newTilt) { wishTilt = newTilt; }

    public void SetSensitivity(Vector2 newSensitivity) { sensitivity = newSensitivity; }

    public void SetFov(float newFov) { baseFov = newFov; }
    public void SetADSFov(float newFov) { FOV = newFov; }
    #endregion
}