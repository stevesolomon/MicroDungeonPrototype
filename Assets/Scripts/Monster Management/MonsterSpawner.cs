using UnityEngine;
using System.Collections;
using System;

public enum SpawnColliderType
{
	Ignore,
	SlowDown,
	Suppress
}

public class MonsterSpawner : MonoBehaviour, IPauseable
{
	public MonsterInformation monsterToSpawn;
	
	public Vector3 spawnLocation;
	
	public float timeToSpawn;
	
	/// <summary>
	/// The maximum number of spawned monsters from this monster spawned at any given 
	/// moment in time.
	/// </summary>
	public int maxSpawnedAtOnce;
	
	/// <summary>
	/// The total number of monsters spawned by this MonsterSpawner.
	/// </summary>
	public int totalSpawned;
	
	/// <summary>
	/// The total number of monsters spawned by this MonsterSpawner that are
	/// currently alive.
	/// </summary>
	public int totalAlive;
	
	/// <summary>
	/// Whether or not this spawner can currently spawn monsters.
	/// </summary>
	public bool canSpawn;	
	
	public float baseSpawnSpeedMultiplier = 1.0f;
	
	protected float currentSpawnSpeedMultiplier;
	
	public float supressSpawnSpeed = 0.5f;
	
	/// <summary>
	/// Whether or not this spawner can have a monster built on it.
	/// </summary>
	public bool canBuild;
	
	/// <summary>
	/// Whether or not this spawner can sell the monster currently located in it.
	/// </summary>
	public bool canSell;
	
	public SpawnColliderType spawnColliderType = SpawnColliderType.Suppress;
	
	protected float timeSinceLastSpawn;
	
	protected SpawnCollider spawnCollider;
	
	protected LevelManager levelManager;
	
	protected EntityFactory entityFactory;
	
	public PlayerStatusManager playerStatusManager;
	
	protected Camera viewCamera;
	
	protected Collider selectionCollider;
	
	protected tk2dSprite sprite;
	
	protected tk2dSprite dungeonSprite;
	
	protected tk2dSprite[] monsterIndicators;
	
	public MonsterGUIManager monsterGUIManager;
	
	public MonsterInformation MonsterToSpawn
	{
		get { return monsterToSpawn; }
		set
		{
			if (value != monsterToSpawn)
			{
				monsterToSpawn = value;
				timeToSpawn = 5f;				
			}			
		}
	}	
	
	public float TimeUntilNextSpawn
	{
		get { return timeToSpawn - timeSinceLastSpawn; }		
	}
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	public bool Stopped
	{
		get;
		protected set;
	}
		
	void Start()
	{
		if (spawnLocation == Vector3.zero)
			spawnLocation = transform.FindChild("SpawnLocation").transform.position;
		
		if (monsterGUIManager == null)
			monsterGUIManager = GameObject.Find("MonsterGUIManager").GetComponent<MonsterGUIManager>();
		
		if (playerStatusManager == null)
			playerStatusManager = GameObject.Find("PlayerStatusManager").GetComponent<PlayerStatusManager>();
		
		currentSpawnSpeedMultiplier = baseSpawnSpeedMultiplier;
		
		monsterIndicators = new tk2dSprite[5];
		
		monsterIndicators[0] = transform.FindChild("monsterIndicators").FindChild("1").GetComponent<tk2dSprite>();
		monsterIndicators[1] = transform.FindChild("monsterIndicators").FindChild("2").GetComponent<tk2dSprite>();
		monsterIndicators[2] = transform.FindChild("monsterIndicators").FindChild("3").GetComponent<tk2dSprite>();
		monsterIndicators[3] = transform.FindChild("monsterIndicators").FindChild("4").GetComponent<tk2dSprite>();
		monsterIndicators[4] = transform.FindChild("monsterIndicators").FindChild("5").GetComponent<tk2dSprite>();		
		
		levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
		
		entityFactory = EntityFactory.GetInstance();
		
		viewCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		
		selectionCollider = transform.FindChild("SelectCollider").GetComponent<Collider>();
		
		spawnCollider = transform.FindChild("SpawnCollider").GetComponent<SpawnCollider>();
		totalSpawned = 0;
		totalAlive = 0;
		
		sprite = transform.FindChild("MonsterSprite").GetComponent<tk2dSprite>();
		dungeonSprite = GetComponent<tk2dSprite>();
		
		canSell = false;
	}
	
	protected void CheckSpawnConditions()
	{
		currentSpawnSpeedMultiplier = baseSpawnSpeedMultiplier;
		canSpawn = true;
		
		if (totalAlive >= maxSpawnedAtOnce)
		{
			canSpawn = false;
			return;
		}
		
		if (spawnColliderType == SpawnColliderType.SlowDown && spawnCollider.InsideSpawn)
		{
			currentSpawnSpeedMultiplier = supressSpawnSpeed;
			return;
		}
		else if (spawnColliderType == SpawnColliderType.Suppress && spawnCollider.InsideSpawn)
		{
			canSpawn = false;
			return;
		}	
	}
		
	// Update is called once per frame
	void Update () 
	{
		bool savedCanSpawn = canSpawn;
		
		CheckSpawnConditions();
				
		if (!Paused)
		{
			if (!Stopped && canSpawn && monsterToSpawn != null)
			{
				timeSinceLastSpawn += Time.deltaTime * currentSpawnSpeedMultiplier;
				
				if (timeSinceLastSpawn > timeToSpawn)
				{			
					SpawnMonster();
					timeSinceLastSpawn  = 0.0f;	
				}
			}
			
			if (canBuild || canSell)
			{
				StartCoroutine(coHandleBuilding());
			}
		}
		
		canSpawn = savedCanSpawn;
	}
	
	protected IEnumerator coHandleBuilding()
	{
		Vector3 location = Vector3.zero;
		bool inputExists = false;
		bool inputHit = false;
		
		if (Paused)
			yield return null;
			
		if (canBuild || canSell)
		{
			if (Input.GetMouseButtonDown(0))
			{
				location = Input.mousePosition;
				inputExists = true;
			}
			else //Check for touches
			{
				foreach (Touch touch in Input.touches)
				{
					if (touch.phase == TouchPhase.Began)
					{
						location = touch.position;
						inputExists = true;
						break;
					}
				}
			}
			
			//If we had some input then cast a ray to see if we're hit!
			if (inputExists)
			{
				Ray ray = viewCamera.ScreenPointToRay(location);
	            RaycastHit hitInfo;
				inputHit = selectionCollider.Raycast(ray, out hitInfo, 1.0e8f);
			}
				
			if (inputHit)
			{
				Debug.Log("Input captured on Spawner.");
				
				ShowSelect();
				
				//If we have nothing in here then display the choice dialogue, otherwise 
				//if this spawner can be sold then prompt with the sell dialogue!.				
				if (monsterToSpawn == null)
				{					
					monsterGUIManager.LaunchMonsterGUI(this, OnMonsterChosen);
				}
				else if (canSell)
				{
					monsterGUIManager.LaunchSellMonsterGUI(this, OnMonsterSaleResults);
				}
			}
		}
		
		yield return null;		
	}
	
	protected void PopulateIndicators()
	{
		int index = 1;
		foreach (tk2dSprite indicatorSprite in monsterIndicators)
		{
			if (MonsterToSpawn == null || index > MonsterToSpawn.maxAlive)
			{
				indicatorSprite.spriteId = indicatorSprite.GetSpriteIdByName("blank");
			}
			else
			{
				indicatorSprite.spriteId = indicatorSprite.GetSpriteIdByName("MonsterAvailableIndicator");
			}		
			
			index++;
		}		
	}
	
	protected void UpdateIndicators()
	{
		int index = 1;
		foreach (tk2dSprite indicatorSprite in monsterIndicators)
		{
			if (MonsterToSpawn == null || index > MonsterToSpawn.maxAlive)
			{
				indicatorSprite.spriteId = indicatorSprite.GetSpriteIdByName("blank");
			}
			else if (index > totalAlive)
			{
				indicatorSprite.spriteId = indicatorSprite.GetSpriteIdByName("MonsterAvailableIndicator");
			}
			else
			{
				indicatorSprite.spriteId = indicatorSprite.GetSpriteIdByName("MonsterSpawnedIndicator");
			}		
			
			index++;
		}		
	}
	
	protected void OnMonsterSaleResults(bool sold)
	{
		HideSelect();
		
		if (sold)
		{
			playerStatusManager.AddGold((int) (MonsterToSpawn.goldCost * 0.5f));
			this.MonsterToSpawn = null;
			this.sprite.spriteId = sprite.GetSpriteIdByName("blank");
			
			this.totalAlive = 0;
			//this.totalSpawned = 0;
			
			this.canBuild = true;
			this.canSell = false;
			this.canSpawn = false;
			this.timeSinceLastSpawn = 0f;
			
			PopulateIndicators();
		}		
	}
	
	protected void OnMonsterChosen(MonsterInformation info)
	{
		HideSelect();
		
		if (info == null)
		{
			return;
		}	
		
		monsterToSpawn = info;
		
		this.canBuild = false;
		this.canSpawn = true;
		this.canSell = true;
		this.maxSpawnedAtOnce = info.maxAlive;
		this.timeToSpawn = info.spawnTime;
		
		sprite.spriteId = sprite.GetSpriteIdByName(info.UISpriteName);
		sprite.color = Color.grey;	
		
		PopulateIndicators();
	}	
	
	protected void OnMonsterDeath(EntityComponent monster, EntityComponent lastAttacker)
	{
		//Unhook ourselves from the event
		monster.onEntityDeath -= OnMonsterDeath;
		totalAlive--;		
		
		UpdateIndicators();
	}
	
	protected void SpawnMonster()
	{			
		Debug.Log("Spawning new monster: " + monsterToSpawn);
		GameObject newMonster = entityFactory.GenerateMonster(monsterToSpawn.dataName);

		newMonster.transform.position = spawnLocation;
				
		EntityComponent component = newMonster.GetComponent<EntityComponent>();
		component.onEntityDeath += OnMonsterDeath;		
		
		newMonster.SetActiveRecursively(true);
		
		totalAlive++;
		totalSpawned++;
		
		UpdateIndicators();
	}
	
	protected void ShowSelect()
	{
		dungeonSprite.color = Color.yellow;		
	}
	
	protected void HideSelect()
	{
		dungeonSprite.color = Color.white;		
	}
	
	public void Pause()
	{
		Paused = true;	
	}
	
	public void Unpause()
	{
		Paused = false;		
	}
	
	public void Begin()
	{
		Stopped = false;		
	}
	
	public void Stop()
	{		
		Stopped = true;
		timeSinceLastSpawn = 0.0f;
	}
}
