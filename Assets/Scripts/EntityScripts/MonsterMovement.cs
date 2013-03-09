using UnityEngine;
using System.Collections;

public class MonsterMovement : MovementManager
{
	protected HeroPartyManager partyManager;
	
	protected override void Start ()
	{
		partyManager = GameObject.Find("PartyManager").GetComponent<HeroPartyManager>();
		
		//myBehaviorManager.OnBehaviorStateChangedEvent += HandleOnBehaviorStateChangedEvent;
		
		base.Start ();
	}
	
	protected override void Update ()
	{		
		base.Update();
	}
}
