using UnityEngine;
using System.Collections;

public class MonsterComponent : EntityComponent 
{
	/*protected GameObject target;
	
	protected GameObject Target
	{
		get { return target; }
		set
		{			
			if (myMovement != null)
			{
				if (value != null && value != target)
				{				
					myMovement.CurrTarget = value.transform;				
				}
				else if (value == null)
					myMovement.CurrTarget = null;
				
				target = value;
			}
		}
	}
	
	//protected HeroPartyManager partyManager;
		
	//void Awake()
	//{
		//Target = null;		
		//partyManager = GameObject.Find("PartyManager").GetComponent<HeroPartyManager>();
	//}
	
	//void Update () 
	//{
		//If we don't currently have a target then let's go get a new one.
		//if (Target == null)
		//	ChangeTarget();
	//}
	
	protected void ChangeTarget()
	{
		HeroParty closestParty = partyManager.GetClosestParty(gameObject.transform);
			
		if (closestParty != null && closestParty.Count > 0)
		{				
			Target = closestParty.GetLeader();				
			myMovement.CurrTarget = closestParty.GetLeader().transform;
				
			EntityComponent component = Target.GetComponent<EntityComponent>();
			component.onEntityDeath += OnTargetDeath;				
		}		
	}
	
	protected void OnTargetDeath(EntityComponent deadEntity, EntityComponent lastAttacker)
	{
		if (deadEntity.gameObject == Target)
		{
			Target = null;
		}
		
		deadEntity.onEntityDeath -= OnTargetDeath;			
	}
	
	*/
	
	public int experienceOnDeath = 1;
	
	public  ExpGUIManager expGUIManager;
	
	protected override void Start()
	{
		expGUIManager = GameObject.Find("ExpGUIManager").GetComponent<ExpGUIManager>();
		
		base.Start();
	}
	
	protected override void OnEntityDeath ()
	{
		if (!dying)
		{
			HeroComponent heroAttacker = lastAttacker as HeroComponent;
			
			if (heroAttacker != null)
			{
				heroAttacker.SendMessage("AddExperience", experienceOnDeath, SendMessageOptions.RequireReceiver);	
				
				if (expGUIManager != null)
				{
					expGUIManager.FireExpParticle(experienceOnDeath, damageParticleLocation.position);	
				}
			}			
		}
		
		base.OnEntityDeath();
	}
}
