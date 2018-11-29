using GoogleARCore.Examples.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Since the user can not see through the camera when playing a boundery sytem can be activated by pressing the trigger button of the controller.
 * We use the build in function of ARCore to display the room features to display a blue feature point cloud when pressing the trigger button. 
 * TODO: This could be further improved by storing the points and slowly build a skeleton of the environment independent of the location the device 
 * is used in. 
 */
public class BounderyController : MonoBehaviour {

    bool switchTruthValue = true;
    int counter = 0;

    // Use this for initialization
    void Start () {
        GetComponent<PointcloudVisualizer>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            counter++;

            // hold for 33 frames (half a second) to trigger effect
            if (counter % 13 == 0)
            {  
                switchTruthValue = !switchTruthValue;
                GetComponent<PointcloudVisualizer>().enabled = switchTruthValue;    // enable/disable point cloud
                GetComponent<MeshRenderer>().enabled = switchTruthValue;            // enable/disable point cloud
            }
        }
	}
}