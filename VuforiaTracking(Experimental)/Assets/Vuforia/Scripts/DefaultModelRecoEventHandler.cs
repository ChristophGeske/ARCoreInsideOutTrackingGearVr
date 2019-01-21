/*==============================================================================
Copyright (c) 2017-2018 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
==============================================================================*/

using System.Linq;
using UnityEngine;
using Vuforia;

/// <summary>
/// A default implementation of Model Reco Event Handler.
/// It registers itself at the ModelRecoBehaviour and is notified of new search results.
/// </summary>
public class DefaultModelRecoEventHandler : MonoBehaviour, IObjectRecoEventHandler
{
    #region PRIVATE_MEMBER_VARIABLES

    private ModelTargetBehaviour mLastRecoModelTarget;
    private bool mSearching;
    private float mLastStatusCheckTime;

    #endregion // PRIVATE_MEMBER_VARIABLES


    #region PROTECTED_MEMBER_VARIABLES

    // ModelRecoBehaviour reference to avoid lookups
    protected ModelRecoBehaviour mModelRecoBehaviour;
    
    // Target Finder reference to avoid lookups
    protected TargetFinder mTargetFinder;

    #endregion  // PROTECTED_MEMBER_VARIABLES


    #region PUBLIC_VARIABLES

    /// <summary>
    /// The Model Target used as template when a Model is recognized.
    /// </summary>
    [Tooltip("The Model Target used as Template when a model is recognized.")]
    public ModelTargetBehaviour ModelTargetTemplate;

    /// <summary>
    /// Whether the model should be augmented with a bounding box.
    /// Only applicable to Template model targets.
    /// </summary>
    [Tooltip("Whether the model should be augmented with a bounding box.")]
    public bool ShowBoundingBox;

    /// <summary>
    /// Can be set in the Unity inspector to display error messages in UI.
    /// </summary>
    [Tooltip("UI Text label to display model reco errors.")]
    public UnityEngine.UI.Text ModelRecoErrorText;

    /// <summary>
    /// Can be set in the Unity inspector to tell Vuforia whether it should:
    /// - stop searching for new models, once a first model was found,
    ///   or:
    /// - continue searching for new models, even after a first model was found.
    /// </summary>
    [Tooltip("Whether Vuforia should stop searching for other models, after the first model was found.")]
    public bool StopSearchWhenModelFound = false;

    /// <summary>
    /// Can be set in the Unity inspector to tell Vuforia whether it should:
    /// - stop searching for new models, while a target is being tracked and is in view,
    ///   or:
    /// - continue searching for new models, even if a target is currently being tracked.
    /// </summary>
    [Tooltip("Whether Vuforia should stop searching for other models, while current model is tracked and visible.")]
    public bool StopSearchWhileTracking = true;//true by default, as this is the recommended behaviour

    #endregion // PUBLIC_VARIABLES



    #region UNITY_MONOBEHAVIOUR_METHODS

    /// <summary>
    /// register for events at the ModelRecoBehaviour
    /// </summary>
    void Start()
    {
        // register this event handler at the model reco behaviour
        var modelRecoBehaviour = GetComponent<ModelRecoBehaviour>();
        if (modelRecoBehaviour)
        {
            modelRecoBehaviour.RegisterEventHandler(this);
        }

        // remember modelRecoBehaviour for later
        mModelRecoBehaviour = modelRecoBehaviour;
    }

    void Update()
    {
        if (!VuforiaARController.Instance.HasStarted)
            return;

        if (mTargetFinder == null)
            return;

        
        // Check periodically if model target is tracked and in view
        // The test is not necessary when the search is stopped after first model was found
        float elapsed = Time.realtimeSinceStartup - mLastStatusCheckTime;
        if (!StopSearchWhenModelFound && StopSearchWhileTracking && elapsed > 0.5f)
        {
            mLastStatusCheckTime = Time.realtimeSinceStartup;

            if (mSearching)
            {
                if (IsModelTrackedInView(mLastRecoModelTarget))
                {
                    // Switch Model Reco OFF when model is being tracked/in-view
                    mModelRecoBehaviour.ModelRecoEnabled = false;
                    mSearching = false;
                }
            }
            else
            {
                if (!IsModelTrackedInView(mLastRecoModelTarget))
                {
                    // Switch Mode Reco ON when no model is tracked/in-view
                    mModelRecoBehaviour.ModelRecoEnabled = true;
                    mSearching = true;
                }
            }
        }
    }
    

    private void OnDestroy()
    {
        if (mModelRecoBehaviour != null)
        {
            mModelRecoBehaviour.UnregisterEventHandler(this);
        }

        mModelRecoBehaviour = null;
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region IModelRecoEventHandler_IMPLEMENTATION

    /// <summary>
    /// called when TargetFinder has been initialized successfully
    /// </summary>
    public void OnInitialized(TargetFinder targetFinder)
    {
        Debug.Log("ModelReco initialized.");

        // Keep a reference to the Target Finder
        mTargetFinder = targetFinder;
    }

    /// <summary>
    /// visualize initialization errors
    /// </summary>
    public void OnInitError(TargetFinder.InitState initError)
    {
        // Reset target finder reference
        mTargetFinder = null;

        Debug.LogError("Model Reco init error: " + initError.ToString());
        ShowErrorMessageInUI(initError.ToString());
    }

    /// <summary>
    /// visualize update errors
    /// </summary>
    public void OnUpdateError(TargetFinder.UpdateState updateError)
    {
        Debug.LogError("Model Reco update error: " + updateError.ToString());
        ShowErrorMessageInUI(updateError.ToString());
    }

    /// <summary>
    /// when we start scanning, clear all trackables
    /// </summary>
    public void OnStateChanged(bool searching)
    {
        Debug.Log("ModelReco: state changed: " + (searching ? "searching" : "not searching"));

        mSearching = searching;

        if (searching)
        {
            // clear all known trackables
            if (mTargetFinder != null)
                mTargetFinder.ClearTrackables(false);
        }
    }

    /// <summary>
    /// Handles new search results.
    /// </summary>
    /// <param name="searchResult"></param>
    public virtual void OnNewSearchResult(TargetFinder.TargetSearchResult searchResult)
    {
        Debug.Log("ModelReco: new search result available: " + searchResult.TargetName);

        // Find or create the referenced model target
        GameObject modelTargetGameObj = null;
        bool builtFromTemplate = false;
        var existingModelTarget = FindExistingModelTarget((TargetFinder.ModelRecoSearchResult)searchResult);
        if (existingModelTarget)
        {
            modelTargetGameObj = existingModelTarget.gameObject;
            builtFromTemplate = false;
        }
        else if (ModelTargetTemplate)
        {
            modelTargetGameObj = Instantiate(ModelTargetTemplate.gameObject);
            builtFromTemplate = true;
        }

        if (!modelTargetGameObj)
        {
            Debug.LogError("Could not create a Model Target.");
            return;
        }

        // Enable the new search result as a Model Target
        ModelTargetBehaviour mtb = mTargetFinder.EnableTracking(
            searchResult, modelTargetGameObj) as ModelTargetBehaviour;

        if (mtb)
        {
            mLastRecoModelTarget = mtb;

            // If the model target was created from a template,
            // we augment it with a bounding box game object
            if (builtFromTemplate && ShowBoundingBox)
            {
                var modelBoundingBox = mtb.ModelTarget.GetBoundingBox();
                var bboxGameObj = CreateBoundingBox(mtb.ModelTarget.Name, modelBoundingBox);

                // Parent the bounding box under the model target.
                bboxGameObj.transform.SetParent(modelTargetGameObj.transform, false);
            }

            if (StopSearchWhenModelFound)
            {
                // Stop the target finder
                mModelRecoBehaviour.ModelRecoEnabled = false;
            }
        }
    }

    #endregion // IModelRecoEventHandler_IMPLEMENTATION



    #region PRIVATE_METHODS

    private ModelTargetBehaviour FindExistingModelTarget(TargetFinder.ModelRecoSearchResult searchResult)
    {
        var modelTargetsInScene = Resources.FindObjectsOfTypeAll<ModelTargetBehaviour>().ToList().Where(mt => mt.ModelTargetType == ModelTargetType.PREDEFINED).ToArray();

        if (modelTargetsInScene == null || modelTargetsInScene.Length == 0)
            return null;

        string targetName = searchResult.TargetName;
        //string targetUniqueId = searchResult.UniqueTargetId;

        foreach (var mt in modelTargetsInScene)
        {
            if (mt.TrackableName == targetName)
            {
                mt.gameObject.SetActive(true);
                return mt;
            }
        }

        return null;
    }


    private GameObject CreateBoundingBox(string modelTargetName, OrientedBoundingBox3D bbox)
    {
        var bboxGameObj = new GameObject(modelTargetName + "_BoundingBox");
        bboxGameObj.transform.localPosition = bbox.Center;
        bboxGameObj.transform.localRotation = Quaternion.identity;
        bboxGameObj.transform.localScale = 2 * bbox.HalfExtents;
        bboxGameObj.AddComponent<BoundingBoxRenderer>();
        return bboxGameObj;
    }

    private void ShowErrorMessageInUI(string text)
    {
        if (ModelRecoErrorText)
            ModelRecoErrorText.text = text;
    }

    public static Bounds GetModelTargetWorldBounds(ModelTargetBehaviour mtb)
    {
        var bbox = mtb.ModelTarget.GetBoundingBox();
        var localCenter = bbox.Center;
        var localExtents = bbox.HalfExtents;

        // transform local center to World space
        var worldCenter = mtb.transform.TransformPoint(localCenter);

        // transform the local extents to World space
        var axisX = mtb.transform.TransformVector(localExtents.x, 0, 0);
        var axisY = mtb.transform.TransformVector(0, localExtents.y, 0);
        var axisZ = mtb.transform.TransformVector(0, 0, localExtents.z);
        
        Vector3 worldExtents = Vector3.zero;
        worldExtents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        worldExtents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        worldExtents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = worldCenter, extents = worldExtents };
    }

    private bool IsModelTrackedInView(ModelTargetBehaviour modelTarget)
    {
        if (!modelTarget)
            return false;

        if (modelTarget.CurrentStatus == TrackableBehaviour.Status.NO_POSE)
            return false;

        var cam = DigitalEyewearARController.Instance.PrimaryCamera;
        if (!cam)
            return false;

        // Compute the center of the model in World coordinates
        Bounds modelBounds = GetModelTargetWorldBounds(modelTarget);
        
        var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, modelBounds);
    }

    #endregion PRIVATE_METHODS


    #region PUBLIC_METHODS

    public TargetFinder GetTargetFinder()
    {
        return mTargetFinder;
    }


    public void ResetModelReco(bool destroyGameObjects)
    {
        var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

        if (objectTracker != null)
        {
            objectTracker.Stop();

            if (mTargetFinder != null)
            {
                mTargetFinder.ClearTrackables(destroyGameObjects);
                mTargetFinder.Stop();
                mTargetFinder.StartRecognition();
            }
            else
            {
                Debug.LogError("Could not reset TargetFinder");
            }

            objectTracker.Start();
        }
        else
        {
            Debug.LogError("Could not reset ObjectTracker");
        }
    }

    #endregion  // PUBLIC_METHODS
}
