using System.Collections.Generic;
using UnityEngine;

public delegate void OnNewWaypoint(Waypoint waypoint);

public class WaypointManager : MonoBehaviour
{
    protected List<Waypoint> waypoints;

    protected Dictionary<GameObject, int> waypointListing;

    protected Dictionary<GameObject, OnNewWaypoint> onAssignNewWaypoint;

    protected int currWaypointActive;

    protected List<GameObject> gameObjectsToChange;
 
    /// <summary>
    /// When we start we want to search for all GameObjects with a 'waypoint' tag, and order them appropriately.
    /// </summary>
	void Awake ()
    {
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        Waypoint[] waypointComps = new Waypoint[waypointObjects.Length];

        waypoints = new List<Waypoint>();
        waypointListing = new Dictionary<GameObject, int>();
        onAssignNewWaypoint = new Dictionary<GameObject, OnNewWaypoint>();
        gameObjectsToChange = new List<GameObject>(64);

        currWaypointActive = 0;

        int j = 0;
        for (int i = 0; i < waypointObjects.Length; i++)
        {
            Waypoint waypoint = waypointObjects[i].GetComponent<Waypoint>();
            
            if (waypoint != null)
                waypointComps[j++] = waypoint;
        }

        //Organize our waypoints into a stack, highest priority first.
        BuildWaypoints(waypointComps);
    }

    /// <summary>
    /// Gets the current Waypoint assigned to the given GameObject.
    /// </summary>
    /// <param name="gameObject">The GameObject we want to retrieve the current Waypoint for.</param>
    /// <returns>The current Waypoint, or null if the GameObject has reached the final Waypoint or no Waypoints exist.</returns>
    public Waypoint GetWaypoint(GameObject gameObject)
    {
        if (!waypointListing.ContainsKey(gameObject))
            waypointListing.Add(gameObject, currWaypointActive);

        if (waypointListing[gameObject] != -1 && waypointListing[gameObject] < waypoints.Count)
            return waypoints[waypointListing[gameObject]];

        return null;
    }

    /// <summary>
    /// Subscribes to the event fired when a GameObject is assigned a new Waypoint.
    /// </summary>
    /// <param name="toSubscribe">We want to know when this GameObject has a Waypoint assigned to it.</param>
    /// <param name="onNewWaypoint">The OnNewWaypoint delegate to be called.</param>
    public void SubscribeToAssignedWaypointEvent(GameObject toSubscribe, OnNewWaypoint onNewWaypoint)
    {
        if (!onAssignNewWaypoint.ContainsKey(toSubscribe))
            onAssignNewWaypoint.Add(toSubscribe, null);

        onAssignNewWaypoint[toSubscribe] += onNewWaypoint;
    }
	
	/// <summary>
    /// Unsubscribes from the event fired when a GameObject is assigned a new Waypoint.
    /// </summary>
    /// <param name="toSubscribe">We no longer want to know when this GameObject has a Waypoint assigned to it.</param>
    /// /// <param name="onNewWaypoint">The OnNewWaypoint delegate that was originally forwarded by the subscriber.</param>
    public void UnsubscribeToAssignedWaypointEvent(GameObject toSubscribe, OnNewWaypoint onNewWaypoint)
    {
        if (onAssignNewWaypoint.ContainsKey(toSubscribe))
        	onAssignNewWaypoint[toSubscribe] -= onNewWaypoint;
    }

    /// <summary>
    /// When a GameObject has reached its target waypoint, set the waypoint to the next one.
    /// </summary>
    /// <param name="waypoint"></param>
    /// <param name="gameObject"></param>
    public void WaypointReached(Waypoint waypoint, GameObject gameObject)
    {      
		Debug.Log("Waypoint " + waypointListing[gameObject] + " was reached.");
		
        if (waypointListing.ContainsKey(gameObject) && waypointListing[gameObject] < waypoints.Count &&  waypoints[waypointListing[gameObject]] == waypoint)
        {			
            if (waypoint.deactivateOnVisit)
                DeactivateWaypoint(waypoint);

            SetNextWaypoint(gameObject);
        }
    }
	
	public void AddNewEntity(GameObject gameObject)
	{
		if (!waypointListing.ContainsKey(gameObject))
		{
			waypointListing.Add(gameObject, 1);		
			
			SetCurrentWaypoint(gameObject, waypoints[0]);
		}
	}

    /// <summary>
    /// Sets the current waypoint for the given GameObject.
    /// </summary>
    /// <param name="gameObject">The GameObject to set the waypoint for.</param>
    /// <param name="waypoint">The Waypoint to set for the given GameObject.</param>
    public void SetCurrentWaypoint(GameObject gameObject, Waypoint waypoint)
    {
        if (!waypointListing.ContainsKey(gameObject))
            waypointListing.Add(gameObject, currWaypointActive);

        int index = GetWaypointIndex(waypoint);

        if (index >= 0)
            waypointListing[gameObject] = index;

        if (onAssignNewWaypoint.ContainsKey(gameObject) && onAssignNewWaypoint[gameObject] != null)
        {
            if (waypointListing[gameObject] >= 0 && waypointListing[gameObject] <= waypoints.Count)
                onAssignNewWaypoint[gameObject](waypoints[waypointListing[gameObject]]);
            else
                onAssignNewWaypoint[gameObject](null);

            Debug.Log("Setting next waypoint to " + waypointListing[gameObject]);
        }
    }

    protected int GetWaypointIndex(Waypoint waypoint)
    {
        int index = -1;
        bool found = false;

        for (int i = 0; i < waypoints.Count; i++)
        {
            index++;

            if (waypoints[i] == waypoint)
            {
                found = true;
                break;
            }
        }

        if (!found)
            index = -1;

        return index;
    }

    /// <summary>
    /// Deactivates the given waypoint on a global level.
    /// </summary>
    /// <param name="waypoint">The Waypoint to be deactivated.</param>
    /// <remarks>Ensures that any GameObjects currently headed towards this Waypoint are directed to the next Waypoint.</remarks>
    protected void DeactivateWaypoint(Waypoint waypoint)
    {
        if (waypoint != null)
        {
            waypoint.activated = false;

            //If this is the current initial waypoint then we need to find a new one.
            SetInitialWaypoint();

            gameObjectsToChange.Clear();

            //If any game objects have been headed towards this waypoint then we have to send them to the next.
            //Dictionary doesn't like us fiddling with the values during a foreach, so we'll have to store which 
            //GameObjects we need to change in a List, and modify it later.
            foreach (KeyValuePair<GameObject, int> waypointValue in waypointListing)
            {
                if (waypoints[waypointValue.Value] == waypoint)
                    gameObjectsToChange.Add(waypointValue.Key);
            }

            for (int i = 0; i < gameObjectsToChange.Count; i++)
                SetNextWaypoint(gameObjectsToChange[i]);
        }
    }

    /// <summary>
    /// Deactivates the given waypoint on a global level.
    /// </summary>
    /// <param name="index">The index of the Waypoint to be deactivated.</param>
    /// <remarks>Ensures that any GameObjects currently headed towards this Waypoint are directed to the next Waypoint.</remarks>
    protected void DeactivateWaypoint(int index)
    {
        if (index < waypoints.Count && index >= 0)
            DeactivateWaypoint(waypoints[index]);
    }

    /// <summary>
    /// Sets the initial active waypoint that all new GameObjects are assigned as their first
    /// waypoint. This is set to waypoint with the highest priority that is still activated.
    /// </summary>
    protected void SetInitialWaypoint()
    {
        int initialWaypoint = currWaypointActive;

        while (!waypoints[initialWaypoint].activated)
            initialWaypoint++;

        currWaypointActive = initialWaypoint;
    }

    /// <summary>
    /// Sets the next waypoint for the given GameObject. If the GameObject has reached the final
    /// waypoint then the value is set to -1.
    /// </summary>
    /// <param name="gameObject">The GameObject that needs a new waypoint to move to.</param>
    protected void SetNextWaypoint(GameObject gameObject)
    {
        int nextWaypoint = waypointListing[gameObject] + 1;

        while (nextWaypoint < waypoints.Count && !waypoints[nextWaypoint].activated)
            nextWaypoint++;

        if (nextWaypoint >= waypoints.Count)
            nextWaypoint = -1;

        waypointListing[gameObject] = nextWaypoint;

        //Inform any subscribers to this gameobject that its waypoint has changed.
        if (onAssignNewWaypoint.ContainsKey(gameObject) && onAssignNewWaypoint[gameObject] != null)
        {
            if (waypointListing[gameObject] >= 0 && waypointListing[gameObject] <= waypoints.Count)
                onAssignNewWaypoint[gameObject](waypoints[waypointListing[gameObject]]);
            else
                onAssignNewWaypoint[gameObject](null);

            Debug.Log("Setting next waypoint to " + waypointListing[gameObject]);
        }
    }

    #region Helper Methods

    protected void BuildWaypoints(Waypoint[] waypointComps)
    {
        //This will be a small array so just use an insertion sort.
        for (int i = 1; i < waypointComps.Length; i++)
        {
            Waypoint currWaypoint = waypointComps[i];
            int j = i - 1;

            while (j >= 0 && waypointComps[j].priority < currWaypoint.priority)
            {
                waypointComps[j + 1] = waypointComps[j];
                j--;
            }

            waypointComps[j + 1] = currWaypoint;
        }

        //Now push them into the waypoint stack.
        for (int i = waypointComps.Length - 1; i >= 0; i--)
        {
            waypointComps[i].onWaypointReached += WaypointReached;
            waypoints.Add(waypointComps[i]);
        }
    }

    #endregion
}
