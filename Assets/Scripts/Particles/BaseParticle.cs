using UnityEngine;
using System.Collections;

public abstract class BaseParticle : MonoBehaviour, IParticle
{
	//Workaround class since we can't use the IParticle interface with GetComponent<>
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	public bool Active
	{
		get;
		set;
	}
	
	public event ParticleCompletedHandler OnParticleCompletedEvent;
	
	public abstract void Pause();
	public abstract void Unpause();
	
	public virtual void OnParticleCompleted()
	{
		if (OnParticleCompletedEvent != null)
			OnParticleCompletedEvent(this);
	}
}
