using UnityEngine;

public delegate void WaypointReached(Waypoint waypoint, GameObject gameObject);

public class Waypoint : MonoBehaviour
{
    public int priority = 0;

    public bool deactivateOnVisit = false;

    public bool activated = true;

    public event WaypointReached onWaypointReached;
    
    /// <summary>
    /// When something has entered our trigger let the WaypointManager know.
    /// </summary>
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Waypoint registered a hit within trigger!");

        if (onWaypointReached != null)
        {
            onWaypointReached(this, other.gameObject);

            Debug.Log("Waypoint registered a hit within trigger!");
        }
    }
}
