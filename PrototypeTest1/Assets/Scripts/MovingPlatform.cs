using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private Transform waypointsParent;
    [SerializeField] private Transform[] waypoints;

    private bool isWaiting = false;

    void Start()
    {
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[currentWaypointIndex].position;
        }
    }

    void Update()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        // Calc and move to next waypoint
        int nextWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        Transform nextWaypoint = waypoints[nextWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, nextWaypoint.position, speed * Time.deltaTime);

        // Check if arrived
        if (Vector3.Distance(transform.position, nextWaypoint.position) < 0.1f)
        {
            StartCoroutine(WaitBeforeMoving(nextWaypointIndex));
        }
    }

    IEnumerator WaitBeforeMoving(int nextIndex)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex = nextIndex;
        isWaiting = false;
    }

    // Populates the waypoints array with the children of waypointsParent in the editor so that we don't have to manually assign them
    private void OnValidate()
    {
        if (waypointsParent != null)
        {
            waypoints = new Transform[waypointsParent.childCount];

            // Assign waypoitns from the children of waypointsParent
            for (int i = 0; i < waypointsParent.childCount; i++)
            {
                waypoints[i] = waypointsParent.GetChild(i);
            }
        }
    }
}