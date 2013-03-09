using UnityEngine;
using System.Collections;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

public abstract class TimedEntityModifier : EntityModifier
{
	public float totalEffectTime;
	
	protected float timeRemaining;
	
	public TimedEntityModifier() { }
	
	public TimedEntityModifier(TimedEntityModifier old)
		: base(old)
	{
		this.totalEffectTime = old.totalEffectTime;	
	}
	
	public override bool Attach(EntityComponent targetEntity)
	{
		bool attached = base.Attach(targetEntity);
		
		if (attached)
			timeRemaining = totalEffectTime;
		
		return attached;
	}
	
	public override void Update ()
	{
		if (target == null)
			Detach();
		
		timeRemaining -= Time.deltaTime;
		
		if (timeRemaining <= 0f)
			Detach();
						
		base.Update();
	}
	
	public override void LoadFromXml(XmlNode dataNode)
	{
		float time = 0f;
		
		if (dataNode.SelectSingleNode("time") != null)
		{
			float.TryParse(dataNode.SelectSingleNode("time").InnerText, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out time);			
		}
		
		totalEffectTime = time;
	}
}

	
