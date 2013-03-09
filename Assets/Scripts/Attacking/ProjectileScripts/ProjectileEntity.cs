using UnityEngine;
using System.Collections;

public delegate void ProjectileHitTargetHandler(ProjectileEntity source, EntityComponent target);

public class ProjectileEntity : MonoBehaviour, IPauseable
{
	public EntityComponent Target
	{
		get;
		set;
	}
	
	public bool Paused
	{
		get;
		set;
	}
	
	public Vector2 FiringOffset
	{
		get;
		set;
	}
	
	public float speed = 10f;
		
	public float minDistanceToTarget = 5f;
		
	public event ProjectileHitTargetHandler OnProjectileHitTargetEvent;
		
	// Use this for initialization
	protected virtual void Start () 
	{
		FiringOffset = Vector2.zero;
	}
	
	// Update is called once per frame
	protected virtual void Update () 
	{
		if (Paused)
			return;
		
		//First figure out how far we are from the target (based on our entity's position).
		if (Target != null)
		{
		    Vector3 direction = Target.transform.position - transform.position;		
			Vector3 forwardDir;
			
			//Rotate towards the target
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 1f);
			transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
				
			//Set our forward movement
			forwardDir = transform.forward;
			forwardDir = forwardDir * speed;
			forwardDir *= Mathf.Clamp01(Vector3.Dot(direction.normalized, transform.forward));
			forwardDir = Vector3.ClampMagnitude(forwardDir, Vector3.Distance(Target.transform.position, transform.position));
						
			transform.position += forwardDir;	
	
			if(Mathf.Abs(Target.transform.position.x - transform.position.x) < minDistanceToTarget && 
			   Mathf.Abs(Target.transform.position.z - transform.position.z) < minDistanceToTarget)
			{
				TargetReached();
			}
		}
		else //We lost our target, destroy ourselves!
			Destroy(gameObject);
	}
	
	protected virtual void TargetReached()
	{
		if (OnProjectileHitTargetEvent != null)
			OnProjectileHitTargetEvent(this, Target);
		
		Destroy(this.gameObject);
	}
	
	public virtual void Pause()
	{
		if (!Paused)
		{
			Paused = true;
		}
	}
	
	public virtual void Unpause()
	{
		if (Paused)
		{
			Paused = false;
		}
	}
}
