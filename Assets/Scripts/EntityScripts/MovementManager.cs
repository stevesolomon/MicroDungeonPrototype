using UnityEngine;
using System.Collections;
using Pathfinding;

public delegate void ReachedEndOfPathHandler(EntityComponent entity);

public class MovementManager : MonoBehaviour, IPauseable
{	
	/// <summary>
	/// How often we will search for new paths.
	/// </summary>
	public float repathRate = 0.65f;
	
	/// <summary>
	/// How close we need to be to a node along the path to count it as visited.
	/// </summary>
	public float minPathNodeDistance = 15f;
	
	/// <summary>
	/// How close we need to be to a target to count it as visited.
	/// </summary>
	public float minTargetDistance = 15f;
	
	/// <summary>
	/// How fast we move in units per second.
	/// </summary>
	public float movementSpeed = 10f;
	
	/// <summary>
	/// How fast we rotate to move along a different axis.
	/// </summary>
	public float rotationSpeed = 100f;
	
	/// <summary>
    /// Whether or not we can search for paths. If false, the AI will no longer search for paths.
    /// Every repathRate seconds, it will check to see if it can search for paths once again.
    /// </summary>
    public bool canSearchForPaths = true;

    /// <summary>
    /// Whether or not we can move at all.
    /// </summary>
    public bool canMove = true;
	
	public BehaviorManager myBehaviorManager;
	
	public ReachedEndOfPathHandler ReachedEndOfPathEvent;
	
	/// <summary>
    /// The CharacterController used to control our movement.
    /// </summary>
    public CharacterController controller;
	
	/// <summary>
    /// Our Seeker component which handles the pathfinding calls.
    /// </summary>
    public Seeker seeker;
	
	/// <summary>
	/// The target position to move towards.
	/// </summary>
	protected Vector3 targetPosition;
	
	/// <summary>
    /// Current A* pathfinding path node we're moving towards.
    /// </summary>
    protected int pathIndex = 0;
	
	/// <summary>
    /// The A* pathfinding path we're traveling down towards the target.
    /// </summary>
    protected Vector3[] path;
	
	/// <summary>
	/// The time when we last searched for a path/repathed.
	/// </summary>
	protected float timeOfLastPathSearch = -1f;
	
	protected float timeSinceLastRepath = 9999f;
	
	public Vector3 PreviousPosition
	{
		get;
		protected set;
	}
	
	public Vector3 DistanceLastMoved
	{
		get { return transform.position - PreviousPosition; }
	}
	
	public Facing Facing
	{
		get;
		set;
	}
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	protected Transform currTarget;
	
	/// <summary>
	/// Gets or sets the current target we wish to move towards.
	/// </summary>
	/// <value>
	/// The current target to move towards.
	/// </value>
	public virtual Transform CurrTarget
	{
		get { return currTarget; }
		set
		{			
			if (currTarget != value)
			{
				currTarget = value;
				
				if (value != null)
				{
					targetPosition = currTarget.position;	
					ForceRepath();
				}				
			}
		}
	}
	
	// Use this for initialization
	protected virtual void Start () 
	{
		
		if (myBehaviorManager == null)
			myBehaviorManager = GetComponent<BehaviorManager>();
		
		//Link up with our behavior manager's movement events
		myBehaviorManager.OnRequestStartMovingEvent += HandleOnMovementRequestEvent;
		myBehaviorManager.OnRequestStopMovingEvent += HandleOnStopMovementRequestEvent;
		myBehaviorManager.OnBehaviorStateChangedEvent += HandleOnBehaviorStateChangedEvent;
	}

	protected virtual void HandleOnBehaviorStateChangedEvent(EntityComponent entity, BaseBehaviorStates newState)
	{
		if (newState == BaseBehaviorStates.Dying)
			canMove = false;
	}

	protected virtual void HandleOnStopMovementRequestEvent(EntityComponent source)
	{
		//When the BehaviourManager tells us to stop moving towards an entity, just set our current target to null.
		//We'll set it to a waypoint later (presumably) once the BehaviorManager decides to either shift state or set a new entity target.
		CurrTarget = null;
	}

	protected virtual void HandleOnMovementRequestEvent(EntityComponent source, Transform target)
	{
		//When the BehaviorManager tells us to move towards an Entity, let's just do it!
		CurrTarget = target;
	}
	
	// Update is called once per frame
	protected virtual void Update () 
	{
		if (Paused || !canMove)
			return;	
		
		PreviousPosition = transform.position;
		timeSinceLastRepath += Time.deltaTime;
		
		if (CurrTarget == null)
			return;
		
		//If we have an actual target as a transform them update our target position if the transform has changed position.
		if (CurrTarget.transform.position != this.targetPosition)
		{
			this.targetPosition = CurrTarget.transform.position;
		}
		
		//if (timeSinceLastRepath > repathRate)
		//{
		//	timeSinceLastRepath = 0f;
		//	Repath();
		//}
		
		if (path == null || pathIndex >= path.Length || pathIndex < 0 || !canMove)
        {
            return;
        }	
		
		//If we're close enough to the current waypoint node then target the next one in the Path.
	    Vector3 currWaypoint = path[pathIndex];
	    currWaypoint.y = transform.position.y;
		
		while ((currWaypoint - transform.position).sqrMagnitude < minPathNodeDistance * minPathNodeDistance)
        {
            pathIndex++;
            
            //If we're going to be going to the target now, we want to ensure we have to get targetReached distance to it
            if (pathIndex >= path.Length)
            {
                if ((currWaypoint - transform.position).sqrMagnitude < minTargetDistance * minTargetDistance)
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
            currWaypoint.y = transform.position.y;
        }
		
		//How far are we to the waypoint...
	    Vector3 direction = currWaypoint - transform.position;		
		
        // Rotate towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        //Move Forwards - forwardDir is already normalized
        Vector3 forwardDir = transform.forward;
        forwardDir = forwardDir * movementSpeed;
        forwardDir *= Mathf.Clamp01(Vector3.Dot(direction.normalized, transform.forward));

		controller.Move(forwardDir * Time.deltaTime);
		
		if (forwardDir.x > 0 && Facing != Facing.Right)
			Facing = Facing.Right;
		else if (forwardDir.x <= 0 && Facing != Facing.Left)
			Facing = Facing.Left;
	}
	
	/// <summary>
    /// Constructs a new path to the target.
    /// </summary>
    public virtual void Repath()
    {
        timeOfLastPathSearch = Time.time;  
		
		if (CurrTarget == null)
			return;
		
        //Make sure we can actually construct a new path. The last case deals with the seeker still constructing
        //the path from the last time we queried it, so we need to wait longer.
        if (seeker == null || !canSearchForPaths || (!seeker.IsDone() && this.gameObject.active))
        {			
            StartCoroutine(WaitToRepath());
            return;
        }		
		
		ResumeMovement();
		
		//Start a new path from transform.positon to target.position, return the result to the function OnPathComplete
        seeker.StartPath(transform.position, targetPosition, OnPathConstructionComplete);
	}
	
	protected void ForceRepath()
	{
		seeker.StartPath(transform.position, targetPosition, OnPathConstructionComplete);		
	}
	
	/// <summary>
    /// Waits for repathRate seconds and then requests that a new Path be constructed.
    /// </summary>
    public IEnumerator WaitToRepath()
    {
        float timeLeft = repathRate; //- (Time.time - timeOfLastPathSearch);

        yield return new WaitForSeconds(timeLeft);
		
        Repath();
    }
	
	/// <summary>
    /// Callback for when the construction of a Path has been completed.
    /// </summary>
    /// <param name="p">The completed Path.</param>
    public virtual void OnPathConstructionComplete(Path p)
    {
		timeSinceLastRepath = 0.0f;		
		
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
            float dist = Mathfx.DistancePointSegmentStrict(path[i], path[i + 1], transform.position);

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
    /// Stops us from moving or requesting new paths to be constructed.
    /// </summary>
    public void StopMovement()
    {
        canMove = false;
        canSearchForPaths = false;
    }

    /// <summary>
    /// Lets us resume moving or requesting new paths to be constructed.
    /// </summary>
    public void ResumeMovement()
    {
        canMove = true;
        canSearchForPaths = true;
    }
	
	/// <summary>
	/// Fired when the unit has reached the end of its proscribed path.
	/// </summary>
	protected virtual void ReachedEndOfPath()
    {
		CurrTarget = null;
		path = null;
		
        if (ReachedEndOfPathEvent != null)
			ReachedEndOfPathEvent(GetComponent<EntityComponent>());			
    }
	
	public virtual void Pause()
	{
		if (!Paused)
			Paused = true;
	}
	
	public virtual void Unpause()
	{
		if (Paused)
			Paused = false;
	}
}
