using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void OnBehaviorStateChangedHandler(EntityComponent entity, BaseBehaviorStates newState);

public delegate void OnRequestMovementHandler(EntityComponent source, Transform target);

public delegate void OnRequestStopMovementHandler(EntityComponent source);

public class BehaviorManager : MonoBehaviour 
{
	public event OnBehaviorStateChangedHandler OnBehaviorStateChangedEvent;
	
	public event OnRequestMovementHandler OnRequestStartMovingEvent;
	
	public event OnRequestStopMovementHandler OnRequestStopMovingEvent;
	
	public ActionManager myActionManager; 
	
	public TargetingManager myTargetingManager;
	
	public MovementManager myMovementManager;
	
	public EntityComponent myEntityComponent;
	
	protected IBehavior myBehavior;
	
	public IBehavior MyBehavior 
	{ 
		get { return myBehavior; }
		set
		{
			myBehavior = value;
			myBehavior.MyBehaviorManager = this;
		}
	}
	
	public BaseBehaviorStates State 
	{ 
		get { return MyBehavior.State; } 
	}
	
	public bool IsBeingAttacked { get { return attackers.Count > 0; } }
	
	public bool HasTargets { get { return myTargetingManager.TargetsInRange; } }	
	
	public EntityComponent CurrentTarget 
	{ 
		get; 
		set; 
	}
	
	public Transform MovingTowards { get; set; }
	
	/// <summary>
	/// A list of all EntityComponents registered as attacking us.
	/// </summary>
	protected List<EntityComponent> attackers;
	
	void Start () 
	{
		attackers = new List<EntityComponent>(4);
		
		myEntityComponent.onEntityDeath += HandleOnEntityDeath;
		myEntityComponent.onEntityDestroyed += HandleOnEntityDestroyed;
	}

	void HandleOnEntityDestroyed(EntityComponent entityComponent)
	{
		myEntityComponent.onEntityDeath -= HandleOnEntityDeath;
		myEntityComponent.onEntityDestroyed -= HandleOnEntityDestroyed;
	}

	void HandleOnEntityDeath(EntityComponent source, EntityComponent killer)
	{
		myActionManager.StopActions();
		
		BehaviorChangedState(BaseBehaviorStates.Dying);
	}
	
	void Update () 
	{
		if (!myEntityComponent.Alive)
			return;
		
		//Update our behavior first and foremost.
		if (MyBehavior != null)
			MyBehavior.Update();	
	}
	
	public void BehaviorChangedState(BaseBehaviorStates newState)
	{
		Debug.Log(string.Format("{0} Changed State to: {1}", myEntityComponent, newState));
		
		if (OnBehaviorStateChangedEvent != null)
			OnBehaviorStateChangedEvent(myEntityComponent, newState);
	}
	
	/// <summary>
	/// Called via messaging when another EntityComponent starts attacking us.
	/// </summary>
	/// <param name='attacker'>
	/// The EntityComponent that is attacking us.
	/// </param>
	public virtual void BeingAttacked(EntityComponent attacker)
	{
		//We're being attacked! Add the enemy to our attackers list and watch its on death event.
		if (!attackers.Contains(attacker))
		{
			attackers.Add(attacker);
			attacker.onEntityDeath += OnWatchedEntityDeath;
		}		
	}
	
	/// <summary>
	/// Called via messaging when an attacker decides to stop attacking us.
	/// </summary>
	/// <param name='attacker'>
	/// The EntityComponent that has decided to stop attacking us.
	/// </param>
	public virtual void NoLongerAttacking(EntityComponent attacker)
	{
		if (attackers.Contains(attacker))
		{	
			attacker.onEntityDeath -= OnWatchedEntityDeath;
			attackers.Remove(attacker);
		}
	}
		
	public virtual void MoveTowardsTransform(Transform target)
	{
		MovingTowards = target;	
		
		if (OnRequestStartMovingEvent != null)
			OnRequestStartMovingEvent(myEntityComponent, target);
	}
	
	/// <summary>
	/// Requests that we stop moving towards the given target EntityComponent.
	/// </summary>
	/// <param name='target'>
	/// The target EntityComponent to stop moving towards.
	/// </param>
	public virtual void StopMoving()
	{
		MovingTowards = null;
		
		if (OnRequestStopMovingEvent != null)
			OnRequestStopMovingEvent(myEntityComponent);
	}
	
	public virtual void OnWatchedEntityDeath(EntityComponent died, EntityComponent lastAttacker)
	{		
		//If this entity was attacking us then we're safe from it!
		bool removed = attackers.Remove(died);	
		
		if (removed)			
			died.onEntityDeath -= OnWatchedEntityDeath;		
		
		if (MovingTowards == died)
			MovingTowards = null;
	}
	
	public EntityComponent GetNearestAttacker()
	{
		EntityComponent nearestAttacker = null;
		float closestDistance = float.MaxValue;
		
		//We want to target a unit. Let's pick the closest one that we can still see (if we're targeting with line-of-sight).
		//If not, then we'll simply remove that target from our unitsInRange list and pick another. If we run out of targets
		//(ie: we can't see any targets any more) then we just exit.
		for (int i = 0; i < attackers.Count; i++)
		{
			EntityComponent tempTarget = attackers[i] != null ? attackers[i].GetComponent<EntityComponent>() : null;
			
			//Our target is still alive and we can see it, let's see if it's closer than our best target thus dar!
			if (tempTarget != null && tempTarget.Alive && ((myTargetingManager.LineOfSightTargeting && myTargetingManager.EntityInView(tempTarget)) || !myTargetingManager.LineOfSightTargeting))
			{
				float distance = Vector3.Distance(tempTarget.transform.position, this.myEntityComponent.transform.position);
				
				if (distance < closestDistance)
				{
					closestDistance = distance;
					nearestAttacker = tempTarget;				
				}				
			}
		}
		
		return nearestAttacker;		
	}
	
}
