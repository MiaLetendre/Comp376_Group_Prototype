using UnityEngine;

public class Swing : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Swing Settings")]
    public float swingSpeed;
    public KeyCode swingKey = KeyCode.Mouse0; // Default to left mouse button
    private float maxSwingDistance = 75f; // Maximum distance for the swing
    private Vector3 swingPoint; // Point where the swing is aimed
    private SpringJoint springJoint; // Spring joint for the swing
    public PlayerMovement movementScript; // Reference to the PlayerMovement script

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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(swingKey))
        {
            startSwing();
        }
        else if (Input.GetKeyUp(swingKey))
        {
            stopSwing();
        }

        checkSwingPoint();

        if (springJoint != null)
        {
            // Extend the cable length over time
            swingGearMovement();
        }
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void startSwing()
    {
        if (predictionHit.point == Vector3.zero)
        {
            //nothing found to grapple
            return;
        }

        


        movementScript.activeGrapple = true;
        movementScript.swinging = true;
        /*
        RaycastHit hit;
        Ray screenRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        //cahnged to cam from screenray
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable))
        {
        */
    

            movementScript.activeGrapple = true;
            movementScript.swinging = true;

            swingPoint = predictionHit.point;
            springJoint = player.gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // The distance grapple will try to keep from grapple point. 
            springJoint.maxDistance = distanceFromPoint * 0.8f;
            springJoint.minDistance = distanceFromPoint * 0.25f;

            // need to adjust these 
            springJoint.spring = 4.5f;
            springJoint.damper = 7f;
            springJoint.massScale = 4.5f;

            lineRenderer.positionCount = 2;

            if (predictionPoint != null)
            {
                predictionPoint.gameObject.SetActive(false);
            }
        
        
        /* if animation is used later
        // Start the swing animation
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Swing");
        }
        */
    }

    void stopSwing()
    {
        movementScript.activeGrapple = false;
        movementScript.swinging = false;
        lineRenderer.positionCount = 0;
        if (springJoint != null)
        {
            Destroy(springJoint);
        }
        else
        {
            Debug.Log("No spring joint to destroy.");
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
        //thrust
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(orientation.forward * verticalThrustForce * Time.deltaTime);
        }
      

        //shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToSwingPoint = (swingPoint - transform.position).normalized;
            rb.AddForce(directionToSwingPoint * verticalThrustForce * Time.deltaTime);

            float distanceToSwingPoint = Vector3.Distance(transform.position, swingPoint);

            springJoint.maxDistance = distanceToSwingPoint * 0.8f;
            springJoint.minDistance = distanceToSwingPoint * 0.25f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            float distanceToSwingPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed * Time.deltaTime;

            springJoint.maxDistance = distanceToSwingPoint * 0.8f;
            springJoint.minDistance = distanceToSwingPoint * 0.25f;
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
}