using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class TongueDetector : MonoBehaviour
{
    private Dictionary<string, float> currentBlendShapes;
    private bool shapeEnabled;
    public GameObject tongueImage;

    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
        UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
        UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
    }

    private void OnGUI()
    {
        var enableTongue = false;

        if (shapeEnabled)
            if (currentBlendShapes.ContainsKey(ARBlendShapeLocation.TongueOut))
                enableTongue = currentBlendShapes[ARBlendShapeLocation.TongueOut] > 0.5f;

        tongueImage.SetActive(enableTongue);
    }

    private void FaceAdded(ARFaceAnchor anchorData)
    {
        shapeEnabled = true;
        currentBlendShapes = anchorData.blendShapes;
    }

    private void FaceUpdated(ARFaceAnchor anchorData)
    {
        currentBlendShapes = anchorData.blendShapes;
    }

    private void FaceRemoved(ARFaceAnchor anchorData)
    {
        shapeEnabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}