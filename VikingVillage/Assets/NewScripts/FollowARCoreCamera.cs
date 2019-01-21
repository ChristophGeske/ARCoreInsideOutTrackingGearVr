using UnityEngine;

/* The ArCore Camera only takes 30 pictures per second which in turn leads to a maximal tracking refreshrate of 30 fps as well since we only get
 * new data points about our position in space 30 times a second. To solve this and get a 60 fps tracking to work we are spliting the distance 
 * data we recieve in two halfs allow us to spread the way on two frames instead of going all the way every second frame. The disadvantage of a 
 * slight lag is not noticable. Playing with those values and the Lerp method might further enhance the experience. This needs some trial and error 
 * to find the perfect settings. The current settings work quite well.
 */
public class FollowARCoreCamera : MonoBehaviour {

	public Transform arCoreCamera;
    public Transform userCamera;

    private float speed = 25.5f; //The speed determins how fast the userCamera follows the arCamera. Higher values means following faster and in turn less lag but also less smoothing of the movement. 25.5f is a relative high speed and almost no lag.
    private float maxSpeed;

    private Vector3 arCamPos;
    private Vector3 userCamPos;
    private Vector3 camDist; // Distance between vector of userCamera and the arCamera. When the arCamera updates its position the user camera stays behind and camDist describes the distance the cameras are appart.
    private Vector3 halfCamDist; 
    private Vector3 oldArCamPos; // We remember the ArCamera position of the previous frame to determine if the position has changed. This helps us to determine that the user camera has to follow the arCamera now.

    void Update()
    {
        //Application.targetFrameRate = 60;
    }

        void LateUpdate()
    {      
        arCamPos = arCoreCamera.transform.position;
        userCamPos = userCamera.transform.position;
        camDist = arCamPos - userCamPos;
        halfCamDist = camDist / 2.0F; // By splitting the distance in half we can go half way the first frame and the rest of the way the next frame. This way we double the frame rate from 30fps to 60fps introducing half a frame of lag which is not noticable.

        maxSpeed = speed * Time.deltaTime;

        /* If the arcamera position stayed the same since the last Update funktion call, we know that the arCamera has not moved. 
         * This in turn means we reached an frame where the arCamera does not deliver new data. We also know that the next frame 
         * we should get new data from the arCamera. Knowing this we can set the userCamer to the position of the arCamera because we know
         * that in the next frame we get a new position which we can go half way with the UserCamera.
         * since the last method call. This means we
         */
        if (oldArCamPos == arCamPos)  
        {
            userCamera.transform.position = Vector3.Lerp(userCamPos, arCamPos, maxSpeed);
        }
        /* If the arcamera position changed since the last Update funktion call, we know that the arCamera has moved.  We know should move the 
         * UserCamera as well but nit the full distance because we have to spread the distance ofer two frames. Therfore we only move half the way and 
         * use the rest of the way for the next fraim where we do not expect to get a new position from the arCamera. 
         */
        else // (oldArCamPos != arCamPos)
        {
            userCamera.transform.position = Vector3.Lerp(userCamPos, userCamPos + halfCamDist, maxSpeed);
        }

        oldArCamPos = arCamPos;
    }
    

}
