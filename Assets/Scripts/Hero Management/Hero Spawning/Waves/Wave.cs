using UnityEngine;
using System.Collections;
using System.Xml;
using System;
using System.Globalization;
using System.Collections.Generic;

public class WaveHero
{
	/// <summary>
	/// The name of the hero object to be created.
	/// </summary>
	public string heroName;
	
	/// <summary>
	/// The name of the party this hero will belong to.
	/// </summary>
	public string partyName;
	
	/// <summary>
	/// The name of the spawner that this WaveHero will spawn at.
	/// </summary>T
	public string spawnerName;
	
	/// <summary>
	/// The delay from the last hero spawn until the hero is created.
	/// </summary>
	public float delay;
	
	/// <summary>
	/// The information on this hero's stats and other relevant data.
	/// </summary>
	public HeroInformation heroInfo;
	
	public WaveHero(string heroName, string partyName, string spawnerName, float delay)
	{
		this.heroName = heroName;
		this.partyName = partyName;
		this.spawnerName = spawnerName;
		this.delay = delay;		
		
		heroInfo = new HeroInformation();
	}
}

/// <summary>
/// Contains the definitions for a single Wave of hero units.
/// Each Wave consists of a number of units, with an amount of time between each
/// representing how much time to wait before spawning them.
/// </summary>
public class Wave
{
	protected List<WaveHero> waveHeroes;
	
	protected int currHero;
	
	protected EntityFactory entityFactory;
	
	/// <summary>
	/// The time until the Wave starts.
	/// </summary>
	protected float startDelay;
	
	/// <summary>
	/// Gets a list containing all heroes in the wave.
	/// </summary>
	public List<WaveHero> WaveHeroes
	{
		get { return waveHeroes; }
	}
	
	/// <summary>
	/// Gets the number of heroes in this Wave.
	/// </summary>
	public int NumHeroes
	{
		get { return waveHeroes.Count; }
	}
	
	public bool WaveCompleted
	{
		get { return currHero >= NumHeroes; }
	}
	
	public float StartDelay
	{
		get { return startDelay; }
	}
	
	public Wave()
	{
		entityFactory = EntityFactory.GetInstance();
		waveHeroes = new List<WaveHero>(8);
		currHero = 0;
	}
	
	/// <summary>
	/// Gets the next Hero in this Wave to be spawned,
	/// </summary>
	/// <returns>
	/// The name of the Hero and time delay information for the next Hero to be spawned for this Wave.
	/// If the Wave is completed the WaveHero returned contains the empty string.
	/// </returns>
	public WaveHero GetNext()
	{
		WaveHero hero;
		
		if (currHero < NumHeroes)
		{
			hero = waveHeroes[currHero];
			currHero++;			
		}
		else
			hero = new WaveHero(String.Empty, String.Empty, String.Empty, -1.0f);
		
		return hero;		
	}	
	
	/// <summary>
	/// Loads the Wave based on the information in the provided XML Node.
	/// </summary>
	/// <param name="xmlNode">
	/// The XML Node containing wave definitions.
	/// </param>
	public void LoadWave(XmlNode xmlNode)
	{
		startDelay = 0f;
		
		if (xmlNode.Attributes["delay"] != null)
			float.TryParse(xmlNode.Attributes["delay"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out startDelay);
		
		if (xmlNode.Name.Equals("wave", StringComparison.InvariantCultureIgnoreCase))
		{
			XmlNodeList nodes = xmlNode.SelectNodes("./hero");
			
			foreach (XmlNode node in nodes)
				AddWaveHero(node);			
		}		
	}
	
	/// <summary>
	/// Adds a hero to the wave following the definition in the provided XmlNode.
	/// </summary>
	/// <param name="node">
	/// The XmlNode containing the definition of the hero to be added to the wave.
	/// </param>
	protected void AddWaveHero(XmlNode node)
	{
		string type = node.Attributes["type"].Value;
		string partyName = node.Attributes["party"].Value;
		string spawnerName = node.Attributes["spawner"].Value;
		float delay = float.Parse(node["delay"].InnerText, NumberStyles.Float, CultureInfo.InvariantCulture);
		
		WaveHero waveHero = new WaveHero(type, partyName, spawnerName, delay);
		waveHero.heroInfo = entityFactory.GetHeroInfo(type);
		
		waveHeroes.Add(waveHero);		
	}
}
