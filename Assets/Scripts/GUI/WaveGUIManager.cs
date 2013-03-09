using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveGUIManager : MonoBehaviour, IPauseable
{
	public delegate void startWaveButtonPressedDelegate(); 
	
	public GameObject waveCompletedImage;
	
	public float waveCompletedMessageSpinInTime = 1f;
	
	public float waveCompletedMessageHoldTime = 5f;
	
	public string waveStartButtonName = "StartNextWaveButton";
	
	public string waveInProgressButtonName = "WaveInProgressButton";
	
	protected UIInteractivePanel startWavePanel;
	
	protected UIButton startWaveButton;
	
	protected UIPanel waveInfoPanel;
	
	protected UIScrollList waveInfoScrollList;
	
	protected HeroWaveManager heroWaveManager;
	
	protected tk2dSprite waveButton;
	
	public bool Paused
	{
		get;
		protected set;
	}
		
	public event startWaveButtonPressedDelegate onStartWaveButtonPressed;
	
	void Awake()
	{
		startWavePanel = GameObject.Find("WavePanel").GetComponent<UIInteractivePanel>();
		startWaveButton = GameObject.Find("startWaveButton").GetComponent<UIButton>();	
		
		waveInfoPanel = GameObject.Find("WaveInfoPanel").GetComponent<UIPanel>();
		waveInfoScrollList = GameObject.Find("WaveInfoScrollList").GetComponent<UIScrollList>();
		waveInfoPanel.BringIn();
		
		heroWaveManager = GameObject.Find("WaveManager").GetComponent<HeroWaveManager>();
		heroWaveManager.onAllWaveEnemiesDefeated += HandleAllWaveEnemiesDefeated;
		heroWaveManager.onNextWaveReady += HandleOnNextWaveReady;
		heroWaveManager.onSpawnNewHero += HandleOnSpawnNewHero;
		
		waveButton = GameObject.Find("startWaveButton").GetComponent<tk2dSprite>();
		waveButton.spriteId = waveButton.GetSpriteIdByName(waveStartButtonName);
	}
	
	/// <summary>
	/// Handles the event that fires when all enemies in a given wave are defeated.
	/// </summary>
	/// <param name='currWaveNum'>
	/// The current wave number that has been completed.
	/// </param>
	void HandleAllWaveEnemiesDefeated(int currWaveNum)
	{
		StartCoroutine(coDisplayWaveCompletedMessage());		
	}
	
	/// <summary>
	/// Displays the -Wave Completed- message for set period of time.
	/// </summary>
	IEnumerator coDisplayWaveCompletedMessage()
	{
		GameObject msg = Instantiate(waveCompletedImage, new Vector3(0, 10f, 0), Quaternion.identity) as GameObject;
		
		float sleepTime = 0.033f;
		float numSleeps = waveCompletedMessageSpinInTime / sleepTime;
		int currSleeps = 0;
		float degPerSleep = 90f / numSleeps;
		bool goingAway = false;
		
		while (true) 
		{
			currSleeps++;
			
			if (currSleeps > numSleeps)
			{
				currSleeps = 0;
				
				if (!goingAway)
				{
					yield return new WaitForSeconds(waveCompletedMessageHoldTime);
					goingAway = true;
				}
				else
					break;
			}
			
			if (!goingAway)
				msg.transform.Rotate(degPerSleep, 0f, 0f, Space.World);			
			else
				msg.transform.Rotate(-degPerSleep, 0f, 0f, Space.World);	
			
			yield return new WaitForSeconds(sleepTime);			
		}
		
		Destroy(msg);		
	}

	void HandleOnSpawnNewHero(WaveHero hero)
	{
		waveInfoScrollList.RemoveItem(0, true);
	}

	void HandleOnNextWaveReady(Wave wave)
	{
		if (wave != null)
		{			
			waveInfoScrollList.ClearList(false);
			List<WaveHero> heroes = wave.WaveHeroes;	
			
			foreach(WaveHero hero in heroes)
			{
				GameObject heroEntry = Instantiate(Resources.Load("WaveHeroEntry")) as GameObject;
				tk2dSprite sprite = heroEntry.transform.FindChild("Sprite").GetComponent<tk2dSprite>();
				SpriteText text = heroEntry.transform.FindChild("Name").GetComponent<SpriteText>();
				
				sprite.spriteId = sprite.GetSpriteIdByName(hero.heroInfo.UISpriteName);	
				text.Text = hero.heroInfo.gameName;
				
				waveInfoScrollList.AddItem(heroEntry);
			}			
		}
	}
	
	void Start()
	{
		startWavePanel.BringIn();
	}
	
	// Update is called once per frame
	void Update ()  
	{
	
	}
	
	public void EnableStartWaveButton()
	{
		startWaveButton.controlIsEnabled = true;	
		
		waveButton.spriteId = waveButton.GetSpriteIdByName(waveStartButtonName);
	}
	
	public void DisableStartWaveButton()
	{
		startWaveButton.controlIsEnabled = false;
		
		waveButton.spriteId = waveButton.GetSpriteIdByName(waveInProgressButtonName);
	}
	
	public void StartWaveButtonPressed()
	{
		Debug.Log("Start Wave Button has been pressed - letting listeners know!");
		
		if (onStartWaveButtonPressed != null)
			onStartWaveButtonPressed();		
	}
	
	protected bool startWaveButtonSavedState;
	public void Pause()
	{
		if (!Paused)
		{
			Paused = true;
			startWaveButtonSavedState = startWaveButton.controlIsEnabled;
			startWaveButton.controlIsEnabled = false;			
		}		
	}
	
	public void Unpause()
	{
		if (Paused)
		{
			Paused = false;			
			startWaveButton.controlIsEnabled = startWaveButtonSavedState;
		}
	}
}
