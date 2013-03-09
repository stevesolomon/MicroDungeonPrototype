using Pathfinding;
using UnityEngine;
using System.Collections;

public class MovementToObject : MonoBehaviour, IPauseable
{
	protected Transform target;
	
    /// <summary>
    /// The target we want to move towards.
    /// </summary>
    public Transform Target
	{
		get { return target; }
		set
		{
			if (target != value)
			{
				target = value;
				Repath();
			}
		}
	}
	
	public Facing Facing
	{
		get;
		protected set;
	}
	
	public Vector3 DistanceLastMoved
	{
		get { return transform.position - PreviousPosition; }
	}
	
	public Vector3 PreviousPosition
	{
		get;
		protected set;
	}

    /// <summary>
    /// How often we will search for a new path.
    /// </summary>
    public float repathRate = 1.5f;

    /// <summary>
    /// How close we need to be to a waypoint to consider it as visited.
    /// </summary>
    public float minWaypointDistance = 16f;

    /// <summary>
    /// How close we need to be to the target to consider it reached.
    /// </summary>
    public float minTargetDistance = 32f;

    /// <summary>
    /// The speed at which we travel in units per second.
    /// </summary>
    public float speed = 8f;

    /// <summary>
    /// Whether or not we can rotate.
    /// </summary>
    public bool rotation = false;

    /// <summary>
    /// How fast we rotate/turn around if rotation is allowed.
    /// </summary>
    public float rotationSpeed = 1f;

    /// <summary>
    /// Whether or not we should draw gizmos.
    /// </summary>
    public bool drawGizmos = false;

    /// <summary>
    /// Whether or not we can search for paths. If false, the AI will no longer search for paths.
    /// Every repathRate seconds, it will check to see if it can search for paths once again.
    /// </summary>
    public bool canSearchForPaths = true;

    /// <summary>
    /// Whether or not we can move at all.
    /// </summary>
    public bool canMove = true;

    /// <summary>
    /// Our Seeker component which handles the pathfinding calls.
    /// </summary>
    protected Seeker seeker;

    /// <summary>
    /// The CharacterController used to control our movement.
    /// </summary>
    protected CharacterController controller;

    /// <summary>
    /// Our transform, cached for performance.
    /// </summary>
    protected Transform xf;

    protected float lastPathSearch = -9999f;

    /// <summary>
    /// Current path node we're moving towards.
    /// </summary>
    protected int pathIndex = 0;

    /// <summary>
    /// The path we're traveling down.
    /// </summary>
    protected Vector3[] path;
	
	public bool Paused
	{
		get;
		protected set;
	}

    public virtual void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();
        xf = transform;

        //Let's just launch a repath operation right now if we have a target.
        if (target != null)
            Repath();
		
		Facing = Facing.Left;
    }

	void Update () 
    {
		PreviousPosition = transform.position;
		
        //Make sure we have a valid path and waypoint node, and can actually move!
        if (path == null || pathIndex >= path.Length || pathIndex < 0 || !canMove)
        {
            return;
        }

        //If we're close enough to the current waypoint node then target the next one in the Path.
	    Vector3 currWaypoint = path[pathIndex];
	    currWaypoint.y = xf.position.y;

        while ((currWaypoint - xf.position).sqrMagnitude < minWaypointDistance * minWaypointDistance)
        {
            pathIndex++;
            
            //If we're going to be going to the target now, we want to ensure we have to get targetReached distance to it
            if (pathIndex >= path.Length)
            {
                if ((currWaypoint - xf.position).sqrMagnitude < minTargetDistance * minTargetDistance)
                {
                    ReachedEndOfPath();
                    return;
                }
                else //Break from our loop
                {
                    pathIndex--;
                    break;
                }
            }

            currWaypoint = path[pathIndex];
            currWaypoint.y = xf.position.y;
        }

        //How far are we to the waypoint...
	    Vector3 direction = currWaypoint - xf.position;		
		
        // Rotate towards the target
        xf.rotation = Quaternion.Slerp(xf.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        xf.eulerAngles = new Vector3(0, xf.eulerAngles.y, 0);

        //Move Forwards - forwardDir is already normalized
        Vector3 forwardDir = transform.forward;
        forwardDir = forwardDir * speed;
        forwardDir *= Mathf.Clamp01(Vector3.Dot(direction.normalized, xf.forward));

        controller.SimpleMove(forwardDir);
		
		if (forwardDir.x > 0)
		{
			Facing = Facing.Right;
		}
		else
		{
			Facing = Facing.Left;
		}
	}

    public void RemoveComponent()
    {
		StopAllCoroutines();
        Destroy(this);
    }
	
	public virtual void ResumeMovement(Transform target)
	{
		this.Target = target;		
	}

    /// <summary>
    /// Callback for when the construction of a Path has been completed.
    /// </summary>
    /// <param name="p">The completed Path.</param>
    public virtual void OnPathComplete(Path p)
    {
        //We'll want to generate a new path after a certain amount of time, so just run this
        //as a coroutine which will wait that amount of time and generate a new path for us.
        StartCoroutine(WaitToRepath());
        
        //Make sure the Path was generated without errors.
        if (p.error)
            return;

        path = p.vectorPath;

        //Find the segment in the path which is closest to the AI
        //If a closer segment hasn't been found in '6' iterations, break because it is unlikely to find any closer ones then

        //Locate the closest node in the Path to us.
        //We'll search for a few iterations to be sure we've got the closest.
        float minDist = Mathf.Infinity;
        int notCloserHits = 0;

        for (int i = 0; i < path.Length - 1; i++)
        {
            float dist = Mathfx.DistancePointSegmentStrict(path[i], path[i + 1], xf.position);

            if (dist < minDist)
            {
                notCloserHits = 0;
                minDist = dist;
                pathIndex = i + 1;
            }
            else if (notCloserHits > 6)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Waits for repathRate seconds and then requests that a new Path be constructed.
    /// </summary>
    public IEnumerator WaitToRepath()
    {
        float timeLeft = repathRate - (Time.time - lastPathSearch);

        yield return new WaitForSeconds(timeLeft);
		
        Repath();
    }
    
    /// <summary>
    /// Constructs a new path to the target.
    /// </summary>
    public virtual void Repath()
    {
        lastPathSearch = Time.time;
		
		if (this == null)
			Debug.LogError("I should be dead!!!" + gameObject);

        //Make sure we can actually construct a new path. The last case deals with the seeker still constructing
        //the path from the last time we queried it, so we need to wait longer.
        if (seeker == null || target == null || !canSearchForPaths || !seeker.IsDone() && this.gameObject.active)
        {
			if (target == null)
				Stop();
			
            StartCoroutine(WaitToRepath());
            return;
        }
		
		Resume();

        //Start a new path from transform.positon to target.position, return the result to the function OnPathComplete
        seeker.StartPath(transform.position, target.position, OnPathComplete);
    }


    /// <summary>
    /// Stops us from moving or requesting new paths to be constructed.
    /// </summary>
    public void Stop()
    {
        canMove = false;
        canSearchForPaths = false;
    }

    /// <summary>
    /// Lets us resume moving or requesting new paths to be constructed.
    /// </summary>
    public void Resume()
    {
        canMove = true;
        canSearchForPaths = true;
    }

    public virtual void ReachedEndOfPath()
    {
        //The AI has reached the end of the path
		//Stop();
    }

    /// <summary>
    /// Draws the Gizmos if enabled.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (!drawGizmos || path == null || pathIndex >= path.Length || pathIndex < 0)
        {
            return;
        }

        Vector3 currentWaypoint = path[pathIndex];
        currentWaypoint.y = xf.position.y;

        Debug.DrawLine(transform.position, currentWaypoint, Color.blue);

        float rad = minWaypointDistance;
        if (pathIndex == path.Length - 1)
        {
            rad *= minTargetDistance;
        }

        Vector3 pP = currentWaypoint + rad*new Vector3(1, 0, 0);
        for (float i = 0; i < 2*System.Math.PI; i += 0.1F)
        {
            Vector3 cP = currentWaypoint +
                         new Vector3((float) System.Math.Cos(i)*rad, 0, (float) System.Math.Sin(i)*rad);
            Debug.DrawLine(pP, cP, Color.yellow);
            pP = cP;
        }
        Debug.DrawLine(pP, currentWaypoint + rad*new Vector3(1, 0, 0), Color.yellow);
    }
	
	public bool canMoveSavedState;
	public bool canSearchForPathsSavedState;
	
	public virtual void Pause()
	{
		if (!Paused)
		{
			canMoveSavedState = canMove;
			canSearchForPathsSavedState = canSearchForPaths;
			
			canMove = false;
			canSearchForPaths = false;
			
			Paused = true;
		}		
	}	
	
	public virtual void Unpause()
	{
		if (Paused)
		{
			canMove = canMoveSavedState;
			canSearchForPaths = canSearchForPathsSavedState;
			
			Paused = false;			
		}		
	}
}
