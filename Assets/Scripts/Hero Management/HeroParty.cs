using UnityEngine;
using System.Collections;

public class HeroParty 
{
    protected HeroQueue heroQueue = new HeroQueue();

    protected WaypointManager waypointManager;
	
	public event OnEntityDeath OnHeroDied;
	
	public int Count
	{
		get { return heroQueue.Count; }
	}

    public HeroParty()
    {
        heroQueue.onLeaderChanged += LeaderChanged;
		heroQueue.onFollowerChanged += FollowerChanged;
        waypointManager = GameObject.Find("WaypointManager").GetComponent<WaypointManager>();
    }
	
	public HeroComponent GetLeader()
	{
		if (heroQueue.Count > 0)
			return heroQueue[0];
		
		return null;
	}
	
	/// <summary>
	/// Gets the HeroComponent that the given HeroComponent should be following.
	/// </summary>
	/// <param name="hero">
	/// The HeroComponent in question.
	/// </param>
	/// <returns>
	/// The HeroComponent to follow or null if the HeroComponent provided is a leader.
	/// </returns>
	public HeroComponent GetWhoIFollow(HeroComponent hero)
	{
		//If it's a leader then return null.
		HeroComponent toFollow = null;
		int index = heroQueue.FindNextHero(hero);
		
		if (index >= 0)
			toFollow = heroQueue[index].GetComponent<HeroComponent>();		
		
		return toFollow;
	}

    public bool AddPartyMember(HeroComponent newMember)
    {		
         bool added = heroQueue.AddHero(newMember);
		
		if (added)
		{
			newMember.myParty = this;
			
			newMember.myMovement.ReachedEndOfPathEvent += MemberReachedWaypoint;
			newMember.onEntityDeath += OnHeroDeath;
			
			waypointManager.AddNewEntity(newMember.gameObject);
		}
		
		return added;
    }

    public void RemovePartyMember(HeroComponent oldMember)
    {
		oldMember.myMovement.ReachedEndOfPathEvent -= MemberReachedWaypoint;
        heroQueue.RemoveHero(oldMember);
    }
	
	protected void OnHeroDeath(EntityComponent dead, EntityComponent lastAttacker)
	{
		if (dead is HeroComponent)
		{
			RemovePartyMember(dead as HeroComponent);
			dead.onEntityDeath -= OnHeroDeath;;
			
			if (OnHeroDied != null)
				OnHeroDied(dead, lastAttacker);
		}
	}
	
	public void FollowerChanged(HeroComponent follower, HeroComponent toFollow)
	{
		//HeroMovement heroMove = follower.GetComponent<HeroMovement>();
		
		//if (heroMove != null)
	//		heroMove.ChangeHeroToFollow(toFollow.GetComponent<HeroComponent>());
	}

    public void LeaderChanged(HeroComponent newLeader, HeroComponent oldLeader, HeroComponent firstFollower)
    {        		
		if (newLeader != null)
		{
			HeroComponent newLeaderComp = newLeader;
			HeroComponent oldLeaderComp = null;
			
			if (oldLeader != null)
			{				
				//Demote the old leader to a follower who follows the new leader if it is still alive for some reason.
				oldLeaderComp = oldLeader.GetComponent<HeroComponent>();
				
				if (oldLeaderComp.Alive)
				{
					oldLeaderComp.SendMessage("DemotedToFollower", newLeaderComp, SendMessageOptions.DontRequireReceiver);
				}
			}
			
			//Promote the new leader and set its waypoint accordingly.
			newLeaderComp.SendMessage("PromotedToLeader", oldLeaderComp, SendMessageOptions.DontRequireReceiver);
		}			
	}
		
	protected void HeroReachedWaypoint(EntityComponent hero, Waypoint waypoint)
	{
		waypointManager.WaypointReached(waypoint, hero.gameObject);
		Waypoint newWaypoint = waypointManager.GetWaypoint(hero.gameObject);
		
		//Let all the party members know to move to the new waypoint and have the waypoint manager 
		//update its value accordingly if and only if we received a positive response from the hero.
		foreach (HeroComponent heroComp in heroQueue)
		{
			bool changed = heroComp.ChangeWaypoint(newWaypoint);
			
			if (changed)
				waypointManager.SetCurrentWaypoint(heroComp.gameObject, newWaypoint);			
		}		
	}
	
	protected void MemberReachedWaypoint(EntityComponent member)
	{
		waypointManager.WaypointReached(waypointManager.GetWaypoint(member.gameObject), member.gameObject);
		
		//Let all the party members know to move to the new waypoint.	
		foreach (HeroComponent hero in heroQueue)
		{
			if (hero != member)
			{
				waypointManager.WaypointReached(waypointManager.GetWaypoint(hero.gameObject), hero.gameObject);
			
				//((HeroMovement) heroComp.myMovement).SetWaypoint(waypointManager.GetWaypoint(hero));
			}
		}	
	}
	
	protected void LeaderReachedWaypoint(EntityComponent leader)
	{
		waypointManager.WaypointReached(waypointManager.GetWaypoint(leader.gameObject), leader.gameObject);
	//	leader.myMovement.CurrTarget = waypointManager.GetWaypoint(leader.gameObject).transform;		
	}
}
