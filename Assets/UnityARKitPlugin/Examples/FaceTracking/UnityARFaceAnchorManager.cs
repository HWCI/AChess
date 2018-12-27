using UnityEngine;
using UnityEngine.XR.iOS;

public class UnityARFaceAnchorManager : MonoBehaviour
{
    [SerializeField] private GameObject anchorPrefab;

    private UnityARSessionNativeInterface m_session;

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
    }

    private void FaceAdded(ARFaceAnchor anchorData)
    {
        anchorPrefab.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
        anchorPrefab.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        anchorPrefab.SetActive(true);
    }

    private void FaceUpdated(ARFaceAnchor anchorData)
    {
        if (anchorPrefab.activeSelf != anchorData.isTracked) anchorPrefab.SetActive(anchorData.isTracked);

        if (anchorData.isTracked)
        {
            anchorPrefab.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            anchorPrefab.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        }
    }

    private void FaceRemoved(ARFaceAnchor anchorData)
    {
        anchorPrefab.SetActive(false);
    }


    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDestroy()
    {
    }
}