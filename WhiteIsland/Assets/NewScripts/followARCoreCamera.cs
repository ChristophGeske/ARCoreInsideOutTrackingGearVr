using UnityEngine;

/* The ArCore Camera only takes 30 pictures per second which in turn leads to a maximal tracking refreshrate of 30 fps as well since we only get
 * new data points about our position in space 30 times a second. To solve this and get a 60 fps tracking to work we use a trick. By spliting the distance 
 * data we recieve in half we can use them to acive 60 fps tracking with the disadvantage of a slight lag since the virtual world only moves half way while 
 * in reality the head moves the full distance. Playing with those values and the Lerp method might further enhance the experience by reducing the lag. This needs
 * some trial and error to find the perfect settings.
 */
public class FollowARCoreCamera : MonoBehaviour {

	public Transform aRCoreCamera;
    public Transform userCamera;

    Vector3 smoothedPosition; 
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float speed = 25.5f; //2.5 works also with some slow down
    private float maxSpeed;

    private Vector3 arCamPos;
    private Vector3 currentCamPos;
    private Vector3 camDist;
    private Vector3 halfCamDist;
    private Vector3 oldArCamPos;
    private int counter = 0;

    float smoothTime = 0.06f;
    float yVelocity = 0.0f;

    // 0.0159 is the distance the camera is away from the head. This leads to position changes when turning the had and we wanna avoid that. The current setting need still more refinement. 
    private Vector3 headRotationCorrection = new Vector3(0,0,0.0159f);

    /*
    // the update method is twice as fast as the ARCore Camera. To avoid jittering we use Lerp to smooth out the movement at the start and the end of the movement.
    void Update () {
        startPosition = userCamera.transform.position; 
        endPosition = aRCoreCamera.transform.position - headRotationCorrection;
        maxSpeed = speed * Time.deltaTime;
        // Using Lerp this way is a trick. TODO: There seem to be better solutions which should also be tested and might reduce lag further.
        smoothedPosition = Vector3.Lerp(startPosition, endPosition, maxSpeed); 
        
        this.transform.position = smoothedPosition;                             
    }
    */

    void Update()
    {
        
        arCamPos = aRCoreCamera.transform.position;
        currentCamPos = userCamera.transform.position;
        camDist = arCamPos - currentCamPos;
        halfCamDist = camDist / 2.0F;

        maxSpeed = speed * Time.deltaTime;

        /* // This solution works quite well wenn 60 fps can be reached
        if (oldArCamPos == arCamPos)
        {
            userCamera.transform.position = arCamPos;
        }
        else // (oldArCamPos != arCamPos)
        {
            userCamera.transform.position = currentCamPos + halfCamDist;
        }
        */

        /* // Works well but a small jittereing at the corners is noticable speed is 25
        if (oldArCamPos == arCamPos)
        {
            userCamera.transform.position = Vector3.Lerp(currentCamPos, currentCamPos + halfCamDist, maxSpeed);
        } else
        {
            userCamera.transform.position = Vector3.Lerp(currentCamPos, arCamPos, maxSpeed);
        }
        */



        // Even the wrong one works well hope this one works great. Here we have the correct order. speed is 25. works perfect when 60fps are reached
        if (oldArCamPos == arCamPos)
        {
            userCamera.transform.position = Vector3.Lerp(currentCamPos, arCamPos, maxSpeed);
        }
        else // (oldArCamPos != arCamPos)
        {
            userCamera.transform.position = Vector3.Lerp(currentCamPos, currentCamPos + halfCamDist, maxSpeed);
        }


        /*
        if (currentCamPos != arCamPos && counter == 0)
        {
            userCamera.transform.position = Vector3.Lerp(currentCamPos, currentCamPos + halfCamDist, speed); 
            counter = 1;
        }
        else
        {
            if (currentCamPos != arCamPos && counter == 1)
            {
                counter = 0;
                userCamera.transform.position = Vector3.Lerp(currentCamPos, arCamPos, speed);  
            }
        }
        */

        oldArCamPos = aRCoreCamera.transform.position;

        //this.transform.position = currentCamPos;
    }
    

}
