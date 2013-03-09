using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityAnimator : MonoBehaviour, IPauseable
{
	public EntityComponent entityComponent;
	
	public ActionManager actionManager;
	
	protected Dictionary<string, int> animationInfo;
	
	public tk2dAnimatedSprite mySprite;
	
	protected int chargingActionIndex, performingActionIndex, hitActionIndex, idleIndex, movingIndex, deathIndex;
	
	protected Facing prevFacing;
	
	public bool playingIdleAnim;
	
	public bool playingEventAnim;
	
	public bool playingWalkAnim;
	
	protected bool dying;
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	public Dictionary<string, int> AnimationInfo
	{
		get { return animationInfo; }
		set
		{
			animationInfo = value;
			LoadIndices();
		}
	}
	
	public bool HasDeathAnimation()
	{
		if (deathIndex != -1)
			return true;
		
		return false;
	}
		
	void Start () 
	{
		if (entityComponent == null)
			entityComponent = transform.parent.parent.GetComponent<EntityComponent>();
		
		if (actionManager == null)
			actionManager = transform.parent.parent.FindChild("Attack").GetComponent<ActionManager>();
		
		if (mySprite == null)
			mySprite = this.GetComponent<tk2dAnimatedSprite>();
		
		//Attach ourselves to the relevant events
		entityComponent.onEntityDeath += HandleOnEntityDeath;
		entityComponent.onEntityDestroyed += HandleOnEntityDestroyed;
		
		if (actionManager != null)
		{
			actionManager.OnChargingActionEvent += HandleOnChargingActionEvent;
			actionManager.OnPerformingActionEvent += HandleOnActionEvent;
			actionManager.OnTargetHitEvent += HandleOnTargetHitEvent;
		}
		
		prevFacing = Facing.Right;
		playingIdleAnim = true;		
		playingWalkAnim = false;
		playingEventAnim = false;
		
		mySprite.animationCompleteDelegate = OnAnimationComplete;
		mySprite.animationEventDelegate = OnAnimationEvent;
				
		//When the Entity moves we want to play the movement animation
		//When the Entity does a pre/during/post attack we want to play the pre/during/post attack animation
		//We'll have to check every once in a while to see if the entity is moving or not in Update and change the animation accordingly
		//while also keeping track of the fact that we don't want to co-opt other animations currently playing.
	
	}	
	
	void OnDestroy()
	{
		if (mySprite.anim != null)
			Destroy(mySprite.anim);		
	}

	void HandleOnChargingActionEvent(EntityComponent attacker, EntityComponent target, float chargeTime)
	{
		if (chargingActionIndex != -1)
		{
			//We have to complete the entire animation in the given charge time!
			int frameCount = mySprite.anim.clips[chargingActionIndex].frames.Length;			
			float fps = frameCount / chargeTime;
			mySprite.anim.clips[chargingActionIndex].fps = fps;
			
			mySprite.Play(chargingActionIndex);
			playingEventAnim = true;
			playingIdleAnim = false;
			playingWalkAnim = false;
		}		
	}
	
	void HandleOnActionEvent(EntityComponent attacker, EntityComponent target)
	{
		if (performingActionIndex != -1)
		{
			mySprite.Play(performingActionIndex);
			playingEventAnim = true;
			playingIdleAnim = false;
			playingWalkAnim = false;
		}
	}
	
	void HandleOnTargetHitEvent(EntityComponent attacker, EntityComponent target)
	{
		if (hitActionIndex != -1)
		{
			mySprite.Play(hitActionIndex);
			playingEventAnim = true;
			playingIdleAnim = false;	
			playingWalkAnim = false;
		}
	}
	
	protected void OnAnimationComplete(tk2dAnimatedSprite sprite, int clipId)
	{
		playingEventAnim = false;	
	}
	
	/// <summary>
	/// Loads the common indices that we'll use for the basic animation. Any subclasses can define their own, we
	/// don't care, but we expect these to be the defaults (though they need not exist!).
	/// </summary>
	protected void LoadIndices()
	{
		chargingActionIndex = -1;
		if(animationInfo.ContainsKey("chargeAction"))
		{
			chargingActionIndex = animationInfo["chargeAction"];			
		}
		
		performingActionIndex = -1;
		if(animationInfo.ContainsKey("performAction"))
		{
			performingActionIndex = animationInfo["performAction"];			
		}
		
		hitActionIndex = -1;
		if(animationInfo.ContainsKey("hitAction"))
		{
			hitActionIndex = animationInfo["hitAction"];			
		}
		
		idleIndex = -1;
		if(animationInfo.ContainsKey("idle"))
		{
			idleIndex = animationInfo["idle"];			
		}
		
		movingIndex = -1;
		if(animationInfo.ContainsKey("moving"))
		{
			movingIndex = animationInfo["moving"];			
		}
		
		deathIndex = -1;
		if(animationInfo.ContainsKey("death"))
		{
			deathIndex = animationInfo["death"];			
		}		
	}	
	

	void HandleOnEntityDestroyed(EntityComponent entityComponent)
	{
		//Unhook ourselves from our events
		entityComponent.onEntityDeath -= HandleOnEntityDeath;
		entityComponent.onEntityDestroyed -= HandleOnEntityDestroyed;
		
		if (actionManager != null)
		{
			actionManager.OnChargingActionEvent -= HandleOnChargingActionEvent;
			actionManager.OnPerformingActionEvent -= HandleOnActionEvent;
			actionManager.OnTargetHitEvent -= HandleOnTargetHitEvent;
		}
	}

	void HandleOnEntityDeath(EntityComponent source, EntityComponent killer)
	{		
		//Play the death animation if we have one...
		if (!dying && deathIndex != -1)
		{
			mySprite.Play(deathIndex);
			playingEventAnim = true;
			playingIdleAnim = false;	
			playingWalkAnim = false;
		}
		
		dying = true;
	}
	
	void OnAnimationEvent(tk2dAnimatedSprite sprite, tk2dSpriteAnimationClip clip, tk2dSpriteAnimationFrame frame, int frameNum)
	{
		SendMessageUpwards(frame.eventInfo);
	}
	
	void Update() 
	{
		if (!dying)
		{
			//Check if we've changed directions from the last frame, if so, flip the sprite along the X axis.
			if (prevFacing != entityComponent.Facing)
			{
				prevFacing = entityComponent.Facing;
				transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
			}
			
			//If we're playing any animation that isn't the idle animation then just wait.
			//Otherwise, play the idle animation if we're not moving, or the moving animation if we're moving.
			if (!playingEventAnim)
			{
				if (!playingWalkAnim && entityComponent.HasMoved)
				{
					mySprite.Play(1);
					playingWalkAnim = true;
					playingIdleAnim = false;
				}
				else if (!playingIdleAnim && !entityComponent.HasMoved)
				{
					mySprite.Play(0);
					playingIdleAnim = true;
					playingWalkAnim = false;
				}
			}		
		}
	}
	
	public void Pause()
	{
		if (!Paused)
		{
			Paused = true;
			
			mySprite.Pause();
		}
	}
	
	public void Unpause()
	{
		if (Paused)
		{
			Paused = false;
			
			mySprite.Resume();				
		}
	}
}
