using UnityEngine;
using System.Collections;

public class ParticleActionEffect : VisualActionEffect 
{	
	public int particleCount = 10;
	
	protected float waitTime;
	
	protected float timeRemaining;
	
	protected bool visible;
	
	public override bool Visible 
	{
		get { return visible; }
		set 
		{
			visible = value;
			particleSystem.renderer.enabled = visible;			
		}
	}
	
	public override Transform Target
	{
		get { return target; }	
		set
		{
			if (value != null)
			{
				target = value;
				this.transform.position = target.position;
			}
		}
	}
	
	// Use this for initialization
	protected override void Start () 
	{	
		playing = false;
	}
	
	public override void Play()
	{
		particleSystem.Emit(particleCount);	
		waitTime = particleSystem.duration;
		timeRemaining = waitTime;
		playing = true;
	}
	
	protected override void Update ()
	{		
		if (Paused)
			return;
		
		if (playing)
		{	
			if (target != null)
				this.transform.position = target.position + new Vector3(0f, 16f, 0f);
			
			timeRemaining -= Time.deltaTime;
			
			if (timeRemaining <= 0f)
			{
				ActionEffectCompleted();
				playing = false;
			}
		}
	}
	
	public override void Pause()
	{
		if (!Paused)
		{
			Paused = true;
			
			particleSystem.Pause();
		}
	}
	
	public override void Unpause()
	{
		if (Paused)
		{
			Paused = false;
			
			particleSystem.Play();
		}
	}
}
