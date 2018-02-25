# Inside Out Positional Tracking (6DoF) for GearVR using ARCore v1.0.0
ARCore v1.0.0 enabled Inside Out Positional Tracking (six degrees of freedom) for the Galaxy S7, S8, S8+ and Note 8.
      
      
      
      
       - WARNING YOU MIGHT GET SICK: The positional tracking via the camera leads to small movements even when 
       standing still or moving slowly. 
       The reason for that seems to be the low camera frame rate (30fps) which the ARCore app is using. 
       The position changes in the environment therfore can only be updated 30 times per seconed. 
       Oculus recomends at least 60fps or better 90fps. 
       This app dose not reach those fps and therefore can make you sick. The frame rate issue was discussed here: 
       (https://github.com/google-ar/arcore-unity-sdk/issues/34) 
      
      - This was tested on the Samsung Galaxy S7 but the S8, S8+ and Note8 seem to work also thanks to the new ARCore v1.0   
      (https://developers.google.com/ar/discover/#supported_devices)
      
      - Fast movements, featureless white areas and poorly lit areas can affect the quality of tracking severely. 
      When the app starts it takes 3-5 seconds for ARCore to detect a plain at this stage it is best to stay still 
      and onle move slightly from left to right. 
      After the tracking is initialised slowly move and look once around your room to allow ARCore to scann the area. 
      Now the tracking should be stable enought for faster head movements. If the vr and real world movement don't
      line up correctly restart the app and keep your head steady when the app is starting up.  
         

++ ARCore1.0InsideOutPositionalTracking.rar is a working Unity project you can use to build your own app with positional tracking enabled. You have to unzip it before importing it to Unity 2017.3.1f1 or later.

++ ARCore1.0InsideOutPositionalTrackingBasic.apk simple scene with positional tracking enabled.

++ ARCore1.0InsideOutTrackingSkyRoom.apk complex scene with some objects to interact with. Use the touchpad on the headset or the trigger button on your GearVR hand controller to fly through the room. The GearVR hand controller transforms into a wooden bat and allows you to interact with the objects in the room.


You have three options to get the positional tracking to run on your phone. I listed them in order of ease of use. 

Option 1 (quick option installing the apk):

Download the ARCore1.0InsideOutTrackingBasic.apk or the ARCore1.0InsideOutTrackingSkyRoom.apk with a more interesting scene. Than you have to find out your Device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/. With the apk file and the device id you can sign the apk. How you can sign the apk is explained in this Youtube video https://www.youtube.com/watch?v=Ho1TbQozyO0. I recommend the option where you download the addosig.bat program to sign the apk. You can either follow the link under the Youtube page or download the Add OSIG.zip file containing the addosig.bat program directly from this repository. You also have to download and install the ARCore v1.0.0 app via the Play Store. https://play.google.com/store/apps/details?id=com.google.ar.core.  

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

build and run the .apk file with Unity 2017.3.1f1 or higher. The project just contains a basic cube that you can walk around using Inside Out Tracking.

or

Option 3 (more detailed option):

Follow these steps to get Inside-Out-Tracking working on your S7 and GearVR:


1.) Import the arcore-unity-sdk-v1.0.0.unitypackage into Unity like outlined here: https://developers.google.com/ar/develop/unity/quickstart
      
      - Make sure to install Unity 2017.3.1f1 or higher! Older Unity versions or Unity 2018 don't work!
      
      - Try out the HalloAR app before you continue!
      
2.) Download the "Oculus Utilities for Unity" here: https://developer.oculus.com/downloads/package/oculus-utilities-for-unity-5/

3.) Install the ARCore App on your phone via the Play Store. https://play.google.com/store/apps/details?id=com.google.ar.core

4.) Find out your Device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/

5.) Generate an osig file which you need for your app to run on your GearVr https://dashboard.oculus.com/tools/osig-generator/

6.) Place the osig file under Assets -> Plugins -> Android -> assets. If those folders don't exist add them yourselfe.  

7.) Under Build Settings -> Player Settings -> XR Settings check Virtual Reality Supported and choose Oculus via the plus symbole.

8.) Remove "Canves", "ExampleController", "PointCloud", "Environmental Light" and "EventSystem" from the Hierarchy in Unity so you are left with "ARCore Device" and "Directional light"

9.) Click on "First Person Camera" under "ARCore Device". Click the "add Component" button and add "OVR Camera Rig" and "OVR Manager" to the "First Person Camera"

10.) Add a cube and bring it close to the camera. 

11.) Build and Run. You should now see the cube and the camera feed in the backround. Inside out positional tracking is active after a few seconds. If you get the app to run on your GearVR but you don't get any positional tracking to work try to restart the app.

12.) To improve the experience further you can also do the following steps:
      
      - Go to "First Person Camera" -> "Tracked Pose driver" -> "Tracking Type" and set it from "Rotation and Position" to "Position"
      (makes head movements smoother) also set Update Type to "Update and before Render" (makes ARCore tracking better).
      Uncheck the check mark of "Camera" under "First Person Camera". If the head rotation is 
      not recognised correctly, try to restart the phone, because sometimes the ARCore Preview 2 stops in the background.
      - If you experience flickering corners add "QualitySettings.antiAliasing = 2;" to a Start function of your choice.
      This could look like this:  void Start () { QualitySettings.antiAliasing = 2;}
      - To get better performance click on the "DefaultSessionConfig" and remove the checkmark from "Enable Plane Finding",
      "Match Camera Framerate" and "Enable Light Estimation" 
      - To disable the live camera feed in the background set "Background Material" under "AR Core Background Renderer" to  None
      - Follow Oculus guidlines for VR settings https://developer.oculus.com/documentation/unity/latest/concepts/unity-build-android/ 
      but uncheck "Multithreaded Rendering" and check "GPU Skinning"
 







