using UnityEngine;
using UnityEngine.XR.iOS;

public class GenerateImageAnchor : MonoBehaviour
{
    private GameObject imageAnchorGO;

    [SerializeField] private GameObject prefabToGenerate;


    [SerializeField] private ARReferenceImage referenceImage;

    // Use this for initialization
    private void Start()
    {
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;
    }

    private void AddImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor added[{0}] : tracked => {1}", arImageAnchor.identifier, arImageAnchor.isTracked);
        if (arImageAnchor.referenceImageName == referenceImage.imageName)
        {
            var position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);
            var rotation = UnityARMatrixOps.GetRotation(arImageAnchor.transform);

            imageAnchorGO = Instantiate(prefabToGenerate, position, rotation);
        }
    }

    private void UpdateImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor updated[{0}] : tracked => {1}", arImageAnchor.identifier,
            arImageAnchor.isTracked);
        if (arImageAnchor.referenceImageName == referenceImage.imageName)
        {
            if (arImageAnchor.isTracked)
            {
                if (!imageAnchorGO.activeSelf) imageAnchorGO.SetActive(true);
                imageAnchorGO.transform.position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);
                imageAnchorGO.transform.rotation = UnityARMatrixOps.GetRotation(arImageAnchor.transform);
            }
            else if (imageAnchorGO.activeSelf)
            {
                imageAnchorGO.SetActive(false);
            }
        }
    }

    private void RemoveImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor removed[{0}] : tracked => {1}", arImageAnchor.identifier,
            arImageAnchor.isTracked);
        if (imageAnchorGO) Destroy(imageAnchorGO);
    }

    private void OnDestroy()
    {
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent -= AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent -= UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveImageAnchor;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}