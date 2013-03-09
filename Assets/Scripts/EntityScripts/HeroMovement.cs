using UnityEngine;
using System.Collections;

public delegate void OnPathTargetChanged(Transform newTarget);

public class HeroMovement : MovementManager 
{
	public WaypointManager waypointManager;	
	
	public HeroParty myParty;
	
	public bool performingActions;
	
	public bool movingToEntity;
		
	public bool IsLeader
	{
		get;
		protected set;
	}
	
	public Waypoint CurrWaypoint
	{
		get;
		set;
	}
		
	protected HeroComponent myEntityComponent;
		
	protected void Awake()
	{		
		waypointManager = GameObject.Find("WaypointManager").GetComponent<WaypointManager>();
		waypointManager.SubscribeToAssignedWaypointEvent(this.gameObject, SetWaypoint);
		
		myBehaviorManager.OnBehaviorStateChangedEvent += HandleOnBehaviorStateChangedEvent;
		
		myEntityComponent = GetComponent<HeroComponent>();
		performingActions = false;
		movingToEntity = false;
	}
	
	protected override void Update()
	{
		//Check if we're close enough to our waypoint to set the next one.
		if ((CurrWaypoint.transform.position - transform.position).sqrMagnitude < minTargetDistance * minTargetDistance)
		{
			ReachedEndOfPath();
		}	
				
		base.Update ();
	}	
	
	protected override void HandleOnBehaviorStateChangedEvent(EntityComponent entity, BaseBehaviorStates newState)
	{
		//If we're performing actions at this point in time then mark that down so we don't change our Current Waypoint
		//And then let's stop moving!
		if (newState  == BaseBehaviorStates.PerformingActions) 
		{
			performingActions = true;
			StopMovement();
		}
		else if (newState == BaseBehaviorStates.Moving) //If we're moving then we have to find out if we need to grab a new target or not...
		{
			if (CurrTarget == null && CurrWaypoint != null) //We have no target, set our waypoint to our target
			{
				CurrTarget = CurrWaypoint.transform;				
			}
			
			ResumeMovement();
			ForceRepath();
			performingActions = false;
		}
		
		base.HandleOnBehaviorStateChangedEvent (entity, newState);
	}
	
	protected void OnAttackStateTransition(EntityComponent source, AttackManagerState state)
	{
		//If we're transitioning to idle then set our current target to the current waypoint
		if (state == AttackManagerState.Idle && CurrWaypoint != null)
		{
			CurrTarget = CurrWaypoint.transform;
			ForceRepath();
			performingActions = false;
		}		
		else if (state == AttackManagerState.Attacking)
		{
			performingActions = true;
		}
	}
	
	protected override void ReachedEndOfPath()
	{			
		base.ReachedEndOfPath();	
		
		if (!performingActions)
		{
			CurrTarget = CurrWaypoint.transform;
		}
		
		ForceRepath();
	}
	
	public void SetWaypoint(Waypoint waypoint)
    {
		if (waypoint != null)
		{
        	CurrWaypoint = waypoint;
			
			if (!performingActions)
			{
				CurrTarget = CurrWaypoint.transform;
			}
		}		
    }
}
