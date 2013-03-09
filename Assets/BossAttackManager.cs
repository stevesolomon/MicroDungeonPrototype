using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossAttackManager : AttackManager 
{
	protected AttackNoticeWatcher attackNoticeWatcher;
	
	protected MovementManager movementComponent;
	
	protected HeroComponent enemyTracking;
	
	protected List<HeroComponent> enemiesInRange;
	
	protected override void Start () 
	{
		myTargetType = typeof(HeroComponent);
		
		base.Start();
	}
	
	protected override void Awake()
	{
		base.Awake();
		
		attackNoticeWatcher = GetComponentInChildren<AttackNoticeWatcher>();
//		attackNoticeWatcher.onAttackNotice += HandleOnAttackNotice;
		
		movementComponent = transform.parent.GetComponent<MovementManager>();
		movementComponent.StopMovement();
		
		enemyTracking = null;
		
		enemiesInRange = new List<HeroComponent>();
	}

	protected virtual void HandleOnAttackNotice(GameObject noticed)
	{		
		HeroComponent heroComp = noticed.GetComponent<HeroComponent>();		
		
		if (heroComp != null && !enemiesInRange.Contains(heroComp))
		{
			enemiesInRange.Add(heroComp);
			heroComp.onEntityDeath += HandleOnWatchedHeroDeath;
			
			Debug.Log(this + " noticed a new enemy: " + noticed);
		}
	}

	void HandleOnWatchedHeroDeath(EntityComponent source, EntityComponent killer)
	{
		HeroComponent heroComp = source as HeroComponent;
		
		if (heroComp != null)
		{
			heroComp.onEntityDeath -= HandleOnWatchedHeroDeath;
			enemiesInRange.Remove(heroComp);
			
			if (enemyTracking == heroComp)
			{
				SetEnemy();
			}			
		}
	}
	
	protected override void OnStopTargeting(EntityComponent attacker, EntityComponent attackee)
	{
		myEntityComponent.myMovement.ResumeMovement();
		base.OnStopTargeting(attacker, attackee);
	}
	
	protected void SetEnemy()
	{
		if (enemiesInRange.Count > 0)
		{
			enemyTracking = enemiesInRange[0];
			
			movementComponent.CurrTarget = enemyTracking.transform;
			movementComponent.ResumeMovement();		
		}
		else //We have nothing, reset our target.
		{
			enemyTracking = null;
			movementComponent.CurrTarget = null;
		}
	}

	protected override void Update () 
	{
		//If we have an enemy in our range of vision but no current target then move towards the first enemy we had seen.
		if (enemyTracking == null && enemiesInRange.Count > 0)
		{
			SetEnemy();
		}		
		
		if (enemyTracking != null || attackers.Count > 0)
			base.Update();			
	}
}
