using UnityEngine;

public class InteractionScript : MonoBehaviour
{
    private void Update()
    {
        /*Detect for a left mouse click
        if( Input.GetMouseButtonDown(0) ) { 
 
            //Create a ray that is aiming at where your mouse is aiming
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //This will store the raycast information
            RaycastHit hit;
 
            //return true when the ray hit something, store information in the hit-variable
            if( Physics.Raycast( ray, out hit ) )
            {
                GameObject obj = hit.transform.gameObject;
                //call the lookat function of the camera's transform to point/lookAt the hit gameObject
                if ( obj.CompareTag("Grid"))
                {
                    obj.GetComponent<GridScript>().SetTarget();
                }

                if (obj.GetComponent<CharacterScript>() != null)
                {
                    obj.GetComponent<CharacterScript>().SetChara();
                }
            }
        }
        if( Input.touchCount > 0 ) { 
 
            //Create a ray that is aiming at where your mouse is aiming
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            //This will store the raycast information
            RaycastHit hit;
 
            //return true when the ray hit something, store information in the hit-variable
            if( Physics.Raycast( ray, out hit ) )
            {
                GameObject obj = hit.transform.gameObject;
                //call the lookat function of the camera's transform to point/lookAt the hit gameObject
                if ( obj.CompareTag("Grid"))
                {
                    obj.GetComponent<GridScript>().SetTarget();
                }

                if (obj.GetComponent<CharacterScript>() != null)
                {
                    obj.GetComponent<CharacterScript>().SetChara();
                }
            }
        }*/
    }
}