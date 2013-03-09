using UnityEngine;
using System.Collections;

public class HeroDefaultBehavior : BaseBehavior
{
	public override void Update()
	{
		//If we're currently in an idle state then let our movement component know it's time to move again.
		if (State == BaseBehaviorStates.Idle)
		{
			ChangeState(BaseBehaviorStates.Moving);
		}
		else if (State == BaseBehaviorStates.Moving) //If we're moving and we don't have a CurrentTarget, make sure we can't get one (in range or being attacked).
		{
			if (MyBehaviorManager.CurrentTarget == null)
			{
				//If we have any targets in range then start performing actions on them.
				if (MyBehaviorManager.HasTargets)
				{
					EntityComponent target = MyBehaviorManager.myTargetingManager.LocateNearestInRange();
					
					if (target != null) 
					{
						//Request that we lock on to the target and, if it worked, switch our state to performing actions!
						bool lockedOn = MyBehaviorManager.myTargetingManager.LockOnToTarget(target);
						
						if (lockedOn)
						{
							ChangeState(BaseBehaviorStates.PerformingActions);	
							MyBehaviorManager.CurrentTarget = target;
						}			
					}				
				}
				else if (MyBehaviorManager.IsBeingAttacked) //Check to see if we have any entities attacking us and, if so, let's get them in-range!
				{				
					EntityComponent entityToMoveTo = MyBehaviorManager.GetNearestAttacker();
					
					if (entityToMoveTo != null) //Tell our BehvaiorManager that we want to move towards this entity!
					{
						MyBehaviorManager.MoveTowardsTransform(entityToMoveTo.transform);		
					}
				}
			}
		}	
		else if (State == BaseBehaviorStates.PerformingActions)
		{
			//If we're performing actions but we don't currently have a target then we have to switch to idling.
			if (MyBehaviorManager.CurrentTarget == null)
			{
				ChangeState(BaseBehaviorStates.Idle);
			}
			
			//If we still have a target then we don't have to do anything, our BehaviorManager will keep performing actions.
		}
		
		base.Update();
	}
	
}
