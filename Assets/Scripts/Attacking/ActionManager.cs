using UnityEngine;
using System.Collections;

public delegate void OnChargingActionHandler(EntityComponent source, EntityComponent target, float chargeTime);

public delegate void OnPerformingActionHandler(EntityComponent source, EntityComponent target);

public delegate void OnTargetHitHandler(EntityComponent source, EntityComponent target);

public class ActionManager : MonoBehaviour 
{	
	public enum ActionState
	{
		Idle,
		Charging,
		FiringAction
	}
	
	protected IAction myAction;
	
	public IAction MyAction 
	{ 
		get { return myAction; }
		set
		{
			if (myAction != null)
			{
				myAction.OnTargetHitEvent -= OnTargetHitHandler;
			}
			
			myAction = value;
			myAction.OnTargetHitEvent += OnTargetHitHandler;
			myTargetingManager.TargetingRadius = myAction.Range;			
		}
	}
	
	/// <summary>
	/// Gets or sets the TargetingManager used to manage targets for this ActionManager.
	/// </summary>
	public TargetingManager myTargetingManager;
	
	public BehaviorManager myBehaviorManager;
	
	public EntityComponent myEntityComponent;
	
	protected bool performingActions;
	
	public bool PerformingActions 
	{ 
		get { return performingActions; }
		set
		{
			performingActions = value;
		}
	}
	
	public ActionState State { get; protected set; }
		
	protected float idleTimeRemaining;
	
	protected float chargeTimeRemaining;
	
	/// <summary>
	/// An event that fires when this ActionManager begins charging an action against a target.
	/// </summary>
	public event OnChargingActionHandler OnChargingActionEvent;		
			
	/// <summary>
	/// An event that fires when this ActionManager launches the actual action against the target.
	/// </summary>
	public event OnPerformingActionHandler OnPerformingActionEvent;
	
	/// <summary>
	/// An event that fires when the target has been hit by an action.
	/// </summary>
	public event OnTargetHitHandler OnTargetHitEvent;
	

	void Start() 
	{
		if (myBehaviorManager == null)
			myBehaviorManager = this.GetComponent<BehaviorManager>();
		
		myBehaviorManager.OnBehaviorStateChangedEvent += HandleOnBehaviorStateChangedEvent;
		idleTimeRemaining = 0f;
	}

	void HandleOnBehaviorStateChangedEvent(EntityComponent entity, BaseBehaviorStates newState)
	{
		if (newState == BaseBehaviorStates.PerformingActions)
		{
			PerformingActions = true;	
		}
		else
		{
			StopActions();
		}
	}
	
	void Update() 
	{
		if (PerformingActions)
		{
			switch(State)
			{
				case ActionState.Idle:
					StartAction();
					break;
				case ActionState.Charging:
					ChargeAction();
					break;
				case ActionState.FiringAction:
					PerformAction();
					break;
			}			
		}		
	}
	
	public void StopActions()
	{
		performingActions = false;
		State = ActionState.Idle;
	}
	
	protected virtual void OnChargingAction()
	{
		if (OnChargingActionEvent != null)
			OnChargingActionEvent(myEntityComponent, myBehaviorManager.CurrentTarget, MyAction.ChargeTime);		
	}
	
	protected void StartAction()
	{
		if (myEntityComponent == null)
			return;
		
		//Make sure that we're facing the enemy
		if (myBehaviorManager.CurrentTarget.transform.position.x > transform.position.x && myEntityComponent.myMovement.Facing != Facing.Right)
			myEntityComponent.myMovement.Facing = Facing.Right;
		else if (myBehaviorManager.CurrentTarget.transform.position.x < transform.position.x && myEntityComponent.myMovement.Facing != Facing.Left)
			myEntityComponent.myMovement.Facing = Facing.Left;
		
		//Check if it's time to perform our action.
		if (idleTimeRemaining <= 0f)
		{
			State = ActionState.Charging;
			chargeTimeRemaining = MyAction.ChargeTime;
			OnChargingAction();
		}	
		else
		{
			idleTimeRemaining -= Time.deltaTime;
		}
	}
	
	protected void ChargeAction()
	{
		//If we're done charging then change our state to FiringAction!
		if (myBehaviorManager.CurrentTarget != null && chargeTimeRemaining <= 0f)
		{
			State = ActionState.FiringAction;
		}
		else
			chargeTimeRemaining -= Time.deltaTime;			
	}
	
	protected virtual void OnPerformingAction()
	{
		if (OnPerformingActionEvent != null)
			OnPerformingActionEvent(this.myEntityComponent, myBehaviorManager.CurrentTarget);
	}
	
	protected void PerformAction()
	{		
		MyAction.FireAction(myBehaviorManager.CurrentTarget);
		
		OnPerformingAction();
			
		State = ActionState.Idle;
		idleTimeRemaining = MyAction.ActionSpeed;		
	}
	
	protected void OnTargetHitHandler(EntityComponent source, EntityComponent target)
	{
		if (OnTargetHitEvent != null)
			OnTargetHitEvent(source, target);		
	}
}
