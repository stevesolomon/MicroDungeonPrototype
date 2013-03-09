using UnityEngine;
using System.Collections;
using System;

public delegate void OnNoticedEntityHandler(EntityComponent noticed);

public class AttackNoticeWatcher : MonoBehaviour 
{
	public event OnNoticedEntityHandler OnNoticedEntityEvent;
	
	public event OnNoticedEntityHandler OnStoppedNoticingEntityEvent;
	
	public TargetingManager myTargetManager;
	
	protected Type myTargetType;
	
	protected void Start()
	{
		myTargetType = myTargetManager.myTargetType;		
	}
	
	protected void OnTriggerEnter(Collider other)
	{
		EntityComponent newTarget;
		
		if (other.gameObject.CompareTag("Hero") || other.gameObject.CompareTag("Monster") || other.gameObject.CompareTag("Boss"))
		{
			newTarget = other.gameObject.GetComponent<EntityComponent>();
				
			//Check to make sure it's actually an enemy that we found.
			if (newTarget != null && newTarget.GetType() == myTargetType)
			{
				//If we have raycast targeting enabled, check to be sure we can actually see it.
				if ((myTargetManager.LineOfSightTargeting && myTargetManager.EntityInView(newTarget)) || !myTargetManager.LineOfSightTargeting)
				{
					//We can see it, let's let anyone listening know!
					if (OnNoticedEntityEvent != null)
						OnNoticedEntityEvent(newTarget);
				}
			}
		}
	}
	
	protected void OnTriggerExit(Collider other)
	{
		EntityComponent newTarget;
		
		if (other.gameObject.CompareTag("Hero") || other.gameObject.CompareTag("Monster") || other.gameObject.CompareTag("Boss"))
		{
			newTarget = other.gameObject.GetComponent<EntityComponent>();
				
			//Check to make sure it's actually a target that we found.
			if (newTarget != null && newTarget.GetType() == myTargetType)
			{
				if (OnStoppedNoticingEntityEvent != null)
					OnStoppedNoticingEntityEvent(newTarget);
			}
		}		
	}
}
