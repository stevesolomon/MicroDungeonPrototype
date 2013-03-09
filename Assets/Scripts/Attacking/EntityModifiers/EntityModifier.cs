using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;

public abstract class EntityModifier : IPauseable
{
	public float competitionValue;
	
	protected EntityComponent target;
	
	public virtual string MyType
	{
		get;
		protected set;
	}
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	public EntityModifier() { }
	
	public EntityModifier(EntityModifier original)
	{
		this.competitionValue = original.competitionValue;
		this.MyType = original.MyType;		
	}
	
	public virtual bool Attach(EntityComponent targetEntity)
	{
		bool attached = false;
		
		if (targetEntity != null)
		{
			attached = targetEntity.AttachModifier(this);	
			
			if (attached)
			{
				this.target = targetEntity;
				ModifyEntity();
			}			
		}	
		
		return attached;
	}
	
	public virtual void Detach()
	{
		if (target != null)
		{
			target.DetachModifier(this);
		}
		
		ResetEntity();		
		target = null;
	}
	
	public virtual void Update()
	{
		
		
	}
	
	public virtual void Pause()
	{
		Paused = true;
	}
	
	public virtual void Unpause()
	{
		Paused = false;
	}
		
	
	protected abstract void ModifyEntity();
	protected abstract void ResetEntity();
	public abstract EntityModifier DeepClone();
	public abstract void LoadFromXml(XmlNode dataNode);	
}
