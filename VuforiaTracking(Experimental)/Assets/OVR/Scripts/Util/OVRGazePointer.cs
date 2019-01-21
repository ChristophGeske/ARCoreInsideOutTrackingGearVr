﻿/************************************************************************************

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
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI pointer driven by gaze input.
/// </summary>
public class OVRGazePointer : MonoBehaviour {
    private Transform trailFollower; //the transform that rotates according to our movement

    [Tooltip("Should the pointer be hidden when not over interactive objects.")]
    public bool hideByDefault = true;

    [Tooltip("Time after leaving interactive object before pointer fades.")]
    public float showTimeoutPeriod = 1;

    [Tooltip("Time after mouse pointer becoming inactive before pointer unfades.")]
    public float hideTimeoutPeriod = 0.1f;

    [Tooltip("Keep a faint version of the pointer visible while using a mouse")]
    public bool dimOnHideRequest = true;

    [Tooltip("Angular scale of pointer")]
    public float depthScaleMultiplier = 0.03f;

    /// <summary>
    /// The gaze ray.
    /// </summary>
    public Transform rayTransform;

    /// <summary>
    /// Is gaze pointer current visible
    /// </summary>
    public bool hidden { get; private set; }

    /// <summary>
    /// Current scale applied to pointer
    /// </summary>
    public float currentScale { get; private set; }

    /// <summary>
    /// Current depth of pointer from camera
    /// </summary>
    private float depth;
    private float hideUntilTime;
    /// <summary>
    /// How many times position has been set this frame. Used to detect when there are no position sets in a frame.
    /// </summary>
    private int positionSetsThisFrame = 0;
    /// <summary>
    /// Position last frame.
    /// </summary>
    private Vector3 lastPosition;
    /// <summary>
    /// Last time code requested the pointer be shown. Usually when pointer passes over interactive elements.
    /// </summary>
    private float lastShowRequestTime;
    /// <summary>
    /// Last time pointer was requested to be hidden. Usually mouse pointer activity.
    /// </summary>
    private float lastHideRequestTime;

    [Tooltip("Radius of the cursor. Used for preventing geometry intersections.")]
    public float cursorRadius = 1f;

    

    // How much the gaze pointer moved in the last frame
    public Vector3 positionDelta { private set; get; }

    // Optionally present GUI element displaying progress when using gaze-to-select mechanics
    private OVRProgressIndicator progressIndicator;

    private static OVRGazePointer _instance;
    public static OVRGazePointer instance 
    { 
        // If there's no GazePointer already in the scene, instanciate one now.
        get
        {
            if (_instance == null)
            {
                Debug.Log(string.Format("Instanciating GazePointer", 0));
                _instance = (OVRGazePointer)GameObject.Instantiate((OVRGazePointer)Resources.Load("Prefabs/GazePointerRing", typeof(OVRGazePointer)));
            }
            return _instance;
        }
            
    }


    /// <summary>
    /// Used to determine alpha level of gaze cursor. Could also be used to determine cursor size, for example, as the cursor fades out.
    /// </summary>
    public float visibilityStrength 
    { 
        get 
        {
            // It's possible there are reasons to show the cursor - such as it hovering over some UI - and reasons to hide 
            // the cursor - such as another input method (e.g. mouse) being used. We take both of these in to account.
            

            float strengthFromShowRequest;
            if (hideByDefault)
            {
                // fade the cursor out with time
                strengthFromShowRequest =  Mathf.Clamp01(1 - (Time.time - lastShowRequestTime) / showTimeoutPeriod);
            }
            else
            {
                // keep it fully visible
                strengthFromShowRequest = 1;
            }

            // Now consider factors requesting pointer to be hidden
            float strengthFromHideRequest;
            
            strengthFromHideRequest = (lastHideRequestTime + hideTimeoutPeriod > Time.time) ? (dimOnHideRequest ? 0.1f : 0) : 1;
            

            // Hide requests take priority
            return Mathf.Min(strengthFromShowRequest, strengthFromHideRequest);
        } 
    }

    public float SelectionProgress
    {
        get
        {
            return progressIndicator ? progressIndicator.currentProgress : 0;
        }
        set
        {
            if (progressIndicator)
                progressIndicator.currentProgress = value;
        }
    }

    public void Awake()
    {
        currentScale = 1;
        // Only allow one instance at runtime.
        if (_instance != null && _instance != this)
        {
            enabled = false;
            DestroyImmediate(this);
            return;
        }

        _instance = this;

        trailFollower = transform.Find("TrailFollower");
        progressIndicator = transform.GetComponent<OVRProgressIndicator>();
    }
    
    void Update () 
    {
		if (rayTransform == null && Camera.main != null)
			rayTransform = Camera.main.transform;
		
        // Move the gaze cursor to keep it in the middle of the view
        transform.position = rayTransform.position + rayTransform.forward * depth;

        // Should we show or hide the gaze cursor?
        if (visibilityStrength == 0 && !hidden)
        {
            Hide();
        }
        else if (visibilityStrength > 0 && hidden)
        {
            Show();
        }
    }

    /// <summary>
    /// Set position and orientation of pointer
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="normal"></param>
    public void SetPosition(Vector3 pos, Vector3 normal)
    {
        transform.position = pos;
        
        // Set the rotation to match the normal of the surface it's on. For the other degree of freedom (rotation around its own normal) use
        // the direction of movement so that trail effects etc are easier
        Quaternion newRot = transform.rotation;
        newRot.SetLookRotation(normal, rayTransform.up);
        transform.rotation = newRot;

        // record depth so that distance doesn't pop when pointer leaves an object
        depth = (rayTransform.position - pos).magnitude;

        //set scale based on depth
        currentScale = depth * depthScaleMultiplier;
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        positionSetsThisFrame++;
    }

    /// <summary>
    /// SetPosition overload without normal. Just makes cursor face user
    /// </summary>
    /// <param name="pos"></param>
    public void SetPosition(Vector3 pos)
    {
        SetPosition(pos, rayTransform.forward);
    }

    public float GetCurrentRadius()
    {
        return cursorRadius * currentScale;
    }

    void LateUpdate()
    {
        // This happens after all Updates so we know that if positionSetsThisFrame is zero then nothing set the position this frame
        if (positionSetsThisFrame == 0)
        {
            // No geometry intersections, so gazing into space. Make the cursor face directly at the camera
            Quaternion newRot = transform.rotation;
            newRot.SetLookRotation(rayTransform.forward, rayTransform.up);
            transform.rotation = newRot;
        }

        // rotate the trail-follower to movement direction so we get a nicer particle effect
        Quaternion trailRotation = trailFollower.rotation;
        //we're setting the global rotation of the child (positions are in global coordinates), so premultiply the look vector with the parent transform
        trailRotation.SetLookRotation(transform.rotation * new Vector3(0, 0, 1), (lastPosition - transform.position).normalized);
        trailFollower.rotation = trailRotation;

        // Keep track of cursor movement direction
        positionDelta = transform.position - lastPosition;
        lastPosition = transform.position;
        
        positionSetsThisFrame = 0;
    }

    /// <summary>
    /// Request the pointer be hidden
    /// </summary>
    public void RequestHide()
    {
        if (!dimOnHideRequest)
        {
            Hide();
        }
        lastHideRequestTime = Time.time;
    }

    /// <summary>
    /// Request the pointer be shown. Hide requests take priority
    /// </summary>
    public void RequestShow()
    {
        Show();
        lastShowRequestTime = Time.time;
    }


    // Disable/Enable child elements when we show/hide the cursor. For performance reasons.
    void Hide()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        if (GetComponent<Renderer>())
            GetComponent<Renderer>().enabled = false;
        hidden = true;
    }

    void Show()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        if (GetComponent<Renderer>())
            GetComponent<Renderer>().enabled = true;
        hidden = false;
    }

}
