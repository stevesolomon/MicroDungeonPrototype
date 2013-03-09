using UnityEngine;
using System.Collections;

public class MonsterSpawningBarWatcher : MonoBehaviour 
{
	public MonsterSpawner targetSpawner;
	
	public UIProgressBar progressBar;
	
	public UIPanel panel;
	
	protected float lastValue;

	void Start () 
	{
		progressBar = GetComponent<UIProgressBar>();
		progressBar.Value = 0f;
	}
	
	void Update () 
	{
		float currValue = targetSpawner.TimeUntilNextSpawn;
		
		if (Mathf.Abs(lastValue - currValue) > 0.001f)
		{
			float totalTime = targetSpawner.timeToSpawn;
			
			lastValue = currValue;
			
			progressBar.Value = (totalTime - currValue) / totalTime;
		}
	}
}
