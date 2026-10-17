using UnityEngine;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed;
    public Transform orientation;
     float horizontalInput;
     float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    public float walkSpeed;
    public float groundDrag;
    public float stopSpeedDamping = 0f;

    [Header("Camera")]
    public CinemachineCamera gameCamera;

    [Header("Jumping")]
    public LayerMask whatIsGround;
    bool grounded;
    public float playerHeight;
    
     // Reference to the PlayerMovementGrapple script
    public Swing grappleScript;

    [Header("Sprinting")]    
    public float sprintSpeed;

    [Header("Steps")]
    public GameObject stepRayLower;
    public GameObject stepRayUpper;
    public float stepHeight = 0.5f;
    public float stepSmooth = 2f;

    [Header("Air Movement")]


    [Header("Grappling")]
    public float swingSpeed;
    public bool activeGrapple;
    public bool swinging;
    

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


        readyToJump = true;
    }

   
    // Update is called once per frame
    void Update()
    {
        //sphere casting instead of ray casting because it is more reliable for ground detection, especially on uneven terrain
        Vector3 sphereCastOrigin = transform.position + Vector3.up * (groundCheckDistance + 0.1f); 
        float castDistance = playerHeight * .5f + groundCheckDistance;
        isGroundedRaw = Physics.SphereCast(sphereCastOrigin, groundCheckDistance, Vector3.down, out RaycastHit hit, castDistance, whatIsGround);

        if (isGroundedRaw)
        {
            groundedTimer = 0.05f;
        }
        else
        {
            groundedTimer -= Time.deltaTime;
        }
        grounded = groundedTimer > 0f;
        
        if (grounded && readyToJump)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = stopSpeedDamping;
        }

        float cleanYVelocity = Mathf.Abs(rb.linearVelocity.y) < 0.01f ? 0f : rb.linearVelocity.y;
        lastY = cleanYVelocity;

        MoveInput();
        SpeedControl();
  
        

        
            
        
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



        if (Input.GetKey(jumpKey) && readyToJump && grounded)
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
            } else{
                //breaking when no input is given
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Vector3 targetVel =Vector3.Lerp(flatVel, Vector3.zero, Time.fixedDeltaTime * stopSpeedDamping);
                rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);

            }
            
        } else 
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

        RaycastHit hitLower;
        RaycastHit hitUpper;
        Debug.DrawRay(stepRayLower.transform.position, movementDirection * 0.5f, Color.red);
        Debug.DrawRay(stepRayUpper.transform.position, movementDirection * 0.5f, Color.blue);
        
        if (Physics.Raycast(stepRayLower.transform.position, movementDirection, out hitLower, 0.5f))
        {
            if (!Physics.Raycast(stepRayUpper.transform.position, -movementDirection, out hitUpper, 0.5f))
            {
                Vector3 stepUpPosition = rb.position + Vector3.up * (stepHeight *Time.fixedDeltaTime * stepSmooth);
                rb.MovePosition(stepUpPosition);

                if(rb.linearVelocity.y < 0f)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                }
            }
        }

    }

    //Controlls the player speed
    void SpeedControl(){
        //limit velocity on the x-z plane
        if (activeGrapple || swinging)
        {
            return;
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if(flatVel.magnitude > moveSpeed){
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            Vector3 smoothedVel = Vector3.Lerp(flatVel, limitedVel, Time.fixedDeltaTime * 10f);
            rb.linearVelocity = new Vector3(smoothedVel.x, rb.linearVelocity.y, smoothedVel.z);
        }

    }

    //jump function
    void goAheadAndJump(){
        //reset y velocity
        rb.linearDamping = 0f;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);


    }

    //resets the jump
    void resetJump(){
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
        else if (grounded && Input.GetKey(KeyCode.LeftShift))
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

        //slowly lerp to the desired move speed, if the difference is too big, lerp slower
        if (Mathf.Abs(desiredMoveSpeed- moveSpeed) >4f)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, desiredMoveSpeed, Time.deltaTime * 1f);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, desiredMoveSpeed, Time.deltaTime * 2f);        
        }
    }



    
}
