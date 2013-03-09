using UnityEngine;
using System.Collections;

public class BossDefaultAttackBehavior : BaseBehavior
{
	
	public override void Update()
	{
		//A boss will simply sit around and idle until a target enters its range (and can be seen)
		//or something attacks it (in which case it will begin moving towards the target.
		if (State == BaseBehaviorStates.Idle)
		{
			//Check if we have any units in range to attack.
			if (MyBehaviorManager.myTargetingManager.TargetsInRange)
			{
				if (TargetNearestInRange())
				{					
					ChangeState(BaseBehaviorStates.PerformingActions);
					MyBehaviorManager.StopMoving();
				}				
			}
			else if (MyBehaviorManager.IsBeingAttacked) //If we're being attacked by anything not in range then move towards them
			{
				EntityComponent entityToMoveTo = MyBehaviorManager.GetNearestAttacker();
					
				if (entityToMoveTo != null) //Tell our BehvaiorManager that we want to move towards this entity!
				{
					MyBehaviorManager.MoveTowardsTransform(entityToMoveTo.transform);
					ChangeState(BaseBehaviorStates.Moving);
				}				
			}
			
		}		
		else if (State == BaseBehaviorStates.Moving)
		{
			//If we have any targets in range then begin attacking them.
			if (MyBehaviorManager.myTargetingManager.TargetsInRange)
			{
				if (TargetNearestInRange())
				{
					ChangeState(BaseBehaviorStates.PerformingActions);
					MyBehaviorManager.StopMoving();
				}				
			}
			else if (MyBehaviorManager.MovingTowards == null) //We've lost our target, let's move back to home.
			{
				BossMovement movement = MyBehaviorManager.myEntityComponent.myMovement as BossMovement;
				
				ChangeState(BaseBehaviorStates.Idle);
				MyBehaviorManager.MoveTowardsTransform(movement.parentSpawner.transform);				
			}			
		}
		else if (State == BaseBehaviorStates.PerformingActions)
		{
			//If we're performing actions but we don't currently have a target then we have to switch to idling.
			if (MyBehaviorManager.CurrentTarget == null)
			{
				//Move back to our home base and then idle...
				BossMovement movement = MyBehaviorManager.myEntityComponent.myMovement as BossMovement;
				MyBehaviorManager.MoveTowardsTransform(movement.parentSpawner.transform);	
				ChangeState(BaseBehaviorStates.Idle);				
			}			
		}
	}
}
