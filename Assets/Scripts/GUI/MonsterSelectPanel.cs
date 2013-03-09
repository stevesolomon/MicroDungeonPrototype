using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class MonsterSelectPanel : MonoBehaviour 
{
	/// <summary>
	/// Location of the Level Definition File.
	/// </summary>
	public string levelFile;
	
	public float spaceBetweenBoxes = 2f;
	
	public float firstBoxSpacer = 1.5f;
	
	protected XmlDocument xmlDoc;
	
	protected List<GameObject> monsterSelectionBoxes;
	
	protected Camera viewCamera;
	
	protected Camera boxCamera;
	
	bool buttonDown = false;
	
//	float targetScale = 1.1f;
	//float scaleTime = 0.05f;
	//float pressedWaitTime = 0.3f;
	
	void Start () 
	{		
		viewCamera = GameObject.Find("GUIPanelCamera").GetComponent<Camera>();
		boxCamera = GameObject.Find("GUIBoxCamera").GetComponent<Camera>();
		
		//Open the XML document and start initializing our panel!
		xmlDoc = new XmlDocument();
		xmlDoc.Load(levelFile);
		
		if (collider == null)
		{
			BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
			Vector3 colliderExtents = newCollider.extents;
			colliderExtents.z = 0.2f;
			newCollider.extents = colliderExtents;
		}	
		
		InitializePanel();	
	}
	
	protected void InitializePanel()
	{
		if (xmlDoc != null)
		{
			XmlNodeList monsterNodes = xmlDoc.SelectNodes("/level/monsters/monster");
			
			int numMonsters = monsterNodes.Count;			
			monsterSelectionBoxes = new List<GameObject>(numMonsters);			
			
			float extent = transform.GetComponent<MeshFilter>().sharedMesh.bounds.extents.y / firstBoxSpacer;
			Vector3 xPos = Vector3.Scale(transform.right, transform.position);
			Vector3 yPos = Vector3.Scale(transform.up, transform.position + new Vector3(extent, extent, extent)); 		
			
			//Load each of the available monsters into their own monster selection box which we instantiate.
			foreach (XmlNode monsterNode in monsterNodes)
			{				
				string monsterType = monsterNode.Attributes["name"].InnerText;
				
				if (monsterType != string.Empty)
				{
					GameObject monsterSelectBox = (GameObject) Instantiate(Resources.Load("MonsterSelectionBox"));
					MonsterSelectBox box = monsterSelectBox.GetComponent<MonsterSelectBox>();
					monsterSelectBox.transform.parent = gameObject.transform;
					
					box.monsterName = monsterType;
					
					box.transform.Translate(xPos + yPos);
					box.transform.rotation = transform.rotation;
					
					monsterSelectionBoxes.Add(monsterSelectBox);	
					
					yPos += Vector3.Scale(box.transform.up, new Vector3(-spaceBetweenBoxes, -spaceBetweenBoxes, -spaceBetweenBoxes));				
				}
			}
		}
		else
			Debug.LogError("Could not open Level XML File.");
	}
	
	IEnumerator coHandleButtonPress()
	{
		buttonDown = true; // inhibit processing in Update()
//		bool buttonPressed = true; // the button is currently being pressed
		
		// Button has been pressed for the first time, cursor/finger is still on it
		
		/*while (Input.GetMouseButton(0))
		{
            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
			bool colliderHit = collider.Raycast(ray, out hitInfo, 1.0e8f);
			
            if (buttonPressed && !colliderHit)
			{
				// Finger is still on screen / button is still down, but the cursor has left the bounds of the button

				buttonPressed = false;
			}
			else if (!buttonPressed & colliderHit)
			{
				// Cursor had left the bounds before, but now has come back in
				buttonPressed = true;
			}
			
			yield return 0;
		}
		
		// Handle the case where the cursor was in bounds when the button was released.
		if (buttonPressed)
		{*/
			// Handle case when cursor was in bounds when the button was released / finger lifted
		while (Input.GetMouseButton(0))
		{
			float mouse = Input.GetAxis("Mouse Y");
			
			Debug.Log(mouse);
			
			boxCamera.transform.Translate(0f, -mouse, 0f, Space.Self);
			
			yield return 0;
		}
		//}
		
		buttonDown = false;
		
		yield return 0;
	}
	
	void Update () 
	{
		if (!buttonDown && Input.GetMouseButtonDown(0))
        {
			Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
			
            if (collider.Raycast(ray, out hitInfo, 1.0e8f))
            {
				StartCoroutine(coHandleButtonPress());
            }
        }
	}
}
