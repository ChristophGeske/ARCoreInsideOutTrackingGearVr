# Inside Out Positional Head Tracking (standalone 6DoF) for GearVR/Cardboard/Daydream using ARCore v1.6.0
ARCore v1.6.0 enabled Inside Out Positional Tracking (six degrees of freedom) for all ARCore capable devices.


# Introducing Remarks:

- WARNING YOU MIGHT GET SICK: The current versions use interpolation and smoothing functions to cover up imprecise tracking. This leads to relative high latency in response to head motion. This is bad for people who get sick from motion sickness quickly. If you know that you are susceptible to motion sickness, these apps might not be for you jet.
      
- Fast movements, featureless and poorly lit areas can affect the quality of tracking severely. When the app starts up it takes 3-5 seconds for ARCore to detect a plain at this stage it is best to move just a little.   
      
- Your phone might get hot very quickly. Make sure to end the game after using. The Cardboard version seems to stay on even if the phone gets very hot while the GearVR version turns itself off automatically. Therefore, cardboard users should make sure to manually end the app after use.
      
- This project can give you a good taste for the capabilities of the HTC Vive Focus and the Daydream powered Lenovo Mirage Solo. Both devices are very pricy and not available everywhere yet. Because they use dedicated hardware, they can offer better performance and quality tracking. The Oculus Santa Cruz/Quest which is expected for early 2019 will also offer 6DoF tracking and feature two 6DoF tracked hand controllers which should make it a highly desirable device. 
      
- Before installing one of the apps (.apk files) make sure you have installed Google ARCore from the [Play Store](https://play.google.com/store/apps/details?id=com.google.ar.core&hl=en) on your device. Check if your device supports ARCore on [this page](https://developers.google.com/ar/discover/supported-devices).


# Software Description:

|Projects and Apps (.apk)|Description|Recommended Devices|VR Headset|
|---|---|---|---|
| | | | |
|[WhiteIsland.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/WhiteIsland.apk)|Simple environment. Using nativeARCore for tracking. Noticeable latency due to the used smoothing function. Use the controller touchpad to fly forward in viewing direction and the trigger button to enable/disable visualization of real-world boundaries.|All ARcore + GearVR compatible devices|GearVR|
| | | | |
|[BoxyRoomCardboard.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/BoxyRoomCardboard.apk)|App should run on every smartphone with ARCore support. Daydream users need to free the camera maybe like explained [here](https://lauren.vortex.com/2018/05/20/using-googles-daydream-vr-headset-for-augmented-reality-and-positional-tracking-applications) and might need to turn of NFC to avoid Daydream from starting.|All ARcore compatible devices|Cardboard|
| | | | |
|[VikingVillageGearVR.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/VikingVillageGearVR.apk)|Interesting because of the high-quality environment which was captured with the Seurat tool. This tool allows for the capture of a (small) area of a high-quality environments which allows for a limited free movement in this captured area.|All ARcore + GearVR compatible devices|GearVR|
| | | | |
|[VikingVillageCardboard.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/VikingVillageCardboard.apk)|Interesting because of the high-quality environment which was captured with the Seurat tool. This tool allows for the capture of a (small) area of a high-quality environments which allows for a limited free movement in this captured area.|All ARcore compatible devices|Cardboard|
| | | | |
|[VikingVillageForOculusGO.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/VikingVillageForOculusGO.apk)|The Oculus GO has no camera so there is no head tracking possible, but because the high-quality environment scene captured with the Seurat tool could be interesting for Go users as well, I added controller support so that flying through the scene by pressing the touchpad is possible. (I haven't tested the app on the GO myself so please let me know if it works or not.|OculusGo|OculusGo|
| | | | |
|VuforiaTracking(Experimental)|This app uses Vuforia which has an ARCore integration. But the performance of the ARCore integration is very low compared to native ARCore. Therfore this project will be discontinued.|All GearVR capable devices (low tracking quality)|GearVR|
| | | | |
|NativeARCoreTracking(Experimental)| Is not up to date and will be removed soon. Please use WhiteIsland instead. |All ARCore + GearVR capable devices|GearVR|
| | | | |
[MovingMarkerTracking.apk](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/raw/master/MovingMarkerTracking/MovingMarkerTracking.apk)|Uses Vuforia Vusion which combines ARCore and Vuforias advanced marker tracking. This combination allows for 6DOF marker tracking in a limited area in front of the camera. The marker can be found [here](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/blob/master/MovingMarkerTracking/King.jpg). Simply open the marker on your pc screen or print it on paper and look at it through the camera when the app is running. Since Google just announced that ARCore 1.9 now supports moving marker tracking this project will probably be replaced soon since Vuforia Marker tracking has many issues.|All GearVR capable devices (low tracking quality)|GearVR| 


# Installation 

### On Cardboard

Works with all phone who support ARCore. Just download the BoxyRoomCardboard.apk or the VikingVillageCardboard.apk (Seurat) and install. 

<p align="center">
<a href="https://youtu.be/EFglp19C8tg"><img src="https://user-images.githubusercontent.com/12700187/54432048-5e6d5c00-4728-11e9-82b8-994b5ab7114b.png" width="100%"></a>
</p>

<p align="center">
Video showing BoxyRoomCardboard.apk in action.
</p>

You need to restart the app when misalignment between the headset and the head movement is observed. 

CardboardVR lags the time warp function that we can use in GearVR and Daydream apps therefore the head rotation is less smooth. If you can please try to use Daydream or GearVR the experience will be noticeably better.


### On GearVR (apps need to be signed first):

Installing on GearVR requires the app to be signed first. Since the current GearVR version is better than the Daydream version the few extra steps to sign it are worth it.

<b>Signing the .apk file:</b>

First. Download the WhiteIsland.apk. or the vikingVillage.apk. 

Second. The quickest option I found is to sign the apk using the free [Injector for GearVR app](https://play.google.com/store/apps/details?id=com.zgsbrgr.gearvr.injector&hl=en).
Just safe the .apk file on your phone, use the Injector app to sign it and install. 
Make sure the app you want to sign and install is not already installed on your phone because in my case this prevents the app from installing.  

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

You can use the touchpad on the side of the headset to recenter the view when you notice misalignment.


### On Daydream and iPhone

Daydream users: You can simply switch a cardboard project over to Daydream inside the Unity project. Use for example the "BoxyRoomChardboard" project and under build settings, XR settings replace Cardboard with Daydream. I will upload a version specifically build for Daydream users as soon as I get access to a daydream device and headset to test it. 

iPhone user: You should look at [this project](https://github.com/andrewnakas/ARKit-Cardboard-VR/blob/master/README.md) first for a quick solution. Unfortenatly only Cardboard VR which lacks some critical features like reprojection and low persistency screen mode runs on iphones making iphones not well suited for mobile VR.


# TODO's and future work:

### Improve Head Tracking 

The current tracking version you find in this repository suffers from lag, jumps in position, drift from tracking being lost and other inconveniences. Fixing these issues seems very doable, and I am currently working on that full time. The most promising solution seems to use a filter which detects tracking errors and removes them. First tests look very promising so stay tuned for improved versions in the future. I don't think we reach the same quality as dual camera setups in extrema situations but I predict a future version could offer almost perfect head tracking when just performing normal head movements. 

### Hand Tracking 

Hand tracking using software from [Manomotion](https://www.manomotion.com/) and [uSenseAR](https://en.usens.com/products/usensar/) ([video](https://youtu.be/wdiC7l_Wecg)) is available and works with VR and ARCore. Unfortunately, the performance requirements are very high. Manomotion is already accessible and uSenseAR is not available yet. First tests with Manomotion where not so successful but I will follow the development closely. Another disadvantage is that the Manomotion software is not free of charge. Moving markers attached to the hand look like it could work more reliable and with better performance for now.

<p align="center">
<a href="https://youtu.be/z7-JSaSOgfU"><img src="https://user-images.githubusercontent.com/12700187/54431726-72fd2480-4727-11e9-8719-322ee3dae8f0.png" width="100%"></a>
</p>

<p align="center">
Video showing Manomotion hand tracking in combination with ARCore. 
</p>

### Moving Marker Tracking

ARCore just announced that version 1.9 now supports moving marker tracking. Read more about it [here](https://developers.google.com/ar/develop/unity/augmented-images/). 
Until now only ARKit2([ARKit2 marker tracking](https://www.youtube.com/watch?v=ySYFZwkZoio)) and Vuforia where able to offer that feature and some time ago I experimented with it the result is [my MovingMarkerTracking project](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr/tree/master/MovingMarkerTracking) which uses Vuforia marker tracking. I am currently looking into ARCore based moving marker tracking which should be a much better solution. The interesting questions moving marker tracking opens up are: 1) If we can use it to further improve tracking. 2) If you can add a marker to your hands/controller to get some sort of hand/controller tracking but even if it works the limited field of view of the camera will probably not make it that useful for many applications but for some application it might be just enough. 

### 3D Scenning The Environment 

<p align="center">
<a href="https://www.youtube.com/watch?v=sN61K-6Ai1c"><img src="https://user-images.githubusercontent.com/12700187/54491273-6ad6ed80-48bd-11e9-8978-fdec337e10f0.png" width="100%"></a>
</p>

Two 3D Scanner projects using Google Tango [Open Constructor](https://github.com/lvonasek/tango/wiki/Open-Constructor) and the [master thesis Tango-Poc](https://github.com/stetro/project-tango-poc#pointcloud-app-pc) are available as open source projects. An [ARCore version](https://github.com/lvonasek/tango/wiki/3D-Scanner-for-ARcore) is also available (not open source) and a plugin for Unity can be bought [here](https://assetstore.unity.com/packages/tools/integration/3d-reconstruction-for-arcore-android-only-136919). It could be used to further improve the boundary system and automatically building a virtual environment which fits perfectly over your real environment. This is incredible, and I will try to implement something similar as well. I recommend reading the [related paper](https://scss.tcd.ie/publications/theses/diss/2018/TCD-SCSS-DISSERTATION-2018-035.pdf) and download the [free 3D scanning app](https://play.google.com/store/apps/details?id=com.lvonasek.arcore3dscanner&hl=en_US) to check it out. I also recommend this informative [video](https://youtu.be/1TF7esI3sMQ) about Tango 3D scanning technology.

### 6DOF Images, Videos and Light Fields  

360° and 180° photos and videos are nice but with additional depth information they become really interesting. Positional tracking on GearVR would be a natural fit since no hand controllers are needed, mobile phones can display high resolution images and since the head movements are rather slow we have no problems with camera motion blur and loss of tracking. There are three distinct ways of approaching this idea. 1.) Image overlayed on a depth map either by capturing the depth map separately or by using software to recover a point cloud from a stereoscopic video [(seen here)](https://www.youtube.com/watch?v=HSXMs2wnNc4) and further discussed [here](https://www.reddit.com/r/6DoF/). There is already an [app](https://www.oculus.com/experiences/go/1790370494337451/) available on the Oculus store for GearVR. 2.) Photogrametry allows for higly realistic 3D models. 3.) Using [a light field](https://www.blog.google/products/google-ar-vr/experimenting-light-fields/) capture of the environment. A demo project [(video)](https://www.youtube.com/watch?v=a-JX3ZPi720) was published by Google for free but is only running on desktop pc VR headsets. 
Using the seurat pipeline and [this github light field project](https://github.com/PeturDarri/Fluence-Unity-Plugin/tree/master/Assets) it might be possible to bring the same experience to mobile VR. The same is true for bringing very detailed photogrametry models on the phone in VR. I am currently exploring those ideas for my first app for the Oculus store.

### Further Ideas Worth Exploring 

For safty reasons a boundery system like shown in this [video](https://youtu.be/aVdWED6kfKc) would be nice as soon as the tracking works well enough.

Object detection/classification using the camera feet could allow for some unique game and application mechanics. Two projects using Google tensor flow as the underling machine learning software are available [here](https://github.com/Syn-McJ/TFClassify-Unity) and [here](https://github.com/MatthewHallberg/TensorFlowUnity-ImageClassification).

A multiplayer VR game using ARCores cloud anchors should be possible, but I am currently not working on it. [Here](https://i.imgur.com/ZX9Veen.gifv) is how it could look like.

Using VR outside or in a large play space is a lot of fun and feels very different from games experienced inside where the ability to move freely is limited. Here accessing  [3D maps](https://cloud.google.com/maps-platform/gaming/) might be interesting and result in exiting apps consisting of AR and VR elements.

# Other Interesting Projects:

### Shooter game Quake 
This iconic game can now be played using the GearVR with the option of using positional tracking. A good introduction video can be found [here](https://youtu.be/acEz98Ol8NI). The game and source code can be downloaded [here](https://github.com/DrBeef/QuakeGVR/releases). After signing the .apk you can use the GearVR controller to activate the positional tracking mode in the game settings. I am very impressed with the performance and tracking quality even on old phones like the S7. The developer didn't use a game engine like Unity to build the game but instead used the Android Studio which seemed to allow him to get a higher performance out of the phone by putting some tasks on different cores. I like the way he managed to give the user a window to the outside by tapping the headset but I think it is better to show the surrounding automatically when you come closer to an obstacle since you might forget to check the surrounding when you immersed in the game (I will start working on this soon since I have an idea already). I think this is excellent work and you should try it out.  

### iPhone + ARKit + Cardboard 
Such a project can be found [here](https://github.com/andrewnakas/ARKit-Cardboard-VR/blob/master/README.md) and [here](https://github.com/hanleyweng/iOS-ARKit-Headset-View)

### Fast Travel Games 
They also experimented with ARCore and GearVR. They put some serious thought in solving many of the little issues but the code they use seems not to be public. Learn more [here](https://community.arm.com/graphics/b/blog/posts/achieving-console-like-experiences-on-mobile-vr-with-apex-construct)

### Google Tango
Tango is only available on the Asus Zenfone AR and has better room tracking capabilities than ARCore. Useful links [here](https://community.arm.com/graphics/b/blog/posts/mobile-inside-out-vr-tracking-now-on-your-phone-with-unity), [here](https://twitter.com/youten_redo/status/921295583180079104) and an excellent talk about the underlying technology [here](http://voicesofvr.com/544-google-tangos-engineering-director-on-ar-capabilities-enabled-by-depth-sensors/).

### HoloKit (Cardboard AR) 

<a href="https://www.amazon.com/HoloKit-Cardboard-like-Augmented-Everyone-Compatible/dp/B075FB1ZVX/ref=as_li_ss_tl?keywords=HoloKit&qid=1557830221&s=electronics&sr=1-1-catcorr&linkCode=ll1&tag=christophgesk-20&linkId=49edf4d1665f30169ebadfe26914bb04&language=en_US"><img src="https://user-images.githubusercontent.com/12700187/57692138-522d4e80-7646-11e9-9ea2-185a19396093.png" width="35%"></a>

<p align="left">
If the Amazon link is dead check out the HoloKit website instead (https://holokit.io/).

[This introductory video](https://www.youtube.com/watch?v=ao8Gb9yDrBM) gives you a good overview and the [github project](https://github.com/holokit/holokitsdk) is also informative.
The possibilities such a headset provide are almost limitless some examples [here (markers)](https://www.youtube.com/watch?v=8fNdM0q7gUc), [here (hand tracking)](https://www.youtube.com/watch?v=N5xl3HBNsqg) and [here](https://www.youtube.com/watch?time_continue=2&v=NwkpiE4Tgjs)

There is also a company (Aryzon) selling almost the same product with thair own SDK but I don't know why this company even exists when there is HoloKit as an open-source alternative. Their website is https://www.aryzon.com/.

Be care full when buying a no name headset they are very likely not compatible since distance between the lenses and phone might be important for a good experience.

### Kinect + GearVR 
This combination seems be suited for bringing full body movement to the GearVR: Check out the related [reddit post](https://www.reddit.com/r/GearVR/comments/4k78ur/gearvr_positional_tracking/)

### ALVR 
This sweet open source software project connects the GearVR to the pc and uses head tracking for simulating expensive headsets. Check out the project if you haven't heard about it [here](https://github.com/polygraphene/ALVR). Might be interesting for the Oculus Quest too.

Are you aware of further interesting projects, or you work on an interesting project yourself? Please don't hesitate to open a new issue and let us know about it.


# Some thoughts about the optimal and future hardware:

1. The top of the list but still somehow speculative since it is not released yet is the Galaxy S10 5G and the Note 10 which will feature a hQVGA 3d depth sensor. When we get access to the depth sensing data low latency positional tracking similar to the Asus Zenfone AR could be possible. The ultrawide field of view of the camera as well as the depth sensor could be used for better hand/moving marker tracking. Compatibility with the GearVR should result in a great VR experience and the 5G feature might allow for streaming high quality data directly from the cloud allowing for experiences outperforming even desktop VR. Sadly it seems that support for the Daydream platform was dropped and will not be renewed in the future. Be aware that most of the underling software is still not even developed so don't expect these features to be available anytime soon. 

2. The Asus Zenfone AR is supporting Daydream which delivers a good enough VR experience and in combination with its highly accurate depth camera and the Google Tango software it allows for excellent positional tracking. 
The disadvantage is that Google stopped developing Tango and Daydream, that almost no one owns the hardware and that no performance improvements are expected in the future making the development for this device and the Tango platform not very attractive.

3. The Galaxy S10 and S10+, S8, S9 , Note8 and so on deliver better performance than the S7 which is interesting for running hand tracking and better looking games. In terms of image quality, field of view and tracking latency, they are identical to the S7. The ultrawide field of view camera of the S10 and S10+ could also be interesting for improving hand and marker tracking in the future but this would depend on the tracking software (Manomotion or uSense) and ARCore if they support the wide field of view camera. Be aware that the S10 dose not support Daydream so if you want to use Daydream and GearVR on the same device you should get an S9 instead.

4. The Galaxy S7 which is running ARCore and GearVR without immediately overheating is the lowest spec device allowing for positional tracking. For hand-tracking the performance might not be enough but this is still an open research question. In combination with a cheap second hand GearVR for roughly 30$ it might be the best value device for experiencing VR right now. This software project was tested on the S7. 

5. Daydream ready phones are powerful, often also support ARCore and the VR quality with the second generation Daydream View is almost as good as GearVR. Daydream is a much more open platform compared to GearVR and runs on [multiple phones](https://vr.google.com/daydream/smartphonevr/phones/) which only need to meet the [hardware requirements](https://www.androidauthority.com/google-daydream-ready-phones-705245/) and the ok from Google or the device manufacturer to be supported. That said the S10 didn't seem to get Daydream support which is strange since it clearly has the capable hardware. I am not sure why Google or Samsung made this move it looks counter-productive to me. It might be possible to root and force Daydream on the S10 or other ARCore ready phones but that trick comes which some risks and in my case testing it on the S7 it kind of worked but I had to many issues making it unusable. If you are interested in how to force Daydream on your device have a look at [this XDA developer post](https://www.xda-developers.com/force-daydream-vr-compatibility/). 
Currently, this project is working better on GearVR since resetting the view is somewhat more complicated using the Daydream software. This might change in the future if the issue can be solved.

6. ARCore ready phones that do not support GearVR or Daydream work with google cardboard but the experience is noticeably worse because of two factors. The lense quality of cardboard lenses and the inferior cardboard software affecting refrashrate of the display. This project offers a cardboard version but only because it is so accesible and cheap. I do not recommend using it when you have the option to use a GearVR or Daydream View headset instead. The quality is just not good enough.  

An overview over all the commercially available mobile VR headsets can be found [here](https://www.aniwaa.com/best-of/vr-ar/best-standalone-vr-headset/).

# Best Hardware To Buy Right Now:

## Mobile VR:

### Best VR Ready Smartphones:

If you are looking for a new phone which is well suited for VR and works in combination with this project I can recommend these 3 devices:

<a href="https://www.amazon.co.uk/gp/product/B06XYMCMHD/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B06XYMCMHD&linkCode=as2&tag=christophge03-21&linkId=df992f57728629e394a1d8c0412a1bc6"><img src="https://user-images.githubusercontent.com/12700187/57588508-7ccbba00-7515-11e9-9ef1-d3db759597f5.png" width="35%"></a> <a href="https://www.amazon.co.uk/gp/product/B07NWR7QYQ/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B07NWR7QYQ&linkCode=as2&tag=christophge03-21&linkId=99a1abe6046750764feb01bd85d42be4"><img src="https://user-images.githubusercontent.com/12700187/57588556-48a4c900-7516-11e9-9a22-4bf8b316f0ba.png" width="35%"></a> <a href="https://www.amazon.com/gp/product/B07CTYBVMM/ref=as_li_tl?ie=UTF8&camp=1789&creative=9325&creativeASIN=B07CTYBVMM&linkCode=as2&tag=christophgesk-20&linkId=47d8897fa39d664057ec1bb41e4978c5"><img src="https://user-images.githubusercontent.com/12700187/57588666-bf8e9180-7517-11e9-9533-9235f834043b.png" width="35%"></a> 

### Cheapest Deal | GearVR and Daydream View:

If you own a high-end smartphone already, you can get good VR for a very low price. If you own a Samsung Smartphone S7 or above it will support ARCore and I recommend the GearVR with a controller. Make sure to find the right GearVR headset for your phone the connector and size requirements might change depending on your phone. If you own a smartphone which only has Daydream support, I can also recommend the second generation of the Daydream headsets the View 2 which includes a controller. I cannot recommend the Cardboard headset the quality is just not good enough. In short they offer great 3DOF VR experiences and interesting experimental 6DOF and computer vision possibilities.

<a href="https://www.amazon.co.uk/gp/product/B07142L1V6/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B07142L1V6&linkCode=as2&tag=christophge03-21&linkId=9e83db0ebbc078333027f48745c506fe"><img src="https://user-images.githubusercontent.com/12700187/57588245-88b57d00-7511-11e9-8cc6-58e94b2e1d39.png" width="30%"></a>
<a href="https://www.amazon.co.uk/gp/product/B0773RZWV1/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B0773RZWV1&linkCode=as2&tag=christophge03-21&linkId=7ec33d0127adf6dbafa89690a2fa7af4"><img src="https://user-images.githubusercontent.com/12700187/57588145-738c1e80-7510-11e9-8d98-d9e4188ff43b.png" width="30%"></a> 

### Best Deal | Oculus Quest:

The Oculus Quest is an all in one device with better resolution, better head/controller tracking. But it has some disadvantages compared to the phone based solutions for example the tracking will not work outside when the sun is to bright, the tracking area is limited to 60m^2 ~ (7mx7m), many features that ARCore provides like marker tracking, cloud anchors (multiplayer in one location) and hand tracking will not be available but it will be a great device, nonetheless.

<p align="left">
<a href="https://www.amazon.co.uk/gp/product/B07P6RJ39C/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B07P6RJ39C&linkCode=as2&tag=christophge03-21&linkId=2caf77e9cd6bb286b5234e844d36501b"><img src="https://user-images.githubusercontent.com/12700187/57587580-6dde0b00-7507-11e9-87af-cd566ab1d8e1.png" width="35%"></a> 
</p>

Amazon Store Links: [USA](https://www.amazon.com/Oculus-Quest-All-Gaming-Headset-android/dp/B07PRDGYTW/ref=as_sl_pc_tf_til?tag=christophgesk-20&linkCode=w00&linkId=02f542650443d2d2e88177ea73cdb6c2&creativeASIN=B07PRDGYTW) | [UK](https://www.amazon.co.uk/gp/product/B07P6RJ39C/ref=as_li_tl?ie=UTF8&camp=1634&creative=6738&creativeASIN=B07P6RJ39C&linkCode=as2&tag=christophge03-21&linkId=2caf77e9cd6bb286b5234e844d36501b) | [DE](https://www.amazon.de/Oculus-Quest-All-Gaming-Headset/dp/B07P6Y5DNT/ref=as_sl_pc_tf_til?tag=christophgesk-21&linkCode=w00&linkId=15c29b24c42becf5c701c6b13b4c1296&creativeASIN=B07P6Y5DNT)     


These are Amazon affiliated links, they are a great way to support my work directly. Using them will not change the price for you. 

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
+ [FusedVR](https://www.youtube.com/watch?v=4EWPUdE_kqU) did some very erly work in this area.
+ Roberto Lopez Mendez with his [very first positional tracking project](https://blogs.unity3d.com/2017/10/18/mobile-inside-out-vr-tracking-now-readily-available-on-your-phone-with-unity/)
+ Dimitri Diakopoulos (ddiakopoulos) provided a [simplified Seurat pipeline](https://github.com/ddiakopoulos/seurat/releases)
+ Reddit user st6315 (Jung Yi Hung) provided me with a mesh for the vikingVillage app and posted useful information [here](https://www.youtube.com/watch?v=CpZ94YDufqk&feature=youtu.be).  

# Privacy Notice:

This project uses software from Google (ARCore, Daydream, Cardboard) and Facebook (Oculus) which is not fully accessible so we don't know exactly what data is collected. When using thair software, we also have to agree to their privacy terms and conditions. In principle gyro/accelerometer sensor data like head rotation and heat maps of what you are looking at could be logged in the background. Data about your movement could be used to make predictions about your physical health and provide a digital fingerprint of you since the movement is unique to every user. Information about your surrounding using the camera might be even more problematic and should not be sent to googles servers if possible. This project doesn't store any of your data but please refer to the privacy policy of Google and Oculus to learn what data they might collect on you.

[Google Privacy Policy](https://policies.google.com/privacy)

"This application runs on ARCore, which is provided by Google LLC and governed by the Google Privacy Policy"

[Oculus Privacy Policy](https://support.oculus.com/947039345464468/)

# Support this Project:

If you like this work and you want to support the development of this free software project, please consider a donation via Bitcoin and PayPal.

<a href="https://www.coinbase.com/join/5a7a5c59852a7a06c9329bcf"><img src="https://user-images.githubusercontent.com/12700187/40888344-38a572f8-6756-11e8-9a93-eedc76f0d676.jpg" width="148" href="https://www.paypal.me/ChristophGeske"><img src="https://user-images.githubusercontent.com/12700187/54650628-c23caf80-4aaf-11e9-8747-59d55fc90090.png" width="100">  </a>

Bitcoin wallet address is: [15aaSbgsZzwP3mrAGdZm7yvuZbu62f6JY4](https://www.coinbase.com/join/5a7a5c59852a7a06c9329bcf)

Paypal: [paypal.me/ChristophGeske](https://www.paypal.me/ChristophGeske). 


