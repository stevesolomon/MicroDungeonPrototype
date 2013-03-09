using UnityEngine;

public delegate void OnReachedWaypoint(HeroComponent hero, Waypoint waypoint);

public class HeroComponent : EntityComponent
{   
	public HeroParty myParty;	
	
	public event OnReachedWaypoint OnReachedWaypointEvent;
	
	protected override void Start()
	{
		base.Start ();
	}
	
	public int CurrExperience
	{
		get;
		protected set;
	}
	
	public void AddExperience(int experience)
	{
		if (experience > 0)
			CurrExperience += experience;		
		
		Debug.Log(this + " was awarded " + experience + " experience points.");
	}
	
	public virtual bool ChangeWaypoint(Waypoint waypoint)
	{
		((HeroMovement) this.myMovement).SetWaypoint(waypoint);		
		
		return true;		
	}
}
