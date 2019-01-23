# Inside Out Positional Head Tracking (standalone 6DoF) for GearVR/Cardboard/Daydream using ARCore v1.6.0
ARCore v1.6.0 enabled Inside Out Positional Tracking (six degrees of freedom) for all ARCore capable devices.


# Introducing Remarks:


- WARNING YOU MIGHT GET SICK: The current version uses interpolation and smoothing functions to cover up unprecise tracking. This leads to relative high latency in response to head motion. This is bad for people who get sick from motion sickness quickly. If you know that you are susceptible to motion sickness these apps are not for you jet. I am working very hard on a fix so stay tuned for future updates. 
      
- Fast movements, featureless and poorly lit areas can affect the quality of tracking severely. When the app starts up it takes 3-5 seconds for ARCore to detect a plain at this stage it is best to stay still.   
      
- Your phone might get hot very quickly. Make sure to end the game after using. The Cardboard version seems to stay on even if the phone gets very hot while the GearVR version turns itself off if the phone gets too hot. Therefore Cardboard users should make sure to end the app after use.
      
- This project can give you a good taste for the capabilities of the HTC Vive Focus and the Daydream powered Lenovo Mirage Solo. Both devices are very pricy and not available everywhere yet. Because they use dedicated hardware, they can offer better performance and quality tracking. The Oculus Santa Cruz/Quest which is expected for early 2019 will also offer 6DoF tracking and feature two 6DoF tracked hand controllers which should make it a highly desirable device. 
      
- Before installing one of the apps (.apk files) make sure you have installed Google ARCore (Playstore) on your device. Also check if your device is supported on [this page](https://developers.google.com/ar/discover/supported-devices).
      
- GearVR users can use the touchpad on the side of the headset to recenter the view when you notice misalignment. Cardboard users need to restart the app since there is no finished solution yet.    


# Software Description:


|App (.apk)|Description|Recommended Devices|VR Headset|
|---|---|---|---|
| | | | |
|WhiteIsland|Simple environment. Using nativeARCore for tracking. Noticeable latency due to the used smoothing function. Use the controller touchpad to fly forward in viewing direction and the trigger button to enable/disable visualization of real-world boundaries.|S7|GearVR|
| | | | |
|BoxyRoomCardboard|App should run on every smartphone with ARCore support. Daydream users need to free the camera maybe like explained [here](https://lauren.vortex.com/2018/05/20/using-googles-daydream-vr-headset-for-augmented-reality-and-positional-tracking-applications) and might need to turn of NFC to avoid Daydream from starting.|All ARcore compatible devices|Cardboard|
| | | | |
|VikingVillage|Interesting because of the high-quality environment which was captured with the Seurat tool. This tool allows for the capture of a (small) area of a high-quality environments which allows for a limited free movement in this captured area.|S7|GearVR|
| | | | |
|VikingVillageCardboard|Interesting because of the high-quality environment which was captured with the Seurat tool. This tool allows for the capture of a (small) area of a high-quality environments which allows for a limited free movement in this captured area.|S7|Cardboard|
| | | | |
|VikingVillageForOculusGO|The Oculus GO has no camera so there is no head tracking possible, but because the high quality environment scene captured with the Seurat tool could be interesting for Go users as well, I added controller support so that flying through the scene by pressing the touchpad is possible. (I haven't tested the app on the GO myself so please let me know if it works or not.|OculusGo|OculusGo|
| | | | |
|VuforiaTracking(Experimental)|This app uses Vuforia to combine GearVR and ARCore which might help fix an ARCore bug which leads to low performance when VR support is activated in Unity. This app is experimental since I only tested it with the S7 which as I said works not that well.|All GearVR capable devices, (S6 and S7 with low tracking quality)|GearVR|
| | | | |
|NativeARCoreTracking(Experimental)|The Native ARCore in combination with VR leads to a bug which results in bad performance. Therefore, the S7 cannot handle this combination very well. More capable GearVR devices should have enough performance so that the bug doesn't affect the tracking. Therefor this app requires a more powerful phone. This app is experimental since I only tested it with the S7 which as I said works not that well.|All GearVR capable devices, (S6 and S7 with low tracking quality)|GearVR|
| | | | |
MovingMarkerTracking|Uses Vuforia Vusion which combines ARCore and Vuforias advanced marker tracking. This combination allows for 6DOF marker tracking in a limited area in front of the camera. The marker can be found [here](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/blob/master/MovingMarkerTracking/King.jpg). Simply open the marker on your pc screen or print it on paper and look at it through the camera when the app is running.|All GearVR capable devices, (S6 and S7 with low tracking quality)|GearVR| 

     
# Installation options:

# Daydream and Cardboard Versions:

Works best with daydream ready phones  but It also works with all other phone who support ARCore but quality depends on the phone sensors which are generally better in Daydream ready phones. Just download the BoxyRoomCardboard.apk or the VikingVillageCardboard.apk (Seurat) and install. Also install Google ARCore from the Play Store.   


<a href="https://youtu.be/EFglp19C8tg"><img src="https://user-images.githubusercontent.com/12700187/42138751-a815c4d4-7d82-11e8-90d9-ac537b67ce7b.jpg" width="448"></a>

Video showing BoxyRoomCardboard.apk in action.

# GearVR Versions (apps need to be signed first):

Installing on GearVR requires the app to be signed first. Since the current GearVR version is better than the Daydream version the few extra steps to sign it are worth it.

<b>Signing the .apk file:</b>

First. Download the WhiteIsland.apk. or the vikingVillage.apk. 

Second. The quickest option I found is to sign the apk using the free 'Injector for GearVR' app: 
https://play.google.com/store/apps/details?id=com.zgsbrgr.gearvr.injector&hl=en
Just safe the .apk file on your phone, use the Injector app to sign it and install. 
Make sure the app you wanna sign and install is not already installed on your phone because in my case this prevents the app from installing.  

If the Injector app doesn't work for you try this alternative:
Find out your device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/. With the .apk file and the device ID you can sign the apk yourself. How you can sign the apk is explained in this Youtube video https://www.youtube.com/watch?v=Ho1TbQozyO0. I recommend the option where you download the addosig.bat program to sign the apk. You can either follow the link under the Youtube page or download the Add OSIG.zip file containing the addosig.bat program directly from this repository. 

<a href="https://youtu.be/HLbtWRxVu04"><img src="https://user-images.githubusercontent.com/12700187/42417805-6778b948-8293-11e8-8d00-973239fa4345.png" width="448"></a>

Video showing WhiteIsland.apk 6DoF GearVR version.

<a href="https://youtu.be/WX626Dbj1Cc"><img src="https://user-images.githubusercontent.com/12700187/42417779-b72dc812-8292-11e8-87a4-4639223cc6f0.jpg" width="448"></a>


Video showing VikingVillage (Seurat) 6DoF GearVR version.

# TODO's and future work:

I am currently only focused on improving the tracking quality. The current versions uses smoothing functions to simulate 60fps and reduce tracking errors but this introduces latency which leads to motion sickness. The main problem is that ARCore and VR settings in Unity are not compatible which makes these tricks necessary. I am currently working on creating my own VR camera hopfully this will help solve most of the issues the current versions are suffering from. 

MovingMarkerTracking is a version using the Vuforia Fusion Unity plugin (https://library.vuforia.com/articles/Training/vuforia-fusion-article.html) to combine the power of ARCore and Vuforia marker tracking. But Vuforia Fusion is less convenient than ARCore

ALVR might profit from some good head tracking. Check out the project if you haven't heard about it. https://github.com/polygraphene/ALVR They already have an open issue dealing with this topic: https://github.com/polygraphene/ALVR/issues/8. 

I am currently looking into implementing hand tracking using software from Manomotion (https://www.manomotion.com/) since the latest version supports ARCore. Unfortunately the performance requirements are very high preventing it from being used for VR applications. Moving marker tracking looks like it could work better since the software is more optimized. Also detecting skin color and thereby the hands seems to be complicated and doesn't work very well right now.   

<a href="https://youtu.be/z7-JSaSOgfU"><img src="https://user-images.githubusercontent.com/12700187/41001778-3c12a10a-6912-11e8-9bc2-c0bc9016022d.png" width="448"></a>

uSenseAR (https://en.usens.com/products/usensar/) is also trying to get hand tracking via the smartphone camera to work I try to get access to the software as well but had no luck so far.

<a href="https://youtu.be/wdiC7l_Wecg"><img src="https://user-images.githubusercontent.com/12700187/41001899-923ad9c6-6912-11e8-90bd-48f4588c08fb.png" width="448"></a>

In the future ARCore might get moving marker tracking like ARKit2 and Vuforia are already able to. See here for ARKit2 marker tracking (https://www.youtube.com/watch?v=ySYFZwkZoio) and see my MovingMarkerTracking project if you are interested in a working version for Android. 

A multiplayer VR game using ARCores cloud anchors should be possible, but I am currently not working on it. Here is how it could look like. https://i.imgur.com/ZX9Veen.gifv

The boundary system should be further improved the feature points alone are a good start but it could be better and be activated automatically.

# Other Interesting Projects:

The iconic shooter game Quake can now be played using the GearVR with the option of using positional tracking. A good introduction video can be found here: https://youtu.be/acEz98Ol8NI. The game and source code can be downloaded here: https://github.com/DrBeef/QuakeGVR/releases. After signing the .apk you can use the GearVR controller to activate the positional tracking mode in the game settings. I am very impressed with the performance and tracking quality even on old phones like the S7. The developer didn't use a game engine like Unity to build the game but instead used the Android Studio which seemed to allow him to get a higher performance out of the phone by putting some tasks on different cores. I like the way he managed to give the user a window to the outside by tapping the headset but I think it is better to show the surrounding automatically when you come closer to an obstacle since you might forget to check the surrounding when you immersed in the game (I will start working on this soon since I have an idea already). I think this is excellent work and you should try it out.  

iPhone + ARKit + Cardboard project can be found here: 
https://github.com/andrewnakas/ARKit-Cardboard-VR/blob/master/README.md

Fast Travel Games also experimented with ARCore and GearVR. They put some serious thought in solving many of the little issues but the code they use seems not to be public. 
https://community.arm.com/graphics/b/blog/posts/achieving-console-like-experiences-on-mobile-vr-with-apex-construct

Using Google Tango:
Tango is only available on the Asus Zenfone AR and has better room tracking abilities than ARCore. Useful links here: https://community.arm.com/graphics/b/blog/posts/mobile-inside-out-vr-tracking-now-on-your-phone-with-unity and here: https://twitter.com/youten_redo/status/921295583180079104

Cardboard AR (HoloKit): 
https://youtu.be/Wp21BynNl1k They used Tango but ARCore should work as well. 

Kinect + GearVR seems be suited to bring full body movement to the GearVR: https://www.reddit.com/r/GearVR/comments/4k78ur/gearvr_positional_tracking/
Since Kinect and Leap Motion sensors are quite cheap combining this and ARCore head tracking could result in a cheap immersive VR experience. 


# How to Seurat:

What is Seurat? Read the introduction here: https://developers.google.com/vr/discover/seurat
In short it can capture a high-quality scene and make it run on a mobile device. The catch only a predefined area looks sharp and correct. In this experience it is a 2x3 meter box between the wooden gate and the stone bridge. This means if you try to cross the bridge you will see holes in the textures and distorted meshes. This is the limitation of this technique, but the area can be made bigger. I am not sure how big the area can get I will try that later for now enjoy the limited area between bridge and wooden gate. ;)
Here you can learn how to capture a high-quality scene: 
https://www.reddit.com/r/daydream/comments/8vsdnx/have_give_google_seurat_tool_a_try_using_unity/. Please watch this video if you are interested in how the scene was captured.


<a href="https://youtu.be/CpZ94YDufqk"><img src="https://user-images.githubusercontent.com/12700187/42789551-2fcdf678-8966-11e8-9f7d-95efef814bc2.png" width="448"></a>


You will need to download this project (https://github.com/ddiakopoulos/seurat/releases) for the pipeline to transform the captured data. See the reddit post for more details. 
Here you can learn how to bring the captured scene in your unity project:
https://github.com/googlevr/seurat-unity-plugin

# Trouble Shooting:

If you lose tracking and the virtual and real-world movement doesn't line up correctly press the touchpad on your GearVR controller or on the side of your GearVR to realign the view. If using the daydream version there is no convenient solution yet and you need to restart the whole app. Automatically detecting the angle of the offset would be a possible solution. Moving slowly also prevents the cameras from becoming misaligned on Daydream devices.

If the positional tracking does not work, please make sure you have installed the ARCore App from the Appstore. 

If you experience very low frames and bad jittering please restart the phone and try again sometimes the phone does things in the background that hurt performance. The app should run smoothly at 60fps.

Everything here was tested on the S7 if you have problems getting it to work please open a new issue and let me know.

# Credit:

I want to give credit to the following developers who published useful information I used in building this project:
+ FusedVR https://www.youtube.com/watch?v=4EWPUdE_kqU
+ Roberto Lopez Mendez https://blogs.unity3d.com/2017/10/18/mobile-inside-out-vr-tracking-now-readily-available-on-your-phone-with-unity/
+ Dimitri Diakopoulos (ddiakopoulos) provided a simplified Seurat pipeline (https://github.com/ddiakopoulos/seurat/releases)
+ Reddit user st6315 (Jung Yi Hung) provided me with a mesh for the vikingVillage app and posted useful information here (https://www.youtube.com/watch?v=CpZ94YDufqk&feature=youtu.be).  

# Support this Project:

If you like this work and you want to support the development of free software, please consider a donation via Bitcoin. 
Bitcoin wallet address: 15aaSbgsZzwP3mrAGdZm7yvuZbu62f6JY4

<a href="https://www.coinbase.com/join/5a7a5c59852a7a06c9329bcf"><img src="https://user-images.githubusercontent.com/12700187/40888344-38a572f8-6756-11e8-9a93-eedc76f0d676.jpg" width="148"></a>
