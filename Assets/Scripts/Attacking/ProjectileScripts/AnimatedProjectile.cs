using UnityEngine;
using System.Collections;

public class AnimatedProjectile : ProjectileEntity 
{
	public tk2dAnimatedSprite mySprite;
	
	public string animationName;

	// Use this for initialization
	protected void Awake () 
	{
		if (mySprite == null)
		{
			mySprite = GetComponent<tk2dAnimatedSprite>();				
		}
		
		mySprite.Play(animationName);
	}
	
	//void OnDestroy()
	//{
	//	if (mySprite != null && mySprite.anim != null)
	//		Destroy(mySprite.anim.gameObject);
	//}
	
	public override void Pause ()
	{
		if (!Paused && mySprite != null)
			mySprite.Pause();
		
		base.Pause ();
	}
	
	public override void Unpause ()
	{
		if (Paused && mySprite != null)
			mySprite.Resume();
		
		base.Unpause ();
	}
}
