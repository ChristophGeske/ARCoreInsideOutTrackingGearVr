using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The AlignementCorrection class solves the problem that booth the ArCore software and gearVr headset calculates the rotation. 
 * Since we want the position in space from the ArCore software but the rotation from the GearVR headset we have to make sure that both 
 * rotations are aligned to one another. We orient ourself on the ArCore position and rotation and reset the headset rotation either
 * by pushing the trigger button or by determening a maximal angle booth rotations are alowed to differ. In principle the rotation values of 
 * the headset and the ArCore software should be the same but fast movements, loss of tracking by the ArCore camera can lead to different 
 * rotation values leading to problems. I decided to use the rotation of the gearVR headset since it is very acurate which is importent to not 
 * sick. The position provided by the ArCore camera does not need to be so precise at least in my experience a small lack is not even noticable in most cases.
 */
public class AlignementCorrection : MonoBehaviour {

    public Transform arCoreCamera;
    public Transform userCamera;

    // Update is called once per frame
    void Update () {
        // Use controller trigger or touchpad to activate realignment when headset orientation and ARCore Orientation became missaligned. 
        // TODO: Alternative activation would use difference in angles between the headset rotation and ArCore Camera instead of using the trgger but that still needs some testing first: (Vector3.Angle(ArCamera.transform.forward , UserCamera.transform.forward) >= 5)  
        if (OVRInput.GetDown(OVRInput.Button.One)) // || (Vector3.Angle(arCoreCamera.transform.forward, userCamera.transform.forward) >= 5) /*|| OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)*/ )
        {
            //Unity engine XR allows to acesses the headset rotation (CenterEye part of the UserCamera object can be used to acess the rotation). The UserCamera rotation needs to be subtrackted by using the Inverse function otherwise the rotation of the ARCoreCamere and UserCamera would be addet together. We also have to set the ArCoreCamera to track position and rotation so we can use the rotation of the ArCoreCamera to reset the UserCamera to this rotation.
            userCamera.rotation = arCoreCamera.rotation * Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye));
        }
    }
}
