using UnityEngine;
using System.Collections;
using System.Xml;

public interface IAction
{
	ActionManager MyActionManager { get; set; }
	
	/// <summary>
	/// An event that fires when this IAction hits a target.
	/// </summary>
	event OnTargetHitHandler OnTargetHitEvent;
	
	/// <summary>
	/// An event that fires when this IAction begins performing its Action.
	/// </summary>
	event OnPerformingActionHandler OnPerformingActionEvent;
	
	/// <summary>
	/// Gets or sets the range of this IAction.
	/// </summary>
	float Range { get; set; }
	
	/// <summary>
	/// Gets or sets the time between each IAction execution.
	/// </summary>
	float ActionSpeed { get; set; }
	
	/// <summary>
	/// Gets or sets the time this IAction takes to charge up - the time from initially being executed to the action itself being performed.
	/// </summary>
	float ChargeTime { get; set; }
	
	/// <summary>
	/// Gets or sets the (optional) projectile used for displaying this IAction.
	/// </summary>
	GameObject Projectile { get; set; }
	
	/// <summary>
	/// Gets whether or not this IAction is current executing.
	bool IsRunning { get; }
		
	/// <summary>
	/// Gets the EntityComponent this IAction works under.
	/// </summary>
	EntityComponent MyEntityComponent { get; }
		
	void FireAction(EntityComponent target);
	
	/// <summary>
	/// Deserializes the properties for this IAction from an XmlNode.
	/// </summary>
	/// <param name='node'>
	/// The XmlNode containing properties to deserialize
	/// </param>
	void DeserializeProperties(XmlNode node);
}
