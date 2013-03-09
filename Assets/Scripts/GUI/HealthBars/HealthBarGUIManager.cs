using UnityEngine;
using System.Collections;

public delegate void OnToggleHealthBar(bool enabled);

public class HealthBarGUIManager : MonoBehaviour 
{
	public string enableButtonName = "ShowHealthButton";
	
	public string disableButtonName = "HideHealthButton";
	
	protected bool barEnabled = false;
	
	protected UIButton toggleHealthButton;
	
	protected tk2dSprite healthButton;
	
	public event OnToggleHealthBar onToggleHealthBar;
	
	public bool Enabled
	{
		get { return barEnabled; }
	}
	
	void Start()
	{
		toggleHealthButton = GameObject.Find("HealthBarToggleButton").GetComponent<UIButton>();
		healthButton = GameObject.Find("HealthBarToggleButton").GetComponent<tk2dSprite>();
		
		SetButton();
	}
	
	protected void SetButton()
	{
		string name = barEnabled ? disableButtonName : enableButtonName;
		
		healthButton.spriteId = healthButton.GetSpriteIdByName(name);	
	}
	
	public void ToggleHealthBars()
	{
		if (onToggleHealthBar != null)
			onToggleHealthBar(!barEnabled);
		
		barEnabled = !barEnabled;
		
		SetButton();
	}
}
