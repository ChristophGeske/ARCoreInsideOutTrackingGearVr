/************************************************************************************

Copyright   :   Copyright 2014-Present Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Collections;

/// <summary>
/// Mouse-driven UI pointer.
/// </summary>
[RequireComponent(typeof(OVRRaycaster))]
public class OVRMousePointer : MonoBehaviour {

    [Tooltip("Period of inactivity before mouse disappears")]
    public float inactivityTimeout = 1;

    public enum MouseShowPolicy
    {
        always,
        withGaze,
        withActivity
    };
    
    [Tooltip("Policy regarding when mouse pointer should be shown")]
    public MouseShowPolicy mouseShowPolicy;

    [Tooltip("Should the mouse pointer being active cause the gaze pointer to fade")]
    public bool hideGazePointerWhenActive = false;

    [Tooltip("Move the pointer in response to mouse movement")]
    public bool defaultMouseMovement = true;

    public float mouseMoveSpeed = 5;
    
    private float lastMouseActivityTime;


    public GameObject pointerObject
    {
        get
        {
            return GetComponent<OVRRaycaster>().pointer;
        }
    }
    
    private OVRRaycaster raycaster;

    void Awake()
    {        
        raycaster = GetComponent<OVRRaycaster>();       
#if UNITY_ANDROID
        pointerObject.SetActive(false);
        this.enabled = false;
#endif
    }

	// Update is called once per frame
	void Update () 
    {
        // Set mouse visibility based on the chosen policy
        if (mouseShowPolicy == MouseShowPolicy.withActivity)
        {
            // Active if mouse has moved recently and this raycaster (and associated canvas) is active
            pointerObject.SetActive(HasMovedRecently() && raycaster.IsFocussed());
        }
        else if (mouseShowPolicy == MouseShowPolicy.withGaze)
        {
            // Active if this raycaster (and associated canvas) is active
            pointerObject.SetActive(raycaster.IsFocussed());
        }

        if (hideGazePointerWhenActive && HasMovedRecently() && raycaster.IsFocussed())
        {
            // With this policy we hide the gaze pointer while the mouse is active. It can be confusing to
            // have a mouse pointer and a gaze pointer active at the same time
            OVRGazePointer.instance.RequestHide();
        }

        if (defaultMouseMovement && raycaster.IsFocussed())
        {
            // If we're responsible for moving the pointer and we're the currently active
            // raycaster then update the mouse position, keeping it clamped to inside the canvas.
            Vector2 mousePos = pointerObject.GetComponent<RectTransform>().localPosition;
            mousePos += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseMoveSpeed;
            float currentPanelWidth = GetComponent<RectTransform>().rect.width;
            float currentPanelHeight = GetComponent<RectTransform>().rect.height;
            mousePos.x = Mathf.Clamp(mousePos.x, -currentPanelWidth / 2, currentPanelWidth / 2);
            mousePos.y = Mathf.Clamp(mousePos.y, -currentPanelHeight / 2, currentPanelHeight / 2);

            // Position mouse pointer
            SetLocalPosition(mousePos);
        }
	
	}

    public bool HasMovedRecently()
    {
        return lastMouseActivityTime + inactivityTimeout > Time.time;
    }
    
    /// <summary>
    /// Set position of pointer. Only makes sense to use if defaultMouseMovement is false
    /// </summary>
    public void SetLocalPosition(Vector3 pos)
    {
        if ((pointerObject.GetComponent<RectTransform>().localPosition - pos).magnitude> 0.001f)
        {
            lastMouseActivityTime = Time.time;
        }
        pointerObject.GetComponent<RectTransform>().localPosition = pos;
    }

  
}
