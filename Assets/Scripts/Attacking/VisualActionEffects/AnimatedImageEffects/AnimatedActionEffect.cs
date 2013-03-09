using UnityEngine;
using System.Collections;

public class AnimatedActionEffect : VisualActionEffect 
{
	public tk2dAnimatedSprite actionEffectAnimation;
	
	public int animationIndex;
	
	protected bool visible;
	
	public override bool Visible 
	{
		get { return visible; }
		set 
		{
			visible = value;
			actionEffectAnimation.renderer.enabled = visible;	
		} 
	}

	// Use this for initialization
	protected override void Start () 
	{
		if (actionEffectAnimation == null)
			actionEffectAnimation = GetComponent<tk2dAnimatedSprite>();
		
		actionEffectAnimation.animationCompleteDelegate = AnimationCompleteDelegate;	
		
		base.Start();
	}
	
	protected virtual void AnimationCompleteDelegate(tk2dAnimatedSprite sprite, int clipId)
	{
		playing = false;
		ActionEffectCompleted();
	}
	
	public override void Play()
	{
		actionEffectAnimation.Play(animationIndex);		
		playing = true;
	}
	
	public override void Pause()
	{
		if (!Paused)
		{
			Paused = true;
			
			actionEffectAnimation.Pause();
		}
	}
	
	public override void Unpause()
	{
		if (Paused)
		{
			Paused = false;
			
			actionEffectAnimation.Resume();
		}
	}
}
