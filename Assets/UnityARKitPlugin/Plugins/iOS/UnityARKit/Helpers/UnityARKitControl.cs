namespace UnityEngine.XR.iOS
{
    public class UnityARKitControl : MonoBehaviour
    {
        private readonly UnityARAlignment[] alignmentOptions = new UnityARAlignment[3];
        private int currentAlignmentIndex;

        private int currentOptionIndex;
        private int currentPlaneIndex;
        private readonly UnityARPlaneDetection[] planeOptions = new UnityARPlaneDetection[4];

        private readonly UnityARSessionRunOption[] runOptions = new UnityARSessionRunOption[4];

        // Use this for initialization
        private void Start()
        {
            runOptions[0] = UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors |
                            UnityARSessionRunOption.ARSessionRunOptionResetTracking;
            runOptions[1] = UnityARSessionRunOption.ARSessionRunOptionResetTracking;
            runOptions[2] = UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors;
            runOptions[3] = 0;

            alignmentOptions[0] = UnityARAlignment.UnityARAlignmentCamera;
            alignmentOptions[1] = UnityARAlignment.UnityARAlignmentGravity;
            alignmentOptions[2] = UnityARAlignment.UnityARAlignmentGravityAndHeading;

            planeOptions[0] = UnityARPlaneDetection.Horizontal;
            planeOptions[1] = UnityARPlaneDetection.None;
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 200, 50), "Stop"))
                UnityARSessionNativeInterface.GetARSessionNativeInterface().Pause();

            if (GUI.Button(new Rect(300, 100, 200, 50), "Start"))
            {
                var sessionConfig = new ARKitWorldTrackingSessionConfiguration(alignmentOptions[currentAlignmentIndex],
                    planeOptions[currentPlaneIndex]);
                UnityARSessionNativeInterface.GetARSessionNativeInterface()
                    .RunWithConfigAndOptions(sessionConfig, runOptions[currentOptionIndex]);
            }


            if (GUI.Button(new Rect(100, 300, 200, 100), "Start Non-WT Session"))
            {
                var sessionConfig = new ARKitSessionConfiguration(alignmentOptions[currentAlignmentIndex], true, true);
                UnityARSessionNativeInterface.GetARSessionNativeInterface().RunWithConfig(sessionConfig);
            }


            var runOptionStr = currentOptionIndex == 0 ? "Full" :
                currentOptionIndex == 1 ? "Tracking" :
                currentOptionIndex == 2 ? "Anchors" : "None";
            if (GUI.Button(new Rect(100, 200, 150, 50), "RunOption:" + runOptionStr))
                currentOptionIndex = (currentOptionIndex + 1) % 4;

            var alignmentOptionStr = currentAlignmentIndex == 0 ? "Camera" :
                currentAlignmentIndex == 1 ? "Gravity" : "GravityAndHeading";
            if (GUI.Button(new Rect(300, 200, 150, 50), "AlignOption:" + alignmentOptionStr))
                currentAlignmentIndex = (currentAlignmentIndex + 1) % 3;

            var planeOptionStr = currentPlaneIndex == 0 ? "Horizontal" : "None";
            if (GUI.Button(new Rect(500, 200, 150, 50), "PlaneOption:" + planeOptionStr))
                currentPlaneIndex = (currentPlaneIndex + 1) % 2;
        }
    }
}