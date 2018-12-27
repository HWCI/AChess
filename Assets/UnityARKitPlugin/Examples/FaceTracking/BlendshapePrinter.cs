using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class BlendshapePrinter : MonoBehaviour
{
    private Dictionary<string, float> currentBlendShapes;

    private bool shapeEnabled;

    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
        UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
        UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
    }

    private void OnGUI()
    {
        if (shapeEnabled)
        {
            var blendshapes = "";
            var shapeNames = "";
            var valueNames = "";
            foreach (var kvp in currentBlendShapes)
            {
                blendshapes += " [";
                blendshapes += kvp.Key;
                blendshapes += ":";
                blendshapes += kvp.Value.ToString();
                blendshapes += "]\n";
                shapeNames += "\"";
                shapeNames += kvp.Key;
                shapeNames += "\",\n";
                valueNames += kvp.Value.ToString();
                valueNames += "\n";
            }

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            GUILayout.Box(blendshapes);
            GUILayout.EndHorizontal();

            Debug.Log(shapeNames);
            Debug.Log(valueNames);
        }
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