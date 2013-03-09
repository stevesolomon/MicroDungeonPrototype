using UnityEngine;
using System.Collections;
using System.Xml;
using System;

public abstract class AreaOfEffectAction : BaseAction 
{
	public float Radius { get; set; }
	
	public int layerMask = 1 << LayerMask.NameToLayer("Entity");
	
	protected Type myTargetType;
	
	public override void DeserializeProperties(XmlNode node)
	{
		base.DeserializeProperties(node);
		
		float radius = 0f;
		
		XmlNode radiusNode = node.SelectSingleNode("./radius");
		
		if (radiusNode != null)
			radius = float.Parse(radiusNode.InnerText);
			
		this.Radius = radius;		
		
		myTargetType = this.MyActionManager.myTargetingManager.myTargetType;
	}
}
