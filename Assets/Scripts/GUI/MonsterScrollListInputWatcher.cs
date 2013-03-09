using UnityEngine;
using System.Collections;

public class MonsterScrollListInputWatcher : MonoBehaviour 
{	
	protected MonsterGUIManager monsterGUIManager;
	
	void Awake()
	{
		monsterGUIManager = GameObject.Find("MonsterGUIManager").GetComponent<MonsterGUIManager>();	
	}
	
	//We want to watch for input and dismiss the scroll list if the user presses the cancel button.
	void Update () 
	{
		if (Input.GetButtonDown("Cancel"))
			monsterGUIManager.DismissAll();		
	}
	
	public void MonsterPanelCloseClicked()
	{		
		monsterGUIManager.DismissAll();	
	}
}
