#region
using System;
using System.Collections;
using UnityEngine;
#endregion

public class PlayerMovement : MonoBehaviour
{
    //Ground
    [Header("Ground")]
    [SerializeField] float groundSpeed = 4f;
    [SerializeField] float runSpeed = 6f;
    [SerializeField] float groundAccel = 20f;
    [SerializeField] AudioSource walkSFX;

    //Air
    [Header("Airborne")]
    [SerializeField] float airSpeed = 3f;
    [SerializeField] float airAccel = 20f;

    //Jump
    [Header("Jumping")]
    [SerializeField] float jumpUpSpeed = 9.2f;
    [SerializeField] float dashSpeed = 6f;

    //Wall
    [Header("Walls")]
    [SerializeField] float wallSpeed = 10f;
    [SerializeField] float wallClimbSpeed = 4f;
    [SerializeField] float wallAccel = 20f;
    [SerializeField] float wallRunTime = 3f;
    [SerializeField] float wallStickiness = 20f;
    [SerializeField] float wallStickDistance = 1f;
    [SerializeField] float wallFloorBarrier = 40f;
    [SerializeField] float wallBanTime = 4f;

    Vector3 bannedGroundNormal;

    //Cooldowns
    [Header("Cooldowns ()")]
    [SerializeField] bool canJump = true;
    [SerializeField] bool canDJump = true;
    [SerializeField] float wallBan;
    [SerializeField] float wrTimer;
    [SerializeField] float wallStickTimer;

    //States
    [Header("States ()")]
    [SerializeField] bool running;
    [SerializeField] bool jump;
    [SerializeField] bool crouched;
    [SerializeField] bool isGrounded;

     public bool IsGrounded
    {
        get => isGrounded;
        private set => isGrounded = value;
    }

    public bool IsJumping { get; set; }

    Collider ground;
    Vector3 groundNormal = Vector3.up;
    CapsuleCollider col;
    PlayerParticles playerParticles;

    enum Mode
    {
        Walking,
        Flying,
        Wallrunning,
    }

    Mode mode = Mode.Flying;

    CameraController camCon;
    Rigidbody rb;
    Vector3 dir = Vector3.zero;

    bool isWalking;
    bool isWallRunning;

    void Start()
    {
        rb              = GetComponent<Rigidbody>();
        camCon          = GetComponentInChildren<CameraController>();
        col             = GetComponent<CapsuleCollider>();
        playerParticles = FindObjectOfType<PlayerParticles>();
    }

    void OnGUI()
    {
        var velocity = rb.velocity;
        GUILayout.Label("Speed: "   + new Vector3(velocity.x, 0, velocity.z).magnitude);
        GUILayout.Label("SpeedUp: " + rb.velocity.y);
    }

    void Update()
    {
        col.material.dynamicFriction = 0f;
        dir                          = Direction();

        running  = Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0.9;
        crouched = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        if (Input.GetKeyDown(KeyCode.Space)) jump = true;

        //Special use
        //if (Input.GetKeyDown(KeyCode.T)) transform.position = new Vector3(0f, 30f, 0f);
        //if (Input.GetKeyDown(KeyCode.X)) rb.velocity = new Vector3(rb.velocity.x, 40f, rb.velocity.z);
        //if (Input.GetKeyDown(KeyCode.V)) rb.AddForce(dir * 20f, ForceMode.VelocityChange);
    }

    void FixedUpdate()
    {
        col.height = crouched
            ? Mathf.Max(0.6f, col.height - Time.deltaTime * 10f)
            : Mathf.Min(1.8f, col.height + Time.deltaTime * 10f);

        if (wallStickTimer == 0f && wallBan > 0f) bannedGroundNormal = groundNormal;
        else bannedGroundNormal                                      = Vector3.zero;

        wallStickTimer = Mathf.Max(wallStickTimer - Time.deltaTime, 0f);
        wallBan        = Mathf.Max(wallBan        - Time.deltaTime, 0f);

        switch (mode)
        {
            case Mode.Wallrunning:
                playerParticles.StopParticle(PlayerParticles.Type.WallRunning);
                camCon.SetTilt(WallrunCameraAngle());
                Wallrun(dir, wallSpeed, wallClimbSpeed, wallAccel);
                playerParticles.ParticlePlayer(PlayerParticles.Type.WallRunning);
                if (!ground.CompareTag("InfiniteWallrun")) wrTimer = Mathf.Max(wrTimer - Time.deltaTime, 0f);
                break;

            case Mode.Walking:
                playerParticles.StopParticle(PlayerParticles.Type.WallRunning);
                walkSFX.Play();
                camCon.SetTilt(0);
                Walk(dir, running ? runSpeed : groundSpeed, groundAccel);
                playerParticles.ParticlePlayer(PlayerParticles.Type.Running);
                break;

            case Mode.Flying:
                playerParticles.StopParticle(PlayerParticles.Type.WallRunning);
                camCon.SetTilt(0);
                AirMove(dir, airSpeed, airAccel);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        jump = false;
    }

    Vector3 Direction()
    {
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        var direction = new Vector3(hAxis, 0, vAxis);
        return rb.transform.TransformDirection(direction);
    }

    #region Collisions
    void OnCollisionStay(Collision collision)
    {
        if (collision.contactCount <= 0)
            return;

        bool foundGround = false;
        bool foundWall   = false;
        bool walkingMode = mode == Mode.Walking;

        foreach (ContactPoint contact in collision.contacts)
        {
            float angle = Vector3.Angle(contact.normal, Vector3.up);

            if (angle < wallFloorBarrier)
            {
                if (walkingMode)
                    return;

                EnterWalking();
                IsGrounded     = true;
                groundNormal = contact.normal;
                ground       = contact.otherCollider;
                foundGround  = true;
                break;
            }

            if (foundWall || !(angle < 120f) || contact.otherCollider.CompareTag("NoWallrun") ||
                contact.otherCollider.CompareTag("Player")) continue;

            IsGrounded     = true;
            groundNormal = contact.normal;
            ground       = contact.otherCollider;
            EnterWallrun();
            foundWall = true;
        }

        if (!foundGround && VectorToGround().sqrMagnitude > 0.2f * 0.2f)
            IsGrounded = false;
    }


    void OnCollisionExit(Collision collision)
    {
        if (collision.contactCount == 0) EnterFlying();
    }
    #endregion

    #region Entering States
    void EnterWalking()
    {
        if (mode != Mode.Walking && canJump)
        {
            if (mode == Mode.Flying && crouched) rb.AddForce(-rb.velocity.normalized, ForceMode.VelocityChange);

            if (rb.velocity.y < -1.2f)
            {
                //camCon.Punch(new Vector2(0, -3f));
            }

            //StartCoroutine(bHopCoroutine(bhopLeniency));
            //gameObject.SendMessage("OnStartWalking");
            mode = Mode.Walking;
        }
    }

    void EnterFlying(bool wishFly = false)
    {
        IsGrounded = false;
        if (mode == Mode.Wallrunning && VectorToWall().magnitude < wallStickDistance && !wishFly) return;

        if (mode != Mode.Flying)
        {
            wallBan  = wallBanTime;
            canDJump = true;
            mode     = Mode.Flying;
        }
    }

    void EnterWallrun()
    {
        if (mode != Mode.Wallrunning)
        {
            if (VectorToGround().magnitude > 0.2f && CanRunOnThisWall(bannedGroundNormal) && wallStickTimer == 0f)
            {
                //gameObject.SendMessage("OnStartWallrunning");
                wrTimer  = wallRunTime;
                canDJump = true;
                mode     = Mode.Wallrunning;
            }
            else { EnterFlying(true); }
        }
    }
    #endregion

    #region Movement Types
    void Walk(Vector3 wishDir, float maxSpeed, float acceleration)
    {
        if (jump && canJump)
        {
            //gameObject.SendMessage("OnJump");
            Jump();
        }
        else
        {
            //if (crouched) acceleration = 0.5f;
            wishDir = wishDir.normalized;
            Vector3 velocity = rb.velocity;

            var speed = new Vector3(velocity.x, 0f, velocity.z);
            if (speed.magnitude > maxSpeed) acceleration *= speed.magnitude / maxSpeed;

            Vector3 direction = wishDir * maxSpeed - speed;

            if (direction.magnitude < 0.5f) acceleration *= direction.magnitude / 0.5f;

            direction = direction.normalized * acceleration;
            float magnitude = direction.magnitude;
            direction =  direction.normalized;
            direction *= magnitude;

            Vector3 slopeCorrection = groundNormal * Physics.gravity.y / groundNormal.y;
            slopeCorrection.y = 0f;
            //if(!crouched)
            direction += slopeCorrection;

            rb.AddForce(direction, ForceMode.Acceleration);
        }
    }

    void AirMove(Vector3 wishDir, float maxSpeed, float acceleration)
    {
        if (jump && !crouched)
        {
            //gameObject.SendMessage("OnDoubleJump");
            DoubleJump(wishDir);
        }

        if (crouched && rb.velocity.y > -10 && Input.GetKey(KeyCode.Space))
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);

        var velocity = rb.velocity;

        float projVel =
            Vector3.Dot(new (velocity.x, 0f, velocity.z),
                        wishDir); // Vector projection of Current velocity onto accelDir.

        float accelVel = acceleration * Time.deltaTime; // Accelerated velocity in direction of movement

        // If necessary, truncate the accelerated velocity so the vector projection does not exceed max_velocity
        if (projVel + accelVel > maxSpeed)
            accelVel = Mathf.Max(0f, maxSpeed - projVel);

        rb.AddForce(wishDir.normalized * accelVel, ForceMode.VelocityChange);
    }


    void Wallrun(Vector3 wishDir, float maxSpeed, float climbSpeed, float acceleration)
    {
        if (jump)
        {
            //Vertical
            float upForce = Mathf.Clamp(jumpUpSpeed - rb.velocity.y, 0, Mathf.Infinity);
            rb.AddForce(new (0, upForce, 0), ForceMode.VelocityChange);

            //Horizontal
            Vector3 jumpOffWall = groundNormal.normalized;
            jumpOffWall   *= dashSpeed;
            jumpOffWall.y =  0;
            rb.AddForce(jumpOffWall, ForceMode.VelocityChange);
            wrTimer = 0f;
            EnterFlying(true);
        }
        else if (wrTimer == 0f || crouched)
        {
            rb.AddForce(groundNormal * 3f, ForceMode.VelocityChange);
            EnterFlying(true);
        }
        else
        {
            //Horizontal
            Vector3 distance = VectorToWall();
            wishDir   =  RotateToPlane(wishDir, -distance.normalized);
            wishDir   *= maxSpeed;
            wishDir.y =  Mathf.Clamp(wishDir.y, -climbSpeed, climbSpeed);
            Vector3 wallrunForce                            = wishDir - rb.velocity;
            if (wallrunForce.magnitude > 0.2f) wallrunForce = wallrunForce.normalized * acceleration;

            //Vertical
            if (rb.velocity.y < 0f && wishDir.y > 0f) wallrunForce.y = 2f * acceleration;

            //Anti-gravity force
            Vector3 antiGravityForce = -Physics.gravity;

            if (wrTimer < 0.33 * wallRunTime)
            {
                antiGravityForce *= wrTimer / wallRunTime;
                wallrunForce     += Physics.gravity + antiGravityForce;
            }

            //Forces
            rb.AddForce(wallrunForce, ForceMode.Acceleration);
            rb.AddForce(antiGravityForce, ForceMode.Acceleration);
            if (distance.magnitude > wallStickDistance) distance = Vector3.zero;
            rb.AddForce(distance * wallStickiness, ForceMode.Acceleration);
        }

        if (!IsGrounded)
        {
            wallStickTimer = 0.2f;
            EnterFlying();
        }
    }

    void Jump()
    {
        if (mode == Mode.Walking && canJump)
        {
            IsJumping = true;
            float upForce = Mathf.Clamp(jumpUpSpeed - rb.velocity.y, 0, Mathf.Infinity);
            rb.AddForce(new (0, upForce, 0), ForceMode.VelocityChange);
            playerParticles.ParticlePlayer(PlayerParticles.Type.Jump);
            StartCoroutine(jumpCooldownCoroutine(0.2f));
            EnterFlying(true);
        }
    }

    void DoubleJump(Vector3 wishDir)
    {
        if (canDJump)
        {
            //Vertical
            float upForce = Mathf.Clamp(jumpUpSpeed - rb.velocity.y, 0, Mathf.Infinity);

            rb.AddForce(new (0, upForce, 0), ForceMode.VelocityChange);

            //Horizontal
            if (wishDir != Vector3.zero)
            {
                Vector3 velocity         = rb.velocity;
                var     horizontalSpeed          = new Vector3(velocity.x, 0, velocity.z);
                Vector3 newSpeed          = wishDir.normalized;
                float   newSpeedMagnitude = dashSpeed;

                if (horizontalSpeed.magnitude > dashSpeed)
                {
                    float dot = Vector3.Dot(wishDir.normalized, horizontalSpeed.normalized);

                    if (dot > 0) newSpeedMagnitude = dashSpeed + (horizontalSpeed.magnitude - dashSpeed) * dot;
                    else
                        newSpeedMagnitude = Mathf.Clamp(dashSpeed * (1 + dot),
                                                       dashSpeed * (dashSpeed / horizontalSpeed.magnitude), dashSpeed);
                }

                newSpeed *= newSpeedMagnitude;

                rb.AddForce(newSpeed - horizontalSpeed, ForceMode.VelocityChange);
            }

            canDJump = false;
        }
    }
    #endregion

    #region MathGenious
    Vector2 ClampedAdditionVector(Vector2 a, Vector2 b)
    {
        float k = Mathf.Sqrt(Mathf.Pow(a.x, 2)       + Mathf.Pow(a.y, 2)) /
                  Mathf.Sqrt(Mathf.Pow(a.x + b.x, 2) + Mathf.Pow(a.y + b.y, 2));

        float x = k * (a.x + b.x) - a.x;
        float y = k * (a.y + b.y) - a.y;
        return new (x, y);
    }

    static Vector3 RotateToPlane(Vector3 vect, Vector3 normal)
    {
        Vector3    rotDir   = Vector3.ProjectOnPlane(normal, Vector3.up);
        Quaternion rotation = Quaternion.AngleAxis(-90f, Vector3.up);
        rotDir = rotation * rotDir;
        float angle = -Vector3.Angle(Vector3.up, normal);
        rotation = Quaternion.AngleAxis(angle, rotDir);
        vect     = rotation * vect;
        return vect;
    }

    float WallrunCameraAngle()
    {
        Vector3    rotDir   = Vector3.ProjectOnPlane(groundNormal, Vector3.up);
        Quaternion rotation = Quaternion.AngleAxis(-90f, Vector3.up);
        rotDir = rotation * rotDir;
        float angle = Vector3.SignedAngle(Vector3.up, groundNormal, Quaternion.AngleAxis(90f, rotDir) * groundNormal);
        angle -= 90;
        angle /= 180;
        Vector3 playerDir = transform.forward;
        var     normal    = new Vector3(groundNormal.x, 0, groundNormal.z);

        return Vector3.Cross(playerDir, normal).y * angle;
    }

    bool CanRunOnThisWall(Vector3 normal) => Vector3.Angle(normal, groundNormal) > 10 || wallBan == 0f;

    Vector3 VectorToWall()
    {
        Vector3    position = transform.position + Vector3.up * col.height / 2f;

        if (Physics.Raycast(position, -groundNormal, out RaycastHit hit, wallStickDistance) &&
            Vector3.Angle(groundNormal, hit.normal) < 70)
        {
            groundNormal = hit.normal;
            Vector3 direction = hit.point - position;
            return direction;
        }

        return Vector3.positiveInfinity;
    }

    Vector3 VectorToGround()
    {
        Vector3    position = transform.position;
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, wallStickDistance)) return hit.point - position;
        return Vector3.positiveInfinity;
    }
    #endregion

    #region Coroutines
    IEnumerator jumpCooldownCoroutine(float time)
    {
        canJump = false;
        yield return new WaitForSeconds(time);
        canJump = true;
    }
    #endregion
}