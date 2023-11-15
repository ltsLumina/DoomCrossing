#region
using UnityEngine;
#endregion

public class ViewBobbing : MonoBehaviour
{
    [Header("View Bobbing Options")]
    [SerializeField] float bobbingSpeed = 0.18f;       // Speed of the bobbing effect
    [SerializeField] float bobbingAmount = 0.2f;       // Amount of bobbing effect
    [SerializeField] float sprintBobbingSpeed = 0.24f; // Speed of bobbing while sprinting
    [SerializeField] float sprintBobbingAmount = 0.3f;                 // Amount of bobbing while sprinting

    [SerializeField,Header("Toggleable Options")]
    bool enableSprintBobbing = true;

    // Cached References
    PlayerMovement player;
    Rigidbody playerRB;
    float defaultPosY;
    float timer;
    bool isSprinting;

    void Start()
    {
        player = GetComponentInParent<PlayerMovement>();
        playerRB = player.GetComponent<Rigidbody>();
        defaultPosY         = transform.localPosition.y;
    }

    void Update()
    {
        if (!enableSprintBobbing) return;

        float speed      = playerRB.velocity.magnitude;
        bool  isGrounded = player.IsGrounded;

        if (isGrounded && speed > 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Sprinting
                timer += Time.deltaTime * sprintBobbingSpeed;

                var localPosition = transform.localPosition;

                localPosition = new Vector3
                (localPosition.x, defaultPosY + Mathf.Sin(timer) * sprintBobbingAmount,
                 localPosition.z);

                transform.localPosition = localPosition;
            }
            else
            {
                // Walking/Running
                timer += Time.deltaTime * bobbingSpeed;

                var localPosition = transform.localPosition;

                localPosition = new Vector3
                (localPosition.x, defaultPosY + Mathf.Sin(timer) * bobbingAmount,
                 localPosition.z);

                transform.localPosition = localPosition;
            }
        }
        else
        {
            // Not moving or in the air
            timer = 0f;
            var localPosition = transform.localPosition;
            localPosition           = new Vector3(localPosition.x, defaultPosY, localPosition.z);
            transform.localPosition = localPosition;
        }
    }
}