using UnityEngine;
using System.Collections;

public delegate void ParticleCompletedHandler(BaseParticle particle);

public interface IParticle : IPauseable
{	
	bool Active { get; set; }
	
	event ParticleCompletedHandler OnParticleCompletedEvent;
}
