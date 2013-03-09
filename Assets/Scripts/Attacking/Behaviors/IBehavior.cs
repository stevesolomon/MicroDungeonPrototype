using UnityEngine;
using System.Collections;
using System.Xml;

public interface IBehavior
{
	BehaviorManager MyBehaviorManager { get; set; }
	
	BaseBehaviorStates State { get; }
	
	void ChangeState(BaseBehaviorStates state);
	
	void Update();
	
	void DeserializeProperties(XmlNode node);
}
