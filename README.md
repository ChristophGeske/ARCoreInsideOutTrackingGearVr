# ARCoreInsideOutTrackingS7
Inside Out Tracking for the Galaxy S7

Steps:


1.) Follow the ARCore installation steps outlined here: https://developers.google.com/ar/develop/unity/getting-started

      - Make sure to install Unity 2017.3.0f2 or higher! Older Unity versions do not work!
      - Try out the HalloAR app before you continue!
      
2.) Download the latest Oculus Utilities for Unity here: https://developer.oculus.com/downloads/package/oculus-utilities-for-unity-5/

3.) Import the arcore-unity-sdk-preview2.unitypackage into Unity in the same way you imported the arcore-unity-sdk-preview2.unitypackage described in step 1.) 

4.) Find out your Device ID https://startvr.co/how-to-get-your-samsung-gear-vr-device-id/

5.) Generate an osig file which you need for your app to run on your GearVr https://dashboard.oculus.com/tools/osig-generator/

6.) Place the osig file under Assets -> Plugins -> Android -> assets. If those folders don't exist add them yourselfe.  

7.) Under Build Settings -> Player Settings -> XR Settings check Virtual Reality Suporrted and choose Oculus via the plus simbole.







