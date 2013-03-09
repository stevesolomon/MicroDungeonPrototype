using UnityEngine;
using System.Collections;

public class HeroSpawner : MonoBehaviour 
{	
	protected EntityFactory entityFactory;
	
	protected HeroPartyManager partyManager;
	
	protected static int num = 0;

	// Use this for initialization
	void Start () 
	{		
		entityFactory = EntityFactory.GetInstance();
		partyManager = GameObject.Find("PartyManager").GetComponent<HeroPartyManager>();
	}
	
	public void SpawnHero(WaveHero heroDef)
	{
		if (heroDef != null)
		{
			GameObject newHero = entityFactory.GenerateHero(heroDef.heroName);
			newHero.name = newHero.name + num++;
			newHero.transform.position = transform.position;
			
			//Add the hero to the correct party.
			partyManager.AddHeroToParty(heroDef.partyName, newHero.GetComponent<HeroComponent>());
		}		
	}
}
