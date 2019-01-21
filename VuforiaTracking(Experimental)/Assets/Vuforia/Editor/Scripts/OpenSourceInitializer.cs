/*===============================================================================
Copyright (c) 2017-2018 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using System.Linq;
using UnityEditor;
using UnityEngine;
using Vuforia;
using Vuforia.EditorClasses;

/// <summary>
/// Creates connection between open source files and the Vuforia library.
/// Do not modify.
/// </summary>
[InitializeOnLoad]
public static class OpenSourceInitializer
{
    static OpenSourceInitializer()
    {
        GameObjectFactory.SetDefaultBehaviourTypeConfiguration(new DefaultBehaviourAttacher());
        ReplacePlaceHolders();
    }

    static void ReplacePlaceHolders()
    {
        var trackablePlaceholders = Object.FindObjectsOfType<DefaultTrackableBehaviourPlaceholder>().ToList();
        var initErrorsPlaceholders = Object.FindObjectsOfType<DefaultInitializationErrorHandlerPlaceHolder>().ToList();
        var modelRecoEventPlaceholders = Object.FindObjectsOfType<DefaultModelRecoEventHandlerPlaceHolder>().ToList();
        
        trackablePlaceholders.ForEach(ReplaceTrackablePlaceHolder);
        initErrorsPlaceholders.ForEach(ReplaceInitErrorPlaceHolder);
        modelRecoEventPlaceholders.ForEach(ReplaceModelRecoEventPlaceHolder);
    }
    
    static void ReplaceTrackablePlaceHolder(DefaultTrackableBehaviourPlaceholder placeHolder)
    {
        var go = placeHolder.gameObject;
        go.AddComponent<DefaultTrackableEventHandler>();

        Object.DestroyImmediate(placeHolder);
    }

    static void ReplaceInitErrorPlaceHolder(DefaultInitializationErrorHandlerPlaceHolder placeHolder)
    {
        var go = placeHolder.gameObject;
        go.AddComponent<DefaultInitializationErrorHandler>();

        Object.DestroyImmediate(placeHolder);
    }

    static void ReplaceModelRecoEventPlaceHolder(DefaultModelRecoEventHandlerPlaceHolder placeHolder)
    {
        var go = placeHolder.gameObject;
        go.AddComponent<DefaultModelRecoEventHandler>();

        Object.DestroyImmediate(placeHolder);
    }

    class DefaultBehaviourAttacher : IDefaultBehaviourAttacher
    {
        public void AddDefaultTrackableBehaviour(GameObject go)
        {
            go.AddComponent<DefaultTrackableEventHandler>();
        }

        public void AddDefaultInitializationErrorHandler(GameObject go)
        {
            go.AddComponent<DefaultInitializationErrorHandler>();
        }

        public void AddDefaultModelRecoEventHandler(GameObject modelReco, ModelTargetBehaviour modelTargetTemplate)
        {
            var mreh = modelReco.AddComponent<DefaultModelRecoEventHandler>();
            mreh.ShowBoundingBox = true;
            mreh.ModelTargetTemplate = modelTargetTemplate;
        }
    }
}
