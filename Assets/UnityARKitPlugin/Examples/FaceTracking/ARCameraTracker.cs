using UnityEngine;
using UnityEngine.XR.iOS;

public class ARCameraTracker : MonoBehaviour
{
    private bool sessionStarted;

    [SerializeField] private Camera trackedCamera;

    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += FirstFrameUpdate;
    }

    private void OnDestroy()
    {
    }

    private void FirstFrameUpdate(UnityARCamera cam)
    {
        sessionStarted = true;
        UnityARSessionNativeInterface.ARFrameUpdatedEvent -= FirstFrameUpdate;
    }

    // Update is called once per frame
    private void Update()
    {
        if (trackedCamera != null && sessionStarted)
        {
            var cameraPose = UnityARSessionNativeInterface.GetARSessionNativeInterface().GetCameraPose();
            trackedCamera.transform.localPosition = UnityARMatrixOps.GetPosition(cameraPose);
            trackedCamera.transform.localRotation = UnityARMatrixOps.GetRotation(cameraPose);

            trackedCamera.projectionMatrix =
                UnityARSessionNativeInterface.GetARSessionNativeInterface().GetCameraProjection();
        }
    }
}