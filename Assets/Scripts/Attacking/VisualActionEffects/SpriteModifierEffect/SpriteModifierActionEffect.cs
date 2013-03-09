using UnityEngine;
using System.Collections;

public class SpriteModifierActionEffect : VisualActionEffect 
{
	public tk2dSprite targetSprite;
	
	public Color tint;
	
	protected Color originalTint;
	
	protected bool visible;
	
	public override bool Visible 
	{
		get { return visible; }
		set 
		{
			visible = value; 
		}
	}
	
	public override void Play ()
	{
		if (Target != null)
		{
			targetSprite = Target.parent.GetComponentInChildren<tk2dSprite>();
			
			if (targetSprite != null)
			{
				originalTint = targetSprite.color;
				targetSprite.color = tint;
			}
			else
				Debug.LogError("No sprite found to apply sprite modifier effect to!");
		}
		
		ActionEffectCompleted();
	}
	
	public override void Pause()
	{
		if (!Paused)
		{
			Paused = true;
		}
	}
	
	public override void Unpause()
	{
		if (Paused)
		{
			Paused = false;
		}
	}
}
