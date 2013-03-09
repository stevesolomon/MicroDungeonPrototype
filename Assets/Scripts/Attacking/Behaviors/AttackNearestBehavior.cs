using UnityEngine;
using System.Collections;

/// <summary>
/// A Behavior that moves towards the nearest entity and applies Actions to 
/// the nearest target. Being attacked results in this Behavior immediately
/// shifting to perform actions against the attacker.
/// </summary>
public class AttackNearestBehavior : BaseBehavior 
{	
	public override void Update () 
	{
		//If we're idle, then ask the TargetingManager for the closest target to us and move towards it.
		if (State == BaseBehaviorStates.Idle)
		{
			//EntityComponent nearest = MyBehaviorManager.myTargetingManager.LocateNearestTarget();
			
		}
		else if (State == BaseBehaviorStates.Moving) //If we're moving then check if we can target anything yet.
		{
			
			
		}
		
	}
}
