# Inside Out Positional Tracking (6DOF) for Galaxy S7 using ARCore preview 2
ARCore preview 2 enabled Inside Out Positional Tracking (six degrees of freedom) for the Galaxy S7.
      
      
      - The Galaxy S7 is not officially ARCore supported but with the new preview 2 it works non the less.
      
      - Fast movements and poorly lit areas can effect the quality of tracking sevearly. 
      The positional tracking via the camera leads to small movements even when standing still or moving slowly. 

You have three options to get the positional tracking to run on your S7. I listed them in order of ease of use. 

Option 1 (quick option installing the apk):

Download the ARCoreInsideOutTrackingBasic.apk for a simple scene with a block you can walk around or the ARCoreInsideOutTrackingSkyRoom.apk with a more interesting scene with some objects to interact and play with. Than you have to find out your Device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/. With the apk file and the device id you can sign the apk. How you can sign the apk is explained in this youtube video https://www.youtube.com/watch?v=Ho1TbQozyO0. I recommand the option where you download the addosig.bat programm to sign the apk. 

or

Option 2 (working unity project):

You can directly download or clone the working "ARCoreInsideOutTrackingBasic" project to your pc, add your phone specific osig file to the path ARCoreInsideOutTrackingBasic/Assets/Plugins/Android/assets, change the following player settings in Unity 
(

Go to: file -> build settings -> player settings -> other settings

Change the package name to for example "com.unity3d.test"

Uncheck Multithreaded Rendering

Set the Minimum API Level to 7.0 

Set the Target API Level to 7.0

Also go to: file -> build settings -> player settings -> XR settings
check mark virtual reality supported and choose Oculus by using the plus symbol also check mark ARCore supported
), 

build and run the .apk file with Unity 2017.3.0f2 or higher. The project just contains a basic cube that you can walk around using Inside Out Tracking.

or

Option 3 (more detailed option):

Follow these steps to get Inside-Out-Tracking working on your S7 and GearVR:


1.) Follow the ARCore Preview 2 installation steps outlined here: https://developers.google.com/ar/develop/unity/getting-started

      - Make sure to install Unity 2017.3.0f2 or higher! Older Unity versions don't work!
      - Try out the HalloAR app before you continue!
      
2.) Download the "Oculus Utilities for Unity" here: https://developer.oculus.com/downloads/package/oculus-utilities-for-unity-5/

3.) Import the arcore-unity-sdk-preview2.unitypackage into Unity in the same way you imported the arcore-unity-sdk-preview2.unitypackage described in step 1.) 

4.) Find out your Device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/

5.) Generate an osig file which you need for your app to run on your GearVr https://dashboard.oculus.com/tools/osig-generator/

6.) Place the osig file under Assets -> Plugins -> Android -> assets. If those folders don't exist add them yourselfe.  

7.) Under Build Settings -> Player Settings -> XR Settings check Virtual Reality Supported and choose Oculus via the plus symbole.

8.) Remove "Canves", "ExampleController", "PointCloud", "Environmental Light" and "EventSystem" from the Hierachy in Unity so you are left with "ARCore Device" and "Directional light"

9.) Click on "First Person Camera" under "ARCore Device". Click the "add Component" button and add "OVR Camera Rig" and "OVR Manager" to the "First Person Camera"

10.) Add a cube and bring it close to the camera. 

11.) Build and Run. You should now see the cube and the camera feed in the backround. Inside out positional tracking is now active. Sometimes the still experimental ARCore Preview 2 stops in the backround so if you encounter tracking issues try restarting the phone get the ARCore Preview 2 running agein.

12.) To improve the experiance further you can also do the following steps:
      
      - Go to "First Person Camera" -> "Tracked Pose driver" -> "Tracking Type" and set it from "Rotation and Position" to "Position"
      (makes head movements smoother) also set Update Type to "Update and before Render" (makes ARCore tracking better).
      Uncheck the checkmark of "Camera" under "First Person Camera". If the head rotation is 
      not recognised correctly, try to restart the phone, because sometimes the ARCore Preview 2 stops in the backround.
      - If you experience flickering corners add "QualitySettings.antiAliasing = 2;" to a Start function of your coice.
      This could look like this:  void Start () { QualitySettings.antiAliasing = 2;}
      - To get better performance click on the "DefaultSessionConfig" and remove the checkmark from "Enable Plane Finding" and "Enable 
      Light Estimation" 
      - To dissable the live camera feed in the backround set "Backround Material" under "AR Core Backround Renderer" to  None
      - Follow Oculus guidlines for VR settings https://developer.oculus.com/documentation/unity/latest/concepts/unity-build-android/ 
      but uncheck "Multithreaded Rendering" and check "GPU Skinning"
 







