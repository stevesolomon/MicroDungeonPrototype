using UnityEngine;
using System.Collections;

public class MonsterAttackNearestBehavior : BaseBehavior
{
	public override void Update () 
	{
		//If we're idle...
		if (State == BaseBehaviorStates.Idle)
		{			
			//First check if we have any targets, and if so, target them!
			if (TargetNearestInRange())
			{
				ChangeState(BaseBehaviorStates.PerformingActions);
				MyBehaviorManager.myMovementManager.StopMovement();
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
			else //If none of the above then simply query for the nearest target and let's get moving!
			{
				EntityComponent nearest = MyBehaviorManager.myTargetingManager.LocateNearestTarget();
				
				if (nearest != null)
				{
					MyBehaviorManager.MoveTowardsTransform(nearest.transform);
					ChangeState(BaseBehaviorStates.Moving);
				}
			}
			
		}
		else if (State == BaseBehaviorStates.Moving) //If we're moving then check if we have anything in range to target, otherwise keep on moving.
		{
			if (TargetNearestInRange())
			{
				MyBehaviorManager.StopMoving();
				ChangeState(BaseBehaviorStates.PerformingActions);	
			}
			else if (MyBehaviorManager.MovingTowards == null)
			{
				ChangeState(BaseBehaviorStates.Idle);
			}
		}
		else if (State == BaseBehaviorStates.PerformingActions)
		{
			//If we're performing actions but we don't currently have a target then we have to switch to idling.
			if (MyBehaviorManager.CurrentTarget == null)
			{
				ChangeState(BaseBehaviorStates.Idle);				
			}
		}
		
		base.Update();		
	}	
}
