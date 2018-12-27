using UnityEngine;
using UnityEngine.XR.iOS;

public class GenerateObjectAnchor : MonoBehaviour
{
    private GameObject objectAnchorGO;

    [SerializeField] private GameObject prefabToGenerate;

    [SerializeField] private ARReferenceObjectAsset referenceObjectAsset;

    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARObjectAnchorAddedEvent += AddObjectAnchor;
        UnityARSessionNativeInterface.ARObjectAnchorUpdatedEvent += UpdateObjectAnchor;
        UnityARSessionNativeInterface.ARObjectAnchorRemovedEvent += RemoveObjectAnchor;
    }

    private void AddObjectAnchor(ARObjectAnchor arObjectAnchor)
    {
        Debug.Log("object anchor added");
        if (arObjectAnchor.referenceObjectName == referenceObjectAsset.objectName)
        {
            var position = UnityARMatrixOps.GetPosition(arObjectAnchor.transform);
            var rotation = UnityARMatrixOps.GetRotation(arObjectAnchor.transform);

            objectAnchorGO = Instantiate(prefabToGenerate, position, rotation);
        }
    }

    private void UpdateObjectAnchor(ARObjectAnchor arObjectAnchor)
    {
        Debug.Log("object anchor updated");
        if (arObjectAnchor.referenceObjectName == referenceObjectAsset.objectName)
        {
            objectAnchorGO.transform.position = UnityARMatrixOps.GetPosition(arObjectAnchor.transform);
            objectAnchorGO.transform.rotation = UnityARMatrixOps.GetRotation(arObjectAnchor.transform);
        }
    }

    private void RemoveObjectAnchor(ARObjectAnchor arObjectAnchor)
    {
        Debug.Log("object anchor removed");
        if (arObjectAnchor.referenceObjectName == referenceObjectAsset.objectName && objectAnchorGO != null)
            Destroy(objectAnchorGO);
    }

    private void OnDestroy()
    {
        UnityARSessionNativeInterface.ARObjectAnchorAddedEvent -= AddObjectAnchor;
        UnityARSessionNativeInterface.ARObjectAnchorUpdatedEvent -= UpdateObjectAnchor;
        UnityARSessionNativeInterface.ARObjectAnchorRemovedEvent -= RemoveObjectAnchor;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}