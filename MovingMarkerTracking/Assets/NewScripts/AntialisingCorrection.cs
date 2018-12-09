using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Antialising smoothes corners and setting it via script was easier than setting it under the 
 * quality options.
 */
public class AntialisingCorrection : MonoBehaviour {

	void Start () {
        // higher values up to 4 ight look better but 2 is recommendet for mobile VR.
        QualitySettings.antiAliasing = 2;
	}
}
