using UnityEngine;
using System.Collections;

public delegate void GoldChangedHandler(int currGold, int change);

public class PlayerStatusManager : MonoBehaviour
{
	public event GoldChangedHandler OnGoldChanged;
	
	public HeroPartyManager heroManager;
	
	public GoldGUIManager goldGUIManager;
		
	void Start()
	{
		if (heroManager == null)
			heroManager = GameObject.Find("PartyManager").GetComponent<HeroPartyManager>();
		
		heroManager.onHeroDied += HandleOnHeroDied;
		
		if (goldGUIManager == null)
			goldGUIManager = GameObject.Find("GoldGUIManager").GetComponent<GoldGUIManager>();
	}

	protected void HandleOnHeroDied(EntityComponent source, EntityComponent killer)
	{
		if (killer != null && killer is MonsterComponent)
		{
			int gold = EntityFactory.GetInstance().GetHeroInfo(source.DataName).gold;
			AddGold(gold);	
			goldGUIManager.FireGoldParticle(gold, source.transform.position);
		}
	}
	
	public int Gold
	{
		get;
		protected set;
	}
	
	public void AddGold(int gold)
	{
		Gold += gold;	
		
		ChangedGold(gold);
	}
	
	public void RemoveGold(int gold)
	{
		Gold -= gold;
		
		ChangedGold(gold);
	}
	
	private void ChangedGold(int change)
	{
		if (OnGoldChanged != null)
			OnGoldChanged(Gold, change);		
	}
}
