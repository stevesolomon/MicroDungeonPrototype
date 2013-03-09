using UnityEngine;
using System.Collections;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;
using System;

public abstract class BaseAction : IAction 
{
	public ActionManager MyActionManager { get; set; }
	
	public float Range { get; set; }
	
	public float ActionSpeed { get; set; }
	
	public float ChargeTime { get; set; }
	
	public float Magnitude { get; set; }
	
	public GameObject Projectile { get; set; }
		
	public bool IsRunning { get; protected set; }
	
	public EntityComponent MyEntityComponent 
	{ 
		get { return MyActionManager.myEntityComponent; } 
	}
	
	public event OnPerformingActionHandler OnPerformingActionEvent;
	
	public event OnTargetHitHandler OnTargetHitEvent;
	
	/// <summary>
	/// The Dictionary of all attack modifiers caused by attacks from this Entity.
	/// </summary>
	protected Dictionary<string, EntityModifier> actionModifiers;
	
	public BaseAction()
	{
		actionModifiers = new Dictionary<string, EntityModifier>();	
	}
			
	public virtual void FireAction(EntityComponent target)
	{
		OnPerformingAction(target);
		
		if (Projectile != null) //We're firing a projectile, wait for it before we apply the results of our action!
		{
			Vector3 offset = MyEntityComponent.attackPoint.position;
			offset.x += Projectile.GetComponent<ProjectileEntity>().FiringOffset.x * (MyEntityComponent.Facing == Facing.Left ? -1 : 1);
			offset.z += Projectile.GetComponent<ProjectileEntity>().FiringOffset.y;
			
			GameObject newProj = (GameObject) GameObject.Instantiate(Projectile, offset, Quaternion.identity);
			AnimatedProjectile projectileEntityScript = newProj.GetComponent<AnimatedProjectile>();
			projectileEntityScript.Target = target;
			projectileEntityScript.speed = Projectile.GetComponent<ProjectileEntity>().speed;
			
			//projectileEntityScript.animationName = Projectile.GetComponent<AnimatedProjectile>().animationName;
			
			projectileEntityScript.OnProjectileHitTargetEvent += HandleOnTargetHit;
		}
		else
			HandleOnTargetHit(null, target);
	}
	
	protected virtual void OnPerformingAction(EntityComponent target)
	{
		if (OnPerformingActionEvent != null)
			OnPerformingActionEvent(MyEntityComponent, target);
	}
	
	public virtual void HandleOnTargetHit(ProjectileEntity source, EntityComponent target)
	{
		//Apply our attack effects.
		if (target != null && target.Alive)
		{
			foreach (EntityModifier modifier in actionModifiers.Values)
			{
				modifier.DeepClone().Attach(target);
			}
		}		
		
		//Let any listeners know
		if (OnTargetHitEvent != null)
			OnTargetHitEvent(MyEntityComponent, MyActionManager.myBehaviorManager.CurrentTarget);
	}
	
	public virtual void DeserializeProperties(XmlNode node)
	{
		float range = 0.0f, actionSpeed = 0.0f, chargeTime = 0.0f, magnitude = 0.0f;
		string targetTypeName = "";
		Type targetType = null;
		
		if (node.SelectSingleNode("./range") != null)
		{
			float.TryParse(node.SelectSingleNode("./range").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out range);	
		}
		
		if (node.SelectSingleNode("./speed") != null)
		{
			float.TryParse(node.SelectSingleNode("./speed").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out actionSpeed);			
		}
		
		if (node.SelectSingleNode("./chargeTime") != null)
		{
			float.TryParse(node.SelectSingleNode("./chargeTime").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out chargeTime);			
		}
		
		if (node.SelectSingleNode("./magnitude") != null)
		{
			float.TryParse(node.SelectSingleNode("./magnitude").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out magnitude);	
		}
		
		if (node.SelectSingleNode("./targetType") != null)
		{
			targetTypeName = node.SelectSingleNode("./targetType").InnerText;
		}
		
		this.Range = range;
		this.ActionSpeed = actionSpeed;
		this.ChargeTime = chargeTime;
		this.Magnitude = magnitude;
		
		XmlNode projectileNode = node.SelectSingleNode("./projectile");
		
		switch (targetTypeName.ToLowerInvariant())
		{
			case "monster":	
				targetType = typeof(MonsterComponent);
				break;
			case "hero":
				targetType = typeof(HeroComponent);
				break;			
		}
		
		MyActionManager.myTargetingManager.myTargetType = targetType;
		
		if (projectileNode != null)
		{
			DeserializeProjectile(projectileNode);	
		}
		
		XmlNode actionModifiersNode = node.SelectSingleNode("./actionModifiers");
		
		if (actionModifiersNode != null)
		{
			DeserializeActionModifiers(actionModifiersNode);
		}
	}
	
	protected virtual void DeserializeProjectile(XmlNode node)
	{
		GameObject projectile = EntityFactory.GenerateProjectile(node);
		this.Projectile = projectile;		
	}
	
	protected virtual void DeserializeActionModifiers(XmlNode actionModifiersNode)
	{
		actionModifiers = EntityFactory.LoadActionModifiers(actionModifiersNode);		
	}
}
