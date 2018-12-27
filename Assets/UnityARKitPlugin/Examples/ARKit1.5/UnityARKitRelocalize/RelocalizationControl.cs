using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class RelocalizationControl : MonoBehaviour
{
    public Text buttonText;
    public Text trackingReasonText;
    public Text trackingStateText;

    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARSessionShouldAttemptRelocalization = false;
        UpdateText();

        UnityARSessionNativeInterface.ARSessionTrackingChangedEvent += TrackingChanged;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void TrackingChanged(UnityARCamera cam)
    {
        trackingStateText.text = cam.trackingState.ToString();
        trackingReasonText.text = cam.trackingReason.ToString();
    }

    private void OnDestroy()
    {
        UnityARSessionNativeInterface.ARSessionTrackingChangedEvent -= TrackingChanged;
    }

    private void UpdateText()
    {
        buttonText.text = UnityARSessionNativeInterface.ARSessionShouldAttemptRelocalization
            ? "SHOULD RELOCALIZE"
            : "NO RELOCALIZE";
    }

    public void ToggleRelocalization()
    {
        UnityARSessionNativeInterface.ARSessionShouldAttemptRelocalization =
            !UnityARSessionNativeInterface.ARSessionShouldAttemptRelocalization;
        UpdateText();
    }
}