using UnityEngine;
using System.Collections;

public class VisualActionEffectManager : MonoBehaviour, IPauseable
{
	public GameObject chargingActionEffect;
	protected VisualActionEffect chargingActionEffectScript;
	
	public GameObject startedActionEffect;
	protected VisualActionEffect startedActionEffectScript;
	
	public GameObject hitActionEffect;
	protected VisualActionEffect hitActionEffectScript;
	
	public Transform ParentObject;
	
	public ActionManager actionManager;
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	void Start () 
	{
		if (chargingActionEffect != null)
		{
			chargingActionEffect = Instantiate(chargingActionEffect) as GameObject;			
			chargingActionEffect.transform.localPosition = Vector3.zero;
			
			chargingActionEffectScript = chargingActionEffect.GetComponent<VisualActionEffect>();			
			chargingActionEffectScript.Visible = false;
			
			chargingActionEffectScript.OnActionEffectCompletedDelegate = HandleChargingActionEffectCompleted;
		}
		
		if (startedActionEffect != null)
		{
			startedActionEffect = Instantiate(startedActionEffect) as GameObject;			
			startedActionEffect.transform.localPosition = Vector3.zero;
			
			startedActionEffectScript = startedActionEffect.GetComponent<VisualActionEffect>();			
			startedActionEffectScript.Visible = false;
			
			startedActionEffectScript.OnActionEffectCompletedDelegate = HandleActionStartedEffectCompleted;
		}	
		
		if (hitActionEffect != null)
		{
			hitActionEffect = Instantiate(hitActionEffect) as GameObject;			
			hitActionEffect.transform.localPosition = Vector3.zero;
			
			hitActionEffectScript = hitActionEffect.GetComponent<VisualActionEffect>();			
			hitActionEffectScript.Visible = false;
			
			hitActionEffectScript.OnActionEffectCompletedDelegate = HandleActionHitEffectCompleted;
		}	
		
		actionManager.OnChargingActionEvent += HandleOnChargingActionEvent;
		actionManager.OnPerformingActionEvent += HandleOnActionStartedEvent;;
		actionManager.OnTargetHitEvent += HandleOnActionHitEvent;
	}
	
	void OnDestroy()
	{
		if (chargingActionEffect != null)
			Destroy(chargingActionEffect);
		
		if (startedActionEffect != null)
			Destroy(startedActionEffect);
		
		if (hitActionEffect != null)
			Destroy(hitActionEffect);
	}
	
	void HandleOnChargingActionEvent(EntityComponent attacker, EntityComponent target, float chargeTime)
	{
		if (chargingActionEffectScript != null)
		{
			chargingActionEffectScript.Target = target.transform;
			chargingActionEffectScript.Visible = true;
			chargingActionEffectScript.Play();
		}
	}
	
	void HandleChargingActionEffectCompleted()
	{
		chargingActionEffectScript.Visible = false;
	}
	
	void HandleOnActionStartedEvent(EntityComponent attacker, EntityComponent target)
	{
		if (startedActionEffectScript != null)
		{
			startedActionEffectScript.Target = target.transform;
			startedActionEffectScript.Visible = true;
			startedActionEffectScript.Play();
		}
	}
	
	void HandleActionStartedEffectCompleted()
	{
		startedActionEffectScript.Visible = false;
	}
	
	void HandleOnActionHitEvent(EntityComponent attacker, EntityComponent target)
	{
		if (hitActionEffectScript != null)
		{
			hitActionEffectScript.Target = target.transform;
			hitActionEffectScript.Visible = true;
			hitActionEffectScript.Play();
		}
	}
	
	void HandleActionHitEffectCompleted()
	{
		hitActionEffectScript.Visible = false;
	}
		
	public void Pause()
	{
		if (!Paused)
		{
			Paused = true;
			
			if (chargingActionEffectScript != null)
				chargingActionEffectScript.Pause();
			
			if (startedActionEffectScript != null)
				startedActionEffectScript.Pause();
			
			if (hitActionEffectScript != null)
				hitActionEffectScript.Pause();
		}
	}
	
	public void Unpause()
	{
		if (Paused)
		{
			Paused = false;
			
			if (chargingActionEffectScript != null)
				chargingActionEffectScript.Unpause();
			
			if (startedActionEffectScript != null)
				startedActionEffectScript.Unpause();
			
			if (hitActionEffectScript != null)
				hitActionEffectScript.Unpause();
		}
	}
}
