# Inside Out Positional Tracking (6DoF) for GearVR using ARCore v1.1.0
ARCore v1.1.0 enabled Inside Out Positional Tracking (six degrees of freedom) for the Galaxy S7, S8, S8+, Note 8. S9 and S9+ probably get supported soon.
      
      
      
      
       - WARNING YOU MIGHT GET SICK: The positional tracking via the camera leads to small movements even when 
       standing still or moving slowly. 
       The reason for that seems to be the low camera frame rate (30fps) which the ARCore app is using. 
       The position changes in the environment therefore can only be updated 30 times per second. 
       Oculus recommends at least 60fps or higer and ARCore does not reach those fps possibly making you sick. 
      
      - This was tested on the Samsung Galaxy S7 but the S8, S8+, Note8 seem to work as well.   
      (https://developers.google.com/ar/discover/#supported_devices)
      
      - Fast movements, featureless white areas and poorly lit areas can affect the quality of tracking severely. 
      When the app starts it takes 3-5 seconds for ARCore to detect a plain at this stage it is best to stay still 
      and only move slightly from left to right. 
      After the tracking is initialised slowly move and look once around your room to allow ARCore to scan the area. 
      Now the tracking should be stable enough for faster head movements. If the vr and real world movement don't
      line up correctly restart the app and keep your head steady when the app is starting up. To better understand 
      how ARCore scans the room you can install the HelloARCore.apk (no signing needed).
         

++ ARCore1.1InsideOutPositionalTrackingGearVRBasic.rar is a working Unity project you can use to build your own app with positional tracking enabled. You have to unzip it before importing it to Unity 2017.3.1f1 or later.

++ ARCore1.1InsideOutPositionalTrackingGearVRBasic.apk simple scene with positional tracking enabled. Should run smoothly on all devices even the S7 and will deliver the overall best performance.

++ ARCore1.1InsideOutPositionalTrackingGearVRWhiteRoom.apk optimesed positional tracking scene which should run smoothly on all devices even the S7.

++ HelloARCore.apk	small non GearVR app which can visualise the scanning process of the ARCore app for you. This apk doesn't need to be signed but the ARCore app must be installed. ARCore v1.0.0 can scan horizontal plains (floor/ceiling) and highly textured vertical plaines (posters/bookshelf) and this app shows you how that lokes like.


You have four options to get positional tracking to run on your phone and GearVR. I listed them in order of ease of use. 

Option 0 (quick option installing via sideloadVR):

There is a positional tracking App available on sidloadVR (http://sideloadvr.com/detail.php?id=11424). Which uses ARCore 1.0.0. and seems to work with S7, S8, S8+, Note8, S9 and S9+. It was not developed by me but you can also download it for free. The installation is easy and the app runs smooth even on the S7 without overheating. The app has the same problem of jittery corners probably caused by low frame rate of the ARCore app.

Option 1 (quick option installing the apk):

Download the ARCore1.0InsideOutTrackingBasic.apk. Than you have to find out your Device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/. With the apk file and the device id you can sign the apk. How you can sign the apk is explained in this Youtube video https://www.youtube.com/watch?v=Ho1TbQozyO0. I recommend the option where you download the addosig.bat program to sign the apk. You can either follow the link under the Youtube page or download the Add OSIG.zip file containing the addosig.bat program directly from this repository. You also have to download and install the ARCore v1.1.0 app via the Play Store. https://play.google.com/store/apps/details?id=com.google.ar.core.  

or

Option 2 (working unity project):

You can directly download or clone the working "ARCoreInsideOutTracking.rar" project to your pc, decopmpress it, add your phone specific osig file to the path ARCoreInsideOutTrackingBasic/Assets/Plugins/Android/assets, change the following player settings in Unity 
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


1.) Import the arcore-unity-sdk-v1.1.0.unitypackage into Unity like outlined here: https://developers.google.com/ar/develop/unity/quickstart
      
      - Make sure to install Unity 2017.3.1f1 or higher! Older Unity versions or Unity 2018 don't work!
      
      - Try out the HalloAR app before you continue!
      
2.) Download the "Oculus Utilities for Unity" here: https://developer.oculus.com/downloads/package/oculus-utilities-for-unity-5/

3.) Install the ARCore App on your phone via the Play Store. https://play.google.com/store/apps/details?id=com.google.ar.core

4.) Find out your Device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/

5.) Generate an osig file which you need for your app to run on your GearVr https://dashboard.oculus.com/tools/osig-generator/

6.) Place the osig file under Assets -> Plugins -> Android -> assets. If those folders don't exist add them yourself.  

7.) Under Build Settings -> Player Settings -> XR Settings check Virtual Reality Supported and choose Oculus via the plus symbol.

8.) Remove "Canvas", "ExampleController", "PointCloud", "Environmental Light" and "EventSystem" from the Hierarchy in Unity so you are left with "ARCore Device" and "Directional light"

9.) Click on "First Person Camera" under "ARCore Device". Click the "add Component" button and add "OVR Camera Rig" and "OVR Manager" to the "First Person Camera"

10.) Add a cube and bring it close to the camera. 

11.) Build and Run. You should now see the cube and the camera feed in the background. Inside out positional tracking is active after a few seconds. If you get the app to run on your GearVR but you don't get any positional tracking to work try to restart the app.

12.) To improve the experience further you can also do the following steps:
      
      - Go to "First Person Camera" -> "Tracked Pose driver" -> "Tracking Type" and set it from "Rotation and Position" to "Position"
      (makes head movements smoother) also set Update Type to "Update and before Render" (makes ARCore tracking better).
      Uncheck the check mark of "Camera" under "First Person Camera". 
      - If you experience flickering corners add "QualitySettings.antiAliasing = 2;" to a Start function of your choice.
      This could look like this:  void Start () { QualitySettings.antiAliasing = 2;}
      - To get better performance click on the "DefaultSessionConfig" and remove the checkmark from "Enable Plane Finding",
      "Match Camera Framerate" and "Enable Light Estimation" 
      - To disable the live camera feed in the background set "Background Material" under "AR Core Background Renderer" to  None
      - Follow Oculus guidlines for VR settings https://developer.oculus.com/documentation/unity/latest/concepts/unity-build-android/ 
      but uncheck "Multithreaded Rendering" and check "GPU Skinning"
 

TODO's and future work:

Regarding the issue with the low frame rates (30fps) of the ARCamera. The issue was shortly discussed in the ARCore developer forum (https://github.com/google-ar/arcore-unity-sdk/issues/34) but it is unclear of Google is working on improving it because it does not seem such a big issue for most AR apps. Interestingly the ARKit from Apple seems to render with higher fps. I found one video showing off positional tracking using ARKit (https://www.youtube.com/watch?v=jrzffJPekRo) and it seems to be less jittery at the corners. I hope the issue can be solved with one of the next updates of ARCore. Hoping that the ARCore developers get a bit more competitive towards ARKit. Therefore I asked more specifically hoping for good news from Google in the future (https://github.com/google-ar/arcore-unity-sdk/issues/141).

I also work on implementing hand tracking into the app using software from Manomotion (https://www.manomotion.com/) and I hope to tell you more about it in the future. Right now Manomotion is working hard on getting their hand tracking software compatible with ARCore 1.0.0. I have some doubts that the S7 can handle the extra load but with a simpler environment it might just work.

The ARCore1.0InsideOutTrackingSkyRoom.apk is a more complex scene with some objects to interact with. This scene is for testing the limits to find out how much the S7 can handle. If you wanne try the app use the touchpad on the headset or the trigger button on your GearVR hand controller to fly through the room. The GearVR hand controller transforms into a wooden bat and allows you to interact with the objects in the room. Due to the additional objects, textures and physics this app runs not as smoothly on the S7. I will work on it to enhance the performance in the near future.


Credit:

I wanna give credit to the following developers which published useful informations I used in realising this project:
+ FusedVR https://www.youtube.com/watch?v=4EWPUdE_kqU
+ Roberto Lopez Mendez https://blogs.unity3d.com/2017/10/18/mobile-inside-out-vr-tracking-now-readily-available-on-your-phone-with-unity/


Interesting Projects:

This developer recorded the head position, safed the location data and used smooth position changes in post processing to end up with a smooth video. I think this could be used for some interesting games where the head tracking is recorded and played back with full 60 fps.
https://www.youtube.com/watch?v=L9VjQKvirxs&feature=em-comments

Daydream Support:

It is possible to create a positional tracking app for daydream devices as well. Check out this video: https://www.youtube.com/watch?v=_yg4urvqoBQ. More informations about the project can be found here: https://bitbucket.org/TobiasPott/noxp.morsvr/





