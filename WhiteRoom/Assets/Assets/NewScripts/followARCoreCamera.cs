using UnityEngine;

public class followARCoreCamera : MonoBehaviour {

	public Transform ARCoreCamera;
	public float smoothSpeed = 0.5f; // can be between 0.0f and 1.0f.
	public Vector3 StoredARCorePos; 
	public Vector3 latestARCorePos;
   
	void Start () {
		StoredARCorePos = GoogleARCore.Frame.Pose.position;
	}

	void Update () {
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, ARCoreCamera.transform.position, smoothSpeed); // with lerp we go half way between the given positions. Doing this each frame allows us to transform 30fps into 60 fps
        this.transform.position = smoothedPosition; // smoothed position is updated 60 fps
    }

}
