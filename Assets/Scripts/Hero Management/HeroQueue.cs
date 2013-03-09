using System.Collections.Generic;
using UnityEngine;

public delegate void LeaderChanged(HeroComponent newLeader, HeroComponent oldLeader, HeroComponent second);

public delegate void FollowerChanged(HeroComponent follower, HeroComponent toFollow);

public class HeroQueue 
{
    /// <summary>
    /// A List of all the Heroes currently in this Queue.
    /// </summary>
    protected List<HeroComponent> heroes;

    public event LeaderChanged onLeaderChanged;
	
	public event FollowerChanged onFollowerChanged;

    public HeroQueue()
    {
        heroes = new List<HeroComponent>(16);
    }
	
	public IEnumerator<HeroComponent> GetEnumerator()
	{
		return heroes.GetEnumerator();
	}
	
	

    public HeroComponent Leader
    {
        get
        {
            if (heroes.Count > 0)
                return heroes[0];

             return null;
         }
    }
	
	public int Count
	{
		get { return heroes.Count; }
	}
	
	public HeroComponent this[int index]
	{
		get
		{
			if (heroes.Count > index)
				return heroes[index];
			
			return null;
		}		
	}
    
	public int FindNextHero(HeroComponent hero)
	{
		int index = -1;
		
		for (int i = 0; i < heroes.Count; i++)
		{
			if (heroes[i] == hero)
			{
				index = i - 1;
				break;
			}
		}			
			
		return index;		
	}
	
    /// <summary>
    /// Adds the given Hero to the end of the HeroQueue. 
    /// </summary>
    /// <param name="hero">The Hero to be added to this HeroQueue.</param>
    /// <returns>True if the Hero was added, false if otherwise.</returns>
    /// <remarks>A Hero must have a HeroComponent attached to its GameObject</remarks>
    public bool AddHero(HeroComponent hero)
    {
        bool added = false;

        if (!heroes.Contains(hero) && hero != null)
        {
            heroes.Add(hero);
            added = true;

            if (heroes.Count == 1 && onLeaderChanged != null) //We have a new leader by default, fire the event!
            {
                onLeaderChanged(heroes[0], null, null);
                Debug.Log("New party leader has been set!");
            }
			else //A new follower, set its target to be the next Hero in line...
				onFollowerChanged(heroes[heroes.Count - 1], heroes[heroes.Count - 2]);
        }
		

        return added;
    }

    public void RemoveHero(HeroComponent hero)
    {		
		int removedIndex = -1;
		
		for (int i = 0; i < heroes.Count; i++)
		{
			if (heroes[i] == hero)
			{
				removedIndex = i;
				break;
			}
		}
		
		//Remove the Hero from the Queue, and be sure we point the person directly behind in the right direction.
		if (removedIndex < heroes.Count && removedIndex >= 0)
		{			
			if (removedIndex == 0) //We will remove the leader so the next follower is now the leader!
			{				
				if (heroes.Count > 1) //We haven't actually removed the old leader yet, so ensure that  we have 2 units + in the queue.
				{
					HeroComponent firstFollower = heroes.Count > 2 ? heroes[2] : null;
					onLeaderChanged(heroes[1], hero, firstFollower);
				}
			}
			else //We just removed a follower
			{
				if (heroes.Count > removedIndex + 1) //This follower has a follower, so redirect it!
				{
					onFollowerChanged(heroes[removedIndex + 1], heroes[removedIndex - 1]);					
				}				
			}	
			heroes.RemoveAt(removedIndex);
		}
    }
}
