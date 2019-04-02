using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.iOS;

public class MovementController : MonoBehaviour
{
    public static MovementController instance;

    public CharacterScript _chara;

    public Transform _target;
    public GameObject Trail;
    private GameObject _trail;

    // Use this for initialization
    private void Start()
    {
        if (instance == null) instance = this;

        if (instance != this) DestroyImmediate(gameObject);
    }

    // Update is called once per frame
    private void Update()
    {
#if UNITY_EDITOR //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
        if (GameManager.instance.gameStage == GameManager.GameState.PlayerTurn)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Vector3 inputpos;
                inputpos = Input.mousePosition;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(inputpos), out hit, 100))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    if (hit.collider.gameObject.CompareTag("Grid"))
                        if (_chara != null)
                        {
                            if (_target == hit.transform)
                            {
                                _chara.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                                Destroy(_trail);
                            }
                            else
                            {
                                _target = hit.transform;
                                if (_trail != null)
                                {
                                    Destroy(_trail);
                                }

                                _trail = Instantiate(Trail, _chara.transform.position, Quaternion.identity);
                                _trail.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                            }
                        }

                    if (hit.collider.gameObject.CompareTag("Player"))
                        _chara = hit.transform.GetComponent<CharacterScript>();
                }
            }
        }
    }
#else
if (GameManager.instance.gameStage == GameManager.GameState.PlayerTurn)
        {
		if (Input.touchCount > 0 )
		{
			var touch = Input.GetTouch(0);
			/*if (touch.phase == TouchPhase.Began)
			{
				var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
				ARPoint point = new ARPoint {
					x = screenPosition.x,
					y = screenPosition.y
				};
						
				List<ARHitTestResult> hitResults =
 UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, 
					ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
				if (hitResults.Count > 0) {
					foreach (var hitResult in hitResults) {
						Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.CompareTag("Grid"))
                    if (_chara != null)
                    {
                        if (_target == hit.transform)
                        {
                            _chara.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                            Destroy(_trail);
                        }
                        else
                        {
                            _target = hit.transform;
                            if (_trail != null)
                            {
                                Destroy(_trail);
                            }
                            _trail = Instantiate(Trail, _chara.transform.position, Quaternion.identity);
                            _trail.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                        }
                    }

                if (hit.collider.gameObject.CompareTag("Player"))
                    _chara = hit.transform.GetComponent<CharacterScript>();
            }
					}
				}*/
            RaycastHit hit;
            Vector3 inputpos;
            inputpos = touch.position;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(inputpos), out hit, 100))
            {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.CompareTag("Grid"))
                    if (_chara != null)
                    {
                        if (_target == hit.transform)
                        {
                            _chara.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                            Destroy(_trail);
                        }
                        else
                        {
                            _target = hit.transform;
                            if (_trail != null)
                            {
                                Destroy(_trail);
                            }
                            _trail = Instantiate(Trail, _chara.transform.position, Quaternion.identity);
                            _trail.gameObject.GetComponent<NavMeshAgent>().destination = hit.transform.position;
                        }
                    }

                if (hit.collider.gameObject.CompareTag("Player"))
                    _chara = hit.transform.GetComponent<CharacterScript>();
            }
    
			}
}
		}
#endif 

    public void SetChara(CharacterScript chara)
    {
        _chara = chara;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        Move();
    }

    public void Move()
    {
        if (_chara != null && _target != null) _chara.Move(_target);
    }
}