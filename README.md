# ARCore Inside Out Tracking for Galaxy S7
ARCore enabled Inside Out Tracking for the Galaxy S7

      - The Galaxy S7 is officially not ARCore supported but it works non the less.   

You can directly download a working Inside-Out-Tracking Unity project by downloading the "ARCoreInsideOutTrackingBasic" project to your pc, adding your phone specific osig file under ARCoreInsideOutTrackingBasic/Assets , build and run the .apk file.

or

Follow these steps to get Inside-Out-Tracking working on your S7 and GearVR:


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

11.) Build and Run. You should now see the cube and the camera feed in the backround. Inside out positional tracking is now active. Sometimes the still experimental ARCore Preview 2 stops in the backround so if you encounter tracking issues try restarting the phone get the ARCore Preview 2 running agein.

12.) To improve the experiance further you can also do the following steps:
      
      - Go to "First Person Camera" -> "Tracked Pose driver" -> "Tracking Type" and set it from "Rotation and Position" to "Position".
      This will make head movements smoother. If the head rotation is tracked but not the head movement try to uncheck the checkmark
      of "Camera" under "First Person Camera" and try agein. If the head rotation is still not recognised, try to restart the phone. I
      encountered thosse problems when the ARCore Preview 2 stooped in the backround.
      try again. If you than still 
      - If you experience flickering corners add "QualitySettings.antiAliasing = 2;" to a Start function of your coice.
      This could look like this:  void Start () { QualitySettings.antiAliasing = 2;}
      - To get better performance click on the "DefaultSessionConfig" and remove the checkmark from "Enable Plane Finding" 
      - To dissable the live camera feed in the backround set "Backround Material" under "AR Core Backround Renderer" to  None 
 







