using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class followARCoreCamera : MonoBehaviour {

	public Transform target;
	public float smoothSpeed = 0.5f; // can be between 0 and 1.
	public Vector3 posARCore; 
	// test late update also for camera movement

	void Start () {
		posARCore = GoogleARCore.Frame.Pose.position;
	}
		
	void Update () { 

		if (GoogleARCore.Frame.Pose.position.Equals(posARCore)) {
			transform.position = target.position;
		}else {
			Vector3 desiredPosition = target.position;
			Vector3 smoothedPosition = Vector3.Lerp (transform.position, desiredPosition, smoothSpeed); 
			//Vector3 smoothedPosition = target.position - transform.position; // point half way
			//transform.position = target.position;
			transform.position = smoothedPosition;
		}
		posARCore = GoogleARCore.Frame.Pose.position;
	}
		
}
