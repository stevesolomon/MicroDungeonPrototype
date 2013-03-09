using UnityEngine;
using System.Collections;

public class HeroAttackManager : AttackManager 
{
	protected override void Start () 
	{
		myTargetType = typeof(MonsterComponent);
		
		base.Start();
	}
	
	protected override void OnStopTargeting (EntityComponent attacker, EntityComponent attackee)
	{
		//Let us start moving again normally.
		myEntityComponent.myMovement.ResumeMovement();
		//myEntityComponent.myMovement.CurrTarget = null;
		
		base.OnStopTargeting(attacker, attackee);
	}
}
