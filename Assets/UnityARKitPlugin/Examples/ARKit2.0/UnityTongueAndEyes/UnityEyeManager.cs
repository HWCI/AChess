using UnityEngine;
using UnityEngine.XR.iOS;

public class UnityEyeManager : MonoBehaviour
{
    [SerializeField] private GameObject eyePrefab;

    private GameObject leftEyeGo;

    private UnityARSessionNativeInterface m_session;
    private GameObject rightEyeGo;

    // Use this for initialization
    private void Start()
    {
        m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

        Application.targetFrameRate = 60;
        var config = new ARKitFaceTrackingConfiguration();
        config.alignment = UnityARAlignment.UnityARAlignmentGravity;
        config.enableLightEstimation = true;

        if (config.IsSupported)
        {
            m_session.RunWithConfig(config);

            UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
            UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
            UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
        }

        leftEyeGo = Instantiate(eyePrefab);
        rightEyeGo = Instantiate(eyePrefab);

        leftEyeGo.SetActive(false);
        rightEyeGo.SetActive(false);
    }

    private void FaceAdded(ARFaceAnchor anchorData)
    {
        leftEyeGo.transform.position = anchorData.leftEyePose.position;
        leftEyeGo.transform.rotation = anchorData.leftEyePose.rotation;

        rightEyeGo.transform.position = anchorData.rightEyePose.position;
        rightEyeGo.transform.rotation = anchorData.rightEyePose.rotation;

        leftEyeGo.SetActive(true);
        rightEyeGo.SetActive(true);
    }

    private void FaceUpdated(ARFaceAnchor anchorData)
    {
        leftEyeGo.transform.position = anchorData.leftEyePose.position;
        leftEyeGo.transform.rotation = anchorData.leftEyePose.rotation;

        rightEyeGo.transform.position = anchorData.rightEyePose.position;
        rightEyeGo.transform.rotation = anchorData.rightEyePose.rotation;
    }

    private void FaceRemoved(ARFaceAnchor anchorData)
    {
        leftEyeGo.SetActive(false);
        rightEyeGo.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}