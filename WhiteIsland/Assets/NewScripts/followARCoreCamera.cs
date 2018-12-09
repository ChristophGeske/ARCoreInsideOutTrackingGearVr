using UnityEngine;
using UnityEngine.UI;

/* The ArCore Camera only takes 30 pictures per second which in turn leads to a maximal tracking refreshrate of 30 fps as well since we only get
 * new data points about our position in space 30 times a second. To solve this and get a 60 fps tracking to work we use a trick/interpolation. 
 * By spliting the distance from current to next position we can produce a new point half way between current and future position. We can use 
 * these new points to fill every second missing frame to end up with 60 fps tracking. 
 * Unfortenatly the ARCore camera doesn't seem to deliver good data 30 times a second as promissed. Therefore tracking sometimes jumps since the 
 * ARCore camera fails to deliveres new position data or overshootes the true distance moved. To reduce the effect of these sudden jumps by the 
 * arcore camera we use lerp which smoothes all movements that are to quick like the sudden jumps that come from the arcamera. This also smoothes 
 * sudden movements of the player therefore we need to find the right balance bettween smooth down and responsiveness.
 */
public class FollowARCoreCamera : MonoBehaviour {

	public Transform aRCoreCamera;
    public Transform userCamera;

    private float speed = 9f; //Values up to 10 seem to be most compfortable despite slight lag. 25.5 is a high value produces faster responsivness with almost no lag. Lower values would produce more lag but also smooth out jittery movements when for examle bad tracking by the ARCoreCamera occures.
    private float maxSpeed;

    private Vector3 arCamPos;
    private Vector3 currentCamPos;
    private Vector3 camDist;
    private Vector3 halfCamDist;
    private Vector3 oldArCamPos;
    private Vector3 behindArCamPos;

    public Text myText; // for Debug massages displays data in front of camera

	private float smoothTime = 0.05F;// smaller value reaches targed faster. For use with the SmoothDamp function
    private Vector3 velocity = Vector3.zero;
    private float distanceBehindARCam = 0.3F; //The user camera should be set behind the AR camera to avoided distortions when the head rotates.

    private void Start()
    {
        OVRPlugin.occlusionMesh = true; // recommended by oculus not sure what the exact effects on performance are.
    }
		
    void Update()
    { 
        arCamPos = aRCoreCamera.transform.position;
        currentCamPos = userCamera.transform.position;
        //camDist = arCamPos - currentCamPos;
        //halfCamDist = camDist / 2.0F;

        behindArCamPos = arCamPos - (aRCoreCamera.forward * distanceBehindARCam);

        userCamera.transform.position = Vector3.SmoothDamp (currentCamPos, behindArCamPos, ref velocity, smoothTime);

        /*
		// For testing what speed is best suited for the SmoothDamp function wich smoothes the bad tracking by the arcore camera.
        if (OVRInput.GetDown(OVRInput.Button.DpadUp))
        {
            distanceBehindARCam -= 0.05F;
            myText.text = "dist= " + distanceBehindARCam;
        }
        if (OVRInput.GetDown(OVRInput.Button.DpadDown))
        {
            distanceBehindARCam += 0.05F;
            myText.text = "dist= " + distanceBehindARCam;
        }
        */

        /*
		// For testing what speed is best suited for the SmoothDamp function wich smoothes the bad tracking by the arcore camera.
		if (OVRInput.GetDown (OVRInput.Button.DpadUp)) {
			smoothTime -= 0.01F;
			myText.text = "smoothTime= " + smoothTime;
		}
		if (OVRInput.GetDown (OVRInput.Button.DpadDown)) {
			smoothTime += 0.01F;
			myText.text = "smoothTime= " + smoothTime;
		}
        */

        //private bool switcher = false;

        /* 
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
		*/


        /*
		maxSpeed = speed * Time.deltaTime;

		if (oldArCamPos == arCamPos)
		{
			userCamera.transform.position = Vector3.Lerp(currentCamPos, arCamPos, maxSpeed); // lerp helps smoothes out the sudden jumps which are caused by the bad tracking of the arcore camera. A speed of roughly 10 seems to be the best trade of between speed and introduced lag.
		}
		else // (oldArCamPos != arCamPos)
		{
			userCamera.transform.position = Vector3.Lerp(currentCamPos, currentCamPos + halfCamDist, maxSpeed);
		}
		*/

        /*
		maxSpeed = speed * Time.deltaTime;
		userCamera.transform.position = Vector3.Lerp(currentCamPos, arCamPos, maxSpeed);


		/* The idea to create new frames by only going half way would work fine if the ARCore tracking would reliably deliver 30 fps. But 
		 * unfortenatly it doesn't delivers a such data. This leads to jumps in tracking. When we than use these jumps to predict the next step we havent won anything.
		 * When we however use smoothDamp interpolation to go from one frame to the next we can smooth the big jumps a bit. 
		 *
		if (oldArCamPos == arCamPos)
			{
				userCamera.transform.position = Vector3.SmoothDamp(currentCamPos, arCamPos, ref velocity, smoothTime); // lerp helps smoothes out the sudden jumps which are caused by the bad tracking of the arcore camera. A speed of roughly 10 seems to be the best trade of between speed and introduced lag.
			}
			else // (oldArCamPos != arCamPos)
			{
				userCamera.transform.position = Vector3.SmoothDamp(currentCamPos, currentCamPos + halfCamDist, ref velocity, smoothTime);
			}
			oldArCamPos = aRCoreCamera.transform.position;
		*/

    }

    /* This cut of filter could be used to remove spikes.
	public Vector3 highPass(Vector3 accVal) {

		Vector3 filteredAcc = lowPass(accVal);
		filteredAcc.x -= accVal.x;
		filteredAcc.y -= accVal.y;
		filteredAcc.z -= accVal.z;

		return filteredAcc;

	}
	public Vector3 lowPass(Vector3 accVal) {
		xFilt = accVal.x * kFilterFactor + xFilt * (1 - kFilterFactor) ;
		yFilt = accVal.y * kFilterFactor + yFilt * (1 - kFilterFactor) ;
		zFilt = accVal.z * kFilterFactor + zFilt * (1 - kFilterFactor) ;
		Vector3 filteredAcc = new Vector3(xFilt,yFilt,zFilt);
		return filteredAcc;
	}
	*/
}
