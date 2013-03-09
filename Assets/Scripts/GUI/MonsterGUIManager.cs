using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public delegate void MonsterSelected(MonsterInformation monsterInfo);

public delegate void SellMonsterResults(bool sold);

public class MonsterGUIManager : MonoBehaviour
{
	protected Camera scrollListCamera;
	
	protected UIScrollList monsterScrollList;
	
	protected UIPanel statsPanel;
	
	public float damageBarMaxValue = 100f;	
	public float attackSpeedBarMaxValue = 3f;	
	public float attackRangeBarMaxValue = 200f;	
	public float movementSpeedBarMaxValue = 100f;	
	public float healthBarMaxValue = 100f;
	
	protected UIProgressBar magnitudeBar;
	protected UIProgressBar attackSpeedBar;
	protected UIProgressBar attackRangeBar;
	protected UIProgressBar movementSpeedBar;	
	protected UIProgressBar healthBar;
	protected SpriteText spawnRate;
	protected SpriteText monsterName;
	protected SpriteText goldCost;
	protected tk2dSprite monsterSprite;
	protected tk2dSprite confirmButtonImage;
	
	protected tk2dSprite[] actionEffectIcons;
	
	protected UIButton confirmButton;
	
	protected UIPanel monsterSaleConfirmationPanel;
	protected UIButton monsterSaleConfirmButton;
	protected UIButton monsterSaleCancelButton;
	
	protected bool monsterChosen = false;
	
	protected string chosenMonsterName;
	
	protected LevelManager levelManager;
	
	protected MonsterSelected monsterSelectedCallback;
	
	protected EntityFactory entityFactory;
	
	protected UIPanel listPanel;
	
	public PlayerStatusManager playerStatusManager;
	
	bool monsterSelectionGUIOpen = false;
	
	void Start()
	{		
		monsterScrollList = GameObject.Find("MonsterScrollList").GetComponent<UIScrollList>();
		statsPanel = GameObject.Find("MonsterInfoPanel").GetComponent<UIPanel>();
		
		magnitudeBar = statsPanel.transform.FindChild("damage").FindChild("damageBar").GetComponent<UIProgressBar>();
		attackSpeedBar = statsPanel.transform.FindChild("attackSpeed").FindChild("attackSpeedBar").GetComponent<UIProgressBar>();
		attackRangeBar = statsPanel.transform.FindChild("range").FindChild("rangeBar").GetComponent<UIProgressBar>();
		movementSpeedBar = statsPanel.transform.FindChild("moveSpeed").FindChild("moveSpeedBar").GetComponent<UIProgressBar>();
		healthBar = statsPanel.transform.FindChild("health").FindChild("healthBar").GetComponent<UIProgressBar>();
		spawnRate = statsPanel.transform.FindChild("spawnRate").GetComponent<SpriteText>();
		
		monsterName = statsPanel.transform.FindChild("name").GetComponent<SpriteText>();
		monsterSprite = statsPanel.transform.FindChild("sprite").GetComponent<tk2dSprite>();
		
		goldCost = statsPanel.transform.FindChild("goldCost").GetComponent<SpriteText>();
		
		confirmButton = statsPanel.transform.FindChild("okayButton").GetComponent<UIButton>();
		confirmButtonImage = confirmButton.GetComponent<tk2dSprite>();
		
		monsterSaleConfirmationPanel = GameObject.Find("SellConfirmationPanel").GetComponent<UIPanel>();
		monsterSaleConfirmButton = monsterSaleConfirmationPanel.transform.FindChild("okayButton").GetComponent<UIButton>();
		monsterSaleCancelButton = monsterSaleConfirmationPanel.transform.FindChild("cancelButton").GetComponent<UIButton>();
		monsterSaleConfirmButton.scriptWithMethodToInvoke = this;
		monsterSaleCancelButton.scriptWithMethodToInvoke = this;
		monsterSaleConfirmButton.methodToInvoke = "MonsterSaleConfirmed";
		monsterSaleCancelButton.methodToInvoke = "MonsterSaleCancelled";
				
		scrollListCamera = monsterScrollList.renderCamera;		
		
		levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
		
		listPanel = GameObject.Find("MonsterSelectionPanel").GetComponent<UIPanel>();
		
		Transform attackEffectParent = GameObject.Find("attackEffectIcons").transform.FindChild("icons");
		actionEffectIcons = attackEffectParent.GetComponentsInChildren<tk2dSprite>();	
		
		if (playerStatusManager == null)
			playerStatusManager = GameObject.Find("PlayerStatusManager").GetComponent<PlayerStatusManager>();
		
		entityFactory = EntityFactory.GetInstance();
		
		monsterSelectedCallback = null;
		
		LoadMonsterScrollPanel();
	}
	
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
	public void LaunchMonsterGUI(MonsterSpawner spawner, MonsterSelected monsterChosenCallback)
	{
		//If we already have an active spawner set then deactivate it
		if (this.monsterSelectedCallback != null)
			this.monsterSelectedCallback(null);
			
		this.monsterSelectedCallback = monsterChosenCallback;		
		
		if (!monsterSelectionGUIOpen)
		{
			StartCoroutine(coLaunchGUI(spawner));
			monsterSelectionGUIOpen = true;
		}					
	}	
	
	public IEnumerator coLaunchGUI(MonsterSpawner monsterSpawner)
	{		
		bool done = false;
		
		DismissSaleConfirmationPanel();
		
		if (monsterSoldCallback != null)
		{
			monsterSoldCallback(false);
			monsterSoldCallback = null;
		}
		
		//this.monsterSelectedCallback = monsterSelectedCallback;
		
		monsterChosen = false;
		
		listPanel.BringIn();
		
		while (!done)
		{
			if (monsterChosen)
			{
				done = true;
				
				Debug.Log("Monster chosen is: " + monsterScrollList.LastClickedControl.Data);			
			}
			
			yield return new WaitForSeconds(0.033f);
		}		
		
		yield return null;	
	}
	
	MonsterInformation monsterInfo;
	
	public void OnMonsterChosen()
	{
		monsterChosen = true;	
		
		monsterInfo = EntityFactory.GetInstance().GetMonsterInfo((string) monsterScrollList.LastClickedControl.Data);
		
		ShowMonsterStats(monsterInfo);
	}
	
	public void OnMonsterAcceptButtonPressed()
	{
		Debug.Log("Accepted!!");
		
		if (monsterSelectedCallback != null)
		{
			monsterSelectedCallback(monsterInfo);
			monsterSelectedCallback = null;
		}
		
		listPanel.Dismiss();
		statsPanel.Dismiss();	
		monsterSelectionGUIOpen = false;
	}
	
	public void OnMonsterCancelButtonPressed()
	{
		Debug.Log("Monster choice cancelled");
		
		if (monsterSelectedCallback != null)
		{
			monsterSelectedCallback(null);
			monsterSelectedCallback = null;
		}
		
		listPanel.BringIn();
		statsPanel.Dismiss();			
		monsterSelectionGUIOpen = false;
	}
	
	public void DismissListPanel()
	{		
		listPanel.Dismiss();
		
		//We'll want to clean this up just to be safe. Let the subscribers know that
		//no monster was chosen.
		if (monsterSelectedCallback != null)
		{
			monsterSelectedCallback(null);
			monsterSelectedCallback = null;
		}
	}
	
	public void DismissStatsPanel()
	{
		statsPanel.Dismiss();		
	}
	
	public void DismissSaleConfirmationPanel()
	{
		monsterSaleConfirmationPanel.Dismiss();
	}
	
	public void DismissAll()
	{
		DismissStatsPanel();
		DismissListPanel();		
		DismissSaleConfirmationPanel();
		
		monsterSelectionGUIOpen = false;
	}
	
	protected void ShowMonsterStats(MonsterInformation monsterInfo)
	{		
		statsPanel.BringIn();
		
		magnitudeBar.Value = monsterInfo.actionInformation.magnitude / damageBarMaxValue;
		attackSpeedBar.Value = (attackSpeedBarMaxValue - monsterInfo.actionInformation.actionSpeed) / attackSpeedBarMaxValue;
		attackRangeBar.Value = monsterInfo.actionInformation.actionRange / attackRangeBarMaxValue;
		movementSpeedBar.Value = monsterInfo.moveSpeed / movementSpeedBarMaxValue;
		healthBar.Value = monsterInfo.maxAlive / healthBarMaxValue;
		
		spawnRate.Text = monsterInfo.spawnTime + "/" + monsterInfo.maxAlive;
		
		confirmButton.controlIsEnabled = true;
		confirmButtonImage.color = Color.white;
		
		goldCost.Text = monsterInfo.goldCost.ToString();
		goldCost.Color = Color.green;
		
		monsterName.Text = monsterInfo.gameName;
		monsterName.SetAlignment(SpriteText.Alignment_Type.Center);
		monsterSprite.spriteId = monsterSprite.GetSpriteIdByName(monsterInfo.UISpriteName);	
		
		//Push the attack modifier icons
		for (int i = 0; i < actionEffectIcons.Length && i < monsterInfo.actionInformation.actionModifierSpriteNames.Count; i++)
		{			
			actionEffectIcons[i].spriteId = actionEffectIcons[i].GetSpriteIdByName(monsterInfo.actionInformation.actionModifierSpriteNames[i]);			
		}
		
		//Make sure the player can purchase the monster, otherwise we'll disable the buy button.
		if (playerStatusManager.Gold < monsterInfo.goldCost)
		{
			goldCost.Color = Color.red;
			confirmButtonImage.color = Color.gray;
			confirmButton.controlIsEnabled = false;
		}
	}
	
	public void AddMonster(MonsterInformation monsterInfo)
	{	
		GameObject monsterButton = Instantiate(Resources.Load("MonsterButton")) as GameObject;
		tk2dSprite sprite = monsterButton.transform.FindChild("box").FindChild("sprite").GetComponent<tk2dSprite>();
				
		sprite.spriteId = sprite.GetSpriteIdByName(monsterInfo.UISpriteName);
		monsterButton.GetComponentInChildren<UIButton>().data = monsterInfo.dataName;
				
		monsterScrollList.AddItem(monsterButton);		
	}
	
	protected SellMonsterResults monsterSoldCallback;	
	
	public void LaunchSellMonsterGUI(MonsterSpawner spawner, SellMonsterResults monsterSoldCallback)
	{
		DismissAll();
		
		if (monsterSelectedCallback != null)
		{
			monsterSelectedCallback(null);
			monsterSelectedCallback = null;
		}
		
		//If we already have an active callback set then return false to that caller since it's monster wasn't sold.
		if (this.monsterSoldCallback != null)
			this.monsterSoldCallback(false);
		
		this.monsterSoldCallback = monsterSoldCallback;
		monsterSaleConfirmationPanel.BringIn();
	}
	
	public void MonsterSaleConfirmed()
	{
		if (monsterSoldCallback != null)
		{
			monsterSoldCallback(true);
			monsterSoldCallback = null;
		}		
		
		monsterSaleConfirmationPanel.Dismiss();
	}
	
	public void MonsterSaleCancelled()
	{
		if (monsterSoldCallback != null)
		{
			monsterSoldCallback(false);
			monsterSoldCallback = null;
		}		
		
		monsterSaleConfirmationPanel.Dismiss();
	}
	
	protected void LoadMonsterScrollPanel()
	{
		foreach (KeyValuePair<string, int> monsterEntry in levelManager.availableMonsters)
		{
			for (int i = 0; i < monsterEntry.Value; i++)
				AddMonster(entityFactory.GetMonsterInfo(monsterEntry.Key));			
		}		
	}
}


