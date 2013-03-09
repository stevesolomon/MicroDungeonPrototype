using UnityEngine;
using System.Collections;
using System.Xml;
using System.Globalization;

public class AttackAction : BaseAction
{	
	public override void HandleOnTargetHit(ProjectileEntity source, EntityComponent target)
	{
		base.HandleOnTargetHit(source, target);
		
		target.Damage(Magnitude, MyEntityComponent);		
		target.BroadcastMessage("BeingAttacked", this.MyEntityComponent);
	}
}
