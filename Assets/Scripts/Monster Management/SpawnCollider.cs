using UnityEngine;
using System.Collections;

public class SpawnCollider : MonoBehaviour 
{
	public bool InsideSpawn
	{
		get;
		protected set;
	}
	
	public bool insideThisFrame;
	
	void OnTriggerStay(Collider other)
	{
		if ((other.CompareTag("Monster") || other.CompareTag("Hero") || other.CompareTag("Boss")) && !other.isTrigger)
		{
			insideThisFrame = true;
		}
	}
	
	void FixedUpdate()
	{
		if (insideThisFrame)
		{
			InsideSpawn = true;
			insideThisFrame = false;
		}
		else
			InsideSpawn = false;		
	}
}
