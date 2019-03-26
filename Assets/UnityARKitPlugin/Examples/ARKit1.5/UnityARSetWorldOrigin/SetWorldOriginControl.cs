using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class SetWorldOriginControl : MonoBehaviour
{
    public Camera arCamera;
    public Text positionText;
    public Text rotationText;


    // Update is called once per frame
    private void Update()
    {
        positionText.text = "Camera position=" + arCamera.transform.position;
        rotationText.text = "Camera rotation=" + arCamera.transform.rotation;
    }

    public void SetWorldOrigin()
    {
        UnityARSessionNativeInterface.GetARSessionNativeInterface().SetWorldOrigin(arCamera.transform);
    }
}