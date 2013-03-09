using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public delegate void OnEntityDeath(EntityComponent source, EntityComponent killer);

public delegate void OnEntityDestroyed(EntityComponent entityComponent);

public delegate void OnHealthChanged(EntityComponent entityComponent, float change, float currHealth, float maxHealth);

public enum Facing
{
	Left,
	Right
}

public abstract class EntityComponent : MonoBehaviour, IPauseable 
{
	protected float health = 10;
	
	public float timeFromDeathToDestroyed = 2.5f;
	
	protected bool alive;
	
	protected bool dying;
		
	public event OnEntityDeath onEntityDeath; 
	
	public event OnEntityDestroyed onEntityDestroyed;
	
	public event OnHealthChanged onHealthChanged;
	
	protected EntityComponent lastAttacker;
	
	public EntityAnimator animator;

    protected float maxHealth = 10;
	
	public Transform damageParticleLocation;
	
	public Transform damagePoint;
	
	public Transform attackPoint;
	
	public MovementManager myMovement;
	
	public ParticleManager particleManager;
	
	public const float movedDistanceThreshold = 0.005f;
	
	protected Dictionary<string, EntityModifier> attachedEntityModifiers;
	
	protected List<EntityModifier> modifiersToRemove;
	
	protected float timeRemainingToDestroyed;
	
	public Facing Facing
	{
		get { return myMovement.Facing; }
	}
		
	public string DataName
	{
		get;
		set;
	}
	
	public bool HasMoved
	{
		get 
		{ 
			Vector3 lastMovedDist = myMovement.DistanceLastMoved;
			
			return (Mathf.Abs(lastMovedDist.x) > movedDistanceThreshold || Mathf.Abs(lastMovedDist.z) > movedDistanceThreshold);
		}
	}
	
	/// <summary>
	/// The point on the object where damage effects will occur and attacks will launch at.
	/// </summary>
	public Transform DamagePoint
	{
		get { return damagePoint; }
	}
		
	public float Health
	{
		get { return health; }
		set
		{
			if (health != value)
			{			
				float oldHealth = health;
				health = value;
				
				if (onHealthChanged != null)
					onHealthChanged(this, health - oldHealth, health, maxHealth);
				
				if (health <= 0)
				{
					if (health < 0) health = 0f;
					
					Alive = false;
					timeRemainingToDestroyed = timeFromDeathToDestroyed;
				}
				
				
			}
		}
	}
	
	public float MaxHealth
	{
		get { return maxHealth; }
		set
		{
			maxHealth = value;
			
			if (maxHealth < Health)
				Health = maxHealth;			
		}
	}
	
	protected float moveSpeed;
	
	public float MoveSpeed
	{
		get { return moveSpeed; }
		set 
		{
			if (Math.Abs(value - moveSpeed) > 0.0001f)	
			{
				moveSpeed = value;
								
				if (myMovement != null)
					myMovement.movementSpeed = moveSpeed;				
			}			
		}
	}
	
	public bool Alive
    {
        get { return alive; } 
        protected set
        {
            alive = value;
        }
    } 
	
	public bool CanBeDestroyed
	{
		get;
		protected set;
	}
	
	public void DeathAnimationCompleted()
	{
		CanBeDestroyed = true;
	}
	
	public bool Paused
	{
		get;
		protected set;
	}

	protected virtual void Start() 
	{
		Health = maxHealth;	
		attachedEntityModifiers = new Dictionary<string, EntityModifier>(8);
		modifiersToRemove = new List<EntityModifier>(4);
		alive = true;
		CanBeDestroyed = false;
		
		if (particleManager == null)
			particleManager = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
	}
	
	void Update()
	{
		if (Paused)
			return;	
		
		if (!Alive)
		{
			timeRemainingToDestroyed -= Time.deltaTime;
			
			if (timeRemainingToDestroyed <= 0f)
				CanBeDestroyed = true;
		}
		
		if (CanBeDestroyed)
		{
			OnDestroy();
		}
		
		UpdateAttackModifiers();
	}
	
	void OnDestroy()
	{
		if (onEntityDeath != null)
		{
			foreach(Delegate d in onEntityDeath.GetInvocationList())
			{
				onEntityDeath -= (OnEntityDeath) d;
			}
		}	
		
		if (onEntityDestroyed != null)
			onEntityDestroyed(this);	
		
		Destroy(gameObject);
	}
	
	public void Damage(float damage, EntityComponent attacker)
	{
		if (Alive)
		{
			lastAttacker = attacker;
			Health -= damage;	
			OnDamaged(damage);
		}
		
		//Now if we are no longer Alive after this damage...
		if (!Alive)
		{
			OnEntityDeath();
		}
	}
	
	protected void OnDamaged(float damage)
	{	
		particleManager.EmitDamageParticle(damage, damageParticleLocation.position);	
	}
	
	protected virtual void OnEntityDeath()
	{
		if (onEntityDeath != null)
		{
			onEntityDeath(this, lastAttacker);
		}
		
		if (!animator.HasDeathAnimation())
			CanBeDestroyed = true;
		
		DetachAllModifiers();		
		
		dying = true;
	}	
	
	#region Entity Modifier support (visitor pattern)
	
	public bool AttachModifier(EntityModifier modifier)
	{
		bool attached = false;
		
		//If we don't have this type of EntityModifier attached already then simply add it in.
		if (!attachedEntityModifiers.ContainsKey(modifier.MyType))
		{
			attachedEntityModifiers.Add(modifier.MyType, modifier);
			attached = true;
		}
		else //We have to have the new modifier compete with the old for the right to be here.	
		{
			float oldCompValue, newCompValue;
			
			oldCompValue = attachedEntityModifiers[modifier.MyType].competitionValue;
			newCompValue = modifier.competitionValue;
			
			if (newCompValue >= oldCompValue) //Replace the old modifier with the new one.
			{
				attachedEntityModifiers[modifier.MyType].Detach();
				attachedEntityModifiers[modifier.MyType] = modifier;	
				attached = true;
			}			
		}	
		
		return attached;
	}
	
	public void DetachModifier(EntityModifier modifier)
	{
		attachedEntityModifiers.Remove(modifier.MyType);		
	}
	
	public void DetachAllModifiers()
	{
		foreach (EntityModifier modifier in attachedEntityModifiers.Values.ToList())
			modifier.Detach();
		
		attachedEntityModifiers.Clear();		
	}
	
	protected void UpdateAttackModifiers()
	{
		foreach (EntityModifier modifier in attachedEntityModifiers.Values.ToArray())
			modifier.Update();		
	}
	
	#endregion
	
	public void Pause()
	{
		if (!Paused)
		{
			MonoBehaviour[] test = GetComponentsInChildren<MonoBehaviour>();
			
			//Pause all of our (and our children's) IPauseable components.
			foreach (MonoBehaviour component in test)
			{
				IPauseable pauseableComponent = component as IPauseable;
				
				if (pauseableComponent != null && !(pauseableComponent is EntityComponent))
					pauseableComponent.Pause();
			}
			
			foreach (EntityModifier modifier in attachedEntityModifiers.Values)
				modifier.Pause();
			
			Paused = true;
		}	
	}
	
	public void Unpause()
	{
		if (Paused)
		{
			MonoBehaviour[] test = GetComponentsInChildren<MonoBehaviour>();
			
			//Unpause all of our (and our children's) IPauseable components.
			foreach (Component component in test)
			{
				IPauseable pauseableComponent = component as IPauseable;
				
				if (pauseableComponent != null && !(pauseableComponent is EntityComponent))
					pauseableComponent.Unpause();
			}
			
			foreach (EntityModifier modifier in attachedEntityModifiers.Values)
				modifier.Unpause();
			
			Paused = false;
		}		
	}
}
