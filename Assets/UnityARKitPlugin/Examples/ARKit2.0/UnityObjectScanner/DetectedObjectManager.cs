using Collections.Hybrid.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class DetectedObjectManager : MonoBehaviour
{
    public GameObject m_ObjectPrefab;

    private LinkedListDictionary<string, GameObject> objectAnchorMap;

    // Use this for initialization
    private void Start()
    {
        objectAnchorMap = new LinkedListDictionary<string, GameObject>();
        UnityARSessionNativeInterface.ARObjectAnchorAddedEvent += ObjectAnchorAdded;
        UnityARSessionNativeInterface.ARObjectAnchorRemovedEvent += ObjectAnchorRemoved;
        UnityARSessionNativeInterface.ARObjectAnchorUpdatedEvent += ObjectAnchorUpdated;
    }

    private void ObjectAnchorUpdated(ARObjectAnchor anchorData)
    {
        Debug.Log("ObjectAnchorUpdated");
        if (objectAnchorMap.ContainsKey(anchorData.referenceObjectName))
        {
            var go = objectAnchorMap[anchorData.referenceObjectName];
            //do coordinate conversion from ARKit to Unity
            go.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            go.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
        }
    }

    private void ObjectAnchorRemoved(ARObjectAnchor anchorData)
    {
        Debug.Log("ObjectAnchorRemoved");
        if (objectAnchorMap.ContainsKey(anchorData.referenceObjectName))
        {
            var rpgo = objectAnchorMap[anchorData.referenceObjectName];
            Destroy(rpgo.gameObject);
            objectAnchorMap.Remove(anchorData.identifier);
        }
    }

    private void ObjectAnchorAdded(ARObjectAnchor anchorData)
    {
        Debug.Log("ObjectAnchorAdded");
        var go = Instantiate(m_ObjectPrefab);
        if (go != null)
        {
            //do coordinate conversion from ARKit to Unity
            go.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            go.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

            objectAnchorMap[anchorData.referenceObjectName] = go;
            go.name = anchorData.referenceObjectName;
            var objText = go.GetComponent<ObjectText>();
            if (objText) objText.UpdateTextMesh(anchorData.referenceObjectName);
        }
    }

    private void OnDestroy()
    {
        UnityARSessionNativeInterface.ARObjectAnchorAddedEvent -= ObjectAnchorAdded;
        UnityARSessionNativeInterface.ARObjectAnchorRemovedEvent -= ObjectAnchorRemoved;
        UnityARSessionNativeInterface.ARObjectAnchorUpdatedEvent -= ObjectAnchorUpdated;

        foreach (var rpgo in objectAnchorMap.Values) Destroy(rpgo);

        objectAnchorMap.Clear();
    }


    // Update is called once per frame
    private void Update()
    {
    }
}