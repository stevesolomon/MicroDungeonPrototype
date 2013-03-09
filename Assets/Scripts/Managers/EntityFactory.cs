using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;
using System;

public class ActionEffectInfo
{
	/// <summary>
	/// The name of the attack effect to use for this entity.
	/// </summary>
	public string chargingActionEffectName;
	
	public string startingActionEffectName;
	
	public string hitActionEffectName;
	
	public ActionEffectInfo()
	{
		chargingActionEffectName = String.Empty;
		startingActionEffectName = String.Empty;
		hitActionEffectName = String.Empty;
	}
}

public class AudioInfo
{
	public Dictionary<string, string> audioClipNames;
	
	public AudioInfo()
	{
		audioClipNames = new Dictionary<string, string>();
	}
}

public class AnimationInfo
{
	public Dictionary<string, int> animationIndices;
	
	public AnimationInfo()
	{
		animationIndices = new Dictionary<string, int>();
	}
}

public class ProjectileInfo
{
	public string projectileName;	
	
	public float speed;
	
	public float minDistanceToTarget;
	
	public Vector2 projectileOffset;
	
	public ProjectileInfo()
	{
		speed = 10f;
		minDistanceToTarget = -1f;
		projectileOffset = Vector2.zero;
	}
}

public class AnimatedProjectileInfo : ProjectileInfo
{
	public string animationName;	
}

public class BaseInfo
{
	/// <summary>
	/// The game-safe name of the entity. 
	/// </summary>
	public string dataName;
	
	/// <summary>
	/// The in-game name used for this entity.
	/// </summary>
	public string gameName;
	
	/// <summary>
	/// The name of the game resource to use.
	/// </summary>
	public string prefabName;
		
	/// <summary>
	/// The name of the sprite used for the UI for this entity. 
	/// </summary>
	public string UISpriteName;	
	
	/// <summary>
	/// The name of the animated sprite used in-game.
	/// </summary>
	public string gameSpriteName;
	
	/// <summary>
	/// The attack effect info for this entity.
	/// </summary>
	public ActionEffectInfo attackEffectInfo;
	
	public List<string> attackModifierIcons;
	
	/// <summary>
	/// The animation info for this entity, describing the animations used under various circumstances.
	/// </summary>
	public AnimationInfo animationInfo;
	
	public AudioInfo audioInfo;
	
	/// <summary>
	/// Whether or not this entity has to successfully raycast to an enemy to target it.
	/// </summary>
	public bool raycastTargeting;
		
	/// <summary>
	/// The maximum health of this entity. 
	/// </summary>
	public float health;
		
	/// <summary>
	/// The amount of time between attacks.
	/// </summary>
	public float timeBetweenAttacks;
	
	/// <summary>
	/// The amount of time taken to charge an attack (before the attack connects or the projectile is fired).
	/// </summary>
	public float attackChargeTime;
		
	/// <summary>
	/// How much damage this entity causes when it attacks. 
	/// </summary>
	public float attackDamage;
	
	/// <summary>
	/// The attack range of the entity in world units.
	/// </summary>
	public float attackRange;
			
	/// <summary>
	/// The type of attack this entity attacks with. 
	/// </summary>
	public AttackType attackType;
		
	/// <summary>
	/// The speed at which this entity moves. 
	/// </summary>
	public float movementSpeed;
	
	public ProjectileInfo projectileInfo;
	
	public BaseInfo()
	{
		attackEffectInfo = new ActionEffectInfo();
		animationInfo = new AnimationInfo();
		audioInfo = new AudioInfo();
		
		attackModifierIcons = new List<string>();
		
		projectileInfo = null;
	}
}

/// <summary>
/// Helper struct for storing monster information. 
/// </summary>
public class MonsterInfo : BaseInfo
{
	/// <summary>
	/// The time in between each spawn of this monster.
	/// </summary>
	public float spawnTime;
	
	/// <summary>
	/// The amount of gold required to build this monster.
	/// </summary>
	public int goldCost;
	
	/// <summary>
	/// The maximum number of this monster that can be alive from a single spawner at any given moment in time.
	/// </summary>
	public int maxAlive;
	
	/// <summary>
	/// The amount of experience points gained by killing this monster.
	/// </summary>
	public int experience;
	
	public MonsterInfo()
	{
		this.prefabName = "monster";
	}
}

/// <summary>
/// Helper struct for storing boss information.
/// </summary>
public class BossInfo : MonsterInfo
{
	/// <summary>
	/// The range at which a boss notices an enemy and wakes up.
	/// </summary>
	public float attackNoticeRange;
	
	public BossInfo()
	{
		this.prefabName = "boss";
	}	
}

/// <summary>
/// Helper struct for storing hero information. 
/// </summary>
public class HeroInfo : BaseInfo
{	
	public int gold;
	
	public HeroInfo()
	{
		this.prefabName = "hero";
	}
}


public class ActionInformation
{
	public float magnitude;
	
	public float actionSpeed;
	
	public float actionRange;
		
	public List<string> actionModifierSpriteNames;
	
	public ActionInformation()
	{
		actionModifierSpriteNames = new List<string>();
	}
}

public class HeroInformation : BaseInformation
{
	public int gold;
}

public class MonsterInformation : BaseInformation
{	
	/// <summary>
	/// The time in between each spawn of this monster.
	/// </summary>
	public float spawnTime;
	
	/// <summary>
	/// The amount of gold required to build this monster.
	/// </summary>
	public int goldCost;
	
	/// <summary>
	/// The maximum number of this monster that can be alive from a single spawner at any given moment in time.
	/// </summary>
	public int maxAlive;
	
	/// <summary>
	/// The amount of experience points gained by killing this monster.
	/// </summary>
	public int experience;
}

public abstract class BaseInformation
{
	/// <summary>
	/// The name of the sprite used for UI-related materials.
	/// </summary>
	public string UISpriteName;
	
	public float maxHealth;
	
	public float moveSpeed;
	
	public string gameName;
	
	public string dataName;
	
	public ActionInformation actionInformation;	
	
	public BaseInformation()
	{
		actionInformation = new ActionInformation();
	}
}

public class EntityFactory
{
	private static EntityFactory instance;
		
	protected Dictionary<string, GameObject> cachedMonsters;
	
	protected Dictionary<string, GameObject> cachedHeroes;
	
	protected Dictionary<string, GameObject> cachedBosses;
	
	protected Dictionary<string, GameObject> cachedAttackEffectObjects;
		
	protected Dictionary<string, MonsterInfo> cachedMonsterInfo;
	
	protected Dictionary<string, HeroInfo> cachedHeroInfo;
	
	protected Dictionary<string, BossInfo> cachedBossInfo;
	
	protected Dictionary<string, MonsterInformation> cachedMonsterInformation;
	
	protected Dictionary<string, HeroInformation> cachedHeroInformation;	
	
	protected Dictionary<string, GameObject> cachedEntityPrefabs;
	
	protected XmlDocument monsterXmlDoc;
	
	protected XmlDocument heroXmlDoc;
	
	protected XmlDocument bossXmlDoc;
	
	protected const string MonsterXMLPath = "EntityDefinitions/monsters";
	
	protected const string HeroXMLPath = "EntityDefinitions/heroes";
	
	protected const string BossXMLPath = "EntityDefinitions/bosses";
	
	private EntityFactory()
	{
		cachedMonsters = new Dictionary<string, GameObject>(8);
		cachedHeroes = new Dictionary<string, GameObject>(8);
		cachedBosses = new Dictionary<string, GameObject>(4);
		cachedAttackEffectObjects = new Dictionary<string, GameObject>(16);
		
		cachedHeroInformation = new Dictionary<string, HeroInformation>();
		cachedMonsterInformation = new Dictionary<string, MonsterInformation>();
		
		cachedEntityPrefabs = new Dictionary<string, GameObject>(16);
		
		cachedMonsterInfo = new Dictionary<string, MonsterInfo>(8);
		cachedHeroInfo = new Dictionary<string, HeroInfo>(8);
		cachedBossInfo = new Dictionary<string, BossInfo>(4);
		
		TextAsset XMLTextAsset = (TextAsset)Resources.Load(MonsterXMLPath);
		monsterXmlDoc = new XmlDocument ();
		monsterXmlDoc.LoadXml (XMLTextAsset.text);
		
		XMLTextAsset = (TextAsset)Resources.Load(HeroXMLPath);
		heroXmlDoc = new XmlDocument ();
		heroXmlDoc.LoadXml (XMLTextAsset.text);
		
		XMLTextAsset = (TextAsset)Resources.Load(BossXMLPath);
		bossXmlDoc = new XmlDocument ();
		bossXmlDoc.LoadXml (XMLTextAsset.text);
	}
	
	public void Reset()
	{
		cachedMonsters = new Dictionary<string, GameObject>(8);
		cachedHeroes = new Dictionary<string, GameObject>(8);
		cachedBosses = new Dictionary<string, GameObject>(4);
		cachedAttackEffectObjects = new Dictionary<string, GameObject>(16);
		
		cachedEntityPrefabs = new Dictionary<string, GameObject>();
		
		cachedMonsterInfo = new Dictionary<string, MonsterInfo>(8);
		cachedHeroInfo = new Dictionary<string, HeroInfo>(8);
		cachedBossInfo = new Dictionary<string, BossInfo>(4);
	}
	
	public static EntityFactory GetInstance()
	{		
		if (instance == null)
			instance = new EntityFactory();
		
		return instance;
	}	
	
	public GameObject GenerateMonster(string monsterName)
	{
		GameObject monster = null;
		
		if (monsterXmlDoc != null)
		{
			string prefabName = "monster";
			XmlNode node = monsterXmlDoc.SelectSingleNode("/monsters/monster[@name=\'" + monsterName + "\']");
			
			if (!cachedEntityPrefabs.ContainsKey(prefabName))
				LoadPrefab(prefabName);
			
			monster = GameObject.Instantiate(cachedEntityPrefabs[prefabName]) as GameObject;
			
			InitializeEntity(monster, node, monsterName);
		}
		
		return monster;
	}
	
	public GameObject GenerateHero(string heroName)
	{
		GameObject hero = null;
		
		if (heroXmlDoc != null)
		{
			string prefabName = "hero";
			XmlNode node = heroXmlDoc.SelectSingleNode("/heroes/hero[@name=\'" + heroName + "\']");
			
			if (!cachedEntityPrefabs.ContainsKey(prefabName))
				LoadPrefab(prefabName);
			
			hero = GameObject.Instantiate(cachedEntityPrefabs[prefabName]) as GameObject;
			
			InitializeEntity(hero, node, heroName);
		}
		
		return hero;	
	}
	
	public GameObject GenerateBoss(string bossName)
	{
		GameObject boss = null;
		
		if (bossXmlDoc != null)
		{
			string prefabName = "boss";
			XmlNode node = bossXmlDoc.SelectSingleNode("/bosses/boss[@name=\'" + bossName + "\']");
			
			if (!cachedEntityPrefabs.ContainsKey(prefabName))
				LoadPrefab(prefabName);
			
			boss = GameObject.Instantiate(cachedEntityPrefabs[prefabName]) as GameObject;
			
			InitializeEntity(boss, node, bossName);
		}
		
		return boss;	
	}
	
	/// <summary>
	/// Loads the prefab with the given name into the cached entities dictionary.
	/// </summary>
	/// <param name='prefabName'>
	/// The name of the prefab to load into the cache.
	/// </param>
	protected void LoadPrefab(string prefabName)
	{
		GameObject prefabMonster = Resources.Load(prefabName) as GameObject;
		
		cachedEntityPrefabs.Add(prefabName, prefabMonster);
	}
	
	#region Retrieving Information 
	
	public HeroInformation GetHeroInfo(string heroName)
	{
		if (!cachedHeroInformation.ContainsKey(heroName))	
			LoadHeroInfo(heroName);
		
		return cachedHeroInformation[heroName];		
	}
	
	public MonsterInformation GetMonsterInfo(string monsterName)
	{
		if (!cachedMonsterInformation.ContainsKey(monsterName))	
			LoadMonsterInfo(monsterName);
		
		return cachedMonsterInformation[monsterName];		
	}
	
	protected void LoadMonsterInfo(string monsterName)
	{
		MonsterInformation info = new MonsterInformation();
		
		if (monsterXmlDoc != null)
		{
			string UIsprite, gameName, dataName;
			float spawnTime, maxHealth, moveSpeed;
			int goldCost, maxAlive, experience;
			
			XmlNode node = monsterXmlDoc.SelectSingleNode("/monsters/monster[@name=\'" + monsterName + "\']");
			
			UIsprite = node.SelectSingleNode("./UISprite").InnerText;
			gameName = node.SelectSingleNode("./gameName").InnerText;
			dataName = monsterName;
			
			spawnTime = float.Parse(node.SelectSingleNode("./stats").SelectSingleNode("./spawnTime").InnerText);
			goldCost = int.Parse(node.SelectSingleNode("./goldCost").InnerText);
			maxAlive = int.Parse(node.SelectSingleNode("./stats").SelectSingleNode("./maxAlive").InnerText);
			experience = int.Parse(node.SelectSingleNode("./experience").InnerText);	
			maxHealth = float.Parse(node.SelectSingleNode("./stats").SelectSingleNode("./health").InnerText);
			moveSpeed = float.Parse(node.SelectSingleNode("./stats").SelectSingleNode("./moveSpeed").InnerText);
			
			info.UISpriteName = UIsprite;
			info.spawnTime = spawnTime;
			info.goldCost = goldCost;
			info.maxAlive = maxAlive;
			info.maxHealth = maxHealth;
			info.moveSpeed = moveSpeed;
			info.gameName = gameName;
			info.dataName = dataName;
			
			LoadActionInfo(info.actionInformation, node.SelectSingleNode("./action"));
			LoadActionModifierInfo(info.actionInformation, node.SelectSingleNode("./action").SelectSingleNode("./actionModifiers"));
		}
		
		cachedMonsterInformation.Add(monsterName, info);		
	}
	
	protected void LoadHeroInfo(string heroName)
	{
		HeroInformation info = new HeroInformation();
		
		if (heroXmlDoc != null)
		{
			string UIsprite, gameName, dataName;
			float maxHealth, moveSpeed;
			int gold;
						
			XmlNode node = heroXmlDoc.SelectSingleNode("/heroes/hero[@name=\'" + heroName + "\']");
			
			UIsprite = node.SelectSingleNode("./UISprite").InnerText;
			gameName = node.SelectSingleNode("./gameName").InnerText;
			dataName = heroName;
			
			maxHealth = float.Parse(node.SelectSingleNode("./stats").SelectSingleNode("./health").InnerText);
			moveSpeed = float.Parse(node.SelectSingleNode("./stats").SelectSingleNode("./moveSpeed").InnerText);
			gold = int.Parse(node.SelectSingleNode("./gold").InnerText);
			
			info.UISpriteName = UIsprite;
			info.maxHealth = maxHealth;
			info.moveSpeed = moveSpeed;
			info.gameName = gameName;
			info.dataName = dataName;
			info.gold = gold;
		}
		
		cachedHeroInformation.Add(heroName, info);		
	}
	
	protected void LoadActionInfo(ActionInformation actionInfo, XmlNode actionNode)
	{
		actionInfo.actionRange = float.Parse(actionNode.SelectSingleNode("./range").InnerText);
		actionInfo.actionSpeed = float.Parse(actionNode.SelectSingleNode("./speed").InnerText);
		actionInfo.magnitude = float.Parse(actionNode.SelectSingleNode("./magnitude").InnerText);
		
		LoadActionModifierInfo(actionInfo, actionNode.SelectSingleNode("./actionModifiers"));
	}
	
	protected void LoadActionModifierInfo(ActionInformation actionInfo, XmlNode modifiersNode)
	{
		if (modifiersNode != null)
		{
			foreach (XmlNode node in modifiersNode.SelectNodes("./icons/icon"))
			{
				actionInfo.actionModifierSpriteNames.Add(node.InnerText);	
			}			
		}		
	}
	
	#endregion
		
	
	#region Loading Info from XML
	
	
	/*
	
	public MonsterInfo GetMonsterInfo(string monsterName)
	{
		if (!cachedMonsterInfo.ContainsKey(monsterName))	
			LoadMonsterInfo(monsterName);
		
		return cachedMonsterInfo[monsterName];		
	}
	
	public HeroInfo GetHeroInfo(string heroName)
	{
		if (!cachedHeroInfo.ContainsKey(heroName))	
			LoadHeroInfo(heroName);
		
		return cachedHeroInfo[heroName];		
	}
	
	public BossInfo GetBossInfo(string bossName)
	{
		if (!cachedBossInfo.ContainsKey(bossName))	
			LoadBossInfo(bossName);
		
		return cachedBossInfo[bossName];			
	}
	
	*/
	
	
	#region Internal Loading
	
	/*

	protected XmlNode GetMonsterNode(string monsterName)
	{
		return monsterXmlDoc.SelectSingleNode("/monsters/monster[@name=\'" + monsterName + "\']");			
	}
	
	protected XmlNode GetHeroNode(string heroName)
	{
		return heroXmlDoc.SelectSingleNode("/heroes/hero[@name=\'" + heroName + "\']");			
	}
	
	protected XmlNode GetBossNode(string bossName)
	{
		return bossXmlDoc.SelectSingleNode("/bosses/boss[@name=\'" + bossName + "\']");			
	}
	
	protected void LoadHeroInfo(string heroName)
	{
		HeroInfo heroInfo = new HeroInfo();
			
		if (heroXmlDoc != null) 
		{
			XmlNode heroNode = heroXmlDoc.SelectSingleNode ("/heroes/hero[@name=\'" + heroName + "\']");
			
			int gold = 0;
			
			if (heroNode.SelectSingleNode("./gold") != null)
				int.TryParse(heroNode.SelectSingleNode("./gold").InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out gold);
			
			heroInfo.gold = gold;
			
			LoadSharedInfo(heroInfo, heroNode, heroName);
			
			cachedHeroInfo.Add(heroName, heroInfo);
		} 
		else
			Debug.LogError ("Hero XML Document not loaded.");
	}
	
	protected void LoadMonsterInfo(string monsterName)
	{
		MonsterInfo monsterInfo = new MonsterInfo ();
			
		if (monsterXmlDoc != null) 
		{
			XmlNode monsterNode = monsterXmlDoc.SelectSingleNode ("/monsters/monster[@name=\'" + monsterName + "\']");
			
			LoadSharedInfo(monsterInfo, monsterNode, monsterName);
			
			int goldCost;
			int experience = 0;
			int.TryParse(monsterNode.SelectSingleNode("./goldCost").InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out goldCost);
			
			if (monsterNode.SelectSingleNode("./experience") != null)
				int.TryParse(monsterNode.SelectSingleNode("./experience").InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out experience);
			
			monsterInfo.goldCost = goldCost;
			monsterInfo.experience = experience;
			
			XmlNode statsNode = monsterNode.SelectSingleNode("./stats");
			
			if (statsNode != null)
			{
				float spawnTime;
				int maxAlive;
				
				float.TryParse (statsNode.SelectSingleNode ("./spawnTime").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out spawnTime);	
				int.TryParse (statsNode.SelectSingleNode ("./maxAlive").InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out maxAlive);
				
				monsterInfo.spawnTime = spawnTime;
				monsterInfo.maxAlive = maxAlive;				
			}
			
			cachedMonsterInfo.Add(monsterName, monsterInfo);
		} 
		else
			Debug.LogError ("Monster XML Document not loaded.");
	}
	
	protected void LoadBossInfo(string bossName)
	{
		BossInfo bossInfo = new BossInfo();
			
		if (bossXmlDoc != null) 
		{
			XmlNode monsterNode = bossXmlDoc.SelectSingleNode("/bosses/boss[@name=\'" + bossName + "\']");
			
			LoadSharedInfo(bossInfo, monsterNode, bossName);
			
			XmlNode statsNode = monsterNode.SelectSingleNode("./stats");
			
			if (statsNode != null)
			{
				float spawnTime, attackNoticeRange;
				int maxAlive;
				
				float.TryParse(statsNode.SelectSingleNode ("./spawnTime").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out spawnTime);	
				float.TryParse(statsNode.SelectSingleNode("./attackNoticeRadius").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out attackNoticeRange);
				int.TryParse(statsNode.SelectSingleNode ("./maxAlive").InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out maxAlive);
				
				bossInfo.spawnTime = spawnTime;
				bossInfo.maxAlive = maxAlive;				
				bossInfo.attackNoticeRange = attackNoticeRange;
			}
			
			cachedBossInfo.Add(bossName, bossInfo);
		} 
		else
			Debug.LogError ("Monster XML Document not loaded.");		
	}
	
	protected void LoadSharedInfo(BaseInfo info, XmlNode infoNode, string name)
	{
		if (infoNode != null) 
		{
			info.UISpriteName = infoNode.SelectSingleNode ("./UISprite").InnerText;
			
			info.gameName = name;
			info.dataName = name;
					
			if (infoNode.SelectSingleNode("./gameName") != null)
			{
				info.gameName = infoNode.SelectSingleNode ("./gameName").InnerText;
			}
			
			if (infoNode.SelectSingleNode("./gameSprite") != null)
			{
				info.gameSpriteName = infoNode.SelectSingleNode("./gameSprite").InnerText;
			}			
				
			XmlNode statsNode = infoNode.SelectSingleNode ("./stats");
				
			if (statsNode != null) 
			{
				float health, attackSpeed, attackDamage, attackRange, movementSpeed, chargeTime;
				AttackType attackType;
				bool raycastTargeting;
					
				float.TryParse (statsNode.SelectSingleNode ("./health").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out health);				
				float.TryParse (statsNode.SelectSingleNode ("./movementSpeed").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out movementSpeed);
					
				info.health = health;
				info.movementSpeed = movementSpeed;
			}		
			
			XmlNode attackEffectNode = infoNode.SelectSingleNode("./attackEffects");
			
			if (attackEffectNode != null)
				LoadAttackEffectInfo(attackEffectNode, info);	
			
			XmlNode animationNode = infoNode.SelectSingleNode("./animations");
			
			if (animationNode != null)
				LoadAnimationInfo(animationNode, info);
			
			XmlNode projectileNode = infoNode.SelectSingleNode("./projectile");
			
			if (projectileNode != null)
				LoadProjectileInfo(projectileNode, info);
			
			XmlNode soundNode = infoNode.SelectSingleNode("./audio");
			
			if (soundNode != null)
				LoadSoundEffectInfo(soundNode, info);
			
			XmlNode attackModifierIconNode = infoNode.SelectSingleNode("./attackModifiers/icons");
			
			if (attackModifierIconNode != null)
				LoadAttackModifierIconInfo(attackModifierIconNode, info);
		}
		else
			Debug.LogError("No info node retrieved for entity name " + name);		
	}
	
	protected void LoadAttackModifierIconInfo(XmlNode attackModifierIconNode, BaseInfo info)
	{
		XmlNodeList iconNodes = attackModifierIconNode.SelectNodes("./icon");
		
		for (int i = 0; i < iconNodes.Count; i++)
		{
			info.attackModifierIcons.Add(iconNodes[i].InnerText);			
		}	
	}
	
	protected void LoadAnimationInfo(XmlNode animationNode, BaseInfo info)
	{
		XmlNodeList list = animationNode.SelectNodes("./animation");
		AnimationInfo animInfo = info.animationInfo;
		
		foreach (XmlNode animNode in list)
		{
			string name = animNode.Attributes["name"].Value;
			int index = 0;
			
			int.TryParse(animNode.InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out index);
			
			if (!animInfo.animationIndices.ContainsKey(name))
				animInfo.animationIndices.Add(name, index);			
		}	
	}
	
	protected void LoadAttackEffectInfo(XmlNode attackEffectNode, BaseInfo info)
	{
		if (attackEffectNode != null)
		{
			if (attackEffectNode.SelectSingleNode("./charge") != null)
				info.attackEffectInfo.chargingActionEffectName = attackEffectNode.SelectSingleNode("./charge").InnerText;
			
			if (attackEffectNode.SelectSingleNode("./attack") != null)
				info.attackEffectInfo.startingActionEffectName = attackEffectNode.SelectSingleNode("./attack").InnerText;
			
			if (attackEffectNode.SelectSingleNode("./attackHit") != null)
				info.attackEffectInfo.hitActionEffectName = attackEffectNode.SelectSingleNode("./attackHit").InnerText;
		}		
	}
	
	protected void LoadProjectileInfo(XmlNode projectileNode, BaseInfo info)
	{
		if (projectileNode != null)
		{
			string type = projectileNode.Attributes["type"].Value;
			
			switch (type)
			{
				case "animated":
					AnimatedProjectileInfo projInfo = new AnimatedProjectileInfo();
					info.projectileInfo = projInfo;
				
					if (projectileNode.SelectSingleNode("./animation") != null)
						projInfo.animationName = projectileNode.SelectSingleNode("./animation").InnerText;
					break;
			}			
			
			if (projectileNode.SelectSingleNode("./name") != null)
				info.projectileInfo.projectileName = projectileNode.SelectSingleNode("./name").InnerText;
			
			float speed = 10f;
			if (projectileNode.SelectSingleNode("./speed") != null)
				float.TryParse(projectileNode.SelectSingleNode("./speed").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out speed);
			info.projectileInfo.speed = speed;
			
			float minDist = 5f;
			if (projectileNode.SelectSingleNode("./minDistanceToTarget") != null)
				float.TryParse(projectileNode.SelectSingleNode("./minDistanceToTarget").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out minDist);
			info.projectileInfo.minDistanceToTarget = minDist;		
			
			if (projectileNode.SelectSingleNode("./offset") != null)
			{
				float x = 0.0f, y = 0.0f;
				
				float.TryParse(projectileNode.SelectSingleNode("./offset").Attributes["x"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
				float.TryParse(projectileNode.SelectSingleNode("./offset").Attributes["y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
				
				info.projectileInfo.projectileOffset = new Vector2(x, y);
			}
		}		
	}
	
	protected void LoadSoundEffectInfo(XmlNode soundNode, BaseInfo info)
	{
		AudioInfo audioInfo = info.audioInfo;
		
		if (soundNode != null)
		{
			XmlNodeList soundNodes = soundNode.SelectNodes("./effect");
			
			foreach (XmlNode node in soundNodes)
			{
				string name = node.Attributes["name"].Value;
				string resource = node.InnerText;				
				
				if (!audioInfo.audioClipNames.ContainsKey(name))
					audioInfo.audioClipNames.Add(name, resource);			
			}				
		}		
	}
	*/
	
	#endregion
	
	
	#endregion
	
		
	#region Initializing Instantiated Entities	
	
	protected void InitializeEntity(GameObject entity, XmlNode entityNode, string name)
	{
		EntityComponent entityComponent = entity.GetComponent<EntityComponent>();
		
		XmlNode behaviorNode = entityNode.SelectSingleNode("./behavior");
		XmlNode actionNode = entityNode.SelectSingleNode("./action");
		XmlNode statsNode = entityNode.SelectSingleNode("./stats");
		
		entityComponent.DataName = name;
		
		entityComponent.Health = int.Parse(statsNode.SelectSingleNode("./health").InnerText);
		entityComponent.MaxHealth = entityComponent.Health;
		entityComponent.MoveSpeed = int.Parse(statsNode.SelectSingleNode("./moveSpeed").InnerText);
		
		InitializeSprite(entity, entityNode.SelectSingleNode("./gameSprite").InnerText);	
		InitializeSpriteAnimations(entity, entityNode.SelectSingleNode("./animations"));
		
		//Let's load up the Behaviors, Actions, and Targeting!
		InitializeBehavior(entity, behaviorNode);
		InitializeAction(entity, actionNode);
		InitializeTargeting(entity, entityNode);
		
		//Finally let's initialize the audio!
		//InitializeAudioSettings(...);
	}
	
	protected void InitializeBehavior(GameObject entity, XmlNode behaviorNode)
	{
		GameObject actionObject = entity.transform.FindChild("Attack").gameObject;
		BehaviorManager manager = actionObject.GetComponent<BehaviorManager>();
				
		Type behaviorType = Type.GetType(behaviorNode.Attributes["type"].Value);
		IBehavior behavior = Activator.CreateInstance(behaviorType) as IBehavior;	
		
		behavior.DeserializeProperties(behaviorNode);
		
		manager.MyBehavior = behavior;
	}
	
	protected void InitializeAction(GameObject entity, XmlNode actionNode)
	{
		GameObject actionObject = entity.transform.FindChild("Attack").gameObject;
		ActionManager manager = actionObject.GetComponent<ActionManager>();
		
		Type actionType = Type.GetType(actionNode.Attributes["type"].Value);
		IAction action = Activator.CreateInstance(actionType) as IAction;
		action.MyActionManager = manager;
		
		action.DeserializeProperties(actionNode);
		
		manager.MyAction = action;	
		
		InitializeActionEffects(entity, actionNode.SelectSingleNode("./actionEffects"));
	}
	
	protected void InitializeTargeting(GameObject entity, XmlNode entityNode)
	{
		bool useLOS = false;
		GameObject actionObject = entity.transform.FindChild("Attack").gameObject;
		TargetingManager manager = actionObject.GetComponent<TargetingManager>();
		
		XmlNode LOSNode = entityNode.SelectSingleNode("./stats").SelectSingleNode("./lineOfSightTargeting");
		
		if (LOSNode != null)
		{
			useLOS = bool.Parse(LOSNode.InnerText);	
		}
		
		manager.LineOfSightTargeting = useLOS;
	}
	
	protected void InitializeActionEffects(GameObject entity, XmlNode actionEffectsNode)
	{
		VisualActionEffectManager manager = entity.transform.FindChild("Attack").GetComponent<VisualActionEffectManager>();
		
		if (actionEffectsNode != null)
		{
			if (actionEffectsNode.SelectSingleNode("./chargeAction") != null)
			{
				string name = actionEffectsNode.SelectSingleNode("./chargeAction").InnerText;
				manager.chargingActionEffect = GameObject.Instantiate(Resources.Load("AttackEffects/" + name)) as GameObject;
			}
			
			if (actionEffectsNode.SelectSingleNode("./performAction") != null)
			{
				string name = actionEffectsNode.SelectSingleNode("./performAction").InnerText;
				manager.startedActionEffect = GameObject.Instantiate(Resources.Load("AttackEffects/" + name)) as GameObject;
			}
			
			if (actionEffectsNode.SelectSingleNode("./hitAction") != null)
			{
				string name = actionEffectsNode.SelectSingleNode("./hitAction").InnerText;
				manager.hitActionEffect = GameObject.Instantiate(Resources.Load("AttackEffects/" + name)) as GameObject;
			}
		}				
	}
	
	
	#region Loading Inner-Entity Objects (Sprites, Projectiles, Effects, etc.)
	
	protected void InitializeSprite(GameObject entity, string spriteName)
	{
		//entity.transform.FindChild("Properties").FindChild("Sprite").gameObject.AddComponent<tk2dAnimatedSprite>();
		//entity.transform.FindChild("Properties").FindChild("Sprite").gameObject.AddComponent<EntityAnimator>();
		tk2dAnimatedSprite sprite = entity.transform.FindChild("Properties").FindChild("Sprite").GetComponent<tk2dAnimatedSprite>();
		sprite.anim = ((GameObject) GameObject.Instantiate(Resources.Load("Animations/" + spriteName))).GetComponent<tk2dSpriteAnimation>();
		sprite.clipId = 0;		
	}
	
	protected void InitializeSpriteAnimations(GameObject entity, XmlNode animationNode)
	{
		Dictionary<string, int> animations = new Dictionary<string, int>(4);
		
		foreach (XmlNode node in animationNode.SelectNodes("./animation"))
		{
			string name;
			int index;
			
			name = node.Attributes["name"].Value;
			index = int.Parse(node.InnerText);
			
			animations.Add(name, index);
		}
		
		EntityAnimator animator = entity.transform.FindChild("Properties").FindChild("Sprite").GetComponent<EntityAnimator>();
		animator.AnimationInfo = animations;		
	}
	
	protected void InitializeAudioSettings(GameObject entity, AudioInfo info)
	{
		if (info != null)
		{
			AttackManager attackManager = entity.transform.FindChild("Attack").GetComponent<AttackManager>();
		
			if (attackManager != null)
			{
				attackManager.audioInfo = info;	
			}			
		}		
	}		
	
	#endregion
	
	
	#region Loading and Instantiating Projectiles
	
	public static GameObject GenerateProjectile(XmlNode node)
	{
		GameObject projectile = null;
		
		if (node != null)
		{
			string type = string.Empty;
			string name = string.Empty;
			
			if (node.Attributes["type"] != null)
				type = node.Attributes["type"].Value;
			
			if (node.SelectSingleNode("./name") != null)
				name = node.SelectSingleNode("./name").InnerText;			
			
			if (type.ToLowerInvariant().Equals("animated"))
			{
				projectile = InitializeAnimatedProjectile(name, node);							
			}
			
			SetProjectileSettings(projectile.GetComponent<ProjectileEntity>(), node);				
		}
		
		return projectile;
	}
	
	public static Dictionary<string, EntityModifier> LoadActionModifiers(XmlNode actionModifiersNode)
	{
		Dictionary<string, EntityModifier> modifiers = new Dictionary<string, EntityModifier>();
		
		if (actionModifiersNode != null)
		{			
			XmlNodeList modifierNodes = actionModifiersNode.SelectNodes("./modifier");
				
			foreach (XmlNode modifierNode in modifierNodes)
			{
				EntityModifier modifier = LoadModifier(modifierNode);
				modifiers.Add(modifier.MyType, modifier);
			}
		}
		
		return modifiers;
	}
	
	protected static EntityModifier LoadModifier(XmlNode modifierNode)
	{
		string typeName = modifierNode.Attributes["type"].Value;
		
		Type type = Type.GetType(typeName);
		
		EntityModifier modifier = Activator.CreateInstance(type) as EntityModifier;
		modifier.LoadFromXml(modifierNode);		
		
		return modifier;
	}
	
	protected static GameObject InitializeAnimatedProjectile(string name, XmlNode node)
	{
		string animationName = string.Empty;
		
		if (node.SelectSingleNode("./animation") != null)
			animationName = node.SelectSingleNode("./animation").InnerText;
		
		GameObject proj =  GameObject.Instantiate(Resources.Load("Projectiles/" + name)) as GameObject;	
		proj.SetActiveRecursively(false);
		AnimatedProjectile projScript = proj.GetComponent<AnimatedProjectile>();
		
		projScript.mySprite.anim = ((GameObject) GameObject.Instantiate(Resources.Load("Animations/ProjectileAnimations"))).GetComponent<tk2dSpriteAnimation>();
		projScript.animationName = animationName;
		//projScript.mySprite.Play(animationName);		
		
		return proj;
	}
	
	protected static void SetProjectileSettings(ProjectileEntity projectile, XmlNode node)
	{		
		float speed = 10f, minDistanceToTarget = 10f;
		Vector2 projectileOffset = Vector2.zero;
		
		if (node.SelectSingleNode("./speed") != null)
		{
			float.TryParse(node.SelectSingleNode("./speed").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out speed);	
		}
		
		if (node.SelectSingleNode("./minDistanceToTarget") != null)
		{
			float.TryParse(node.SelectSingleNode("./minDistanceToTarget").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out minDistanceToTarget);	
		}
		
		if (node.SelectSingleNode("./offset") != null)
		{			
			float x = 0, y = 0;	
			
			float.TryParse(node.SelectSingleNode("./offset").Attributes["x"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
			float.TryParse(node.SelectSingleNode("./offset").Attributes["y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
			
			projectileOffset = new Vector2(x, y);
		}
		
		projectile.speed = speed;
		projectile.minDistanceToTarget = minDistanceToTarget;
		projectile.FiringOffset = projectileOffset;
	}
	
	protected static void SetAnimatedProjectileSettings(AnimatedProjectile projectile, XmlNode node)
	{
		SetProjectileSettings(projectile as ProjectileEntity, node);	
	}
	
	#endregion
	
	
	#endregion
}
