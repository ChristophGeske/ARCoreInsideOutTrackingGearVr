# Inside Out Positional Head Tracking (standalone 6DoF) for GearVR/Cardboard/Daydream using ARCore v1.6.0
ARCore v1.6.0 enabled Inside Out Positional Tracking (six degrees of freedom) for all ARCore capable devices.


# Introducing Remarks:

- WARNING YOU MIGHT GET SICK: The current versions use interpolation and smoothing functions to cover up imprecise tracking. This leads to relative high latency in response to head motion. This is bad for people who get sick from motion sickness quickly. If you know that you are susceptible to motion sickness, these apps might not be for you jet.
      
- Fast movements, featureless, extrem sunny and poorly lit areas can affect the quality of tracking severely. When the app starts up it takes 3-5 seconds for ARCore to detect a plain at this stage it is best to move just a little.   
      
- Your phone might get hot very quickly. Make sure to end the game after using. The Cardboard version seems to stay on even if the phone gets very hot while the GearVR version turns itself off automatically. Therefore, cardboard users should make sure to manually end the app after use.
      
- The goal of this project is to bring high quality (6DoF) positional tracking to GearVR, Daydream and Cardboard using ARCore/ARKit software. The HTC Vive Focus and the Daydream powered Lenovo Mirage Solo offer this functionality already but are very pricy and not available everywhere yet. Because they use dedicated hardware, they can offer better performance and quality tracking. The Oculus Santa Cruz/Quest currently offers the best 6DoF head and controller tracking which makes it a highly desirable device. 
      
- Before installing one of the apps (.apk files) make sure you have installed Google ARCore from the [Play Store](https://play.google.com/store/apps/details?id=com.google.ar.core&hl=en) on your device. Check if your device supports ARCore on [this page](https://developers.google.com/ar/discover/supported-devices).

- This project was part of my bachelor thesis. The thesis describes the challanges of bringing mobile VR to smartphones in detail and describes some experiments done to test the performance and other factors. It can be found [here](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/files/4120231/BachelorThesis_ChristophGeske.pdf).

- GearVR and Daydream support is officially over what's left for us is the open source Google Cardboard SDK which is said to recive updates in the future and run on the latest versions of the Unity engine. This open source Cardboard SDK is similar to the Daydream SDK which was quite good. How good the Carboard SDK is in delivering responsive VR I don't know yet since I haven't tested and compared it to the old and phasing out Daydream SDK. But since the Chardboard SDK is the only mobile phone VR solution left, we have no other choice to use it for all future projects. One unfortunate fact is that the carboard SDK doesn’t support a controller input. I hope the VR performance regarding latency and performance is on pair with what we are used to from Daydream VR, because I consider the Daydream and Gear VR solutions the lowest acceptable quality for VR. If the Chardboard SDK shows higher latency because it misses key features that where developed for Daydream developing any project with it would be doomed to be a bad experience making users sick.

# Software Description:

|Projects and Apps (.apk)|Description|Recommended Devices|VR Headset|
|---|---|---|---|
| | | | |
|[WhiteIsland.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/WhiteIsland.apk)|Simple environment. Using nativeARCore for tracking. Noticeable latency due to the used smoothing function. Use the controller touchpad to fly forward in viewing direction and the trigger button to enable/disable visualization of real-world boundaries.|All ARcore + GearVR compatible devices|GearVR|
| | | | |
|[BoxyRoomCardboard.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/BoxyRoomCardboard.apk)|App should run on every smartphone with ARCore support. Daydream users need to uncover the camera e.g. as explained [here](https://lauren.vortex.com/2018/05/20/using-googles-daydream-vr-headset-for-augmented-reality-and-positional-tracking-applications) and might need to turn of NFC to avoid Daydream from starting on top of the Cardboard app.|All ARCore compatible devices|Cardboard|
| | | | |
|[VikingVillageGearVR.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/VikingVillageGearVR.apk)|Interesting because of the high-quality environment which was captured with the Seurat tool. This tool allows for the capture of a (small) area of a high-quality environment which allows for a limited free movement only in this captured area.|All ARCore + GearVR compatible devices|GearVR|
| | | | |
|[VikingVillageCardboard.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/VikingVillageCardboard.apk)|Interesting because of the high-quality environment which was captured with the Seurat tool. This tool allows for the capture of a (small) area of a high-quality environments which allows for a limited free movement only in this captured area.|All ARCore compatible devices|Cardboard|
| | | | |
|[VikingVillageForOculusGO.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/VikingVillageForOculusGO.apk)|The Oculus GO has no camera so there is no head tracking possible, but because the high-quality environment scene captured with the Seurat tool could be interesting for Go users as well, I added controller support so that flying through the scene by pressing the touchpad is possible.|OculusGo|OculusGo|
| | | | |
|VuforiaTracking(Experimental)|This app uses Vuforia which has an ARCore integration for tracking. But the performance of the ARCore integration is very poor compared to native ARCore. Therfore this project will be discontinued.|All GearVR capable devices (low tracking quality)|GearVR|
| | | | |
|NativeARCoreTracking(Experimental)| Is not up to date and will be removed soon. Please use WhiteIsland instead. |All ARCore + GearVR capable devices|GearVR|
| | | | |
[MovingMarkerTracking.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/MovingMarkerTracking/MovingMarkerTracking.apk)|Uses Vuforia Vusion which combines ARCore and Vuforias marker tracking. This combination allows for 6DOF marker tracking in a limited area in front of the camera. The marker can be found [here](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/blob/master/MovingMarkerTracking/King.jpg). Simply open the marker on your pc screen or print it on paper and look at it through the camera when the app is running. Since ARCore 1.9 now supports moving marker tracking this project will likely be replaced soon since Vuforia Marker tracking has many issues.|All GearVR capable devices (low tracking quality)|GearVR| 


# Installation 

### On Cardboard

Works with all phones who support ARCore. Just download the BoxyRoomCardboard.apk or the VikingVillageCardboard.apk (Seurat) and install. 

<p align="center">
<a href="https://youtu.be/EFglp19C8tg"><img src="https://user-images.githubusercontent.com/12700187/54432048-5e6d5c00-4728-11e9-82b8-994b5ab7114b.png" width="100%"></a>
</p>

<p align="center">
Video showing BoxyRoomCardboard.apk in action.
</p>

You need to restart the app when misalignment between the headset and the head movement is observed. The misaligning is a bug which was solved for the GearVR version but is still present in the Cardboard/Daydream versions. 

Cardboard VR apps lag the time warp functionalities that we can use in GearVR and Daydream apps. The Cardboard headset is not very good either if you can use a GearVR or Daydream headset to get a noticeably improved experience.


### On GearVR (apps need to be signed first):

Installing on GearVR requires the app to be signed first. But since the current GearVR version is better than the Daydream/Cardboard version the few extra steps to sign the apk file are worth it for you.

<b>Signing the .apk file:</b>

First. Download the WhiteIsland.apk. or the VikingVillage.apk. 

Second. Sign the apk using the free [Injector for GearVR app](https://play.google.com/store/apps/details?id=com.zgsbrgr.gearvr.injector&hl=en).
Just safe the .apk file on your phone, use the Injector app to sign it and install. 
Make sure the app you want to sign and install is not already installed on your phone because in my case this prevents the app from installing again.  

If the Injector app doesn't work for you try this alternative:
Find out your device ID [here](https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/). With the .apk file and the device ID you can sign the apk yourself. How you can sign the apk is explained in [this Youtube video](https://www.youtube.com/watch?v=Ho1TbQozyO0). I recommend the option where you download the addosig.bat program to sign the apk. You can either follow the link under the Youtube page or download the Add OSIG.zip file containing the addosig.bat program directly from this repository. 

<p align="center">
<a href="https://youtu.be/HLbtWRxVu04"><img src="https://user-images.githubusercontent.com/12700187/54431979-26feaf80-4728-11e9-9045-6ac215b8c5bc.png" width="100%"></a>
</p>

<p align="center">
Video showing WhiteIsland.apk 6DoF GearVR version.
</p>

<p align="center">
<a href="https://youtu.be/WX626Dbj1Cc"><img src="https://user-images.githubusercontent.com/12700187/54431833-b8b9ed00-4727-11e9-9c3c-911e16503dd6.png" width="100%"></a>
</p>

<p align="center">
Video showing VikingVillage (Seurat) 6DoF GearVR version.
</p>

Click the touchpad on the side of the headset once to recenter the view when you notice misalignment.


### On Daydream and iPhone

Daydream users: You can either use one of the Cardboard apks or even better switch the Cardboard project over to Daydream inside the Unity project yourself. Use for example the "BoxyRoomChardboard" project and under build settings, XR settings replace Cardboard with Daydream. I will upload a version specifically build for Daydream users as soon as I get access to a Daydream device and headset to test it. 

iPhone user: You should look at [this project](https://github.com/andrewnakas/ARKit-Cardboard-VR/blob/master/README.md) first for a quick solution. Unfortunately only Cardboard VR, which lags some critical features like reprojection runs on iphones making iphones not well suited for mobile VR. However [most iPhones](https://developers.google.com/ar/discover/supported-devices) support ARCore so you can try to download the cardboard project, switch it to IOS and install it on your device if you interested in this project. If I find the time, I will test and upload an iphone cardboard version as well.

# TODO's and future work:

### Improve Head Tracking 

The current tracking version you find in this repository suffers from lag, jumps in position, drift from tracking being lost and other inconveniences. Fixing these issues seems very doable, and I am currently working on that most of the time. The most promising solution seems to use a filter which detects tracking errors and removes them. For filtering errors the kalman filter [(video lecture here)](https://www.youtube.com/playlist?list=PLX2gX-ftPVXU3oUFNATxGXY90AULiqnWT) and [block post here](https://www.bzarg.com/p/how-a-kalman-filter-works-in-pictures/) looks like a good start and an Unity implementation can also be found [here](https://github.com/zalo/MathUtilities). I don't think we reach the same quality as dual camera setups in extreme situations but I predict a future version could offer almost perfect head tracking when just performing normal head movements. Since ARCore and phones get improved all the time, the tracking should constantly get better. 

The new ARCore version [(v1.11.0)](https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.11.0.1) now called "Google Play Services for AR" promises to support 60fps tracking which would be great for a smoother head tracking. The previous ARCore versions which are still used in this project use the 30fps tracking which in reality are not even real 30fps since there are a lot of tracking errors even with perfect lighting and environment condition. My first test showed that older devices like the S7 can not yet make use of the new 60fps feature and Google lists only the Pixel 2 and Pixel 3 as 60fps compatible on the [supported device page](https://developers.google.com/ar/discover/supported-devices). Also, the new ARCore supports build in depth cameras which might be interesting for the S10 5G and should improve tracking even more. 

### Hand Tracking 

Hand and finger tracking seems to be the easiest way to bring 6DoF input to phone-based VR systems, luckily ARKit which is ahead of ARCore in many aspects recently announced full body tracking and joint detection which can also be used for hand (but not finger) tracking without the need to use special external software allowing for high performance hand tracking on phones supporting ARKit 3 right now.

<p align="center">
<a href="https://youtu.be/YnvoSJkmMjs"><img src="https://user-images.githubusercontent.com/12700187/67143663-02465980-f26e-11e9-9bff-a5e64dbc6ad2.png" width="100%"></a>
</p>
<p align="center">
Video showing ARKit 3 hand tracking. 
</p>

Google recently released an apparently extremely fast and robust mono camera hand and finger tracking solution. The open-source code can be found [here](https://github.com/google/mediapipe) and a blog post [here](https://ai.googleblog.com/2019/08/on-device-real-time-hand-tracking-with.html).

<p align="center">
<a href="https://github.com/google/mediapipe"><img src="https://user-images.githubusercontent.com/12700187/63429941-b157e780-c41b-11e9-9e77-15dcfd08b0d9.gif" width="20%"></a>
</p>
<p align="center">
Robust and quick mono camera hand tracking developed by Google. 
</p>

Hand and finger tracking software is also available from [Manomotion](https://www.manomotion.com/) and [uSenseAR](https://en.usens.com/products/usensar/) ([video](https://youtu.be/wdiC7l_Wecg)) and works in combination with VR and ARCore. Unfortunately, the performance requirements are very high and the software is not free of charge. 

<p align="center">
<a href="https://youtu.be/z7-JSaSOgfU"><img src="https://user-images.githubusercontent.com/12700187/54431726-72fd2480-4727-11e9-8719-322ee3dae8f0.png" width="100%"></a>
</p>
<p align="center">
Video showing Manomotion hand and finger tracking in combination with ARCore. 
</p>

Another open-source project for hand gesture recognition on ARKit can be found [here](https://github.com/hanleyweng/Gesture-Recognition-101-CoreML-ARKit).

### Moving Marker Tracking

ARCore version 1.9 now supports moving marker tracking. Read more about it [here](https://developers.google.com/ar/develop/unity/augmented-images/). 
The MovingMarkerTracking app you find in this project is currently using an older version based on Vuforia marker tracking. I am now looking into ARCore based moving marker tracking which should be a much better solution. The interesting questions moving marker tracking opens up are: 

1) Can we use it to further improve head tracking? 

2) Can we add a marker to your hands/elbows/feets/controller to get some sort of hand/body/controller tracking? 

But even if it works the limited field of view of the camera will probably make it less useful for many applications. 

The company ZapBox already sells kits based on this idea as you can see in [this video](https://www.youtube.com/watch?v=SMyPTfuy8Ms). Notice that they use a clip-on-lens to achive a greater field of view for the see-through image and the area where the moving markers can be tracked also gets bigger. 
I tested multiple wide angle clip-on-lenses but non of them worked with ARCore either stoping tracking all together or drifting heavily. The same seems to be true for ARKit as you can read [here](https://forum.zap.works/t/will-zapworks-integrate-arkit-capabilities-for-ios/867). 
Since some of the latest smartphones like the S10 have a dedicated wide field of view camera we can only hope that the ARCore team adds support for these cameras as well.

### Markerless Controller Tracking

Google clearly thought about solutions to 6DOF controller tracking as you can read in this [roadToVR article](https://www.roadtovr.com/google-mobile-6dof-vr-controller-tracking-inside-out/). But since thay use stereo cameras the results may not be directly transferable to mobile phones with only a single camera.

<p align="center">
<a href="https://arxiv.org/pdf/1804.05870.pdf"><img src="https://user-images.githubusercontent.com/12700187/60089175-371f2580-9740-11e9-8f86-cbdb5794c673.png" width="100%"></a>
</p>
<p align="center">
      Images from a [paper](https://arxiv.org/pdf/1804.05870.pdf) published by Google describing markerless controller tracking.
</p>

### Environmental Understanding, 3D Scanning And Boundary System

ARCore finaly supports depth maps. This means a 3D depth mesh of your sourounding is generated which can be used for exciting new features. Read about it in [this article](https://medium.com/@riteshkanjee/how-googles-arcore-depth-api-tackles-occlusion-with-a-single-camera-6cbd8d23708b). 

<p align="center">
<a href="https://www.youtube.com/watch?v=VOVhCTb-1io"><img src="https://user-images.githubusercontent.com/12700187/93025713-98ab0880-f600-11ea-9a04-d14f99451b2e.jpg" width="100%"></a>
</p>
<p align="center">
      Introduction video and some early apps developed with the new ARCore depth API 
</p>

Also be aware that many phones do not support this feature yet. If your phone supports this new feature can be foun on the ARCore supported devices page. When your phone supports the new depth API this feature should work. For example the Snapdragon version of the S10 supports the new depth API but the international Exynos version doesn't. Why there are these differences even on phones with the same name I dont know, maybe this feature requires a special chip architecture.

<p align="center">
<a href="https://www.youtube.com/watch?v=sN61K-6Ai1c"><img src="https://user-images.githubusercontent.com/12700187/54491273-6ad6ed80-48bd-11e9-8978-fdec337e10f0.png" width="100%"></a>
</p>

Two 3D Scanner projects using Google Tango [Open Constructor](https://github.com/lvonasek/tango/wiki/Open-Constructor) and the [master thesis Tango-Poc](https://github.com/stetro/project-tango-poc#pointcloud-app-pc) are available as open source projects. I also recommend this informative [video](https://youtu.be/1TF7esI3sMQ) about Tango 3D scanning technology. 

An [ARCore version](https://github.com/lvonasek/tango/wiki/3D-Scanner-for-ARcore) is also available (not open source) and a plugin for Unity can be bought [here](https://assetstore.unity.com/packages/tools/integration/3d-reconstruction-for-arcore-android-only-136919). It could be used to further improve the boundary system and automatically building a virtual environment which fits perfectly over your real environment. This is incredible, and I will try to implement something similar when the tracking works well. I recommend reading the [related paper here](https://scss.tcd.ie/publications/theses/diss/2018/TCD-SCSS-DISSERTATION-2018-035.pdf) and [here](https://dl.acm.org/citation.cfm?id=3275041) (use [scihub.tw](https://sci-hub.tw/) to get free access). Also download the [free 3D scanning app](https://play.google.com/store/apps/details?id=com.lvonasek.arcore3dscanner&hl=en_US) to check it out directly. 

Another idea for the boundary system could be a kind of edge detection and visualisation algorithm as shown in this [video](https://youtu.be/aVdWED6kfKc).

Object detection/classification using the camera feet could allow for some unique game and application mechanics. Two projects using Google tensor flow as the underling machine learning software are available [here](https://github.com/Syn-McJ/TFClassify-Unity) and [here](https://github.com/MatthewHallberg/TensorFlowUnity-ImageClassification).

### 6DOF Images, Videos and Light Fields  

360° and 180° photos and videos are nice but with additional depth information they become really interesting. Positional tracking on GearVR would be a natural fit since no hand controllers are needed, mobile phones can display high resolution images and since the head movements are rather slow we have no problems with camera motion blur and loss of tracking. There are three distinct ways of approaching this idea. 
1.) Image overlayed on a depth map either by capturing the depth map separately or by using software to recover a point cloud from a stereoscopic video [(seen here)](https://www.youtube.com/watch?v=HSXMs2wnNc4) and further discussed [here](https://www.reddit.com/r/6DoF/). There is already an [app](https://www.oculus.com/experiences/go/1790370494337451/) available on the Oculus store for GearVR which displays images and videos with additional 3D depth information. 
2.) Photogrametry allows for higly realistic but static 3D models. 
3.) Using [a light field](https://www.blog.google/products/google-ar-vr/experimenting-light-fields/) capture of the environment seems to be the holy grail of 3D photography. A demo project [(video)](https://www.youtube.com/watch?v=a-JX3ZPi720) was published by Google for free but is only running on desktop pc VR headsets. 
Using the seurat pipeline and [this github light field project](https://github.com/PeturDarri/Fluence-Unity-Plugin/tree/master/Assets) it might be possible to bring the same experience to mobile VR. The same is true for bringing very detailed photogrametry models on the phone in VR.

### Further Ideas Worth Exploring 

To further increase the reach, it makes sense to build apps that run on Android and iOS devices. The [AR Foundation github project](https://github.com/Unity-Technologies/arfoundation-samples) brings this functionality to Unity. 

Using VR outside or in a large play space is a lot of fun and feels very different from games experienced inside where the ability to move freely is limited. Here accessing  [3D maps](https://cloud.google.com/maps-platform/gaming/) might be interesting and result in exiting apps consisting of AR and VR elements.

# Other Interesting Projects:

### Shooter Game Quake 
This iconic shooter game can now be played using the GearVR with the option of using positional tracking. A good introduction video can be found [here](https://youtu.be/acEz98Ol8NI). The game and source code can be downloaded [here](https://github.com/DrBeef/QuakeGVR/releases). After signing the .apk you can use the GearVR controller to activate the positional tracking mode in the game settings. I am very impressed with the performance and tracking quality even on old phones like the S7. The developer didn't use a game engine like Unity to build the game but instead used the Android Studio which seemed to allow him to get a higher performance out of the phone by putting some tasks on different cores. I like the way he managed to give the user a window to the outside by tapping the headset but I think it is better to show the surrounding automatically when you come closer to an obstacle since you might forget to check the surrounding when you immersed in the game (I will start working on this soon since I have an idea already). I think this is excellent work and you should check it out.  

### iPhone + ARKit + Cardboard 
Such a project can be found [here](https://github.com/andrewnakas/ARKit-Cardboard-VR/blob/master/README.md) and [here](https://github.com/hanleyweng/iOS-ARKit-Headset-View). A paper comparing ARCore and ARKit can be found [here](https://link.springer.com/chapter/10.1007/978-3-030-19501-4_36) (use [sci-hub.tw](https://sci-hub.tw/) if you don't have access to the full text). Most important findings of the paper for this project are that ARKit tracks with 60fps, can deal with faster movements and plain detection is more accurate indoors. ARCore seems to be better outdoors. Also noticeable is the finding that the performance drops on some older Android phones like Nexus 6P and Nexus 5X. According to @lvonasek ARKit is more accurate because of a better camera calibration process by Apple.  

<p align="center">
<a href="https://www.youtube.com/watch?v=8d9Qbt3J9Uo"><img src="http://i.imgur.com/WedVqFt.gif" width="40%"></a>
</p>
<p align="center">
Positional tracking on iPhone using ARKit + Cardboard. 
</p>

### Fast Travel Games 
They also experimented with ARCore and GearVR. They put some serious thought in solving many of the little issues but the code they use seems not to be public. Learn more [here](https://community.arm.com/graphics/b/blog/posts/achieving-console-like-experiences-on-mobile-vr-with-apex-construct).

<p align="center">
<a href="https://twitter.com/fasttravelgames/status/971791094915780611/photo/1?ref_src=twsrc%5Etfw%7Ctwcamp%5Etweetembed%7Ctwterm%5E971791094915780611&ref_url=https%3A%2F%2Fcommunity.arm.com%2Fdeveloper%2Ftools-software%2Fgraphics%2Fb%2Fblog%2Fposts%2Fachieving-console-like-experiences-on-mobile-vr-with-apex-construct"><img src="https://user-images.githubusercontent.com/12700187/59551307-eeb17c00-8f77-11e9-9881-808c89eaca33.jpg" width="60%"></a>
</p>
<p align="center">
Fast Travel Games showing of simplefied Apex Construct running on GearVR. 
</p>

### Google Tango
Tango is only available on the Asus Zenfone AR and has better room tracking capabilities than ARCore thanks to the [time of flight sensor](https://www.youtube.com/watch?v=OMDfQC0m4i4) build in. Useful links [here](https://community.arm.com/graphics/b/blog/posts/mobile-inside-out-vr-tracking-now-on-your-phone-with-unity), [here](https://twitter.com/youten_redo/status/921295583180079104) and an excellent talk about the underlying technology [here](http://voicesofvr.com/544-google-tangos-engineering-director-on-ar-capabilities-enabled-by-depth-sensors/).
It was also shown in [this video](https://www.youtube.com/watch?v=_tcIezxtjF4) that some form of hand tracking using the depth sensor is possible. The main issue with the now rather old devices is that the chip can't handle all the performance requirements and overheats rather quickly as was mentioned in [this article](https://uploadvr.com/inside-out-google-solve-tracking/). 

### HoloKit, Aryzon, ZapBox (Cardboard AR) 
<a href="https://www.zappar.com/zapbox/"><img src="https://user-images.githubusercontent.com/12700187/59551535-6f25ac00-8f7b-11e9-95e7-fd818d94e14e.jpeg" width="30%"></a> 
<a href="https://holokit.io/"><img src="https://user-images.githubusercontent.com/12700187/59551463-26212800-8f7a-11e9-9cfe-e5f0de8acb69.jpg" width="30%"></a> <a href="https://www.aryzon.com/"><img src="https://user-images.githubusercontent.com/12700187/59551506-fd4d6280-8f7a-11e9-82e3-2f354b76b9f4.jpg" width="30%"></a> 
<p align="center">
ZapBox, HoloKit and Aryzon headsets. 
</p>

Some projects (e.g. [ZapBox](https://www.zappar.com/zapbox/)) use the camera seethrough mode for AR projects but I don't think it is a good approach to use a mono camera and add 3D objects on top of the 2D camera image. This is guaranteed to give you a bad experience. The HoloKit project builds on a much better idea but would profit from a better headset design and some improvements we already see in the Daydream and GearVR headsets like low persistency screen mode and smooth head tracking. It would also profit from smoother positional head tracking which is the mission of this project. 

[This introductory video](https://www.youtube.com/watch?v=ao8Gb9yDrBM), the [github project site](https://github.com/holokit/holokitsdk) and the [HoloKit website](https://holokit.io/) can give you a good overview over the HoloKit project. You can also buy the headset from thair website but more on that down below.

There is also a company called [Aryzon](https://www.aryzon.com/) which is selling almost the same product with thair own SDK which is also open source. A good review of this headset can be found [here](https://skarredghost.com/2018/07/04/aryzon-ar-headset-2-0-unboxing-and-review/) and [here](https://www.youtube.com/watch?v=bJd3Nwp6cYo). I only found an [older video](https://www.youtube.com/watch?v=EFDl5Nyah3Y) showing a view through the headset with the tracking looking terrible but tracking should be much better with ARCore. 

There are many possibilities that such a headset would allow for some examples [here (markers)](https://www.youtube.com/watch?v=8fNdM0q7gUc), [here (hand tracking)](https://www.youtube.com/watch?v=N5xl3HBNsqg) and [here](https://www.youtube.com/watch?time_continue=2&v=NwkpiE4Tgjs). 

Be careful when buying a no name headset they are very likely not compatible with the HoloKit and Aryzon SDK's since distance between the lenses and phone as well as the mirror and lense quality will make a big difference. The lenses on the original HoloKit ([video](https://vimeo.com/215842939)) and Aryzon [video](https://www.youtube.com/watch?v=_TDAgqFxlUU) headset look much more advanced compared to the ones on the cheaper Chinese versions ([video 1](https://www.youtube.com/watch?v=O8LKG2MqVe8) and [video 2](https://www.youtube.com/watch?v=l_Br9Ehbf7s)). 

Despite the interesting use cases, I can not give an unconditional recommendation to buy the HoloKit or Aryzon headset right know mainly because the software is not as advanced as we used to from the GearVR/Daydream tracking which in my opinion is the minimal standard we need to have an enjoyable experience.

Improvements to the ergonomic would also be required. A different headset design which was already planed and promised for the end of 2018 by the HoloKit team is still not available and I would suggests the software flaws need to be fixed first.
Improving the HoloKit idea further seems to be an interesting project and I am currently testing the Aryzon headset to see if it can be made better with the right software tweaks.

ZapBox shows of some interesting ideas like a lens to increase the field of view and markers for the controller tracking.

<p align="center">
<a href="https://youtu.be/LzIwlPfBmuc"><img src="https://user-images.githubusercontent.com/12700187/59551251-f0c70b00-8f76-11e9-9068-1ed5a5dc9a79.png" width="50%"></a>
</p>
<p align="center">
Marker based 6DoF hand controller developed by ZapBox. 
</p>

However, the ZapBox software needs a lot of improvement (video showing funny tracking errors [here](https://www.youtube.com/watch?v=LzIwlPfBmuc)) and in my opinion lags far behind Googles ARCore tracking. The underling idea of creating a mixed reality headset with only one camera seems somewhat silly, and I can not recommend doing it when not also tackling the stereoscopic issue. Zapbox tries to improve the image by using depth informations from the pointcloud and bluring parts of the image as they state in [this reddit post](https://www.reddit.com/r/oculus/comments/5f2pvs/zapbox_mixed_reality_for_30/). 
The Phantom AR project also worked on the issue of generating a stereoscopic view from a monoscopic image [see here](https://www.reddit.com/r/PHNTM) for more info. You can try out the 2D to 3D conversion results yourself by watching [this video](https://www.youtube.com/watch?v=SRmVH2_rPpY&feature=youtu.be) and putting your phone inside a headset starting at timestamp 2:31. 

To get a 3D view from a 2D image it is a good approach to first capture a depth map and use it together with the mono camera image to generate a 3D stereoscopic view. For this approach the upcoming depth sensors in the latest phones could be leveraged. There is also a lot of machine learning research in the area of inferring depth information from 2D video or directly [generating 3D video output from 2D video input](https://arxiv.org/pdf/1604.03650.pdf). 
Having a high quality, low latency, wide field of view 3D passthrough image on the GearVR would open the possibility for some interesting mixed reality applications on such a cheap headset and would also be beneficial to the boundary system so something similar to the solution found in the Oculus Quest where the real world is displayed to the user when he comes to close to objects could be implemented. 

### Kinect + GearVR 
This combination seems be suited for bringing full body movement to the GearVR: Check out the related [reddit post](https://www.reddit.com/r/GearVR/comments/4k78ur/gearvr_positional_tracking/)

### ALVR 
This sweet open source software project connects the GearVR to the pc and uses head tracking for simulating expensive headsets. Check out the project [here](https://github.com/polygraphene/ALVR) if you haven't heard about it. 
For Oculus Quest users this should be interesting as well, see [this video](https://www.youtube.com/watch?v=gtmJInS7RxU) to learn more. 

### Phantom AR A Cloud Processing and Networking Platform
Using the computing power of the cloud to process data seems to be the path of the future. The Phantom AR project explores the possibilities of such a future. Learn more about it in this [video](https://www.youtube.com/watch?v=SRmVH2_rPpY&feature=youtu.be)

### Multi User Experiences
The [Just a Line experimental project](https://www.blog.google/products/google-vr/just-line-first-cross-platform-collaborative-ar-app-doodling/) explored the possibilities of multiuser experiences between iPhone and Android phones. This project makes use of the cloud anchor capabilities of ARCore.

Owlchemy labs showed of the [research project mobile spectator](https://owlchemylabs.com/owlchemy-mobile-spectator-ar-spectator-camera/) which allow a third person to use a mobile phone running ARCore to see and participate in a VR experience. This allows for interesting new experiences/games or could make VR more social. 

A multiplayer VR game based just on the same starting point ([seen here](https://i.imgur.com/ZX9Veen.gifv)) would also be possible not requiring a connection to the cloud but some kind of marker or phone to phone communication to know the location of the other player. However in my opinion using cloud anchors is the right way to go since it relies on finding the same feature points in a scene to match the location. 

### Leveraging The Front Camera For Eye Tracking
Is an interesting idea and in [this paper](https://dl.acm.org/citation.cfm?doid=3131785.3131809) (use [scihub.tw](https://sci-hub.tw/) to get free access) a working version was developed. 

### Are you aware of further interesting projects, or you work on an interesting project yourself? Please don't hesitate to open a new issue and spread the word.


# Some thoughts about the optimal and future hardware:

### The End For Mobile Smartphone Based VR?

The Daydream and GearVR platforms are both loosing momentum with Daydream not beeing supported by the latest Pixel phones and GearVR no longer beeing supported by the upcomming Note 10. All this is understandable since new GearVR and Daydream headsets might be the cheapest way to get into VR when you already own a compatible phone but with ever more affordable standalone headsets like the Oculus GO and Quest which offer a simpler setup and often a better experience customer have a great alternative.

Unity 2019.3 also drops support for phone based VR like Daydream and GearVR in coming updates, [read here](https://blogs.unity3d.com/2020/01/24/unity-xr-platform-updates/)

Phones will get better in the future and get more capable when it comes to display VR experiences. However they will likely not play a significant role despite them beeing the chepest way to enter VR if you have the phone already and only a headset with lenses is required. But with the losing support it will get harder and harder and as a developer to create experiences and distribute them to your followers therfore you should not think twice about it and go with a standalone Oculus Quest, PC headset or the PSVR.

### Qualcomm Chips 

Qualcomm already offers chips that have positional tracking in mind. Starting with the Snapdragon 835 in the Samsung S8 better stereo camera based tracking should be possible regarding to Qualcomm and you can read more about it here. Qualcomm mentions 4 key advantages of stereo over monocular 6-DoF:

• Instant accurate scene depth

• Faster initialization

• Better performance with quick and rotational motions

• Improved tolerance to camera occlusion


### Galaxy S10 5G 
On top of the list but still somehow speculative since it is not clear if the featured hQVGA 3d [depth/time of flight sensor](https://www.youtube.com/watch?v=OMDfQC0m4i4) will work in combination with the GearVR. When we get access to the depth sensing data low latency positional tracking similar to the Asus Zenfone AR could be possible. The ultrawide field of view of the camera as well as the depth sensor could be used for better hand/moving marker tracking. Compatibility with the GearVR should result in a great VR experience and the 5G feature might allow for streaming high quality data directly from the cloud allowing for experiences outperforming even desktop VR. Interestingly it seems that support for the Daydream platform was dropped for these phones. Also be aware that most of the underling software is still not even developed so don't expect these features to be available anytime soon. 

### Asus Zenfone AR 
Supporting Daydream which delivers a good enough VR experience and in combination with its highly accurate depth camera and the Google Tango software it allows for excellent positional tracking. 
The disadvantage is that Google stopped developing Tango, that almost no one owns the hardware and that no performance improvements are expected in the future making the development for this device and the Tango platform not very attractive. If you decide to purchase this device be aware of the overheating issue due to the rather old chipset.

### Galaxy S10 and S10+, S8, S9 , Note8 and so on 
They deliver better performance than the S7 which is important for running hand tracking and better looking games. In terms of image quality, field of view and tracking latency, they are identical to the S7. The ultrawide field of view camera of the S10 and S10+ could be interesting for improving hand and marker tracking in the future but this would depend on the tracking software (Manomotion or uSense) and ARCore if they support the wide field of view camera. Be aware that the S10 dose not support Daydream so if you want to use Daydream and GearVR on the same device you should get an S8 or S9 instead.

### Galaxy S7 
Runs ARCore and works with many GearVR headsets without immediately overheating. For hand-tracking the performance might be not enough but this is still an open research question. In combination with a cheap second hand GearVR for roughly 30$ it might be the best value device for experiencing VR right now. This software project was tested on the S7. 

### Daydream ready phones 
They are powerful, often also support ARCore and the VR quality with the second generation Daydream View is almost as good as GearVR. Daydream is a much more open platform compared to GearVR and runs on [multiple phones](https://vr.google.com/daydream/smartphonevr/phones/) which only need to meet the [hardware requirements](https://www.androidauthority.com/google-daydream-ready-phones-705245/) and the ok from Google or the device manufacturer to be supported. That said the S10 didn't seem to get Daydream support which is strange since it clearly has the capable hardware. It should be possible to root and force Daydream on the S10 or other ARCore ready phones but that trick comes which some risks and might not work for you in my case testing it on the S7 once it kind of worked but I had many issues making it unusable. If you are interested in how to force Daydream on your device have a look at [this](https://www.xda-developers.com/force-daydream-vr-compatibility/) and [this XDA developer post](https://forum.xda-developers.com/mobile-vr/google-daydream-vr/magisk-daydream-cardboard-enabler-nfc-t3917601). 
Currently, this project is working better on GearVR since resetting the view on Daydream is somewhat more complicated. This might change in the future if the issue can be solved.
Since the Pixel 3 is one of the few phones which now supports 60fps tracking it might become one of the best phones for mobile VR with positional tracking and this should be further investigated.

### Most other phones 
Most phones out there do not support GearVR or Daydream and if you are not willing to mess with the system, your only option is Google Cardboard which is noticeably worse because of two factors. The lense quality of Cardboard lenses and the inferior Cardboard software. This project offers a Cardboard version but I recommend when you have the option to use a GearVR or Daydream project instead. 
On some phones which support Daydream the Cardboard apps now also offer low persistency mode and great tracking performance which where introduces in a new Cardboard SDK ([read more about that here](https://www.roadtovr.com/google-cardboard-apps-daydream-phone-performance/)) but older phones do not offer a good experience using the Cardboard software.
If you are willing to mess around, you can try to force Daydream on the phone like explained in [this post](https://forum.xda-developers.com/mobile-vr/google-daydream-vr/magisk-daydream-cardboard-enabler-nfc-t3917601). The problem is that the Daydream requirements often have their justification (the S10 might be an exaction from the rule) and the experience might not be as good as on supported devices.

ARCore runs on many more devices compared to Daydream/GearVR and your phone might support it even when VR is not supported. When your phone is not an ARCore ready phone you can force it either with the help of this [ARCore for everyone github project](https://github.com/tomthecarrot/arcore-for-all) or by rooting your phone and using a special ROM which contains the ARCore feature.
But the chances of getting good tracking when forcing ARCore on an unsupported device are not very good since the ARCore software seems to be specifically adjusted to the phone camera to work correctly. But you can give it a try to know for sure.

The OnePlus 7 Pro should also be mentioned here since it will offer a 90Hz refresh rate display which should make a noticeable difference in the visual quality. Currently, it is not clear if it will get ARCore support and if forcing Daydream onto it will work. So no recommendation yet but keep an eye on this interesting device.

An overview over all the commercially available mobile VR headsets can be found [here](https://www.aniwaa.com/best-of/vr-ar/best-standalone-vr-headset/).


# Best Hardware To Buy Right Now:

## Mobile VR:

### Best VR Ready Smartphones:

If you are looking for a new phone which is well suited for VR and works in combination with this project I can recommend these 3 devices:

<a href="https://www.amazon.co.uk/gp/product/B06XYMCMHD/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B06XYMCMHD&linkCode=as2&tag=christophge03-21&linkId=df992f57728629e394a1d8c0412a1bc6"><img src="https://user-images.githubusercontent.com/12700187/57588508-7ccbba00-7515-11e9-9ef1-d3db759597f5.png" width="35%"></a> <a href="https://www.amazon.co.uk/gp/product/B07NWR7QYQ/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B07NWR7QYQ&linkCode=as2&tag=christophge03-21&linkId=99a1abe6046750764feb01bd85d42be4"><img src="https://user-images.githubusercontent.com/12700187/57588556-48a4c900-7516-11e9-9a22-4bf8b316f0ba.png" width="35%"></a> 

<a href="https://www.amazon.com/gp/product/B07CTYBVMM/ref=as_li_tl?ie=UTF8&camp=1789&creative=9325&creativeASIN=B07CTYBVMM&linkCode=as2&tag=christophgesk-20&linkId=47d8897fa39d664057ec1bb41e4978c5"><img src="https://user-images.githubusercontent.com/12700187/57588666-bf8e9180-7517-11e9-9533-9235f834043b.png" width="35%"></a> 

### Cheapest Deal | GearVR and Daydream View:

If you own a Samsung Smartphone S7 or above it will support ARCore and I recommend the GearVR with a controller. Make sure to find the right GearVR headset for your phone the connector and size requirements might change depending on your phone. If you own a smartphone which only has Daydream support, I can also recommend the second generation of the Daydream View headsets which offers improved lenses compared to the first Daydream View generation. Booth GearVR and Daydream View 2 offer great 3DOF VR experiences and interesting experimental 6DOF and computer vision capabilities.
I cannot recommend the Cardboard headset the quality is just not good enough. 

<a href="https://www.amazon.co.uk/gp/product/B07142L1V6/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B07142L1V6&linkCode=as2&tag=christophge03-21&linkId=9e83db0ebbc078333027f48745c506fe"><img src="https://user-images.githubusercontent.com/12700187/57588245-88b57d00-7511-11e9-8cc6-58e94b2e1d39.png" width="30%"></a>
<a href="https://www.amazon.co.uk/gp/product/B0773RZWV1/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B0773RZWV1&linkCode=as2&tag=christophge03-21&linkId=7ec33d0127adf6dbafa89690a2fa7af4"><img src="https://user-images.githubusercontent.com/12700187/57588145-738c1e80-7510-11e9-8d98-d9e4188ff43b.png" width="30%"></a> 

### Best Deal | Oculus Quest:

The Oculus Quest is an all in one device with better resolution, better head/controller tracking. But it has some disadvantages compared to the phone based solutions for example the tracking will not work outside when the sun is to bright (at dawn it seems to work quite well see this [video](https://www.youtube.com/watch?v=Eb_xZYDAfjM)), the tracking area is limited to 60m^2 ~ (7mx7m), many features that ARCore provides like marker tracking, cloud anchors for multiplayer (some experiments kind of work already see this [video](https://www.youtube.com/watch?v=msbTbfep_sY)) and hand tracking will not be available but it will be a great device, nonetheless.

<p align="left">
<a href="https://www.amazon.co.uk/gp/product/B07P6RJ39C/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B07P6RJ39C&linkCode=as2&tag=christophge03-21&linkId=2caf77e9cd6bb286b5234e844d36501b"><img src="https://user-images.githubusercontent.com/12700187/57587580-6dde0b00-7507-11e9-87af-cd566ab1d8e1.png" width="35%"></a> 
</p>

# How to Seurat:

What is Seurat? Read the introduction [here](https://developers.google.com/vr/discover/seurat)
In short it can capture a high-quality scene and make it run on a mobile device. The catch only a predefined area looks sharp and correct. In this experience it is a 2x3 meter box between the wooden gate and the stone bridge. This means if you try to cross the bridge you will see holes in the textures and distorted meshes. This is the limitation of this technique, but the area can be made bigger. I am not sure how big the area can get I will try that later for now enjoy the limited area between bridge and wooden gate. ;)
[Here](https://www.reddit.com/r/daydream/comments/8vsdnx/have_give_google_seurat_tool_a_try_using_unity/) and [here](https://www.youtube.com/watch?v=FTI_79f02Lg) you can learn how to capture a high-quality scene.


<p align="center">
<a href="https://youtu.be/CpZ94YDufqk"><img src="https://user-images.githubusercontent.com/12700187/54431116-d1c19e80-4725-11e9-802d-61d15897cf2f.png" width="100%" ></a>
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
+ [FusedVR](https://www.youtube.com/watch?v=4EWPUdE_kqU) did some very early work in this area.
+ Roberto Lopez Mendez with his [very first positional tracking project](https://blogs.unity3d.com/2017/10/18/mobile-inside-out-vr-tracking-now-readily-available-on-your-phone-with-unity/). [Paper](https://www.researchgate.net/publication/326918327_Mobile_inside-out_VR_tracking_now_available_on_your_phone_extended_abstract) (you can use [sci-hub.tw](https://sci-hub.tw/) to get free access)
+ Dimitri Diakopoulos (ddiakopoulos) provided a [simplified Seurat pipeline](https://github.com/ddiakopoulos/seurat/releases)
+ Reddit user st6315 (Jung Yi Hung) provided me with a mesh for the vikingVillage app and posted useful information [here](https://www.youtube.com/watch?v=CpZ94YDufqk&feature=youtu.be).  

# Privacy Notice:

This project uses software from Google (ARCore, Daydream, Cardboard) and Facebook (Oculus) and we don't know exactly what data is collected now or in the future. When using their software, we also have to agree to their privacy terms and conditions. In principle gyro/accelerometer sensor data like head rotation and heat maps of what you are looking at could be logged in the background and passwords could be infer from this [read related paper](https://pdfs.semanticscholar.org/ab85/10d054b232a5693bdd88b20422bde9ea4b26.pdf) to learn more about the risks. Data about your movement could be used to make predictions about your physical health and provide a digital fingerprint of you since the movement is unique to every user. Information about your surrounding using the camera might be even more problematic and should not be sent to Google servers. This project doesn't store any of your data but please refer to the privacy policy of Google and Oculus to learn what data they might collect on you.

[Google Privacy Policy](https://policies.google.com/privacy)

"This application runs on ARCore, which is provided by Google LLC and governed by the Google Privacy Policy"

[Oculus Privacy Policy](https://support.oculus.com/947039345464468/)

An excellent vice article about the challenges we face in VR/AR privacy can be found [here](https://www.vice.com/en_us/article/bj9ygv/the-eyes-are-the-prize-eye-tracking-technology-is-advertisings-holy-grail) and I can highly recommend this read if the topic is new to you. If this topic is close to your heart you might be interested in the [openarcloud.org](https://www.openarcloud.org) oragnisation as well.
