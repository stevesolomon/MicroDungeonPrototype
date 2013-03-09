using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System;

public delegate void MonsterSelectedForSpawner(MonsterInformation monsterInfo);



/// <summary>
/// Manages the current state of the level. Can be queried for hero waves, available monsters, etc.
/// </summary>
public class LevelManager : MonoBehaviour 
{	
	public string levelPath = "LevelDefinitions/";
	
	public string levelName = "level1";
	
	public int startingGold;
	
	public XmlDocument xmlDoc;
		
	protected HeroWaveManager waveManager;
	
	protected HeroPartyManager partyManager;
	
	protected EntityFactory entityFactory;
	
	protected MonsterGUIManager monsterGUIManager;
	
	protected GameObject[] monsterSpawners;
	
	public PlayerStatusManager playerStatusManager;
	
	public int NumBossesAlive
	{
		get;
		protected set;
	}
	
	public HeroPartyManager PartyManager
	{
		get { return partyManager; }
	}
	
	public HeroWaveManager WaveManager
	{
		get { return waveManager; }
	}
		
	#region Initialization
	
	void Start ()
	{		
		LoadMonsters();
		LoadWaves();
		WatchBosses();
		
		monsterSpawners = GameObject.FindGameObjectsWithTag("MonsterSpawner");
		
		foreach (GameObject monsterSpawner in monsterSpawners)
			monsterSpawner.GetComponent<MonsterSpawner>().Stop();
		
		playerStatusManager.AddGold(startingGold);
	}
	
	void Awake () 
	{		
		TextAsset XMLTextAsset = (TextAsset) Resources.Load(levelPath + levelName);
		xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(XMLTextAsset.text);		
		
		startingGold = int.Parse(xmlDoc.SelectSingleNode("/level/startingGold").InnerText);
		
		entityFactory = EntityFactory.GetInstance();
		monsterGUIManager = GameObject.Find("MonsterGUIManager").GetComponent<MonsterGUIManager>();
		
		waveManager = GameObject.Find("WaveManager").GetComponent<HeroWaveManager>();
		
		if (playerStatusManager == null)
			playerStatusManager = GameObject.Find("PlayerStatusManager").GetComponent<PlayerStatusManager>();
				
		//waveManager.onSpawnNewHero += HandleOnSpawnNewHero;
		waveManager.onWaveCompletedSpawning += HandleOnWaveCompletedSpawning;	
		waveManager.onAllWaveEnemiesDefeated += HandleOnWaveEnemiesDefeated;	
		waveManager.onNextWaveStarted += HandleOnNextWaveStarted;		
	}

	void HandleOnNextWaveStarted(int currWaveNum)
	{
		foreach (GameObject monsterSpawner in monsterSpawners)
			monsterSpawner.GetComponent<MonsterSpawner>().Begin();
	}
	
	#endregion
	
	#region Boss Initialization Logic
	
	protected void WatchBosses()
	{
		//Find all of the GameObjects tagged as 'BossSpawner', these contain the locations for the bosses.
		GameObject[] bossSpawners = GameObject.FindGameObjectsWithTag("BossSpawner");	
		
		foreach (GameObject bossSpawner in bossSpawners)
		{
			BossSpawner spawner = bossSpawner.GetComponent<BossSpawner>();
			
			if (spawner != null)
			{
				spawner.OnBossSpawned += OnBossSpawned;
				spawner.OnBossDeath += OnBossDeath;
			}					
		}
	}
	
	#endregion
	
	
	#region General Game Logic
	
	protected void OnBossSpawned(EntityComponent boss)
	{
		NumBossesAlive++;		
	}
	
	protected void OnBossDeath(EntityComponent boss, EntityComponent lastAttacker)
	{
		NumBossesAlive--;
		
		if (NumBossesAlive == 0)
			StartGameOver();
	}
	
	protected void StartGameOver()
	{
		entityFactory.Reset();
		Application.LoadLevel("LevelScene");		
	}
	
	
	#endregion
	
	
	#region Wave Management
	
	bool moreWaves = true;
	
	void HandleOnWaveCompletedSpawning(int currWaveNum, bool moreWaves, float requestedTimeToNextWave)
	{
		Debug.Log("Wave completed");
		this.moreWaves = moreWaves;
		
		if (!moreWaves)
		{
			Debug.Log("No more waves detected.");
		}
		else
			Debug.Log("Waiting to start another wave...");
	}
	
	void HandleOnWaveEnemiesDefeated(int currWaveNum)
	{
		if (!moreWaves)
		{
			entityFactory.Reset();
			Application.LoadLevel("LevelScene");
			return;
		}		
		
		foreach (GameObject monsterSpawner in monsterSpawners)
			monsterSpawner.GetComponent<MonsterSpawner>().Stop();
		
		GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
		foreach (GameObject monster in monsters)
		{
			MonsterComponent monsterComp = monster.GetComponent<MonsterComponent>();
			
			if (monsterComp != null)
			{
				monsterComp.Damage(float.MaxValue, null);
			}
		}
		
	}	

	protected void LoadWaves()
	{
		if (xmlDoc != null)
		{
			XmlNodeList waveNodes = xmlDoc.SelectNodes("/level/waves/wave");			
			waveManager.BuildWaves(waveNodes);			
		}		
	}
	
	#endregion
	
	
	#region Monster Management
	
	public Dictionary<string, int> availableMonsters;
	
	/// <summary>
	/// Reads in the level file and loads the available monsters for this level.
	/// Feeds the available monsters to the MonsterManager and also keeps a record
	/// of the monster names of each monster available in this level.
	/// </summary>
	protected void LoadMonsters()
	{
		availableMonsters = new Dictionary<string, int>();
		
		if (xmlDoc != null)
		{
			XmlNode monsterNode = xmlDoc.SelectSingleNode("/level/monsters");
			
			foreach (XmlNode monster in monsterNode.SelectNodes("./monster"))
			{
				string monsterName = monster.Attributes["name"].InnerText;
				
				if (!availableMonsters.ContainsKey(monsterName))
					availableMonsters.Add(monsterName, 1);
				else
					availableMonsters[monsterName]++;
			}			
		}		
	}
		
	protected MonsterSelectedForSpawner spawnerCallback;
	
	protected bool guiOpen = false;
	
	/// <summary>
	/// Launches the monster GUI and returns the chosen monster to the last caller of this
	/// method at any given moment in time.
	/// </summary>
	/// <returns>
	/// The chosen monster.
	/// </returns>
	/// <param name='spawner'>
	/// The MonsterSpawner requesting that the user be allowed to choose a monster to spawn.
	/// </param>
	public void LaunchMonsterGUI1(MonsterSpawner spawner, MonsterSelectedForSpawner monsterChosenCallback)
	{
		//If we already have an active spawner set then deactivate it
		if (spawnerCallback != null)
			spawnerCallback(null);
			
		spawnerCallback = monsterChosenCallback;		
		
		if (!guiOpen)
		{
			StartCoroutine(monsterGUIManager.coLaunchGUI(spawner));
			guiOpen = true;
		}					
	}	
			
	public void OnMonsterSelected(string monsterName)
	{
		if (monsterName != null && monsterName != string.Empty)
		{
			Debug.Log("Monster " + monsterName + " was selected.");
			guiOpen = false;
			
			MonsterInformation info = entityFactory.GetMonsterInfo(monsterName);
			
			if (playerStatusManager.Gold >= info.goldCost)
			{	
				playerStatusManager.RemoveGold(info.goldCost);
				
				if (spawnerCallback != null)
					spawnerCallback(entityFactory.GetMonsterInfo(monsterName));
			}
			else
			{
				Debug.Log("Could not afford monster.");
				spawnerCallback(null);
			}					
		}
		else
		{
			spawnerCallback(null);
			Debug.Log("No monster was selected.");	
			guiOpen = false;
		}
		
		spawnerCallback = null;
	}
	
	#endregion
	
	// Update is called once per frame
	void Update () 
	{
		//waveManager.Update();
	}
}
