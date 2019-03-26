using UnityEngine;
using UnityEngine.XR.iOS;

public class ObjectScanSessionManager : MonoBehaviour
{
    public bool enableAutoFocus = true;
    public bool enableLightEstimation = true;
    public bool getPointCloudData = true;

    public Camera m_camera;
    private UnityARSessionNativeInterface m_session;
    public UnityARPlaneDetection planeDetection = UnityARPlaneDetection.Horizontal;


    private bool sessionStarted;

    [Header("AR Config Options")] public UnityARAlignment startAlignment = UnityARAlignment.UnityARAlignmentGravity;

    public ARKitWorldTrackingSessionConfiguration sessionConfiguration
    {
        get
        {
            var config = new ARKitWorldTrackingSessionConfiguration();
            config.planeDetection = planeDetection;
            config.alignment = startAlignment;
            config.getPointCloudData = getPointCloudData;
            config.enableLightEstimation = enableLightEstimation;
            config.enableAutoFocus = enableAutoFocus;

            return config;
        }
    }

    //Warning: using this configuration is expensive CPU and battery-wise - use in limited amounts!
    public ARKitObjectScanningSessionConfiguration objScanSessionConfiguration
    {
        get
        {
            var config = new ARKitObjectScanningSessionConfiguration();
            config.planeDetection = planeDetection;
            config.alignment = startAlignment;
            config.getPointCloudData = getPointCloudData;
            config.enableLightEstimation = enableLightEstimation;
            config.enableAutoFocus = enableAutoFocus;

            return config;
        }
    }

    // Use this for initialization
    private void Start()
    {
        m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        if (m_camera == null) m_camera = Camera.main;
        Application.targetFrameRate = 60;

        StartObjectScanningSession();
    }


    public void StartObjectScanningSession()
    {
        sessionStarted = false;
        var config = objScanSessionConfiguration;
        if (config.IsSupported)
        {
            m_session.RunWithConfig(config);
            UnityARSessionNativeInterface.ARFrameUpdatedEvent += FirstFrameUpdate;
        }
    }


    private void FirstFrameUpdate(UnityARCamera cam)
    {
        sessionStarted = true;
        UnityARSessionNativeInterface.ARFrameUpdatedEvent -= FirstFrameUpdate;
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