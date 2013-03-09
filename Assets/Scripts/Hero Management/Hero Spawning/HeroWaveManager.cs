using UnityEngine;
using System.Collections;
using System.Xml;
using System;
using System.Collections.Generic;

public delegate void SpawnNewHeroHandler(WaveHero hero);

public delegate void NextWaveReadyHandler(Wave wave);
	
public delegate void WaveCompletedSpawningHandler(int currWaveNum, bool moreWaves, float requestedTimeToNextWave);

public delegate void AllWaveEnemiesDefeatedHandler(int currWaveNum);

public delegate void NextWaveStartedHandler(int currWaveNum);

/// <summary>
/// Handles the management of all spawns for an entire level.
/// </summary>
public class HeroWaveManager : MonoBehaviour
{	
	protected enum WaveState
	{
		Stopped,
		LaunchingWave,
		WaitingForEndOfWave,
		WaitingToStartWave,
		Completed
	}
	
	protected WaveState waveState;
	
	protected EntityFactory entityFactory;
	
	protected List<Wave> waves;
		
	protected int currWave;
	
	protected bool setupNextWave;
	
	/// <summary>
	/// An event that fires when a new Hero is ready to be spawned.
	/// </summary>
	public event SpawnNewHeroHandler onSpawnNewHero;
	
	/// <summary>
	/// An event that fires when the current wave has been completed.
	/// </summary>
	public event WaveCompletedSpawningHandler onWaveCompletedSpawning;
	
	/// <summary>
	/// An event that fires when the next wave is ready to be spawned.
	/// </summary>
	public event NextWaveReadyHandler onNextWaveReady;
	
	/// <summary>
	/// An event that fires when the next wave has started.
	/// </summary>
	public event NextWaveStartedHandler onNextWaveStarted;
	
	public event AllWaveEnemiesDefeatedHandler onAllWaveEnemiesDefeated;
	
	protected float timeSinceLastSpawn;
	
	protected float timeUntilNextSpawn;
		
	protected WaveHero nextHeroToSpawn;
	
	protected HeroPartyManager partyManager;
	
	protected LevelManager levelManager;
	
	protected Dictionary<string, HeroSpawner> spawners;
	
	protected WaveGUIManager waveGUIManager;
	
	/// <summary>
	/// Gets the total number of waves stored.
	/// </summary>
	public int NumWaves
	{
		get { return waves.Count; }
	}
	
	public int NumHeroesAlive
	{
		get { return partyManager.NumHeroesAlive; }
	}
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	void Awake()
	{		
		spawners = new Dictionary<string, HeroSpawner>();
		
		waves = new List<Wave>(8);
		currWave = 0;				//We're not on any wave yet!
		timeSinceLastSpawn = 0.0f;  //We haven't had anything spawn yet!
		timeUntilNextSpawn  = 0.0f; 
		
		//Locate all of the spawners and store them and their names into a Dictionary.
		GameObject[] spawnerObjects = GameObject.FindGameObjectsWithTag("Spawner");
		
		foreach (GameObject spawnerObject in spawnerObjects)
			spawners.Add(spawnerObject.name, spawnerObject.GetComponent<HeroSpawner>());	
		
		//Link up with the managers we need.
		waveGUIManager = GameObject.Find("WaveGUIManager").GetComponent<WaveGUIManager>();
		waveGUIManager.onStartWaveButtonPressed += HandleOnStartWaveButtonPressed;
		
		partyManager = GameObject.Find("PartyManager").GetComponent<HeroPartyManager>();
		
		entityFactory = EntityFactory.GetInstance();
		
		waveState = WaveState.WaitingToStartWave;
		setupNextWave = true;
	}

	void HandleOnStartWaveButtonPressed()
	{
		Debug.Log("Wave Manager received Start Wave Button Press");
		StartWave();
		
		if (onNextWaveStarted != null)
			onNextWaveStarted(currWave);
	}
	
	/// <summary>
	/// Updates the HeroWaveManager and decides if new heroes need to be spawned.
	/// </summary>
	public void Update()
	{	
		//If we're paused then just return and do nothing for now...
		if (Paused)
			return;
		
		//Don't do anything regarding spawning heroes unless we are actually launching a wave!
		if (waveState == WaveState.LaunchingWave)
		{
			timeSinceLastSpawn += Time.deltaTime;
			
			//If we have a hero to spawn and it's time to spawn them then fire the event!
			if (nextHeroToSpawn != null && timeSinceLastSpawn >= timeUntilNextSpawn)
			{
				SpawnNewHero(nextHeroToSpawn);
				nextHeroToSpawn = null;
				timeSinceLastSpawn = 0.0f;
			}
			
			//If we don't have a next hero to spawn then load one
			if (nextHeroToSpawn == null)
			{
				bool valid = LoadNextHero();
				
				//If we didn't get a valid hero back then fire the wave completed spawning event.
				if (!valid)
					OnWaveCompletedSpawning();
			}			
		}
		else if ((waveState == WaveState.WaitingForEndOfWave || waveState == WaveState.Completed) && NumHeroesAlive == 0)
		{
			waveState = WaveState.WaitingToStartWave;
			OnAllWaveEnemiesDefeated(currWave);
			setupNextWave = true;
		}
			
		if (setupNextWave && waveState == WaveState.WaitingToStartWave)
		{
			waveGUIManager.EnableStartWaveButton();	
			OnNextWaveReady(currWave);		
			setupNextWave = false;
		}
	}	
	
	protected void OnNextWaveReady(int waveNum)
	{
		if (onNextWaveReady != null)
			onNextWaveReady(waves[waveNum]);		
	}
	
	protected void OnAllWaveEnemiesDefeated(int waveNum)
	{
		if (onAllWaveEnemiesDefeated != null)
			onAllWaveEnemiesDefeated(waveNum);
	}
	
	protected void SpawnNewHero(WaveHero nextHeroToSpawn)
	{
		//Locate the correct spawner for the hero.
		HeroSpawner spawner = spawners[nextHeroToSpawn.spawnerName];
		
		//Have the Spawner spawn the hero
		spawner.SpawnHero(nextHeroToSpawn);		
	}
	
	/// <summary>
	/// Launches the next wave.
	/// </summary>
	/// <returns>
	/// True if the next wave was launched, false if there are no more waves.
	/// </returns>
	public bool StartWave()
	{
		timeSinceLastSpawn = 0.0f;
		
		if (waveState == WaveState.WaitingToStartWave)
			waveState = WaveState.LaunchingWave;
		
		waveGUIManager.DisableStartWaveButton();
		
		return waveState == WaveState.LaunchingWave;
	}
	
	/// <summary>
	/// Called when the current wave's hero spawns have been completed.
	/// </summary>
	protected void OnWaveCompletedSpawning()
	{
		currWave++;

		bool moreWaves = currWave < waves.Count;
		
		if (moreWaves)
			waveState = WaveState.WaitingForEndOfWave;
		else
			waveState = WaveState.Completed;
		
		float requestedDelay = moreWaves ? waves[currWave].StartDelay : -1f;
		
		if (onWaveCompletedSpawning != null)
			onWaveCompletedSpawning(currWave, moreWaves, requestedDelay);		
	}
	
	/// <summary>
	/// Loads the next hero in the current wave into nextHeroToSpawn.
	/// </summary>
	/// <returns>
	/// True if the next hero is valid, false if the end of the wave was passed.
	/// </returns>
	protected bool LoadNextHero()
	{
		nextHeroToSpawn = waves[currWave].GetNext();
		bool valid = false;
		
		if (nextHeroToSpawn.heroName != String.Empty)
		{
			timeUntilNextSpawn = nextHeroToSpawn.delay;
			
			valid = true;			
		}
		else
			nextHeroToSpawn = null;
		
		return valid;		
	}
	
	public void BuildWaves(XmlNodeList waveNodes)
	{			
		//Load the waves themselves.
		foreach (XmlNode waveNode in waveNodes)
		{
			//Load in any party definitions for this wave
			XmlNodeList partyNodes = waveNode.SelectNodes("./party");
			
			foreach (XmlNode partyNode in partyNodes)
				partyManager.CreateHeroParty(partyNode.Attributes["name"].Value);
			
			Wave wave = new Wave();
			wave.LoadWave(waveNode);
			waves.Add(wave);
			
			Debug.Log("Added a wave!");
		}		
	}
	
	public void Pause()
	{
		Paused = true;		
	}
	
	public void Unpause()
	{
		Paused = false;
	}
}
