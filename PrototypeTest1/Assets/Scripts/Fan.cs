using UnityEngine;
using System.Collections.Generic;

public class Fan : MonoBehaviour
{
    [SerializeField] private float continuousForce = 1000f; // Force applied while riding inside the fan
    [SerializeField] private float entryBoost = 30f; // Instant velocity burst when touching ANY part of the fan
    [SerializeField] private float radius = 1f;
    [SerializeField] private float length = 5f;
    [SerializeField] private LayerMask affectedLayers;
    [SerializeField] private Transform fanCenter;

    // Tracks objects currently in the fan to detect exact entry moments
    private HashSet<Collider> objectsInFan = new HashSet<Collider>();

    void FixedUpdate()
    {
        if (fanCenter == null) return;

        Vector3 bottomPoint = fanCenter.position;
        Vector3 topPoint = fanCenter.position + (fanCenter.up * length);

        Collider[] colliders = Physics.OverlapCapsule(bottomPoint, topPoint, radius, affectedLayers);
        HashSet<Collider> currentColliders = new HashSet<Collider>(colliders);

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;

            if (rb != null)
            {
                // If this is the very first frame the player touched the fan
                if (!objectsInFan.Contains(col))
                {
                    Vector3 currentVel = rb.linearVelocity;

                    // Override falling momentum and apply the big burst instantly
                    if (currentVel.y < entryBoost)
                    {
                        currentVel.y = entryBoost;
                        rb.linearVelocity = currentVel;
                    }
                }
                else
                {
                    // Add continuous draft force for every subsequent frame they remain inside
                    rb.AddForce(fanCenter.up * continuousForce, ForceMode.Force);
                }
            }
        }

        // Store the colliders for the next frame's comparison
        objectsInFan = currentColliders;
    }

    // Draws visuals in the Scene view when the object is selected
    private void OnDrawGizmosSelected()
    {
        if (fanCenter == null) return;

        Gizmos.color = Color.cyan;

        Vector3 bottom = fanCenter.position;
        Vector3 top = bottom + (fanCenter.up * length);

        // Draw the top and bottom spheres
        Gizmos.DrawWireSphere(bottom, radius);
        Gizmos.DrawWireSphere(top, radius);

        // Draw connecting lines
        Vector3 rightOffset = fanCenter.right * radius;
        Vector3 forwardOffset = fanCenter.forward * radius;

        Gizmos.DrawLine(bottom + rightOffset, top + rightOffset);
        Gizmos.DrawLine(bottom - rightOffset, top - rightOffset);
        Gizmos.DrawLine(bottom + forwardOffset, top + forwardOffset);
        Gizmos.DrawLine(bottom - forwardOffset, top - forwardOffset);
    }
}