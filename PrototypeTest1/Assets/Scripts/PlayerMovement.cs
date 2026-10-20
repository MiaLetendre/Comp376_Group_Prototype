using UnityEngine;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed = 8f;
    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    public float walkSpeed = 8f;
    public float groundDrag = 2f;
    public float stopSpeedDamping = 40f;

    [Header("Camera")]
    public CinemachineCamera gameCamera;

    [Header("Jumping")]
    public LayerMask whatIsGround;
    bool grounded;
    public float playerHeight;

    // Reference to the PlayerMovementGrapple script
    public Swing grappleScript;

    [Header("Sprinting")]
    public float sprintSpeed = 14f;

    [Header("Steps")]
    public GameObject stepRayLower;
    public GameObject stepRayUpper;
    public float stepHeight = 0.35f;
    public float stepSmooth = 12f;

    [Header("Air Movement")]


    [Header("Grappling")]
    public float swingSpeed;
    public bool activeGrapple;
    public bool swinging;
    private Vector3 startingPosition;


    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float fallMultiplier = 2.5f;
    bool readyToJump = true;
    public float groundCheckDistance = 0.3f;
    public bool isGroundedRaw;
    private float groundedTimer;

    [Header("Keys")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("States")]
    public float desiredMoveSpeed;
    public float lastDesiredMoveSpeed;
    public bool jumping;
    public float lastY;


    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air,
        swinging
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //player falls over if we don't freeze rotation, so we freeze it
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        startingPosition = rb.position;


        readyToJump = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReturnToStartingPosition();
        }

        //sphere casting instead of ray casting because it is more reliable for ground detection, especially on uneven terrain
        Vector3 sphereCastOrigin = transform.position + Vector3.up * 0.25f;
        isGroundedRaw = Physics.SphereCast(sphereCastOrigin, 0.2f, Vector3.down, out RaycastHit hit, 0.35f, whatIsGround);

        if (isGroundedRaw)
        {
            groundedTimer = 0.05f;
        }
        else
        {
            groundedTimer -= Time.deltaTime;
        }
        grounded = groundedTimer > 0f;

        if (activeGrapple || swinging || !grounded)
        {
            rb.linearDamping = 0f;
        }
        else
        {
            rb.linearDamping = groundDrag;
        }

        float cleanYVelocity = Mathf.Abs(rb.linearVelocity.y) < 0.01f ? 0f : rb.linearVelocity.y;
        lastY = cleanYVelocity;

        MoveInput();
        SpeedControl();






    }

    private void ReturnToStartingPosition()
    {
        if (grappleScript != null)
        {
            grappleScript.StopSwing();
        }

        rb.position = startingPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        MovePlayer();
        SnappyGravity();
        goUpSteps();
    }

    //inputs for movements
    private void MoveInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        jumping = Input.GetKey(jumpKey);



        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded && !activeGrapple && !swinging)
        {
            readyToJump = false;

            goAheadAndJump();

            Invoke(nameof(resetJump), jumpCooldown);
        }

    }

    //allows player to move
    private void MovePlayer()
    {
        stateHandler();
        if (activeGrapple || swinging)
        {
            return;
        }

        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            if (moveDirection.magnitude > .01f)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            }
            else
            {
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                flatVel = Vector3.MoveTowards(flatVel, Vector3.zero, stopSpeedDamping * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector3(flatVel.x, rb.linearVelocity.y, flatVel.z);
            }

        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }



    //makes the player fall faster than the default gravity, giving a snappier feel to the jump
    void SnappyGravity()
    {
        if (activeGrapple || swinging)
        {
            return;
        }

        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Vector3.down * (fallMultiplier - 1) * Physics.gravity.magnitude * .5F, ForceMode.Acceleration);
        }
    }

    void goUpSteps()
    {
        if (activeGrapple || swinging)
        {
            return;
        }

        Vector3 movementDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (movementDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        movementDirection.Normalize();
        int stepMask = ~LayerMask.GetMask("Player");
        float checkDistance = 0.45f;
        Vector3 lowerOrigin = transform.position + Vector3.up * 0.08f;
        Vector3 upperOrigin = transform.position + Vector3.up * (stepHeight + 0.08f);
        Debug.DrawRay(lowerOrigin, movementDirection * checkDistance, Color.red);
        Debug.DrawRay(upperOrigin, movementDirection * checkDistance, Color.blue);

        bool lowerHit = Physics.Raycast(lowerOrigin, movementDirection, checkDistance, stepMask, QueryTriggerInteraction.Ignore);
        bool upperBlocked = Physics.Raycast(upperOrigin, movementDirection, checkDistance, stepMask, QueryTriggerInteraction.Ignore);

        if (lowerHit && !upperBlocked)
        {
            Vector3 stepUpPosition = rb.position + Vector3.up * (stepHeight * Time.fixedDeltaTime * stepSmooth);
            rb.MovePosition(stepUpPosition);

            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
        }

    }

    //Controlls the player speed
    void SpeedControl()
    {
        //limit velocity on the x-z plane
        if (activeGrapple || swinging)
        {
            return;
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            Vector3 smoothedVel = Vector3.Lerp(flatVel, limitedVel, Time.fixedDeltaTime * 10f);
            rb.linearVelocity = new Vector3(smoothedVel.x, rb.linearVelocity.y, smoothedVel.z);
        }

    }

    //jump function
    void goAheadAndJump()
    {
        //reset y velocity
        rb.linearDamping = 0f;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);


    }

    //resets the jump
    void resetJump()
    {
        readyToJump = true;
    }

    //find the currect state of the player
    void stateHandler()
    {
        if (swinging)
        {
            state = MovementState.swinging;
            desiredMoveSpeed = swingSpeed; // Set moveSpeed to swingSpeed when swinging
        }
        else if (grounded && (Input.GetKey(sprintKey) || Input.GetKey(KeyCode.RightShift)))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed; // Set moveSpeed to sprint speed
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed; // Set moveSpeed to walk speed

        }
        else
        {
            state = MovementState.air;

            float currentSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            desiredMoveSpeed = Mathf.Max(currentSpeed, walkSpeed); // Set moveSpeed to current speed or walk speed, whichever is greater
                                                                   //movement speed remains


        }

        /*
        if (!grounded)
        {
            var orbitalFollow = gameCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineOrbitalFollow;
            orbitalFollow.Orbits.Bottom.Height = -2f;
        }
        else
        {
            var orbitalFollow = gameCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineOrbitalFollow;
            orbitalFollow.Orbits.Bottom.Height = 0f;
        }
        */

        moveSpeed = desiredMoveSpeed;
    }




}
