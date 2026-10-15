using UnityEngine;

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
    
    [Header("Jumping")]
    public LayerMask whatIsGround;
    bool grounded;
    public float playerHeight;
    
     // Reference to the PlayerMovementGrapple script
    public Swing grappleScript;

    [Header("Sprinting")]    
    public float sprintSpeed;

    [Header("Air Movement")]


    [Header("Grappling")]
    public float swingSpeed;
    public bool activeGrapple;
    public bool swinging;
    

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
     bool readyToJump = true;

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
        
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.1f, whatIsGround);
        
        if (grounded && readyToJump)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = stopSpeedDamping;
        }
        MoveInput();
        SpeedControl();
        MoveInput();

        if (jumping )
        {
            
            if (lastY < rb.linearVelocity.y)
            {
                rb.mass  = rb.mass* 2;
            }

        }
            
        
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    //inputs for movements
     private void MoveInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        jumping = Input.GetKey(jumpKey);
        lastY = rb.linearVelocity.y;

    
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
            if (moveDirection.magnitude > .01)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            } else{//breaking when no input is given
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Vector3 targetVel =Vector3.Lerp(flatVel, Vector3.zero, Time.fixedDeltaTime * stopSpeedDamping);
                rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);

            }
            
        } else 
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
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

        if (Mathf.Abs(desiredMoveSpeed- moveSpeed) >4f)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, desiredMoveSpeed, Time.deltaTime * 3f);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, desiredMoveSpeed, Time.deltaTime * 10f);        
        }
    }



    
}
