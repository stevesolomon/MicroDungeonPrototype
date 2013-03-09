using UnityEngine;
using System.Collections;

public class MovingTextParticle : BaseParticle
{
	public float yScrollVelocity = 0.08f;
	
	public float xScrollVelocity = -0.025f;
	
	public float yDecayVelocity = 0.12f;
	
	public float duration = 1.5f;
	
	public Color color;
	
	public float alphaStart = 1f;
	
	public float alphaEnd = 0f;	
	
	protected float alpha;
	
	protected float timeAlive;
	
	protected SpriteText spriteText;
	
	public string Text
	{
		get { return spriteText.Text; }
		set { spriteText.Text = value; }
	}

	// Use this for initialization
	void Start () 
	{
		spriteText = GetComponent<SpriteText>();
		spriteText.Color = color;
		
		transform.Rotate(90f, 0f, 0f);
		
		alpha = alphaStart;
		timeAlive = 0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Paused)
			return;
		
		if (timeAlive < duration)
		{
			timeAlive += Time.deltaTime;
			
			Vector3 setPos = transform.position;
			yScrollVelocity -= yDecayVelocity * Time.deltaTime;
			setPos.z += yScrollVelocity * Time.deltaTime;
			setPos.x += xScrollVelocity * Time.deltaTime;
			transform.position = setPos;
			
			alpha -= Time.deltaTime / duration;
			Color setCol = spriteText.Color;
			setCol.a = alpha;
			spriteText.Color = setCol;
		}
		else
		{	
			OnParticleCompleted();
		}
	}
	
	public override void Pause()
	{
		if (!Paused)
			Paused = true;
	}
	
	public override void Unpause()
	{
		if (Paused)
			Paused = false;
	}
}
