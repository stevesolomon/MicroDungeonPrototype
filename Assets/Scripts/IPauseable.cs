using System;

/// <summary>
/// An interface for any object that can be paused.
/// </summary>
public interface IPauseable
{
	bool Paused
	{
		get; 
	}
	
	void Pause();
	
	void Unpause();
}

