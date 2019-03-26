using UnityEngine;
using UnityEngine.XR.iOS;

[RequireComponent(typeof(ReflectionProbe))]
public class ReflectionProbeGameObject : MonoBehaviour
{
    [SerializeField] private GameObject debugExtentGO;

    private Cubemap latchedTexture;
    private bool latchUpdate;

    private ReflectionProbe reflectionProbe;

    // Use this for initialization
    private void Start()
    {
        reflectionProbe = GetComponent<ReflectionProbe>();
    }


    public void UpdateEnvironmentProbe(AREnvironmentProbeAnchor environmentProbeAnchor)
    {
        transform.position = UnityARMatrixOps.GetPosition(environmentProbeAnchor.transform);

        var rot = UnityARMatrixOps.GetRotation(environmentProbeAnchor.transform);

        //rot.z = -rot.z;
        //rot.w = -rot.w;

        transform.rotation = rot;

        if (reflectionProbe != null) reflectionProbe.size = environmentProbeAnchor.Extent;

        if (debugExtentGO != null) debugExtentGO.transform.localScale = environmentProbeAnchor.Extent;

        latchedTexture = environmentProbeAnchor.Cubemap;
        latchUpdate = true;
    }

    private void Update()
    {
        //always make sure to update texture in next update
        if (latchUpdate && reflectionProbe != null)
        {
            if (reflectionProbe.customBakedTexture != null) Destroy(reflectionProbe.customBakedTexture);
            reflectionProbe.customBakedTexture = latchedTexture;
            latchUpdate = false;
        }
    }
}