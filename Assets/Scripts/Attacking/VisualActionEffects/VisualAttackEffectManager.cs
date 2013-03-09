using UnityEngine;
using System.Collections;

public class VisualAttackEffectManager : MonoBehaviour, IPauseable
{
	public GameObject ChargingAttackEffect;
	protected VisualActionEffect ChargingAttackEffectScript;
	
	public GameObject AttackStartedEffect;
	protected VisualActionEffect AttackStartedEffectScript;
	
	public GameObject AttackHitEffect;
	protected VisualActionEffect AttackHitEffectScript;
	
	public Transform ParentObject;
	
	public AttackManager attackManager;
	
	public bool Paused
	{
		get;
		protected set;
	}
	
	void Start () 
	{
		if (ChargingAttackEffect != null)
		{
			ChargingAttackEffect = Instantiate(ChargingAttackEffect) as GameObject;			
			ChargingAttackEffect.transform.localPosition = Vector3.zero;
			
			ChargingAttackEffectScript = ChargingAttackEffect.GetComponent<VisualActionEffect>();			
			ChargingAttackEffectScript.Visible = false;
			
			ChargingAttackEffectScript.OnActionEffectCompletedDelegate = HandleChargingAttackEffectCompleted;
		}
		
		if (AttackStartedEffect != null)
		{
			AttackStartedEffect = Instantiate(AttackStartedEffect) as GameObject;			
			AttackStartedEffect.transform.localPosition = Vector3.zero;
			
			AttackStartedEffectScript = AttackStartedEffect.GetComponent<VisualActionEffect>();			
			AttackStartedEffectScript.Visible = false;
			
			AttackStartedEffectScript.OnActionEffectCompletedDelegate = HandleAttackStartedEffectCompleted;
		}	
		
		if (AttackHitEffect != null)
		{
			AttackHitEffect = Instantiate(AttackHitEffect) as GameObject;			
			AttackHitEffect.transform.localPosition = Vector3.zero;
			
			AttackHitEffectScript = AttackHitEffect.GetComponent<VisualActionEffect>();			
			AttackHitEffectScript.Visible = false;
			
			AttackHitEffectScript.OnActionEffectCompletedDelegate = HandleAttackHitEffectCompleted;
		}	
		
		//attackManager.OnChargingAttackEvent += HandleOnChargingAttackEvent;
		//attackManager.OnAttackEvent += HandleOnAttackStartedEvent;
		//attackManager.OnTargetHitEvent += HandleOnAttackHitEvent;
	}
	
	void OnDestroy()
	{
		if (ChargingAttackEffect != null)
			Destroy(ChargingAttackEffect);
		
		if (AttackStartedEffect != null)
			Destroy(AttackStartedEffect);
		
		if (AttackHitEffect != null)
			Destroy(AttackHitEffect);
	}
	
	void HandleOnChargingAttackEvent(EntityComponent attacker, EntityComponent target, float chargeTime)
	{
		if (ChargingAttackEffectScript != null)
		{
			ChargingAttackEffectScript.Target = target.transform;
			ChargingAttackEffectScript.Visible = true;
			ChargingAttackEffectScript.Play();
		}
	}
	
	void HandleChargingAttackEffectCompleted()
	{
		ChargingAttackEffectScript.Visible = false;
	}
	
	void HandleOnAttackStartedEvent(EntityComponent attacker, EntityComponent target)
	{
		if (AttackStartedEffectScript != null)
		{
			AttackStartedEffectScript.Target = target.transform;
			AttackStartedEffectScript.Visible = true;
			AttackStartedEffectScript.Play();
		}
	}
	
	void HandleAttackStartedEffectCompleted()
	{
		AttackStartedEffectScript.Visible = false;
	}
	
	void HandleOnAttackHitEvent(EntityComponent attacker, EntityComponent target)
	{
		if (AttackHitEffectScript != null)
		{
			AttackHitEffectScript.Target = target.transform;
			AttackHitEffectScript.Visible = true;
			AttackHitEffectScript.Play();
		}
	}
	
	void HandleAttackHitEffectCompleted()
	{
		AttackHitEffectScript.Visible = false;
	}
		
	public void Pause()
	{
		if (!Paused)
		{
			Paused = true;
			
			if (ChargingAttackEffectScript != null)
				ChargingAttackEffectScript.Pause();
			
			if (AttackStartedEffectScript != null)
				AttackStartedEffectScript.Pause();
			
			if (AttackHitEffectScript != null)
				AttackHitEffectScript.Pause();
		}
	}
	
	public void Unpause()
	{
		if (Paused)
		{
			Paused = false;
			
			if (ChargingAttackEffectScript != null)
				ChargingAttackEffectScript.Unpause();
			
			if (AttackStartedEffectScript != null)
				AttackStartedEffectScript.Unpause();
			
			if (AttackHitEffectScript != null)
				AttackHitEffectScript.Unpause();
		}
	}
}
