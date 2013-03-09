using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/GUI/MonsterSelectBox")]
[RequireComponent(typeof(tk2dSprite))]
public class MonsterSelectBox : MonoBehaviour 
{
	public Camera viewCamera;
	
	// Button Up = normal state
	// Button Down = held down
	// Button Pressed = after it is pressed and activated	
	public string buttonDownSprite = "monster_select_box";
	public string buttonUpSprite = "monster_select_box";
	public string buttonPressedSprite = "monster_select_box";
	
	public float Width
	{
		get { return sprite.renderer.bounds.size.x; }
	}
	
	public float Height
	{
		get { return sprite.renderer.bounds.size.y; }
	}
	
	/// <summary>
	/// Name of the monster (as defined in XML) this MonsterSelectBox will select.
	/// If null (or we cannot find a matching entry in the XML file) this entire select
	/// box will be deactivated.
	/// </summary>
	public string monsterName;
	
	public string MonsterName
	{
		get { return monsterName; }
		set
		{
			if (!monsterName.Equals(value))
			{
				monsterName = value;
				LoadMonster();
			}
		}
	}
	
	int buttonDownSpriteId = -1, buttonUpSpriteId = -1, buttonPressedSpriteId = -1;
	
	public AudioClip buttonDownSound = null;
	public AudioClip buttonUpSound = null;
	public AudioClip buttonPressedSound = null;

	// Messaging
	public GameObject targetObject = null;
    public string messageName = "";
	
	tk2dSprite sprite;
	bool buttonDown = false;
	
	float targetScale = 1.1f;
	float scaleTime = 0.05f;
	float pressedWaitTime = 0.3f;
	
	protected tk2dSprite monsterSprite;

	// Use this for initialization
	void Start () 
	{
		if (viewCamera == null)
			viewCamera = GameObject.Find("GUIBoxCamera").GetComponent<Camera>();
		
		sprite = GetComponent<tk2dSprite>();
		
		// Change this to use animated sprites if necessary
		// Same concept here, lookup Ids and call Play(xxx) instead of .spriteId = xxx
		buttonDownSpriteId = sprite.GetSpriteIdByName(buttonDownSprite);
		buttonUpSpriteId = sprite.GetSpriteIdByName(buttonUpSprite);
		buttonPressedSpriteId = sprite.GetSpriteIdByName(buttonPressedSprite);
		
		if (collider == null)
		{
			BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
			Vector3 colliderExtents = newCollider.extents;
			colliderExtents.z = 0.2f;
			newCollider.extents = colliderExtents;
		}	
		
		monsterSprite = transform.FindChild("MonsterImage").GetComponent<tk2dSprite>();
		monsterSprite.enabled = false;
		LoadMonster();
		
		StartCoroutine(coHandleMouseEnter());
	}
	
	// Modify this to suit your audio solution
	// In our case, we have a global audio manager to play one shot sounds and pool them
	void PlaySound(AudioClip source)
	{
		if (audio && source)
		{
			audio.PlayOneShot(source);
		}
	}
	
	IEnumerator coScale(Vector3 defaultScale, float startScale, float endScale)
    {
		Vector3 scale = defaultScale;
		float s = 0.0f;
		while (s < scaleTime)
		{
			float t = Mathf.Clamp01(s / scaleTime);
			float scl = Mathf.Lerp(startScale, endScale, t);
			scale = defaultScale * scl;
			transform.localScale = scale;
			
			s += Time.deltaTime;
			yield return 0;
		}
		
		transform.localScale = defaultScale * endScale;
    }
	
	IEnumerator coHandleButtonPress()
	{
		buttonDown = true; // inhibit processing in Update()
		bool buttonPressed = true; // the button is currently being pressed
		
		// Button has been pressed for the first time, cursor/finger is still on it
		PlaySound(buttonDownSound);
		sprite.spriteId = buttonDownSpriteId;
		
		while (Input.GetMouseButton(0))
		{
            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
			bool colliderHit = collider.Raycast(ray, out hitInfo, 1.0e8f);
			
            if (buttonPressed && !colliderHit)
			{
				// Finger is still on screen / button is still down, but the cursor has left the bounds of the button
				PlaySound(buttonUpSound);
				sprite.spriteId = buttonUpSpriteId;

				buttonPressed = false;
			}
			else if (!buttonPressed & colliderHit)
			{
				// Cursor had left the bounds before, but now has come back in
				PlaySound(buttonDownSound);
				sprite.spriteId =  buttonDownSpriteId;

				buttonPressed = true;
			}
			
			yield return 0;
		}
		
		if (buttonPressed)
		{
			// Handle case when cursor was in bounds when the button was released / finger lifted
			PlaySound(buttonPressedSound);
			sprite.spriteId = buttonPressedSpriteId;
				
			if (targetObject)
				targetObject.SendMessage(messageName);
			
			yield return new WaitForSeconds(pressedWaitTime);
			sprite.spriteId = buttonUpSpriteId;
		}
		
		buttonDown = false;
	}
	
	IEnumerator coHandleMouseEnter()
	{
		Vector3 defaultScale = transform.localScale;
		bool prevHit = false;
		
		while(true)
		{
			//Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            //RaycastHit hitInfo;
			//bool colliderHit = collider.Raycast(ray, out hitInfo, 1.0e8f);
			
			RaycastHit hitInfo;
			Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
			Physics.Raycast(ray, out hitInfo, 100f, 1 << 13);  
			bool colliderHit = hitInfo.collider == collider;
						
			if (colliderHit && !prevHit)
			{
				yield return StartCoroutine(coScale(defaultScale, 1.0f, targetScale));	
				prevHit = true;
			}
			else if (!colliderHit && prevHit)
			{
				yield return StartCoroutine(coScale(defaultScale, targetScale, 1.0f));		
				prevHit = false;
			}
			else
				yield return new WaitForSeconds(0.033f);
		}
		
	}
	
	// Update is called once per frame
	void Update ()
	{		
		if (!buttonDown && Input.GetMouseButtonDown(0))
        {
			RaycastHit hitInfo;
			Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
			Physics.Raycast(ray, out hitInfo, 100f, 1 << 13);            
			
           // if (collider.Raycast(ray, out hitInfo, 1.0e8f))
            //{
			//	StartCoroutine(coHandleButtonPress());
            //}
			
			if (hitInfo.collider == collider)
			{
				StartCoroutine(coHandleButtonPress());
			}
        }
	}
	
	protected bool LoadMonster()
	{
		bool loaded = false;
		
//		MonsterInfo info = XMLMonsterLoader.GetMonsterInfo(monsterName);
		
/*		if (info.UISpriteName != string.Empty)
		{
			int spriteID = sprite.GetSpriteIdByName(info.UISpriteName);
			monsterSprite.spriteId = spriteID;
			loaded = true;
			
			monsterSprite.enabled = true;
		}*/
		
		return loaded;				
	}
}
