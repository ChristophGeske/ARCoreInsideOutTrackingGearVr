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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Provides a unified input system for Oculus controllers and gamepads.
/// </summary>
public static class OVRInput
{
	[Flags]
	/// Virtual button mappings that allow the same input bindings to work across different controllers.
	public enum Button
	{
		None                      = 0,          ///< Maps to RawButton: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		One                       = 0x00000001, ///< Maps to RawButton: [Gamepad, Touch, RTouch: A], [LTouch: X], [LTrackedRemote: LTouchpad], [RTrackedRemote: RTouchpad], [Touchpad, Remote: Start]
		Two                       = 0x00000002, ///< Maps to RawButton: [Gamepad, Touch, RTouch: B], [LTouch: Y], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: Back]
		Three                     = 0x00000004, ///< Maps to RawButton: [Gamepad, Touch: X], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Four                      = 0x00000008, ///< Maps to RawButton: [Gamepad, Touch: Y], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Start                     = 0x00000100, ///< Maps to RawButton: [Gamepad: Start], [Touch, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Start], [RTouch: None]
		Back                      = 0x00000200, ///< Maps to RawButton: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Back], [Touch, LTouch, RTouch: None]
		PrimaryShoulder           = 0x00001000, ///< Maps to RawButton: [Gamepad: LShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryIndexTrigger       = 0x00002000, ///< Maps to RawButton: [Gamepad, Touch, LTouch, LTrackedRemote: LIndexTrigger], [RTouch, RTrackedRemote: RIndexTrigger], [Touchpad, Remote: None]
		PrimaryHandTrigger        = 0x00004000, ///< Maps to RawButton: [Touch, LTouch: LHandTrigger], [RTouch: RHandTrigger], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbstick         = 0x00008000, ///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstick], [RTouch: RThumbstick], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbstickUp       = 0x00010000, ///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstickUp], [RTouch: RThumbstickUp], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbstickDown     = 0x00020000, ///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstickDown], [RTouch: RThumbstickDown], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbstickLeft     = 0x00040000, ///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstickLeft], [RTouch: RThumbstickLeft], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbstickRight    = 0x00080000, ///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstickRight], [RTouch: RThumbstickRight], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryTouchpad           = 0x00000400, ///< Maps to RawButton: [LTrackedRemote, Touchpad: LTouchpad], [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, Remote: None]
		SecondaryShoulder         = 0x00100000, ///< Maps to RawButton: [Gamepad: RShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryIndexTrigger     = 0x00200000, ///< Maps to RawButton: [Gamepad, Touch: RIndexTrigger], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryHandTrigger      = 0x00400000, ///< Maps to RawButton: [Touch: RHandTrigger], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryThumbstick       = 0x00800000, ///< Maps to RawButton: [Gamepad, Touch: RThumbstick], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryThumbstickUp     = 0x01000000, ///< Maps to RawButton: [Gamepad, Touch: RThumbstickUp], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryThumbstickDown   = 0x02000000, ///< Maps to RawButton: [Gamepad, Touch: RThumbstickDown], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryThumbstickLeft   = 0x04000000, ///< Maps to RawButton: [Gamepad, Touch: RThumbstickLeft], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryThumbstickRight  = 0x08000000, ///< Maps to RawButton: [Gamepad, Touch: RThumbstickRight], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryTouchpad         = 0x00000800, ///< Maps to RawButton: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		DpadUp                    = 0x00000010, ///< Maps to RawButton: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadUp], [Touch, LTouch, RTouch: None]
		DpadDown                  = 0x00000020, ///< Maps to RawButton: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadDown], [Touch, LTouch, RTouch: None]
		DpadLeft                  = 0x00000040, ///< Maps to RawButton: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadLeft], [Touch, LTouch, RTouch: None]
		DpadRight                 = 0x00000080, ///< Maps to RawButton: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadRight], [Touch, LTouch, RTouch: None]
		Up                        = 0x10000000, ///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstickUp], [RTouch: RThumbstickUp], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadUp]
		Down                      = 0x20000000, ///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstickDown], [RTouch: RThumbstickDown], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadDown]
		Left                      = 0x40000000, ///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstickLeft], [RTouch: RThumbstickLeft], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadLeft]
		Right      = unchecked((int)0x80000000),///< Maps to RawButton: [Gamepad, Touch, LTouch: LThumbstickRight], [RTouch: RThumbstickRight], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadRight]
		Any                       = ~None,      ///< Maps to RawButton: [Gamepad, Touch, LTouch, RTouch: Any]
	}

	[Flags]
	/// Raw button mappings that can be used to directly query the state of a controller.
	public enum RawButton
	{
		None                      = 0,          ///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		A                         = 0x00000001, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: A], [LTrackedRemote: LIndexTrigger], [RTrackedRemote: RIndexTrigger], [LTouch, Touchpad, Remote: None]
		B                         = 0x00000002, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: B], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		X                         = 0x00000100, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: X], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Y                         = 0x00000200, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: Y], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Start                     = 0x00100000, ///< Maps to Physical Button: [Gamepad, Touch, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Start], [RTouch: None]
		Back                      = 0x00200000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Back], [Touch, LTouch, RTouch: None]
		LShoulder                 = 0x00000800, ///< Maps to Physical Button: [Gamepad: LShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LIndexTrigger             = 0x10000000, ///< Maps to Physical Button: [Gamepad, Touch, LTouch, LTrackedRemote: LIndexTrigger], [RTouch, RTrackedRemote, Touchpad, Remote: None]
		LHandTrigger              = 0x20000000, ///< Maps to Physical Button: [Touch, LTouch: LHandTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbstick               = 0x00000400, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstick], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbstickUp             = 0x00000010, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickUp], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbstickDown           = 0x00000020, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickDown], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbstickLeft           = 0x00000040, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickLeft], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbstickRight          = 0x00000080, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickRight], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LTouchpad                 = 0x40000000, ///< Maps to Physical Button: [LTrackedRemote: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Touchpad, Remote: None]
		RShoulder                 = 0x00000008, ///< Maps to Physical Button: [Gamepad: RShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RIndexTrigger             = 0x04000000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch, RTrackedRemote: RIndexTrigger], [LTouch, LTrackedRemote, Touchpad, Remote: None]
		RHandTrigger              = 0x08000000, ///< Maps to Physical Button: [Touch, RTouch: RHandTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RThumbstick               = 0x00000004, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstick], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RThumbstickUp             = 0x00001000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickUp], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RThumbstickDown           = 0x00002000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickDown], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RThumbstickLeft           = 0x00004000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickLeft], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RThumbstickRight          = 0x00008000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickRight], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RTouchpad  = unchecked((int)0x80000000),///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		DpadUp                    = 0x00010000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadUp], [Touch, LTouch, RTouch: None]
		DpadDown                  = 0x00020000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadDown], [Touch, LTouch, RTouch: None]
		DpadLeft                  = 0x00040000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadLeft], [Touch, LTouch, RTouch: None]
		DpadRight                 = 0x00080000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadRight], [Touch, LTouch, RTouch: None]
		Any                       = ~None,      ///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Any]
	}

    [Flags]
	/// Virtual capacitive touch mappings that allow the same input bindings to work across different controllers with capacitive touch support.
	public enum Touch
	{
		None                      = 0,                            ///< Maps to RawTouch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		One                       = Button.One,                   ///< Maps to RawTouch: [Touch, RTouch: A], [LTouch: X], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Two                       = Button.Two,                   ///< Maps to RawTouch: [Touch, RTouch: B], [LTouch: Y], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Three                     = Button.Three,                 ///< Maps to RawTouch: [Touch: X], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Four                      = Button.Four,                  ///< Maps to RawTouch: [Touch: Y], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryIndexTrigger       = Button.PrimaryIndexTrigger,   ///< Maps to RawTouch: [Touch, LTouch: LIndexTrigger], [RTouch: RIndexTrigger], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbstick         = Button.PrimaryThumbstick,     ///< Maps to RawTouch: [Touch, LTouch: LThumbstick], [RTouch: RThumbstick], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbRest          = 0x00001000,                   ///< Maps to RawTouch: [Touch, LTouch: LThumbRest], [RTouch: RThumbRest], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryTouchpad           = Button.PrimaryTouchpad,       ///< Maps to RawTouch: [LTrackedRemote, Touchpad: LTouchpad], [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, Remote: None]
		SecondaryIndexTrigger     = Button.SecondaryIndexTrigger, ///< Maps to RawTouch: [Touch: RIndexTrigger], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryThumbstick       = Button.SecondaryThumbstick,   ///< Maps to RawTouch: [Touch: RThumbstick], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryThumbRest        = 0x00100000,                   ///< Maps to RawTouch: [Touch: RThumbRest], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryTouchpad         = Button.SecondaryTouchpad,     ///< Maps to RawTouch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Any                       = ~None,                        ///< Maps to RawTouch: [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad: Any], [Gamepad, Remote: None]
	}

    [Flags]
	/// Raw capacitive touch mappings that can be used to directly query the state of a controller.
	public enum RawTouch
	{
		None                      = 0,                            ///< Maps to Physical Touch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		A                         = RawButton.A,                  ///< Maps to Physical Touch: [Touch, RTouch: A], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		B                         = RawButton.B,                  ///< Maps to Physical Touch: [Touch, RTouch: B], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		X                         = RawButton.X,                  ///< Maps to Physical Touch: [Touch, LTouch: X], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Y                         = RawButton.Y,                  ///< Maps to Physical Touch: [Touch, LTouch: Y], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LIndexTrigger             = 0x00001000,                   ///< Maps to Physical Touch: [Touch, LTouch: LIndexTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbstick               = RawButton.LThumbstick,        ///< Maps to Physical Touch: [Touch, LTouch: LThumbstick], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbRest                = 0x00000800,                   ///< Maps to Physical Touch: [Touch, LTouch: LThumbRest], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LTouchpad                 = RawButton.LTouchpad,          ///< Maps to Physical Touch: [LTrackedRemote, Touchpad: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Remote: None]
		RIndexTrigger             = 0x00000010,                   ///< Maps to Physical Touch: [Touch, RTouch: RIndexTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RThumbstick               = RawButton.RThumbstick,        ///< Maps to Physical Touch: [Touch, RTouch: RThumbstick], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RThumbRest                = 0x00000008,                   ///< Maps to Physical Touch: [Touch, RTouch: RThumbRest], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RTouchpad                 = RawButton.RTouchpad,          ///< Maps to Physical Touch: [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, Touchpad, Remote: None]
		Any                       = ~None,                        ///< Maps to Physical Touch: [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad: Any], [Gamepad, Remote: None]
	}

    [Flags]
	/// Virtual near touch mappings that allow the same input bindings to work across different controllers with near touch support.
	/// A near touch uses the capacitive touch sensors of a controller to detect approximate finger proximity prior to a full touch being reported.
	public enum NearTouch
	{
		None                      = 0,          ///< Maps to RawNearTouch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryIndexTrigger       = 0x00000001, ///< Maps to RawNearTouch: [Touch, LTouch: LIndexTrigger], [RTouch: RIndexTrigger], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbButtons       = 0x00000002, ///< Maps to RawNearTouch: [Touch, LTouch: LThumbButtons], [RTouch: RThumbButtons], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryIndexTrigger     = 0x00000004, ///< Maps to RawNearTouch: [Touch: RIndexTrigger], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryThumbButtons     = 0x00000008, ///< Maps to RawNearTouch: [Touch: RThumbButtons], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Any                       = ~None,      ///< Maps to RawNearTouch: [Touch, LTouch, RTouch: Any], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
	}

    [Flags]
	/// Raw near touch mappings that can be used to directly query the state of a controller.
	public enum RawNearTouch
	{
		None                      = 0,          ///< Maps to Physical NearTouch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LIndexTrigger             = 0x00000001, ///< Maps to Physical NearTouch: [Touch, LTouch: Implies finger is in close proximity to LIndexTrigger.], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbButtons             = 0x00000002, ///< Maps to Physical NearTouch: [Touch, LTouch: Implies thumb is in close proximity to LThumbstick OR X/Y buttons.], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RIndexTrigger             = 0x00000004, ///< Maps to Physical NearTouch: [Touch, RTouch: Implies finger is in close proximity to RIndexTrigger.], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RThumbButtons             = 0x00000008, ///< Maps to Physical NearTouch: [Touch, RTouch: Implies thumb is in close proximity to RThumbstick OR A/B buttons.], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Any                       = ~None,      ///< Maps to Physical NearTouch: [Touch, LTouch, RTouch: Any], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
	}

    [Flags]
	/// Virtual 1-dimensional axis (float) mappings that allow the same input bindings to work across different controllers.
	public enum Axis1D
	{
		None                      = 0,     ///< Maps to RawAxis1D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryIndexTrigger       = 0x01,  ///< Maps to RawAxis1D: [Gamepad, Touch, LTouch: LIndexTrigger], [RTouch: RIndexTrigger], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryHandTrigger        = 0x04,  ///< Maps to RawAxis1D: [Touch, LTouch: LHandTrigger], [RTouch: RHandTrigger], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryIndexTrigger     = 0x02,  ///< Maps to RawAxis1D: [Gamepad, Touch: RIndexTrigger], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryHandTrigger      = 0x08,  ///< Maps to RawAxis1D: [Touch: RHandTrigger], [Gamepad, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Any                       = ~None, ///< Maps to RawAxis1D: [Gamepad, Touch, LTouch, RTouch: Any], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
	}

    [Flags]
	/// Raw 1-dimensional axis (float) mappings that can be used to directly query the state of a controller.
	public enum RawAxis1D
	{
		None                      = 0,     ///< Maps to Physical Axis1D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LIndexTrigger             = 0x01,  ///< Maps to Physical Axis1D: [Gamepad, Touch, LTouch: LIndexTrigger], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LHandTrigger              = 0x04,  ///< Maps to Physical Axis1D: [Touch, LTouch: LHandTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RIndexTrigger             = 0x02,  ///< Maps to Physical Axis1D: [Gamepad, Touch, RTouch: RIndexTrigger], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RHandTrigger              = 0x08,  ///< Maps to Physical Axis1D: [Touch, RTouch: RHandTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Any                       = ~None, ///< Maps to Physical Axis1D: [Gamepad, Touch, LTouch, RTouch: Any], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
	}

    [Flags]
	/// Virtual 2-dimensional axis (Vector2) mappings that allow the same input bindings to work across different controllers.
	public enum Axis2D
	{
		None                      = 0,     ///< Maps to RawAxis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryThumbstick         = 0x01,  ///< Maps to RawAxis2D: [Gamepad, Touch, LTouch: LThumbstick], [RTouch: RThumbstick], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		PrimaryTouchpad           = 0x04,  ///< Maps to RawAxis2D: [LTrackedRemote, Touchpad: LTouchpad], RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, Remote: None]
		SecondaryThumbstick       = 0x02,  ///< Maps to RawAxis2D: [Gamepad, Touch: RThumbstick], [LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		SecondaryTouchpad         = 0x08,  ///< Maps to RawAxis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		Any                       = ~None, ///< Maps to RawAxis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad: Any], [Remote: None]
	}

    [Flags]
	/// Raw 2-dimensional axis (Vector2) mappings that can be used to directly query the state of a controller.
	public enum RawAxis2D
	{
		None                      = 0,     ///< Maps to Physical Axis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LThumbstick               = 0x01,  ///< Maps to Physical Axis2D: [Gamepad, Touch, LTouch: LThumbstick], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		LTouchpad                 = 0x04,  ///< Maps to Physical Axis2D: [LTrackedRemote, Touchpad: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Remote: None]
		RThumbstick               = 0x02,  ///< Maps to Physical Axis2D: [Gamepad, Touch, RTouch: RThumbstick], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
		RTouchpad                 = 0x08,  ///< Maps to Physical Axis2D: [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, Touchpad, Remote: None]
		Any                       = ~None, ///< Maps to Physical Axis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote: Any], [Touchpad, Remote: None]
	}

	[Flags]
	/// Identifies a controller which can be used to query the virtual or raw input state.
	public enum Controller
	{
		None                      = OVRPlugin.Controller.None,           ///< Null controller.
		LTouch                    = OVRPlugin.Controller.LTouch,         ///< Left Oculus Touch controller. Virtual input mapping differs from the combined L/R Touch mapping.
		RTouch                    = OVRPlugin.Controller.RTouch,         ///< Right Oculus Touch controller. Virtual input mapping differs from the combined L/R Touch mapping.
		Touch                     = OVRPlugin.Controller.Touch,          ///< Combined Left/Right pair of Oculus Touch controllers.
		Remote                    = OVRPlugin.Controller.Remote,         ///< Oculus Remote controller.
		Gamepad                   = OVRPlugin.Controller.Gamepad,        ///< Xbox 360 or Xbox One gamepad on PC. Generic gamepad on Android.
		Touchpad                  = OVRPlugin.Controller.Touchpad,       ///< GearVR touchpad on Android.
		LTrackedRemote            = OVRPlugin.Controller.LTrackedRemote, ///< Left GearVR tracked remote on Android.
		RTrackedRemote            = OVRPlugin.Controller.RTrackedRemote, ///< Right GearVR tracked remote on Android.
		Active                    = OVRPlugin.Controller.Active,         ///< Default controller. Represents the controller that most recently registered a button press from the user.
		All                       = OVRPlugin.Controller.All,            ///< Represents the logical OR of all controllers.
	}

	private static readonly float AXIS_AS_BUTTON_THRESHOLD = 0.5f;
	private static readonly float AXIS_DEADZONE_THRESHOLD = 0.2f;
	private static List<OVRControllerBase> controllers;
	private static Controller activeControllerType = Controller.None;
	private static Controller connectedControllerTypes = Controller.None;
	private static OVRPlugin.Step stepType = OVRPlugin.Step.Render;
    private static int fixedUpdateCount = 0;


	private static bool _pluginSupportsActiveController = false;
	private static bool _pluginSupportsActiveControllerCached = false;
	private static System.Version _pluginSupportsActiveControllerMinVersion = new System.Version(1, 9, 0);
	private static bool pluginSupportsActiveController
	{
		get
		{
			if (!_pluginSupportsActiveControllerCached)
			{
				bool isSupportedPlatform = true;
#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
				isSupportedPlatform = false;
#endif
				_pluginSupportsActiveController = isSupportedPlatform && (OVRPlugin.version >= _pluginSupportsActiveControllerMinVersion);
				_pluginSupportsActiveControllerCached = true;
			}

			return _pluginSupportsActiveController;
		}
	}

	/// <summary>
	/// Creates an instance of OVRInput.
	/// </summary>
	static OVRInput()
	{
		controllers = new List<OVRControllerBase>
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			new OVRControllerGamepadAndroid(),
			new OVRControllerTouchpad(),
			new OVRControllerLTrackedRemote(),
			new OVRControllerRTrackedRemote(),
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			new OVRControllerGamepadMac(),
#else
            new OVRControllerGamepadPC(),
			new OVRControllerTouch(),
			new OVRControllerLTouch(),
			new OVRControllerRTouch(),
			new OVRControllerRemote(),
#endif
		};
	}

    /// <summary>
    /// Updates the internal state of OVRInput. Must be called manually if used independently from OVRManager.
    /// </summary>
    public static void Update()
	{
		connectedControllerTypes = Controller.None;
		stepType = OVRPlugin.Step.Render;
		fixedUpdateCount = 0;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			connectedControllerTypes |= controller.Update();

			if ((connectedControllerTypes & controller.controllerType) != 0)
			{
				RawButton rawButtonMask = RawButton.Any;
				RawTouch rawTouchMask = RawTouch.Any;

				if (Get(rawButtonMask, controller.controllerType)
					|| Get(rawTouchMask, controller.controllerType))
				{
					activeControllerType = controller.controllerType;
				}
			}
		}

		if ((activeControllerType == Controller.LTouch) || (activeControllerType == Controller.RTouch))
		{
			// If either Touch controller is Active, set both to Active.
			activeControllerType = Controller.Touch;
		}

		if ((connectedControllerTypes & activeControllerType) == 0)
		{
			activeControllerType = Controller.None;
		}

        // Promote TrackedRemote to Active if one is connected and no other controller is active
		if (activeControllerType == Controller.None)
		{
            if ((connectedControllerTypes & Controller.RTrackedRemote) != 0)
            {
                activeControllerType = Controller.RTrackedRemote;
            }
            else if ((connectedControllerTypes & Controller.LTrackedRemote) != 0)
            {
                activeControllerType = Controller.LTrackedRemote;
            }
		}

		if (pluginSupportsActiveController)
		{
			// override locally derived active and connected controllers if plugin provides more accurate data
			connectedControllerTypes = (OVRInput.Controller)OVRPlugin.GetConnectedControllers();
			activeControllerType = (OVRInput.Controller)OVRPlugin.GetActiveController();
		}
	}

	/// <summary>
	/// Updates the internal physics state of OVRInput. Must be called manually if used independently from OVRManager.
	/// </summary>
	public static void FixedUpdate()
	{
		stepType = OVRPlugin.Step.Physics;

		double predictionSeconds = (double)fixedUpdateCount * Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 1e-6f);
		fixedUpdateCount++;
		
		OVRPlugin.UpdateNodePhysicsPoses(0, predictionSeconds);
	}

	/// <summary>
	/// Returns true if the given Controller's orientation is currently tracked.
	/// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return false.
	/// </summary>
	public static bool GetControllerOrientationTracked(OVRInput.Controller controllerType)
	{
		switch (controllerType)
		{
			case Controller.LTouch:
            case Controller.LTrackedRemote:
                return OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.HandLeft);
            case Controller.RTouch:
            case Controller.RTrackedRemote:
                return OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.HandRight);
            default:
				return false;
		}
	}

	/// <summary>
	/// Returns true if the given Controller's position is currently tracked.
	/// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return false.
	/// </summary>
	public static bool GetControllerPositionTracked(OVRInput.Controller controllerType)
	{
		switch (controllerType)
		{
			case Controller.LTouch:
			case Controller.LTrackedRemote:
                return OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandLeft);
            case Controller.RTouch:
            case Controller.RTrackedRemote:
                return OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandRight);
            default:
				return false;
		}
	}

	/// <summary>
	/// Gets the position of the given Controller local to its tracking space.
	/// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
	/// </summary>
	public static Vector3 GetLocalControllerPosition(OVRInput.Controller controllerType)
	{
		switch (controllerType)
		{
			case Controller.LTouch:
			case Controller.LTrackedRemote:
                return OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, stepType).ToOVRPose().position;
            case Controller.RTouch:
			case Controller.RTrackedRemote:
                return OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, stepType).ToOVRPose().position;
            default:
				return Vector3.zero;
		}
	}

	/// <summary>
    /// Gets the linear velocity of the given Controller local to its tracking space.
    /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
    /// </summary>
    public static Vector3 GetLocalControllerVelocity(OVRInput.Controller controllerType)
    {
        switch (controllerType)
        {
            case Controller.LTouch:
			case Controller.LTrackedRemote:
				return OVRPlugin.GetNodeVelocity(OVRPlugin.Node.HandLeft, stepType).FromFlippedZVector3f();
            case Controller.RTouch:
			case Controller.RTrackedRemote:
				return OVRPlugin.GetNodeVelocity(OVRPlugin.Node.HandRight, stepType).FromFlippedZVector3f();
            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Gets the linear acceleration of the given Controller local to its tracking space.
    /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
    /// </summary>
    public static Vector3 GetLocalControllerAcceleration(OVRInput.Controller controllerType)
    {
        switch (controllerType)
        {
            case Controller.LTouch:
			case Controller.LTrackedRemote:
				return OVRPlugin.GetNodeAcceleration(OVRPlugin.Node.HandLeft, stepType).FromFlippedZVector3f();
            case Controller.RTouch:
			case Controller.RTrackedRemote:
				return OVRPlugin.GetNodeAcceleration(OVRPlugin.Node.HandRight, stepType).FromFlippedZVector3f();
            default:
                return Vector3.zero;
        }
    }

	/// <summary>
	/// Gets the rotation of the given Controller local to its tracking space.
	/// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Quaternion.identity.
	/// </summary>
	public static Quaternion GetLocalControllerRotation(OVRInput.Controller controllerType)
	{
		switch (controllerType)
		{
			case Controller.LTouch:
			case Controller.LTrackedRemote:
				return OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, stepType).ToOVRPose().orientation;
            case Controller.RTouch:
			case Controller.RTrackedRemote:
				return OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, stepType).ToOVRPose().orientation;
            default:
				return Quaternion.identity;
		}
	}

	/// <summary>
	/// Gets the angular velocity of the given Controller local to its tracking space in radians per second around each axis.
	/// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Quaternion.identity.
	/// </summary>
	public static Vector3 GetLocalControllerAngularVelocity(OVRInput.Controller controllerType)
	{
		switch (controllerType)
		{
		case Controller.LTouch:
		case Controller.LTrackedRemote:
			return OVRPlugin.GetNodeAngularVelocity(OVRPlugin.Node.HandLeft, stepType).FromFlippedZVector3f();
		case Controller.RTouch:
		case Controller.RTrackedRemote:
			return OVRPlugin.GetNodeAngularVelocity(OVRPlugin.Node.HandRight, stepType).FromFlippedZVector3f();
		default:
			return Vector3.zero;
		}
	}

	/// <summary>
	/// Gets the angular acceleration of the given Controller local to its tracking space in radians per second per second around each axis.
	/// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Quaternion.identity.
	/// </summary>
	public static Vector3 GetLocalControllerAngularAcceleration(OVRInput.Controller controllerType)
	{
		switch (controllerType)
		{
		case Controller.LTouch:
		case Controller.LTrackedRemote:
			return OVRPlugin.GetNodeAngularAcceleration(OVRPlugin.Node.HandLeft, stepType).FromFlippedZVector3f();
		case Controller.RTouch:
		case Controller.RTrackedRemote:
			return OVRPlugin.GetNodeAngularAcceleration(OVRPlugin.Node.HandRight, stepType).FromFlippedZVector3f();
		default:
			return Vector3.zero;
		}
	}

	/// <summary>
	/// Gets the current state of the given virtual button mask with the given controller mask.
	/// Returns true if any masked button is down on any masked controller.
	/// </summary>
	public static bool Get(Button virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButton(virtualMask, RawButton.None, controllerMask);
	}

	/// <summary>
	/// Gets the current state of the given raw button mask with the given controller mask.
	/// Returns true if any masked button is down on any masked controllers.
	/// </summary>
	public static bool Get(RawButton rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButton(Button.None, rawMask, controllerMask);
	}

	private static bool GetResolvedButton(Button virtualMask, RawButton rawMask, Controller controllerMask)
	{
		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawButton resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawButton)controller.currentState.Buttons & resolvedMask) != 0)
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Gets the current down state of the given virtual button mask with the given controller mask.
	/// Returns true if any masked button was pressed this frame on any masked controller and no masked button was previously down last frame.
	/// </summary>
	public static bool GetDown(Button virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButtonDown(virtualMask, RawButton.None, controllerMask);
	}

	/// <summary>
	/// Gets the current down state of the given raw button mask with the given controller mask.
	/// Returns true if any masked button was pressed this frame on any masked controller and no masked button was previously down last frame.
	/// </summary>
	public static bool GetDown(RawButton rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButtonDown(Button.None, rawMask, controllerMask);
	}

	private static bool GetResolvedButtonDown(Button virtualMask, RawButton rawMask, Controller controllerMask)
	{
		bool down = false;

		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawButton resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawButton)controller.previousState.Buttons & resolvedMask) != 0)
				{
					return false;
				}

				if ((((RawButton)controller.currentState.Buttons & resolvedMask) != 0)
					&& (((RawButton)controller.previousState.Buttons & resolvedMask) == 0))
				{
					down = true;
				}
			}
		}

		return down;
	}

	/// <summary>
	/// Gets the current up state of the given virtual button mask with the given controller mask.
	/// Returns true if any masked button was released this frame on any masked controller and no other masked button is still down this frame.
	/// </summary>
	public static bool GetUp(Button virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButtonUp(virtualMask, RawButton.None, controllerMask);
	}

	/// <summary>
	/// Gets the current up state of the given raw button mask with the given controller mask.
	/// Returns true if any masked button was released this frame on any masked controller and no other masked button is still down this frame.
	/// </summary>
	public static bool GetUp(RawButton rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButtonUp(Button.None, rawMask, controllerMask);
	}

	private static bool GetResolvedButtonUp(Button virtualMask, RawButton rawMask, Controller controllerMask)
	{
		bool up = false;

		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawButton resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawButton)controller.currentState.Buttons & resolvedMask) != 0)
				{
					return false;
				}

				if ((((RawButton)controller.currentState.Buttons & resolvedMask) == 0)
					&& (((RawButton)controller.previousState.Buttons & resolvedMask) != 0))
				{
					up = true;
				}
			}
		}

		return up;
	}

	/// <summary>
	/// Gets the current state of the given virtual touch mask with the given controller mask.
	/// Returns true if any masked touch is down on any masked controller.
	/// </summary>
	public static bool Get(Touch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouch(virtualMask, RawTouch.None, controllerMask);
	}

	/// <summary>
	/// Gets the current state of the given raw touch mask with the given controller mask.
	/// Returns true if any masked touch is down on any masked controllers.
	/// </summary>
	public static bool Get(RawTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouch(Touch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedTouch(Touch virtualMask, RawTouch rawMask, Controller controllerMask)
	{
		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawTouch resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawTouch)controller.currentState.Touches & resolvedMask) != 0)
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Gets the current down state of the given virtual touch mask with the given controller mask.
	/// Returns true if any masked touch was pressed this frame on any masked controller and no masked touch was previously down last frame.
	/// </summary>
	public static bool GetDown(Touch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouchDown(virtualMask, RawTouch.None, controllerMask);
	}

	/// <summary>
	/// Gets the current down state of the given raw touch mask with the given controller mask.
	/// Returns true if any masked touch was pressed this frame on any masked controller and no masked touch was previously down last frame.
	/// </summary>
	public static bool GetDown(RawTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouchDown(Touch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedTouchDown(Touch virtualMask, RawTouch rawMask, Controller controllerMask)
	{
		bool down = false;

		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawTouch resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawTouch)controller.previousState.Touches & resolvedMask) != 0)
				{
					return false;
				}

				if ((((RawTouch)controller.currentState.Touches & resolvedMask) != 0)
					&& (((RawTouch)controller.previousState.Touches & resolvedMask) == 0))
				{
					down = true;
				}
			}
		}

		return down;
	}

	/// <summary>
	/// Gets the current up state of the given virtual touch mask with the given controller mask.
	/// Returns true if any masked touch was released this frame on any masked controller and no other masked touch is still down this frame.
	/// </summary>
	public static bool GetUp(Touch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouchUp(virtualMask, RawTouch.None, controllerMask);
	}

	/// <summary>
	/// Gets the current up state of the given raw touch mask with the given controller mask.
	/// Returns true if any masked touch was released this frame on any masked controller and no other masked touch is still down this frame.
	/// </summary>
	public static bool GetUp(RawTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouchUp(Touch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedTouchUp(Touch virtualMask, RawTouch rawMask, Controller controllerMask)
	{
		bool up = false;

		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawTouch resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawTouch)controller.currentState.Touches & resolvedMask) != 0)
				{
					return false;
				}

				if ((((RawTouch)controller.currentState.Touches & resolvedMask) == 0)
					&& (((RawTouch)controller.previousState.Touches & resolvedMask) != 0))
				{
					up = true;
				}
			}
		}

		return up;
	}

	/// <summary>
	/// Gets the current state of the given virtual near touch mask with the given controller mask.
	/// Returns true if any masked near touch is down on any masked controller.
	/// </summary>
	public static bool Get(NearTouch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouch(virtualMask, RawNearTouch.None, controllerMask);
	}

	/// <summary>
	/// Gets the current state of the given raw near touch mask with the given controller mask.
	/// Returns true if any masked near touch is down on any masked controllers.
	/// </summary>
	public static bool Get(RawNearTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouch(NearTouch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedNearTouch(NearTouch virtualMask, RawNearTouch rawMask, Controller controllerMask)
	{
		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawNearTouch resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawNearTouch)controller.currentState.NearTouches & resolvedMask) != 0)
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Gets the current down state of the given virtual near touch mask with the given controller mask.
	/// Returns true if any masked near touch was pressed this frame on any masked controller and no masked near touch was previously down last frame.
	/// </summary>
	public static bool GetDown(NearTouch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouchDown(virtualMask, RawNearTouch.None, controllerMask);
	}

	/// <summary>
	/// Gets the current down state of the given raw near touch mask with the given controller mask.
	/// Returns true if any masked near touch was pressed this frame on any masked controller and no masked near touch was previously down last frame.
	/// </summary>
	public static bool GetDown(RawNearTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouchDown(NearTouch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedNearTouchDown(NearTouch virtualMask, RawNearTouch rawMask, Controller controllerMask)
	{
		bool down = false;

		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawNearTouch resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawNearTouch)controller.previousState.NearTouches & resolvedMask) != 0)
				{
					return false;
				}

				if ((((RawNearTouch)controller.currentState.NearTouches & resolvedMask) != 0)
					&& (((RawNearTouch)controller.previousState.NearTouches & resolvedMask) == 0))
				{
					down = true;
				}
			}
		}

		return down;
	}

	/// <summary>
	/// Gets the current up state of the given virtual near touch mask with the given controller mask.
	/// Returns true if any masked near touch was released this frame on any masked controller and no other masked near touch is still down this frame.
	/// </summary>
	public static bool GetUp(NearTouch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouchUp(virtualMask, RawNearTouch.None, controllerMask);
	}

	/// <summary>
	/// Gets the current up state of the given raw near touch mask with the given controller mask.
	/// Returns true if any masked near touch was released this frame on any masked controller and no other masked near touch is still down this frame.
	/// </summary>
	public static bool GetUp(RawNearTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouchUp(NearTouch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedNearTouchUp(NearTouch virtualMask, RawNearTouch rawMask, Controller controllerMask)
	{
		bool up = false;

		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawNearTouch resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if (((RawNearTouch)controller.currentState.NearTouches & resolvedMask) != 0)
				{
					return false;
				}

				if ((((RawNearTouch)controller.currentState.NearTouches & resolvedMask) == 0)
					&& (((RawNearTouch)controller.previousState.NearTouches & resolvedMask) != 0))
				{
					up = true;
				}
			}
		}

		return up;
	}

	/// <summary>
	/// Gets the current state of the given virtual 1-dimensional axis mask on the given controller mask.
	/// Returns the value of the largest masked axis across all masked controllers. Values range from 0 to 1.
	/// </summary>
	public static float Get(Axis1D virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedAxis1D(virtualMask, RawAxis1D.None, controllerMask);
	}

	/// <summary>
	/// Gets the current state of the given raw 1-dimensional axis mask on the given controller mask.
	/// Returns the value of the largest masked axis across all masked controllers. Values range from 0 to 1.
	/// </summary>
	public static float Get(RawAxis1D rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedAxis1D(Axis1D.None, rawMask, controllerMask);
	}

	private static float GetResolvedAxis1D(Axis1D virtualMask, RawAxis1D rawMask, Controller controllerMask)
	{
		float maxAxis = 0.0f;

		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawAxis1D resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if ((RawAxis1D.LIndexTrigger & resolvedMask) != 0)
				{
					float axis = controller.currentState.LIndexTrigger;

					if (controller.shouldApplyDeadzone)
						axis = CalculateDeadzone(axis, AXIS_DEADZONE_THRESHOLD);

					maxAxis = CalculateAbsMax(maxAxis, axis);
				}
				if ((RawAxis1D.RIndexTrigger & resolvedMask) != 0)
				{
					float axis = controller.currentState.RIndexTrigger;

					if (controller.shouldApplyDeadzone)
						axis = CalculateDeadzone(axis, AXIS_DEADZONE_THRESHOLD);

					maxAxis = CalculateAbsMax(maxAxis, axis);
				}
				if ((RawAxis1D.LHandTrigger & resolvedMask) != 0)
				{
					float axis = controller.currentState.LHandTrigger;

					if (controller.shouldApplyDeadzone)
						axis = CalculateDeadzone(axis, AXIS_DEADZONE_THRESHOLD);

					maxAxis = CalculateAbsMax(maxAxis, axis);
				}
				if ((RawAxis1D.RHandTrigger & resolvedMask) != 0)
				{
					float axis = controller.currentState.RHandTrigger;

					if (controller.shouldApplyDeadzone)
						axis = CalculateDeadzone(axis, AXIS_DEADZONE_THRESHOLD);

					maxAxis = CalculateAbsMax(maxAxis, axis);
				}
			}
		}

		return maxAxis;
	}

	/// <summary>
	/// Gets the current state of the given virtual 2-dimensional axis mask on the given controller mask.
	/// Returns the vector of the largest masked axis across all masked controllers. Values range from -1 to 1.
	/// </summary>
	public static Vector2 Get(Axis2D virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedAxis2D(virtualMask, RawAxis2D.None, controllerMask);
	}

	/// <summary>
	/// Gets the current state of the given raw 2-dimensional axis mask on the given controller mask.
	/// Returns the vector of the largest masked axis across all masked controllers. Values range from -1 to 1.
	/// </summary>
	public static Vector2 Get(RawAxis2D rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedAxis2D(Axis2D.None, rawMask, controllerMask);
	}

	private static Vector2 GetResolvedAxis2D(Axis2D virtualMask, RawAxis2D rawMask, Controller controllerMask)
	{
		Vector2 maxAxis = Vector2.zero;

		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				RawAxis2D resolvedMask = rawMask | controller.ResolveToRawMask(virtualMask);

				if ((RawAxis2D.LThumbstick & resolvedMask) != 0)
				{
					Vector2 axis = new Vector2(
						controller.currentState.LThumbstick.x,
						controller.currentState.LThumbstick.y);

					if (controller.shouldApplyDeadzone)
						axis = CalculateDeadzone(axis, AXIS_DEADZONE_THRESHOLD);

					maxAxis = CalculateAbsMax(maxAxis, axis);
				}
				if ((RawAxis2D.LTouchpad & resolvedMask) != 0)
				{
					Vector2 axis = new Vector2(
						controller.currentState.LTouchpad.x,
						controller.currentState.LTouchpad.y);

					//if (controller.shouldApplyDeadzone)
					//	axis = CalculateDeadzone(axis, AXIS_DEADZONE_THRESHOLD);

					maxAxis = CalculateAbsMax(maxAxis, axis);
				}
				if ((RawAxis2D.RThumbstick & resolvedMask) != 0)
				{
					Vector2 axis = new Vector2(
						controller.currentState.RThumbstick.x,
						controller.currentState.RThumbstick.y);

					if (controller.shouldApplyDeadzone)
						axis = CalculateDeadzone(axis, AXIS_DEADZONE_THRESHOLD);

					maxAxis = CalculateAbsMax(maxAxis, axis);
				}
				if ((RawAxis2D.RTouchpad & resolvedMask) != 0)
				{
					Vector2 axis = new Vector2(
						controller.currentState.RTouchpad.x,
						controller.currentState.RTouchpad.y);

					//if (controller.shouldApplyDeadzone)
					//	axis = CalculateDeadzone(axis, AXIS_DEADZONE_THRESHOLD);

					maxAxis = CalculateAbsMax(maxAxis, axis);
				}
			}
		}

		return maxAxis;
	}

	/// <summary>
	/// Returns a mask of all currently connected controller types.
	/// </summary>
	public static Controller GetConnectedControllers()
	{
		return connectedControllerTypes;
	}
    
	/// <summary>
	/// Returns true if the specified controller type is currently connected.
	/// </summary>
	public static bool IsControllerConnected(Controller controller)
	{
		return (connectedControllerTypes & controller) == controller;
	}

	/// <summary>
	/// Returns the current active controller type.
	/// </summary>
	public static Controller GetActiveController()
	{
		return activeControllerType;
	}

	/// <summary>
	/// Activates vibration with the given frequency and amplitude with the given controller mask.
	/// Ignored on controllers that do not support vibration. Expected values range from 0 to 1.
	/// </summary>
	public static void SetControllerVibration(float frequency, float amplitude, Controller controllerMask = Controller.Active)
	{
		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				controller.SetControllerVibration(frequency, amplitude);
			}
		}
	}

	/// <summary>
	/// Triggers a recenter to realign the specified controller's virtual pose with the user's real-world pose.
	/// Only applicable to controllers that require recentering, such as the GearVR Controller.
	/// Ignored for controllers that do not require recentering.
	/// </summary>
	public static void RecenterController(Controller controllerMask = Controller.Active)
	{
		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				controller.RecenterController();
			}
		}
	}

	/// <summary>
	/// Returns true if the specified controller was recentered this frame.
	/// Only applicable to controllers that require recentering, such as the GearVR Controller.
	/// Returns false for controllers that do not require recentering.
	/// </summary>
	public static bool GetControllerWasRecentered(Controller controllerMask = Controller.Active)
	{
		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		bool wasRecentered = false;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				wasRecentered |= controller.WasRecentered();
			}
		}

		return wasRecentered;
	}

	/// <summary>
	/// Returns the number of times the controller has been recentered this session.
	/// Useful for detecting recenter events and resetting state such as arm model simulations, etc.
	/// Wraps around to 0 after 255.
	/// Only applicable to controllers that require recentering, such as the GearVR Controller.
	/// Returns 0 for controllers that do not require recentering.
	/// </summary>
	public static byte GetControllerRecenterCount(Controller controllerMask = Controller.Active)
	{
		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		byte recenterCount = 0;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				recenterCount = controller.GetRecenterCount();
				break;
			}
		}

		return recenterCount;
	}

	/// <summary>
	/// Returns the battery percentage remaining for the specified controller. Values range from 0 to 100.
	/// Only applicable to controllers that report battery level, such as the GearVR Controller.
	/// Returns 0 for controllers that do not report battery level.
	/// </summary>
	public static byte GetControllerBatteryPercentRemaining(Controller controllerMask = Controller.Active)
	{
		if ((controllerMask & Controller.Active) != 0)
			controllerMask |= activeControllerType;

		byte battery = 0;

		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase controller = controllers[i];

			if (ShouldResolveController(controller.controllerType, controllerMask))
			{
				battery = controller.GetBatteryPercentRemaining();
				break;
			}
		}

		return battery;
	}

	private static Vector2 CalculateAbsMax(Vector2 a, Vector2 b)
	{
		float absA = a.sqrMagnitude;
		float absB = b.sqrMagnitude;

		if (absA >= absB)
			return a;
		return b;
	}

	private static float CalculateAbsMax(float a, float b)
	{
		float absA = (a >= 0) ? a : -a;
		float absB = (b >= 0) ? b : -b;

		if (absA >= absB)
			return a;
		return b;
	}

	private static Vector2 CalculateDeadzone(Vector2 a, float deadzone)
	{
		if (a.sqrMagnitude <= (deadzone * deadzone))
			return Vector2.zero;

		a *= ((a.magnitude - deadzone) / (1.0f - deadzone));

		if (a.sqrMagnitude > 1.0f)
			return a.normalized;
		return a;
	}

	private static float CalculateDeadzone(float a, float deadzone)
	{
		float mag = (a >= 0) ? a : -a;

		if (mag <= deadzone)
			return 0.0f;

		a *= (mag - deadzone) / (1.0f - deadzone);

		if ((a * a) > 1.0f)
			return (a >= 0) ? 1.0f : -1.0f;
		return a;
	}

	private static bool ShouldResolveController(Controller controllerType, Controller controllerMask)
	{
		bool isValid = false;

		if ((controllerType & controllerMask) == controllerType)
		{
			isValid = true;
		}

		// If the mask requests both Touch controllers, reject the individual touch controllers.
		if (((controllerMask & Controller.Touch) == Controller.Touch)
			&& ((controllerType & Controller.Touch) != 0)
			&& ((controllerType & Controller.Touch) != Controller.Touch))
		{
			isValid = false;
		}

		return isValid;
	}

	private abstract class OVRControllerBase
	{
		public class VirtualButtonMap
		{
			public RawButton None                     = RawButton.None;
			public RawButton One                      = RawButton.None;
			public RawButton Two                      = RawButton.None;
			public RawButton Three                    = RawButton.None;
			public RawButton Four                     = RawButton.None;
			public RawButton Start                    = RawButton.None;
			public RawButton Back                     = RawButton.None;
			public RawButton PrimaryShoulder          = RawButton.None;
			public RawButton PrimaryIndexTrigger      = RawButton.None;
			public RawButton PrimaryHandTrigger       = RawButton.None;
			public RawButton PrimaryThumbstick        = RawButton.None;
			public RawButton PrimaryThumbstickUp      = RawButton.None;
			public RawButton PrimaryThumbstickDown    = RawButton.None;
			public RawButton PrimaryThumbstickLeft    = RawButton.None;
			public RawButton PrimaryThumbstickRight   = RawButton.None;
			public RawButton PrimaryTouchpad          = RawButton.None;
			public RawButton SecondaryShoulder        = RawButton.None;
			public RawButton SecondaryIndexTrigger    = RawButton.None;
			public RawButton SecondaryHandTrigger     = RawButton.None;
			public RawButton SecondaryThumbstick      = RawButton.None;
			public RawButton SecondaryThumbstickUp    = RawButton.None;
			public RawButton SecondaryThumbstickDown  = RawButton.None;
			public RawButton SecondaryThumbstickLeft  = RawButton.None;
			public RawButton SecondaryThumbstickRight = RawButton.None;
			public RawButton SecondaryTouchpad        = RawButton.None;
			public RawButton DpadUp                   = RawButton.None;
			public RawButton DpadDown                 = RawButton.None;
			public RawButton DpadLeft                 = RawButton.None;
			public RawButton DpadRight                = RawButton.None;
			public RawButton Up                       = RawButton.None;
			public RawButton Down                     = RawButton.None;
			public RawButton Left                     = RawButton.None;
			public RawButton Right                    = RawButton.None;

			public RawButton ToRawMask(Button virtualMask)
			{
				RawButton rawMask = 0;

				if (virtualMask == Button.None)
					return RawButton.None;

				if ((virtualMask & Button.One) != 0)
					rawMask |= One;
				if ((virtualMask & Button.Two) != 0)
					rawMask |= Two;
				if ((virtualMask & Button.Three) != 0)
					rawMask |= Three;
				if ((virtualMask & Button.Four) != 0)
					rawMask |= Four;
				if ((virtualMask & Button.Start) != 0)
					rawMask |= Start;
				if ((virtualMask & Button.Back) != 0)
					rawMask |= Back;
				if ((virtualMask & Button.PrimaryShoulder) != 0)
					rawMask |= PrimaryShoulder;
				if ((virtualMask & Button.PrimaryIndexTrigger) != 0)
					rawMask |= PrimaryIndexTrigger;
				if ((virtualMask & Button.PrimaryHandTrigger) != 0)
					rawMask |= PrimaryHandTrigger;
				if ((virtualMask & Button.PrimaryThumbstick) != 0)
					rawMask |= PrimaryThumbstick;
				if ((virtualMask & Button.PrimaryThumbstickUp) != 0)
					rawMask |= PrimaryThumbstickUp;
				if ((virtualMask & Button.PrimaryThumbstickDown) != 0)
					rawMask |= PrimaryThumbstickDown;
				if ((virtualMask & Button.PrimaryThumbstickLeft) != 0)
					rawMask |= PrimaryThumbstickLeft;
				if ((virtualMask & Button.PrimaryThumbstickRight) != 0)
					rawMask |= PrimaryThumbstickRight;
				if ((virtualMask & Button.PrimaryTouchpad) != 0)
					rawMask |= PrimaryTouchpad;
				if ((virtualMask & Button.SecondaryShoulder) != 0)
					rawMask |= SecondaryShoulder;
				if ((virtualMask & Button.SecondaryIndexTrigger) != 0)
					rawMask |= SecondaryIndexTrigger;
				if ((virtualMask & Button.SecondaryHandTrigger) != 0)
					rawMask |= SecondaryHandTrigger;
				if ((virtualMask & Button.SecondaryThumbstick) != 0)
					rawMask |= SecondaryThumbstick;
				if ((virtualMask & Button.SecondaryThumbstickUp) != 0)
					rawMask |= SecondaryThumbstickUp;
				if ((virtualMask & Button.SecondaryThumbstickDown) != 0)
					rawMask |= SecondaryThumbstickDown;
				if ((virtualMask & Button.SecondaryThumbstickLeft) != 0)
					rawMask |= SecondaryThumbstickLeft;
				if ((virtualMask & Button.SecondaryThumbstickRight) != 0)
					rawMask |= SecondaryThumbstickRight;
				if ((virtualMask & Button.SecondaryTouchpad) != 0)
					rawMask |= SecondaryTouchpad;
				if ((virtualMask & Button.DpadUp) != 0)
					rawMask |= DpadUp;
				if ((virtualMask & Button.DpadDown) != 0)
					rawMask |= DpadDown;
				if ((virtualMask & Button.DpadLeft) != 0)
					rawMask |= DpadLeft;
				if ((virtualMask & Button.DpadRight) != 0)
					rawMask |= DpadRight;
				if ((virtualMask & Button.Up) != 0)
					rawMask |= Up;
				if ((virtualMask & Button.Down) != 0)
					rawMask |= Down;
				if ((virtualMask & Button.Left) != 0)
					rawMask |= Left;
				if ((virtualMask & Button.Right) != 0)
					rawMask |= Right;

				return rawMask;
			}
		}

		public class VirtualTouchMap
		{
			public RawTouch None                      = RawTouch.None;
			public RawTouch One                       = RawTouch.None;
			public RawTouch Two                       = RawTouch.None;
			public RawTouch Three                     = RawTouch.None;
			public RawTouch Four                      = RawTouch.None;
			public RawTouch PrimaryIndexTrigger       = RawTouch.None;
			public RawTouch PrimaryThumbstick         = RawTouch.None;
			public RawTouch PrimaryThumbRest          = RawTouch.None;
			public RawTouch PrimaryTouchpad           = RawTouch.None;
			public RawTouch SecondaryIndexTrigger     = RawTouch.None;
			public RawTouch SecondaryThumbstick       = RawTouch.None;
			public RawTouch SecondaryThumbRest        = RawTouch.None;
			public RawTouch SecondaryTouchpad         = RawTouch.None;

			public RawTouch ToRawMask(Touch virtualMask)
			{
				RawTouch rawMask = 0;

				if (virtualMask == Touch.None)
					return RawTouch.None;

				if ((virtualMask & Touch.One) != 0)
					rawMask |= One;
				if ((virtualMask & Touch.Two) != 0)
					rawMask |= Two;
				if ((virtualMask & Touch.Three) != 0)
					rawMask |= Three;
				if ((virtualMask & Touch.Four) != 0)
					rawMask |= Four;
				if ((virtualMask & Touch.PrimaryIndexTrigger) != 0)
					rawMask |= PrimaryIndexTrigger;
				if ((virtualMask & Touch.PrimaryThumbstick) != 0)
					rawMask |= PrimaryThumbstick;
				if ((virtualMask & Touch.PrimaryThumbRest) != 0)
					rawMask |= PrimaryThumbRest;
				if ((virtualMask & Touch.PrimaryTouchpad) != 0)
					rawMask |= PrimaryTouchpad;
				if ((virtualMask & Touch.SecondaryIndexTrigger) != 0)
					rawMask |= SecondaryIndexTrigger;
				if ((virtualMask & Touch.SecondaryThumbstick) != 0)
					rawMask |= SecondaryThumbstick;
				if ((virtualMask & Touch.SecondaryThumbRest) != 0)
					rawMask |= SecondaryThumbRest;
				if ((virtualMask & Touch.SecondaryTouchpad) != 0)
					rawMask |= SecondaryTouchpad;

				return rawMask;
			}
		}

		public class VirtualNearTouchMap
		{
			public RawNearTouch None                      = RawNearTouch.None;
			public RawNearTouch PrimaryIndexTrigger       = RawNearTouch.None;
			public RawNearTouch PrimaryThumbButtons       = RawNearTouch.None;
			public RawNearTouch SecondaryIndexTrigger     = RawNearTouch.None;
			public RawNearTouch SecondaryThumbButtons     = RawNearTouch.None;

			public RawNearTouch ToRawMask(NearTouch virtualMask)
			{
				RawNearTouch rawMask = 0;

				if (virtualMask == NearTouch.None)
					return RawNearTouch.None;

				if ((virtualMask & NearTouch.PrimaryIndexTrigger) != 0)
					rawMask |= PrimaryIndexTrigger;
				if ((virtualMask & NearTouch.PrimaryThumbButtons) != 0)
					rawMask |= PrimaryThumbButtons;
				if ((virtualMask & NearTouch.SecondaryIndexTrigger) != 0)
					rawMask |= SecondaryIndexTrigger;
				if ((virtualMask & NearTouch.SecondaryThumbButtons) != 0)
					rawMask |= SecondaryThumbButtons;

				return rawMask;
			}
		}

		public class VirtualAxis1DMap
		{
			public RawAxis1D None                      = RawAxis1D.None;
			public RawAxis1D PrimaryIndexTrigger       = RawAxis1D.None;
			public RawAxis1D PrimaryHandTrigger        = RawAxis1D.None;
			public RawAxis1D SecondaryIndexTrigger     = RawAxis1D.None;
			public RawAxis1D SecondaryHandTrigger      = RawAxis1D.None;

			public RawAxis1D ToRawMask(Axis1D virtualMask)
			{
				RawAxis1D rawMask = 0;

				if (virtualMask == Axis1D.None)
					return RawAxis1D.None;

				if ((virtualMask & Axis1D.PrimaryIndexTrigger) != 0)
					rawMask |= PrimaryIndexTrigger;
				if ((virtualMask & Axis1D.PrimaryHandTrigger) != 0)
					rawMask |= PrimaryHandTrigger;
				if ((virtualMask & Axis1D.SecondaryIndexTrigger) != 0)
					rawMask |= SecondaryIndexTrigger;
				if ((virtualMask & Axis1D.SecondaryHandTrigger) != 0)
					rawMask |= SecondaryHandTrigger;

				return rawMask;
			}
		}

		public class VirtualAxis2DMap
		{
			public RawAxis2D None                      = RawAxis2D.None;
			public RawAxis2D PrimaryThumbstick         = RawAxis2D.None;
			public RawAxis2D PrimaryTouchpad           = RawAxis2D.None;
			public RawAxis2D SecondaryThumbstick       = RawAxis2D.None;
			public RawAxis2D SecondaryTouchpad         = RawAxis2D.None;

			public RawAxis2D ToRawMask(Axis2D virtualMask)
			{
				RawAxis2D rawMask = 0;

				if (virtualMask == Axis2D.None)
					return RawAxis2D.None;

				if ((virtualMask & Axis2D.PrimaryThumbstick) != 0)
					rawMask |= PrimaryThumbstick;
				if ((virtualMask & Axis2D.PrimaryTouchpad) != 0)
					rawMask |= PrimaryTouchpad;
				if ((virtualMask & Axis2D.SecondaryThumbstick) != 0)
					rawMask |= SecondaryThumbstick;
				if ((virtualMask & Axis2D.SecondaryTouchpad) != 0)
					rawMask |= SecondaryTouchpad;

				return rawMask;
			}
		}

		public Controller controllerType = Controller.None;
		public VirtualButtonMap buttonMap = new VirtualButtonMap();
		public VirtualTouchMap touchMap = new VirtualTouchMap();
		public VirtualNearTouchMap nearTouchMap = new VirtualNearTouchMap();
		public VirtualAxis1DMap axis1DMap = new VirtualAxis1DMap();
		public VirtualAxis2DMap axis2DMap = new VirtualAxis2DMap();
		public OVRPlugin.ControllerState4 previousState = new OVRPlugin.ControllerState4();
		public OVRPlugin.ControllerState4 currentState = new OVRPlugin.ControllerState4();
		public bool shouldApplyDeadzone = true;

		public OVRControllerBase()
		{
			ConfigureButtonMap();
			ConfigureTouchMap();
			ConfigureNearTouchMap();
			ConfigureAxis1DMap();
			ConfigureAxis2DMap();
		}

		public virtual Controller Update()
		{
			OVRPlugin.ControllerState4 state = OVRPlugin.GetControllerState4((uint)controllerType);

			if (state.LIndexTrigger >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LIndexTrigger;
			if (state.LHandTrigger >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LHandTrigger;
			if (state.LThumbstick.y >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LThumbstickUp;
			if (state.LThumbstick.y <= -AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LThumbstickDown;
			if (state.LThumbstick.x <= -AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LThumbstickLeft;
			if (state.LThumbstick.x >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LThumbstickRight;

			if (state.RIndexTrigger >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RIndexTrigger;
			if (state.RHandTrigger >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RHandTrigger;
			if (state.RThumbstick.y >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RThumbstickUp;
			if (state.RThumbstick.y <= -AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RThumbstickDown;
			if (state.RThumbstick.x <= -AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RThumbstickLeft;
			if (state.RThumbstick.x >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RThumbstickRight;

			previousState = currentState;
			currentState = state;

			return ((Controller)currentState.ConnectedControllers & controllerType);
		}

		public virtual void SetControllerVibration(float frequency, float amplitude)
		{
			OVRPlugin.SetControllerVibration((uint)controllerType, frequency, amplitude);
		}

		public virtual void RecenterController()
		{
			OVRPlugin.RecenterTrackingOrigin(OVRPlugin.RecenterFlags.Controllers);
		}

		public virtual bool WasRecentered()
		{
			return false;
		}

		public virtual byte GetRecenterCount()
		{
			return 0;
		}

		public virtual byte GetBatteryPercentRemaining()
		{
			return 0;
		}

		public abstract void ConfigureButtonMap();
		public abstract void ConfigureTouchMap();
		public abstract void ConfigureNearTouchMap();
		public abstract void ConfigureAxis1DMap();
		public abstract void ConfigureAxis2DMap();

		public RawButton ResolveToRawMask(Button virtualMask)
		{
			return buttonMap.ToRawMask(virtualMask);
		}

		public RawTouch ResolveToRawMask(Touch virtualMask)
		{
			return touchMap.ToRawMask(virtualMask);
		}

		public RawNearTouch ResolveToRawMask(NearTouch virtualMask)
		{
			return nearTouchMap.ToRawMask(virtualMask);
		}

		public RawAxis1D ResolveToRawMask(Axis1D virtualMask)
		{
			return axis1DMap.ToRawMask(virtualMask);
		}

		public RawAxis2D ResolveToRawMask(Axis2D virtualMask)
		{
			return axis2DMap.ToRawMask(virtualMask);
		}
	}

	private class OVRControllerTouch : OVRControllerBase
	{
		public OVRControllerTouch()
		{
			controllerType = Controller.Touch;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.A;
			buttonMap.Two                      = RawButton.B;
			buttonMap.Three                    = RawButton.X;
			buttonMap.Four                     = RawButton.Y;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.None;
			buttonMap.PrimaryShoulder          = RawButton.None;
			buttonMap.PrimaryIndexTrigger      = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger       = RawButton.LHandTrigger;
			buttonMap.PrimaryThumbstick        = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp      = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown    = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft    = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight   = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad          = RawButton.None;
			buttonMap.SecondaryShoulder        = RawButton.None;
			buttonMap.SecondaryIndexTrigger    = RawButton.RIndexTrigger;
			buttonMap.SecondaryHandTrigger     = RawButton.RHandTrigger;
			buttonMap.SecondaryThumbstick      = RawButton.RThumbstick;
			buttonMap.SecondaryThumbstickUp    = RawButton.RThumbstickUp;
			buttonMap.SecondaryThumbstickDown  = RawButton.RThumbstickDown;
			buttonMap.SecondaryThumbstickLeft  = RawButton.RThumbstickLeft;
			buttonMap.SecondaryThumbstickRight = RawButton.RThumbstickRight;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.None;
			buttonMap.DpadDown                 = RawButton.None;
			buttonMap.DpadLeft                 = RawButton.None;
			buttonMap.DpadRight                = RawButton.None;
			buttonMap.Up                       = RawButton.LThumbstickUp;
			buttonMap.Down                     = RawButton.LThumbstickDown;
			buttonMap.Left                     = RawButton.LThumbstickLeft;
			buttonMap.Right                    = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.A;
			touchMap.Two                       = RawTouch.B;
			touchMap.Three                     = RawTouch.X;
			touchMap.Four                      = RawTouch.Y;
			touchMap.PrimaryIndexTrigger       = RawTouch.LIndexTrigger;
			touchMap.PrimaryThumbstick         = RawTouch.LThumbstick;
			touchMap.PrimaryThumbRest          = RawTouch.LThumbRest;
			touchMap.PrimaryTouchpad           = RawTouch.None;
			touchMap.SecondaryIndexTrigger     = RawTouch.RIndexTrigger;
			touchMap.SecondaryThumbstick       = RawTouch.RThumbstick;
			touchMap.SecondaryThumbRest        = RawTouch.RThumbRest;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                      = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger       = RawNearTouch.LIndexTrigger;
			nearTouchMap.PrimaryThumbButtons       = RawNearTouch.LThumbButtons;
			nearTouchMap.SecondaryIndexTrigger     = RawNearTouch.RIndexTrigger;
			nearTouchMap.SecondaryThumbButtons     = RawNearTouch.RThumbButtons;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                      = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger       = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger        = RawAxis1D.LHandTrigger;
			axis1DMap.SecondaryIndexTrigger     = RawAxis1D.RIndexTrigger;
			axis1DMap.SecondaryHandTrigger      = RawAxis1D.RHandTrigger;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                      = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick         = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad           = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick       = RawAxis2D.RThumbstick;
			axis2DMap.SecondaryTouchpad         = RawAxis2D.None;
		}
	}

	private class OVRControllerLTouch : OVRControllerBase
	{
		public OVRControllerLTouch()
		{
			controllerType = Controller.LTouch;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.X;
			buttonMap.Two                      = RawButton.Y;
			buttonMap.Three                    = RawButton.None;
			buttonMap.Four                     = RawButton.None;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.None;
			buttonMap.PrimaryShoulder          = RawButton.None;
			buttonMap.PrimaryIndexTrigger      = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger       = RawButton.LHandTrigger;
			buttonMap.PrimaryThumbstick        = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp      = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown    = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft    = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight   = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad          = RawButton.None;
			buttonMap.SecondaryShoulder        = RawButton.None;
			buttonMap.SecondaryIndexTrigger    = RawButton.None;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.None;
			buttonMap.SecondaryThumbstickUp    = RawButton.None;
			buttonMap.SecondaryThumbstickDown  = RawButton.None;
			buttonMap.SecondaryThumbstickLeft  = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.None;
			buttonMap.DpadDown                 = RawButton.None;
			buttonMap.DpadLeft                 = RawButton.None;
			buttonMap.DpadRight                = RawButton.None;
			buttonMap.Up                       = RawButton.LThumbstickUp;
			buttonMap.Down                     = RawButton.LThumbstickDown;
			buttonMap.Left                     = RawButton.LThumbstickLeft;
			buttonMap.Right                    = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.X;
			touchMap.Two                       = RawTouch.Y;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.LIndexTrigger;
			touchMap.PrimaryThumbstick         = RawTouch.LThumbstick;
			touchMap.PrimaryThumbRest          = RawTouch.LThumbRest;
			touchMap.PrimaryTouchpad           = RawTouch.None;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                      = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger       = RawNearTouch.LIndexTrigger;
			nearTouchMap.PrimaryThumbButtons       = RawNearTouch.LThumbButtons;
			nearTouchMap.SecondaryIndexTrigger     = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons     = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                      = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger       = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger        = RawAxis1D.LHandTrigger;
			axis1DMap.SecondaryIndexTrigger     = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger      = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                      = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick         = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad           = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick       = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad         = RawAxis2D.None;
		}
	}

	private class OVRControllerRTouch : OVRControllerBase
	{
		public OVRControllerRTouch()
		{
			controllerType = Controller.RTouch;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.A;
			buttonMap.Two                      = RawButton.B;
			buttonMap.Three                    = RawButton.None;
			buttonMap.Four                     = RawButton.None;
			buttonMap.Start                    = RawButton.None;
			buttonMap.Back                     = RawButton.None;
			buttonMap.PrimaryShoulder          = RawButton.None;
			buttonMap.PrimaryIndexTrigger      = RawButton.RIndexTrigger;
			buttonMap.PrimaryHandTrigger       = RawButton.RHandTrigger;
			buttonMap.PrimaryThumbstick        = RawButton.RThumbstick;
			buttonMap.PrimaryThumbstickUp      = RawButton.RThumbstickUp;
			buttonMap.PrimaryThumbstickDown    = RawButton.RThumbstickDown;
			buttonMap.PrimaryThumbstickLeft    = RawButton.RThumbstickLeft;
			buttonMap.PrimaryThumbstickRight   = RawButton.RThumbstickRight;
			buttonMap.PrimaryTouchpad          = RawButton.None;
			buttonMap.SecondaryShoulder        = RawButton.None;
			buttonMap.SecondaryIndexTrigger    = RawButton.None;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.None;
			buttonMap.SecondaryThumbstickUp    = RawButton.None;
			buttonMap.SecondaryThumbstickDown  = RawButton.None;
			buttonMap.SecondaryThumbstickLeft  = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.None;
			buttonMap.DpadDown                 = RawButton.None;
			buttonMap.DpadLeft                 = RawButton.None;
			buttonMap.DpadRight                = RawButton.None;
			buttonMap.Up                       = RawButton.RThumbstickUp;
			buttonMap.Down                     = RawButton.RThumbstickDown;
			buttonMap.Left                     = RawButton.RThumbstickLeft;
			buttonMap.Right                    = RawButton.RThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.A;
			touchMap.Two                       = RawTouch.B;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.RIndexTrigger;
			touchMap.PrimaryThumbstick         = RawTouch.RThumbstick;
			touchMap.PrimaryThumbRest          = RawTouch.RThumbRest;
			touchMap.PrimaryTouchpad           = RawTouch.None;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                      = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger       = RawNearTouch.RIndexTrigger;
			nearTouchMap.PrimaryThumbButtons       = RawNearTouch.RThumbButtons;
			nearTouchMap.SecondaryIndexTrigger     = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons     = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                      = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger       = RawAxis1D.RIndexTrigger;
			axis1DMap.PrimaryHandTrigger        = RawAxis1D.RHandTrigger;
			axis1DMap.SecondaryIndexTrigger     = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger      = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                      = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick         = RawAxis2D.RThumbstick;
			axis2DMap.PrimaryTouchpad           = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick       = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad         = RawAxis2D.None;
		}
	}

	private class OVRControllerRemote : OVRControllerBase
	{
		public OVRControllerRemote()
		{
			controllerType = Controller.Remote;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.Start;
			buttonMap.Two                      = RawButton.Back;
			buttonMap.Three                    = RawButton.None;
			buttonMap.Four                     = RawButton.None;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.Back;
			buttonMap.PrimaryShoulder          = RawButton.None;
			buttonMap.PrimaryIndexTrigger      = RawButton.None;
			buttonMap.PrimaryHandTrigger       = RawButton.None;
			buttonMap.PrimaryThumbstick        = RawButton.None;
			buttonMap.PrimaryThumbstickUp      = RawButton.None;
			buttonMap.PrimaryThumbstickDown    = RawButton.None;
			buttonMap.PrimaryThumbstickLeft    = RawButton.None;
			buttonMap.PrimaryThumbstickRight   = RawButton.None;
			buttonMap.PrimaryTouchpad          = RawButton.None;
			buttonMap.SecondaryShoulder        = RawButton.None;
			buttonMap.SecondaryIndexTrigger    = RawButton.None;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.None;
			buttonMap.SecondaryThumbstickUp    = RawButton.None;
			buttonMap.SecondaryThumbstickDown  = RawButton.None;
			buttonMap.SecondaryThumbstickLeft  = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.DpadUp;
			buttonMap.DpadDown                 = RawButton.DpadDown;
			buttonMap.DpadLeft                 = RawButton.DpadLeft;
			buttonMap.DpadRight                = RawButton.DpadRight;
			buttonMap.Up                       = RawButton.DpadUp;
			buttonMap.Down                     = RawButton.DpadDown;
			buttonMap.Left                     = RawButton.DpadLeft;
			buttonMap.Right                    = RawButton.DpadRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.None;
			touchMap.Two                       = RawTouch.None;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.None;
			touchMap.PrimaryThumbstick         = RawTouch.None;
			touchMap.PrimaryThumbRest          = RawTouch.None;
			touchMap.PrimaryTouchpad           = RawTouch.None;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                  = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger   = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons   = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                     = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger      = RawAxis1D.None;
			axis1DMap.PrimaryHandTrigger       = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger    = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger     = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                     = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick        = RawAxis2D.None;
			axis2DMap.PrimaryTouchpad          = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick      = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad        = RawAxis2D.None;
		}
	}

	private class OVRControllerGamepadPC : OVRControllerBase
	{
		public OVRControllerGamepadPC()
		{
			controllerType = Controller.Gamepad;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.A;
			buttonMap.Two                      = RawButton.B;
			buttonMap.Three                    = RawButton.X;
			buttonMap.Four                     = RawButton.Y;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.Back;
			buttonMap.PrimaryShoulder          = RawButton.LShoulder;
			buttonMap.PrimaryIndexTrigger      = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger       = RawButton.None;
			buttonMap.PrimaryThumbstick        = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp      = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown    = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft    = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight   = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad          = RawButton.None;
			buttonMap.SecondaryShoulder        = RawButton.RShoulder;
			buttonMap.SecondaryIndexTrigger    = RawButton.RIndexTrigger;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.RThumbstick;
			buttonMap.SecondaryThumbstickUp    = RawButton.RThumbstickUp;
			buttonMap.SecondaryThumbstickDown  = RawButton.RThumbstickDown;
			buttonMap.SecondaryThumbstickLeft  = RawButton.RThumbstickLeft;
			buttonMap.SecondaryThumbstickRight = RawButton.RThumbstickRight;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.DpadUp;
			buttonMap.DpadDown                 = RawButton.DpadDown;
			buttonMap.DpadLeft                 = RawButton.DpadLeft;
			buttonMap.DpadRight                = RawButton.DpadRight;
			buttonMap.Up                       = RawButton.LThumbstickUp;
			buttonMap.Down                     = RawButton.LThumbstickDown;
			buttonMap.Left                     = RawButton.LThumbstickLeft;
			buttonMap.Right                    = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.None;
			touchMap.Two                       = RawTouch.None;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.None;
			touchMap.PrimaryThumbstick         = RawTouch.None;
			touchMap.PrimaryThumbRest          = RawTouch.None;
			touchMap.PrimaryTouchpad           = RawTouch.None;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                      = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger       = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons       = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger     = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons     = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                      = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger       = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger        = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger     = RawAxis1D.RIndexTrigger;
			axis1DMap.SecondaryHandTrigger      = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                      = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick         = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad           = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick       = RawAxis2D.RThumbstick;
			axis2DMap.SecondaryTouchpad         = RawAxis2D.None;
		}
	}

	private class OVRControllerGamepadMac : OVRControllerBase
	{
		/// <summary> An axis on the gamepad. </summary>
		private enum AxisGPC
		{
			None = -1,
			LeftXAxis = 0,
			LeftYAxis,
			RightXAxis,
			RightYAxis,
			LeftTrigger,
			RightTrigger,
			DPad_X_Axis,
			DPad_Y_Axis,
			Max,
		};

		/// <summary> A button on the gamepad. </summary>
		public enum ButtonGPC
		{
			None = -1,
			A = 0,
			B,
			X,
			Y,
			Up,
			Down,
			Left,
			Right,
			Start,
			Back,
			LStick,
			RStick,
			LeftShoulder,
			RightShoulder,
			Max
		};

		private bool initialized = false;

		public OVRControllerGamepadMac()
		{
			controllerType = Controller.Gamepad;

			initialized = OVR_GamepadController_Initialize();
		}

		~OVRControllerGamepadMac()
		{
			if (!initialized)
				return;

			OVR_GamepadController_Destroy();
		}

		public override Controller Update()
		{
			if (!initialized)
			{
				return Controller.None;
			}

			OVRPlugin.ControllerState4 state = new OVRPlugin.ControllerState4();

			bool result = OVR_GamepadController_Update();

			if (result)
				state.ConnectedControllers = (uint)Controller.Gamepad;

			if (OVR_GamepadController_GetButton((int)ButtonGPC.A))
				state.Buttons |= (uint)RawButton.A;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.B))
				state.Buttons |= (uint)RawButton.B;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.X))
				state.Buttons |= (uint)RawButton.X;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.Y))
				state.Buttons |= (uint)RawButton.Y;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.Up))
				state.Buttons |= (uint)RawButton.DpadUp;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.Down))
				state.Buttons |= (uint)RawButton.DpadDown;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.Left))
				state.Buttons |= (uint)RawButton.DpadLeft;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.Right))
				state.Buttons |= (uint)RawButton.DpadRight;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.Start))
				state.Buttons |= (uint)RawButton.Start;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.Back))
				state.Buttons |= (uint)RawButton.Back;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.LStick))
				state.Buttons |= (uint)RawButton.LThumbstick;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.RStick))
				state.Buttons |= (uint)RawButton.RThumbstick;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.LeftShoulder))
				state.Buttons |= (uint)RawButton.LShoulder;
			if (OVR_GamepadController_GetButton((int)ButtonGPC.RightShoulder))
				state.Buttons |= (uint)RawButton.RShoulder;

			state.LThumbstick.x = OVR_GamepadController_GetAxis((int)AxisGPC.LeftXAxis);
			state.LThumbstick.y = OVR_GamepadController_GetAxis((int)AxisGPC.LeftYAxis);
			state.RThumbstick.x = OVR_GamepadController_GetAxis((int)AxisGPC.RightXAxis);
			state.RThumbstick.y = OVR_GamepadController_GetAxis((int)AxisGPC.RightYAxis);
			state.LIndexTrigger = OVR_GamepadController_GetAxis((int)AxisGPC.LeftTrigger);
			state.RIndexTrigger = OVR_GamepadController_GetAxis((int)AxisGPC.RightTrigger);

			if (state.LIndexTrigger >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LIndexTrigger;
			if (state.LHandTrigger >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LHandTrigger;
			if (state.LThumbstick.y >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LThumbstickUp;
			if (state.LThumbstick.y <= -AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LThumbstickDown;
			if (state.LThumbstick.x <= -AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LThumbstickLeft;
			if (state.LThumbstick.x >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.LThumbstickRight;

			if (state.RIndexTrigger >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RIndexTrigger;
			if (state.RHandTrigger >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RHandTrigger;
			if (state.RThumbstick.y >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RThumbstickUp;
			if (state.RThumbstick.y <= -AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RThumbstickDown;
			if (state.RThumbstick.x <= -AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RThumbstickLeft;
			if (state.RThumbstick.x >= AXIS_AS_BUTTON_THRESHOLD)
				state.Buttons |= (uint)RawButton.RThumbstickRight;

			previousState = currentState;
			currentState = state;

			return ((Controller)currentState.ConnectedControllers & controllerType);
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.A;
			buttonMap.Two                      = RawButton.B;
			buttonMap.Three                    = RawButton.X;
			buttonMap.Four                     = RawButton.Y;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.Back;
			buttonMap.PrimaryShoulder          = RawButton.LShoulder;
			buttonMap.PrimaryIndexTrigger      = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger       = RawButton.None;
			buttonMap.PrimaryThumbstick        = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp      = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown    = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft    = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight   = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad          = RawButton.None;
			buttonMap.SecondaryShoulder        = RawButton.RShoulder;
			buttonMap.SecondaryIndexTrigger    = RawButton.RIndexTrigger;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.RThumbstick;
			buttonMap.SecondaryThumbstickUp    = RawButton.RThumbstickUp;
			buttonMap.SecondaryThumbstickDown  = RawButton.RThumbstickDown;
			buttonMap.SecondaryThumbstickLeft  = RawButton.RThumbstickLeft;
			buttonMap.SecondaryThumbstickRight = RawButton.RThumbstickRight;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.DpadUp;
			buttonMap.DpadDown                 = RawButton.DpadDown;
			buttonMap.DpadLeft                 = RawButton.DpadLeft;
			buttonMap.DpadRight                = RawButton.DpadRight;
			buttonMap.Up                       = RawButton.LThumbstickUp;
			buttonMap.Down                     = RawButton.LThumbstickDown;
			buttonMap.Left                     = RawButton.LThumbstickLeft;
			buttonMap.Right                    = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.None;
			touchMap.Two                       = RawTouch.None;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.None;
			touchMap.PrimaryThumbstick         = RawTouch.None;
			touchMap.PrimaryThumbRest          = RawTouch.None;
			touchMap.PrimaryTouchpad           = RawTouch.None;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                      = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger       = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons       = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger     = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons     = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                      = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger       = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger        = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger     = RawAxis1D.RIndexTrigger;
			axis1DMap.SecondaryHandTrigger      = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                      = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick         = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad           = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick       = RawAxis2D.RThumbstick;
			axis2DMap.SecondaryTouchpad         = RawAxis2D.None;
		}

		public override void SetControllerVibration(float frequency, float amplitude)
		{
			int gpcNode = 0;
			float gpcFrequency = frequency * 200.0f; //Map frequency from 0-1 CAPI range to 0-200 GPC range
			float gpcStrength = amplitude;

			OVR_GamepadController_SetVibration(gpcNode, gpcStrength, gpcFrequency);
		}

		private const string DllName = "OVRGamepad";

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_Initialize();
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_Destroy();
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_Update();
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern float OVR_GamepadController_GetAxis(int axis);
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_GetButton(int button);
		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_SetVibration(int node, float strength, float frequency);
	}

	private class OVRControllerGamepadAndroid : OVRControllerBase
	{
		public OVRControllerGamepadAndroid()
		{
			controllerType = Controller.Gamepad;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.A;
			buttonMap.Two                      = RawButton.B;
			buttonMap.Three                    = RawButton.X;
			buttonMap.Four                     = RawButton.Y;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.Back;
			buttonMap.PrimaryShoulder          = RawButton.LShoulder;
			buttonMap.PrimaryIndexTrigger      = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger       = RawButton.None;
			buttonMap.PrimaryThumbstick        = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp      = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown    = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft    = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight   = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad          = RawButton.None;
			buttonMap.SecondaryShoulder        = RawButton.RShoulder;
			buttonMap.SecondaryIndexTrigger    = RawButton.RIndexTrigger;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.RThumbstick;
			buttonMap.SecondaryThumbstickUp    = RawButton.RThumbstickUp;
			buttonMap.SecondaryThumbstickDown  = RawButton.RThumbstickDown;
			buttonMap.SecondaryThumbstickLeft  = RawButton.RThumbstickLeft;
			buttonMap.SecondaryThumbstickRight = RawButton.RThumbstickRight;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.DpadUp;
			buttonMap.DpadDown                 = RawButton.DpadDown;
			buttonMap.DpadLeft                 = RawButton.DpadLeft;
			buttonMap.DpadRight                = RawButton.DpadRight;
			buttonMap.Up                       = RawButton.LThumbstickUp;
			buttonMap.Down                     = RawButton.LThumbstickDown;
			buttonMap.Left                     = RawButton.LThumbstickLeft;
			buttonMap.Right                    = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.None;
			touchMap.Two                       = RawTouch.None;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.None;
			touchMap.PrimaryThumbstick         = RawTouch.None;
			touchMap.PrimaryThumbRest          = RawTouch.None;
			touchMap.PrimaryTouchpad           = RawTouch.None;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                      = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger       = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons       = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger     = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons     = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                      = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger       = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger        = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger     = RawAxis1D.RIndexTrigger;
			axis1DMap.SecondaryHandTrigger      = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                      = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick         = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad           = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick       = RawAxis2D.RThumbstick;
			axis2DMap.SecondaryTouchpad         = RawAxis2D.None;
		}
	}

	private class OVRControllerTouchpad : OVRControllerBase
	{
        private OVRPlugin.Vector2f moveAmount;
		private float maxTapMagnitude = 0.1f;
		private float minMoveMagnitude = 0.15f;

		public OVRControllerTouchpad()
		{
			controllerType = Controller.Touchpad;
		}

		public override Controller Update()
        {
            Controller res = base.Update();

            if (GetDown(RawTouch.LTouchpad, OVRInput.Controller.Touchpad))
			{
                moveAmount = currentState.LTouchpad;
			}

            if (GetUp(RawTouch.LTouchpad, OVRInput.Controller.Touchpad))
			{
				moveAmount.x = previousState.LTouchpad.x - moveAmount.x;
				moveAmount.y = previousState.LTouchpad.y - moveAmount.y;

				Vector2 move = new Vector2(moveAmount.x, moveAmount.y);
                float moveMag = move.magnitude;

                if (moveMag < maxTapMagnitude)
                {
                    // Emit Touchpad Tap
                    currentState.Buttons |= (uint)RawButton.Start;
                    currentState.Buttons |= (uint)RawButton.LTouchpad;
                }
				else if (moveMag >= minMoveMagnitude)
				{
					move.Normalize();

					// Left/Right
					if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
					{
						if (move.x < 0.0f)
						{
							currentState.Buttons |= (uint)RawButton.DpadLeft;
						}
						else
						{
							currentState.Buttons |= (uint)RawButton.DpadRight;
						}
					}
					// Up/Down
					else
					{
						if (move.y < 0.0f)
						{
							currentState.Buttons |= (uint)RawButton.DpadDown;
						}
						else
						{
							currentState.Buttons |= (uint)RawButton.DpadUp;
						}
					}
				}
			}

            return res;
        }

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.LTouchpad;
			buttonMap.Two                      = RawButton.Back;
			buttonMap.Three                    = RawButton.None;
			buttonMap.Four                     = RawButton.None;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.Back;
			buttonMap.PrimaryShoulder          = RawButton.None;
			buttonMap.PrimaryIndexTrigger      = RawButton.None;
			buttonMap.PrimaryHandTrigger       = RawButton.None;
			buttonMap.PrimaryThumbstick        = RawButton.None;
			buttonMap.PrimaryThumbstickUp      = RawButton.None;
			buttonMap.PrimaryThumbstickDown    = RawButton.None;
			buttonMap.PrimaryThumbstickLeft    = RawButton.None;
			buttonMap.PrimaryThumbstickRight   = RawButton.None;
            buttonMap.PrimaryTouchpad          = RawButton.LTouchpad;
			buttonMap.SecondaryShoulder        = RawButton.None;
			buttonMap.SecondaryIndexTrigger    = RawButton.None;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.None;
			buttonMap.SecondaryThumbstickUp    = RawButton.None;
			buttonMap.SecondaryThumbstickDown  = RawButton.None;
			buttonMap.SecondaryThumbstickLeft  = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.DpadUp;
			buttonMap.DpadDown                 = RawButton.DpadDown;
			buttonMap.DpadLeft                 = RawButton.DpadLeft;
			buttonMap.DpadRight                = RawButton.DpadRight;
			buttonMap.Up                       = RawButton.DpadUp;
			buttonMap.Down                     = RawButton.DpadDown;
			buttonMap.Left                     = RawButton.DpadLeft;
			buttonMap.Right                    = RawButton.DpadRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.LTouchpad;
			touchMap.Two                       = RawTouch.None;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.None;
			touchMap.PrimaryThumbstick         = RawTouch.None;
			touchMap.PrimaryThumbRest          = RawTouch.None;
			touchMap.PrimaryTouchpad           = RawTouch.LTouchpad;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                  = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger   = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons   = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                     = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger      = RawAxis1D.None;
			axis1DMap.PrimaryHandTrigger       = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger    = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger     = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                     = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick        = RawAxis2D.None;
			axis2DMap.PrimaryTouchpad          = RawAxis2D.LTouchpad;
			axis2DMap.SecondaryThumbstick      = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad        = RawAxis2D.None;
		}
	}

	private class OVRControllerLTrackedRemote : OVRControllerBase
	{
        private bool emitSwipe;
        private OVRPlugin.Vector2f moveAmount;
		private float minMoveMagnitude = 0.3f;

		public OVRControllerLTrackedRemote()
		{
			controllerType = Controller.LTrackedRemote;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.LTouchpad;
			buttonMap.Two                      = RawButton.Back;
			buttonMap.Three                    = RawButton.None;
			buttonMap.Four                     = RawButton.None;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.Back;
			buttonMap.PrimaryShoulder          = RawButton.None;
			buttonMap.PrimaryIndexTrigger      = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger       = RawButton.LHandTrigger;
			buttonMap.PrimaryThumbstick        = RawButton.None;
			buttonMap.PrimaryThumbstickUp      = RawButton.None;
			buttonMap.PrimaryThumbstickDown    = RawButton.None;
			buttonMap.PrimaryThumbstickLeft    = RawButton.None;
			buttonMap.PrimaryThumbstickRight   = RawButton.None;
			buttonMap.PrimaryTouchpad          = RawButton.LTouchpad;
			buttonMap.SecondaryShoulder        = RawButton.None;
			buttonMap.SecondaryIndexTrigger    = RawButton.None;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.None;
			buttonMap.SecondaryThumbstickUp    = RawButton.None;
			buttonMap.SecondaryThumbstickDown  = RawButton.None;
			buttonMap.SecondaryThumbstickLeft  = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.DpadUp;
			buttonMap.DpadDown                 = RawButton.DpadDown;
			buttonMap.DpadLeft                 = RawButton.DpadLeft;
			buttonMap.DpadRight                = RawButton.DpadRight;
			buttonMap.Up                       = RawButton.DpadUp;
			buttonMap.Down                     = RawButton.DpadDown;
			buttonMap.Left                     = RawButton.DpadLeft;
			buttonMap.Right                    = RawButton.DpadRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.LTouchpad;
			touchMap.Two                       = RawTouch.None;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.LIndexTrigger;
			touchMap.PrimaryThumbstick         = RawTouch.None;
			touchMap.PrimaryThumbRest          = RawTouch.None;
			touchMap.PrimaryTouchpad           = RawTouch.LTouchpad;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                  = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger   = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons   = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                     = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger      = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger       = RawAxis1D.LHandTrigger;
			axis1DMap.SecondaryIndexTrigger    = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger     = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                     = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick        = RawAxis2D.None;
			axis2DMap.PrimaryTouchpad          = RawAxis2D.LTouchpad;
			axis2DMap.SecondaryThumbstick      = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad        = RawAxis2D.None;
		}

        public override Controller Update()
        {
            Controller res = base.Update();

            if (GetDown(RawTouch.LTouchpad, OVRInput.Controller.LTrackedRemote))
			{
                emitSwipe = true;
                moveAmount = currentState.LTouchpad;
			}

            if (GetDown(RawButton.LTouchpad, OVRInput.Controller.LTrackedRemote))
            {
                emitSwipe = false;
            }

            if (GetUp(RawTouch.LTouchpad, OVRInput.Controller.LTrackedRemote) && emitSwipe)
			{
                emitSwipe = false;

				moveAmount.x = previousState.LTouchpad.x - moveAmount.x;
				moveAmount.y = previousState.LTouchpad.y - moveAmount.y;

				Vector2 move = new Vector2(moveAmount.x, moveAmount.y);

				if (move.magnitude >= minMoveMagnitude)
				{
					move.Normalize();

					// Left/Right
					if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
					{
						if (move.x < 0.0f)
						{
							currentState.Buttons |= (uint)RawButton.DpadLeft;
						}
						else
						{
							currentState.Buttons |= (uint)RawButton.DpadRight;
						}
					}
					// Up/Down
					else
					{
						if (move.y < 0.0f)
						{
							currentState.Buttons |= (uint)RawButton.DpadDown;
						}
						else
						{
							currentState.Buttons |= (uint)RawButton.DpadUp;
						}
					}
				}
			}

            return res;
        }

		public override bool WasRecentered()
		{
			return (currentState.LRecenterCount != previousState.LRecenterCount);
		}

		public override byte GetRecenterCount()
		{
			return currentState.LRecenterCount;
		}

		public override byte GetBatteryPercentRemaining()
		{
			return currentState.LBatteryPercentRemaining;
		}
	}

	private class OVRControllerRTrackedRemote : OVRControllerBase
	{
        private bool emitSwipe;
        private OVRPlugin.Vector2f moveAmount;
		private float minMoveMagnitude = 0.3f;

		public OVRControllerRTrackedRemote()
		{
			controllerType = Controller.RTrackedRemote;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None                     = RawButton.None;
			buttonMap.One                      = RawButton.RTouchpad;
			buttonMap.Two                      = RawButton.Back;
			buttonMap.Three                    = RawButton.None;
			buttonMap.Four                     = RawButton.None;
			buttonMap.Start                    = RawButton.Start;
			buttonMap.Back                     = RawButton.Back;
			buttonMap.PrimaryShoulder          = RawButton.None;
			buttonMap.PrimaryIndexTrigger      = RawButton.RIndexTrigger;
			buttonMap.PrimaryHandTrigger       = RawButton.RHandTrigger;
			buttonMap.PrimaryThumbstick        = RawButton.None;
			buttonMap.PrimaryThumbstickUp      = RawButton.None;
			buttonMap.PrimaryThumbstickDown    = RawButton.None;
			buttonMap.PrimaryThumbstickLeft    = RawButton.None;
			buttonMap.PrimaryThumbstickRight   = RawButton.None;
			buttonMap.PrimaryTouchpad          = RawButton.RTouchpad;
			buttonMap.SecondaryShoulder        = RawButton.None;
			buttonMap.SecondaryIndexTrigger    = RawButton.None;
			buttonMap.SecondaryHandTrigger     = RawButton.None;
			buttonMap.SecondaryThumbstick      = RawButton.None;
			buttonMap.SecondaryThumbstickUp    = RawButton.None;
			buttonMap.SecondaryThumbstickDown  = RawButton.None;
			buttonMap.SecondaryThumbstickLeft  = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad        = RawButton.None;
			buttonMap.DpadUp                   = RawButton.DpadUp;
			buttonMap.DpadDown                 = RawButton.DpadDown;
			buttonMap.DpadLeft                 = RawButton.DpadLeft;
			buttonMap.DpadRight                = RawButton.DpadRight;
			buttonMap.Up                       = RawButton.DpadUp;
			buttonMap.Down                     = RawButton.DpadDown;
			buttonMap.Left                     = RawButton.DpadLeft;
			buttonMap.Right                    = RawButton.DpadRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None                      = RawTouch.None;
			touchMap.One                       = RawTouch.RTouchpad;
			touchMap.Two                       = RawTouch.None;
			touchMap.Three                     = RawTouch.None;
			touchMap.Four                      = RawTouch.None;
			touchMap.PrimaryIndexTrigger       = RawTouch.RIndexTrigger;
			touchMap.PrimaryThumbstick         = RawTouch.None;
			touchMap.PrimaryThumbRest          = RawTouch.None;
			touchMap.PrimaryTouchpad           = RawTouch.RTouchpad;
			touchMap.SecondaryIndexTrigger     = RawTouch.None;
			touchMap.SecondaryThumbstick       = RawTouch.None;
			touchMap.SecondaryThumbRest        = RawTouch.None;
			touchMap.SecondaryTouchpad         = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None                  = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger   = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons   = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None                     = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger      = RawAxis1D.RIndexTrigger;
			axis1DMap.PrimaryHandTrigger       = RawAxis1D.RHandTrigger;
			axis1DMap.SecondaryIndexTrigger    = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger     = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None                     = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick        = RawAxis2D.None;
			axis2DMap.PrimaryTouchpad          = RawAxis2D.RTouchpad;
			axis2DMap.SecondaryThumbstick      = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad        = RawAxis2D.None;
		}

        public override Controller Update()
        {
            Controller res = base.Update();

            if (GetDown(RawTouch.RTouchpad, OVRInput.Controller.RTrackedRemote))
			{
                emitSwipe = true;
                moveAmount = currentState.RTouchpad;
			}

            if (GetDown(RawButton.RTouchpad, OVRInput.Controller.RTrackedRemote))
			{
                emitSwipe = false;
			}

            if (GetUp(RawTouch.RTouchpad, OVRInput.Controller.RTrackedRemote) && emitSwipe)
			{
                emitSwipe = false;

				moveAmount.x = previousState.RTouchpad.x - moveAmount.x;
				moveAmount.y = previousState.RTouchpad.y - moveAmount.y;

				Vector2 move = new Vector2(moveAmount.x, moveAmount.y);

				if (move.magnitude >= minMoveMagnitude)
				{
					move.Normalize();

					// Left/Right
					if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
					{
						if (move.x < 0.0f)
						{
							currentState.Buttons |= (uint)RawButton.DpadLeft;
						}
						else
						{
							currentState.Buttons |= (uint)RawButton.DpadRight;
						}
					}
					// Up/Down
					else
					{
						if (move.y < 0.0f)
						{
							currentState.Buttons |= (uint)RawButton.DpadDown;
						}
						else
						{
							currentState.Buttons |= (uint)RawButton.DpadUp;
						}
					}
				}
			}

            return res;
        }

		public override bool WasRecentered()
		{
			return (currentState.RRecenterCount != previousState.RRecenterCount);
		}

		public override byte GetRecenterCount()
		{
			return currentState.RRecenterCount;
		}

		public override byte GetBatteryPercentRemaining()
		{
			return currentState.RBatteryPercentRemaining;
		}
    }
}
