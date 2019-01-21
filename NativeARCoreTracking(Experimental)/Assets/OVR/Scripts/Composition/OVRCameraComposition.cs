﻿using UnityEngine;
using System.Collections;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

public abstract class OVRCameraComposition : OVRComposition {
	protected GameObject cameraFramePlaneObject;
	protected float cameraFramePlaneDistance;

	protected readonly bool hasCameraDeviceOpened = false;
	protected readonly bool useDynamicLighting = false;

	internal readonly OVRPlugin.CameraDevice cameraDevice = OVRPlugin.CameraDevice.WebCamera0;

	private OVRCameraRig cameraRig;

	private Mesh boundaryMesh = null;
	private float boundaryMeshTopY = 0.0f;
	private float boundaryMeshBottomY = 0.0f;
	private OVRManager.VirtualGreenScreenType boundaryMeshType = OVRManager.VirtualGreenScreenType.Off;

	protected OVRCameraComposition(OVRManager.CameraDevice inCameraDevice, bool inUseDynamicLighting, OVRManager.DepthQuality depthQuality)
	{
		cameraDevice = OVRCompositionUtil.ConvertCameraDevice(inCameraDevice);

		Debug.Assert(!hasCameraDeviceOpened);
		Debug.Assert(!OVRPlugin.IsCameraDeviceAvailable(cameraDevice) || !OVRPlugin.HasCameraDeviceOpened(cameraDevice));
		hasCameraDeviceOpened = false;
		useDynamicLighting = inUseDynamicLighting;

		bool cameraSupportsDepth = OVRPlugin.DoesCameraDeviceSupportDepth(cameraDevice);
		if (useDynamicLighting && !cameraSupportsDepth)
		{
			Debug.LogWarning("The camera device doesn't support depth. The result of dynamic lighting might not be correct");
		}

		if (OVRPlugin.IsCameraDeviceAvailable(cameraDevice))
		{
			OVRPlugin.CameraExtrinsics extrinsics;
			OVRPlugin.CameraIntrinsics intrinsics;
			if (OVRPlugin.GetExternalCameraCount() > 0 && OVRPlugin.GetMixedRealityCameraInfo(0, out extrinsics, out intrinsics))
			{
				OVRPlugin.SetCameraDevicePreferredColorFrameSize(cameraDevice, intrinsics.ImageSensorPixelResolution.w, intrinsics.ImageSensorPixelResolution.h);
			}

			if (useDynamicLighting)
			{
				OVRPlugin.SetCameraDeviceDepthSensingMode(cameraDevice, OVRPlugin.CameraDeviceDepthSensingMode.Fill);
				OVRPlugin.CameraDeviceDepthQuality quality = OVRPlugin.CameraDeviceDepthQuality.Medium;
				if (depthQuality == OVRManager.DepthQuality.Low)
				{
					quality = OVRPlugin.CameraDeviceDepthQuality.Low;
				}
				else if (depthQuality == OVRManager.DepthQuality.Medium)
				{
					quality = OVRPlugin.CameraDeviceDepthQuality.Medium;
				}
				else if (depthQuality == OVRManager.DepthQuality.High)
				{
					quality = OVRPlugin.CameraDeviceDepthQuality.High;
				}
				else
				{
					Debug.LogWarning("Unknown depth quality");
				}
				OVRPlugin.SetCameraDevicePreferredDepthQuality(cameraDevice, quality);
			}

			OVRPlugin.OpenCameraDevice(cameraDevice);
			if (OVRPlugin.HasCameraDeviceOpened(cameraDevice))
			{
				hasCameraDeviceOpened = true;
			}
		}
	}

	public override void Cleanup()
	{
		OVRCompositionUtil.SafeDestroy(ref cameraFramePlaneObject);
		if (hasCameraDeviceOpened)
		{
			OVRPlugin.CloseCameraDevice(cameraDevice);
		}
	}

	public override void RecenterPose()
	{
		boundaryMesh = null;
	}

	protected void CreateCameraFramePlaneObject(GameObject parentObject, Camera mixedRealityCamera, bool useDynamicLighting)
	{
		Debug.Assert(cameraFramePlaneObject == null);
		cameraFramePlaneObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
		cameraFramePlaneObject.name = "MRCameraFrame";
		cameraFramePlaneObject.transform.parent = parentObject.transform;
		cameraFramePlaneObject.GetComponent<Collider>().enabled = false;
		cameraFramePlaneObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		Material cameraFrameMaterial = new Material(Shader.Find(useDynamicLighting ? "Oculus/OVRMRCameraFrameLit" : "Oculus/OVRMRCameraFrame"));
		cameraFramePlaneObject.GetComponent<MeshRenderer>().material = cameraFrameMaterial;
		cameraFrameMaterial.SetColor("_Color", Color.white);
		cameraFrameMaterial.SetFloat("_Visible", 0.0f);
		cameraFramePlaneObject.transform.localScale = new Vector3(4, 4, 4);
		cameraFramePlaneObject.SetActive(true);
		OVRCameraFrameCompositionManager cameraFrameCompositionManager = mixedRealityCamera.gameObject.AddComponent<OVRCameraFrameCompositionManager>();
		cameraFrameCompositionManager.cameraFrameGameObj = cameraFramePlaneObject;
		cameraFrameCompositionManager.composition = this;
	}

	private bool nullcameraRigWarningDisplayed = false;
	protected void UpdateCameraFramePlaneObject(Camera mainCamera, Camera mixedRealityCamera, RenderTexture boundaryMeshMaskTexture)
	{
		bool hasError = false;
		Material cameraFrameMaterial = cameraFramePlaneObject.GetComponent<MeshRenderer>().material;
		Texture2D colorTexture = Texture2D.blackTexture;
		Texture2D depthTexture = Texture2D.whiteTexture;
		if (OVRPlugin.IsCameraDeviceColorFrameAvailable(cameraDevice))
		{
			colorTexture = OVRPlugin.GetCameraDeviceColorFrameTexture(cameraDevice);
		}
		else
		{
			Debug.LogWarning("Camera: color frame not ready");
			hasError = true;
		}
		bool cameraSupportsDepth = OVRPlugin.DoesCameraDeviceSupportDepth(cameraDevice);
		if (useDynamicLighting && cameraSupportsDepth)
		{
			if (OVRPlugin.IsCameraDeviceDepthFrameAvailable(cameraDevice))
			{
				depthTexture = OVRPlugin.GetCameraDeviceDepthFrameTexture(cameraDevice);
			}
			else
			{
				Debug.LogWarning("Camera: depth frame not ready");
				hasError = true;
			}
		}
		if (!hasError)
		{
			Vector3 offset = mainCamera.transform.position - mixedRealityCamera.transform.position;
			float distance = Vector3.Dot(mixedRealityCamera.transform.forward, offset);
			cameraFramePlaneDistance = distance;

			cameraFramePlaneObject.transform.position = mixedRealityCamera.transform.position + mixedRealityCamera.transform.forward * distance;
			cameraFramePlaneObject.transform.rotation = mixedRealityCamera.transform.rotation;

			float tanFov = Mathf.Tan(mixedRealityCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
			cameraFramePlaneObject.transform.localScale = new Vector3(distance * mixedRealityCamera.aspect * tanFov * 2.0f, distance * tanFov * 2.0f, 1.0f);

			float worldHeight = distance * tanFov * 2.0f;
			float worldWidth = worldHeight * mixedRealityCamera.aspect;

			float cullingDistance = float.MaxValue;

			cameraRig = null;
			if (OVRManager.instance.virtualGreenScreenType != OVRManager.VirtualGreenScreenType.Off)
			{
				cameraRig = mainCamera.GetComponentInParent<OVRCameraRig>();
				if (cameraRig != null)
				{
					if (cameraRig.centerEyeAnchor == null)
					{
						cameraRig = null;
					}
				}
				RefreshBoundaryMesh(mixedRealityCamera, out cullingDistance);
			}

			cameraFrameMaterial.mainTexture = colorTexture;
			cameraFrameMaterial.SetTexture("_DepthTex", depthTexture);
			cameraFrameMaterial.SetVector("_FlipParams", new Vector4((OVRManager.instance.flipCameraFrameHorizontally ? 1.0f : 0.0f), (OVRManager.instance.flipCameraFrameVertically ? 1.0f : 0.0f), 0.0f, 0.0f));
			cameraFrameMaterial.SetColor("_ChromaKeyColor", OVRManager.instance.chromaKeyColor);
			cameraFrameMaterial.SetFloat("_ChromaKeySimilarity", OVRManager.instance.chromaKeySimilarity);
			cameraFrameMaterial.SetFloat("_ChromaKeySmoothRange", OVRManager.instance.chromaKeySmoothRange);
			cameraFrameMaterial.SetFloat("_ChromaKeySpillRange", OVRManager.instance.chromaKeySpillRange);
			cameraFrameMaterial.SetVector("_TextureDimension", new Vector4(colorTexture.width, colorTexture.height, 1.0f / colorTexture.width, 1.0f / colorTexture.height));
			cameraFrameMaterial.SetVector("_TextureWorldSize", new Vector4(worldWidth, worldHeight, 0, 0));
			cameraFrameMaterial.SetFloat("_SmoothFactor", OVRManager.instance.dynamicLightingSmoothFactor);
			cameraFrameMaterial.SetFloat("_DepthVariationClamp", OVRManager.instance.dynamicLightingDepthVariationClampingValue);
			cameraFrameMaterial.SetFloat("_CullingDistance", cullingDistance);
			if (OVRManager.instance.virtualGreenScreenType == OVRManager.VirtualGreenScreenType.Off || boundaryMesh == null || boundaryMeshMaskTexture == null)
			{
				cameraFrameMaterial.SetTexture("_MaskTex", Texture2D.whiteTexture);
			}
			else
			{
				if (cameraRig == null)
				{
					if (!nullcameraRigWarningDisplayed)
					{
						Debug.LogWarning("Could not find the OVRCameraRig/CenterEyeAnchor object. Please check if the OVRCameraRig has been setup properly. The virtual green screen has been temporarily disabled");
						nullcameraRigWarningDisplayed = true;
					}

					cameraFrameMaterial.SetTexture("_MaskTex", Texture2D.whiteTexture);
				}
				else
				{
					if (nullcameraRigWarningDisplayed)
					{
						Debug.Log("OVRCameraRig/CenterEyeAnchor object found. Virtual green screen is activated");
						nullcameraRigWarningDisplayed = false;
					}

					cameraFrameMaterial.SetTexture("_MaskTex", boundaryMeshMaskTexture);
				}
			}
		}
	}

	protected void RefreshBoundaryMesh(Camera camera, out float cullingDistance)
	{
		float depthTolerance = OVRManager.instance.virtualGreenScreenApplyDepthCulling ? OVRManager.instance.virtualGreenScreenDepthTolerance : float.PositiveInfinity;
		cullingDistance = OVRCompositionUtil.GetMaximumBoundaryDistance(camera, OVRCompositionUtil.ToBoundaryType(OVRManager.instance.virtualGreenScreenType)) + depthTolerance;
		if (boundaryMesh == null || boundaryMeshType != OVRManager.instance.virtualGreenScreenType || boundaryMeshTopY != OVRManager.instance.virtualGreenScreenTopY || boundaryMeshBottomY != OVRManager.instance.virtualGreenScreenBottomY)
		{
			boundaryMeshTopY = OVRManager.instance.virtualGreenScreenTopY;
			boundaryMeshBottomY = OVRManager.instance.virtualGreenScreenBottomY;
			boundaryMesh = OVRCompositionUtil.BuildBoundaryMesh(OVRCompositionUtil.ToBoundaryType(OVRManager.instance.virtualGreenScreenType), boundaryMeshTopY, boundaryMeshBottomY);
			boundaryMeshType = OVRManager.instance.virtualGreenScreenType;

			// Creating GameObject for testing purpose only
			//GameObject boundaryMeshObject = new GameObject("BoundaryMeshObject");
			//boundaryMeshObject.AddComponent<MeshFilter>().mesh = boundaryMesh;
			//boundaryMeshObject.AddComponent<MeshRenderer>();
		}
	}

	public class OVRCameraFrameCompositionManager : MonoBehaviour
	{
		public GameObject cameraFrameGameObj;
		public OVRCameraComposition composition;
		public RenderTexture boundaryMeshMaskTexture;
		private Material cameraFrameMaterial;
		private Material whiteMaterial;

		void Start()
		{
			Shader shader = Shader.Find("Oculus/Unlit");
			if (!shader)
			{
				Debug.LogError("Oculus/Unlit shader does not exist");
				return;
			}
			whiteMaterial = new Material(shader);
			whiteMaterial.color = Color.white;
		}

		void OnPreRender()
		{
			if (OVRManager.instance.virtualGreenScreenType != OVRManager.VirtualGreenScreenType.Off && boundaryMeshMaskTexture != null && composition.boundaryMesh != null)
			{
				RenderTexture oldRT = RenderTexture.active;
				RenderTexture.active = boundaryMeshMaskTexture;

				// The camera matrices haven't been setup when OnPreRender() is executed. Load the projection manually
				GL.PushMatrix();
				GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);

				GL.Clear(false, true, Color.black);

				for (int i = 0; i < whiteMaterial.passCount; ++i)
				{
					if (whiteMaterial.SetPass(i))
					{
						Graphics.DrawMeshNow(composition.boundaryMesh, composition.cameraRig.ComputeTrackReferenceMatrix());
					}
				}

				GL.PopMatrix();
				RenderTexture.active = oldRT;
			}

			if (cameraFrameGameObj)
			{
				if (cameraFrameMaterial == null)
					cameraFrameMaterial = cameraFrameGameObj.GetComponent<MeshRenderer>().material;
				cameraFrameMaterial.SetFloat("_Visible", 1.0f);
			}
		}
		void OnPostRender()
		{
			if (cameraFrameGameObj)
			{
				Debug.Assert(cameraFrameMaterial);
				cameraFrameMaterial.SetFloat("_Visible", 0.0f);
			}
		}
	}

}

#endif