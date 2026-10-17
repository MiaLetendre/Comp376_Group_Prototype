using UnityEngine;

public class Fan : MonoBehaviour
{
    [SerializeField] private float force = 1000f; // The force applied by the fan
    [SerializeField] private float radius = 1f; // The radius of the fan's effect
    [SerializeField] private float length = 5f; // The length of the fan's effect
    [SerializeField] private LayerMask affectedLayers; // The layers that the fan affects
    [SerializeField] private Transform fanCenter; // The center point of the fan's effect

    void FixedUpdate()
    {
        if (fanCenter == null) return;

        Vector3 bottomPoint = fanCenter.position;
        Vector3 topPoint = fanCenter.position + (fanCenter.up * length);

        Collider[] colliders = Physics.OverlapCapsule(bottomPoint, topPoint, radius, affectedLayers);

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;

            if (rb != null)
            {
                rb.AddForce(fanCenter.up * force, ForceMode.Force);
            }
        }
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