using UnityEngine;
using System.Collections;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;

public struct LevelInfo
{
	public int num;
	public string title;
	public string description;
	public string imageName;	
	public string sceneName;
}

public class LevelListPopulator : MonoBehaviour 
{	
	public const string LevelXMLPath = "EntityDefinitions/levelList";
	
	protected XmlDocument levelXmlDoc;
	
	protected UIScrollList levelScrollList;
	
	protected LevelSelectManager levelManager;
	
	protected List<LevelInfo> levels;

	// Use this for initialization
	void Start () 
	{
		TextAsset XMLTextAsset = (TextAsset) Resources.Load(LevelXMLPath);
		levelXmlDoc = new XmlDocument ();
		levelXmlDoc.LoadXml(XMLTextAsset.text);	
		
		levelScrollList = GetComponent<UIScrollList>();
		
		levelManager = transform.parent.GetComponentInChildren<LevelSelectManager>();
		
		//Populate the level list with levels.
		PopulateLevels();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	protected void PopulateLevels()
	{
		if (levelXmlDoc == null)
			return;
		
		XmlNodeList levelNodes = levelXmlDoc.SelectNodes("/levels/level");
		levels = new List<LevelInfo>();
		
		//Pull all of the level information from the XML file.
		foreach (XmlNode levelNode in levelNodes)
		{
			int num = 0;
			string title = "", description = "", imageName = "", sceneName = "";
			
			if (levelNode.Attributes["num"] != null)
				int.TryParse(levelNode.Attributes["num"].Value, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out num);
			
			if (levelNode.SelectSingleNode("./title") != null)
				title = levelNode.SelectSingleNode("./title").InnerText;
			
			if (levelNode.SelectSingleNode("./description") != null)
				description = levelNode.SelectSingleNode("./description").InnerText;
			
			if (levelNode.SelectSingleNode("./imageName") != null)
				imageName = levelNode.SelectSingleNode("./imageName").InnerText;
			
			if (levelNode.SelectSingleNode("./scene") != null)
				sceneName = levelNode.SelectSingleNode("./scene").InnerText;
			
			LevelInfo info = new LevelInfo();
			info.num = num;
			info.title = title;
			info.description = description;
			info.imageName = imageName;		
			info.sceneName = sceneName;
			
			levels.Add(info);
		}		
		
		//Just to be safe, sort them by level number
		levels.Sort(CompareLevelNums);
		
		//Now run through them all and populate the list panel with each level.
		foreach (LevelInfo levelInfo in levels)
		{
			AddLevelToList(levelInfo);		
		}
	}
	
	protected void AddLevelToList(LevelInfo levelInfo)
	{
		GameObject levelEntry = Instantiate(Resources.Load("GUI/LevelEntry")) as GameObject;
		UIListItem listScript = levelEntry.GetComponent<UIListItem>();		
		
		listScript.scriptWithMethodToInvoke = levelManager;
		listScript.methodToInvoke = "LevelSelected";	
		listScript.Data = levelInfo.sceneName;
		
		PopulateLevelObject(levelEntry, levelInfo);
		levelScrollList.AddItem(levelEntry);
		
	}
			
	/// <summary>
	/// Compares the level numbers for sorting purposes
	/// </summary>
	/// <returns>
	/// 1 if a > b, 0 if a = b, -1 if a < b
	/// </returns>
	private int CompareLevelNums(LevelInfo a, LevelInfo b)
	{
		if (a.num > b.num)
			return 1;
		else if (a.num < b.num)
			return -1;
		
		return 0;			
	}
	
	private void PopulateLevelObject(GameObject levelObject, LevelInfo levelInfo)
	{
		SpriteText levelNum, levelTitle, levelDescription;
		
		levelNum = levelObject.transform.FindChild("levelNumText").GetComponent<SpriteText>();
		levelTitle = levelObject.transform.FindChild("levelTitleText").GetComponent<SpriteText>();
		levelDescription = levelObject.transform.FindChild("levelDescriptionText").GetComponent<SpriteText>();
		
		levelNum.Text = levelInfo.num.ToString();
		levelTitle.Text = levelInfo.title;
		levelDescription.Text = levelInfo.description;		
	}
}
