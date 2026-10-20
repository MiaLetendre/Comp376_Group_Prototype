using UnityEngine;
using Unity.Cinemachine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Refrences")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;


    public float rotationSpeed;

    public Transform combatLookAt;
    public CameraStyle currentStyle;
    public GameObject thirdPersonCam;
    public GameObject combatCam;

    [Header("Camera")]
    public CinemachineCamera thirdPersonCamera;
    public CinemachineCamera CombatCamera;

    [Header("Is Combat Active")]
    public bool isCombatActive;

    private float horizontalInput;
    private float verticalInput;


    public enum CameraStyle
    {
        Basic,
        Combat

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (thirdPersonCam != null){
            thirdPersonCam.SetActive(true);
        }
        if (combatCam != null && isCombatActive){ 
            combatCam.SetActive(true);
        }

        switchCameraStyle(CameraStyle.Basic);

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.GetComponent<CameraCollision>() == null)
        {
            mainCamera.gameObject.AddComponent<CameraCollision>();
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        //rotate orientation
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            switchCameraStyle(CameraStyle.Basic);
        } else if (Input.GetKeyDown(KeyCode.Alpha2) && isCombatActive){
            switchCameraStyle(CameraStyle.Combat);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        

        //orientation of the camera, related to the player
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0;
        if (camForward.sqrMagnitude > 0.001f){
            orientation.forward = camForward.normalized;
        }
        

        //Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        //viewDir.y = 0;

        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir.sqrMagnitude > 0.001f){
            Quaternion targetRotation = Quaternion.LookRotation(inputDir.normalized, Vector3.up);
            playerObj.rotation = Quaternion.Slerp(playerObj.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        /*
        if (viewDir != Vector3.zero){
            orientation.forward = viewDir.normalized;
            //orientation.forward = player.forward;
        } else {
            //orientation.forward = viewDir.normalized;
            orientation.forward = player.forward;
        }
        

        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero){
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.fixedDeltaTime * rotationSpeed);
        }
        */
        
        
    }

    public void switchCameraStyle(CameraStyle newStyle){
        if (newStyle == CameraStyle.Basic){
            thirdPersonCamera.Priority = 10;
            if (isCombatActive){
                CombatCamera.Priority = 5;
            }

        } else if (newStyle == CameraStyle.Combat && isCombatActive){
            thirdPersonCamera.Priority = 5;
            CombatCamera.Priority = 10;
        }
        currentStyle = newStyle;
    }
}
