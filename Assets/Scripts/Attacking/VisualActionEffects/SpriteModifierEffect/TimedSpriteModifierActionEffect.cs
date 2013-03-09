using UnityEngine;
using System.Collections;

public class TimedSpriteModifierActionEffect : SpriteModifierActionEffect
{
	public float time;	
	
	protected bool spriteSet = false;
	
	protected IEnumerator coWaitToUntint(float timeRemaining)
	{
		float timeStep = 0.05f;
		
		while (timeRemaining > 0)
		{				
			float currTimeStep = timeRemaining < timeStep ? timeRemaining : timeStep;
			
			if (!Paused)
				timeRemaining -= timeStep;
			
			yield return new WaitForSeconds(currTimeStep);
		}
		
		ResetSprite();
	}
		
	protected void ResetSprite()
	{
		if (spriteSet && targetSprite != null)
			targetSprite.color = originalTint;		
	}

	public override void Play()
	{
		ResetSprite();
		base.Play();
		
		if (targetSprite != null)
		{
			spriteSet = true;
		}	
		
		StopAllCoroutines();		
		StartCoroutine("coWaitToUntint", time);
	}
	
}
