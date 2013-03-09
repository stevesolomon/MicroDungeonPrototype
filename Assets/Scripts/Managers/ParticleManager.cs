using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleManager : MonoBehaviour, IPauseable
{
	public int particlePoolSize;
	
	public string goldParticleName = "GoldParticle";
	
	public string damageParticleName = "DamageParticle";
	
	public string experienceParticleName = "ExperienceParticle";
	
	protected List<BaseParticle> baseParticlePool;
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	void Start()
	{
		baseParticlePool = new List<BaseParticle>(particlePoolSize);
	}
	
	public void EmitGoldParticle(int amount, Vector3 position)
	{
		GameObject particle = Instantiate(Resources.Load(goldParticleName), position, Quaternion.identity) as GameObject;	
		particle.GetComponent<SpriteText>().Text = "+" + amount.ToString() + "G";	
		
		BaseParticle baseParticle = particle.GetComponent<BaseParticle>();
		baseParticle.OnParticleCompletedEvent += HandleOnParticleCompletedEvent;	
		baseParticlePool.Add(baseParticle);
	}
	
	public void EmitDamageParticle(float amount, Vector3 position)
	{
		GameObject particle = (GameObject) Instantiate(Resources.Load(damageParticleName), position, Quaternion.identity);	
		particle.GetComponent<SpriteText>().Text = amount.ToString();
		
		BaseParticle baseParticle = particle.GetComponent<BaseParticle>();
		baseParticle.OnParticleCompletedEvent += HandleOnParticleCompletedEvent;	
		baseParticlePool.Add(baseParticle);
	}
	
	public void EmitExperienceParticle(float amount, Vector3 position)
	{
		GameObject particle = (GameObject) Instantiate(Resources.Load(experienceParticleName), position, Quaternion.identity);	
		particle.GetComponent<SpriteText>().Text = amount.ToString() + "XP";
		
		BaseParticle baseParticle = particle.GetComponent<BaseParticle>();
		baseParticle.OnParticleCompletedEvent += HandleOnParticleCompletedEvent;	
		baseParticlePool.Add(baseParticle);
	}
	
	void HandleOnParticleCompletedEvent(BaseParticle particle)
	{
		baseParticlePool.Remove(particle);
		Destroy(particle.gameObject);
	}
	
	public void Pause()
	{
		if (!Paused)
		{
			Paused = true;
			
			foreach (BaseParticle particle in baseParticlePool)
				particle.Pause();			
		}		
	}
	
	public void Unpause()
	{
		if (Paused)
		{
			Paused = false;
				
			foreach (BaseParticle particle in baseParticlePool)
				particle.Unpause();				
		}
	}
}
