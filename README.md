# ARCoreInsideOutTrackingS7
Inside Out Tracking for the Galaxy S7

      - The Galaxy S7 is officially not supported but it works non the less. 
      After about 2 minutes the phone gets to hot for the GearVr and the phone needs a short cooldown time. 
      Keep that in mind when planing to build a > 2 minute application or use a wet tissue to cool the phone down.  

Steps:


1.) Follow the ARCore Preview 2 installation steps outlined here: https://developers.google.com/ar/develop/unity/getting-started

      - Make sure to install Unity 2017.3.0f2 or higher! Older Unity versions do not work!
      - Try out the HalloAR app before you continue!
      
2.) Download the "Oculus Utilities for Unity" here: https://developer.oculus.com/downloads/package/oculus-utilities-for-unity-5/

3.) Import the arcore-unity-sdk-preview2.unitypackage into Unity in the same way you imported the arcore-unity-sdk-preview2.unitypackage described in step 1.) 

4.) Find out your Device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/

5.) Generate an osig file which you need for your app to run on your GearVr https://dashboard.oculus.com/tools/osig-generator/

6.) Place the osig file under Assets -> Plugins -> Android -> assets. If those folders don't exist add them yourselfe.  

7.) Under Build Settings -> Player Settings -> XR Settings check Virtual Reality Supported and choose Oculus via the plus symbole.

8.) "Remove Canves", "ExampleController", "PointCloud", "Environmental Light" and "EventSystem" from the Hierachy in Unity so you are left with "ARCore Device" and "Directional light"

9.) Ckick on "First Person Camera" under "ARCore Device". Click the "add Component" button and add "OVR Camera Rig" and "OVR Manager" to the "First Person Camera"

10.) Add a cube and bring it close to the camera. 

11.) Build and Run. You should now see the cube and the camera feed in the backround. 6 degrees of freedome should also work now.

12.) To improve the experiance further you can do the following tricks:
      
      - go to "First Person Camera" -> "Tracked Pose driver" -> "Tracking Type" and set it from "Rotation and Position" to "Position".
      This will make head movements smoother.
      - if you experience flickering corners add "QualitySettings.antiAliasing = 2;" to a Start function of your coice.
      This could look like this:  void Start () { QualitySettings.antiAliasing = 2;}
      - to get better performance click on the "DefaultSessionConfig" and remove the checkmark from "Enable Plane Finding" 
      - to dissable the live camera feed in the backround set "Backround Material" under "AR Core Backround Renderer" to  None 







