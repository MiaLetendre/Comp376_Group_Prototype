using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Swing : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Swing Settings")]
    public float swingSpeed;
    public KeyCode swingKey = KeyCode.Mouse0; // Default to left mouse button
    public float maxSwingDistance = 40f; // Maximum distance for the swing
    private Vector3 swingPoint; // Point where the swing is aimed
    private SpringJoint springJoint; // Spring joint for the swing
    public PlayerMovement movementScript; // Reference to the PlayerMovement script
    public float continuousForwardForce = 10f; // Initial forward force when starting the swing
    public float groundedUpwardForce = 5f; // Upward force applied when starting the swing

    [Header("Prediction")]
    public RaycastHit predictionHit; // Raycast hit information for the swing
    public float predictionSphereRadius; // Radius for the prediction spherecast
    public Transform predictionPoint; // Point from which the prediction raycast is cast

    [Header("Refrences")]
    public LineRenderer lineRenderer; // Line renderer for the swing rope
    public Transform gunTip, cam, player; // Tip of the gun where the swing rope starts
    public LayerMask whatIsGrappleable; // Layer mask for grappleable objects

    [Header("Thrust Settings")]
    public float horizontalThrustForce; 
    public float verticalThrustForce; 
    public Rigidbody rb; // Reference to the player's Rigidbody
    public float extendCableSpeed = 10f; // Speed at which the cable extends
    public Transform orientation; // Reference to the player's orientation for thrust direction

    [Header("Timers")]
    public float maxSwingTime = 3f; //only allowed to swing for x seconds
    public float elapsedTime = 0f;
    public float swingCooldown = 0.5f; // Cooldown time between swings
    public float swingTimer;
    private float cooldownTimer;
    public bool isCoolingDown = false;

    [Header("UI")]
    public Slider barSlider; // Prefab for the bar
    public Image barImage; // Image component of the bar
    public Color normalColor = Color.green; // Normal color of the bar
    public Color cooldownColor = Color.red; // Color of the bar during cooldown

 
    
    void Start()
    {
        if (barImage != null)
        {
            barImage.color = normalColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleCooldownAndBarUI();
        if (Input.GetKeyDown(swingKey) && !isCoolingDown && swingTimer > 0f)
        {
            startSwing();
        }
        else if (Input.GetKeyUp(swingKey) && movementScript.swinging)
        {
            stopSwing();
        }

        
        if (movementScript.swinging)
        {
            swingTimer -= Time.deltaTime;
            if (swingTimer <= 0f)
            {
                swingTimer = 0f;
                stopSwing(); 
            }
        }
        
       

        checkSwingPoint();

    }

    void FixedUpdate()
    {
        if (springJoint != null)
        {
            // Apply continuous forward force while swinging
            swingGearMovement();
        }
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void startSwing()
    {
        
        if (springJoint != null)
        {
            
            return;
        }
        if (predictionHit.point == Vector3.zero)
        {
            //nothing found to grapple
            return;
        }
        
        elapsedTime = 0;

        swingTimer = maxSwingTime;


        movementScript.activeGrapple = true;
        movementScript.swinging = true;



        swingPoint = predictionHit.point;
        springJoint = player.gameObject.AddComponent<SpringJoint>();
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        // The distance grapple will try to keep from grapple point. 
        springJoint.maxDistance = distanceFromPoint * 0.7f;
        springJoint.minDistance = distanceFromPoint * 0.1f;

        // need to adjust these 
        springJoint.spring = 12f;
        springJoint.damper = 7f;
        springJoint.massScale = 4.5f;

        lineRenderer.positionCount = 2;

        if (predictionPoint != null)
        {
            predictionPoint.gameObject.SetActive(false);
        }

        bool isGrounded = movementScript.isGroundedRaw; // Access the isGroundedRaw variable from PlayerMovement

        if (isGrounded)
        {
            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }   
            rb.AddForce(Vector3.up * groundedUpwardForce, ForceMode.Impulse);
        }
        
        
        
        
       
    }

    void stopSwing()
    {
        if (movementScript == null || !movementScript.activeGrapple || !movementScript.swinging)
        {
            return;
        }
        elapsedTime = 0f;
        movementScript.activeGrapple = false;
        movementScript.swinging = false;
        lineRenderer.positionCount = 0;
        foreach (SpringJoint joint in player.GetComponents<SpringJoint>())
        {
            Destroy(joint);
        }
        
        springJoint = null;

        isCoolingDown = true;
        cooldownTimer = 0f;
        if (barImage != null)
        {
            barImage.color = cooldownColor;
        }
        /* if animation is used later
        // Stop the swing animation
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.ResetTrigger("Swing");
        }
        */
    }

    void DrawRope()
    {
        // draw the rope only if the spring joint exists
        if (!springJoint) return;

        lineRenderer.SetPosition(0, gunTip.position);
        lineRenderer.SetPosition(1, swingPoint);
    }

    public void swingGearMovement()
    {

        Vector3 directionToSwingPoint = (swingPoint - transform.position).normalized;
        rb.AddForce(directionToSwingPoint * continuousForwardForce * Time.fixedDeltaTime, ForceMode.Force);
        //thrust
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(orientation.right * horizontalThrustForce * Time.fixedDeltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(-orientation.right * horizontalThrustForce * Time.fixedDeltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 thrustDirection;
            bool isGrounded = movementScript.isGroundedRaw;
            if (isGrounded)
            {
                thrustDirection = (orientation.forward + Vector3.up * .3f).normalized;
            }
            else
            {
                thrustDirection = orientation.forward;
            }
            
            rb.AddForce(thrustDirection * verticalThrustForce * Time.deltaTime);
            
        }
      

        //shorten cable
        if (Input.GetKey(KeyCode.LeftControl))
        {
            rb.AddForce(directionToSwingPoint * verticalThrustForce * Time.fixedDeltaTime);

            float distanceToSwingPoint = Vector3.Distance(transform.position, swingPoint);

            springJoint.maxDistance = distanceToSwingPoint * 0.7f;
            springJoint.minDistance = distanceToSwingPoint * 0.1f;
        }

        //extend cable
        if (Input.GetKey(KeyCode.LeftShift))
        {
            float distanceToSwingPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed * Time.fixedDeltaTime;

            springJoint.maxDistance = distanceToSwingPoint * 0.7f;
            springJoint.minDistance = distanceToSwingPoint * 0.1f;
        }

    }

    private void checkSwingPoint()
    {
        if (springJoint != null)
        {
            // If the spring joint exists, we don't need to check for a new swing point
            return;
        }

        RaycastHit sphereCastHit;
        //might need to cahnge  both thtese to cam.pos but will try this
        Physics.SphereCast(cam.position, predictionSphereRadius, cam.forward, out sphereCastHit, maxSwingDistance, whatIsGrappleable);
        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 actualHitPoint = Vector3.zero;

        if (raycastHit.point != Vector3.zero)
        {
            actualHitPoint = raycastHit.point;
        }
        else if (sphereCastHit.point != Vector3.zero)
        {
            actualHitPoint = sphereCastHit.point;
        }

        if (actualHitPoint != Vector3.zero)
        {
            if (predictionPoint != null)
            {
                predictionPoint.gameObject.SetActive(true);
                predictionPoint.position = actualHitPoint;
            }
        }
        else
        {   
            if (predictionPoint != null){
                predictionPoint.gameObject.SetActive(false);
            }
        }

        predictionHit = raycastHit.point != Vector3.zero ? raycastHit : sphereCastHit;
    }

    void HandleCooldownAndBarUI()
    {
        if (isCoolingDown)
        {
            cooldownTimer += Time.deltaTime;
            swingTimer = Mathf.Lerp(0f, maxSwingTime, cooldownTimer / swingCooldown);
            if (cooldownTimer >= swingCooldown)
            {
                isCoolingDown = false;
                swingTimer = maxSwingTime;
                if (barImage != null)
                {
                    barImage.color = normalColor;
                }
            }
        }

        if (barSlider != null)
        {
            barSlider.value = swingTimer / maxSwingTime;
        }

    }
}