using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class followARCoreCamera : MonoBehaviour {

	public Transform target;
	public float smoothSpeed = 0.5f; // can be between 0.0f and 1.0f.
	public Vector3 posARCore; 

	void Start () {
		posARCore = GoogleARCore.Frame.Pose.position;
	}
		
	void Update () { 

		if (GoogleARCore.Frame.Pose.position.Equals(posARCore)) {
			transform.position = target.position;
		}else {
			Vector3 desiredPosition = target.position;
			Vector3 smoothedPosition = Vector3.Lerp (transform.position, desiredPosition, smoothSpeed); 
			transform.position = smoothedPosition;
		}
		posARCore = GoogleARCore.Frame.Pose.position;
	}
		
}
