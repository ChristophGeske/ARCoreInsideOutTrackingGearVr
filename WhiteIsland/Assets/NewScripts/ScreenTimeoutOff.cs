using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenTimeoutOff : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // prevents Screen Timeout when not connected to GearVr for example when using in Daydream or Cardboard
    }
}
