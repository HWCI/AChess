using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class BlendshapeDriver : MonoBehaviour
{
    private Dictionary<string, float> currentBlendShapes;

    private SkinnedMeshRenderer skinnedMeshRenderer;

    // Use this for initialization
    private void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (skinnedMeshRenderer)
        {
            UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
            UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
        }
    }

    private void FaceAdded(ARFaceAnchor anchorData)
    {
        currentBlendShapes = anchorData.blendShapes;
    }

    private void FaceUpdated(ARFaceAnchor anchorData)
    {
        currentBlendShapes = anchorData.blendShapes;
    }


    // Update is called once per frame
    private void Update()
    {
        if (currentBlendShapes != null)
            foreach (var kvp in currentBlendShapes)
            {
                var blendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("blendShape2." + kvp.Key);
                if (blendShapeIndex >= 0) skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, kvp.Value * 100.0f);
            }
    }
}