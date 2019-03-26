using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class UpdateWorldMappingStatus : MonoBehaviour
{
    public Text text;
    public Text tracking;


    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += CheckWorldMapStatus;
    }

    private void CheckWorldMapStatus(UnityARCamera cam)
    {
        text.text = cam.worldMappingStatus.ToString();
        tracking.text = cam.trackingState + " " + cam.trackingReason;
    }

    private void OnDestroy()
    {
        UnityARSessionNativeInterface.ARFrameUpdatedEvent -= CheckWorldMapStatus;
    }
}