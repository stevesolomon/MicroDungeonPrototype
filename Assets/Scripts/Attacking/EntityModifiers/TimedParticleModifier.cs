using UnityEngine;
using System.Collections;
using System.Xml;

public class TimedParticleModifier : TimedEntityModifier
{
	protected GameObject particleEmitterObject;
	
	protected ParticleSystem particleEmitter;
	
	protected string particleEmitterResourceName;
	
	public override string MyType 
	{
		get 
		{
			return "ParticleModifier";
		}
		protected set {}
	}
	
	public TimedParticleModifier() { }
		
	public TimedParticleModifier(TimedParticleModifier old)
		: base(old)
	{
		this.particleEmitterResourceName = old.particleEmitterResourceName;	
	}
	
	protected override void ModifyEntity()
	{
		if (particleEmitterResourceName != null)
		{
			particleEmitterObject = GameObject.Instantiate(Resources.Load(particleEmitterResourceName)) as GameObject;		
			
			particleEmitter = particleEmitterObject.transform.particleSystem;		
			particleEmitter.Play();
		
			if (target != null)
				particleEmitterObject.transform.position = target.transform.position;
		}
	}
	
	protected override void ResetEntity()
	{
		if (particleEmitterObject != null)
			GameObject.Destroy(particleEmitterObject);		
	}
	
	public override void Update ()
	{
		if (target != null && particleEmitterObject != null)
			particleEmitterObject.transform.position = target.transform.position + new Vector3(0f, 16f, 0f);
		
		base.Update ();
	}
	
	public override EntityModifier DeepClone()
	{		
		return new TimedParticleModifier(this);	
	}
	
	public override void LoadFromXml (XmlNode dataNode)
	{
		if (dataNode.SelectSingleNode("./particleEffect") != null)
			particleEmitterResourceName = dataNode.SelectSingleNode("./particleEffect").InnerText;
			
		base.LoadFromXml(dataNode);
	}
	
	public override void Pause ()
	{
		if (!Paused)
			particleEmitter.Pause();
		
		base.Pause();
	}
	
	public override void Unpause ()
	{
		if (Paused)
			particleEmitter.Play();
		
		base.Unpause();
	}
}
