using UnityEngine;
using System.Collections;
using System.Xml;
using System.Globalization;

public class TimedMovementModifier : TimedEntityModifier
{
	public float moveSpeedReductionFactor;
	
	public override string MyType 
	{
		get 
		{
			return "MovementModifier";
		}
		protected set {}
	}
	
	public TimedMovementModifier() { }
	
	public TimedMovementModifier(TimedMovementModifier old)
		: base(old)
	{
		this.moveSpeedReductionFactor = old.moveSpeedReductionFactor;	
	}
		
	protected override void ModifyEntity()
	{
		if (target != null)
			target.myMovement.movementSpeed *= moveSpeedReductionFactor;	
	}
	
	protected override void ResetEntity()
	{
		if (target != null)
			target.myMovement.movementSpeed /= moveSpeedReductionFactor;
	}
	
	public override void LoadFromXml(XmlNode dataNode)
	{
		float moveValue = 1.0f;
		
		if (dataNode.SelectSingleNode("moveSpeedReductionFactor") != null)
		{
			float.TryParse(dataNode.SelectSingleNode("moveSpeedReductionFactor").InnerText, System.Globalization.NumberStyles.Float, 
						   CultureInfo.InvariantCulture, out moveValue);			
		}
		this.moveSpeedReductionFactor = moveValue;
		this.competitionValue = (float) (1f / moveSpeedReductionFactor);
		
		base.LoadFromXml(dataNode);
	}
	
	public override EntityModifier DeepClone()
	{
		return new TimedMovementModifier(this);
	}
}
