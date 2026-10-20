using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class CameraCollision : MonoBehaviour
{
    public float cameraRadius = 0.2f;
    public float minimumDistance = 0.45f;
    public LayerMask collideAgainst;

    CinemachineBrain brain;

    void Awake()
    {
        brain = GetComponent<CinemachineBrain>();
        collideAgainst = ~LayerMask.GetMask("Player");
    }

    void LateUpdate()
    {
        if (brain == null || !brain.isActiveAndEnabled)
        {
            return;
        }

        Vector3 lookAt = brain.State.ReferenceLookAt;
        Vector3 cameraPos = transform.position;
        Vector3 offset = cameraPos - lookAt;
        float distance = offset.magnitude;
        if (distance < 0.05f)
        {
            return;
        }

        Vector3 direction = offset / distance;
        float closest = minimumDistance + cameraRadius;
        if (distance <= closest)
        {
            return;
        }

        if (Physics.SphereCast(
            lookAt + direction * closest,
            cameraRadius,
            direction,
            out RaycastHit hit,
            distance - closest,
            collideAgainst,
            QueryTriggerInteraction.Ignore))
        {
            transform.position = lookAt + direction * (closest + hit.distance);
        }
    }
}
