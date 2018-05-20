# Inside Out Positional Head Tracking (6DoF) for GearVR using ARCore v1.2.0
ARCore v1.2.0 enabled Inside Out Positional Tracking (six degrees of freedom) for the Galaxy S7, S8, S8+, 
Note 8, S9 and S9+. 
      
       - WARNING YOU MIGHT GET SICK: The positional tracking via the camera leads to small movements even 
       when standing still or moving slowly (major improvements in latest updates). 
      
      - This was tested on the Samsung Galaxy S7 but the S8, S8+, Note8, S9 and S9+ are officially 
      supported as well.(https://developers.google.com/ar/discover/supported-devices)
      
      - Fast movements, featureless white areas and poorly lit areas can affect the quality of tracking
      severely.When the app starts it takes 3-5 seconds for ARCore to detect a plain at this stage it is
      best to stay still and only move slightly from left to right. 
      After the tracking is initialised slowly move and look once around your room to allow ARCore to 
      scan the area. Now the tracking should be stable enough for faster head movements. If the VR and 
      real world movement don't line up correctly restart the app and keep your head steady when the app 
      is starting up. To better understand how ARCore scans the room you can install the HelloARCore.apk 
      (no .apk signing needed for the HalloARCore app).
      
      - This project can give you a good taste for the capabilities of the HTC Vive Focus and the 
      Daydream powered Lenovo Mirage Solo. Both devices are very pricy and not available everywhere 
      yet. Because they use two cameras and dedicated hardware they can offer better tracking and will 
      run with better graphics. A good review can be found here (https://medium.com/@iBrews/standalone
      -vr-a-developers-review-1bb69feb6dcc) The Oculus Santa Cruz which is expected in late 2018 will 
      also offer 6DoF tracking and feature two 6DoF tracked hand controllers which should make it a highly 
      desirable device.    

[![IMAGE ALT TEXT](http://img.youtube.com/vi/LgwdZGWZvXk/0.jpg)](http://www.youtube.com/watch?v=LgwdZGWZvXk "GearVR Positional Tracking (6DoF) Shooter Game")

https://www.youtube.com/watch?v=LgwdZGWZvXk

++ WhiteRoom is a working Unity project you can use to build your own app with positional tracking enabled. You can simply import it to Unity 2018.1.0 or higher.

++ WhiteRoom.apk is an optimised positional tracking scene which should run smoothly on all devices even the S7. See option 1 on how to install.

++ HelloARCore.apk is a small non GearVR app which can help you understand the scanning process of the ARCore app. This apk doesn't need to be signed because it isn`t a GearVR app, but the ARCore app must be installed. ARCore v1.0.0 can scan horizontal plains (floor/ceiling) and highly textured vertical plaines (posters/bookshelf).


You have four options to get positional tracking to run on your phone and GearVR. I listed them in order of ease of use. For all options you have to download and install the ARCore app via the Play Store. https://play.google.com/store/apps/details?id=com.google.ar.core.   

# Option 0 (quick option installing via sideloadVR):

There is a positional tracking App not developed by me available on sidloadVR (http://sideloadvr.com/detail.php?id=11424) which uses ARCore 1.0.0. and seems to work with S7, S8, S8+, Note8. The app has not jet implemented the latest improvements and is therefore still very jittery at the corners. It also doesn't hit 60 fps on the S7 and dose not support the S9 and S9+. I am hoping to be able to publish my app on sidloadVR but the sidload team isn`t responding to my mails and therefore I can only offer you the apk file which you need to sign yourself like I explain under option 1. 

# Option 1 (quick option installing the apk provided in this github repository):

First download the WhiteRoom.apk. Than you have to find out your device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/. With the apk file and the device ID you can sign the apk yourself. How you can sign the apk is explained in this Youtube video https://www.youtube.com/watch?v=Ho1TbQozyO0. I recommend the option where you download the addosig.bat program to sign the apk. You can either follow the link under the Youtube page or download the Add OSIG.zip file containing the addosig.bat program directly from this repository. 

Another option for signing an .apk file is explained in this video https://www.youtube.com/watch?v=UkhA10S9VrY and you don't need to use the terminal for that. 

# Option 2 (working unity project):

You can directly download or clone the working "WhiteRoom" project folder to your pc, add your phone specific osig file to the path WhiteRoom/Assets/Plugins/Android/assets.

Then change the following player settings in Unity 
(
Go to: file -> build settings -> player settings -> other settings

Change the package name to for example "com.unity3d.test"

Uncheck Multithreaded Rendering

Set the Minimum API Level to 7.0 

Set the Target API Level to 7.0

Also go to: file -> build settings -> player settings -> XR settings
check mark virtual reality supported and choose Oculus by using the plus symbol also check mark ARCore supported
), 

build and run the .apk file as an Android project with Unity 2018.1.0 or higher. The project contains a room and a gun to shoot the objects. 

# Option 3 (more detailed way):

Follow these steps to get Inside-Out-Tracking working on your GearVR:


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
      
      - Go to "First Person Camera" -> "Tracked Pose driver" -> "Tracking Type" and set it from "Rotation and Position" to "Position".
      Uncheck the check mark of "Camera" under "First Person Camera". 
      - If you experience flickering corners add "QualitySettings.antiAliasing = 2;" to a Start function of your choice.
      This could look like this:  void Start () { QualitySettings.antiAliasing = 2;}
      - To get better performance click on the "DefaultSessionConfig" and remove the checkmark from "Enable Plane Finding",
      "Match Camera Framerate" and "Enable Light Estimation" 
      - To disable the live camera feed in the background set "Background Material" under "AR Core Background Renderer" to  None
      - Follow Oculus guidelines for VR settings https://developer.oculus.com/documentation/unity/latest/concepts/unity-build-android/ 
      but uncheck "Multithreaded Rendering" and check "GPU Skinning"
      - Useful tip on how to create a low requirement scene:
      https://unity3d.com/how-to/optimize-mobile-VR-games
      https://cgcookie.com/articles/maximizing-your-unity-games-performance
      https://developer.oculus.com/documentation/unity/latest/concepts/unity-single-pass/
      https://developer.oculus.com/blog/tech-note-unity-settings-for-mobile-vr/
      https://sassybot.com/blog/lightmapping-in-unity-5/
      - For the latest improvements in low jittery headtracking please have a look in the project files directly. 
      - Be aware of the fact that multithreaded rendering can't be used limiting the performance dramiticaly.
      - ARCore takes up some part of your available resources limiting your abilities even further.  
 

# TODO's and future work:

The Tracking could be more stable regarding the issue with the low frame rates (30fps) of the ARCamera. The latest updates almost fixes the problem compleatly by making it less noticeable. The issue with low frame rates was shortly discussed in the ARCore developer forum here: (https://github.com/google-ar/arcore-unity-sdk/issues/34) and here: (https://github.com/google-ar/arcore-unity-sdk/issues/141) but it is unclear if Google is working on improving it because it doesn't seems to be such a big issue for most non VR apps.  

I am also working on implementing hand tracking using software from Manomotion (https://www.manomotion.com/) and I hope to tell you more about it in the future.

# Other Interesting Projects:

Fast Travel Games also experiment with ARCore and GearVR. They put some serious thought in solving many of the little issues. I am not sure if they managed to solve all the issues though. I would love to know if their project is public somewhere so we could try out all the improvements they came up with.
https://community.arm.com/graphics/b/blog/posts/achieving-console-like-experiences-on-mobile-vr-with-apex-construct

Daydream Support:

It is possible to create a positional tracking app for daydream devices as well. Check out this video: https://www.youtube.com/watch?v=_yg4urvqoBQ. More informations about the project can be found here: https://bitbucket.org/TobiasPott/noxp.morsvr/

# Trouble Shooting:

If the positional tracking does not work, please make sure you installed the right ARCore App 1.2 at the moment. 
Sometimes the ARCore App seems to crash in the background stoping the positional tracking from working simply force stop ARCore or restart the phone to fix this issue.
I test everything on the S7 if you did everything rigt and it is still not working please open a new issue and let me know.

# Support this Project:

If you like my work and you wanna support me with bitcoins, my bitcoin wallet adress is: 15aaSbgsZzwP3mrAGdZm7yvuZbu62f6JY4

# Credit:

I wanna give credit to the following developers who published useful informations I used in building this project:
+ FusedVR https://www.youtube.com/watch?v=4EWPUdE_kqU
+ Roberto Lopez Mendez https://blogs.unity3d.com/2017/10/18/mobile-inside-out-vr-tracking-now-readily-available-on-your-phone-with-unity/








