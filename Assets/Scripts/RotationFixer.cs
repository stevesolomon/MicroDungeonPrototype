using UnityEngine;
using System.Collections;

public class RotationFixer : MonoBehaviour 
{
	protected Transform parentTransform;
	
	public float xLockedRotation = 90f;
	
	public float yLockedRotation = 0f;
	
	public float zLockedRotation = 0f;

	// Use this for initialization
	void Start () 
	{
		parentTransform = transform.parent.transform;
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		/*Vector3 parentRotation = parentTransform.eulerAngles;
		Vector3 myRotation = Vector3.zero;
		
		myRotation.x = 90 - parentRotation.x;	
		myRotation.y = -parentRotation.y;
		myRotation.z = -parentRotation.z;
		
		transform.eulerAngles = myRotation;*/
		
		transform.rotation = Quaternion.Euler(xLockedRotation, yLockedRotation, zLockedRotation);
	}
}
