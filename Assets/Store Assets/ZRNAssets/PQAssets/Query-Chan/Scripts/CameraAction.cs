using UnityEngine;

public class CameraAction : MonoBehaviour
{
    //float rotatePosY = 0.0f;

    //float beforePos = 0.0f;

    private bool click;

    [SerializeField] private GameObject queryChan;
    //float startPosY = 0.0f;

    private float rotatePosX;

    private float startPosX;


    private void Update()
    {
        CameraRotateDevice();
        cameraRotateEditor();
    }


    private void CameraRotateDevice()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            // toutch start
            if (touch.phase == TouchPhase.Began)
            {
                startPosX = Input.GetTouch(0).position.x;
                //startPosY = Input.GetTouch(0).position.y;
            }
            // touch moving
            else if (touch.phase == TouchPhase.Moved)
            {
                rotatePosX = startPosX - Input.mousePosition.x;
                //rotatePosY = (startPosY - Input.mousePosition.y);
                queryChan.transform.localEulerAngles += new Vector3(0, rotatePosX, 0);
                startPosX = Input.mousePosition.x;
                //startPosY = Input.mousePosition.y;
            }
        }
    }


    private void cameraRotateEditor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosX = Input.mousePosition.x;
            //startPosY = Input.mousePosition.y;
            click = true;
        }

        if (click)
        {
            rotatePosX = startPosX - Input.mousePosition.x;
            //rotatePosY = (startPosY - Input.mousePosition.y);
            queryChan.transform.localEulerAngles += new Vector3(0, rotatePosX, 0);
            startPosX = Input.mousePosition.x;
            //startPosY = Input.mousePosition.y;
        }

        if (Input.GetMouseButtonUp(0)) click = false;
    }
}