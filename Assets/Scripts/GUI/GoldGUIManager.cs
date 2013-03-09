using UnityEngine;
using System.Collections;

public class GoldGUIManager : MonoBehaviour 
{
	public SpriteText goldText;
	
	public PlayerStatusManager playerStatusManager;
	
	public ParticleManager particleManager;
	
	public GameObject goldParticle;

	void Awake () 
	{
		if (playerStatusManager == null)
			playerStatusManager = GameObject.Find("PlayerStatusManager").GetComponent<PlayerStatusManager>();
		
		if (particleManager == null)
			particleManager = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
		
		playerStatusManager.OnGoldChanged += HandleOnGoldChanged;
	}

	void HandleOnGoldChanged(int currGold, int change)
	{
		goldText.Text = currGold.ToString();
	}
	
	public void FireGoldParticle(int amount, Vector3 startLocation)
	{
		particleManager.EmitGoldParticle(amount, startLocation);	
	}		
}
