using System;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public enum AttackType
{
	Normal
}
		
/// <summary>
/// Provides helper functionality for loading XML definitions of monster. 
/// </summary>
public class MonsterManager
{
	public const string XMLPath = "MonsterDefinitions/monsters";
	protected XmlDocument xmlDoc;
		
	/// <summary>
	/// We maintain a cache of monsters definitions that we've read. As this will never grow tremendously
	/// we can just maintain an ever-growing list.
	/// </summary>
	protected Dictionary<string, MonsterInfo> cachedReads;
	
	protected Dictionary<string, GameObject> cachedMonsters;
		
	void Awake()
	{		
		TextAsset XMLTextAsset = (TextAsset)Resources.Load (XMLPath);
		xmlDoc = new XmlDocument ();
		xmlDoc.LoadXml (XMLTextAsset.text);
			
		cachedReads = new Dictionary<string, MonsterInfo> ();
		cachedMonsters = new Dictionary<string, GameObject> ();
	}
	
	public void PopulateMonster(string monsterName)
	{
		if (!cachedReads.ContainsKey (monsterName))
			LoadMonsterInfo (monsterName);		
	}
		
	public MonsterInfo GetMonsterInfo(string monsterName)
	{
		PopulateMonster (monsterName);				
			
		if (cachedReads.ContainsKey (monsterName))
			return cachedReads [monsterName];
			
		return new MonsterInfo ();			
	}
	
	public GameObject GetMonster(string monsterName)
	{
		GameObject monster = null;
		MonsterInfo info;
		
		if (cachedMonsters.ContainsKey(monsterName))
			return cachedMonsters[monsterName];
		
		if (!cachedReads.ContainsKey(monsterName))
			PopulateMonster(monsterName);
		
		info = cachedReads[monsterName];
		
//		monster = (GameObject) Instantiate(Resources.Load("monster"));		
		SetMonsterStats(monster, info);
		
		monster.SetActiveRecursively(false);
		
		return monster;
	}
	
	public void SetMonsterStats(GameObject monster, MonsterInfo info)
	{
		MonsterComponent monsterComponent = monster.GetComponent<MonsterComponent>();
		AttackManager attackComponent = monster.GetComponentInChildren<AttackManager>();
		
		monsterComponent.Health = info.health;
		monsterComponent.MaxHealth = info.health;
		monsterComponent.MoveSpeed = info.movementSpeed;
		
		if (attackComponent != null)
		{
			attackComponent.attackDamage = info.attackDamage;
			attackComponent.AttackRange = info.attackRange;
			attackComponent.AttackChargeTime = info.attackChargeTime;
			attackComponent.TimeBetweenAttacks = info.timeBetweenAttacks;			
		}		
	}
		
	private void LoadMonsterInfo(string monsterName)
	{
		MonsterInfo monsterInfo = new MonsterInfo ();
			
		if (xmlDoc != null) 
		{
			XmlNode monsterNode = xmlDoc.SelectSingleNode ("/monsters/monster[@name=\'" + monsterName + "\']");
				
			if (monsterNode != null) 
			{
				monsterInfo.UISpriteName = monsterNode.SelectSingleNode ("./UISprite").InnerText;
				
				monsterInfo.gameName = monsterName;
				monsterInfo.gameName = monsterNode.SelectSingleNode ("./gameName").InnerText;
					
				XmlNode statsNode = monsterNode.SelectSingleNode ("./stats");
					
				if (statsNode != null) {
					float health, attackSpeed, attackDamage, attackRange, movementSpeed, spawnTime;
					int maxAlive;
					AttackType attackType;
						
					float.TryParse (statsNode.SelectSingleNode ("./health").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out health);
					float.TryParse (statsNode.SelectSingleNode ("./attackSpeed").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out attackSpeed);
					float.TryParse (statsNode.SelectSingleNode ("./attackDamage").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out attackDamage);
					float.TryParse (statsNode.SelectSingleNode ("./attackRange").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out attackRange);
					float.TryParse (statsNode.SelectSingleNode ("./movementSpeed").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out movementSpeed);
					float.TryParse (statsNode.SelectSingleNode ("./spawnTime").InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out spawnTime);
					
					int.TryParse (statsNode.SelectSingleNode ("./maxAlive").InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out maxAlive);	
									
					attackType = (AttackType)Enum.Parse (typeof(AttackType), statsNode.SelectSingleNode ("./attackType").InnerText);
						
					monsterInfo.health = health;
					monsterInfo.timeBetweenAttacks = attackSpeed;
					monsterInfo.attackDamage = attackDamage;
					monsterInfo.attackRange = attackRange;
					monsterInfo.attackType = attackType;
					monsterInfo.movementSpeed = movementSpeed;
					monsterInfo.spawnTime = spawnTime;
					
					monsterInfo.maxAlive = maxAlive;
				}		
					
				cachedReads.Add (monsterName, monsterInfo);
			} else
				Debug.LogError ("Could not find monster name " + monsterName + " in the Monster XML Document.");
		} else
			Debug.LogError ("Monster XML Document not loaded.");
	}
}


