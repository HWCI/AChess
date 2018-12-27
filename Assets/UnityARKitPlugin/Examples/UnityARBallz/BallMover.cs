﻿using UnityEngine;

public class BallMover : MonoBehaviour
{
    private GameObject collBallGO;

    public GameObject collBallPrefab;
    public LayerMask collisionLayer = 1 << 10; //ARKitPlane layer
    public float maxRayDistance = 30.0f;

    // Use this for initialization
    private void Start()
    {
        collBallGO = null;
    }

    private void CreateMoveBall(Vector3 explodePosition)
    {
        collBallGO = Instantiate(collBallPrefab, explodePosition, Quaternion.identity);
    }

    // Update is called once per frame
    private void Update()
    {
#if UNITY_EDITOR //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //we'll try to hit one of the plane collider gameobjects that were generated by the plugin
            //effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
            if (Physics.Raycast(ray, out hit, maxRayDistance, collisionLayer))
            {
                //we're going to get the position from the contact point
                Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", hit.point.x, hit.point.y,
                    hit.point.z));

                if (collBallGO == null)
                    CreateMoveBall(hit.point);
                else
                    collBallGO.transform.position =
                        Vector3.MoveTowards(collBallGO.transform.position, hit.point, 0.05f);
            }
        }
        else
        {
            //mouse button no longer down
            Destroy(collBallGO);
            collBallGO = null;
        }
#else
		if (Input.touchCount > 0 )
		{
			var touch = Input.GetTouch(0);
			if (touch.phase == TouchPhase.Began) {
				var screenPosition = Camera.main.ScreenToViewportPoint (touch.position);
				ARPoint point = new ARPoint {
					x = screenPosition.x,
					y = screenPosition.y
				};

				List<ARHitTestResult> hitResults =
 UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, 
					                                   ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
				if (hitResults.Count > 0) {
					foreach (var hitResult in hitResults) {
						Vector3 position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
						CreateMoveBall (position);
						break;
					}
				}

			} else if (touch.phase == TouchPhase.Moved && collBallGO != null) {
				var screenPosition = Camera.main.ScreenToViewportPoint (touch.position);
				ARPoint point = new ARPoint {
					x = screenPosition.x,
					y = screenPosition.y
				};

				List<ARHitTestResult> hitResults =
 UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, 
					                                   ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
				if (hitResults.Count > 0) {
					foreach (var hitResult in hitResults) {
						Vector3 position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
						collBallGO.transform.position =
 Vector3.MoveTowards (collBallGO.transform.position, position, 0.05f);
						break;
					}
				}
			} else if (touch.phase != TouchPhase.Stationary) { //ended or cancelled
				Destroy(collBallGO);
				collBallGO = null;

			}
		}
#endif
    }
}