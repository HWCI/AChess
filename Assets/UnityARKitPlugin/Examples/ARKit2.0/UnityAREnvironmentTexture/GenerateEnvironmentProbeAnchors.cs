using Collections.Hybrid.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class GenerateEnvironmentProbeAnchors : MonoBehaviour
{
    [SerializeField] private ReflectionProbeGameObject m_ReflectionProbePrefab;

    private LinkedListDictionary<string, ReflectionProbeGameObject> probeAnchorMap;


    private void Start()
    {
        probeAnchorMap = new LinkedListDictionary<string, ReflectionProbeGameObject>();
        UnityARSessionNativeInterface.AREnvironmentProbeAnchorAddedEvent += EnvironmentProbeAnchorAdded;
        UnityARSessionNativeInterface.AREnvironmentProbeAnchorRemovedEvent += EnvironmentProbeAnchorRemoved;
        UnityARSessionNativeInterface.AREnvironmentProbeAnchorUpdatedEvent += EnvironmentProbeAnchorUpdated;
    }

    private void EnvironmentProbeAnchorUpdated(AREnvironmentProbeAnchor anchorData)
    {
        if (probeAnchorMap.ContainsKey(anchorData.identifier))
            probeAnchorMap[anchorData.identifier].UpdateEnvironmentProbe(anchorData);
    }

    private void EnvironmentProbeAnchorRemoved(AREnvironmentProbeAnchor anchorData)
    {
        if (probeAnchorMap.ContainsKey(anchorData.identifier))
        {
            var rpgo = probeAnchorMap[anchorData.identifier];
            Destroy(rpgo.gameObject);
            probeAnchorMap.Remove(anchorData.identifier);
        }
    }

    private void EnvironmentProbeAnchorAdded(AREnvironmentProbeAnchor anchorData)
    {
        var go = Instantiate(m_ReflectionProbePrefab);
        if (go != null)
        {
            //do coordinate conversion from ARKit to Unity
            go.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            go.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

            probeAnchorMap[anchorData.identifier] = go;
            go.UpdateEnvironmentProbe(anchorData);
        }
    }

    private void OnDestroy()
    {
        UnityARSessionNativeInterface.AREnvironmentProbeAnchorAddedEvent -= EnvironmentProbeAnchorAdded;
        UnityARSessionNativeInterface.AREnvironmentProbeAnchorRemovedEvent -= EnvironmentProbeAnchorRemoved;
        UnityARSessionNativeInterface.AREnvironmentProbeAnchorUpdatedEvent -= EnvironmentProbeAnchorUpdated;

        foreach (var rpgo in probeAnchorMap.Values) Destroy(rpgo);

        probeAnchorMap.Clear();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}