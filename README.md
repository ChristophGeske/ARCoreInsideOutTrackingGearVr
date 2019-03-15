# Inside Out Positional Head Tracking (standalone 6DoF) for GearVR/Cardboard/Daydream using ARCore v1.6.0
ARCore v1.6.0 enabled Inside Out Positional Tracking (six degrees of freedom) for all ARCore capable devices.


# Introducing Remarks:

- WARNING YOU MIGHT GET SICK: The current versions use interpolation and smoothing functions to cover up imprecise tracking. This leads to relative high latency in response to head motion. This is bad for people who get sick from motion sickness quickly. If you know that you are susceptible to motion sickness, these apps might not be for you jet.
      
- Fast movements, featureless and poorly lit areas can affect the quality of tracking severely. When the app starts up it takes 3-5 seconds for ARCore to detect a plain at this stage it is best to move just a little.   
      
- Your phone might get hot very quickly. Make sure to end the game after using. The Cardboard version seems to stay on even if the phone gets very hot while the GearVR version turns itself off automatically. Therefore, cardboard users should make sure to manually end the app after use.
      
- This project can give you a good taste for the capabilities of the HTC Vive Focus and the Daydream powered Lenovo Mirage Solo. Both devices are very pricy and not available everywhere yet. Because they use dedicated hardware, they can offer better performance and quality tracking. The Oculus Santa Cruz/Quest which is expected for early 2019 will also offer 6DoF tracking and feature two 6DoF tracked hand controllers which should make it a highly desirable device. 
      
- Before installing one of the apps (.apk files) make sure you have installed Google ARCore (Play Store) on your device. Also check if your device is supported on [this page](https://developers.google.com/ar/discover/supported-devices).


# Software Description:

|App (.apk)|Description|Recommended Devices|VR Headset|
|---|---|---|---|
| | | | |
|WhiteIsland|Simple environment. Using nativeARCore for tracking. Noticeable latency due to the used smoothing function. Use the controller touchpad to fly forward in viewing direction and the trigger button to enable/disable visualization of real-world boundaries.|All ARcore + GearVR compatible devices|GearVR|
| | | | |
|BoxyRoomCardboard|App should run on every smartphone with ARCore support. Daydream users need to free the camera maybe like explained [here](https://lauren.vortex.com/2018/05/20/using-googles-daydream-vr-headset-for-augmented-reality-and-positional-tracking-applications) and might need to turn of NFC to avoid Daydream from starting.|All ARcore compatible devices|Cardboard|
| | | | |
|VikingVillage|Interesting because of the high-quality environment which was captured with the Seurat tool. This tool allows for the capture of a (small) area of a high-quality environments which allows for a limited free movement in this captured area.|All ARcore + GearVR compatible devices|GearVR|
| | | | |
|VikingVillageCardboard|Interesting because of the high-quality environment which was captured with the Seurat tool. This tool allows for the capture of a (small) area of a high-quality environments which allows for a limited free movement in this captured area.|All ARcore compatible devices|Cardboard|
| | | | |
|VikingVillageForOculusGO|The Oculus GO has no camera so there is no head tracking possible, but because the high-quality environment scene captured with the Seurat tool could be interesting for Go users as well, I added controller support so that flying through the scene by pressing the touchpad is possible. (I haven't tested the app on the GO myself so please let me know if it works or not.|OculusGo|OculusGo|
| | | | |
|VuforiaTracking(Experimental)|This app uses Vuforia which has an ARCore integration. But the performance of the ARCore integration is very low compared to native ARCore. Therfore this project will be discontinued.|All GearVR capable devices (low tracking quality)|GearVR|
| | | | |
|NativeARCoreTracking(Experimental)| Is not up to date and will be removed soon. Please use WhiteIsland instead. |All ARCore + GearVR capable devices|GearVR|
| | | | |
MovingMarkerTracking|Uses Vuforia Vusion which combines ARCore and Vuforias advanced marker tracking. This combination allows for 6DOF marker tracking in a limited area in front of the camera. The marker can be found [here](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/blob/master/MovingMarkerTracking/King.jpg). Simply open the marker on your pc screen or print it on paper and look at it through the camera when the app is running.|All GearVR capable devices (low tracking quality)|GearVR| 


# Installation on Cardboard:

Works with all phone who support ARCore. Just download the BoxyRoomCardboard.apk or the VikingVillageCardboard.apk (Seurat) and install. 

<p align="center">
<a href="https://youtu.be/EFglp19C8tg"><img width="550" height="300" src="https://user-images.githubusercontent.com/12700187/42138751-a815c4d4-7d82-11e8-90d9-ac537b67ce7b.jpg"></a>
</p>

<p align="center">
Video showing BoxyRoomCardboard.apk in action.
</p>

You need to restart the app when misalignment between the headset and the head movement is observed. 

CardboardVR lags the time warp function that we can use in GearVR and Daydream apps therefore the head rotation is less smooth. If you can please try to use Daydream or GearVR the experience will be noticeably better.


# Installation on GearVR (apps need to be signed first):

Installing on GearVR requires the app to be signed first. Since the current GearVR version is better than the Daydream version the few extra steps to sign it are worth it.

<b>Signing the .apk file:</b>

First. Download the WhiteIsland.apk. or the vikingVillage.apk. 

Second. The quickest option I found is to sign the apk using the free [Injector for GearVR app](https://play.google.com/store/apps/details?id=com.zgsbrgr.gearvr.injector&hl=en).
Just safe the .apk file on your phone, use the Injector app to sign it and install. 
Make sure the app you want to sign and install is not already installed on your phone because in my case this prevents the app from installing.  

If the Injector app doesn't work for you try this alternative:
Find out your device ID [here](https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/). With the .apk file and the device ID you can sign the apk yourself. How you can sign the apk is explained in [this Youtube video](https://www.youtube.com/watch?v=Ho1TbQozyO0). I recommend the option where you download the addosig.bat program to sign the apk. You can either follow the link under the Youtube page or download the Add OSIG.zip file containing the addosig.bat program directly from this repository. 

<p align="center">
<a href="https://youtu.be/HLbtWRxVu04"><img width="450" height="350" src="https://user-images.githubusercontent.com/12700187/42417805-6778b948-8293-11e8-8d00-973239fa4345.png"></a>
</p>

<p align="center">
Video showing WhiteIsland.apk 6DoF GearVR version.
</p>

<p align="center">
<a href="https://youtu.be/WX626Dbj1Cc"><img width="450" height="300" src="https://user-images.githubusercontent.com/12700187/42417779-b72dc812-8292-11e8-87a4-4639223cc6f0.jpg"></a>
</p>

<p align="center">
Video showing VikingVillage (Seurat) 6DoF GearVR version.
</p>

You can use the touchpad on the side of the headset to recenter the view when you notice misalignment.


# Installation on Daydream and iPhone

I do not have access to a Daydream or iPhone so I did not upload a version for those devices yet. 

Daydream user: You can simply switch a cardboard project over to Daydream inside the Unity project ("BoxyRoomChardboard") under the build settings, XR settings replace Cardboard with Daydream. I will upload a version specifically build for Daydream users soon. 

iPhone user: You should look at [this project](https://github.com/andrewnakas/ARKit-Cardboard-VR/blob/master/README.md) first for a quick solution. You can also activate ARKit tracking with vuforia fusion or use the ARFoundation. I will look into ARFoundation maybe I can upload a iPhone version as well in the near future.


# TODO's and future work:

I am also looking into implementing hand tracking using software from [Manomotion](https://www.manomotion.com/) and [uSenseAR](https://en.usens.com/products/usensar/). Unfortunately, the performance requirements are very high. Moving marker tracking looks like it could work more reliable and with better performance since the software is more optimized. Also detecting skin color and thereby the hands seems to be complicated issue not fully solved yet by the developers.

<p align="center">
<a href="https://youtu.be/z7-JSaSOgfU"><img src="https://user-images.githubusercontent.com/12700187/41001778-3c12a10a-6912-11e8-9bc2-c0bc9016022d.png" width="448"></a>
</p>

<p align="center">
Manomotion getting hand tracking via the smartphone camera to work. The tracking is not good enough at this point to replace hand controllers.
</p>

<p align="center">
<a href="https://youtu.be/wdiC7l_Wecg"><img src="https://user-images.githubusercontent.com/12700187/41001899-923ad9c6-6912-11e8-90bd-48f4588c08fb.png" width="448"></a>
</p>

<p align="center">
uSenseAR is also trying to get hand tracking via the smartphone camera to work. Getting access to the software is not easy at this time.
</p>

In the future ARCore might get moving marker tracking like ARKit2 and Vuforia are already able to. [ARKit2 marker tracking](https://www.youtube.com/watch?v=ySYFZwkZoio) seems to be the best at the moment. See [my MovingMarkerTracking project](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/tree/master/MovingMarkerTracking) if you are interested in a working version for Android. 

A multiplayer VR game using ARCores cloud anchors should be possible, but I am currently not working on it. [Here](https://i.imgur.com/ZX9Veen.gifv) is how it could look like.


# Other Interesting Projects:

The iconic shooter game Quake can now be played using the GearVR with the option of using positional tracking. A good introduction video can be found [here](https://youtu.be/acEz98Ol8NI). The game and source code can be downloaded [here](https://github.com/DrBeef/QuakeGVR/releases). After signing the .apk you can use the GearVR controller to activate the positional tracking mode in the game settings. I am very impressed with the performance and tracking quality even on old phones like the S7. The developer didn't use a game engine like Unity to build the game but instead used the Android Studio which seemed to allow him to get a higher performance out of the phone by putting some tasks on different cores. I like the way he managed to give the user a window to the outside by tapping the headset but I think it is better to show the surrounding automatically when you come closer to an obstacle since you might forget to check the surrounding when you immersed in the game (I will start working on this soon since I have an idea already). I think this is excellent work and you should try it out.  

iPhone + ARKit + Cardboard projects can be found [here](https://github.com/andrewnakas/ARKit-Cardboard-VR/blob/master/README.md) and [here](https://github.com/hanleyweng/iOS-ARKit-Headset-View)

Fast Travel Games also experimented with ARCore and GearVR. They put some serious thought in solving many of the little issues but the code they use seems not to be public. Learn more [here](https://community.arm.com/graphics/b/blog/posts/achieving-console-like-experiences-on-mobile-vr-with-apex-construct)

Using Google Tango:
Tango is only available on the Asus Zenfone AR and has better room tracking capabilities than ARCore. Useful links [here](https://community.arm.com/graphics/b/blog/posts/mobile-inside-out-vr-tracking-now-on-your-phone-with-unity) and [here](https://twitter.com/youten_redo/status/921295583180079104)

Cardboard AR (HoloKit): 
They used Tango but ARCore should work as well. Check out this [video](https://youtu.be/Wp21BynNl1k) to learn more.

Kinect + GearVR seems be suited to bring full body movement to the GearVR: Check out the related [reddit post](https://www.reddit.com/r/GearVR/comments/4k78ur/gearvr_positional_tracking/)

ALVR connects the GearVR to the pc and uses head tracking for simulating expensive headsets. Check out the project if you haven't heard about it [here](https://github.com/polygraphene/ALVR).


# Some thoughts about the optimal and future hardware:

1. The top of the list but still somehow speculative since it is not released yet is the Galaxy S10 5G which will feature a hQVGA 3d depth sensor. When we get access to the depth sensing data low latency positional tracking similar to the Asus Zenfone AR could be possible. The ultrawide field of view of the camera as well as the depth sensor could be used for better hand tracking. Compatibility with the GearVR should result in a great VR experience and the 5G feature might allow for streaming high quality data directly from the cloud allowing for experiences outperforming even desktop VR. All this seems possible, but most of the underling software is still not even developed so don't expect these features when the phone is released later this year and it might take many months before these features are available. 

2. The Asus Zenfone AR is supporting Daydream which delivers a good enough VR experience (not quite as good as the GearVR) and in combination with it's highly accurate depth camera and the Google Tango software it allows for excellent positional tracking. 
The disadvantage is that Google stopped developing Tango, that almost no one owns the hardware and that no performance improvements are expected in the future making the development for this device and the Tango platform not very attractive. 

3. The Galaxy  S10 and S10+, S8, S9 , Note8 and so on deliver better performance than the S7 which is interesting for running hand tracking and better looking games. In terms of image quality, field of view and tracking latency they are identical to the S7. The ultrawide field of view camera of the S10 and S10+ could also be interesting for improving hand and marker tracking in the future but this would depend on the tracking software (Manomotion or uSense) and ARcore if they support the wide field of view camera.

4. The Galaxy S7 which is running ARCore and GearVR without immediately overheating is the lowest spec device allowing for positional tracking. For hand-tracking the performance might not be enough but this is still an open research question. In combination with a cheap second hand GearVR for roughly 30$ it might be the best value device for experiencing VR right now.

5. Daydream ready phones are powerful and often also support ARCore. The VR quality with the second generation Daydream View is almost as good as GearVR. Currently, this project is working better on GearVR since resetting the view is somewhat complicated using the Daydream software. This might change in the future if the issue can be solved.

6. ARCore ready phones that do not support GearVR or Daydream work with google cardboard but the experience is noticeably worse because of two factors. The lense quality of cardboard lenses and the inferior cardboard software affecting refrashrate of the display. This project offers a cardboard version but only because it is so accesible and cheap. I do not recommend using it when you have the option to use a GearVR or Daydream View headset instead. The quality is just not good enough.  


# How to Seurat:

What is Seurat? Read the introduction [here](https://developers.google.com/vr/discover/seurat)
In short it can capture a high-quality scene and make it run on a mobile device. The catch only a predefined area looks sharp and correct. In this experience it is a 2x3 meter box between the wooden gate and the stone bridge. This means if you try to cross the bridge you will see holes in the textures and distorted meshes. This is the limitation of this technique, but the area can be made bigger. I am not sure how big the area can get I will try that later for now enjoy the limited area between bridge and wooden gate. ;)
[Here](https://www.reddit.com/r/daydream/comments/8vsdnx/have_give_google_seurat_tool_a_try_using_unity/) you can learn how to capture a high-quality scene.

<p align="center">
<a href="https://youtu.be/CpZ94YDufqk"><img src="https://user-images.githubusercontent.com/12700187/42789551-2fcdf678-8966-11e8-9f7d-95efef814bc2.png" width="448"></a>
</p>

<p align="center">
Video describes how a Searat scene is captured for later use with a mobile VR headset.
</p>

You will need to download [this project](https://github.com/ddiakopoulos/seurat/releases) for the pipeline to transform the captured data. See the reddit post for more details. 
[Here](https://github.com/googlevr/seurat-unity-plugin) you can learn how to bring the captured scene in your unity project.


# Trouble Shooting:

If you lose tracking and the virtual and real-world movement doesn't line up correctly press the touchpad on your GearVR controller or on the side of your GearVR to realign the view. If using the cardboard version there is no convenient solution yet and you need to restart the whole app. Moving slowly also prevents the cameras from becoming misaligned on Daydream devices.

If the positional tracking does not work, please make sure you have installed the ARCore App from the Appstore. 

If you experience very low frames and bad jittering please restart the phone and try again sometimes the phone does things in the background that hurt performance. The app should run smoothly at 60fps.

Everything here was tested on the S7 if you have problems getting it to work please open a new issue and let me know.


# Credit:

I want to give credit to the following developers who published useful information I used in building this project:
+ [FusedVR](https://www.youtube.com/watch?v=4EWPUdE_kqU) did some very erly work in this area.
+ Roberto Lopez Mendez with his [Very first positional tracking project](https://blogs.unity3d.com/2017/10/18/mobile-inside-out-vr-tracking-now-readily-available-on-your-phone-with-unity/)
+ Dimitri Diakopoulos (ddiakopoulos) provided a [simplified Seurat pipeline](https://github.com/ddiakopoulos/seurat/releases)
+ Reddit user st6315 (Jung Yi Hung) provided me with a mesh for the vikingVillage app and posted useful information [here](https://www.youtube.com/watch?v=CpZ94YDufqk&feature=youtu.be).  


# Support this Project:

If you like this work and you want to support the development of free software, please consider a donation via Bitcoin. 
My Bitcoin wallet address is: [15aaSbgsZzwP3mrAGdZm7yvuZbu62f6JY4](https://www.coinbase.com/join/5a7a5c59852a7a06c9329bcf)

<a href="https://www.coinbase.com/join/5a7a5c59852a7a06c9329bcf"><img src="https://user-images.githubusercontent.com/12700187/40888344-38a572f8-6756-11e8-9a93-eedc76f0d676.jpg" width="148"></a>
