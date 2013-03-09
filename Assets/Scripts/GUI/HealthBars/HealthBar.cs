using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour 
{
	public UIPanel healthBarPanel;
	
	public UIProgressBar healthBar;
	
	public EntityComponent myEntity;
	
	public HealthBarGUIManager healthBarGUIManager;

	// Use this for initialization
	void Start () 
	{		
		myEntity.onHealthChanged += OnTargetHealthChanged;
		myEntity.onEntityDestroyed += HandleOnEntityDestroyed;
		
		if (healthBarGUIManager == null)
			healthBarGUIManager = GameObject.Find("HealthBarGUIManager").GetComponent<HealthBarGUIManager>();
		
		healthBarGUIManager.onToggleHealthBar += HandleOnToggleHealthBar;
		
		InitializeHealth();
		
		healthBar.Hide(!healthBarGUIManager.Enabled);
	}

	void HandleOnEntityDestroyed (EntityComponent entityComponent)
	{
		myEntity.onHealthChanged -= OnTargetHealthChanged;
		myEntity.onEntityDestroyed -= HandleOnEntityDestroyed;
		
		healthBarGUIManager.onToggleHealthBar -= HandleOnToggleHealthBar;
	}

	void HandleOnToggleHealthBar(bool enabled)
	{
		healthBar.Hide(!enabled);		
	}
	
	void InitializeHealth()
	{
		healthBar.Value = myEntity.Health / myEntity.MaxHealth;
	}
	
	void OnTargetHealthChanged(EntityComponent target, float change, float currHealth, float maxHealth)
	{
		healthBar.Value = currHealth / maxHealth;		
	}
}
