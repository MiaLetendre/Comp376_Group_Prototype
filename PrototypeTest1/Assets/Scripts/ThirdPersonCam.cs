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
    }

    void Update()
    {
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
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        //rotate the player
        if (currentStyle == CameraStyle.Basic){
            float horizontalInput = -Input.GetAxis("Horizontal");
            float verticalInput = -Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero){
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.fixedDeltaTime * rotationSpeed);
            }
        } else if (currentStyle == CameraStyle.Combat && isCombatActive){
            float horizontalInput = -Input.GetAxis("Horizontal");
            float verticalInput = -Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero){
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.fixedDeltaTime * rotationSpeed);
            }
        }
        
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
