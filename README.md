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
      yet. Because they use dedicated hardware they can offer better performance. A good review can be 
      found here (https://medium.com/@iBrews/standalone-vr-a-developers-review-1bb69feb6dcc) The Oculus 
      Santa Cruz which is expected in 2019 will also offer 6DoF tracking and feature two 6DoF 
      tracked hand controllers which should make it a highly desirable device. Interestingly the HTC 
      Vive Focus only uses one camera for 6DoF tracking while the Mirage Solo uses booth cameras for 
      positional tracking.    

[![IMAGE ALT TEXT](https://user-images.githubusercontent.com/12700187/40888099-8e625070-6752-11e8-81f8-a38fba206351.png)](http://www.youtube.com/watch?v=LgwdZGWZvXk "GearVR Positional Tracking (6DoF) Shooter Game")

++ WhiteIsland is a working Unity project you can use to build your own app with positional tracking enabled. You can simply import it to Unity 2018.1.0 or higher.

++ WhiteIsland.apk is an optimised positional tracking scene which should run smoothly on all devices even the S7. See option 1 on how to install. Use the controller touchped to fly forward. (Will contain the latest updates, Head tracking should be even smoother since latest update)

++ WhiteRoomShootingColours.apk is the game you can see in the video. (It will not be updated any longer)  

++ HelloARCore.apk is a small non GearVR app which can help you understand the scanning process of the ARCore app. This apk doesn't need to be signed because it isn`t a GearVR app, but the ARCore app must be installed. ARCore v1.0.0 can scan horizontal plains (floor/ceiling) and highly textured vertical plaines (posters/bookshelf).


You have three options to get positional tracking to run on your phone and GearVR. I listed them in order of ease of use. For all options you have to download and install the ARCore app via the Play Store. https://play.google.com/store/apps/details?id=com.google.ar.core.   

# Option 0 (quick option installing via sideloadVR):

There is a positional tracking App not developed by me available on sidloadVR (http://sideloadvr.com/detail.php?id=11424) which uses ARCore 1.0.0. and seems to work with S7, S8, S8+, Note8. The app has not jet implemented the latest improvements and is therefore still very jittery at the corners. It also doesn't hit 60 fps on the S7 and dose not support the S9 and S9+. I am hoping to be able to publish my app on sidloadVR too but the sidload team isn`t responding to my mails and therefore I can only offer you the apk file which you need to sign yourself like I explain under option 1. 

# Option 1 (quick option installing the apk provided in this github repository):

First download the WhiteIsland.apk. Then you have to find out your device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/. With the apk file and the device ID you can sign the apk yourself. How you can sign the apk is explained in this Youtube video https://www.youtube.com/watch?v=Ho1TbQozyO0. I recommend the option where you download the addosig.bat program to sign the apk. You can either follow the link under the Youtube page or download the Add OSIG.zip file containing the addosig.bat program directly from this repository. 

Another option for signing an .apk file is explained in this video https://www.youtube.com/watch?v=UkhA10S9VrY and you don't need to use the terminal for that. 

# Option 2 (more detailed way):

Follow these steps to get Inside-Out-Tracking working on your GearVR:


1.) Import the arcore-unity-sdk-v1.2.0.unitypackage into Unity like outlined here: https://developers.google.com/ar/develop/unity/quickstart
      
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
      https://www.youtube.com/watch?v=N0zr0Eqh6ac
      - Use the Oculus Metrics Tool to see your FPS in game.
      - For the latest improvements in low jittery headtracking please have a look in the project files directly. 
      - Be aware of the fact that multithreaded rendering can't be used limiting the performance dramiticaly.
      - ARCore takes up some part of your available resources limiting your abilities even further.  
 

# TODO's and future work:

The tracking could be more stable regarding the issue with the low frame rates (30fps) of the ARCamera. The latest updates almost fixes the problem completely by making it less noticeable. The issue with low frame rates was shortly discussed in the ARCore developer forum here: (https://github.com/google-ar/arcore-unity-sdk/issues/34) and here: (https://github.com/google-ar/arcore-unity-sdk/issues/141) but it is unclear if Google is working on improving it because it doesn't seems to be such a big issue for most non VR apps.  

I am currently working on implementing hand tracking using software from Manomotion (https://www.manomotion.com/) and I hope to tell you more about it in the future.

[![IMAGE ALT TEXT](https://user-images.githubusercontent.com/12700187/40888289-617a2404-6755-11e8-9a27-5111db94294a.png)](https://youtu.be/z7-JSaSOgfU "Demo Day Pitch - AI Category - ManoMotion"=

uSenseAR (https://en.usens.com/products/usensar/) is also trying to get hand tracking via the smartphone camera to work I try to get access to the beta at the moment.

[![IMAGE ALT TEXT](https://user-images.githubusercontent.com/12700187/40888223-27de9df2-6754-11e8-854a-d06d5c9fde83.png)](https://youtu.be/wdiC7l_Wecg "Single RGB Hand/Face Tracking on Smart Phone with uSensAR")

In the future ARCore agumented image might be better in tracking a moving marker for example on your hand to allow for 6DoF hand tracking. I tried this but recognition is to slow to be paractical at this stage. On the google developer phage it says: "ARCore cannot track a moving image, but it can resume tracking that image after it stops moving." 

Using mashine learning and mobile tensor flow might be a path to get 6DoF hand tracking working. This is an interesting video https://youtu.be/9KqNk5keyCc and I will try if something useable comes out of it when I find some time.

# Other Interesting Projects:

Fast Travel Games also experiment with ARCore and GearVR. They put some serious thought in solving many of the little issues. I am not sure if they managed to solve all the issues. I would love to know if their project is public somewhere so we can try out all the improvements they came up with.
https://community.arm.com/graphics/b/blog/posts/achieving-console-like-experiences-on-mobile-vr-with-apex-construct

Daydream Support:

It is possible to create a positional tracking app for daydream devices as well. Check out this video: https://www.youtube.com/watch?v=_yg4urvqoBQ. More informations about the project can be found here: https://bitbucket.org/TobiasPott/noxp.morsvr/



# No GearVR => No problem:

[![IMAGE ALT TEXT](https://user-images.githubusercontent.com/12700187/40882299-a1473fcc-66dd-11e8-8246-87fe5224c136.png)](https://youtu.be/HnfqYk2YqA8 "How to Install GearVr Apps Without a GearVR Headset.")

You can use this app (and all the other GearVR apps as well) without ever touching a GearVR headset but you need at least a Galaxy S6 or higher for this trick. You can put the phone in the daydream headset, Google cardboard or any other headset. Check out laurenweinstein1's website to learn how to modify your daydream headset for use with the camera https://lauren.vortex.com/2018/05/20/using-googles-daydream-vr-headset-for-augmented-reality-and-positional-tracking-applications.


# Trouble Shooting:

Loosing tracking is a problem you will experience when the room is not tracked completely and you moved your head to fast. Try to make slow movements and hold your head still for the first few seconds until ARCore has detected the surrounding.

If the positional tracking does not work, please make sure you installed the right ARCore App (ARCore 1.2). 

Sometimes the ARCore App seems to crash in the background stoping the positional tracking from working (in my case even the regular Camera freezes) restart the phone to fix this issue.

If you experience very low frames and bad jittering please restart the phone and try again sometimes the phone does things in the background that hurt performance. The app should run smoothly at 60fps.

I have tested everything on the S7 if you have problems getting it to work please open a new issue and let me know.

Overheating on the S7 might be an issue. Sticking wet toilet paper to the back of the phone is an effective, cheap and simple solution.

# Support this Project:

If you like this work and you wanna support the development of free software, please consider a donation via Bitcoin. 
Bitcoin wallet adress: 15aaSbgsZzwP3mrAGdZm7yvuZbu62f6JY4

![bitcoin_accepted_here_sign_horizontal2](https://user-images.githubusercontent.com/12700187/40888327-e57f15de-6755-11e8-99d6-26532374f600.png)

# Credit:

I wanna give credit to the following developers who published useful informations I used in building this project:
+ FusedVR https://www.youtube.com/watch?v=4EWPUdE_kqU
+ Roberto Lopez Mendez https://blogs.unity3d.com/2017/10/18/mobile-inside-out-vr-tracking-now-readily-available-on-your-phone-with-unity/








