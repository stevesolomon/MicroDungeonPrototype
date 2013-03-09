using UnityEngine;
using System.Collections;

public delegate void BossSpawnedHandler(EntityComponent boss);

public delegate void BossDiedHandler(EntityComponent bossComponent, EntityComponent lastAttacker);

public class BossSpawner : MonoBehaviour 
{
	public event BossSpawnedHandler OnBossSpawned;
	
	public event BossDiedHandler OnBossDeath;
	
	public EntityFactory entityFactory;
	
	public string bossName;
	
	protected bool spawned;
	
	/// <summary>
	/// We immediately spawn the boss with the basic BossSpawner.
	/// </summary>
	protected virtual void Start()
	{
		entityFactory = EntityFactory.GetInstance();
		spawned = false;
	}
	
	protected void HandleBossSpawned(EntityComponent boss)
	{
		if (OnBossSpawned != null)	
			OnBossSpawned(boss);		
	}
	
	protected void HandleBossDied(EntityComponent boss, EntityComponent lastAttacker)
	{
		boss.onEntityDeath -= HandleBossDied;
		
		if (OnBossDeath != null)
			OnBossDeath(boss, lastAttacker);		
	}
	
	protected virtual void Update () 
	{
		if (!spawned)
		{
			GameObject boss = entityFactory.GenerateBoss(bossName);
			boss.transform.position = transform.position;
			
			EntityComponent bossComponent = boss.GetComponent<EntityComponent>();
			bossComponent.onEntityDeath += HandleBossDied;
			
			BossMovement bossMove = bossComponent.myMovement as BossMovement;
			bossMove.parentSpawner = this;
			
			spawned = true;	
			
			HandleBossSpawned(bossComponent);
		}			
	}
}
