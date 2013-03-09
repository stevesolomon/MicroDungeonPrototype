using UnityEngine;
using System.Collections;

public class ExpGUIManager : MonoBehaviour 
{	
	public PlayerStatusManager playerStatusManager;
	
	public ParticleManager particleManager;

	void Awake () 
	{
		if (playerStatusManager == null)
			playerStatusManager = GameObject.Find("PlayerStatusManager").GetComponent<PlayerStatusManager>();
		
		if (particleManager == null)
			particleManager = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
	}
	
	public void FireExpParticle(int amount, Vector3 startLocation)
	{
		particleManager.EmitExperienceParticle(amount, startLocation);	
	}		
}
