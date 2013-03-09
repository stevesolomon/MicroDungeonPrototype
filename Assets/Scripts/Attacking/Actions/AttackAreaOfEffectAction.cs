using UnityEngine;
using System.Collections;

public class AttackAreaOfEffectAction : AreaOfEffectAction
{
	public override void HandleOnTargetHit(ProjectileEntity source, EntityComponent target)
	{
		base.HandleOnTargetHit(source, target);
		
		Collider[] colliders = Physics.OverlapSphere(target.transform.position, Radius, layerMask);
		
		//Loop through all resulting colliders, seeing if we can snatch an EntityComponent from them that is of our target type
		//Then damage it if that is the case!
		foreach (Collider collider in colliders)
		{
			EntityComponent entity = collider.gameObject.GetComponent<EntityComponent>();
			
			if (entity != null && entity.GetType() == myTargetType)
			{
				entity.Damage(Magnitude, MyEntityComponent);		
				entity.BroadcastMessage("BeingAttacked", this.MyEntityComponent);				
			}			
		}		
	}
}
