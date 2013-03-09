using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossTargetingManager : TargetingManager 
{
	public AttackNoticeWatcher viewWatcher;
	
	protected List<EntityComponent> unitsNoticed;
	
	public bool HasNoticedUnits
	{
		get { return unitsNoticed.Count > 0; }
	}
		
	protected override void Awake ()
	{
		viewWatcher = transform.FindChild("AttackNoticeRange").GetComponent<AttackNoticeWatcher>();
		viewWatcher.OnNoticedEntityEvent += HandleOnNoticedEntityEvent;
		viewWatcher.OnStoppedNoticingEntityEvent += HandleOnStoppedNoticingEntityEvent;
		
		base.Awake();
	}

	void HandleOnStoppedNoticingEntityEvent(EntityComponent noticed)
	{
		unitsNoticed.Remove(noticed);
	}

	void HandleOnNoticedEntityEvent(EntityComponent noticed)
	{
		if (!unitsNoticed.Contains(noticed))
		{
			noticed.onEntityDeath += HandleOnEntityDeath;
			unitsNoticed.Add(noticed);	
		}
	}

	void HandleOnEntityDeath(EntityComponent source, EntityComponent killer)
	{
		unitsNoticed.Remove(source);
		source.onEntityDeath -= HandleOnEntityDeath;
	}
	
	public EntityComponent GetNoticedUnit()
	{
		if (HasNoticedUnits)
		{
			return unitsNoticed[0];			
		}		
		
		return null;
	}
}
