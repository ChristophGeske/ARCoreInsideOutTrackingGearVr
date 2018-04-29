using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;


public class followARCoreCamera : MonoBehaviour {

	public Transform ARCoreCamera;
	public float smoothSpeed = 0.5f; // can be between 0.0f and 1.0f.
	public Vector3 StoredARCorePos; 
	public Vector3 latestARCorePos;

	void Start () {
		StoredARCorePos = GoogleARCore.Frame.Pose.position;
	}

	void Update () { 
		latestARCorePos = GoogleARCore.Frame.Pose.position;
		if (latestARCorePos.Equals(StoredARCorePos)) {
			transform.position = ARCoreCamera.position;
		}else {
			Vector3 smoothedPosition = Vector3.Lerp (transform.position, ARCoreCamera.position, smoothSpeed); 
			transform.position = smoothedPosition;
		}
		StoredARCorePos = GoogleARCore.Frame.Pose.position;
	}

}

/*working but old*
public class followARCoreCamera : MonoBehaviour {

	public Transform ARCoreCamera;
	public float smoothSpeed = 0.5f; // can be between 0.0f and 1.0f.
	public Vector3 StoredARCorePos; 

	void Start () {
		StoredARCorePos = GoogleARCore.Frame.Pose.position;
	}

	void Update () { 

		if (GoogleARCore.Frame.Pose.position.Equals(StoredARCorePos)) {
			transform.position = ARCoreCamera.position;
		}else {
			Vector3 desiredPosition = ARCoreCamera.position;
			Vector3 smoothedPosition = Vector3.Lerp (transform.position, desiredPosition, smoothSpeed); 
			transform.position = smoothedPosition;
		}
		StoredARCorePos = GoogleARCore.Frame.Pose.position;
	}

}
*/