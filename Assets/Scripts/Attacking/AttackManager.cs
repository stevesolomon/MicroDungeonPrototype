using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Pathfinding;



public delegate void StartingAttackHandler(EntityComponent attacker, EntityComponent target);

public delegate void OnChargingAttackHandler(EntityComponent attacker, EntityComponent target, float chargeTime);

public delegate void OnPerformingAttackHandler(EntityComponent attacker, EntityComponent target);

public delegate void OnStateTransitionHandler(EntityComponent source, AttackManagerState newState);

public enum AttackManagerState
{
	Uninitialized,
	Idle,
	Attacking
}

public class AttackManager : MonoBehaviour, IPauseable
{
	public enum AttackState
	{
		LookingForTarget,
		WaitingToAttack,
		Charging,
		Attacking
	}
	
	public AudioInfo audioInfo;
	
	public int raycastLayer = 1 << 14;
	
	public bool raycastTargeting;
	
	/// <summary>
	/// The manager used for handling attack effects (visual elements).
	/// </summary>
	public VisualAttackEffectManager attackEffectManager;
	
	/// <summary>
	/// The entity used to fire off an attack at an enemy.
	/// </summary>
	public GameObject projectile;
		
	/// <summary>
	/// How wide a radius this entities attacks can reach.
	/// </summary>
	public float attackRadius = 1.5f;
		
	/// <summary>
	/// The type of unit this entity attacks.
	/// </summary>
	public Type myTargetType;
	
	/// <summary>
	/// The EntityComponent controlling this Entity.
	/// </summary>
	public EntityComponent myEntityComponent;
		
	/// <summary>
	/// The amount of damage a single attack hits for.
	/// </summary>
	public float attackDamage;
	
	public AttackState State
	{
		get;
		protected set;
	}
		
	/// <summary>
	/// The time between each attack.
	/// </summary>
	protected float timeBetweenAttacks = 2.0f;
	
	protected float attackChargeTime = 1.0f;
	
	/// <summary>
	/// The time since the last attack was performed.
	/// </summary>
	protected float timeSinceLastAttack;
	
	/// <summary>
	/// The amount of attack-charging time left before we perform our attack.
	/// </summary>
	protected float chargeTimeRemaining;
	
	/// <summary>
	/// The sphere around this entity which represents the maximum range of its attacks.
	/// </summary>
	protected SphereCollider attackSphere;
	
	/// <summary>
	/// Whether or not we are currently attacking something.
	/// </summary>
	protected bool attacking = false;
	
	/// <summary>
	/// Whether or not we are currently being attacked by something.
	/// </summary>
	protected bool beingAttacked = false;
	
	/// <summary>
	/// The List of all Entities that are attacking us.
	/// </summary>
	protected List<EntityComponent> attackers;
	
	/// <summary>
	/// The List of all Entities that are in-range for us to attack.
	/// </summary>
	protected List<EntityComponent> unitsInRange;
	
	/// <summary>
	/// The Dictionary of all attack modifiers caused by attacks from this Entity.
	/// </summary>
	protected Dictionary<string, EntityModifier> attackModifiers;
	
	/// <summary>
	/// The Entity that we are currently targeting.
	/// </summary>
	protected EntityComponent currTarget;
	
	/// <summary>
	/// The nearest enemy Entity that we can move to attack.
	/// </summary>
	protected EntityComponent nearestEnemy;
	
	/// <summary>
	/// The transform of our parent, used for location/distance purposes.
	/// </summary>
	protected Transform parentTransform;
	
	/// <summary>
	/// Whether or not we're currently waiting to attack an enemy Entity.
	/// </summary>
	protected bool waitingToAttack;
	
	#region Events
	
	/// <summary>
	/// An event that fires when this AttackManager targets an enemy.
	/// </summary>
	public event OnTargetingHandler OnTargetingEvent;
	
	/// <summary>
	/// An event that fires when this AttackManager stops targeting an enemy.
	/// </summary>
	public event OnTargetingHandler OnStopTargetingEvent;	
	
	/// <summary>
	/// An event that fires when this AttackManager starts charging an attack.
	/// </summary>
	public event OnChargingAttackHandler OnChargingAttackEvent;
	
	/// <summary>
	/// An event that fires when this AttackManager launches the actual attack against the target.
	/// </summary>
	public event OnPerformingAttackHandler OnAttackEvent;
	
	/// <summary>
	/// An event that fires when a DamageEntity from this AttackManager hits its target.
	/// </summary>
	public event OnTargetHitHandler OnTargetHitEvent;
	
	public event OnStateTransitionHandler OnAttackManagerStateTransition;
	
	#endregion
	
	/// <summary>
	/// Gets or sets the distance this Entity can attack from..
	/// </summary>
	/// <value>
	/// The distance this Entity can attack from as a radius value.
	/// </value>
	public float AttackRange
	{
		get { return attackSphere.radius; }
		set 
		{ 
			attackSphere.radius = value;
			attackRadius = value;
		}
	}
	
	/// <summary>
	/// Gets or sets the amount of time required between each attack.
	/// </summary>
	/// <value>
	/// The amount of time in seconds required between attacks.
	/// </value>
	public float TimeBetweenAttacks
	{
		get { return timeBetweenAttacks; }
		set
		{
			if (value <= 0f)
				value = 0.1f;
			
			timeBetweenAttacks = value;
		}
	}
	
	/// <summary>
	/// Gets or sets the time taken to charge each attack.
	/// </summary>
	/// <value>
	/// The time taken to charge an attack.
	/// </value>
	public float AttackChargeTime
	{
		get { return attackChargeTime; }
		set
		{
			if (value < 0f)
				value = 0f;
			
			attackChargeTime = value;
		}
	}
	
	public AttackManagerState AttackManagerState
	{
		get;
		protected set;
	}
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	protected virtual void Awake()
	{
		attackSphere = GetComponent<SphereCollider>();		
		attackModifiers = new Dictionary<string, EntityModifier>();
	}

	protected virtual void Start () 
	{
		attackSphere.radius = attackRadius;	  
				
		parentTransform = gameObject.transform.parent.transform;
		
		unitsInRange = new List<EntityComponent>();
		attackers = new List<EntityComponent>();
		
		//Let's let the unit attack immediately.
		timeSinceLastAttack = timeBetweenAttacks;
		
		if (attackEffectManager == null)
			attackEffectManager = GetComponent<VisualAttackEffectManager>();
				
		State = AttackState.LookingForTarget;
		chargeTimeRemaining = AttackChargeTime;	
	}
	
	protected void OnDestroy()
	{
		//Clear the onEntityDeath events that we subscribed to.		
		foreach (EntityComponent component in attackers)
			component.onEntityDeath -= OnWatchedEntityDeath;
		
		foreach (EntityComponent component in unitsInRange)
			component.onEntityDeath -= OnWatchedEntityDeath;
		
		Destroy(projectile);
	}
	

	protected virtual void Update () 
	{
		//Don't do anything if the game is paused or if we're not alive.
		if (Paused || !myEntityComponent.Alive)
			return;	
		
		//If we have recorded anything as attacking us then we're being attacked!
		if (attackers.Count > 0)
			beingAttacked = true;
		else
			beingAttacked = false;
		
		//Check if we have no units in range and set our attacking set accordingly.
		//We won't set attacking = true here, as we may have *just* had units come into range
		//in the last Update and we're not actually attacking them yet. We'll handle the transition
		//in the state machine below.
		if (unitsInRange.Count == 0)
			attacking = false;
		
		//We can be in one of four different states:
		//
		// !attacking && !beingAttacked - Nothing to worry about, let's check to make sure no 
		//								  enemies are in range. If something is, then set it as the
		//                                target and start attacking! If not, just let our pathfinding
		//								  keep moving us around, we're fine!
		//
		// attacking && !beingAttacked  - We're attacking a target without being attacked
		//							      ourselves. Just keep attacking and choose another
		//								  target from our unitsInRange list when this one dies.
		//								  (unitsInRange is added to with OnTriggerEnters).
		//
		// !attacking && beingAttacked  - We're being attacked and possible too far away from anything to attack!
		//								  Any enemy that messages us with BeingAttacked(EntityComponent)
		//								  gets added to our attackers list, every Update we'll try to move
		//								  towards the closest enemy until something fires our OnTriggerEnters 
		//								  event and we can attack something...
		//								  -Alternatively-, we may still have enemies in our unitsInRange list, so
		//								  check that first, we'll have to deal with them before anything else.
		//
		// attacking && beingAttacked   - We're currently attacking an enemy -and- being attacked by enemies!
		//							      This is really no different than just being attacked, as once we finish
		//								  with all unitsInRange we'll move into that !a && bA state where we'll move
		//								  on to attack anything that's still attacking us!
		
		//We're not attacking anything and not currently being attacked.
		//Check if we have any units in range and, if so, attack them!
		if (!attacking && !beingAttacked)
		{
			//We're not doing anything but moving around...however, if someone entered our attack range
			//in the last update we want to start attacking them immediately! If there's more than one just
			//choose a random unit to attack.
			if (unitsInRange.Count > 0)
			{
				TargetInRangeEnemy();
			}
			else //We're not doing anything so we may have to transition to the idle state.
			{
				TransitionToState(AttackManagerState.Idle);
			}				
		}
		else if (attacking) //We're attacking something so hit it!
		{
			TransitionToState(AttackManagerState.Attacking);
			
			switch (State)
			{
				case AttackState.WaitingToAttack:
					StartAttack();
					break;
				case AttackState.Charging:
					ChargeAttack();
					break;		
				case AttackState.Attacking:
					PerformAttack();
					break;
			}
		}
		else if (!attacking && beingAttacked) //We're being attacked! We may or may not have units in range to deal with first though.
		{			
			TransitionToState(AttackManagerState.Attacking);
			
			if (unitsInRange.Count > 0) //We're being attacked but we have units to attack ourselves still so do it!
			{
				TargetInRangeEnemy();
			}
			else //We're being attacked but not attacking ourselves, find the closest enemy and set them as the target.
			{
				//We're being attacked but not attacking ourselves, find the closest enemy and set
				//them as our target to move towards.		
				nearestEnemy  = FindNearestAttacker();
				
				if (nearestEnemy != null)
				{
					//This enemy is closer, so start moving towards them!
					myEntityComponent.myMovement.CurrTarget = nearestEnemy.transform;
				}
			}
		}
	}	
	
	protected virtual void TransitionToState(AttackManagerState newState)
	{
		if (AttackManagerState != newState)
		{
			AttackManagerState = newState;
			
			if (OnAttackManagerStateTransition != null)
				OnAttackManagerStateTransition(this.myEntityComponent, AttackManagerState);
		}		
	}	
	
	/// <summary>
	/// Checks if we're ready to attack and, if so, changes the attack state to Charging.
	/// </summary>
	protected virtual void StartAttack()
	{		
		//Make sure we're facing the enemy.
		if (currTarget != null)
		{
			if (currTarget.transform.position.x > transform.position.x && myEntityComponent.myMovement.Facing != Facing.Right)
				myEntityComponent.myMovement.Facing = Facing.Right;
			else if (currTarget.transform.position.x < transform.position.x && myEntityComponent.myMovement.Facing != Facing.Left)
				myEntityComponent.myMovement.Facing = Facing.Left;
		
		
			//If we have to see our target (via raycasting) then make sure we can still see them...otherwise we have to stop attacking!
			if (raycastTargeting && !TargetInView(currTarget))
			{
				OnStopTargeting(this.myEntityComponent, currTarget);
				
				currTarget = null;
				attacking = false;			
			}		
			//If it's time again to attack and we have a target then start the charging process.
			else if (timeSinceLastAttack >= timeBetweenAttacks)
			{
				//Set our state to the charging state as we're charging up our attack!
				State = AttackState.Charging;
				
				//Start the charging timer and fire off our event.
				chargeTimeRemaining = AttackChargeTime;
				OnChargingAttack();						
			}
			else
				timeSinceLastAttack += Time.deltaTime;
		}		
	}
	
	/// <summary>
	/// Charges the attack or changes the attack state to Attacking if the charging process is complete.
	/// </summary>
	protected virtual void ChargeAttack()
	{
		//If we have to see our target (via raycasting) then make sure we can still see them...otherwise we have to stop attacking!
		if (raycastTargeting && !TargetInView(currTarget))
		{
			OnStopTargeting(this.myEntityComponent, currTarget);
			
			currTarget = null;
			attacking = false;			
		}		
		else //We can still see the enemy, or we don't have to worry about this - just attack!
		{
			//If we're done charging then set our state to the actual attack!
			if (currTarget != null && chargeTimeRemaining <= 0f)
			{
				State = AttackState.Attacking;
			}
			else
				chargeTimeRemaining -= Time.deltaTime;	
		}
	}
	
	/// <summary>
	/// Performs the attack by spawning a DamageEntity and having it target our current target.
	/// </summary>
	protected void PerformAttack()
	{
		OnPerformingAttack();
		
		if (projectile != null) //We're firing a projectile, wait for it before we apply any damage!
		{
			Vector3 offset = myEntityComponent.attackPoint.position;
			offset.x += projectile.GetComponent<ProjectileEntity>().FiringOffset.x * (myEntityComponent.Facing == Facing.Left ? -1 : 1);
			offset.z += projectile.GetComponent<ProjectileEntity>().FiringOffset.y;
			
			GameObject newProj = (GameObject) Instantiate(projectile, offset, Quaternion.identity);
			ProjectileEntity projectileEntityScript = newProj.GetComponent<ProjectileEntity>();
			projectileEntityScript.Target = currTarget;
			projectileEntityScript.speed = projectile.GetComponent<ProjectileEntity>().speed;
			
			projectileEntityScript.OnProjectileHitTargetEvent += HandleOnTargetHit;
		}
		else
			HandleOnTargetHit(null, currTarget);
			
		State = AttackState.WaitingToAttack;
		timeSinceLastAttack = 0f;		
	}
	
	#region Event Handlers
	
	
	#region Targeting and Attacking
	
	public virtual void OnChargingAttack()
	{
		if (OnChargingAttackEvent != null)
			OnChargingAttackEvent(myEntityComponent, currTarget, AttackChargeTime);		
	}
	
	public virtual void OnPerformingAttack()
	{
		currTarget.BroadcastMessage("BeingAttacked", myEntityComponent, SendMessageOptions.RequireReceiver);
		if (OnAttackEvent != null)
			OnAttackEvent(myEntityComponent, currTarget);		
	}
	
	void HandleOnTargetHit(ProjectileEntity source, EntityComponent target)
	{
		//If it was hit by a projectile then unsubscribe from its event.
		if (source != null)
		{
			source.OnProjectileHitTargetEvent -= HandleOnTargetHit;
		}
		
		if (audioInfo.audioClipNames.ContainsKey("attackHit"))
			SoundEffectManager.PlayClipAtLocation(audioInfo.audioClipNames["attackHit"], target.transform.position);
		
		if (target != null && target.Alive)
			target.Damage(attackDamage, myEntityComponent);		
		
		//Apply our attack effects.
		if (target != null && target.Alive)
		{
			foreach (EntityModifier modifier in attackModifiers.Values)
			{
				modifier.DeepClone().Attach(target);
			}
		}
		
		if (OnTargetHitEvent != null)
			OnTargetHitEvent(myEntityComponent, target);
	}
	
	/// <summary>
	/// Is called indirectly (messaging) when we are being targeted by another Entity.
	/// </summary>
	/// <param name='attacker'>
	/// The attacking entity that is targeting us.
	/// </param>
	public virtual void BeingTargeted(EntityComponent attacker)
	{
		//We're being targeted! Add the enemy to our attackers list and watch its on death event.
		/*if (!attackers.Contains(attacker))
		{
			attackers.Add(attacker);
			attacker.onEntityDeath += OnWatchedEntityDeath;
		}		*/
	}
	
		public virtual void BeingAttacked(EntityComponent attacker)
	{
		//We're being targeted! Add the enemy to our attackers list and watch its on death event.
		if (!attackers.Contains(attacker))
		{
			attackers.Add(attacker);
			attacker.onEntityDeath += OnWatchedEntityDeath;
		}		
	}
	
	/// <summary>
	/// Called when we are targeting an Entity for attack.
	/// </summary>
	/// <param name='attacker'>
	/// The Entity performing the attack (Us!).
	/// </param>
	/// <param name='target'>
	/// The Entity we are targeting.
	/// </param>
	protected virtual void OnTargeting(EntityComponent attacker, EntityComponent target)
	{
		if (OnTargetingEvent != null)
			OnTargetingEvent(attacker, target);	
		
		State = AttackState.WaitingToAttack;
		
		//Stop our movement for the time being since we have units to attack.
		myEntityComponent.myMovement.StopMovement();
		
		//Message the attacker to let them know we're attacking.
		target.BroadcastMessage("BeingTargeted", myEntityComponent, SendMessageOptions.DontRequireReceiver);
	}
	
	/// <summary>
	/// Called when we have stopped targeting an Entity for attack.
	/// </summary>
	/// <param name='attacker'>
	/// The Entity stopping the attack (Us!).
	/// </param>
	/// <param name='attackee'>
	/// The Entity we were targeting/attacking.
	/// </param>
	protected virtual void OnStopTargeting(EntityComponent attacker, EntityComponent attackee)
	{
		if (OnStopTargetingEvent != null)
		{
			OnStopTargetingEvent(attacker, attackee);				
		}
		
		State = AttackState.LookingForTarget;
	}
	
	
	#endregion
	
	
	#region Entity Death and Destruction
	
	public virtual void OnWatchedEntityDeath(EntityComponent died, EntityComponent lastAttacker)
	{
		//If this entity was our target, stop attacking it.
		if (currTarget == died)
		{
			OnStopTargeting(this.myEntityComponent, died);
			
			currTarget = null;
			attacking = false;
			
			Debug.Log(gameObject.transform.parent.gameObject + " just killed " + died);			
		}	
		
		//If this entity was also our nearest enemy, reset it.
		if (nearestEnemy == died)
			nearestEnemy = null;		
		
		//Remove the entity from our lists and usubscribe from the events we subscribed to.
		bool removed = unitsInRange.Remove(died);
		
		if (removed)
			died.onEntityDeath -= OnWatchedEntityDeath;		
		
		//If this entity was attacking us then we're safe from it!
		removed = attackers.Remove(died);	
		
		if (removed)			
			died.onEntityDeath -= OnWatchedEntityDeath;		
	}
	
	#endregion
	
	
	#region Physics
	
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
				if (!unitsInRange.Contains(newTarget) && ((raycastTargeting && TargetInView(newTarget)) || !raycastTargeting))
				{				
					unitsInRange.Add(newTarget);
					
					//Keep an eye on their OnDeath event as well.
					newTarget.onEntityDeath += OnWatchedEntityDeath;	
				}
			}
		}		
	}
	
	protected virtual bool TargetInView(EntityComponent target)
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
		
		bool removedEvent = false;
		
		if (target != null && target.GetType() == myTargetType)
		{
			//If it's our current target then unset it as our target and stop attacking.
			if (currTarget == target)
			{
				currTarget = null;
				attacking = false; 		
			}
			
			//If it was within range then remove it and untie ourselves from any of the events as well.
			if (unitsInRange.Contains(target))
			{
				target.onEntityDeath -= OnWatchedEntityDeath;
				removedEvent = true;
				
				//We have to remove it from our unitsInRange list!
				unitsInRange.Remove(target);		
			}
			
			//If it was one of our attackers do the same!
			if (attackers.Contains(target))
			{
				if (!removedEvent)
					target.onEntityDeath -= OnWatchedEntityDeath;
				
				attackers.Remove(target);				
			}
		}
	}
	
	#endregion
	
	
	#endregion
	
	
	#region Targeting and Finding Enemies Helper Methods
	
	/// <summary>
	/// Finds the nearest attacker (using pathfinding) to use so we can move towards them.
	/// </summary>
	/// <returns>
	/// The nearest nearest attacking Entity, or null if there are no attacking entities.
	/// </returns>
	protected EntityComponent FindNearestAttacker()
	{
		EntityComponent closest = null;
		float distance = float.PositiveInfinity;
		
		for (int i = 0; i < attackers.Count; i++)
		{
			if (attackers[i].Alive)
			{
				float currDistance = (attackers[i].transform.position - parentTransform.position).sqrMagnitude;
				
				if (currDistance < distance)
				{
					distance = currDistance;
					closest = attackers[i];
				}	
			}
		}
		
		return closest;		
	}
		
	/// <summary>
	/// Finds an enemy that's in-range and targets it for an attack.
	/// </summary>
	/// <returns>
	/// True if an enemy was targeted, false if there was nothing in-range.
	/// </returns>
	protected virtual bool TargetInRangeEnemy()
	{
		bool targeted = false;
		EntityComponent tempTarget = null;
		EntityComponent closestTarget = null;
		
		//We want to target a unit. Let's pick a target at random. If we have to target
		//via raycasting then we'll check if we can still see it. If not, then we'll simply
		//remove that target from our unitsInRange list and pick another. If we run out of targets
		//(ie: we can't see any targets any more) then we just exit.
		
		float closestDistance = float.PositiveInfinity;
		List<EntityComponent> toRemove = new List<EntityComponent>();
		
		//We want to target a unit. Let's pick the closest one that we can still see (if we're targeting with raycasting).
		//If not, then we'll simply remove that target from our unitsInRange list and pick another. If we run out of targets
		//(ie: we can't see any targets any more) then we just exit.
		for (int i = 0; i < unitsInRange.Count; i++)
		{
			tempTarget = unitsInRange[i] != null ? unitsInRange[i].GetComponent<EntityComponent>() : null;
			
			//Our target is still alive and we can see it, let's see if it's closer than our best target thus dar!
			if (tempTarget != null && tempTarget.Alive && ((raycastTargeting && TargetInView(tempTarget)) || !raycastTargeting))
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
							
			/* if (unitsInRange.Count > 0)
			{
				int rand = UnityEngine.Random.Range(0, unitsInRange.Count);			
				currTarget  = unitsInRange[rand] != null ? unitsInRange[rand].GetComponent<EntityComponent>() : null;
				
				//As long as our target has an EntityComponent (and maybe as long as we can see it!)
				if (currTarget != null && currTarget.Alive && ((raycastTargeting && TargetInView(currTarget)) || !raycastTargeting))
				{
					Debug.Log(this + " is starting to attack " + currTarget);
					OnTargeting(myEntityComponent, currTarget); 
					attacking = true;
						
					//Stop our movement for the time being as we need to sit still and attack.
					myEntityComponent.myMovement.StopMovement();
					
					targeted = true;
					done = true;
				}
				else //This target wasn't valid, remove it from our unitsInRange list!
					unitsInRange.Remove(currTarget);
			}
			else
				done = true;
				*/
		}
		
		foreach (EntityComponent unit in toRemove)
			unitsInRange.Remove(unit);
		
		currTarget = closestTarget;
		
		if (currTarget != null)
		{
			OnTargeting(myEntityComponent, currTarget); 
			attacking = true;
						
			//Stop our movement for the time being as we need to sit still and attack.
			myEntityComponent.myMovement.StopMovement();
					
			targeted = true;
		}
			
		
		return targeted;
	}
	
	#endregion
	
	
	#region Loading (AttackModifiers)
	
	public void AddAttackModifier(string type, EntityModifier modifier)
	{		
		if (!attackModifiers.ContainsKey(type))
			attackModifiers.Add(type, modifier);		
	}
	
	public void RemoveAttackModifier(string type)
	{
		if (attackModifiers.ContainsKey(type))
			attackModifiers.Remove(type);		
	}
	
	#endregion
	
	
	public virtual void Pause()
	{
		Paused = true;
	}	
	
	public virtual void Unpause()
	{
		Paused = false;
	}
}
