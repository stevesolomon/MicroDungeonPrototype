using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


public delegate void OnTargetingHandler(EntityComponent targeter, EntityComponent target);

public delegate void BeingTargetedHandler(EntityComponent targeter, EntityComponent source);

public class TargetingManager : MonoBehaviour 
{
	#region Events
	
	/// <summary>
	/// An event that fires when this TargetingManager has registered its own EntityComponent as being targeted by something.
	/// </summary>
	public event BeingTargetedHandler OnBeingTargetedEvent;
	
	/// <summary>
	/// An event that fires when this TargetingMananger targets an enemy.
	/// </summary>
	public event OnTargetingHandler OnTargetingEvent;
	
	/// <summary>
	/// An event that fires when this TargetingManager stops targeting an enemy.
	/// </summary>
	public event OnTargetingHandler OnStopTargetingEvent;
	
	/// <summary>
	/// An event that fires when an Entity moves outside of this TargetingManager's range.
	/// </summary>
	public event OnTargetingHandler OnEntityLeftTargetRangeEvent;
	
	/// <summary>
	/// An event that fires when an Entity moves inside of this TargetingManager's range.
	/// </summary>
	public event OnTargetingHandler OnEntityEnteredTargetRangeEvent;
	
	#endregion
	
	
	#region Instance Variables
	
	/// <summary>
	/// The layer that our rays will travel through/collide with if we're using raycast targeting.
	/// </summary>
	public int raycastLayer = 1 << 14;	
	
	/// <summary>
	/// The type of entity that we can target.
	/// </summary>
	public Type myTargetType;
	
	public EntityComponent myEntityComponent;
		
	/// <summary>
	/// A list of all EntityComponents that we are targeting.
	/// </summary>
	protected List<EntityComponent> unitsInRange;
	
	protected SphereCollider myCollider;
	
	protected float targetingRadius;
	
	#endregion
	
	
	#region Properties
	
	public bool TargetsInRange
	{
		get { return unitsInRange.Count > 0; }
	}
	
	/// <summary>
	/// Gets whether or not we need line-of-sight with an entity to target them.
	/// </summary>
	public bool LineOfSightTargeting
	{
		get;
		set;
	}
	
	public EntityComponent LockedOnTarget
	{
		get;
		protected set; 
	}
	
	public float TargetingRadius
	{
		get { return targetingRadius; }
		set
		{
			targetingRadius = value;
			myCollider.radius = value;
		}
	}
			
	
	#endregion
	
	protected virtual void Awake()
	{
		myCollider = GetComponent<SphereCollider>();
	}
	
	protected virtual void Start()
	{
		unitsInRange = new List<EntityComponent>();				
	}
	
	public virtual EntityComponent LocateNearestTarget()
	{
		List<UnityEngine.Object> potentialTargets = GameObject.FindObjectsOfType(myTargetType).ToList();	
		EntityComponent closestEntity = null;
		float closestDistance = float.MaxValue;
		
		foreach (UnityEngine.Object target in potentialTargets)
		{
			EntityComponent entity = target as EntityComponent;
			
			if (entity != null)
			{
				float distance = Math.Abs(Vector3.Distance(entity.transform.position, this.transform.position));
				
				if (distance < closestDistance)
				{
					closestEntity = entity;
					closestDistance = distance;
				}				
			}
		}	
		
		return closestEntity;
	}
	
	/// <summary>
	/// Targets the nearest in-range entity.
	/// </summary>
	/// <returns>
	/// The nearest in-range entity, or null if no targets were found in range.
	/// </returns>
	public virtual EntityComponent LocateNearestInRange()
	{
		EntityComponent tempTarget = null;
		EntityComponent closestTarget = null;
		
		float closestDistance = float.PositiveInfinity;
		List<EntityComponent> toRemove = new List<EntityComponent>();
		
		//We want to target a unit. Let's pick the closest one that we can still see (if we're targeting with line-of-sight).
		//If not, then we'll simply remove that target from our unitsInRange list and pick another. If we run out of targets
		//(ie: we can't see any targets any more) then we just exit.
		for (int i = 0; i < unitsInRange.Count; i++)
		{
			tempTarget = unitsInRange[i] != null ? unitsInRange[i].GetComponent<EntityComponent>() : null;
			
			//Our target is still alive and we can see it, let's see if it's closer than our best target thus dar!
			if (tempTarget != null && tempTarget.Alive && ((LineOfSightTargeting && EntityInView(tempTarget)) || !LineOfSightTargeting))
			{
				float distance = Vector3.Distance(tempTarget.transform.position, this.myEntityComponent.transform.position);
				
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestTarget = tempTarget;				
				}				
			}
			else //This target wasn't valid, remove it from our unitsInRange list!
					toRemove.Add(tempTarget);
		}
		
		foreach (EntityComponent unit in toRemove)
			unitsInRange.Remove(unit);
		
		return closestTarget;
	}
	
	/// <summary>
	/// Locks on to the given EntityComponent target if it is in-range.
	/// </summary>
	/// <returns>
	/// True if we locked on, false if otherwise.
	/// </returns>
	/// <param name='target'>
	/// The EntityComponent target we wish to lock on to.
	/// </param>
	public virtual bool LockOnToTarget(EntityComponent target)
	{
		bool lockedOn = false;
		
		if (unitsInRange.Contains(target))
		{
			LockedOnTarget = target;
			OnTargeting(LockedOnTarget); 
			lockedOn = true;
		}	
		
		return lockedOn;
	}
	
	/// <summary>
	/// Is called indirectly (messaging) when we are being targeted by another Entity.
	/// </summary>
	/// <param name='attacker'>
	/// The attacking entity that is targeting us.
	/// </param>
	public virtual void BeingTargeted(EntityComponent targeter)
	{
		//Let any listeners know that we're being targeted.
		if (OnBeingTargetedEvent != null)
			OnBeingTargetedEvent(targeter, myEntityComponent);
	}
	
	/// <summary>
	/// Called when we are targeting an Entity.
	/// </summary>
	/// <param name='target'>
	/// The Entity we are targeting.
	/// </param>
	protected virtual void OnTargeting(EntityComponent target)
	{
		if (OnTargetingEvent != null)
			OnTargetingEvent(this.myEntityComponent, target);	
		
		//Message the attacker to let them know we're targeting them.
		target.BroadcastMessage("BeingTargeted", myEntityComponent, SendMessageOptions.DontRequireReceiver);
	}
	
	public virtual void OnWatchedEntityDeath(EntityComponent died, EntityComponent lastAttacker)
	{
		//Remove the entity from our lists and usubscribe from the events we subscribed to.
		bool removed = unitsInRange.Remove(died);
		
		if (removed)
			died.onEntityDeath -= OnWatchedEntityDeath;		
	}
	
	protected void EntityEnteredTargetRange(EntityComponent target)
	{
		target.onEntityDeath += OnWatchedEntityDeath;
		
		unitsInRange.Add(target);
		
		if (OnEntityEnteredTargetRangeEvent != null)
			OnEntityEnteredTargetRangeEvent(myEntityComponent, target);		
	}
	
	protected void EntityLeftTargetRange(EntityComponent target)
	{
		target.onEntityDeath -= OnWatchedEntityDeath;
				
		//We have to remove it from our unitsInRange list!
		unitsInRange.Remove(target);	
		
		//Fire off the event letting anyone else know.
		if (OnEntityLeftTargetRangeEvent != null)
			OnEntityLeftTargetRangeEvent(myEntityComponent, target);
			
	}
	
	#region Targeting via Colliders
	
	public virtual void OnTriggerEnter(Collider other)
	{
		EntityComponent newTarget;
		
		//When something enters our trigger, make sure it's not a trigger before we
		//do anything else.
		if (!other.isTrigger)
		{
			newTarget = other.gameObject.GetComponent<EntityComponent>();
			
			//Check to make sure it's actually an enemy that we found.
			if (newTarget != null && newTarget.GetType() == myTargetType)
			{
				//First check to make sure we're not already watching this unit as a target.
				//Then, if we're not, and if we have raycast targeting enabled, check to be sure we can actually see it.
				if (!unitsInRange.Contains(newTarget) && ((LineOfSightTargeting && EntityInView(newTarget)) || !LineOfSightTargeting))
				{				
					unitsInRange.Add(newTarget);
					
					//Keep an eye on their OnDeath event as well.
					newTarget.onEntityDeath += OnWatchedEntityDeath;	
				}
			}
		}		
	}
	
	/// <summary>
	/// Helper method to ensure that a target is in our line-of-sight if required.
	/// </summary>
	/// <returns>
	/// True if the target is in our line-of-sight, false if otherwise.
	/// </returns>
	/// <param name='target'>
	/// The target EntityComponent to check.
	/// </param>
	public virtual bool EntityInView(EntityComponent target)
	{
		RaycastHit hitInfo;
		bool missedTarget = Physics.Linecast(transform.position, target.transform.position, out hitInfo, raycastLayer);
		
		return !missedTarget;
	}
	
	public virtual void OnTriggerStay(Collider other)
	{
		OnTriggerEnter(other);			
	}
	
	public virtual void OnTriggerExit(Collider other)
	{
		//If we were attacking something that left our trigger then we have to stop attacking it.
		EntityComponent target = other.GetComponent<EntityComponent>();
		
		if (target != null && target.GetType() == myTargetType)
		{			
			//If it was within range then remove it and untie ourselves from any of the events as well.
			if (unitsInRange.Contains(target))
			{
				EntityLeftTargetRange(target);					
			}
		}
	}
	
	#endregion
}
