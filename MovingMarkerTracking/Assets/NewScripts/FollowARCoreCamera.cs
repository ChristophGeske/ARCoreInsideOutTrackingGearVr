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

    private float speed = 25.5f; //25.5 is a high value and in combination with lerp produces fast responsivnes with almost no lag. Lower values would produce more lag but also smooth out jittery movements when for examle bad tracking by the ARCoreCamera occures.
    private float maxSpeed;

    private Vector3 arCamPos;
    private Vector3 currentCamPos;
    private Vector3 camDist;
    private Vector3 halfCamDist;
    private Vector3 oldArCamPos;

    public Text myText;

    private void Start()
    {
        OVRPlugin.occlusionMesh = true; // recommended by Oculus but I am not sure what the effects on performance are.
    }

    void Update()
    { 
        arCamPos = aRCoreCamera.transform.position;
        currentCamPos = userCamera.transform.position;
        camDist = arCamPos - currentCamPos;
        halfCamDist = camDist / 2.0F;

        if (OVRInput.GetDown(OVRInput.Button.DpadUp))
        {
            speed += 1;
            myText.text = "Speed= "+speed;
        }

        if (OVRInput.GetDown(OVRInput.Button.DpadDown))
        {
            speed -= 1;
            myText.text = "Speed= " + speed;
        }

        /*
        maxSpeed = speed * Time.deltaTime;
        userCamera.transform.position = Vector3.Lerp(currentCamPos, arCamPos, maxSpeed);
        */

        maxSpeed = speed * Time.deltaTime;

        // Even the wrong one works well hope this one works great. Here we have the correct order. speed is 25. works perfect when 60fps are reached.
        if (oldArCamPos == arCamPos)
        {
            userCamera.transform.position = Vector3.Lerp(currentCamPos, arCamPos, maxSpeed);
        }
        else // (oldArCamPos != arCamPos)
        {
            userCamera.transform.position = Vector3.Lerp(currentCamPos, currentCamPos + halfCamDist, maxSpeed);
        }
        
        oldArCamPos = aRCoreCamera.transform.position;
    }
    

}
