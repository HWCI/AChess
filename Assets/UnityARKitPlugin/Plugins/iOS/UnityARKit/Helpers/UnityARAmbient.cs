namespace UnityEngine.XR.iOS
{
    public class UnityARAmbient : MonoBehaviour
    {
        private Light l;

        public void Start()
        {
            l = GetComponent<Light>();
            UnityARSessionNativeInterface.ARFrameUpdatedEvent += UpdateLightEstimation;
        }

        private void UpdateLightEstimation(UnityARCamera camera)
        {
            if (camera.lightData.arLightingType == LightDataType.LightEstimate)
            {
                // Convert ARKit intensity to Unity intensity
                // ARKit ambient intensity ranges 0-2000
                // Unity ambient intensity ranges 0-8 (for over-bright lights)
                var newai = camera.lightData.arLightEstimate.ambientIntensity;
                l.intensity = newai / 1000.0f;

                //Unity Light has functionality to filter the light color to correct temperature
                //https://docs.unity3d.com/ScriptReference/Light-colorTemperature.html
                l.colorTemperature = camera.lightData.arLightEstimate.ambientColorTemperature;
            }
        }

        private void OnDestroy()
        {
            UnityARSessionNativeInterface.ARFrameUpdatedEvent -= UpdateLightEstimation;
        }
    }
}