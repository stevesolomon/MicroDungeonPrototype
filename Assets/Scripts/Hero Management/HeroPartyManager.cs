using System.Collections.Generic;
using UnityEngine;

public class HeroPartyManager : MonoBehaviour
{
    protected Dictionary<string, HeroParty> heroParties;	
	
	public event OnEntityDeath onHeroDied;
	
	public int NumHeroesAlive
	{
		get;
		protected set;
	}
	 
	void Awake()
    {
	    heroParties = new Dictionary<string, HeroParty>();
    }

    /// <summary>
    /// Creates a new HeroParty with the given name.
    /// </summary>
    /// <param name="partyName">The (unique) name of the HeroParty to create.</param>
    /// <returns>True if the HeroParty was created, false if a HeroParty with the given name already exists.</returns>
    public bool CreateHeroParty(string partyName)
    {
        bool createdNewParty = false;

        if (!heroParties.ContainsKey(partyName))
        {
			HeroParty newParty = new HeroParty();
			newParty.OnHeroDied += HandleOnHeroDied;
            heroParties.Add(partyName, newParty);
            createdNewParty = true;

            Debug.Log("Created a new Hero Party named " + partyName);
        }

        return createdNewParty;
    }

    void HandleOnHeroDied(EntityComponent died, EntityComponent lastAttacker)
    {
		NumHeroesAlive--;
		
		if (onHeroDied != null)
			onHeroDied(died, lastAttacker);
    }

    /// <summary>
    /// Creates a new HeroParty with the given name and a leader already set.
    /// </summary>
    /// <param name="partyName">The (unique) name of the HeroParty to create.</param>
    /// <param name="leader">The Hero that will be the leader of this HeroParty.</param>
    /// <returns>True if the HeroParty was created, false if a HeroParty with the given name already exists.</returns>
    /// <remarks>If a HeroParty with the given name already exists, the GameObject 'leader' may not be assigned to the leader position.</remarks>
    public bool CreateHeroParty(string partyName, HeroComponent leader)
    {
        bool newParty = CreateHeroParty(partyName);
        heroParties[partyName].AddPartyMember(leader);
		
		NumHeroesAlive++;

        return newParty;
    }

    /// <summary>
    /// Creates a new HeroParty with the given name and a leader already set.
    /// </summary>
    /// <param name="partyName">The (unique) name of the HeroParty to create.</param>
    /// <param name="leader">The Hero that will be the leader of this HeroParty.</param>
    /// <param name="partyMembers">An array of Hero party members that does not include the leader.</param>
    /// <returns>True if the HeroParty was created, false if a HeroParty with the given name already exists.</returns>
    /// <remarks>If a HeroParty with the given name already exists, the GameObject 'leader' may not be assigned to the leader position.</remarks>
    public bool CreateHeroParty(string partyName, HeroComponent leader, HeroComponent[] partyMembers)
    {
        bool newParty = CreateHeroParty(partyName, leader);
		NumHeroesAlive++;

        for (int i = 0; i < partyMembers.Length; i++)
			AddHeroToParty(partyName, partyMembers[i]);

        return newParty;
    }

    /// <summary>
    /// Adds a new Hero to the given HeroParty.
    /// </summary>
    /// <param name="partyName">The name of the HeroParty to add a Hero to.</param>
    /// <param name="hero">The Hero to add to the HeroParty.</param>
    /// <returns>True if the Hero was added to the HeroParty, false if otherwise.</returns>
    public bool AddHeroToParty(string partyName, HeroComponent hero)
    {
        bool added = false;

        if (hero != null && heroParties.ContainsKey(partyName))
        {			
            heroParties[partyName].AddPartyMember(hero);
			NumHeroesAlive++;
            added = true;
        }

        return added;
    }
	
	/// <summary>
    /// Removes a Hero from the given HeroParty.
    /// </summary>
    /// <param name="partyName">The name of the HeroParty to remove a Hero from.</param>
    /// <param name="hero">The Hero to remove from the HeroParty.</param>
    /// <returns>True if the Hero was removed to the HeroParty, false if otherwise.</returns>
	public bool RemoveHeroFromParty(string partyName, HeroComponent hero)
	{
		bool removed = false;
		
		if (hero != null && heroParties.ContainsKey(partyName))
		{			
			heroParties[partyName].RemovePartyMember(hero);
			NumHeroesAlive--;
			removed = true;
		}
		
		return removed;
	}
	
	/// <summary>
	/// Gets the party closest to the given transform based on the location
	/// of the party leader.
	/// </summary>
	/// <param name="transform">The location we're querying from.</param>
	/// <returns>The closest HeroParty to the given transform, or null if there are no HeroParties.</returns>
	public HeroParty GetClosestParty(Transform transform)
	{
		HeroParty closest = null;
		float closestDistance = float.PositiveInfinity;
		
		foreach (HeroParty party in heroParties.Values)
		{
			HeroComponent curr = party.GetLeader();
			float currDistance = float.PositiveInfinity;
			
			if (curr != null)
				currDistance = Vector3.Distance(curr.transform.position, transform.position);
			
			if (currDistance < closestDistance)
				closest = party;
		}
		
		return closest;		
	}
}
