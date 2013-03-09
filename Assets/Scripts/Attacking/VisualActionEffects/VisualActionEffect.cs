using UnityEngine;
using System.Collections;

public delegate void ActionEffectCompletedHandler();

public abstract class VisualActionEffect : MonoBehaviour, IPauseable
{		
	protected Transform target;
	
	public ActionEffectCompletedHandler OnActionEffectCompletedDelegate;
	
	protected bool playing;
	
	public abstract bool Visible
	{
		get;
		set;
	}
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	public virtual Transform Target
	{
		get { return target; }	
		set
		{
			if (value != null)
			{
				target = value;
				
				this.transform.position = target.position + new Vector3(0f, 10f, 0f);
			}
		}
	}
	
	public virtual Transform MyEntity
	{
		get;
		set;
	}
	
	protected virtual void Start () 
	{		
		this.Visible = false;
	}
	
	public abstract void Play();
	
	protected virtual void ActionEffectCompleted()
	{
		if (OnActionEffectCompletedDelegate != null)
		{
			OnActionEffectCompletedDelegate();
		}
	}
		
	protected virtual void Update () 
	{
		//Just play our attack effect animation at the position of the target.
		if (playing && target != null)
		{
			this.transform.position = target.position;			
		}
	}
	
	public abstract void Pause();
	
	public abstract void Unpause();
}
