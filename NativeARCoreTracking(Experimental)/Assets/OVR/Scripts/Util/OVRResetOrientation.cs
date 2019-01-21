/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;

/// <summary>
/// Allows you to reset VR input tracking with a gamepad button press.
/// </summary>
public class OVRResetOrientation : MonoBehaviour
{
	/// <summary>
	/// The gamepad button that will reset VR input tracking.
	/// </summary>
	public OVRInput.RawButton resetButton = OVRInput.RawButton.Y;

	/// <summary>
	/// Check input and reset orientation if necessary
	/// See the input mapping setup in the Unity Integration guide
	/// </summary>
	void Update()
	{
		// NOTE: some of the buttons defined in OVRInput.RawButton are not available on the Android game pad controller
		if (OVRInput.GetDown(resetButton))
		{
			//*************************
			// reset orientation
			//*************************
			OVRManager.display.RecenterPose();
		}
	}
}
