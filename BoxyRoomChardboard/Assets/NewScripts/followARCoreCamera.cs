using UnityEngine;

public class followARCoreCamera : MonoBehaviour {

	public Transform ARCoreFirstPersonCamera;

    Vector3 smoothedPosition; 
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float speed = 7.5f; //2.5 works well with some slow down
    private float maxSpeed;
    private Vector3 headRotCorrection = new Vector3(0,0,0.0159f); // 0.0159 is the distance the camera is away from the head. This leads to position changes when turning the had and we wanna avoid it. 

    void Update () { // the update method is twice as fast as the ARCore Camera. To avoid jittering we use Learp to smooth out the movement at the start and the end ov the movement.
        startPosition = this.transform.position; 
        endPosition = ARCoreFirstPersonCamera.transform.position - headRotCorrection;
        maxSpeed = speed * Time.deltaTime;                        
        smoothedPosition = Vector3.Lerp(startPosition, endPosition, maxSpeed);   // Using Lerp this way is a trick and there are better solutions. But I am lazy.
        this.transform.position = smoothedPosition;                             
    }

}
