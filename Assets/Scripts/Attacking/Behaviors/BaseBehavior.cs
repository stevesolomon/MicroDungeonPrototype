using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;

public enum BaseBehaviorStates
{
	Idle,
	Moving,
	PerformingActions,
	Dying,
	Dead
}

public class BaseBehavior : IBehavior
{
	public BehaviorManager MyBehaviorManager { get; set; }
	
	public BaseBehaviorStates State { get; private set; }
	
	public virtual void ChangeState(BaseBehaviorStates newState)
	{
		if (newState != State)
		{
			State = newState;
			MyBehaviorManager.BehaviorChangedState(newState);
		}
	}
	
	public virtual void Update()
	{
			
	}
		
	public virtual void DeserializeProperties(XmlNode node)
	{
		
	}
	
	protected virtual bool TargetNearestInRange()
	{
		bool targeted = false;
		
		if (MyBehaviorManager.HasTargets)
		{
			EntityComponent target = MyBehaviorManager.myTargetingManager.LocateNearestInRange();
				
			if (target != null) 
			{
				//Request that we lock on to the target and, if it worked, switch our state to performing actions!
				bool lockedOn = MyBehaviorManager.myTargetingManager.LockOnToTarget(target);
				
				if (lockedOn)
				{
					MyBehaviorManager.CurrentTarget = target;
					targeted = true;
				}			
			}					
		}
		
		return targeted;		
	}
}
