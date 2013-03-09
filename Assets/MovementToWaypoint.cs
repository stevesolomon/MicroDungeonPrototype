using UnityEngine;
using System.Collections;
using Pathfinding;

public class MovementToWaypoint : MovementToObject 
{
    public Waypoint CurrWaypoint { get; protected set; }

    protected WaypointManager waypointManager;

	// Use this for initialization
	public override void Start() 
    {
        waypointManager = GameObject.Find("WaypointManager").GetComponent<WaypointManager>();

        if (waypointManager != null)
        {
            waypointManager.SubscribeToAssignedWaypointEvent(this.gameObject, SetWaypoint);
            CurrWaypoint = waypointManager.GetWaypoint(this.gameObject);
            this.target = CurrWaypoint.transform;
        }

        base.Start();
	}
	
	public override void ResumeMovement(Transform target)
	{
		if (target == null)
		{
			CurrWaypoint = waypointManager.GetWaypoint(this.gameObject);
			this.target = CurrWaypoint.transform;
		}		
	}

    protected void SetWaypoint(Waypoint waypoint)
    {
        CurrWaypoint = waypoint;

        if (CurrWaypoint != null)
        {
            this.target = CurrWaypoint.transform;
            this.Repath();
        }
        else
            this.Stop();
    }
}
