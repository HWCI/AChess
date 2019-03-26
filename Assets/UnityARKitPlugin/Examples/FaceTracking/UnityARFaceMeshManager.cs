using UnityEngine;
using UnityEngine.XR.iOS;

public class UnityARFaceMeshManager : MonoBehaviour
{
    private Mesh faceMesh;

    private UnityARSessionNativeInterface m_session;

    [SerializeField] private MeshFilter meshFilter;

    // Use this for initialization
    private void Start()
    {
        m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

        Application.targetFrameRate = 60;
        var config = new ARKitFaceTrackingConfiguration();
        config.alignment = UnityARAlignment.UnityARAlignmentGravity;
        config.enableLightEstimation = true;

        if (config.IsSupported && meshFilter != null)
        {
            m_session.RunWithConfig(config);

            UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
            UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
            UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
        }
    }

    private void FaceAdded(ARFaceAnchor anchorData)
    {
        gameObject.transform.localPosition = UnityARMatrixOps.GetPosition(anchorData.transform);
        gameObject.transform.localRotation = UnityARMatrixOps.GetRotation(anchorData.transform);


        faceMesh = new Mesh();
        faceMesh.vertices = anchorData.faceGeometry.vertices;
        faceMesh.uv = anchorData.faceGeometry.textureCoordinates;
        faceMesh.triangles = anchorData.faceGeometry.triangleIndices;

        // Assign the mesh object and update it.
        faceMesh.RecalculateBounds();
        faceMesh.RecalculateNormals();
        meshFilter.mesh = faceMesh;
    }

    private void FaceUpdated(ARFaceAnchor anchorData)
    {
        if (faceMesh != null)
        {
            gameObject.transform.localPosition = UnityARMatrixOps.GetPosition(anchorData.transform);
            gameObject.transform.localRotation = UnityARMatrixOps.GetRotation(anchorData.transform);
            faceMesh.vertices = anchorData.faceGeometry.vertices;
            faceMesh.uv = anchorData.faceGeometry.textureCoordinates;
            faceMesh.triangles = anchorData.faceGeometry.triangleIndices;
            faceMesh.RecalculateBounds();
            faceMesh.RecalculateNormals();
        }
    }

    private void FaceRemoved(ARFaceAnchor anchorData)
    {
        meshFilter.mesh = null;
        faceMesh = null;
    }


    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDestroy()
    {
    }
}