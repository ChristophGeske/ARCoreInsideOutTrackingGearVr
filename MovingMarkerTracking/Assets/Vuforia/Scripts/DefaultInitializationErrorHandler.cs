/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using UnityEngine;
using Vuforia;

/// <summary>
/// A custom handler that registers for Vuforia initialization errors
/// 
/// Changes made to this file could be overwritten when upgrading the Vuforia version. 
/// When implementing custom error handler behavior, consider inheriting from this class instead.
/// </summary>
public class DefaultInitializationErrorHandler : VuforiaMonoBehaviour
{
    #region Vuforia_lifecycle_events

    public void OnVuforiaInitializationError(VuforiaUnity.InitError initError)
    {
        if (initError != VuforiaUnity.InitError.INIT_SUCCESS)
        {
            SetErrorCode(initError);
            SetErrorOccurred(true);
        }
    }

    #endregion // Vuforia_lifecycle_events

    #region PRIVATE_MEMBER_VARIABLES

    string mErrorText = "";
    bool mErrorOccurred;

    const string headerLabel = "Vuforia Initialization Error";

    GUIStyle bodyStyle;
    GUIStyle headerStyle;
    GUIStyle footerStyle;

    Texture2D bodyTexture;
    Texture2D headerTexture;
    Texture2D footerTexture;

    #endregion // PRIVATE_MEMBER_VARIABLES

    #region UNTIY_MONOBEHAVIOUR_METHODS

    void Awake()
    {
        // Check for an initialization error on start.
        VuforiaRuntime.Instance.RegisterVuforiaInitErrorCallback(OnVuforiaInitializationError);
    }

    void Start()
    {
        SetupGUIStyles();
    }

    void OnGUI()
    {
        // On error, create a full screen window.
        if (mErrorOccurred)
            GUI.Window(0, new Rect(0, 0, Screen.width, Screen.height), DrawWindowContent, "");
    }

    /// <summary>
    ///     When this game object is destroyed, it unregisters itself as event handler
    /// </summary>
    void OnDestroy()
    {
        VuforiaRuntime.Instance.UnregisterVuforiaInitErrorCallback(OnVuforiaInitializationError);
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS

    #region PRIVATE_METHODS

    void DrawWindowContent(int id)
    {
        var headerRect = new Rect(0, 0, Screen.width, Screen.height / 8);
        var bodyRect = new Rect(0, Screen.height / 8, Screen.width, Screen.height / 8 * 6);
        var footerRect = new Rect(0, Screen.height - Screen.height / 8, Screen.width, Screen.height / 8);

        GUI.Label(headerRect, headerLabel, headerStyle);
        GUI.Label(bodyRect, mErrorText, bodyStyle);

        if (GUI.Button(footerRect, "Close", footerStyle))
        {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
#endif
        }
    }

    void SetErrorCode(VuforiaUnity.InitError errorCode)
    {
        switch (errorCode)
        {
            case VuforiaUnity.InitError.INIT_EXTERNAL_DEVICE_NOT_DETECTED:
                mErrorText =
                    "Failed to initialize Vuforia because this " +
                    "device is not docked with required external hardware.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_MISSING_KEY:
                mErrorText =
                    "Vuforia App key is missing. Please get a valid key " +
                    "by logging into your account at developer.vuforia.com " +
                    "and creating a new project.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_INVALID_KEY:
                mErrorText =
                    "Vuforia App key is invalid. " +
                    "Please get a valid key by logging into your account at " +
                    "developer.vuforia.com and creating a new project. \n\n" +
                    getKeyInfo();
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_NO_NETWORK_TRANSIENT:
                mErrorText = "Unable to contact server. Please try again later.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_NO_NETWORK_PERMANENT:
                mErrorText = "No network available. Please make sure you are connected to the Internet.";
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_CANCELED_KEY:
                mErrorText =
                    "This App license key has been cancelled and may no longer be used. " +
                    "Please get a new license key. \n\n" +
                    getKeyInfo();
                break;
            case VuforiaUnity.InitError.INIT_LICENSE_ERROR_PRODUCT_TYPE_MISMATCH:
                mErrorText =
                    "Vuforia App key is not valid for this product. Please get a valid key " +
                    "by logging into your account at developer.vuforia.com and choosing the " +
                    "right product type during project creation. \n\n" +
                    getKeyInfo() + " \n\n" +
                    "Note that Universal Windows Platform (UWP) apps require " +
                    "a license key created on or after August 9th, 2016.";
                break;
            case VuforiaUnity.InitError.INIT_NO_CAMERA_ACCESS:
                mErrorText = 
                    "User denied Camera access to this app.\n" +
                    "To restore, enable Camera access in Settings:\n" +
                    "Settings > Privacy > Camera > " + Application.productName + "\n" +
                    "Also verify that the Camera is enabled in:\n" +
                    "Settings > General > Restrictions.";
                break;
            case VuforiaUnity.InitError.INIT_DEVICE_NOT_SUPPORTED:
                mErrorText = "Failed to initialize Vuforia because this device is not supported.";
                break;
            case VuforiaUnity.InitError.INIT_ERROR:
                mErrorText = "Failed to initialize Vuforia.";
                break;
        }

        // Prepend the error code in red
        mErrorText = "<color=red>" + errorCode.ToString().Replace("_", " ") + "</color>\n\n" + mErrorText;

        // Remove rich text tags for console logging
        var errorTextConsole = mErrorText.Replace("<color=red>", "").Replace("</color>", "");

        Debug.LogError("Vuforia initialization failed: " + errorCode + "\n\n" + errorTextConsole);
    }

    void SetErrorOccurred(bool errorOccurred)
    {
        mErrorOccurred = errorOccurred;
    }

    string getKeyInfo()
    {
        string key = VuforiaConfiguration.Instance.Vuforia.LicenseKey;
        string keyInfo;
        if (key.Length > 10)
            keyInfo =
                "Your current key is <color=red>" + key.Length + "</color> characters in length. " +
                "It begins with <color=red>" + key.Substring(0, 5) + "</color> " +
                "and ends with <color=red>" + key.Substring(key.Length - 5, 5) + "</color>.";
        else
            keyInfo =
                "Your current key is <color=red>" + key.Length + "</color> characters in length. \n" +
                "The key is: <color=red>" + key + "</color>.";
        return keyInfo;
    }

    void SetupGUIStyles()
    {
        // Called from Start() to determine physical size of device for text sizing
        var shortSidePixels = Screen.width < Screen.height ? Screen.width : Screen.height;
        var shortSideInches = shortSidePixels / Screen.dpi;
        var physicalSizeMultiplier = shortSideInches > 4.0f ? 2 : 1;

        // Create 1x1 pixel background textures for body, header, and footer
        bodyTexture = CreateSinglePixelTexture(Color.white);
        headerTexture = CreateSinglePixelTexture(new Color(
            Mathf.InverseLerp(0, 255, 220),
            Mathf.InverseLerp(0, 255, 220),
            Mathf.InverseLerp(0, 255, 220))); // RGB(220)
        footerTexture = CreateSinglePixelTexture(new Color(
            Mathf.InverseLerp(0, 255, 35),
            Mathf.InverseLerp(0, 255, 178),
            Mathf.InverseLerp(0, 255, 0))); // RGB(35,178,0)

        // Create body style and set values
        bodyStyle = new GUIStyle();
        bodyStyle.normal.background = bodyTexture;
        bodyStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        bodyStyle.fontSize = (int) (18 * physicalSizeMultiplier * Screen.dpi / 160);
        bodyStyle.normal.textColor = Color.black;
        bodyStyle.wordWrap = true;
        bodyStyle.alignment = TextAnchor.MiddleCenter;
        bodyStyle.padding = new RectOffset(40, 40, 0, 0);

        // Duplicate body style and change necessary values
        headerStyle = new GUIStyle(bodyStyle);
        headerStyle.normal.background = headerTexture;
        headerStyle.fontSize = (int) (24 * physicalSizeMultiplier * Screen.dpi / 160);

        // Duplicate body style and change necessary values
        footerStyle = new GUIStyle(bodyStyle);
        footerStyle.normal.background = footerTexture;
        footerStyle.normal.textColor = Color.white;
        footerStyle.fontSize = (int) (28 * physicalSizeMultiplier * Screen.dpi / 160);
    }

    Texture2D CreateSinglePixelTexture(Color color)
    {
        // Called by SetupGUIStyles() to create 1x1 texture
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    #endregion // PRIVATE_METHODS
}
