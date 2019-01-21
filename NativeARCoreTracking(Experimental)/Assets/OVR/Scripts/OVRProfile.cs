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
using System.Collections;
using System.Threading;
using VR = UnityEngine.VR;

/// <summary>
/// (Deprecated) Contains information about the user's preferences and body dimensions.
/// </summary>
public class OVRProfile : Object
{
	[System.Obsolete]
	public enum State
	{
		NOT_TRIGGERED,
		LOADING,
		READY,
		ERROR
	};

	[System.Obsolete]
	public string id { get { return "000abc123def"; } }
	[System.Obsolete]
	public string userName { get { return "Oculus User"; } }
	[System.Obsolete]
	public string locale { get { return "en_US"; } }

	public float ipd { get { return Vector3.Distance (OVRPlugin.GetNodePose (OVRPlugin.Node.EyeLeft, OVRPlugin.Step.Render).ToOVRPose ().position, OVRPlugin.GetNodePose (OVRPlugin.Node.EyeRight, OVRPlugin.Step.Render).ToOVRPose ().position); } }
	public float eyeHeight { get { return OVRPlugin.eyeHeight; } }
	public float eyeDepth { get { return OVRPlugin.eyeDepth; } }
	public float neckHeight { get { return eyeHeight - 0.075f; } }

	[System.Obsolete]
	public State state { get { return State.READY; } }
}
