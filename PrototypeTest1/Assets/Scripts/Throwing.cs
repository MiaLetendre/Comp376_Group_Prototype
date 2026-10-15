using UnityEngine;
using System.Collections.Generic;

public class Throwing : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created]

    [Header("Refrences")]

    public Transform cam;
    public Transform attackPoint;
    public GameObject projectile;
    public Rigidbody playerRb;
    public LineRenderer lineRenderer;

    [Header("Settings")]
    public int maxProjectiles;
    public int totalProjectiles;
    
    public float throwCooldown;

    [Header("Throwing")]
    public KeyCode throwKey = KeyCode.Mouse2;
    public float throwForce;
    public float throwUpwardForce;
    public float throwSpeed;
    public float arcSpeedMultiplier = 1.5f; // Multiplier for the extra gravity based on throw speed

    [Header("Prediction")]
    public Transform predictionPoint;
    public float timeStep = 0.1f; //time between predictions
    public int maxSteps = 30; //max number of predictions
    public LayerMask hitLayers;


    bool readyToThrow;
    private List<Vector3> trajectoryPoints = new List<Vector3>();

    void Start()
    {
        readyToThrow = true;
        totalProjectiles = maxProjectiles;
        if (predictionPoint != null)
        {
            predictionPoint = Instantiate(predictionPoint, attackPoint.position, Quaternion.identity);
            predictionPoint.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(throwKey) && readyToThrow && totalProjectiles > 0)
        {
            Throw();
        }
        PredictPoint();
    }

    private  Vector3 CalculateInitialVelocity()
    {
        Vector3 aimDirection = GetAimDirection();
        Vector3 initialVelocity = (aimDirection * throwSpeed) + (Vector3.up * throwUpwardForce);
        return initialVelocity;
    }

    void Throw()
    {
        readyToThrow = false;

        totalProjectiles--;

        GameObject projectileObject = Instantiate(projectile, attackPoint.position, cam.rotation);
        projectileObject.SetActive(true);
        Rigidbody projectileRb = projectileObject.GetComponent<Rigidbody>();

        projectileRb.linearVelocity = CalculateInitialVelocity();

        /*
        Vector3 initialVelocity = CalculateInitialVelocity();
        projectileRb.AddForce(initialVelocity, ForceMode.VelocityChange);


        Vector3 forceDirection = GetAimDirection();
        Vector3 force = forceDirection * throwForce + cam.transform.up * throwUpwardForce * throwSpeed;
        projectileRb.AddForce(force, ForceMode.Impulse);
        */

        
        Invoke(nameof(ResetThrow), throwCooldown);

        
        
    }

    private void PredictPoint()
    {
        if (predictionPoint != null)
        {

            //get original aim direction
            trajectoryPoints.Clear();
            Vector3 initialVelocity = CalculateInitialVelocity();

            //get variables
            Vector3 startPosition = attackPoint.position;
            Vector3 gravity = Physics.gravity;
            bool foundGround = false;
            RaycastHit hit = default;
            Vector3 currentPosition = startPosition;

            //trace the curve
            for (int i = 0; i < maxSteps*2; i+=2)
            {
                //for prediction of where it will go
                float time = i * timeStep;
                // p(t) = p0 + v0 * t + 1/2 * g * t^2
                Vector3 newPosition = startPosition + (initialVelocity * time) + (0.5f * gravity * time * time);
                Debug.DrawLine(currentPosition, newPosition, Color.red);
                
                if (Physics.Linecast(currentPosition, newPosition, out hit, hitLayers))
                {
                    
                    foundGround = true;
                    trajectoryPoints.Add(hit.point);
                    break;
                }
                else
                {
                    trajectoryPoints.Add(newPosition);
                    predictionPoint.position = newPosition;
                    currentPosition = newPosition;
                }
            }
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = trajectoryPoints.Count;
                lineRenderer.SetPositions(trajectoryPoints.ToArray());
            }
            if (foundGround)
                {
                    predictionPoint.gameObject.SetActive(true);
                    predictionPoint.transform.position = hit.point + hit.normal * 0.1f;
                }else
                {
                    predictionPoint.gameObject.SetActive(false);
                }
        }
        Invoke(nameof(ResetThrow), throwCooldown);
    }

    public Vector3 GetAimDirection()
    {
        Vector3 forceDirection = cam.transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f, hitLayers))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }
        return forceDirection;
    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }


    public void teleportToPosition(Vector3 position)
    {
        playerRb.position = position;
        playerRb.linearVelocity = Vector3.zero; 
    }

    

}
