using UnityEngine;
using System.Collections;

public class LevelSelectManager : MonoBehaviour 
{
	public GameObject levelConfirmPrefab;
	
	public UIScrollList levelScrollList;
	
	protected GameObject levelConfirmInstance;
	
	protected bool showingLevelConfirm;
	
	protected int levelConfirmIndex;
	
	protected string selectedLevelScene;

	public void LevelSelected()
	{
		if (showingLevelConfirm)
			LevelCanceled();
		
		levelConfirmIndex = levelScrollList.SelectedItem.Index + 1;
		
		selectedLevelScene = (string) levelScrollList.SelectedItem.Data;
			
		//When a level has been selected in the list, push in the level confirm game object to confirm the choice.
		levelConfirmInstance = Instantiate(levelConfirmPrefab) as GameObject;
		levelScrollList.InsertItem(levelConfirmInstance.GetComponent<UIListItemContainer>(), levelConfirmIndex);
		levelScrollList.ScrollToItem(levelConfirmIndex, 1.0f);
			
		UIButton confirmButton = levelConfirmInstance.transform.FindChild("confirmButton").GetComponent<UIButton>();
		UIButton cancelButton = levelConfirmInstance.transform.FindChild("cancelButton").GetComponent<UIButton>();
			
		confirmButton.scriptWithMethodToInvoke = this;
		cancelButton.scriptWithMethodToInvoke = this;
		
		confirmButton.methodToInvoke = "LevelConfirmed";
		cancelButton.methodToInvoke = "LevelCanceled";
			
		showingLevelConfirm = true;		
	}
	
	public void LevelConfirmed()
	{
		Application.LoadLevel(selectedLevelScene);		
	}
	
	public void LevelCanceled()
	{
		levelScrollList.RemoveItem(levelConfirmIndex, true);
		levelScrollList.ScrollToItem(levelConfirmIndex - 1, 1.0f);
		showingLevelConfirm = false;
		selectedLevelScene = null;
	}
}
