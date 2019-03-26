using UnityEngine;
using UnityEngine.XR.iOS;

public class UnityARCameraManager : MonoBehaviour
{
    [Header("Image Tracking")] public ARReferenceImagesSet detectionImages;

    [Header("Object Tracking")] public ARReferenceObjectsSetAsset detectionObjects;

    public bool enableAutoFocus = true;
    public bool enableLightEstimation = true;

    public UnityAREnvironmentTexturing environmentTexturing =
        UnityAREnvironmentTexturing.UnityAREnvironmentTexturingNone;

    public bool getPointCloud = true;

    public Camera m_camera;
    private UnityARSessionNativeInterface m_session;
    public int maximumNumberOfTrackedImages;
    public UnityARPlaneDetection planeDetection = UnityARPlaneDetection.Horizontal;
    private Material savedClearMaterial;
    private bool sessionStarted;

    [Header("AR Config Options")] public UnityARAlignment startAlignment = UnityARAlignment.UnityARAlignmentGravity;

    public ARKitWorldTrackingSessionConfiguration sessionConfiguration
    {
        get
        {
            var config = new ARKitWorldTrackingSessionConfiguration();
            config.planeDetection = planeDetection;
            config.alignment = startAlignment;
            config.getPointCloudData = getPointCloud;
            config.enableLightEstimation = enableLightEstimation;
            config.enableAutoFocus = enableAutoFocus;
            config.maximumNumberOfTrackedImages = maximumNumberOfTrackedImages;
            config.environmentTexturing = environmentTexturing;
            if (detectionImages != null)
                config.referenceImagesGroupName = detectionImages.resourceGroupName;

            if (detectionObjects != null)
            {
                config.referenceObjectsGroupName = ""; //lets not read from XCode asset catalog right now
                config.dynamicReferenceObjectsPtr =
                    m_session.CreateNativeReferenceObjectsSet(detectionObjects.LoadReferenceObjectsInSet());
            }

            return config;
        }
    }

    // Use this for initialization
    private void Start()
    {
        m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

        Application.targetFrameRate = 60;

        var config = sessionConfiguration;
        if (config.IsSupported)
        {
            m_session.RunWithConfig(config);
            UnityARSessionNativeInterface.ARFrameUpdatedEvent += FirstFrameUpdate;
        }

        if (m_camera == null) m_camera = Camera.main;
    }

    private void OnDestroy()
    {
        m_session.Pause();
    }

    private void FirstFrameUpdate(UnityARCamera cam)
    {
        sessionStarted = true;
        UnityARSessionNativeInterface.ARFrameUpdatedEvent -= FirstFrameUpdate;
    }

    public void SetCamera(Camera newCamera)
    {
        if (m_camera != null)
        {
            var oldARVideo = m_camera.gameObject.GetComponent<UnityARVideo>();
            if (oldARVideo != null)
            {
                savedClearMaterial = oldARVideo.m_ClearMaterial;
                Destroy(oldARVideo);
            }
        }

        SetupNewCamera(newCamera);
    }

    private void SetupNewCamera(Camera newCamera)
    {
        m_camera = newCamera;

        if (m_camera != null)
        {
            var unityARVideo = m_camera.gameObject.GetComponent<UnityARVideo>();
            if (unityARVideo != null)
            {
                savedClearMaterial = unityARVideo.m_ClearMaterial;
                Destroy(unityARVideo);
            }

            unityARVideo = m_camera.gameObject.AddComponent<UnityARVideo>();
            unityARVideo.m_ClearMaterial = savedClearMaterial;
        }
    }

    // Update is called once per frame

    private void Update()
    {
        if (m_camera != null && sessionStarted)
        {
            // JUST WORKS!
            var matrix = m_session.GetCameraPose();
            m_camera.transform.localPosition = UnityARMatrixOps.GetPosition(matrix);
            m_camera.transform.localRotation = UnityARMatrixOps.GetRotation(matrix);

            m_camera.projectionMatrix = m_session.GetCameraProjection();
        }
    }
}