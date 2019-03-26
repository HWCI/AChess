using UnityEngine;

public class CharacterViewer : MonoBehaviour
{
    public Transform cameras;
    private Vector3 deltaPosition;
    private Vector3 lastPosition = Vector3.zero;
    private bool rotating;

    private Transform targetForCamera;

    private void Awake()
    {
        targetForCamera = GameObject.Find("RigSpine3").transform;
        deltaPosition = cameras.position - targetForCamera.position;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Input.mousePosition.x < Screen.width * 0.6f)
        {
            lastPosition = Input.mousePosition;
            rotating = true;
        }

        if (Input.GetMouseButtonUp(0))
            rotating = false;

        if (rotating && Input.GetMouseButton(0))
            transform.Rotate(0, -300f * (Input.mousePosition - lastPosition).x / Screen.width, 0);

        lastPosition = Input.mousePosition;
    }

    private void LateUpdate()
    {
        cameras.position += (targetForCamera.position + deltaPosition - cameras.position) * Time.unscaledDeltaTime * 5;
    }
}