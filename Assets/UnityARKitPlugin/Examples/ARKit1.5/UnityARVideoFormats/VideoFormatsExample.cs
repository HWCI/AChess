using UnityEngine;
using UnityEngine.XR.iOS;

public class VideoFormatsExample : MonoBehaviour
{
    public Transform formatsParent;
    public GameObject videoFormatButtonPrefab;

    // Use this for initialization
    private void Start()
    {
        VideoFormatButton.FormatButtonPressedEvent += ExampletButtonPressed;
        PopulateVideoFormatButtons();
    }

    private void OnDestroy()
    {
        VideoFormatButton.FormatButtonPressedEvent -= ExampletButtonPressed;
    }

    private void PopulateVideoFormatButtons()
    {
        foreach (var vf in UnityARVideoFormat.SupportedVideoFormats())
        {
            var go = Instantiate(videoFormatButtonPrefab, formatsParent);
            var vfb = go.GetComponent<VideoFormatButton>();
            if (vfb != null) vfb.Populate(vf);
        }
    }

    public void ExampletButtonPressed(UnityARVideoFormat videoFormat)
    {
        //Restart session with new video format in config

        var session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

        var config = new ARKitWorldTrackingSessionConfiguration();

        if (config.IsSupported)
        {
            config.planeDetection = UnityARPlaneDetection.HorizontalAndVertical;
            config.alignment = UnityARAlignment.UnityARAlignmentGravity;
            config.getPointCloudData = true;
            config.enableLightEstimation = true;
            config.enableAutoFocus = true;
            config.videoFormat = videoFormat.videoFormatPtr;
            Application.targetFrameRate = videoFormat.framesPerSecond;

            var runOption = UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors |
                            UnityARSessionRunOption.ARSessionRunOptionResetTracking;
            session.RunWithConfigAndOptions(config, runOption);
        }
    }
}