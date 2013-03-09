using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SoundEffectManager 
{
	public static Dictionary<string, AudioClip> cachedSounds;
	
	public const string resourcesPath = "Audio/SoundEffects/";
	
	static SoundEffectManager()
	{
		cachedSounds = new Dictionary<string, AudioClip>(32);		
	}
	
	public static void PlayClip(string name)
	{
		PlayClipAtLocation(name, Vector3.zero);		
	}
	
	public static void PlayClipAtLocation(string name, Vector3 position)
	{
		if (!cachedSounds.ContainsKey(name))
			LoadClip(name);
		
		AudioSource.PlayClipAtPoint(cachedSounds[name], position);		
	}
	
	private static void LoadClip(string name)
	{
		AudioClip newClip = (AudioClip) Resources.Load(resourcesPath + name);
		
		if (newClip != null)
			cachedSounds.Add(name, newClip);		
	}
}
