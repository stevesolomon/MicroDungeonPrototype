using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour 
{
	/// <summary>
	/// All of the monster spawners in the current level. We assume that these do not change from the 
	/// start of the level.
	/// </summary>
	protected GameObject[] monsterSpawners;
	
	/// <summary>
	/// The GUI manager responsible for handling monsters.
	/// </summary>
	protected MonsterGUIManager monsterGUIManager;
	
	/// <summary>
	/// The manager responsible for handling custom particles -- we need to pause them!
	/// </summary>
	protected ParticleManager particleManager;
	
	/// <summary>
	/// The GUI manager responsible for handling wave launches.
	/// </summary>
	protected WaveGUIManager waveGUIManager;
	
	/// <summary>
	/// A list of the monster spawners we have modified via a call to Pause.
	/// </summary>
	protected List<MonsterSpawner> pausedSpawners;
	
	/// <summary>
	/// The wave manager that we'll talk to for pausing hero wave spawning.
	/// </summary>
	protected HeroWaveManager waveManager;
	
	/// <summary>
	/// All of the current heroes in the level. Refreshed with every call to Pause.
	/// </summary>
	protected GameObject[] heroes;
	
	/// <summary>
	/// All of the current monsters in the level. Refreshed with every call to Pause.
	/// </summary>
	protected GameObject[] monsters;
	
	/// <summary>
	/// All of the current bosses in the level.
	/// </summary>
	protected GameObject[] bosses;
	
	protected tk2dSprite pauseSprite;
		
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="PauseManager"/> is paused.
	/// </summary>
	/// <value>
	/// <c>true</c> if paused; otherwise, <c>false</c>.
	/// </value>
	public bool Paused
	{
		get;
		protected set;
	}

	// Use this for initialization
	void Start () 
	{
		waveManager = GameObject.Find("WaveManager").GetComponent<HeroWaveManager>();
		waveGUIManager = GameObject.Find("WaveGUIManager").GetComponent<WaveGUIManager>();
		
		monsterSpawners = GameObject.FindGameObjectsWithTag("MonsterSpawner");
		
		particleManager = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
		
		pausedSpawners = new List<MonsterSpawner>(8);
		
		pauseSprite = GameObject.Find("PauseToggleButton").GetComponent<tk2dSprite>();
	}
	
	public void TogglePause()
	{
		if (Paused)
			Unpause();
		else
			Pause();		
	}
	
	public void Pause()
	{
		if (!Paused)
		{
			Color color = Color.grey;
			color.a = 150;
			Paused = true;
			pauseSprite.spriteId = pauseSprite.GetSpriteIdByName("PlayButton");
			pausedSpawners.Clear();
			
			//Pause all of the monster spawners.
			foreach (GameObject spawnerObject in monsterSpawners)
			{
				MonsterSpawner spawner = spawnerObject.GetComponent<MonsterSpawner>();
				
				if (spawner != null)
				{
					spawner.Pause();					
					pausedSpawners.Add(spawner);
				}
			}
			
			//Tell our managers to pause themselves.
			waveManager.Pause();
			waveGUIManager.Pause();			
			particleManager.Pause();
			
			//Tell all of the entities to pause themselves.
			heroes = GameObject.FindGameObjectsWithTag("Hero");
			monsters = GameObject.FindGameObjectsWithTag("Monster");
			bosses = GameObject.FindGameObjectsWithTag("Boss");
			
			foreach (GameObject hero in heroes)
			{
				EntityComponent entity = null;
				
				if (hero != null)
					entity = hero.GetComponent<EntityComponent>();
				
				if (entity != null)
					entity.Pause();				
			}
			
			foreach (GameObject monster in monsters)
			{
				EntityComponent entity = null;
				
				if (monster != null)
					entity = monster.GetComponent<EntityComponent>();
				
				if (entity != null)
					entity.Pause();				
			}
			
			foreach (GameObject boss in bosses)
			{
				EntityComponent entity = null;
				
				if (boss != null)
					entity = boss.GetComponent<EntityComponent>();
				
				if (entity != null)
					entity.Pause();				
			}
		}		
	}
	
	public void Unpause()
	{
		if (Paused)
		{
			Paused = false;
			pauseSprite.spriteId = pauseSprite.GetSpriteIdByName("PauseButton");
			
			//Unpause all of the paused monster spawners.
			foreach (MonsterSpawner spawner in pausedSpawners)
				spawner.Unpause();	
			
			//Tell our managers to unpause themselves
			waveManager.Unpause();
			waveGUIManager.Unpause();
			particleManager.Unpause();
						
			//For an unpause to even do anything it'll have to be followed by a Pause(), so 
			//any entities that were paused previously are still in their respective arrays. So let's run through 'em!
			foreach (GameObject hero in heroes)
			{
				EntityComponent entity = null;
				
				if (hero != null)
					entity = hero.GetComponent<EntityComponent>();
				
				if (entity != null)
					entity.Unpause();				
			}
			
			foreach (GameObject monster in monsters)
			{
				EntityComponent entity = null;
				
				if (monster != null)
					entity = monster.GetComponent<EntityComponent>();
				
				if (entity != null)
					entity.Unpause();				
			}
			
			foreach (GameObject boss in bosses)
			{
				EntityComponent entity = null;
				
				if (boss != null)
					entity = boss.GetComponent<EntityComponent>();
				
				if (entity != null)
					entity.Unpause();				
			}
		}
		
	}
}
