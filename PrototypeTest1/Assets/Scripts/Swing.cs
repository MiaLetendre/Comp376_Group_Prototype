using TMPro;
using Unity.Cinemachine;
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
    Camera aimCamera;

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
    TMP_Text swingTimeLabel;

 
    
    void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    void Start()
    {
        if (cam != null)
        {
            aimCamera = cam.GetComponent<Camera>();
        }

        if (swingTimer <= 0f)
        {
            swingTimer = maxSwingTime;
        }

        if (barSlider != null)
        {
            barSlider.interactable = false;
        }

        BindSwingUi();
        RefreshSwingBar();
    }

    void OnCameraUpdated(CinemachineBrain brain)
    {
        if (aimCamera != null && brain.OutputCamera != aimCamera)
        {
            return;
        }

        checkSwingPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (isCoolingDown)
        {
            cooldownTimer += Time.deltaTime;
            swingTimer = Mathf.Lerp(0f, maxSwingTime, cooldownTimer / swingCooldown);
            if (cooldownTimer >= swingCooldown)
            {
                isCoolingDown = false;
                swingTimer = maxSwingTime;
            }
        }

        if (Input.GetKeyDown(swingKey) && !isCoolingDown && swingTimer > 0f)
        {
            startSwing();
        }
        else if (Input.GetKeyUp(swingKey) && movementScript != null && movementScript.swinging)
        {
            StopSwing();
        }

        if (movementScript != null && movementScript.swinging)
        {
            swingTimer -= Time.deltaTime;
            if (swingTimer <= 0f)
            {
                swingTimer = 0f;
                StopSwing();
            }
        }

        RefreshSwingBar();
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

    public void StopSwing()
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

        if (!TryGetAimRay(out Ray aimRay))
        {
            return;
        }

        bool rayHit = Physics.Raycast(aimRay, out RaycastHit raycastHit, maxSwingDistance, whatIsGrappleable);
        bool sphereHit = false;
        RaycastHit sphereCastHit = default;
        if (!rayHit && predictionSphereRadius > 0f)
        {
            sphereHit = Physics.SphereCast(aimRay, predictionSphereRadius, out sphereCastHit, maxSwingDistance, whatIsGrappleable);
        }

        bool hasHit = rayHit || sphereHit;
        RaycastHit chosenHit = rayHit ? raycastHit : sphereCastHit;

        if (hasHit)
        {
            if (predictionPoint != null)
            {
                predictionPoint.gameObject.SetActive(true);
                predictionPoint.position = chosenHit.point;
            }
        }
        else if (predictionPoint != null)
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = hasHit ? chosenHit : default;
    }

    bool TryGetAimRay(out Ray aimRay)
    {
        if (aimCamera == null && cam != null)
        {
            aimCamera = cam.GetComponent<Camera>();
        }

        if (aimCamera != null)
        {
            aimRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            return true;
        }

        if (cam != null)
        {
            aimRay = new Ray(cam.position, cam.forward);
            return true;
        }

        aimRay = default;
        return false;
    }

    void BindSwingUi()
    {
        if (barSlider == null)
        {
            Slider[] sliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
            for (int i = 0; i < sliders.Length; i++)
            {
                if (sliders[i].name == "Slider")
                {
                    barSlider = sliders[i];
                    break;
                }
            }
        }

        if (barSlider != null)
        {
            barSlider.interactable = false;
            if ((barImage == null || barImage.rectTransform == barSlider.transform as RectTransform) && barSlider.fillRect != null)
            {
                barImage = barSlider.fillRect.GetComponent<Image>();
            }
        }

        if (swingTimeLabel == null)
        {
            TMP_Text[] labels = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].text.StartsWith("Swing Time"))
                {
                    swingTimeLabel = labels[i];
                    break;
                }
            }
        }
    }

    void RefreshSwingBar()
    {
        if (barSlider == null || barImage == null || swingTimeLabel == null)
        {
            BindSwingUi();
        }

        float amount = maxSwingTime <= 0.01f ? 0f : Mathf.Clamp01(swingTimer / maxSwingTime);

        if (barSlider != null)
        {
            barSlider.SetValueWithoutNotify(amount);
        }

        if (barImage != null)
        {
            barImage.type = Image.Type.Simple;
            barImage.color = isCoolingDown ? cooldownColor : normalColor;
            RectTransform fill = barImage.rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(amount, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        if (swingTimeLabel != null)
        {
            swingTimeLabel.text = "Swing Time  " + Mathf.CeilToInt(Mathf.Max(swingTimer, 0f));
        }
    }
}