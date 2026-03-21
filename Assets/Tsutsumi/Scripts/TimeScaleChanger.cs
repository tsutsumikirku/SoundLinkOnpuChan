using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleChanger : MonoBehaviour, IUserInterface
{
	public void UnPlay()
	{
		TimeScaleManager.ChangeTimeScale(1f);
	}

	public void Play(int timeScale)
	{
		TimeScaleManager.ChangeTimeScale(timeScale);
	}
}
