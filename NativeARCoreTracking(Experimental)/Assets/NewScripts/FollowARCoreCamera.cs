using UnityEngine;
using UnityEngine.UI;

/* The ArCore Camera only takes 30 pictures per second which in turn leads to a maximal tracking refreshrate of 30 fps as well since we only get
 * new data points about our position in space 30 times a second. To solve this and get a 60 fps tracking to work we use a trick. By spliting the distance 
 * data we recieve in half we can use them to acive 60 fps tracking with the disadvantage of a slight lag since the virtual world only moves half way while 
 * in reality the head moves the full distance. Playing with those values and the Lerp method might further enhance the experience by reducing the lag. This needs
 * some trial and error to find the perfect settings.
 */
public class FollowARCoreCamera : MonoBehaviour {

	public Transform aRCoreCamera;
    public Transform userCamera;

    private Vector3 arCamPos;
    private Vector3 currentCamPos;
    private Vector3 camDist;
    private Vector3 halfCamDist;
    private Vector3 oldArCamPos;

    public Text myText;


    private void Start()
    {
        /**
         * It is VERY IMPORTENT to set the framerate to 60fps since we use the frames as an indicator when we expect new
         * position data from the ARCore camera. 
         */
        Application.targetFrameRate = 60; 
        OVRPlugin.occlusionMesh = true; // recommended by Oculus but I am not sure what the effects on performance are.
    }

    /**
     * We use LateUpdate to make sure ARCore has finished setting the position of the ARCamera we are following here. It might work 
     * with the standard Update method as well. ARCore tracks with 30fps therefore we need to use interpolation to achive 60fps.
     */
    void LateUpdate()
    { 
        /**
         * This if/else Version introduces half a frame of lag but is not doing any errors when updating the position. 
         * But since latency is very bad for VR experiences it might be not the best solution to achive 60fps 
         * by interpolationg from the 30fps tracking delivered by arcore.
         */
        arCamPos = aRCoreCamera.transform.position;
        currentCamPos = userCamera.transform.position;
        camDist = arCamPos - currentCamPos;
        halfCamDist = camDist / 2.0F;
        if (oldArCamPos == arCamPos)
        {
            userCamera.transform.position = arCamPos;
        }
        else // (oldArCamPos != arCamPos)
        {
            userCamera.transform.position = currentCamPos + halfCamDist;
        }

        oldArCamPos = aRCoreCamera.transform.position;

        /**
        * This Version introduces no lag by guessing what the next position might be. This might introduces errors which could in principle 
        * result in position jumps but since the position gets corrected 30 times and the human anatomy doesnt allow for to much change in the 
        * movement this trick is almost safe and helps us to reduce latency which is one of the maijor reasons for motion sickness in VR.
        */
        /* TODO: THIS SHOULD BECOME THE DEFAULT TO REDUCE LATENCY -- IN DEVELOPMENT
        arCamPos = aRCoreCamera.transform.position;
        currentCamPos = userCamera.transform.position;
        camDist = arCamPos - currentCamPos;
        halfCamDist = camDist / 2.0F;
        if (oldArCamPos == arCamPos)
        {
            userCamera.transform.position = arCamPos;
        }
        else // (oldArCamPos != arCamPos)
        {
            userCamera.transform.position = currentCamPos + halfCamDist;
        }
        oldArCamPos = aRCoreCamera.transform.position;
        */

    }
}
