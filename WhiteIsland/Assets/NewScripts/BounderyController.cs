using GoogleARCore.Examples.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (counter % 13 == 0)
            { // hold for 13 frames to trigger effect 
                switchTruthValue = !switchTruthValue;
                GetComponent<PointcloudVisualizer>().enabled = switchTruthValue;    // enable/disable point cloud
                GetComponent<MeshRenderer>().enabled = switchTruthValue;            // enable/disable point cloud
            }
        }
	}
}
