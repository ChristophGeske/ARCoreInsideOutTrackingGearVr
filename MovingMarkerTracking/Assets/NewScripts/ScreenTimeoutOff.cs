using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* On Daydream/Cardboard devices the screen might turn into TimeOff because it is on but doesn't get touched.
 */
public class ScreenTimeoutOff : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // prevents Screen Timeout when not connected to GearVr for example when using in Daydream or Cardboard
    }
}