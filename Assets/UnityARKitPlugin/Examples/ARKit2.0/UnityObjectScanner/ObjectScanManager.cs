using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class ObjectScanManager : MonoBehaviour
{
    private bool detectionMode;

    [SerializeField] private Text listOfObjects;

    [SerializeField] private ObjectScanSessionManager m_ARSessionManager;

    private int objIndex;

    private PickBoundingBox pickBoundingBox;
    private List<ARReferenceObject> scannedObjects;

    private static UnityARSessionNativeInterface session
    {
        get { return UnityARSessionNativeInterface.GetARSessionNativeInterface(); }
    }

    private void Start()
    {
        scannedObjects = new List<ARReferenceObject>();
        pickBoundingBox = GetComponent<PickBoundingBox>();
    }

    private void OnDestroy()
    {
        ClearScannedObjects();
    }

    public void CreateReferenceObject()
    {
        //this script should be placed on the bounding volume GameObject
        CreateReferenceObject(pickBoundingBox.transform,
            pickBoundingBox.bounds.center - pickBoundingBox.transform.position, pickBoundingBox.bounds.size);
    }

    public void CreateReferenceObject(Transform objectTransform, Vector3 center, Vector3 extent)
    {
        session.ExtractReferenceObjectAsync(objectTransform, center, extent, referenceObject =>
        {
            if (referenceObject != null)
            {
                Debug.LogFormat("ARReferenceObject created: center {0} extent {1}", referenceObject.center,
                    referenceObject.extent);
                referenceObject.name = "objScan_" + objIndex++;
                Debug.LogFormat("ARReferenceObject has name {0}", referenceObject.name);
                scannedObjects.Add(referenceObject);
                UpdateList();
            }
            else
            {
                Debug.Log("Failed to create ARReferenceObject.");
            }
        });
    }

    private void UpdateList()
    {
        var members = "";
        foreach (var arro in scannedObjects) members += arro.name + ",";
        listOfObjects.text = members;
    }

    public void DetectScannedObjects(Text toChange)
    {
        detectionMode = !detectionMode;
        if (detectionMode)
        {
            StartDetecting();
            toChange.text = "Stop Detecting";
        }
        else
        {
            m_ARSessionManager.StartObjectScanningSession();
            toChange.text = "Detect Objects";
        }
    }

    private void StartDetecting()
    {
        //create a set out of the scanned objects
        var ptrReferenceObjectsSet = session.CreateNativeReferenceObjectsSet(scannedObjects);

        //restart session without resetting tracking 
        var config = m_ARSessionManager.sessionConfiguration;

        //use object set from above to detect objects
        config.dynamicReferenceObjectsPtr = ptrReferenceObjectsSet;

        //Debug.Log("Restarting session without resetting tracking");
        session.RunWithConfigAndOptions(config,
            UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors |
            UnityARSessionRunOption.ARSessionRunOptionResetTracking);
    }


    public void ClearScannedObjects()
    {
        detectionMode = false;
        scannedObjects.Clear();
        UpdateList();
        m_ARSessionManager.StartObjectScanningSession();
    }

    public void SaveScannedObjects()
    {
        if (scannedObjects.Count == 0)
            return;

        var pathToSaveTo = Path.Combine(Application.persistentDataPath, "ARReferenceObjects");

        if (!Directory.Exists(pathToSaveTo)) Directory.CreateDirectory(pathToSaveTo);

        foreach (var arro in scannedObjects)
        {
            var fullPath = Path.Combine(pathToSaveTo, arro.name + ".arobject");
            arro.Save(fullPath);
        }
    }
}